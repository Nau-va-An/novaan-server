﻿namespace MongoConnector.Models
{
    public class MongoCollections
	{
		public static string Accounts { get; } = "accounts";
		public static string Recipes { get; } = "recipes";
		public static string CulinaryTips { get; } = "culinaryTips";
        public static string RefreshTokens { get; } = "refreshTokens";
		public static string Diets { get; } = "diet";
		public static string MealTypes { get; } = "mealType";
		public static string Cuisines { get; } = "cuisine";
    }
}

