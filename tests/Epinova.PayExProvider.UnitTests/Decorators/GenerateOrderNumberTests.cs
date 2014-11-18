using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Dectorators.PaymentInitializers;
using Epinova.PayExProvider.Models.PaymentMethods;
using Moq;
using NUnit.Framework;

namespace Epinova.PayExProvider.UnitTests.Decorators
{
    [TestFixture]
    public class GenerateOrderNumberTests
    {
        private GenerateOrderNumber _orderNumberGenerator;

        [SetUp]
        private void Setup()
        {
            Mock<IPaymentInitializer> mockInitializer = new Mock<IPaymentInitializer>();
            _orderNumberGenerator = new GenerateOrderNumber(mockInitializer.Object);
        }

        [Test]
        public void Initialize_HasNoOrderNumber_OrderNumberIsGenerated()
        {
            CreditCard creditCard = new CreditCard();
            creditCard.Payment = new Mock<IPayExPayment>().Object;
            creditCard.OrderGroupId = 1000;

            _orderNumberGenerator.Initialize(creditCard, null, null);

            Assert.IsNotNullOrEmpty(creditCard.Payment.OrderNumber);
        }

        //[Test]
        //public void Initialize_HasDescription_OrderNumberIsAddedToDescription()
        //{
        //    Mock<IPayExPayment> payment = new Mock<IPayExPayment>();
        //    payment.Setup(p => p.Description).Returns("Order number: {0}");

        //    CreditCard creditCard = new CreditCard();
        //    creditCard.Payment = new Mock<IPayExPayment>().Object;
        //    creditCard.OrderGroupId = 1000;

        //    _orderNumberGenerator.Initialize(creditCard, null, null);

        //    Assert.AreEqual(creditCard.Payment.Description, "Order number: 1000");
        //}
    }
}
