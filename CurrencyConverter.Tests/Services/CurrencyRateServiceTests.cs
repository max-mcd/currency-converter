using CurrencyConverter.Services;
using Xunit;

namespace CurrencyConverter.Tests.Services
{
    public class CurrencyRateServiceTests : IDisposable
    {
        private readonly string _testDataPath;

        public CurrencyRateServiceTests()
        {
            _testDataPath = Path.GetTempFileName();
            File.WriteAllLines(_testDataPath, new[]
            {
                "CountryCode,CurrencyName,RateFromUSDToCurrency",
                "USD,United States Dollars,1",
                "CAD,Canada Dollars,1.2071",
                "MXN,Mexico Pesos,15.22",
                "CNY,China Renminbis,6.08"
            });
        }

        [Fact]
        public async Task InitializeRatesAsync_LoadsRatesCorrectly()
        {
            var service = new CurrencyRateService(_testDataPath);
            await service.InitializeRatesAsync();

            // Direct conversion rates: USD -> Target
            Assert.Equal(1.2071m, service.GetConversionRate("USD", "CAD"));
            Assert.Equal(15.22m, service.GetConversionRate("USD", "MXN"));
            Assert.Equal(6.08m, service.GetConversionRate("USD", "CNY"));

            // Reciprocal conversion rates: Target -> USD
            Assert.Equal(1 / 1.2071m, service.GetConversionRate("CAD", "USD"));
            Assert.Equal(1 / 15.22m, service.GetConversionRate("MXN", "USD"));
            Assert.Equal(1 / 6.08m, service.GetConversionRate("CNY", "USD"));
        }

        [Fact]
        public async Task InitializeRatesAsync_SkipsInvalidRows()
        {
            // Temporary CSV contains a mix of valid and invalid rows.
            var invalidTestDataPath = Path.GetTempFileName();
            File.WriteAllLines(invalidTestDataPath, new[]
            {
                "CountryCode,CurrencyName,RateFromUSDToCurrency",
                "USD,United States Dollars,1",     // valid
                "INV,InvalidCurrency,abc",         // invalid rate (non-numeric)
                "ZERO,ZeroCurrency,0",             // invalid rate (zero)
                "   ,EmptyCode,1.5",               // empty currency code (after Trim)
                "CNY,China Renminbis,6.08"         // valid
            });

            var service = new CurrencyRateService(invalidTestDataPath);
            await service.InitializeRatesAsync();

            // Expected valid currencies: only USD and CNY.
            Assert.True(service.IsSupportedCurrency("USD"));
            Assert.True(service.IsSupportedCurrency("CNY"));

            // Invalid rows should be skipped.
            Assert.False(service.IsSupportedCurrency("INV"));
            Assert.False(service.IsSupportedCurrency("ZERO"));
            Assert.False(service.IsSupportedCurrency(""));

            File.Delete(invalidTestDataPath);
        }

        [Theory]
        [InlineData("USD")]
        [InlineData("CAD")]
        [InlineData("MXN")]
        [InlineData("CNY")]
        public async Task GetConversionRate_SameCurrency_ReturnsOne(string currency)
        {
            var service = new CurrencyRateService(_testDataPath);
            await service.InitializeRatesAsync();

            var rate = service.GetConversionRate(currency, currency);
            Assert.Equal(1m, rate);
        }

        [Theory]
        // Case 1: "CAD" -> "MXN": (1 / 1.2071) * 15.22
        [InlineData("CAD", "MXN", 1/1.2071d, 15.22)]
        // Case 2: "MXN" -> "CNY": (1 / 15.22) * 6.08
        [InlineData("MXN", "CNY", 1/15.22d, 6.08)]
        // Case 3: "CNY" -> "CAD": (1 / 6.08) * 1.2071
        [InlineData("CNY", "CAD", 1/6.08d, 1.2071)]
        public async Task GetConversionRate_CrossRateCalculation(
            string originCurrency,
            string targetCurrency,
            double originCurrencyToUsdRate,
            double usdToTargetRate)
        {
            var service = new CurrencyRateService(_testDataPath);
            await service.InitializeRatesAsync();

            // Calculate expected cross rate via USD
            var expected = (decimal)(originCurrencyToUsdRate * usdToTargetRate);
            var rate = service.GetConversionRate(originCurrency, targetCurrency);
            Assert.Equal(expected, rate, 3); // Consider a match if equal to 3 decimal places

            // On subsequent calls, the cached rate should be returned.
            var cachedRate = service.GetConversionRate(originCurrency, targetCurrency);
            Assert.Equal(rate, cachedRate);

            // The reverse conversion should provide the reciprocal.
            var reverseRate = service.GetConversionRate(targetCurrency, originCurrency);
            Assert.Equal(1 / rate, reverseRate, 3);
        }

        [Fact]
        public async Task GetConversionRate_MixedCaseNormalization()
        {
            var service = new CurrencyRateService(_testDataPath);
            await service.InitializeRatesAsync();

            Assert.Equal(1.2071m, service.GetConversionRate("usd", "cad"));
            Assert.Equal(15.22m, service.GetConversionRate("UsD", "mXn"));
        }

        [Theory]
        [InlineData("INVALID", "USD")]
        [InlineData("USD", "INVALID")]
        [InlineData("INVALID", "INVALID")]
        public async Task GetConversionRate_InvalidCurrency_ThrowsArgumentException(
            string originCurrency,
            string targetCurrency)
        {
            var service = new CurrencyRateService(_testDataPath);
            await service.InitializeRatesAsync();

            var exception = Assert.Throws<ArgumentException>(() =>
                service.GetConversionRate(originCurrency, targetCurrency));
            Assert.Contains("No conversion rate found", exception.Message);
        }

        [Theory]
        [InlineData("USD")]
        [InlineData("CAD")]
        [InlineData("MXN")]
        [InlineData("CNY")]
        public async Task IsSupportedCurrency_KnownCurrency_ReturnsTrue(string currency)
        {
            var service = new CurrencyRateService(_testDataPath);
            await service.InitializeRatesAsync();

            Assert.True(service.IsSupportedCurrency(currency));
        }

        [Fact]
        public async Task IsSupportedCurrency_UnknownCurrency_ReturnsFalse()
        {
            var service = new CurrencyRateService(_testDataPath);
            await service.InitializeRatesAsync();

            Assert.False(service.IsSupportedCurrency("EUR"));
        }

        [Theory]
        [InlineData("usd")]
        [InlineData("cad")]
        [InlineData("mxn")]
        [InlineData("cNy")]
        public async Task IsSupportedCurrency_KnownCurrency_MixedCase_ReturnsTrue(string currency)
        {
            var service = new CurrencyRateService(_testDataPath);
            await service.InitializeRatesAsync();

            Assert.True(service.IsSupportedCurrency(currency));
        }

         [Fact]
        public async Task IsSupportedCurrency_UnknownCurrency_MixedCase_ReturnsFalse()
        {
            var service = new CurrencyRateService(_testDataPath);
            await service.InitializeRatesAsync();

            Assert.False(service.IsSupportedCurrency("EuR"));
        }   

        public void Dispose()
        {
            if (File.Exists(_testDataPath))
            {
                File.Delete(_testDataPath);
            }
        }
    }
}