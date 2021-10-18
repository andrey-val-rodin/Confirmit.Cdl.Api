using System;

namespace Confirmit.Cdl.Api.Middleware
{
    public class DeadlockException : Exception
    {
        public DeadlockException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}