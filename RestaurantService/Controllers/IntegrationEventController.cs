using Dapr;
using Dapr.Actors.Client;
using Dinner.Contracts;
using KitchenActor.Contracts;
using Microsoft.AspNetCore.Mvc;
using OrderActor.Contracts;

namespace RestaurantService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationEventController : ControllerBase
    {
        private readonly ILogger<IntegrationEventController> logger;
        private const string DaprPubsubName = "pubsub";

        public IntegrationEventController(ILogger<IntegrationEventController> logger)
        {
            this.logger = logger;
        }

        [HttpPost("order")]
        [Topic(DaprPubsubName, "order")]
        public async Task Order([FromBody] OrderEvent
            orderEvent)
        {            
            this.logger.LogInformation("Verarbeite Order {0}", orderEvent.DishId);
            var actor = ActorProxy.Create<IOrderActor>(new Dapr.Actors.ActorId(orderEvent.Id.ToString()), "OrderActor");
            await actor.AddOrderAsync(orderEvent, CancellationToken.None).ConfigureAwait(true);
        }


        [HttpPost("CookingStarted")]
        [Topic(DaprPubsubName, KitchenActorEvents.CookingStarted)]
        public async Task CookingStartedAsync(KitchenOrder orderEvent)
        {
            this.logger.LogInformation("Cooking started for order {0}", orderEvent.OrderId);
            var actor = ActorProxy.Create<IOrderActor>(new Dapr.Actors.ActorId(orderEvent.OrderId), "OrderActor");
            await actor.CookingStartedAsync();
        }

        [HttpPost("CookingCompleted")]
        [Topic(DaprPubsubName, KitchenActorEvents.CookingCompleted)]
        public async Task CookingCompletedAsync(KitchenOrder orderEvent)
        {
            this.logger.LogInformation("Cooking completed for order {0}", orderEvent.OrderId);
            var actor = ActorProxy.Create<IOrderActor>(new Dapr.Actors.ActorId(orderEvent.OrderId), "OrderActor");
            await actor.CookingCompletedAsync();
        }

        [HttpPost("serveorder")]
        [Topic(DaprPubsubName, "serveorder")]
        public void ServeOrderAsync(Order order)
        {
            this.logger.LogInformation("Serve order {0} for table {1}", order.OrderId, order.TableId);            
            //ToDo: Kellner benachrichtigen, z.B. über Azure SignlarR, Smtp, Sms ... (output Binding)
        }
    }
}
