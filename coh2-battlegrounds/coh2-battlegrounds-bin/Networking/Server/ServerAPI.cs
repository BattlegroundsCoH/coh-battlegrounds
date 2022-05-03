using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Networking.Communication;
using Battlegrounds.Networking.Communication.Golang;
using Battlegrounds.Util;
using Battlegrounds.Verification;

namespace Battlegrounds.Networking.Server;

/// <summary>
/// Delegate function for translating server string values.
/// </summary>
/// <param name="rawTextInput">The raw server string that was received.</param>
/// <param name="propertyName">The property name storing the raw string value.</param>
/// <returns>A translated string of the server-stored value.</returns>
public delegate string ServerAPIResponseStringTranslator(string rawTextInput, string propertyName);

/// <summary>
/// Delegate function for handling upload callback events.
/// </summary>
/// <param name="currentChunk">The currently uploaded chunk.</param>
/// <param name="chunkCount">The total amount of chunks.</param>
public delegate void ServerAPIUploadCallback(int currentChunk, int chunkCount);

/// <summary>
/// Delegate function for handling download callback events.
/// </summary>
/// <param name="result">The download result.</param>
/// <param name="downloadedData">The bytes that were downloaded.</param>
public delegate void ServerAPIDownloadCallback(DownloadResult result, byte[]? downloadedData = null);

/// <summary>
/// API instance for interacting with the server.
/// </summary>
public class ServerAPI {

    private struct CheckExists {

        [JsonIgnore]
        public bool Success => this.ReturnStr == "yes";

        public string ReturnStr { get; set; }

    }

    /// <summary>
    /// Represents a public message from developers regarding the project.
    /// </summary>
    public readonly struct PublicMessage {

        /// <summary>
        /// Get the set of messages tied to specific languages.
        /// </summary>
        /// <remarks>
        /// See <see cref="DefaultMessage"/> if local language version is not available.
        /// </remarks>
        public Dictionary<string, string> Message { get; }

        /// <summary>
        /// Get the default message (english)
        /// </summary>
        [JsonIgnore]
        public string DefaultMessage => this.Message["english"];

        /// <summary>
        /// Get the message type
        /// </summary>
        public string MessageType { get; }

        /// <summary>
        /// Get the message priority
        /// </summary>
        public int MessagePriority { get; }

        /// <summary>
        /// Get the start date tick
        /// </summary>
        public long StartTick { get; }

        /// <summary>
        /// Get the local time this message should start appearing from.
        /// </summary>
        [JsonIgnore]
        public DateTime Start => new DateTime(this.StartTick, DateTimeKind.Utc).ToLocalTime();

        /// <summary>
        /// Get the end date tick
        /// </summary>
        public long EndTick { get; }

        /// <summary>
        /// Get the local time this message should no longer be displayed from.
        /// </summary>
        [JsonIgnore]
        public DateTime End => new DateTime(this.EndTick, DateTimeKind.Utc).ToLocalTime();

        /// <summary>
        /// Initialise a new <see cref="PublicMessage"/> instance with a message and meta data.
        /// </summary>
        /// <param name="Message">The localised messages.</param>
        /// <param name="MessageType">The message type.</param>
        /// <param name="MessagePriority">The message priority.</param>
        /// <param name="StartTick">The start tick.</param>
        /// <param name="EndTick">The end tick.</param>
        [JsonConstructor]
        public PublicMessage(Dictionary<string, string> Message, string MessageType, int MessagePriority, long StartTick, long EndTick) { 
            this.Message = Message;
            this.MessageType = MessageType;
            this.MessagePriority = MessagePriority;
            this.StartTick = StartTick;
            this.EndTick = EndTick;
        }

    }

    /// <summary>
    /// Represents the latest up-to-date version data from the server.
    /// </summary>
    /// <remarks>
    /// Should be used to compare against own version.
    /// </remarks>
    public readonly struct LatestVersion {

        /// <summary>
        /// Get the name of the latest server branch.
        /// </summary>
        public string Branch { get; }

        /// <summary>
        /// Get the numeric version (as a string).
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Get the title given to the version. (May be empty).
        /// </summary>
        public string VersionTitle { get; }

        /// <summary>
        /// Get the checksum as string for the latest version.
        /// </summary>
        public string VersionChecksum { get; }

        /// <summary>
        /// Initialise a new <see cref="LatestVersion"/> instance with branch, version and title.
        /// </summary>
        /// <param name="Branch">The latest version branch.</param>
        /// <param name="Version">The numeric version representation.</param>
        /// <param name="VersionTitle">The title of the version.</param>
        /// <param name="VersionChecksum">The checksum value as string.</param>
        [JsonConstructor]
        public LatestVersion(string Branch, string Version, string VersionTitle, string VersionChecksum) { 
            this.Branch = Branch;
            this.Version = Version;
            this.VersionTitle = VersionTitle;
            this.VersionChecksum = VersionChecksum;
        }

    }

