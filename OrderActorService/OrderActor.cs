using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Actors.Runtime;
using Dapr.Client;
using Dinner.Contracts;
using KitchenActor.Contracts;
using OrderActor.Contracts;
using OrderActorService.Contracts;
using TableActor.Contracts;

namespace OrderActorService
{
    public class OrderActor : Actor, IOrderActor, IRemindable
    {
        private readonly DaprClient daprClient;
        private static readonly string OrderDataStateKey = "orderdata";
        private const string OrderStateReminder = "OrderStateReminder";

        public OrderActor(ActorHost host, DaprClient daprClient) : base(host)
        {
            this.daprClient = daprClient;
        }

        public async Task<Order> AddOrderAsync(OrderEvent orderEvent, CancellationToken cancellationToken)
        {
            var order = new Order()
            {
                DishId = orderEvent.DishId,
                DishName = orderEvent.DishName,
                OrderId = Guid.NewGuid().ToString(),
                OrderState = OrderState.Ordered,
                OrderTime = orderEvent.CreationDate,
                Price = orderEvent.Price,
                RestaurantId = 1,
                TableId = orderEvent.TableId ?? "1"
            };

            await SetOrUpdateOrderDataStateAsync(order, cancellationToken);

            var tableActor = ActorProxy.Create<ITableActor>(new ActorId(order.TableId), "TableActor");
            await tableActor.AddOrderAsync(new TableOrder() { Dish = order.DishName, OrderId = order.OrderId, Price = order.Price }, CancellationToken.None);

            //Register reminder
            await RegisterReminderAsync(OrderStateReminder, null,TimeSpan.FromSeconds(3),TimeSpan.FromSeconds(5));

            return order;
        }

        private async Task SetOrUpdateOrderDataStateAsync(Order order, CancellationToken cancellationToken)
        {
            await this.StateManager.AddOrUpdateStateAsync(OrderDataStateKey, order, (key, current) => order, cancellationToken)
                            .ConfigureAwait(true);
        }

        public async Task<Order> GetOrder(CancellationToken cancellationToken)
        {
            return await this.GetOrderFromState(cancellationToken);
        }

        public async Task CookingStartedAsync()
        {
            await this.SetOrderStateAsync(OrderState.Cooking).ConfigureAwait(true);
            var order = await GetOrderFromState(CancellationToken.None);
            var table = ActorProxy.Create<ITableActor>(new Dapr.Actors.ActorId(order.TableId), "TableActor");
            await table.SetStateAsync(order.OrderId, "wird gekocht", CancellationToken.None);
            await ProcessStateAsync();
        }

        public async Task CookingCompletedAsync()
        {
            await this.SetOrderStateAsync(OrderState.Cooked).ConfigureAwait(true);
            var order = await GetOrderFromState(CancellationToken.None);
            var table = ActorProxy.Create<ITableActor>(new Dapr.Actors.ActorId(order.TableId), "TableActor");
            await table.SetStateAsync(order.OrderId, "gekocht", CancellationToken.None);
            await ProcessStateAsync();
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            switch (reminderName)
            {
                case OrderStateReminder:
                {
                    await ProcessStateAsync();
                    break;
                }
                default:
                {
                    throw new InvalidOperationException("Unexpected reminder: " + reminderName);
                }
            }
        }

        private async Task SetOrderStateAsync(OrderState state)
        {
            var order = await this.GetOrder(CancellationToken.None);
            order.OrderState = state;
            await this.SetOrUpdateOrderDataStateAsync(order, CancellationToken.None);
        }

        private async Task<Order> GetOrderFromState(CancellationToken cancellationToken)
        {
            var orderconstract =
                await StateManager.TryGetStateAsync<Order>(OrderDataStateKey, cancellationToken);
            if (!orderconstract.HasValue)
                return null;
            var val = orderconstract.Value;
            return val;
        }

        private async Task ProcessStateAsync()
        {
            var order = await GetOrderFromState(CancellationToken.None);
            var state = order != null ? order.OrderState : OrderState.None;
            switch (state)
            {
                case OrderState.Ordered:
                    {
                        //Küche benachrichtigen                        
                        var kitchenActor = ActorProxy.Create<IKitchenActor>(new ActorId(order.RestaurantId.ToString()), "KitchenActor");
                        await kitchenActor.AddOrderAsync(new KitchenOrder
                        {
                            DishId = order.DishId,
                            OrderId = this.Id.GetId()
                        });
                        await SetOrderStateAsync(OrderState.InKitchenQueue);
                        var table = ActorProxy.Create<ITableActor>(new Dapr.Actors.ActorId(order.TableId), "TableActor");
                        await table.SetStateAsync(order.OrderId, "Küche informiert", CancellationToken.None);
                        break;
                    }
                case OrderState.Cooked:
                {
                    //Kellner informieren
                    await this.daprClient.PublishEventAsync("pubsub", "serveorder", order).ConfigureAwait(true);
                        await SetOrderStateAsync(OrderState.Payed);
                        break;
                }
                case OrderState.Payed:
                {
                    //Unregister reminder
                    await this.UnregisterReminderAsync(OrderStateReminder);
                    break;
                }
            }
        }
    }
}
