using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Replays;
using Battlegrounds.Serializers;
using Battlegrounds.Services;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Facades.API;

public sealed class HttpBattlegroundsServerAPI(ILogger<HttpBattlegroundsServerAPI> logger, IUserService userService, ICompanyDeserializer companyDeserializer, Configuration configuration) : IBattlegroundsServerAPI {

    private readonly ILogger<HttpBattlegroundsServerAPI> _logger = logger;
    private readonly HttpClient _httpClient = new();
    private readonly IUserService _userService = userService;
    private readonly ICompanyDeserializer _companyDeserializer = companyDeserializer;
    private readonly Configuration _configuration = configuration;

    public static readonly string UploadCompanyEndpoint = "/api/v1/companies/upload"; // Requires authentication
    public static readonly string DeleteCompanyEndpoint = "/api/v1/companies/delete"; // Requires authentication
    public static readonly string DownloadCompanyEndpoint = "/api/v1/companies/download"; // No authentication required

    public static readonly string ReportMatchResultsEndpoint = "/api/v1/match/report"; // Requires authentication

    public string BaseUrl => $"{_configuration.BattlegroundsServerHost}:{_configuration.BattlegroundsHttpServerPort}";

    public async ValueTask<bool> DeleteCompanyAsync(string companyId) {

        string endpoint = $"{BaseUrl}{DeleteCompanyEndpoint}";
        var parameters = new Dictionary<string, string> {
            { "guid", companyId },
            { "userId", (await _userService.GetLocalUserAsync())!.UserId } // Temp for testing, should rely on user claim in the future
        };

        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";
        _logger.LogInformation("Sending DELETE request to {RequestUri}", requestUri);


        HttpRequestMessage request = await GetHttpRequestWithAuthHeaders(HttpMethod.Delete, requestUri);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");

        HttpResponseMessage response = await SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Company {CompanyId} deleted successfully.", companyId);
            return true;
        } else {
            _logger.LogError("Failed to delete company {CompanyId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", companyId, response.StatusCode, response.ReasonPhrase);
            return false;
        }

    }

    public async Task<Company?> GetCompanyAsync(string companyId, string companyUserId) {

        string endpoint = $"{BaseUrl}{DownloadCompanyEndpoint}";
        var parameters = new Dictionary<string, string> {
            { "guid", companyId },
            { "userId", companyUserId }
        };
        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";

        _logger.LogInformation("Sending GET request to {RequestUri}", requestUri);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri); // No authentication required for this endpoint
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");

        HttpResponseMessage response = await SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Company {CompanyId} retrieved successfully.", companyId);
            Stream contentStream = await response.Content.ReadAsStreamAsync();
            return _companyDeserializer.DeserializeCompany(contentStream);
        } else {
            _logger.LogError("Failed to retrieve company {CompanyId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", companyId, response.StatusCode, response.ReasonPhrase);
            return null;
        }

    }

    public async ValueTask<bool> UploadCompanyAsync(string companyId, string faction, Stream serializedCompanyStream) {

        string endpoint = $"{BaseUrl}{UploadCompanyEndpoint}";
        var parameters = new Dictionary<string, string> {
            { "guid", companyId },
            { "faction", faction },
            { "userId", (await _userService.GetLocalUserAsync())!.UserId } // Temp for testing, should rely on user claim in the future
        };

        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";

        _logger.LogInformation("Sending POST request to {RequestUri}", requestUri);
        HttpRequestMessage request = await GetHttpRequestWithAuthHeaders(HttpMethod.Post, requestUri);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");
        request.Content = new StreamContent(serializedCompanyStream);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        request.Content.Headers.ContentLength = serializedCompanyStream.Length; // Set content length for the stream

        HttpResponseMessage response = await SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Company {CompanyId} uploaded successfully.", companyId);
            return true;
        } else {
            _logger.LogError("Failed to upload company {CompanyId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", companyId, response.StatusCode, response.ReasonPhrase);
            return false;
        }

    }

    public async ValueTask<bool> ReportMatchResults(MatchResult result) {

        if (result is null) {
            _logger.LogError("Match result is null. Cannot report match results.");
            return false;
        }

        if (string.IsNullOrEmpty(result.LobbyId)) {
            _logger.LogError("LobbyId is missing. Cannot report match results.");
            return false;
        }

        string endpoint = $"{BaseUrl}{ReportMatchResultsEndpoint}";
        var parameters = new Dictionary<string, string> {
            { "guid", result.LobbyId },
            { "userId", (await _userService.GetLocalUserAsync())!.UserId }, // Temp for testing, should rely on user claim in the future
        };

        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";
        _logger.LogInformation("Sending POST request to {RequestUri}", requestUri);

        HttpRequestMessage request = await GetHttpRequestWithAuthHeaders(HttpMethod.Post, requestUri);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");
        request.Content = JsonContent.Create(result);

        HttpResponseMessage response = await SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Match results for lobby {LobbyId} reported successfully.", result.LobbyId);
            return true;
        } else {
            _logger.LogError("Failed to report match results for lobby {LobbyId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", result.LobbyId, response.StatusCode, response.ReasonPhrase);
            return false;
        }

    }

    private static string ToUrlEncodedString(Dictionary<string, string> parameters) {
        return string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
    }

    private async Task<HttpRequestMessage> GetHttpRequestWithAuthHeaders(HttpMethod method, string requestUri) {
        string token = await _userService.GetLocalUserTokenAsync(); // Will refresh token if expired
        if (string.IsNullOrEmpty(token)) {
            throw new InvalidOperationException("No authentication token found for the local user. Cannot perform API operations that require authentication.");
        }
        HttpRequestMessage request = new(method, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request) {
        using var context = new CancellationTokenSource(_configuration.API.RequestTimeout);
        try {
            return await _httpClient.SendAsync(request, context.Token);
        } catch (TaskCanceledException) when (!context.Token.IsCancellationRequested) {
            _logger.LogError("Request to {RequestUri} was canceled.", request.RequestUri);
            return new HttpResponseMessage(HttpStatusCode.RequestTimeout) {
                RequestMessage = request,
                ReasonPhrase = "Request timed out or was canceled."
            };
        } catch (TaskCanceledException) {
            _logger.LogError("Request to {RequestUri} timed out after {Timeout} seconds.", request.RequestUri, _configuration.API.RequestTimeout.TotalSeconds);
            return new HttpResponseMessage(HttpStatusCode.RequestTimeout) {
                RequestMessage = request,
                ReasonPhrase = "Request timed out or was canceled."
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while sending request to {RequestUri}.", request.RequestUri);
            return new HttpResponseMessage(HttpStatusCode.Conflict) {
                RequestMessage = request,
                ReasonPhrase = ex.Message
            };
        }
    }

}
