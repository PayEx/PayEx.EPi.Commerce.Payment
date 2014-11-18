using Epinova.PayExProvider.Factories;
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
            PaymentMethodFactory factory = new PaymentMethodFactory();
            PaymentMethod result = factory.Create(null);

            Assert.IsNull(result);
        }
    }
}
