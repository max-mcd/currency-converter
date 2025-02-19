// <copyright file="CurrencyModels.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CurrencyConverter.Models
{
    public record CurrencyRateFromUSD
    {
        public required string CountryCode { get; init; }
        public required string CurrencyName { get; init; }
        public decimal RateFromUSDToCurrency { get; init; }
    }

    public record ConversionRequest
    {
        public required string FromCountry { get; init; }
        public required string ToCountry { get; init; }
        public decimal Amount { get; init; }
    }

    public record ConversionResponse
    {
        public decimal ConvertedAmount { get; init; }
    }
}
