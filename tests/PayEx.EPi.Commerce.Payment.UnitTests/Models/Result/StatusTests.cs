using NUnit.Framework;
using PayEx.EPi.Commerce.Payment.Models.Result;

namespace PayEx.EPi.Commerce.Payment.UnitTests.Models.Result
{
    [TestFixture]
    public class StatusTests
    {
        [TestCase("", "", false)]
        [TestCase("OK", "OK", true)]
        [TestCase(null, null, false)]
        [TestCase("OK", "GenericError", false)]
        [TestCase("This is an error", "OK", false)]
        public void Success_ReturnsCorrectResult(string description, string errorCode, bool expected)
        {
            Status status = new Status { Description = description, ErrorCode = errorCode };
            Assert.AreEqual(expected, status.Success);
        }
    }
}
