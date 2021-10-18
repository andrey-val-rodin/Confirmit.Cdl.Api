using System;

namespace Confirmit.Cdl.Api.Middleware
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}