using System.Text.RegularExpressions;

namespace CurrencyConverter.Models
{
    /// <summary>
    /// Provides validation for currency codes.
    /// </summary>
    public static class CurrencyCodeValidator
    {
        private static readonly Regex ValidCurrencyCodePattern = new Regex("^[A-Z]{3}$", RegexOptions.Compiled);

        /// <summary>
        /// Validates if a string is a valid currency code.
        /// </summary>
        /// <param name="code">The currency code to validate.</param>
        /// <returns>True if the code is valid (exactly 3 letters A-Z), false otherwise.</returns>
        public static bool IsValid(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            return ValidCurrencyCodePattern.IsMatch(code.ToUpperInvariant());
        }
    }
}