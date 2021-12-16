namespace RestaurantService.Contracts.Domain
{
    public class Dish
    {
        public string Title { get; set; }

        public decimal Price { get; set; }

        public string ImageUrl { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }
    }
}
