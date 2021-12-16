namespace TableActor.Contracts
{
    public class TableOrder
    {
        public string OrderId { get; set; }

        public decimal Price { get; set; }

        public DateTime? ServedAt { get; set; }

        public string Dish { get; set; }

        public string State { get; set; }
    }
}
