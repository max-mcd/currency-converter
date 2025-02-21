using CurrencyConverter.Helpers;
using CurrencyConverter.Models;
using CurrencyConverter.Services;
using Xunit;

namespace CurrencyConverter.Tests.Helpers
{
    public class ConversionRequestValidatorTests
    {
        private readonly TestCurrencyRateService _rateService;
        private readonly ConversionRequestValidator _validator;

        private class TestCurrencyRateService : ICurrencyRateService
        {
            private readonly HashSet<string> _supportedCurrencies = new() { "USD", "EUR", "GBP" };

            public Task InitializeRatesAsync() => Task.CompletedTask;

            public decimal GetConversionRate(string fromCountry, string toCountry) => 1.0m;

            public bool IsSupportedCurrency(string countryCode) => 
                _supportedCurrencies.Contains(countryCode);
        }

        public ConversionRequestValidatorTests()
        {
            _rateService = new TestCurrencyRateService();
            _validator = new ConversionRequestValidator(_rateService);
        }

        [Fact]
        public void ValidateRequest_WithInvalidCurrencyFormat_ReturnsBadRequest()
        {
            var request = new ConversionRequest
            {
                FromCountry = "US", // Invalid format (2 letters)
                ToCountry = "EUR",
                Amount = 100
            };

            var (isValid, error) = _validator.ValidateRequest(request);

            Assert.False(isValid);
            Assert.NotNull(error);
            Assert.Contains("Invalid currency format", error.Error);
            
            var details = Assert.IsType<ValidationErrorDetails>(error.Details);
            Assert.Contains("US", details.InvalidCodes);
        }

        [Fact]
        public void ValidateRequest_WithNegativeAmount_ReturnsBadRequest()
        {
            var request = new ConversionRequest
            {
                FromCountry = "USD",
                ToCountry = "EUR",
                Amount = -100
            };

            var (isValid, error) = _validator.ValidateRequest(request);

            Assert.False(isValid);
            Assert.NotNull(error);
            Assert.Contains("Amount must be greater than zero", error.Error);
            Assert.Contains("-100", error.Details?.ToString() ?? string.Empty);
        }

        [Fact]
        public void ValidateRequest_WithUnsupportedCurrency_ReturnsNotFound()
        {
            var request = new ConversionRequest
            {
                FromCountry = "XYZ", // Unsupported currency
                ToCountry = "EUR",
                Amount = 100
            };

            var (isValid, error) = _validator.ValidateRequest(request);

            Assert.False(isValid);
            Assert.NotNull(error);
            Assert.Contains("Currency not supported", error.Error);
            Assert.Contains("XYZ", error.Details?.ToString() ?? string.Empty);
        }

        [Fact]
        public void ValidateRequest_WithValidRequest_ReturnsSuccess()
        {
            var request = new ConversionRequest
            {
                FromCountry = "USD", // Known supported currency
                ToCountry = "EUR", // Known supported currency
                Amount = 100
            };

            var (isValid, error) = _validator.ValidateRequest(request);

            Assert.True(isValid);
            Assert.Null(error);
        }
    }
}