namespace Dinner.Contracts
{
    public class OrderEvent : IntegrationEvent
    {
        public int DishId { get; set; }

        public string DishName { get; set; }

        public string TableId { get; set; }

        public decimal Price { get; set; }
    }
}
