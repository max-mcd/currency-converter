using CurrencyConverter.Models;
using Xunit;

namespace CurrencyConverter.Tests.Models
{
    public class CurrencyCodeValidatorTests
    {
        [Theory]
        [InlineData("USD")]
        [InlineData("EUR")]
        [InlineData("GBP")]
        [InlineData("JPY")]
        public void IsValid_ValidCodes_ReturnsTrue(string code)
        {
            Assert.True(CurrencyCodeValidator.IsValid(code));
        }

        [Theory]
        [InlineData("usd", "USD")]   // Lowercase
        [InlineData("Eur", "EUR")]   // Mixed case
        [InlineData("gbp", "GBP")]   // All lowercase
        public void IsValid_DifferentCases_ValidatesCorrectly(string input, string expected)
        {
            Assert.True(CurrencyCodeValidator.IsValid(input));
        }

        [Theory]
        [InlineData("")]             // Empty string
        [InlineData(" ")]            // Whitespace
        [InlineData(null)]           // Null
        [InlineData("US")]           // Too short
        [InlineData("USDD")]         // Too long
        [InlineData("12A")]          // Contains numbers
        [InlineData("US$")]          // Contains symbols
        [InlineData("ЫЫЫ")]          // Cyrillic characters
        [InlineData("元元元")]        // Chinese characters
        public void IsValid_InvalidCodes_ReturnsFalse(string code)
        {
            Assert.False(CurrencyCodeValidator.IsValid(code));
        }
    }
}