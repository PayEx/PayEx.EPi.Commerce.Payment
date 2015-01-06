using EPiServer.Business.Commerce.Payment.PayEx.Contracts;
using EPiServer.Business.Commerce.Payment.PayEx.Contracts.Commerce;
using EPiServer.Business.Commerce.Payment.PayEx.Factories;
using EPiServer.Business.Commerce.Payment.PayEx.Models.PaymentMethods;
using Moq;
using NUnit.Framework;

namespace EPiServer.Business.Commerce.Payment.PayEx.UnitTests.Factories
{
    [TestFixture]
    public class PaymentMethodFactoryTests
    {
        [Test]
        public void Create_PaymentIsNull_ReturnsNull()
        {
            Mock<IPaymentManager> paymentManagerMock = new Mock<IPaymentManager>();
            Mock<IParameterReader> parameterReaderMock = new Mock<IParameterReader>();
            Mock<ILogger> loggerMock = new Mock<ILogger>();
            Mock<ICartActions> cartActionsMock = new Mock<ICartActions>();
            Mock<IVerificationManager> verificationManagerMock  = new Mock<IVerificationManager>();
            Mock<IOrderNumberGenerator> orderNumberGeneratorMock = new Mock<IOrderNumberGenerator>();

            PaymentMethodFactory factory = new PaymentMethodFactory(paymentManagerMock.Object, parameterReaderMock.Object, loggerMock.Object, cartActionsMock.Object, verificationManagerMock.Object, orderNumberGeneratorMock.Object);
            PaymentMethod result = factory.Create(null);

            Assert.IsNull(result);
        }
    }
}
