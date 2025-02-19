namespace CurrencyConverter.Services
{
    /// <summary>
    /// Defines a service for retrieving currency exchange rates.
    /// </summary>
    public interface ICurrencyRateService
    {
        /// <summary>
        /// Initializes the currency rates cache from the data source.
        /// </summary>
        Task InitializeRatesAsync();

        /// <summary>
        /// Retrieves the conversion rate between two currencies.
        /// </summary>
        /// <param name="fromCountry">Three-letter country code from which we are converting the amount</param>
        /// <param name="toCountry">Three-letter country code to which we are converting the amount</param>
        /// <returns>A decimal conversion rate</returns>
        decimal GetConversionRate(string fromCountry, string toCountry);

        /// <summary>
        /// Checks if the given currency code is present in the cache.
        /// </summary>
        /// <param name="countryCode">The three-letter country code to check</param>
        /// <returns>True if the currency is supported, false otherwise</returns>
        bool IsSupportedCurrency(string countryCode);
    }
}
