namespace CurrencyConverter.ServiceDefaults.Exceptions
{
    public enum AppErrorCode
    {
        GENERIC = 1,
        NOT_ALLOWED_OPERATION =50,

        INVALID_PARAMETER = 100,


        CURRENCY_CONVERTER_THIRD_PARTY_SYSTEM_FAILURE = 150,
        CURRENCY_CONVERTER_NOT_SUPPORTED_CURRENCY = 151,
    }
}
