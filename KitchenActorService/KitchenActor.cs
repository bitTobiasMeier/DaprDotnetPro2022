using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Actors.Runtime;
using Dapr.Client;
using KitchenActor.Contracts;

namespace KitchenActorService
{
    public class KitchenActor : Actor, IKitchenActor, IRemindable
    {
        private readonly DaprClient daprClient;
        private const string KitchenQueueName = "KitchenQueue";
        private const string CookingQueueName = "CookingQueue";
        private const string CookingReminderName = "DinnerCookingReminder";
        private const string IsReminderRegisteredFlag = "IsReminderRegisteredFlag";

        public KitchenActor(ActorHost host, DaprClient daprClient) : base(host)
        {
            this.daprClient = daprClient;
        }

        protected override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();

        }

        public async Task AddOrderAsync(KitchenOrder kitchenOrder)
        {
            var queue = await GetKitchenQueue();
            queue.Enqueue(new KitchenOrderContract() { DishId = kitchenOrder.DishId, OrderId = kitchenOrder.OrderId });
            await SetKitchenQueueState(queue);
            await RegisterReminder();
        }

        public async Task<List<KitchenOrder>> GetKitchenQueue(CancellationToken none)
        {
            var queue = await GetKitchenQueue();
            return queue.Select(order => new KitchenOrder()
            {
                DishId = order.DishId,
                OrderId = order.OrderId
            }).ToList();
        }

        public async Task<List<KitchenOrder>> GetKitchenCookingQueue(CancellationToken none)
        {
            var queue = await GetCookingQueue();
            return queue.Select(order => new KitchenOrder()
            {
                DishId = order.DishId,
                OrderId = order.OrderId
            }).ToList();

        }

        private async Task<Queue<KitchenOrderContract>> GetKitchenQueue()
        {
            try
            {
                await this.StateManager.ClearCacheAsync();
                var queue =
                    await StateManager.GetOrAddStateAsync<Queue<KitchenOrderContract>>(KitchenQueueName, new Queue<KitchenOrderContract>());
                return queue;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Queue<KitchenOrderContract>();
            }

        }

        private async Task<Queue<KitchenOrderContract>> GetCookingQueue()
        {
            await this.StateManager.ClearCacheAsync();
            var queue =
                await StateManager.GetOrAddStateAsync<Queue<KitchenOrderContract>>(CookingQueueName, new Queue<KitchenOrderContract>());
            return queue;
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            switch (reminderName)
            {
                case CookingReminderName:
                {
                    await CookAsync();
                    break;
                }
                default:
                {
                        //throw new InvalidOperationException("Unexpected reminder in actor kitchen: " + reminderName);
                        break;
                }
            }
        }
        
        private async Task SetCookingQueueState(Queue<KitchenOrderContract> cookingQueue)
        {
            await this.StateManager.SetStateAsync(CookingQueueName, cookingQueue);
        }

        private async Task SetKitchenQueueState(Queue<KitchenOrderContract> queue)
        {
            await this.StateManager.SetStateAsync(KitchenQueueName, queue);
        }

        private async Task RegisterReminder()
        {
            var isRegisteredCondValue = await this.StateManager.TryGetStateAsync<bool>(IsReminderRegisteredFlag);
            if (isRegisteredCondValue.HasValue == false)
            {
                await RegisterReminderAsync(CookingReminderName, null, TimeSpan.FromSeconds(3),
                   TimeSpan.FromSeconds(3));
                await this.StateManager.AddStateAsync(IsReminderRegisteredFlag, true);
            }
        }

        private async Task CookAsync()
        {
            var cookingQueue = await GetCookingQueue();
            var kitchenQueue = await GetKitchenQueue();
            if (kitchenQueue.Count > 0 && cookingQueue.Count < 5)
            {
                var dishToCook = kitchenQueue.Dequeue();
                await this.daprClient.PublishEventAsync("pubsub", KitchenActorEvents.CookingStarted, new KitchenOrder()
                {
                    DishId = dishToCook.DishId,
                    OrderId = dishToCook.OrderId
                });

                await SetKitchenQueueState(kitchenQueue);
                
                cookingQueue.Enqueue(dishToCook);
                await SetCookingQueueState(cookingQueue);
            }


            var random = new Random();
            var check = random.Next(4);
            //Nur bei einer Zufallszahl 1 ist das Gericht fertiggekocht.
            if (cookingQueue.Count > 0 && check == 1)
            {
                var cooked = cookingQueue.Dequeue();
                await this.daprClient.PublishEventAsync("pubsub", KitchenActorEvents.CookingCompleted, new KitchenOrder()
                {
                    DishId = cooked.DishId,
                    OrderId = cooked.OrderId
                });

                await SetCookingQueueState(cookingQueue);
            }
        }
    }
}
