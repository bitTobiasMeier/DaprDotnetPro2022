using Dapr.Actors;

namespace TableActor.Contracts
{
    public interface ITableActor : IActor
    {
        Task AddOrderAsync(TableOrder tableOrder, CancellationToken cancellationToken);

        Task<List<TableOrder>> GetOrdersAsync(CancellationToken cancellationToken);

        Task SetStateAsync(string orderId, string state, CancellationToken cancellationToken);
    }
}
