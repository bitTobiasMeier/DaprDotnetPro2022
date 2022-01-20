using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Client;
using Dinner.Contracts;
using RestaurantService.Contracts.Domain;
using Microsoft.AspNetCore.Components.Web;
using TableActor.Contracts;

namespace Dinner.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly DaprClient daprClient;

        public IndexModel(ILogger<IndexModel> logger, DaprClient daprClient)
        {
            this.logger = logger;
            this.daprClient = daprClient;
        }

        public async Task OnGet()
        {
            await LoadDataAsync().ConfigureAwait(true);
        }

        private async Task LoadDataAsync()
        {
            this.logger.LogInformation("Bestellseite geladen");
            try
            {
                var menuentries = await this.daprClient.InvokeMethodAsync<IEnumerable<Dish>>(
                    HttpMethod.Get,
                    "restaurantservice",
                    "menu/1").ConfigureAwait(true);

                ViewData["Menu"] = menuentries;
            } 
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message + ex.StackTrace;
                ViewData["Error"] += ex.InnerException?.Message;
                ViewData["Menu"] = Array.Empty<Dish>();
            }
        }

        public async Task<IActionResult> OnPostOrderAsync(int id, string tableNr, string dishName, decimal dishPrice)
        {
            try
            {
                this.logger.LogInformation("Gericht {0} wurde für Tisch {1} bestellt.", dishName, tableNr);
                var ev = new OrderEvent() { DishId = id, TableId = tableNr, DishName = dishName, Price = dishPrice };
                await this.daprClient.PublishEventAsync("pubsub", "order", ev).ConfigureAwait(true);
                
                ViewData["tablenr"] = tableNr;
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.Message + ex.StackTrace;
                ViewData["Error2"] = ex.InnerException?.Message;
            }

            await LoadDataAsync().ConfigureAwait(true);
            return this.Page();
        }

        public async Task<IActionResult> OnPostTableAsync(string tableNr)
        {
            ViewData["tablenr"] = tableNr;
            var tableActor = ActorProxy.Create<ITableActor>(new ActorId(tableNr), "TableActor");
            var orders =  await tableActor.GetOrdersAsync(CancellationToken.None);
            ViewData["orders"] = orders;
            await LoadDataAsync().ConfigureAwait(true);
            return this.Page();
        }
    }
}