    /// <summary>
    /// Reoresents an error report that can be uploaded to the server
    /// </summary>
    private readonly struct ErrorReport {

        /// <summary>
        /// Get the additional information associated with this error report.
        /// </summary>
        public readonly string AdditionalInfo { get; }

        /// <summary>
        /// Get the actual error to report.
        /// </summary>
        public readonly string ErrorLog { get; }

        /// <summary>
        /// Get if this was an app error.
        /// </summary>
        public readonly bool IsAppLog { get; }

        /// <summary>
        /// Get if this was a scar error.
        /// </summary>
        public readonly bool IsScarLog { get; }

        /// <summary>
        /// Create a new <see cref="ErrorReport"/> instance for upload to the server.
        /// </summary>
        /// <param name="AdditionalInfo">The additional information associated with the error report.</param>
        /// <param name="ErrorLog">The log associated with the crash.</param>
        /// <param name="IsAppLog">Flag marking if app crash.</param>
        /// <param name="IsScarLog">Flag marking if scar log.</param>
        public ErrorReport(string AdditionalInfo, string ErrorLog, bool IsAppLog, bool IsScarLog) {
            this.AdditionalInfo = AdditionalInfo;
            this.ErrorLog = ErrorLog;
            this.IsAppLog = IsAppLog;
            this.IsScarLog = IsScarLog;
        }
    }

    private readonly HttpClient m_client;
    private readonly string m_address;
    private readonly int m_port;
    private ulong? m_lobbyGUID;

    /// <summary>
    /// Get the a lobby GUID has been given to the server API to simplify method calls.
    /// </summary>
    public bool HasGUID => this.m_lobbyGUID.HasValue;

    /// <summary>
    /// Get or set whether this <see cref="ServerAPI"/> instance should behave in an exception safe mode.
    /// </summary>
    public bool SafeMode { get; set; }

    /// <summary>
    /// Get the GUID associated with the <see cref="ServerAPI"/> instance.
    /// </summary>
    public string GUID => this.m_lobbyGUID.HasValue ? this.m_lobbyGUID.Value.ToString() : "0";

    /// <summary>
    /// Get the Lobby GUID associated with the <see cref="ServerAPI"/> instance.
    /// </summary>
    public ulong LobbyUID => this.m_lobbyGUID ?? throw new Exception("Value not defined!");

    /// <summary>
    /// Initialise a new server API client for targetted <paramref name="address"/>.
    /// </summary>
    /// <param name="address">The address of the API.</param>
    /// <param name="port">The port to use when invokg the server API</param>
    /// <param name="safeMode">Should API calls cause exceptions or return default values.</param>
    public ServerAPI(string address, int port, bool safeMode = true) {
        this.m_client = new HttpClient();
        this.m_address = address;
        this.m_port = port;
        this.m_lobbyGUID = null;
        this.SafeMode = safeMode;
    }

    /// <summary>
    /// Set the lobby guid to use when requesting lobby-specific information.
    /// </summary>
    /// <param name="guid">The GUID of the lobby to use.</param>
    public void SetLobbyGuid(ulong guid)
        => this.m_lobbyGUID = guid;

    /// <summary>
    /// Get all currently active lobbies on the server.
    /// </summary>
    /// <returns>A list of all active lobbies.</returns>
    /// <exception cref="APIException"/>
    /// <exception cref="APIConnectionException"/>
    /// <exception cref="InvalidProgramException"/>
    public List<ServerLobby> GetLobbies() {

        try {

            // Invoke API
            var apiResponse = this.m_client.GET(this.m_address, "lobbies/getlobbies", port: this.m_port);

            // Verify proper reply was given
            if (!apiResponse.Success) {
                if (this.SafeMode) {
                    return new();
                } else {
                    throw new APIException("lobbies/getlobbies", $"Failed to get 200 status code. (E = {apiResponse.StatusCode})");
                }
            }

            // Try get
            var results = JsonSerializer.Deserialize<List<ServerLobby>>(apiResponse.Value);
            if (results is null) {
                if (this.SafeMode) {
                    return new();
                } else {
                    throw new InvalidProgramException("Json serialiser failed to convert lobby results.");
                }
            }

            // Return found values.
            return results;

        }
        catch (JsonException jex) {

            // Log it
            Trace.WriteLine(jex, nameof(ServerAPI));

            if (this.SafeMode) {
                return new();
            } else {
                throw new APIConnectionException("lobbies/getlobbies", jex.Message, jex);
            }

        } catch (Exception ex) {

            if (this.SafeMode) {
                return new();
            } else {
                throw new APIConnectionException("lobbies/getlobbies", ex.Message, ex);
            }

        }

    }

