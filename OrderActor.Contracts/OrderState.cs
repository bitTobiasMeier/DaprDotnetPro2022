namespace OrderActorService.Contracts
{
    public enum OrderState
    {
        None = 0,
        Ordered = 1,
        InKitchenQueue = 2,
        Cooking = 3,
        Cooked = 4,
        Served = 5,
        Payed = 6
    }

}
