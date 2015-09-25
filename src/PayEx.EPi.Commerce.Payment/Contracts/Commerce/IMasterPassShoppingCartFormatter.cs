namespace PayEx.EPi.Commerce.Payment.Contracts.Commerce
{
    public interface IMasterPassShoppingCartFormatter
    {
        string GenerateShoppingCartXmlString(PayExPayment payExPayment);
    }
}
