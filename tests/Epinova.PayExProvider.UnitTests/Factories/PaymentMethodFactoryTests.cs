using Epinova.PayExProvider.Contracts;
using Epinova.PayExProvider.Contracts.Commerce;
using Epinova.PayExProvider.Factories;
using Moq;
using NUnit.Framework;
using PaymentMethod = Epinova.PayExProvider.Models.PaymentMethods.PaymentMethod;

namespace Epinova.PayExProvider.UnitTests.Factories
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

            PaymentMethodFactory factory = new PaymentMethodFactory(paymentManagerMock.Object, parameterReaderMock.Object, loggerMock.Object, cartActionsMock.Object);
            PaymentMethod result = factory.Create(null);

            Assert.IsNull(result);
        }
    }
}
