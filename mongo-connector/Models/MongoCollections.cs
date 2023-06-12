using System;
namespace MongoConnector.Models
{
	public class MongoCollections
	{
		public static string Accounts { get; } = "accounts";
		public static string CulinaryTips { get; } = "culinaryTips";
        public static string RefreshTokens { get; } = "refreshTokens";
    }
}

