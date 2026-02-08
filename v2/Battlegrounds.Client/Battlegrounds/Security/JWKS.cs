using System.Text.Json.Serialization;

namespace Battlegrounds.Security;

/// <summary>
/// Represents a collection of JSON Web Keys (JWKs) used for cryptographic operations in web security scenarios.
/// </summary>
/// <remarks>This record struct is commonly used in scenarios involving JSON Web Tokens (JWT) and OpenID Connect,
/// where public keys are exchanged in JWK format for validating signatures or encrypting data.</remarks>
/// <param name="Keys">An array of JWK objects that provide the necessary parameters for cryptographic operations such as signing or
/// encryption.</param>
public record struct JWKS([property:JsonPropertyName("keys")] JWKS.JWK[] Keys) {

    /// <summary>
    /// Represents a JSON Web Key (JWK) that provides the necessary parameters for cryptographic operations such as
    /// signing or encryption in web security scenarios.
    /// </summary>
    /// <remarks>This record struct is commonly used in scenarios involving JSON Web Tokens (JWT) and OpenID
    /// Connect, where public keys are exchanged in JWK format for validating signatures or encrypting data. The
    /// parameters correspond to standard JWK fields as defined by RFC 7517.</remarks>
    /// <param name="Kty">The key type, indicating the cryptographic algorithm family associated with the key (for example, 'RSA' or
    /// 'EC').</param>
    /// <param name="Use">The intended use of the key, such as <c>sig</c> for signature or <c>enc</c> for encryption, specifying how the
    /// key should be applied.</param>
    /// <param name="Kid">The unique identifier for the key, used to distinguish this key from others in a set.</param>
    /// <param name="Alg">The algorithm intended for use with the key, specifying the cryptographic algorithm to be employed (for example,
    /// 'RS256').</param>
    /// <param name="N">The modulus value for the key, used in asymmetric cryptographic algorithms such as RSA.</param>
    /// <param name="E">The exponent value for the key, used in asymmetric cryptographic algorithms such as RSA.</param>
    public record struct JWK(
        [property:JsonPropertyName("kty")] string Kty,
        [property:JsonPropertyName("use")] string Use,
        [property:JsonPropertyName("kid")] string Kid,
        [property:JsonPropertyName("alg")] string Alg,
        [property:JsonPropertyName("n")] string N,
        [property:JsonPropertyName("e")] string E
    );

    /// <summary>
    /// Retrieves a JSON Web Key (JWK) from the collection that matches the specified key identifier.
    /// </summary>
    /// <remarks>This method searches the collection of keys and returns the first key with an identifier
    /// equal to the specified value. If no matching key is found, the method returns null.</remarks>
    /// <param name="keyId">The unique identifier of the key to retrieve. This value must not be null or empty.</param>
    /// <returns>A JWK object that matches the specified key identifier, or null if no key is found.</returns>
    public readonly JWK GetKeyById(string keyId) =>
        Array.Find(this.Keys, k => k.Kid == keyId);

}
