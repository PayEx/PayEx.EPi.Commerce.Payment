using Mediachase.Commerce.Orders;
using Moq;
using NUnit.Framework;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Dectorators.PaymentInitializers;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

namespace PayEx.EPi.Commerce.Payment.UnitTests.Decorators.PaymentInitializers
{
    [TestFixture]
    public class GenerateOrderNumberTests
    {
        private GenerateOrderNumber _orderNumberGenerator;
        private Mock<IOrderNumberGenerator> _orderNumberGeneratorMock;

        [SetUp]
        public void Setup()
        {
            Mock<IPaymentInitializer> mockInitializer = new Mock<IPaymentInitializer>();
            _orderNumberGeneratorMock = new Mock<IOrderNumberGenerator>();
            _orderNumberGenerator = new GenerateOrderNumber(mockInitializer.Object, _orderNumberGeneratorMock.Object);
        }

        [Test]
        public void Initialize_HasNoOrderNumber_OrderNumberIsGenerated()
        {
            CreditCard creditCard = new CreditCard();
            Mock<IPayExPayment> paymentMock = new Mock<IPayExPayment>();
            paymentMock.SetupAllProperties();

            _orderNumberGeneratorMock.Setup(x => x.Generate(It.IsAny<Cart>())).Returns("Ordernumber");

            creditCard.Payment = paymentMock.Object;
            creditCard.OrderGroupId = 1000;

            _orderNumberGenerator.Initialize(creditCard, null, null, null, null);

            Assert.IsNotNullOrEmpty(creditCard.Payment.OrderNumber);
        }

        [Test]
        public void Initialize_HasDescription_OrderNumberIsAddedToDescription()
        {
            CreditCard creditCard = new CreditCard();
            Mock<IPayExPayment> paymentMock = new Mock<IPayExPayment>();
            paymentMock.SetupAllProperties();

            creditCard.Payment = paymentMock.Object;
            creditCard.Payment.Description = "Order number: {0}";
            creditCard.OrderGroupId = 1000;

            _orderNumberGenerator.Initialize(creditCard, null, null, null, null);

            Assert.AreEqual(creditCard.Payment.Description, "Order number: " + creditCard.Payment.OrderNumber);
        }
    }
}
