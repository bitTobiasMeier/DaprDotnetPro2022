using Microsoft.AspNetCore.Mvc;
using Dapr.Client;
using RestaurantService.Contracts.Domain;
using Dapr;

namespace RestaurantService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MenuController : ControllerBase
    {
        private const string StateStoreName = "statestore";
        private readonly ILogger<MenuController> logger;

        public MenuController(ILogger<MenuController> logger)
        {
            this.logger = logger;
        }

        [HttpGet("{restaurantid}")]
        public async Task<IEnumerable<Dish>> GetAsync([FromState(StateStoreName, "restaurantid")] StateEntry<List<Dish>> menucard)
        {
            Dish[] dishes = await GetOrInitializeMenuCard(menucard);
            return dishes;
        }

        [HttpGet("get/{restaurantid}")]
        public async Task<IEnumerable<Dish>> Get2Async([FromServices] DaprClient daprClient, string restaurantid)
        {
            var dishesval = await daprClient.GetStateEntryAsync<List<Dish>>(StateStoreName, restaurantid);
            if (dishesval.Value == null)
            {
                dishesval.Value = GetInitialMenuCard();
                await dishesval.SaveAsync();            
            }

            return dishesval.Value;
        }

        [HttpGet("get2/{restaurantid}")]
        public async Task<IEnumerable<Dish>> Get3Async([FromServices] DaprClient daprClient, string restaurantid)
        {
            var dishesval = await daprClient.GetStateAsync<List<Dish>>(StateStoreName, restaurantid);
            if (dishesval == null)
            {
                dishesval  = GetInitialMenuCard();
                await daprClient.SaveStateAsync(StateStoreName, restaurantid, dishesval);
            }           

            return dishesval;
        }

        [HttpPost("{restaurantid}")]
        public async Task<bool> ChangeMenu([FromServices] DaprClient daprClient, string restaurantid, IEnumerable<Dish> menues)
        {
            var (original, originalETag) = await daprClient.GetStateAndETagAsync<IEnumerable<Dish>>(StateStoreName, restaurantid);                       
            await daprClient.SaveStateAsync<IEnumerable<Dish>>(StateStoreName, restaurantid, new List<Dish>());
            var isSaved = await daprClient.TrySaveStateAsync(StateStoreName, restaurantid, menues, originalETag);

            if (!isSaved)
            {
                (original, originalETag) = await daprClient.GetStateAndETagAsync<IEnumerable<Dish>>(StateStoreName, restaurantid);
                isSaved = await daprClient.TrySaveStateAsync(StateStoreName, restaurantid, menues, originalETag);                
            }

            /*var transactionRequests = new List<StateTransactionRequest>()
            {
                new StateTransactionRequest("data", "Mein Wert", StateOperationType.Upsert),
                new StateTransactionRequest("queue", null, StateOperationType.Delete)
            };

            await client.ExecuteStateTransactionAsync(StateStoreName, transactionRequests);*/

            return isSaved;
        }


        private static async Task<Dish[]> GetOrInitializeMenuCard(StateEntry<List<Dish>> menucard)
        {
            if (menucard.Value == null)
            {
                menucard.Value = GetInitialMenuCard();

                var success = await menucard.TrySaveAsync();
                if (!success)
                {
                    throw new Exception("Menucard could not be saved");
                }
            }
            var dishes = menucard.Value.ToArray();
            return dishes;
        }

        private static List<Dish> GetInitialMenuCard()
        {
            return new List<Dish>()
                {
                    new Dish() {Id = 1, Title = "Wolfsbarsch auf Selleriepüree", Description = "Wolfsbarschfilet auf feinem Selleriepürree mit Tomatensalat angemacht mit einer Vanillevinegrait.", Price = 25.25m, ImageUrl = "/dishes/foto2.jpg"},
                    new Dish() {Id = 2, Title = "Focaccio gespickt mit Rosmarin", Description="Selbstgebackenes Foccacio gespickt mit Rosmarinnadel", Price=5.5m, ImageUrl="/dishes/foto6.jpg"},
                    new Dish() {Id = 3, Title="Warmer Kirschkuchen", Description="Warmer Kirschkuchen", Price=10.75m, ImageUrl="/dishes/foto5.jpg"}
                };
        }
    }
}