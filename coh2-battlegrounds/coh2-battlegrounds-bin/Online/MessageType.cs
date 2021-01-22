namespace Battlegrounds.Online {

    public enum MessageType : byte {

        ERROR_MESSAGE, // Error message

        INFORMATION_MESSAGE, // Information message

        CONFIRMATION_MESSAGE, // Confirms

        ORDINARY_RESPONSE, // Ordinary response to a simple query (Identifier must exist for response message to work)

        GET_LOBBIES, // Get the list of lobbies

        GET_LOBBIES_RETURN, // This is a returned lobby

        LOBBY_CREATE, // Create lobby

        LOBBY_REMOVE, // Remove lobby

        LOBBY_JOIN, // Join lobby

        LOBBY_LEAVE, // Leave lobby

        LOBBY_KICK, // User was kicked (To players in lobby: *player* was kicked)

        LOBBY_KICKED, // The user was kicked (Sent to the player who was kicked - to let them know they were kicked)

        LOBBY_AIJOIN, // Tell user an AI has joined

        LOBBY_AILEAVE, // Tell user an AI has left

        LOBBY_ADDAI, // Tell the lobby to introduce a new AI *player*

        LOBBY_REMOVEAI, // Tell the lobby to remove an AI *player*

        LOBBY_INFO, // Get lobby info

        LOBBY_UPDATE, // Update lobby info

        LOBBY_CHATMESSAGE, // Send chatmessage

        LOBBY_TEAMCHATMESSAGE, // Send message to team members only

        LOBBY_METAMESSAGE, // Send metamessage

        LOBBY_REQUEST_COMPANY, // Request company data from player(s)

        LOBBY_REQUEST_RESULTS, // Request match result data from all players

        LOBBY_STARTMATCH, // Tell clients to start match

        LOBBY_SYNCMATCH, // Tell clients to sync match

        LOBBY_STATE, // Set the state of the lobby

        LOBBY_GETSTATE, // Get the state of the lobby

        LOBBY_PLAYERNAMES, // Get the names of all lobby members

        LOBBY_PLAYERIDS, // Get Player IDs

        LOBBY_GETPLAYERID, // Get playerID of the first player with name (Avoid)

        LOBBY_SETHOST, // Message sent to client that they're now the host.

        LOBBY_UNSETHOST, // Message sent to client that they're no longer the host.

        LOBBY_STARTING, // Host has pressed the start button

        LOBBY_CANCEL, // Tell the lobby to cancel match start

        LOBBY_NOTIFY_GAMEMODE, // Notify lobby members the gamemode is available

        LOBBY_NOTIFY_MATCH, // Notify lobby members that match result data is available

        LOBBY_SETUSERDATA, // Set data for lobby member

        LOBBY_GETUSERDATA, // Get data from lobby member

        LOBBY_CHANGEDUSERDATA, // Server to client(s) telling them some user data has been changed

        LOBBY_TRYRECONNECT, // Try to reconnect to the lobby.

        LOBBY_GETHOSTID, // Get ID of host

        LOBBY_CLIENTERROR, // Inform of client error (fatal)

        LOBBY_CLIENTPROBLEM, // Inform of client problem (non-fatal, lacking information).

        LOBBY_DIRECTMESSAGE, // Send direct message to ID

        LOBBY_FWD2HOST, // Forward message to host (Must contain descriptor in first argument).

        USER_SETUSERDATA, // Set the user data

        USER_PING, // User ping-back (SERVER_PING -> reponse = USER_PING)

        SERVER_PING, // Ping from server (Server -> Client)

        SERVER_CLOSE, // Close the server

        SERVER_INTERNAL_PROCESS, // Call a process internally (or receive response)

        SERVER_APP_VCHECK, // Check for current app version

        SERVER_COH2_VCHECK, // Check version of CoH2 that's supported

        STAT_GETLOBBYCOUNT, // Get the current amount of lobbies

        STAT_GETUSERCOUNT, // Get the current amount of users

        STAT_GETPLAYING, // Get the current amount of active matches

        // Add more here

        FatalMessageError = 255

    }

}
