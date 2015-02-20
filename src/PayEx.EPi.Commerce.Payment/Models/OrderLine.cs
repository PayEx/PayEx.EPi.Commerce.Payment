
using Newtonsoft.Json;

namespace PayEx.EPi.Commerce.Payment.Models
{
    public class OrderLine
    {
        public string OrderRef { get; private set; }
        public string ItemNumber { get; private set; }
        public string Description { get; private set; }
        public string Description2 { get; set; }
        public string Description3 { get; set; }
        public string Description4 { get; set; }
        public string Description5 { get; set; }
        public int Quantity { get; private set; }
        public int Amount { get; private set; }
        public int VatAmount { get; private set; }
        public int VatPercentage { get; private set; }

        public OrderLine(string orderRef, string itemNumber, string description, int quantity, int price, decimal vatAmount, decimal vatPercentage)
        {
            OrderRef = orderRef;

            if (string.IsNullOrWhiteSpace(itemNumber))
                itemNumber = "-";
            ItemNumber = itemNumber;

            Description = description;
            Description2 = string.Empty;
            Description3 = string.Empty;
            Description4 = string.Empty;
            Description5 = string.Empty;
            Quantity = quantity;
            Amount = price;
            VatAmount = (int) (vatAmount*100);
            VatPercentage = (int)(vatPercentage * 100);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
