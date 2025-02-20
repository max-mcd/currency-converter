namespace CurrencyConverter.Models
{
    /// <summary>
    /// Represents an error response from the API.
    /// </summary>
    /// <param name="Error">The error message.</param>
    /// <param name="Details">Additional error details (optional).</param>
    public record ErrorResponse(string Error, object? Details = null);

    /// <summary>
    /// Represents validation error details containing invalid currency codes.
    /// </summary>
    /// <param name="InvalidCodes">Collection of invalid currency codes.</param>
    public record ValidationErrorDetails(IEnumerable<string> InvalidCodes);
}