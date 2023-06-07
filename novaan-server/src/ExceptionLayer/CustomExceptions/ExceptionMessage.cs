using System;
namespace NovaanServer.src.ExceptionLayer.CustomExceptions
{
	public static class ExceptionMessage
	{
		public const string EMAIL_TAKEN = "This email had been associated with another account.";

		public const string USERNAME_TAKEN = "This username had been associated with another account.";

		public const string SERVER_UNAVAILABLE = "Server is currently unavailable. Please try again later.";

        public const string EMAIL_OR_PASSWORD_NOTFOUND= "Wrong email and password combination. Please try again.";
    }
}

