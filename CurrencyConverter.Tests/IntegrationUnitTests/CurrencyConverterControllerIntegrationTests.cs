using CurrencyConverter.Api.Dtos.CurrencyConverter.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CurrencyConverter.Tests.IntegrationUnitTests
{
    public class CurrencyConverterControllerIntegrationTests : IClassFixture<IntegrationTestsFixture>
    {
        private readonly HttpClient _httpClient;


        // Base URIs
        private const string BaseUri = "/api/v1/CurrencyConverter";
        private const string _latestRatesUri = $"{BaseUri}/rates/latest";
        private const string _convertUri = $"{BaseUri}/convert";
        private const string _historicalRatesUri = $"{BaseUri}/rates/paged";

        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
        public CurrencyConverterControllerIntegrationTests(IntegrationTestsFixture fixture)
        {
            _httpClient = fixture.HttpClient;
        }

        #region Helper Methods

        private static string GetUserToken()
        {
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjFlMDc1NzM5LTUzZjItNGYwNi1hMjBmLTg1ZjVjZDE0OGEwMSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6IkxhdXJ5bjkzQHlhaG9vLmNvbSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJMYXVyeW45M0B5YWhvby5jb20iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJVc2VyIiwiZXhwIjo0ODk1NDk3MjM5LCJpc3MiOiJDdXJyZW5jeUNvbnZlcnRlciIsImF1ZCI6IkN1cnJlbmN5Q29udmVydGVyQ2xpZW50cyJ9.FmNNXXxL9BqVN7pkiTcFExwa2bTKCyWxb91_OiubbFk";
        }

        private async Task<T> ExecuteGetRequest<T>(string uri, HttpStatusCode expectedStatus = HttpStatusCode.OK, bool authorized = true)
        {
            if (authorized)
            {
                var token = GetUserToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.GetAsync(uri);
            Assert.Equal(expectedStatus, response.StatusCode);
            return await DeserializeResponse<T>(response);
        }

        private async Task ExecuteGetRequest(string uri, HttpStatusCode expectedStatus = HttpStatusCode.OK, bool authorized = true)
        {
            if (authorized)
            {
                var token = GetUserToken();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.GetAsync(uri);
            Assert.Equal(expectedStatus, response.StatusCode);
        }


        private void ClearAuthorizationHeader()
{
    _httpClient.DefaultRequestHeaders.Remove("Authorization");
}
        private async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseString, _jsonOptions)!;
        }

        private static Dictionary<string, string> CreateValidLatestRatesParams() => new()
        {
            ["baseCurrency"] = "USD"
        };

        private static Dictionary<string, string> CreateValidConvertParams() => new()
        {
            ["amount"] = "100",
            ["baseCurrency"] = "USD",
            ["targetCurrency"] = "EUR"
        };

        private static Dictionary<string, string> CreateValidHistoricalRatesParams() => new()
        {
            ["baseCurrency"] = "USD",
            ["targetCurrency"] = "EUR",
            ["pageNumber"] = "1",
            ["pageSize"] = "10",
            ["startDate"] = DateTime.UtcNow.AddMonths(-5).ToString("yyyy-MM-dd"),
            ["endDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
        };

        private static string BuildQueryString(Dictionary<string, string> parameters)
        {
            string query = string.Join("&", parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value.ToString(CultureInfo.InvariantCulture))}"));
            return query;
        }

        #endregion

        #region Latest Rates Tests

        [Theory]
        [InlineData("USD")]
        [InlineData("EUR")]
        public async Task GetLatestRates_ShouldReturnValidResponse_WhenParametersValid(string baseCurrency)
        {
            // Arrange
            var parameters = CreateValidLatestRatesParams();
            if (baseCurrency == null) parameters.Remove("baseCurrency");
            else parameters["baseCurrency"] = baseCurrency;

            // Act
            var response = await ExecuteGetRequest<GetLatestRatesResponseAppDto>(
                $"{_latestRatesUri}?{BuildQueryString(parameters)}");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(baseCurrency ?? "EUR", response.Base);
            Assert.NotEmpty(response.Rates);
        }

        [Theory]
        [InlineData("invalidCurrency")]
        [InlineData("123")]
        [InlineData("US")]
        public async Task GetLatestRates_ShouldReturnBadRequest_ForInvalidBaseCurrency(string invalidCurrency)
        {
            // Arrange & Act
            await ExecuteGetRequest($"{_latestRatesUri}?baseCurrency={invalidCurrency}", HttpStatusCode.BadRequest);
        }
        #endregion

        #region Conversion Tests
        [Theory]
        [InlineData(100, "USD", "EUR")]
        [InlineData(5.5, "GBP", "KRW")]
        public async Task Convert_ShouldReturnValidConversion_ForValidParameters(decimal amount, string from, string to)
        {
            // Arrange
            var parameters = CreateValidConvertParams();
            parameters["amount"] = amount.ToString();
            parameters["baseCurrency"] = from;
            parameters["targetCurrency"] = to;

            // Act
            var response = await ExecuteGetRequest<ConvertResponseAppDto>(
                $"{_convertUri}?{BuildQueryString(parameters)}");

            // Assert
            Assert.NotNull(response);
            Assert.Equal(amount, response.BaseAmount);
            Assert.Equal(from, response.BaseCurrency);
            Assert.Equal(to, response.TargetCurrency);
            Assert.True(response.TargetAmount > 0);
        }

        [Theory]
        [InlineData("-100", "USD", "EUR")]    // Negative amount
        [InlineData("100", "XXX", "EUR")]     // Invalid source
        [InlineData("100", "USD", "XXX")]    // Invalid target
        [InlineData("notNumber", "USD", "EUR")] // Invalid amount type
        public async Task Convert_ShouldReturnBadRequest_ForInvalidParameters(
            string amount, string from, string to)
        {
            // Arrange & Act
            await ExecuteGetRequest($"{_convertUri}?amount={amount}&baseCurrency={from}&targetCurrency={to}", HttpStatusCode.BadRequest);
        }
        #endregion

        #region Historical Rates Tests
        [Theory]
        [InlineData(1, 10)]
        [InlineData(2, 5)]
        [InlineData(1, 50)]
        public async Task GetHistoricalRates_ShouldHandlePagination(int pageNumber, int pageSize)
        {
            // Arrange
            var parameters = CreateValidHistoricalRatesParams();
            parameters["pageNumber"] = pageNumber.ToString();
            parameters["pageSize"] = pageSize.ToString();

            // Act
            var response = await ExecuteGetRequest<GetHistoricalRatesResponseAppDto>(
                $"{_historicalRatesUri}?{BuildQueryString(parameters)}");

            // Assert
            Assert.Equal(pageNumber, response.PageNumber);
            Assert.Equal(pageSize, response.PageSize);
            Assert.True(response.TotalPages >= 1);
            Assert.True(response.TotalItems <= response.TotalPages * response.PageSize);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        public async Task GetHistoricalRates_ShouldReturnBadRequest_ForInvalidParameters(int pageNumber, int pageSize)
        {
            // Arrange
            var parameters = CreateValidHistoricalRatesParams();
            parameters["pageNumber"] = pageNumber.ToString();
            parameters["pageSize"] = pageSize.ToString();
            

            // Act
            await ExecuteGetRequest($"{_historicalRatesUri}?{BuildQueryString(parameters)}", HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("2024-01-01", "2022-01-01")]
        [InlineData("2024-13-01", "2025-01-01")]
        [InlineData("2024-01-01", "2025-13-01")]
        [InlineData("non_data", "2025-01-01")]
        [InlineData("2024-01-01", "non_data")]
        public async Task GetHistoricalRates_ShouldReturnBadRequest_ForInvalidDates(string startDateStr, string endDateStr)
        {
            // Arrange
            var parameters = CreateValidHistoricalRatesParams();
            parameters["startDate"] = startDateStr;
            parameters["endDate"] = endDateStr;

            // Act
            await ExecuteGetRequest($"{_historicalRatesUri}?{BuildQueryString(parameters)}", HttpStatusCode.BadRequest);
        }

        #endregion

        #region Security Tests
        [Theory]
        [InlineData(_latestRatesUri)]
        [InlineData(_convertUri)]
        [InlineData(_historicalRatesUri)]
        public async Task Endpoints_ShouldRequireAuthentication(string endpoint)
        {
            // Arrange
            var parameters = endpoint switch
            {
                _ when endpoint == _latestRatesUri => CreateValidLatestRatesParams(),
                _ when endpoint == _convertUri => CreateValidConvertParams(),
                _ => CreateValidHistoricalRatesParams()
            };

            // Act
            ClearAuthorizationHeader();
            var response = await _httpClient.GetAsync($"{endpoint}?{BuildQueryString(parameters)}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AuthenticatedRequests_ShouldSucceed()
        {
            // Act & Assert
            var latestResponse = await ExecuteGetRequest<GetLatestRatesResponseAppDto>(
                $"{_latestRatesUri}?{BuildQueryString(CreateValidLatestRatesParams())}");
            Assert.NotNull(latestResponse);

            var convertResponse = await ExecuteGetRequest<ConvertResponseAppDto>(
                $"{_convertUri}?{BuildQueryString(CreateValidConvertParams())}");
            Assert.NotNull(convertResponse);

            var historyResponse = await ExecuteGetRequest<GetHistoricalRatesResponseAppDto>(
                $"{_historicalRatesUri}?{BuildQueryString(CreateValidHistoricalRatesParams())}");
            Assert.NotNull(historyResponse);
        }
        #endregion
    }
}