using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Actors;

namespace KitchenActor.Contracts
{
    public interface IKitchenActor : IActor
    {
        Task AddOrderAsync(KitchenOrder kitchenOrder);

        // Task<List<KitchenOrder>> GetKitchenQueue(CancellationToken none);
    }
}