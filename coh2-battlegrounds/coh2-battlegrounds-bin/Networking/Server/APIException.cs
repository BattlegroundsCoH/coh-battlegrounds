using System;

namespace Battlegrounds.Networking.Server;

/// <summary>
/// Represents errors casued when invoking the server API.
/// </summary>
public class APIException : Exception {
    
    /// <summary>
    /// The API call that caused this exception.
    /// </summary>
    public string API { get; }
    
    /// <summary>
    /// Initialise a new instance of the <see cref="APIException"/> class with the causing API call and error message.
    /// </summary>
    /// <param name="apicall">API call that caused the exception.</param>
    /// <param name="message">The error message associated with the exception.</param>
    public APIException(string apicall, string message) : base($"API call '{apicall}' yielded fatal result: {message}") { 
        this.API = apicall;
    }

    /// <summary>
    /// Initialise a new instance of the <see cref="APIException"/> class with the causing API call and error message. Including the inner exception that triggered this exception.
    /// </summary>
    /// <param name="apicall">API call that caused the exception.</param>
    /// <param name="message">The error message associated with the exception.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public APIException(string apicall, string message, Exception innerException) : base($"API call '{apicall}' yielded fatal result: {message}", innerException) {
        this.API = apicall;
    }

}

/// <summary>
/// Represents API connection errors.
/// </summary>
public class APIConnectionException : APIException {

    /// <summary>
    /// Initialise a new <see cref="APIConnectionException"/> instance with a reference to the called API, a custom message and an inner <see cref="Exception"/>.
    /// </summary>
    /// <param name="apicall">API call that caused the exception.</param>
    /// <param name="message">The error message associated with the exception.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public APIConnectionException(string apicall, string message, Exception innerException) : base(apicall, message, innerException) {
    }

}
