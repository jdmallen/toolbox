namespace JDMallen.Toolbox.AspNetCore.Constants
{
	public class JwtClaimTypes
	{
		/// <summary>
		/// Custom claim type to represent an AppUser's primary ID
		/// </summary>
		public const string UserId = "id";

		/// <summary>
		/// Custom claim type to represent an AppRole's primary ID.
		/// </summary>
		public const string UserRole = "role";

		/// <summary>
		/// RFC 7519 - 4.1.1.  "iss" (Issuer) Claim
		/// </summary>
		/// <remarks>
		/// The "iss" (issuer) claim identifies the principal that issued the 
		/// JWT.  The processing of this claim is generally application specific. 
		///	The "iss" value is a case-sensitive string containing a StringOrURI 
		/// value.  Use of this claim is OPTIONAL.
		/// </remarks>
		public const string Issuer = "iss";

		/// <summary>
		/// RFC 7519 - 4.1.2.  "sub" (Subject) Claim
		/// </summary>
		/// <remarks>
		/// The "sub" (subject) claim identifies the principal that is the 
		/// subject of the JWT.  The claims in a JWT are normally statements 
		/// about the subject.  The subject value MUST either be scoped to be 
		/// locally unique in the context of the issuer or be globally unique. 
		/// The processing of this claim is generally application specific.  The 
		/// "sub" value is a case-sensitive string containing a StringOrURI 
		/// value.  Use of this claim is OPTIONAL.
		/// </remarks>
		public const string Subject = "sub";

		/// <summary>
		/// RFC 7519 - 4.1.3.  "aud" (Audience) Claim
		/// </summary>
		/// <remarks>
		/// The "aud" (audience) claim identifies the recipients that the JWT is 
		/// intended for.  Each principal intended to process the JWT MUST 
		/// identify itself with a value in the audience claim.  If the principal 
		/// processing the claim does not identify itself with a value in the 
		/// "aud" claim when this claim is present, then the JWT MUST be 
		/// rejected.  In the general case, the "aud" value is an array of case-
		/// sensitive strings, each containing a StringOrURI value.  In the 
		/// special case when the JWT has one audience, the "aud" value MAY be a 
		/// single case-sensitive string containing a StringOrURI value.  The 
		/// interpretation of audience values is generally application specific. 
		/// Use of this claim is OPTIONAL.
		/// </remarks>
		public const string Audience = "aud";

		/// <summary>
		/// RFC 7519 - 4.1.4.  "exp" (Expiration Time) Claim
		/// </summary>
		/// <remarks>
		/// The "exp" (expiration time) claim identifies the expiration time on 
		/// or after which the JWT MUST NOT be accepted for processing.  The 
		/// processing of the "exp" claim requires that the current date/time 
		/// MUST be before the expiration date/time listed in the "exp" claim. 
		/// Implementers MAY provide for some small leeway, usually no more than 
		/// a few minutes, to account for clock skew.  Its value MUST be a number 
		/// containing a NumericDate value.  Use of this claim is OPTIONAL. 
		/// </remarks>
		public const string Expiration = "exp";

		/// <summary>
		/// RFC 7519 - 4.1.5.  "nbf" (Not Before) Claim
		/// </summary>
		/// <remarks>
		/// The "nbf" (not before) claim identifies the time before which the JWT 
		/// MUST NOT be accepted for processing.  The processing of the "nbf" 
		/// claim requires that the current date/time MUST be after or equal to 
		/// the not-before date/time listed in the "nbf" claim.  Implementers MAY 
		/// provide for some small leeway, usually no more than a few minutes, to 
		/// account for clock skew.  Its value MUST be a number containing a 
		/// NumericDate value.  Use of this claim is OPTIONAL.
		/// </remarks>
		public const string NotBefore = "nbf";

		/// <summary>
		/// RFC 7519 - 4.1.6.  "iat" (Issued At) Claim
		/// </summary>
		/// <remarks>
		/// The "iat" (issued at) claim identifies the time at which the JWT was 
		/// issued.  This claim can be used to determine the age of the JWT.  Its 
		/// value MUST be a number containing a NumericDate value.  Use of this 
		/// claim is OPTIONAL.
		/// </remarks>
		public const string IssuedAt = "iat";

		/// <summary>
		/// RFC 7519 - 4.1.7.  "jti" (JWT ID) Claim
		/// </summary>
		/// <remarks>
		/// The "jti" (JWT ID) claim provides a unique identifier for the JWT. 
		/// The identifier value MUST be assigned in a manner that ensures that 
		/// there is a negligible probability that the same value will be 
		/// accidentally assigned to a different data object; if the application 
		/// uses multiple issuers, collisions MUST be prevented among values 
		/// produced by different issuers as well.  The "jti" claim can be used 
		/// to prevent the JWT from being replayed.  The "jti" value is a case-
		/// sensitive string.  Use of this claim is OPTIONAL.
		/// </remarks>
		public const string JwtId = "jti";

		/// <summary>
		/// RFC 7519 - 5.1.  "typ" (Type) Header Parameter
		/// </summary>
		/// <remarks>
		/// The "typ" (type) Header Parameter defined by [JWS] and [JWE] is used 
		/// by JWT applications to declare the media type [IANA.MediaTypes] of 
		/// this complete JWT.  This is intended for use by the JWT application 
		/// when values that are not JWTs could also be present in an application 
		/// data structure that can contain a JWT object; the application can use 
		/// this value to disambiguate among the different kinds of objects that 
		/// might be present.  It will typically not be used by applications when 
		/// it is already known that the object is a JWT.  This parameter is 
		/// ignored by JWT implementations; any processing of this parameter is 
		/// performed by the JWT application.  If present, it is RECOMMENDED that 
		/// its value be "JWT" to indicate that this object is a JWT.  While 
		/// media type names are not case sensitive, it is RECOMMENDED that "JWT" 
		/// always be spelled using uppercase characters for compatibility with 
		/// legacy implementations.  Use of this Header Parameter is OPTIONAL.
		/// </remarks>
		public const string Type = "typ";

		/// <summary>
		/// RFC 7519 - 5.2.  "cty" (Content Type) Header Parameter
		/// </summary>
		/// <remarks>
		/// The "cty" (content type) Header Parameter defined by [JWS] and [JWE] 
		/// is used by this specification to convey structural information about 
		/// the JWT.
		/// 
		/// In the normal case in which nested signing or encryption operations 
		/// are not employed, the use of this Header Parameter is NOT 
		/// RECOMMENDED.  In the case that nested signing or encryption is 
		/// employed, this Header Parameter MUST be present; in this case, the 
		/// value MUST be "JWT", to indicate that a Nested JWT is carried in this 
		/// JWT.  While media type names are not case sensitive, it is 
		/// RECOMMENDED that "JWT" always be spelled using uppercase characters 
		/// for compatibility with legacy implementations.  See Appendix A.2 for 
		/// an example of a Nested JWT.
		/// </remarks>
		public const string ContentType = "cty";
	}
}
