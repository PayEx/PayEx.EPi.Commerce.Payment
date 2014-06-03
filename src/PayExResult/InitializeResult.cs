
using System;

namespace Epinova.PayExProvider.PayExResult
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
