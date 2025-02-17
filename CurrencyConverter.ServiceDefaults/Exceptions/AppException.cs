namespace CurrencyConverter.ServiceDefaults.Exceptions
{
    /// <summary>
    /// Represents a custom exception used for application-specific error handling.
    /// </summary>
    public class AppException : Exception
    {
        /// <summary>
        /// Gets the error code associated with the exception.
        /// </summary>
        public AppErrorCode ErrorCode { get; init; }

        /// <summary>
        /// Gets the non-technical message intended for end-users or clients.
        /// </summary>
        public string NonTechnicalMessage { get; init; }

        /// <summary>
        /// Gets the technical message intended for developers or logging purposes.
        /// </summary>
        public string TechnicalMessage { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppException"/> class.
        /// </summary>
        /// <param name="errorCode">The error code associated with the exception.</param>
        /// <param name="nonTechnicalMessage">The non-technical message for end-users or clients.</param>
        /// <param name="technicalMessage">The technical message for developers or logging (optional).</param>
        public AppException(AppErrorCode errorCode, string nonTechnicalMessage, string? technicalMessage = null)
            : base(!string.IsNullOrWhiteSpace(technicalMessage) ? $"{technicalMessage} {nonTechnicalMessage}" : nonTechnicalMessage)
        {
            ErrorCode = errorCode;
            NonTechnicalMessage = nonTechnicalMessage;
            TechnicalMessage = technicalMessage ?? nonTechnicalMessage;
        }
    }
}