    /// <summary>
    /// Get all public messages.
    /// </summary>
    /// <returns>A list of <see cref="PublicMessage"/> instances in unoredered form.</returns>
    /// <exception cref="APIException"/>
    /// <exception cref="APIConnectionException"/>
    /// <exception cref="InvalidProgramException"/>
    public List<PublicMessage> GetPublicMessages() {

        try {

            // Invoke API
            var apiResponse = this.m_client.GET(this.m_address, "client/public-info", port: this.m_port);

            // Verify proper reply was given
            if (!apiResponse.Success) {
                return this.SafeMode ? new() : throw new APIException("client/public-info", $"Failed to get 200 status code. (E = {apiResponse.StatusCode})");
            }

            // Return json deserialized
            return JsonSerializer.Deserialize<List<PublicMessage>>(apiResponse.Value) ?? (this.SafeMode ? new() : throw new InvalidProgramException("Jsonserialsier failed."));

        } catch (Exception ex) {

            if (this.SafeMode) {
                return new();
            } else {
                throw new APIConnectionException("client/public-info", ex.Message, ex);
            }

        }

    }

    /// <summary>
    /// Get information object of latest version.
    /// </summary>
    /// <returns>A <see cref="LatestVersion"/> instance.</returns>
    /// <exception cref="APIException"/>
    /// <exception cref="APIConnectionException"></exception>
    public LatestVersion GetLatestVersionInfo() {

        try {

            // Invoke API
            var apiResponse = this.m_client.GET(this.m_address, "client/latest", port: this.m_port);

            // Verify proper reply was given
            if (!apiResponse.Success) {
                return this.SafeMode ? new() : throw new APIException("client/latest", $"Failed to get 200 status code. (E = {apiResponse.StatusCode})");
            }

            // Return json deserialized
            return JsonSerializer.Deserialize<LatestVersion>(apiResponse.Value);

        } catch (Exception ex) {

            if (this.SafeMode) {
                return new();
            } else {
                throw new APIConnectionException("client/latest", ex.Message, ex);
            }

        }

    }

    /// <summary>
    /// Upload results to the server.
    /// </summary>
    /// <param name="matchResults">Server match data to upload as results.</param>
    public bool UploadResults(ServerMatchResults matchResults) {

        // Convert to json
        var jsonContent = JsonSerializer.Serialize(matchResults);

        // Invoke API
        var apiResponse = this.m_client.POST(this.m_address, $"lobbies/results?id={this.m_lobbyGUID}", new StringContent(jsonContent), port: this.m_port);

        // Return result
        return apiResponse.Success;

    }

    /// <summary>
    /// Get a specific lobby instance from the server.
    /// </summary>
    /// <param name="lobbyGuid">The GUID of the lobby.</param>
    /// <returns>If lobby was returned, a <see cref="ServerLobby"/> API representation; Otherwise <see langword="null"/>.</returns>
    public ServerLobby? GetLobby(string lobbyGuid) {

        // Invoke API
        var apiResponse = this.m_client.GET(this.m_address, $"lobbies/getlobby?id={lobbyGuid}", port: this.m_port);

        // Verify proper reply was given
        if (!apiResponse.Success) {
            return null;
        }

        // Return json deserialized
        return JsonSerializer.Deserialize<ServerLobby>(apiResponse.Value);

    }

