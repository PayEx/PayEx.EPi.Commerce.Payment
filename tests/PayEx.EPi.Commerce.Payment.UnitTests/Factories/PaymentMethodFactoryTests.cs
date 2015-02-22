using Moq;
using NUnit.Framework;
using PayEx.EPi.Commerce.Payment.Contracts;
using PayEx.EPi.Commerce.Payment.Contracts.Commerce;
using PayEx.EPi.Commerce.Payment.Factories;
using PayEx.EPi.Commerce.Payment.Models.PaymentMethods;

namespace PayEx.EPi.Commerce.Payment.UnitTests.Factories
{
    [TestFixture]
    public class PaymentMethodFactoryTests
    {
        [Test]
        public void Create_PaymentIsNull_ReturnsNull()
        {
            Mock<IPaymentManager> paymentManagerMock = new Mock<IPaymentManager>();
            Mock<IParameterReader> parameterReaderMock = new Mock<IParameterReader>();
            Mock<ICartActions> cartActionsMock = new Mock<ICartActions>();
            Mock<IVerificationManager> verificationManagerMock  = new Mock<IVerificationManager>();
            Mock<IOrderNumberGenerator> orderNumberGeneratorMock = new Mock<IOrderNumberGenerator>();
            Mock<IAdditionalValuesFormatter> additionalValuesFormatterMock = new Mock<IAdditionalValuesFormatter>();
            Mock<IPaymentActions> paymentActionsMock = new Mock<IPaymentActions>();

            PaymentMethodFactory factory = new PaymentMethodFactory(paymentManagerMock.Object, parameterReaderMock.Object, cartActionsMock.Object,
                verificationManagerMock.Object, orderNumberGeneratorMock.Object, additionalValuesFormatterMock.Object, paymentActionsMock.Object);
            PaymentMethod result = factory.Create(null);

            Assert.IsNull(result);
        }
    }
}
