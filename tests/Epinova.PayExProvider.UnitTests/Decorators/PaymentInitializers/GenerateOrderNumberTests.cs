using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Dectorators.PaymentInitializers;
using Epinova.PayExProvider.Models.PaymentMethods;
using Moq;
using NUnit.Framework;

namespace Epinova.PayExProvider.UnitTests.Decorators.PaymentInitializers
{
    [TestFixture]
    public class GenerateOrderNumberTests
    {
        private GenerateOrderNumber _orderNumberGenerator;

        [SetUp]
        public void Setup()
        {
            Mock<IPaymentInitializer> mockInitializer = new Mock<IPaymentInitializer>();
            _orderNumberGenerator = new GenerateOrderNumber(mockInitializer.Object);
        }

        [Test]
        public void Initialize_HasNoOrderNumber_OrderNumberIsGenerated()
        {
            CreditCard creditCard = new CreditCard();
            Mock<IPayExPayment> paymentMock = new Mock<IPayExPayment>();
            paymentMock.SetupAllProperties();

            creditCard.Payment = paymentMock.Object;
            creditCard.OrderGroupId = 1000;

            _orderNumberGenerator.Initialize(creditCard, null, null, null);

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

            _orderNumberGenerator.Initialize(creditCard, null, null, null);

            Assert.AreEqual(creditCard.Payment.Description, "Order number: " + creditCard.Payment.OrderNumber);
        }
    }
}