    /// <summary>
    /// Upload company json data to specified lobby.
    /// </summary>
    /// <param name="lobbyGuid">The GUID of the lobby.</param>
    /// <param name="playerID">The player who owns the company.</param>
    /// <param name="jsonContent">The json content of the company.</param>
    /// <param name="callback">The callback function to update on upload progress. (Optional)</param>
    /// <returns>A <see cref="UploadResult"/> value describing the result of the upload operation.</returns>
    public UploadResult UploadCompany(string lobbyGuid, ulong playerID, string jsonContent, ServerAPIUploadCallback? callback = null) {

        // Check guid
        if (string.IsNullOrEmpty(lobbyGuid))
            return UploadResult.UPLOAD_INVALIDGUID;

        // Check playerid
        if (playerID is 0)
            return UploadResult.UPLOAD_INVALIDPLAYER;

        // Upload
        return this.UploadFile($"lobbies/company?id={lobbyGuid}&player={playerID}", Encoding.UTF8.GetBytes(jsonContent), callback);

    }

    /// <summary>
    /// Upload company json data to specified lobby.
    /// </summary>
    /// <param name="playerID">The player who owns the company.</param>
    /// <param name="jsonContent">The json content of the company.</param>
    /// <param name="callback">The callback function to update on upload progress. (Optional)</param>
    /// <returns>A <see cref="UploadResult"/> value describing the result of the upload operation.</returns>
    public UploadResult UploadCompany(ulong playerID, string jsonContent, ServerAPIUploadCallback? callback = null)
        => this.UploadCompany(this.GUID, playerID, jsonContent, callback);

    /// <summary>
    /// Download company file from lobby.
    /// </summary>
    /// <param name="lobbyGuid">The GUID of the lobby.</param>
    /// <param name="playerID">The ID of the player who owns the desired company.</param>
    /// <param name="callback">The callback function to handle on successful download.</param>
    /// <returns>A <see cref="DownloadResult"/> value indicating the result of the download API call.</returns>
    public DownloadResult DownloadCompany(string lobbyGuid, ulong playerID, ServerAPIDownloadCallback callback) {

        // Check guid
        if (string.IsNullOrEmpty(lobbyGuid))
            return DownloadResult.DOWNLOAD_INVALIDGUID;

        // Check playerid
        if (playerID is 0)
            return DownloadResult.DOWNLOAD_INVALIDPLAYER;

        // Invoke API
        var response = this.m_client.GET(this.m_address, $"lobbies/company?id={lobbyGuid}&player={playerID}", port: this.m_port);

        // Verify proper reply was given
        if (!response.Success) {
            return DownloadResult.DOWNLOAD_ERROR_UNDEFINED;
        }

        // Make sure there's content to read
        if (response.Content.Length <= 0) {
            return DownloadResult.DOWNLOAD_ERROR_UNDEFINED;
        }

        // Invoke callback
        callback(DownloadResult.DOWNLOAD_SUCCESS, response.Content.ToArray());

        // Return success value
        return DownloadResult.DOWNLOAD_SUCCESS;

    }

    /// <summary>
    /// Download company file from lobby.
    /// </summary>
    /// <param name="playerID">The ID of the player who owns the desired company.</param>
    /// <param name="callback">The callback function to handle on successful download.</param>
    /// <returns>A <see cref="DownloadResult"/> value indicating the result of the download API call.</returns>
    public DownloadResult DownloadCompany(ulong playerID, ServerAPIDownloadCallback callback)
        => this.DownloadCompany(this.GUID, playerID, callback);

    /// <summary>
    /// Get if <paramref name="playerID"/> has a company file registered.
    /// </summary>
    /// <param name="lobbyGuid">The GUID of the lobby.</param>
    /// <param name="playerID">The ID of the player who owns the desired company.</param>
    /// <returns>If company exists true; Otherwise false.</returns>
    public bool PlayerHasCompany(string lobbyGuid, ulong playerID) {

        // Invoke API
        var response = this.m_client.GET(this.m_address, $"lobbies/company?id={lobbyGuid}&player={playerID}&exists=yes", port: this.m_port);

        // Parse response
        if (response.Success) {

            // Parse json
            var exists = GoMarshal.JsonUnmarshal<CheckExists>(response.Content.ToArray());

            // Return val
            return exists.Success;

        }

        // Check if succes
        return false;

    }

    /// <summary>
    /// Get if <paramref name="playerID"/> has a company file registered.
    /// </summary>
    /// <param name="playerID">The ID of the player who owns the desired company.</param>
    /// <returns>If company exists true; Otherwise false.</returns>
    public bool PlayerHasCompany(ulong playerID)
        => this.PlayerHasCompany(this.GUID, playerID);

