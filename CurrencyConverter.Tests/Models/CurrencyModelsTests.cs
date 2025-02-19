using CurrencyConverter.Models;

namespace CurrencyConverter.Tests.Models
{
    public class CurrencyModelsTests
    {
        [Fact]
        public void CountryPairCurrencyRate_Initialization_SetsPropertiesCorrectly()
        {
            var rate = new CountryPairCurrencyRate
            {
                FromCountry = "USD",
                ToCountry = "EUR",
                ConversionRate = 0.85m
            };

            Assert.Equal("USD", rate.FromCountry);
            Assert.Equal("EUR", rate.ToCountry);
            Assert.Equal(0.85m, rate.ConversionRate);
        }

        [Theory]
        [InlineData("USD", "EUR", "EUR", "USD")] // Expected order is ("EUR", "USD")
        [InlineData("EUR", "USD", "EUR", "USD")]
        public void CountryPairCurrencyRate_CreateKey_NormalizesOrder(
            string from, 
            string to, 
            string expectedFrom, 
            string expectedTo)
        {
            var key = CountryPairCurrencyRate.CreateKey(from, to);
            Assert.Equal(expectedFrom, key.FromCountry);
            Assert.Equal(expectedTo, key.ToCountry);
        }

        [Fact]
        public void ConversionRequest_Initialization_SetsPropertiesCorrectly()
        {
            var request = new ConversionRequest
            {
                FromCountry = "USD",
                ToCountry = "EUR",
                Amount = 100.0m
            };

            Assert.Equal("USD", request.FromCountry);
            Assert.Equal("EUR", request.ToCountry);
            Assert.Equal(100.0m, request.Amount);
        }

        [Fact]
        public void ConversionResponse_Initialization_SetsPropertiesCorrectly()
        {
            var response = new ConversionResponse
            {
                ConvertedAmount = 85.0m
            };

            Assert.Equal(85.0m, response.ConvertedAmount);
        }
    }
}