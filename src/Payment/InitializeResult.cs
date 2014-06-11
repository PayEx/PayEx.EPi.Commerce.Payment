
using System;

namespace Epinova.PayExProvider.Payment
{
    public class InitializeResult : ResultBase
    {
        public Guid OrderRef { get; set; }
        public string ReturnUrl { get; set; }

        public InitializeResult(bool success) : base(success)
        {
        }
    }
}
