using ComethSDK.Scripts.Types;
using UnityEngine;

namespace ComethSDK.Scripts.Services
{
	public static class TokenService
	{
		private static string DecodeToken(string token)
		{
			return JWT.JsonWebToken.Decode(token,"",false);
		}
		
		public static string DecodeTokenAndGetUserID(string token)
		{
			var decodedToken = DecodeToken(token);
			var payload = JsonUtility.FromJson<JwtTokenPayload>(decodedToken);
			return payload.sub;
		}
		
		
	}
}