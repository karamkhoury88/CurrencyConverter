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
        public string NonTechnicalMessage { get; init; }
        public string TechnicalMessage { get; init; }

        public AppException(AppErrorCode errorCode, string nonTechnicalMessage, string? technicalMessage = null) 
            : base(!string.IsNullOrWhiteSpace(technicalMessage) ? $"{technicalMessage} {nonTechnicalMessage}" : nonTechnicalMessage)
        {
            ErrorCode = errorCode;
            NonTechnicalMessage = nonTechnicalMessage;
            TechnicalMessage = technicalMessage ?? nonTechnicalMessage;
        }
    }
}
