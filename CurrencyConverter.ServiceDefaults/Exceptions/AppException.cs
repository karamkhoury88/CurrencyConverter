using Grpc.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.ServiceDefaults.Exceptions
{
    public class AppException : Exception
    {
        public AppErrorCode ErrorCode { get; init; }
        public string PublicMessage { get; init; }
        public string TechnicalMessage { get; init; }

        public AppException(AppErrorCode errorCode, string publicMessage = "", string technicalMessage = "") : base(technicalMessage ?? publicMessage)
        {
            ErrorCode = errorCode;
            PublicMessage = publicMessage;
        }
    }
}
