using System;
using System.Security.Cryptography;
using System.Text;

namespace Utils.Hash
{
	public class CustomHash
	{
		public static byte[] GetHash(string input)
		{
			using(HashAlgorithm algo = SHA256.Create())
			{
				return algo.ComputeHash(Encoding.UTF8.GetBytes(input));
			}
		}

		public static string GetHashString(string input)
		{
			StringBuilder sb = new StringBuilder();
			foreach(byte b in GetHash(input))
			{
				sb.Append(b.ToString("X2"));
			}
			return sb.ToString();
		}
	}
}

