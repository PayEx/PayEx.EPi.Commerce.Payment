using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Mediachase.Commerce.Orders;
using Mediachase.MetaDataPlus.Configurator;

namespace Epinova.PayExProvider.UnitTests.TestableModels
{
    public class MockPayment : Mediachase.Commerce.Orders.Payment
    {
        public MockPayment(MetaClass metaClass) : base(metaClass)
        {
        }

        public MockPayment(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
