using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Gamemodes;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Replays;
using Battlegrounds.Serializers;
using Battlegrounds.Services;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Facades.API;

public sealed class HttpBattlegroundsServerAPI(
    ILogger<HttpBattlegroundsServerAPI> logger, 
    IAsyncHttpClient asyncHttpClient, 
    IUserService userService, 
    ICompanyDeserializer companyDeserializer, 
    Configuration configuration) : IBattlegroundsServerAPI {

    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web) {
        Converters = { new ReadOnlySetConverterFactory(), new LinkedListConverterFactory() }
    };

    private readonly ILogger<HttpBattlegroundsServerAPI> _logger = logger;
    private readonly IAsyncHttpClient _httpClient = asyncHttpClient;
    private readonly IUserService _userService = userService;
    private readonly ICompanyDeserializer _companyDeserializer = companyDeserializer;
    private readonly Configuration _configuration = configuration;

    public static readonly string GetLobbiesEndpoint = "/api/v1/lobbies"; // No authentication required
    public static readonly string GetLobbyResultEndpoint = "/api/v1/lobbies/result"; // No authentication required
    public static readonly string UploadCompanyEndpoint = "/api/v1/companies/upload"; // Requires authentication
    public static readonly string DeleteCompanyEndpoint = "/api/v1/companies/delete"; // Requires authentication
    public static readonly string GetCompanyInfoEndpoint = "/api/v1/companies/info"; // No authentication required
    public static readonly string GetUserCompanyInfoEndpoint = "/api/v1/companies/user-info"; // No authentication required
    public static readonly string DownloadCompanyEndpoint = "/api/v1/companies/download"; // No authentication required
    public static readonly string UploadGamemodeEndpoint = "/api/v1/gamemodes/upload"; // Requires authentication
    public static readonly string DownloadGamemodeEndpoint = "/api/v1/gamemodes/download"; // No authentication required
    public static readonly string ReportMatchResultsEndpoint = "/api/v1/match/report"; // Requires authentication
    public static readonly string ServerAvailabilityEndpoint = "/api/v1/up"; // No authentication required
    public static readonly string LatestWinconditionSrcEndpoint = "/api/v1/winconditionsrc/latest"; // No authentication required
    public static readonly string DownloadLatestWinconditionSrcEndpoint = "/api/v1/winconditionsrc/download"; // No authentication required

    public string BaseUrl => $"{_configuration.BattlegroundsServerHost}:{_configuration.BattlegroundsHttpServerPort}";

    public async ValueTask<bool> DeleteCompanyAsync(string companyId) {

        string endpoint = $"{BaseUrl}{DeleteCompanyEndpoint}";
        var parameters = new Dictionary<string, string> {
            { "guid", companyId },
        };

        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";
        _logger.LogInformation("Sending DELETE request to {RequestUri}", requestUri);


        HttpRequestMessage request = await GetHttpRequestWithAuthHeaders(HttpMethod.Delete, requestUri);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _userService.GetLocalUserTokenAsync()); // Ensure we have the latest token

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Company {CompanyId} deleted successfully.", companyId);
            return true;
        } else {
            _logger.LogError("Failed to delete company {CompanyId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", companyId, response.StatusCode, response.ReasonPhrase);
            return false;
        }

    }

    public async Task<Company?> GetCompanyAsync(string companyId, string companyUserId, DownloadProgressUpdateDelegate? progressUpdate = null) {

        string endpoint = $"{BaseUrl}{DownloadCompanyEndpoint}";
        var parameters = new Dictionary<string, string> {
            { "guid", companyId },
            { "userId", companyUserId }
        };
        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";

        _logger.LogInformation("Sending GET request to {RequestUri}", requestUri);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri); // No authentication required for this endpoint
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Company {CompanyId} retrieved successfully.", companyId);

            long? totalBytes = response.Content.Headers.ContentLength;
            long bytesDownloaded = 0;

            using Stream contentStream = await response.Content.ReadAsStreamAsync();
            using MemoryStream dataStream = new MemoryStream();
            byte[] buffer = new byte[512]; // 512B chunks, small enough to provide smooth progress updates even for small companies while not causing too much overhead for larger companies
            // In general, company files are relatively small (often around 2kb-4kb).
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0) {
                await dataStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                bytesDownloaded += bytesRead;
                progressUpdate?.Invoke(bytesDownloaded, totalBytes);
            }

            // Reset stream position to the beginning before deserialization
            dataStream.Position = 0;

            try {
                return _companyDeserializer.DeserializeCompany(dataStream);
            } catch (Exception e) {
                _logger.LogError(e, "Failed to deserialize company {CompanyId}.", companyId);
                return null;
            }

        } else {
            _logger.LogError("Failed to retrieve company {CompanyId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", companyId, response.StatusCode, response.ReasonPhrase);
            return null;
        }

    }

    public async Task<CompanyInfo?> GetCompanyInfoAsync(string companyId, string companyUserId) {

        string endpoint = $"{BaseUrl}{GetCompanyInfoEndpoint}";
        var parameters = new Dictionary<string, string> {
            { "guid", companyId },
            { "userId", companyUserId }
        };
        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";

        _logger.LogDebug("Sending GET request to {RequestUri}", requestUri);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri); // No authentication required for this endpoint
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            Stream contentStream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<CompanyInfo?>(contentStream);
        } else {
            _logger.LogError("Failed to retrieve company info for {CompanyId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", companyId, response.StatusCode, response.ReasonPhrase);
            return null;
        }

    }

    public async Task<UserCompanyInfo?> GetUserCompanyInfoAsync(string userId) {

        string endpoint = $"{BaseUrl}{GetUserCompanyInfoEndpoint}";
        var parameters = new Dictionary<string, string> {
            { "userId", userId }
        };
        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";

        _logger.LogDebug("Sending GET request to {RequestUri}", requestUri);
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri); // No authentication required for this endpoint
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            var contentStream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<UserCompanyInfo?>(contentStream);
        } else {
            _logger.LogError("Failed to retrieve user company info for user {UserId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", userId, response.StatusCode, response.ReasonPhrase);
            return null;
        }

    }

    public async ValueTask<bool> UploadCompanyAsync(string companyId, string faction, uint version, Stream serializedCompanyStream, UploadProgressUpdateDelegate? progressUpdate = null) {

        string endpoint = $"{BaseUrl}{UploadCompanyEndpoint}";
        var parameters = new Dictionary<string, string> {
            { "guid", companyId },
            { "faction", faction },
            { "version", version.ToString() }
        };

        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";

        _logger.LogInformation("Sending POST request to {RequestUri}", requestUri);
        HttpRequestMessage request = await GetHttpRequestWithAuthHeaders(HttpMethod.Post, requestUri);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _userService.GetLocalUserTokenAsync()); // Ensure we have the latest token
        request.Content = new StreamContent(new ProgressStream(serializedCompanyStream, serializedCompanyStream.Length, progressUpdate));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        request.Content.Headers.ContentLength = serializedCompanyStream.Length; // Set content length for the stream

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Company {CompanyId} uploaded successfully.", companyId);
            return true;
        } else {
            _logger.LogError("Failed to upload company {CompanyId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", companyId, response.StatusCode, response.ReasonPhrase);
            return false;
        }

    }

    public async ValueTask<bool> ReportMatchResults(MatchResult result, UploadProgressUpdateDelegate? progressUpdate = null) {

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
            { "guid", result.LobbyId }
        };

        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";
        _logger.LogInformation("Sending POST request to {RequestUri}", requestUri);

        HttpRequestMessage request = await GetHttpRequestWithAuthHeaders(HttpMethod.Post, requestUri);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");

        // Serialize JSON to byte stream and create a ProgressStream to track upload progress
        using var memoryStream = new MemoryStream();
        await JsonSerializer.SerializeAsync(memoryStream, result, serializerOptions);

        memoryStream.Position = 0;
        request.Content = new StreamContent(new ProgressStream(memoryStream, memoryStream.Length, progressUpdate));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request.Content.Headers.ContentLength = memoryStream.Length;

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Match results for lobby {LobbyId} reported successfully.", result.LobbyId);
            return true;
        } else {
            _logger.LogError("Failed to report match results for lobby {LobbyId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", result.LobbyId, response.StatusCode, response.ReasonPhrase);
            return false;
        }

    }

    public async Task<bool> IsServerAvailableAsync() {

        string requestUri = $"{BaseUrl}/api/v1/up";
        _logger.LogInformation("Checking server availability at {RequestUri}", requestUri);

        HttpRequestMessage request = new(HttpMethod.Get, requestUri);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");
        return await _httpClient.SendRequestAsync(request)
            .ContinueWith(responseTask => {
                HttpResponseMessage response = responseTask.Result;
                if (response.IsSuccessStatusCode) {
                    _logger.LogInformation("Battlegrounds server is available.");
                    return true;
                } else {
                    _logger.LogError("Battlegrounds server is not available. Status code: {StatusCode}, Reason: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                    return false;
                }
            });

    }

    public sealed class LobbySummary {
        public required string Id { get; init; }
        public required string Name { get; init; }
        public required string Host { get; init; }
        public required List<Participant> Participants { get; init; }
        public required bool HasPassword { get; init; }
        public required List<Team> Teams { get; init; }
        public required string Game { get; init; } // Specifies if CoH3 or CoH2
        public required Dictionary<string, string> Settings { get; init; } // Contains map, game mode, etc.
        public BrowserLobby ToBrowserLobby() {
            int maxPlayers = Teams.Sum(t => t.Slots.Count(s => !s.Hidden && !s.Locked));
            string hostname = Participants.FirstOrDefault(p => p.ParticipantId == Host)?.ParticipantName ?? "Host name unavailable...";
            return new BrowserLobby {
                Id = Id,
                Name = Name,
                Host = hostname,
                CurrentPlayers = Participants.Count,
                MaxPlayers = maxPlayers,
                Map = Settings.TryGetValue("$map", out string? map) ? map : "Unknown Map",
                Settings = Settings.Where(x => x.Key != "$map").ToDictionary(x => x.Key, x => x.Value), // Exclude map from settings as it has its own property in BrowserLobby
                GameMode = Settings.TryGetValue(LobbySetting.SETTING_GAMEMODE, out string? gamemode) ? gamemode : "Unknown Game Mode",
                IsPasswordProtected = HasPassword,
                Game = Game,
                Team1Slots = Teams.ElementAtOrDefault(0)?.Slots
                    .Where(x => !x.Hidden)
                    .Select(s => new BrowserLobbySlot(s.Index, Participants.FirstOrDefault(p => p.ParticipantId == s.ParticipantId)?.ParticipantName ?? "Unknown", !string.IsNullOrEmpty(s.ParticipantId), s.Hidden, s.Locked, s.Faction, Game)).ToArray() ?? [],
                Team2Slots = Teams.ElementAtOrDefault(1)?.Slots
                    .Where(x => !x.Hidden)
                    .Select(s => new BrowserLobbySlot(s.Index, Participants.FirstOrDefault(p => p.ParticipantId == s.ParticipantId)?.ParticipantName ?? "Unknown", !string.IsNullOrEmpty(s.ParticipantId), s.Hidden, s.Locked, s.Faction, Game)).ToArray() ?? []
            };
        }
    }

    public async Task<IEnumerable<BrowserLobby>> GetLobbiesAsync() {

        var requestUri = $"{BaseUrl}{GetLobbiesEndpoint}";
        _logger.LogInformation("Sending GET request to {RequestUri} to retrieve lobbies", requestUri);

        HttpRequestMessage request = new(HttpMethod.Get, requestUri);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (!response.IsSuccessStatusCode) {
            _logger.LogError("Failed to retrieve lobbies from {RequestUri}. Status code: {StatusCode}, Reason: {ReasonPhrase}", requestUri, response.StatusCode, response.ReasonPhrase);
            return [];
        }

        _logger.LogInformation("Successfully retrieved lobbies from {RequestUri}", requestUri);

        var lobbySummaries = await response.Content.ReadFromJsonAsync<IEnumerable<LobbySummary>>();
        return lobbySummaries?.Select(x => x.ToBrowserLobby()) ?? [];

    }

    public async Task<bool> UploadGamemodeAsync(string lobbyId, string gamemodeLocation, UploadProgressUpdateDelegate? progressUpdate = null) {

        string endpoint = $"{BaseUrl}{UploadGamemodeEndpoint}";
        var parameters = new Dictionary<string, string> {
            { "guid", lobbyId }
        };

        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";
        using var gamemodeStream = File.OpenRead(gamemodeLocation);

        _logger.LogInformation("Sending POST request to {RequestUri}", requestUri);
        HttpRequestMessage request = await GetHttpRequestWithAuthHeaders(HttpMethod.Post, requestUri);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");
        request.Content = new StreamContent(new ProgressStream(gamemodeStream, gamemodeStream.Length, progressUpdate));
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        request.Content.Headers.ContentLength = gamemodeStream.Length; // Set content length for the stream

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Gamemode for lobby {LobbyId} uploaded successfully.", lobbyId);
            return true;
        } else {
            _logger.LogError("Failed to upload gamemode for lobby {LobbyId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", lobbyId, response.StatusCode, response.ReasonPhrase);
            return false;
        }

    }

    public async Task<bool> DownloadGamemodeAsync(string lobbyId, string destinationPath, DownloadProgressUpdateDelegate? progressUpdate = null) {
        string endpoint = $"{BaseUrl}{DownloadGamemodeEndpoint}";
        var parameters = new Dictionary<string, string> {
            { "guid", lobbyId }
        };
        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";
        _logger.LogInformation("Sending GET request to {RequestUri} to download gamemode for lobby {LobbyId}", requestUri, lobbyId);

        HttpRequestMessage request = new(HttpMethod.Get, requestUri);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Gamemode for lobby {LobbyId} retrieved successfully. Saving to {DestinationPath}", lobbyId, destinationPath);

            long? totalBytes = response.Content.Headers.ContentLength;
            long bytesDownloaded = 0;

            try {

                // The destination directory only exists on a machine that has built the archive itself; a joining
                // player has never had it created for them.
                if (Path.GetDirectoryName(destinationPath) is string destinationDirectory && !string.IsNullOrEmpty(destinationDirectory)) {
                    Directory.CreateDirectory(destinationDirectory);
                }

                using Stream contentStream = await response.Content.ReadAsStreamAsync();
                using FileStream fileStream = new(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

                byte[] buffer = new byte[8192]; // 8KB chunks
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0) {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                    bytesDownloaded += bytesRead;
                    progressUpdate?.Invoke(bytesDownloaded, totalBytes);
                }

            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to write gamemode for lobby {LobbyId} to {DestinationPath} after {BytesDownloaded} bytes.", lobbyId, destinationPath, bytesDownloaded);
                return false;
            }

            _logger.LogInformation("Gamemode for lobby {LobbyId} downloaded successfully. Total bytes: {BytesDownloaded}", lobbyId, bytesDownloaded);
            return true;
        } else {
            _logger.LogError("Failed to retrieve gamemode for lobby {LobbyId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", lobbyId, response.StatusCode, response.ReasonPhrase);
            return false;
        }
    }

    public async Task<MatchResult?> GetLatestMatchResult(string lobbyId) {

        string endpoint = $"{BaseUrl}{GetLobbyResultEndpoint}";
        var parameters = new Dictionary<string, string> {
            { "lobbyId", lobbyId }
        };
        string requestUri = $"{endpoint}?{ToUrlEncodedString(parameters)}";
        _logger.LogInformation("Sending GET request to {RequestUri} to get latest match result for lobby {LobbyId}", requestUri, lobbyId);

        HttpRequestMessage request = new(HttpMethod.Get, requestUri);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Latest match result for lobby {LobbyId} retrieved successfully.", lobbyId);
            Stream contentStream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<MatchResult?>(contentStream, serializerOptions);
        } else {
            _logger.LogError("Failed to retrieve latest match result for lobby {LobbyId}. Status code: {StatusCode}, Reason: {ReasonPhrase}", lobbyId, response.StatusCode, response.ReasonPhrase);
            return null;
        }

    }

    public async Task<LatestWinconditionDTO?> GetLatestWinconditionSourceMetadata() {

        string endpoint = $"{BaseUrl}{LatestWinconditionSrcEndpoint}";
        HttpRequestMessage request = new(HttpMethod.Get, endpoint);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Latest wincondition source metadata retrieved successfully.");
            Stream contentStream = await response.Content.ReadAsStreamAsync();
            if (await JsonSerializer.DeserializeAsync<LatestWinconditionDTO?>(contentStream, serializerOptions) is LatestWinconditionDTO dto) {
                return dto;
            } else {
                _logger.LogError("Failed to deserialize latest wincondition source metadata. Response content could not be parsed.");
                throw new InvalidDataException("Response content could not be parsed as LatestWinconditionDTO.");
            }
        } 

        _logger.LogError("Failed to retrieve latest wincondition source metadata. Status code: {StatusCode}, Reason: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
        return null;

    }

    public async Task<bool> DownloadLatestWinconditionSource(string tag, string outWinconditionPath) {

        string requestUri = $"{BaseUrl}{DownloadLatestWinconditionSrcEndpoint}/{tag}";
        _logger.LogInformation("Sending GET request to {RequestUri} to download latest wincondition source for tag {Tag}", requestUri, tag);

        HttpRequestMessage request = new(HttpMethod.Get, requestUri);
        request.Headers.Add("User-Agent", "BattlegroundsClient/1.0");

        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (response.IsSuccessStatusCode) {
            _logger.LogInformation("Latest wincondition source for tag {Tag} downloaded successfully.", tag);
            Stream contentStream = await response.Content.ReadAsStreamAsync();
            await using (FileStream fileStream = new(outWinconditionPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
                await contentStream.CopyToAsync(fileStream);
            }
            return true;
        } else {
            _logger.LogError("Failed to download latest wincondition source for tag {Tag}. Status code: {StatusCode}, Reason: {ReasonPhrase}", tag, response.StatusCode, response.ReasonPhrase);
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

}
