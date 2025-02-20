# Currency Converter API

A .NET Core API that provides currency conversion functionality using rates from a CSV file. The API offers robust validation, error handling, and comprehensive testing.

## Features

- Currency conversion between supported currency pairs
- CSV-based conversion rates
- Input validation for currency codes and amounts
- Comprehensive error handling
- Swagger/OpenAPI documentation
- Unit tests for all components

## Prerequisites

- .NET 7.0 SDK or later
- Visual Studio 2022 or VS Code with C# extensions

## Getting Started

1. Clone the repository:
```bash
git clone https://github.com/max-mcd/currency-converter.git
cd currency-converter
```

2. Build the solution:
```bash
dotnet build
```

3. Run the tests:
```bash
dotnet test
```

4. Run the application:
```bash
cd CurrencyConverter
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5081
- HTTPS: https://localhost:7197

## API Documentation

### Convert Currency

Converts an amount from one currency to another.

**Endpoint:** POST `/api/convert`

**Request Format:**
```json
{
    "fromCountry": "USD",
    "toCountry": "EUR",
    "amount": 100.00
}
```

**Response Formats:**

1. Success (200 OK):
```json
{
    "convertedAmount": 92.14
}
```

2. Invalid Format (400 Bad Request):
```json
{
    "error": "Invalid currency format. Currency codes must be 3 letters.",
    "details": {
        "invalidCodes": ["US"]
    }
}
```

3. Currency Not Found (404 Not Found):
```json
{
    "error": "Currency not supported",
    "details": {
        "invalidCodes": ["XYZ"]
    }
}
```

4. Invalid Amount (400 Bad Request):
```json
{
    "error": "Amount must be between 0.01 and 999999999.99"
}
```

## Project Structure

- `CurrencyConverter/`: Main API project
  - `Models/`: Data models and DTOs
  - `Services/`: Business logic and currency rate service
  - `Helpers/`: Validation and utility classes
- `CurrencyConverter.Tests/`: Unit tests
- `Data/`: Contains conversion rates CSV file

## Configuration

- `appsettings.Development.json`: Development environment configuration
  - Logging levels
  - HTTPS port settings
- `launchSettings.json`: Development-time launch profiles
  - HTTP/HTTPS URLs
  - Environment variables
  - Swagger UI configuration

## Testing

The project includes comprehensive unit tests for:
- Currency code validation
- Conversion request validation
- Currency rate service
- Data models

Run tests using:
```bash
dotnet test
```

## Development Tools

- Visual Studio Code REST Client: Use `CurrencyConverter.http` for API testing
- Swagger UI: Available at `/swagger` when running in development mode

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.