    /// <summary>
    /// Delete the server company file.
    /// </summary>
    /// <param name="lobbyGuid">The GUID of the lobby.</param>
    /// <param name="playerID">The ID of the player who owns the desired company.</param>
    /// <returns>If company was deleted, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool DeleteCompany(string lobbyGuid, ulong playerID)
        => this.m_client.DELETE(this.m_address, $"lobbies/company?id={lobbyGuid}&player={playerID}", this.m_port);

    /// <summary>
    /// Delete the server company file.
    /// </summary>
    /// <param name="playerID">The ID of the player who owns the desired company.</param>
    /// <returns>If company was deleted, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool DeleteCompany(ulong playerID)
        => this.DeleteCompany(this.GUID, playerID);

    /// <summary>
    /// Upload gamemode .sga file to lobby.
    /// </summary>
    /// <param name="lobbyGuid">The GUID of the lobby.</param>
    /// <param name="binary">The binary .sga contents</param>
    /// <param name="callback">The callback function to update on upload progress. (Optional)</param>
    /// <returns>A <see cref="UploadResult"/> value describing the result of the upload operation.</returns>
    public UploadResult UploadGamemode(string lobbyGuid, byte[] binary, ServerAPIUploadCallback? callback = null) {

        // Check guid
        if (string.IsNullOrEmpty(lobbyGuid))
            return UploadResult.UPLOAD_INVALIDGUID;

        // Upload
        return this.UploadFile($"lobbies/gamemode?id={lobbyGuid}&player={NetworkInterface.SelfIdentifier}", binary, callback);

    }

    /// <summary>
    /// Upload gamemode .sga file to lobby.
    /// </summary>
    /// <param name="binary">The binary .sga contents</param>
    /// <param name="callback">The callback function to update on upload progress. (Optional)</param>
    /// <returns>A <see cref="UploadResult"/> value describing the result of the upload operation.</returns>
    public UploadResult UploadGamemode(byte[] binary, ServerAPIUploadCallback? callback = null)
        => this.UploadGamemode(this.GUID, binary, callback);

    /// <summary>
    /// Download the uploaded gamemode file for the lobby.
    /// </summary>
    /// <param name="lobbyGuid">The GUID of the lobby.</param>
    /// <param name="callback">The callback function to handle on successful download.</param>
    /// <returns>A <see cref="DownloadResult"/> value indicating the result of the download API call.</returns>
    public DownloadResult DownloadGamemode(string lobbyGuid, ServerAPIDownloadCallback? callback) {

        // Check guid
        if (string.IsNullOrEmpty(lobbyGuid))
            return DownloadResult.DOWNLOAD_INVALIDGUID;

        // Invoke API
        var response = this.m_client.GET(this.m_address, $"lobbies/gamemode?id={lobbyGuid}", port: this.m_port);
        if (response.Success) {

            // Invoke callback
            callback?.Invoke(DownloadResult.DOWNLOAD_SUCCESS, response.Content.ToArray());

            // Return OK
            return DownloadResult.DOWNLOAD_SUCCESS;

        }

        // Return error code
        return DownloadResult.DOWNLOAD_ERROR_UNDEFINED;

    }

    /// <summary>
    /// Download the uploaded gamemode file for the lobby.
    /// </summary>
    /// <param name="callback">The callback function to handle on successful download.</param>
    /// <returns>A <see cref="DownloadResult"/> value indicating the result of the download API call.</returns>
    public DownloadResult DownloadGamemode(ServerAPIDownloadCallback? callback)
        => this.DownloadGamemode(this.GUID, callback);

    /// <summary>
    /// Get if specified lobby has gamemode file.
    /// </summary>
    /// <param name="lobbyGuid">The lobby GUID to check.</param>
    /// <returns>If server reports gamemode file exists, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool HasGamemode(string lobbyGuid) {

        // Invoke API
        var response = this.m_client.GET(this.m_address, $"lobbies/gamemode?id={lobbyGuid}&exists=yes", port: this.m_port);

        // Parse response
        if (response.Success) {

            // Parse json
            var exists = GoMarshal.JsonUnmarshal<CheckExists>(response.Content.ToArray());

            // Return val
            return exists.Success;

        }

