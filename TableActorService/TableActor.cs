using Dapr.Actors.Runtime;
using TableActor.Contracts;

namespace TableActorService
{
    public class TableActor : Actor, ITableActor
    {
        private const string ListName = "OrderList";

        public TableActor(ActorHost host) : base(host)
        {
        }

        public async Task AddOrderAsync(TableOrder tableOrder, CancellationToken cancellationToken)
        {
            var tableOrderContract = new TableOrder
            {
                OrderId = tableOrder.OrderId,
                Price = tableOrder.Price,
                Dish = tableOrder.Dish
            };
            var list = await GetTableOrderContractsAsync(cancellationToken);
            list.Add(tableOrderContract);
            await StateManager.AddOrUpdateStateAsync(ListName, list, (key, value) => list, cancellationToken);
        }

        public async Task<List<TableOrder>> GetOrdersAsync(CancellationToken cancellationToken)
        {
            var list = await GetTableOrderContractsAsync(cancellationToken);
            return list.Select(o => new TableOrder
            {
                OrderId = o.OrderId,
                Price = o.Price,
                Dish = o.Dish,
                State = o.State
            }).ToList();
        }

        public async Task SetStateAsync (string orderId, string state, CancellationToken cancellationToken)
        {
            var list = await GetTableOrderContractsAsync(cancellationToken);
            var entry = list.First(order => order.OrderId == orderId);
            entry.State = state;
            await StateManager.AddOrUpdateStateAsync(ListName, list, (key, value) => list, cancellationToken);
        }

        public async Task ServeAsync(string orderId, DateTime servedAt, CancellationToken cancellationToken)
        {
            var list = await GetTableOrderContractsAsync(cancellationToken);
            var entry = list.First(order => order.OrderId == orderId);
            entry.ServedAt = servedAt;
            await StateManager.AddOrUpdateStateAsync(ListName, list, (key, value) => list, cancellationToken);
        }

        private async Task<List<TableOrder>> GetTableOrderContractsAsync(CancellationToken cancellationToken)
        {
            var listCond =
                await StateManager.GetOrAddStateAsync(ListName, new List<TableOrder>(), CancellationToken.None);
            var list = listCond ?? new List<TableOrder>();
            return list;
        }

    }

}
