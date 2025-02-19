// Copyright (c) Currency Converter. All rights reserved.
// Licensed under the MIT License.

using CurrencyConverter.Models;

namespace CurrencyConverter.Tests.Models;

/// <summary>
/// Contains tests for the currency converter model classes.
/// </summary>
public class ModelsTests
{
    [Fact]
    public void CurrencyRate_Initialization_SetsPropertiesCorrectly()
    {
        var rate = new CurrencyRateFromUSD
        {
            CountryCode = "USD",
            CurrencyName = "US Dollar",
            RateFromUSDToCurrency = 1.0m
        };

        Assert.Equal("USD", rate.CountryCode);
        Assert.Equal("US Dollar", rate.CurrencyName);
        Assert.Equal(1.0m, rate.RateFromUSDToCurrency);
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