        // Check if succes
        return false;

    }

    /// <summary>
    /// Get if connected lobby has gamemode file.
    /// </summary>
    /// <returns>If server reports gamemode file exists, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool HasGamemode() => this.HasGamemode(this.GUID);

    private UploadResult UploadFile(string apirequest, byte[] file, ServerAPIUploadCallback? uploadCallback = null) {

        // Calculate how many chunks to send
        int chunks = (int)Math.Ceiling(file.Length / 512.0);

        // Set p,q
        int p = 0;
        int q = Math.Min(file.Length, 512);

        // Send chunks
        for (int i = 0; i < chunks; i++) {

            // Grab chunk
            var chunk = file.Slice(p, q);

            // Send
            try {

                // Get flags
                bool init = i == 0;
                bool done = i == chunks - 1;

                // Get API
                string api = apirequest + $"&fresh={(init ? "yes" : "no")}&last={(done ? "yes" : "no")}";

                // Do upload
                var response = this.m_client.POST(this.m_address, api, new ByteArrayContent(chunk), port: this.m_port);
                if (!response.Success) { // TODO: Eval return code
                    uploadCallback?.Invoke(-1, -1);
                    return UploadResult.UPLOAD_ERROR_UNDEFINED;
                }

                // Do callback
                uploadCallback?.Invoke(i + 1, chunks);

            } catch (Exception e) {
                Trace.WriteLine(e, nameof(ServerAPI));
                uploadCallback?.Invoke(-1, -1);
                return UploadResult.UPLOAD_ERROR_UNDEFINED;
            }

            // Update offsets
            p += chunk.Length;
            q = Math.Min(file.Length, p + 512);

        }

        // No errors
        return UploadResult.UPLOAD_SUCCESS;

    }


    public UploadResult UploadFile(byte filetype, ulong uploadIdentity, ulong uploadTarget, byte[] filecontent, ServerAPIUploadCallback? uploadCallback = null) {

        // Calculate how many chunks to send
        int chunks = (int)Math.Ceiling(filecontent.Length / 1024.0);

        // Set p,q
        int p = 0;
        int q = Math.Min(filecontent.Length, 1024);

        try {

            // Prepare intro
            var intro = new IntroMessage() {
                LobbyType = filetype,
                PlayerUID = uploadIdentity,
                LobbyUID = uploadTarget,
                Type = 5
            };

            // Do socket connection
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IPEndPoint.Parse(NetworkInterface.Endpoint.TcpStr));

            // Send
            socket.Send(GoMarshal.JsonMarshal(intro));

            // Send chunks
            for (int i = 0; i < chunks; i++) {

                // Grab chunk
                var chunk = filecontent.Slice(p, q);
                var checksum = Redundancy.CRC(chunk);

                // Get flags
                bool done = i == chunks - 1;

                // Create header
                var header = BitConverter.GetBytes((ushort)chunk.Length).Concat(BitConverter.GetBytes(checksum)).Append((byte)(done ? 1 : 0));
                var packet = header.Concat(chunk);

                // Send
                socket.Send(packet);

                // Get reply
                byte[] response = new byte[2];
                socket.Receive(response);

                // aok
                if (response[0] != 6) {
                    Trace.WriteLine($"Error on server API file upload: {response[1]}", nameof(ServerAPI)); // not ok
                    uploadCallback?.Invoke(-1, -1);
                    return UploadResult.UPLOAD_ERROR_UNDEFINED;
                }

                // Do callback
                uploadCallback?.Invoke(i + 1, chunks);

                // Update offsets
                p += chunk.Length;
                q = Math.Min(filecontent.Length, p + 1024);

            }

        } catch (Exception e) {

            // Log exception
            Trace.WriteLine(e, nameof(ServerAPI));
            
            // Do callback
            uploadCallback?.Invoke(-1, -1);

            // Mark undefined
            return UploadResult.UPLOAD_ERROR_UNDEFINED;

        }

        // No errors
        return UploadResult.UPLOAD_SUCCESS;

    }

    /// <summary>
    /// Uploads a crash report to the server.
    /// </summary>
    /// <param name="additionalInfo">Additional information associated with the error.</param>
    /// <param name="logFilePath">The path to the log file.</param>
    public void UploadAppCrashReport(string additionalInfo, string logFilePath, bool isScar) {

        // Read file contents
        var file = File.ReadAllText(logFilePath);

        // Create report 
        var report = new ErrorReport(additionalInfo, file, !isScar, isScar);

        // Serialise
        var reportJson = GoMarshal.JsonMarshal(report);

        // Upload
        this.UploadFile($"report/error-fatal?uid={new Guid().ToString().Replace("-", "")}", reportJson);

    }

}
