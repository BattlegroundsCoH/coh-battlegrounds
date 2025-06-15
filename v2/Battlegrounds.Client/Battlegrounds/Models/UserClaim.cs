using System.Text.Json.Serialization;

namespace Battlegrounds.Models;

/*
    Go Definition:
	type UserClaim struct {
		UserName  string `json:"user_name"`
		UserRole  string `json:"user_role"` // e.g., "admin", "user", etc. (Maybe unused for now)
		Issuer    string `json:"iss"`       // Issuer of the token, e.g., "cohbattlegrounds.com"
		Subject   string `json:"sub"`
		ExpiresAt int64  `json:"exp"`
		IssuedAt  int64  `json:"iat"`
	}
 */

public sealed record UserClaim(
	[property: JsonPropertyName("user_name")] string UserName,
	[property: JsonPropertyName("user_role")] string UserRole, // e.g., "admin", "user", etc. (Maybe unused for now)
	[property: JsonPropertyName("iss")] string Issuer, // Issuer of the token, e.g., "cohbattlegrounds.com"
	[property: JsonPropertyName("sub")] string Subject,
	[property: JsonPropertyName("exp")] long ExpiresAt, // Expiration time in Unix timestamp format
	[property: JsonPropertyName("iat")] long IssuedAt // Issued at time in Unix timestamp format
    );
