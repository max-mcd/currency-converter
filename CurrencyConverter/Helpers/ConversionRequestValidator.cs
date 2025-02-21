using CurrencyConverter.Models;
using CurrencyConverter.Services;

namespace CurrencyConverter.Helpers
{
    /// <summary>
    /// Validates currency conversion requests.
    /// </summary>
    public class ConversionRequestValidator
    {
        private readonly ICurrencyRateService _rateService;

        public ConversionRequestValidator(ICurrencyRateService rateService)
        {
            _rateService = rateService;
        }

        /// <summary>
        /// Validates a conversion request.
        /// </summary>
        /// <param name="request">The conversion request to validate.</param>
        /// <returns>A tuple containing whether the request is valid and an error response if invalid.</returns>
        public (bool IsValid, ErrorResponse? Error) ValidateRequest(ConversionRequest request)
        {
            // Validate currency code format
            var codesToCheck = new[] { request.FromCountry, request.ToCountry };
            var invalidCodes = codesToCheck
                .Where(code => !CurrencyCodeValidator.IsValid(code))
                .ToArray();

            if (invalidCodes.Any())
            {
                return (false, new ErrorResponse(
                    "Invalid currency format. Currency codes must be 3 letters.",
                    new ValidationErrorDetails(invalidCodes)
                ));
            }

            // Validate amount
            const decimal MaxAmount = 999999999.99M; // Set a reasonable maximum amount
            
            if (request.Amount <= 0)
            {
                return (false, new ErrorResponse(
                    "Amount must be greater than zero",
                    new { Amount = request.Amount }
                ));
            }
            
            if (request.Amount > MaxAmount)
            {
                return (false, new ErrorResponse(
                    $"Amount must not exceed {MaxAmount:N2}",
                    new { Amount = request.Amount, MaximumAllowed = MaxAmount }
                ));
            }
            
            // Ensure amount has no more than 2 decimal places
            var amountString = request.Amount.ToString("0.00########");
            if (amountString.Contains('.') && amountString.Split('.')[1].Length > 2)
            {
                return (false, new ErrorResponse(
                    "Amount cannot have more than 2 decimal places",
                    new { Amount = request.Amount }
                ));
            }

            // Check if currencies are supported
            if (!_rateService.IsSupportedCurrency(request.FromCountry))
            {
                return (false, new ErrorResponse(
                    "Currency not supported",
                    new { Currency = request.FromCountry }
                ));
            }

            if (!_rateService.IsSupportedCurrency(request.ToCountry))
            {
                return (false, new ErrorResponse(
                    "Currency not supported",
                    new { Currency = request.ToCountry }
                ));
            }

            return (true, null);
        }
    }
}