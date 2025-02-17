namespace CurrencyConverter.ServiceDefaults.Exceptions
{
    /// <summary>
    /// Represents error codes used in the application for consistent error handling and reporting.
    /// </summary>
    public enum AppErrorCode
    {
        /// <summary>
        /// A generic error code for unclassified errors.
        /// </summary>
        GENERIC = 1,

        /// <summary>
        /// Indicates that the requested operation is not allowed or permitted.
        /// </summary>
        NOT_ALLOWED_OPERATION = 50,

        /// <summary>
        /// Indicates that one or more input parameters are invalid or malformed.
        /// </summary>
        INVALID_PARAMETER = 100,

        /// <summary>
        /// Indicates a failure or issue with the third-party currency converter system.
        /// </summary>
        CURRENCY_CONVERTER_THIRD_PARTY_SYSTEM_FAILURE = 150,

        /// <summary>
        /// Indicates that the requested currency is not supported by the third-party system.
        /// </summary>
        CURRENCY_CONVERTER_NOT_SUPPORTED_CURRENCY = 151,
    }
}