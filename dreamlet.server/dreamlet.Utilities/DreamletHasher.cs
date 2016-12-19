using System;
using System.Security.Cryptography;

namespace dreamlet.Utilities
{
	class DreamletHasher
	{
		static readonly string SaltKey = "FB3D8588-CEDB-4400-9603-CE0EFB83C595";

		/// <summary>
		/// Creating salt based on some input value with predefined SaltKey 
		/// </summary>
		public static string GetSalt(string input)
		{
			Rfc2898DeriveBytes hasher = new Rfc2898DeriveBytes(input,
				System.Text.Encoding.UTF8.GetBytes(SaltKey), 10000);

			return Convert.ToBase64String(hasher.GetBytes(25));
		}

		/// <summary>
		/// Hash input string with provided Salt
		/// </summary>
		/// <param name="salt"></param>
		/// <param name="value">value to hash</param>
		public static string Hash(string salt, string value)
		{
			Rfc2898DeriveBytes Hasher = new Rfc2898DeriveBytes(value,
				System.Text.Encoding.UTF8.GetBytes(salt), 10000);

			return Convert.ToBase64String(Hasher.GetBytes(25));
		}

		/// <summary>
		/// hash password
		/// </summary>
		/// <param name="Username">Username is used to create Salt for pwd hashing </param>
		public static string GetHashedPassword(string username, string password)
		{
			Rfc2898DeriveBytes Hasher = new Rfc2898DeriveBytes(password,
				System.Text.Encoding.UTF8.GetBytes(GetSalt(username)), 10000);

			return Convert.ToBase64String(Hasher.GetBytes(25));
		}
	}
}
