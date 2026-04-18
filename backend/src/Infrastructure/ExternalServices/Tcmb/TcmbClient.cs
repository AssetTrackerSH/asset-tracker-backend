using System.Text;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioTracker.Infrastructure.Configuration;
using PortfolioTracker.Infrastructure.ExternalServices.Tcmb.Models;

namespace PortfolioTracker.Infrastructure.ExternalServices.Tcmb;

public class TcmbClient : ITcmbClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TcmbClient> _logger;
    private readonly TcmbOptions _options;

    public TcmbClient(
        HttpClient httpClient,
        ILogger<TcmbClient> logger,
        IOptions<TcmbOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<TcmbCurrencyResponse> GetDailyCurrencyRatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching daily currency rates from TCMB...");

            // TCMB URL for today's rates
            var url = "/kurlar/today.xml";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Read XML content with UTF-8 encoding
            var xmlContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogDebug("Received XML response from TCMB: {ContentLength} bytes", xmlContent.Length);

            // Deserialize XML
            var serializer = new XmlSerializer(typeof(TcmbCurrencyResponse));
            using var reader = new StringReader(xmlContent);
            var result = serializer.Deserialize(reader) as TcmbCurrencyResponse;

            if (result is null)
            {
                _logger.LogError("Failed to deserialize TCMB response");
                throw new InvalidOperationException("Failed to deserialize TCMB XML response");
            }

            _logger.LogInformation("Successfully fetched {CurrencyCount} currencies from TCMB for date {Date}",
                result.Currencies.Count, result.Date);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching currency rates from TCMB");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Request to TCMB timed out");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching currency rates from TCMB");
            throw;
        }
    }
}
