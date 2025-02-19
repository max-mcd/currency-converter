using System.Globalization;
using CurrencyConverter.Models;
using System.Collections.Concurrent;
using NLog;

namespace CurrencyConverter.Services
{
    /// <summary>
    /// Provides currency conversion rate services.
    /// This service initializes currency conversion rates from a CSV file and
    /// allows for direct and cross-rate conversion lookups.
    /// </summary>
    public class CurrencyRateService : ICurrencyRateService
    {
        private const string USD = "USD";
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<(string FromCountry, string ToCountry), decimal> _rates 
            = new ConcurrentDictionary<(string FromCountry, string ToCountry), decimal>();

        private readonly string _csvFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyRateService"/> class.
        /// </summary>
        /// <param name="csvFilePath">The path to the CSV file containing initial currency rates.</param>
        public CurrencyRateService(string csvFilePath)
        {
            _csvFilePath = csvFilePath;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This method reads the CSV file and creates a dictionary of currency conversion rates.
        ///  Expected format of the CSV file: <c>CountryCode,CurrencyName,RateFromUSDToCurrency</c>
        /// </remarks>
        public async Task InitializeRatesAsync()
        {
            if (!File.Exists(_csvFilePath))
                throw new FileNotFoundException($"Rates file not found: {_csvFilePath}");

            var lines = await File.ReadAllLinesAsync(_csvFilePath);
            
            foreach (var line in lines.Skip(1)) // Skip header row
            {
                var parts = line.Split(',');
                if (parts.Length < 3) continue;

                var countryCode = parts[0].Trim().ToUpperInvariant();

                if (!CurrencyCodeValidator.IsValid(countryCode))
                {
                    Logger.Warn($"Invalid currency code format {countryCode} in line: {line}");
                    continue;
                }

                // If the rate is invalid or zero, log a warning and skip this line.
                if (
                    !decimal.TryParse(
                        parts[2],
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var rateFromUSD
                    ) || rateFromUSD == 0)
                {
                    Logger.Warn($"Invalid rate for currency code {countryCode} in line: {line}");
                    continue;
                }
                
                // Store the direct and reciprocal conversion rates using normalized keys.
                _rates[(USD, countryCode)] = rateFromUSD;
                _rates[(countryCode, USD)] = 1 / rateFromUSD;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// If a direct conversion rate exists between the two specified currencies,
        /// the method that value. If not, it calculates the cross rate using USD as an intermediary.
        /// The cross rate is computed as:
        /// <c>ConversionRate(fromCountry, toCountry) = (ConversionRate(fromCountry, USD) * ConversionRate(USD, toCountry))</c>
        /// </remarks>
        public decimal GetConversionRate(string fromCountry, string toCountry)
        {
            fromCountry = fromCountry.ToUpperInvariant();
            toCountry = toCountry.ToUpperInvariant();

            // Validate that both currencies exist in the cache.
            foreach (var currency in new[] { fromCountry, toCountry })
            {
                if (!IsSupportedCurrency(currency))
                    throw new ArgumentException($"No conversion rate found for {currency}");
            }

            if (fromCountry == toCountry)
                return 1m;

            // Try to fetch from the cache first.
            if (_rates.TryGetValue((fromCountry, toCountry), out var directRate))
            {
                return directRate;
            }

            // If direct rate not found in cache we calculate the cross rate via USD.
            var originCurrencyToUsdRate = GetRateOrThrow((fromCountry, USD), fromCountry);
            var usdToTargetRate = GetRateOrThrow((USD, toCountry), toCountry);

            // Calculate and store the cross rate - e.g. (CAD-to-USD) * (USD-to-MXN)
            var crossRate = originCurrencyToUsdRate * usdToTargetRate;
            _rates[(fromCountry, toCountry)] = crossRate;
            _rates[(toCountry, fromCountry)] = 1 / crossRate;

            return crossRate;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This method checks if the normalized currency code appears as either 
        /// the source or target in any cached rate.
        /// </remarks>
        public bool IsSupportedCurrency(string countryCode)
        {
            var normalizedCountryCode = countryCode.ToUpperInvariant();
            return _rates.Any(r =>
                r.Key.FromCountry == normalizedCountryCode ||
                r.Key.ToCountry == normalizedCountryCode);
        }

        /// <summary>   
        /// Gets the conversion rate for the specified currency pair.
        /// </summary>
        /// <param name="key">The currency pair to convert.</param>
        /// <param name="currency">The currency to convert.</param>
        /// <returns>The conversion rate.</returns>
        /// <exception cref="ArgumentException">Thrown when the conversion rate is not found.</exception>
        private decimal GetRateOrThrow((string, string) key, string currency)
        {
            if (!_rates.TryGetValue(key, out var rate))
                throw new ArgumentException($"No conversion rate found for {currency}");
            return rate;
        }
    }
}
