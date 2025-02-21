using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Models
{
    /// <summary>
    /// Represents the conversion rate between a pair of countries' currencies.
    /// This record contains the source and target country codes and the conversion rate.
    /// </summary>
    public record CountryPairCurrencyRate
    {
        public required string FromCountry { get; init; }
        public required string ToCountry { get; init; }
        public decimal ConversionRate { get; init; }

        /// <summary>
        /// Creates a canonical key for a pair of country codes.
        /// This method orders the country codes using Unicode ordinal ordering 
        /// so that the key is consistent regardless of the order in which the countries are provided.
        /// </summary>
        /// <param name="fromCountry">The first country code.</param>
        /// <param name="toCountry">The second country code.</param>
        /// <returns>A tuple containing the two country codes in canonical order.</returns>
        public static (string FromCountry, string ToCountry) CreateKey(string fromCountry, string toCountry) =>
            string.Compare(fromCountry, toCountry, StringComparison.Ordinal) < 0
                ? (fromCountry, toCountry)
                : (toCountry, fromCountry);
    }

    /// <summary>
    /// Represents a request for currency conversion.
    /// Contains the source country code, the target country code, and the amount to be converted.
    /// </summary>
    /// <param name="FromCountry">The three-letter country code from which we are converting the amount.</param>
    /// <param name="ToCountry">The three-letter country code to which we are converting the amount.</param>
    /// <param name="Amount">The amount to convert.</param>
    /// <returns>A record representing a currency conversion request.</returns>
    public record ConversionRequest
    {
        public required string FromCountry { get; init; }
        public required string ToCountry { get; init; }

        [Range(typeof(decimal), "0.01", "999999999.99",
            ErrorMessage = "Amount must be between {1} and {2}")]
        public decimal Amount { get; init; }
    }

    /// <summary>
    /// Represents the response after performing a currency conversion.
    /// Contains the converted amount.
    /// </summary>
    /// <param name="ConvertedAmount">The amount after conversion.</param>
    /// <returns>A record representing a currency conversion response.</returns>
    public record ConversionResponse
    {
        public decimal ConvertedAmount { get; init; }
    }
}
