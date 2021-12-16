using System;

using OrderActorService.Contracts;

namespace OrderActor.Contracts
{
    public class Order
    {
        public string OrderId { get; set; }
        public string TableId { get; set; }
        public int DishId { get; set; }

        public string DishName { get; set; }

        public decimal Price { get; set; }
        public DateTime? OrderTime { get; set; }
        public DateTime? ServedTime { get; set; }
        public OrderState OrderState { get; set; }

        public int RestaurantId { get; set; } = 1;

    }

}
