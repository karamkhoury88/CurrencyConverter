namespace CurrencyConverter.Api.Common
{
    public struct CurrencyConverterAuthorizationPolicy
    {
        public const string USER = "RequireUserRole";
        public const string ADMIN = "RequireAdminRole";
    }
}
