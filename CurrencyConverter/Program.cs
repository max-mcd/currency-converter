using CurrencyConverter.Helpers;
using CurrencyConverter.Models;
using CurrencyConverter.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpsPolicy;
var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5081); // HTTP port
    serverOptions.ListenAnyIP(7197, listenOptions =>
    {
        listenOptions.UseHttps();
    }); // HTTPS port
});

// Configure HTTPS
var httpsPort = builder.Configuration.GetValue<int?>("Https:Port");
if (httpsPort.HasValue)
{
    builder.Services.Configure<HttpsRedirectionOptions>(options =>
    {
        options.HttpsPort = httpsPort.Value;
    });
}

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register our services
var csvFilePath = Path.Combine(builder.Environment.ContentRootPath, "..", "Data", "conversion_rates.csv");
builder.Services.AddSingleton<ICurrencyRateService>(sp => new CurrencyRateService(csvFilePath));
builder.Services.AddSingleton<ConversionRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Initialize currency rates
var rateService = app.Services.GetRequiredService<ICurrencyRateService>();
await rateService.InitializeRatesAsync();

app.MapPost("/api/convert", async (
    [FromBody] ConversionRequest request,
    ICurrencyRateService rateService,
    ConversionRequestValidator validator,
    HttpContext context) =>
{
    // Handle model validation errors
    if (!context.Request.HasJsonContentType() || !context.Request.Body.CanRead)
    {
        return Results.BadRequest(new ErrorResponse(
            "Invalid request format",
            new { Error = "Request must be valid JSON with proper decimal values" }
        ));
    }

    // Validate request
    var (isValid, error) = validator.ValidateRequest(request);
    if (!isValid)
    {
        return error!.Error switch
        {
            string msg when msg.Contains("Invalid currency format") => Results.BadRequest(error),
            string msg when msg.Contains("Currency not supported") => Results.NotFound(error),
            _ => Results.BadRequest(error)
        };
    }

    try
    {
        // Get conversion rate and calculate result
        var rate = rateService.GetConversionRate(request.FromCountry, request.ToCountry);
        var convertedAmount = Math.Round(request.Amount * rate, 2, MidpointRounding.AwayFromZero);

        return Results.Ok(new ConversionResponse
        {
            ConvertedAmount = convertedAmount
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An error occurred while processing the conversion"
        );
    }
})
.WithName("ConvertCurrency")
.WithOpenApi()
.Produces<ConversionResponse>(200)
.Produces<ErrorResponse>(400)
.Produces<ErrorResponse>(404)
.Produces<ProblemDetails>(500);

app.Run();