using System.Threading;
using System.Threading.Tasks;
using Dapr.Actors;
using Dinner.Contracts;

namespace OrderActor.Contracts
{
    public interface IOrderActor : IActor
    {
        Task<Order> AddOrderAsync(OrderEvent order, CancellationToken cancellationToken);

        Task<Order> GetOrder(CancellationToken cancellationToken);
        Task CookingStartedAsync();
        Task CookingCompletedAsync();
    }

}
