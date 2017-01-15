using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace dreamlet.Utilities
{
	public static class DreamletCrypto
	{
		private static readonly string _hash = _hash	?? ConfigurationManager.AppSettings["encryption.hash"];
		private static readonly string _salt = _salt	?? ConfigurationManager.AppSettings["encryption.salt"];
		private static readonly string _viKey = _viKey	?? ConfigurationManager.AppSettings["encryption.viKey"];

		public static string Encrypt(string plainText, string hash = null)
		{
			if (String.IsNullOrWhiteSpace(plainText))
				return String.Empty;

			byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

			byte[] keyBytes = new Rfc2898DeriveBytes(hash ?? _hash, Encoding.ASCII.GetBytes(_salt)).GetBytes(256 / 8);
			var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
			var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(_viKey));

			byte[] cipherTextBytes;

			using (var memoryStream = new MemoryStream())
			{
				using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
				{
					cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
					cryptoStream.FlushFinalBlock();
					cipherTextBytes = memoryStream.ToArray();
					cryptoStream.Close();
				}
				memoryStream.Close();
			}

			return Convert.ToBase64String(cipherTextBytes).Replace('+', '-').Replace("/", "_").TrimEnd('=');
		}

		public static string Encrypt(int integer, string hash = null) => Encrypt(integer.ToString(), hash);

		public static string DecryptToString(string encryptedText, string hash = null)
		{
			try
			{
				if (encryptedText == null || String.IsNullOrEmpty(encryptedText.Trim()))
					return null;

				encryptedText = encryptedText.Replace('-', '+').Replace("_", "/");

				switch (encryptedText.Length % 4)
				{
					case 2: encryptedText += "=="; break;
					case 3: encryptedText += "="; break;
				}

				byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
				byte[] keyBytes = new Rfc2898DeriveBytes(_hash, Encoding.ASCII.GetBytes(_salt)).GetBytes(256 / 8);
				var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

				var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(_viKey));
				var memoryStream = new MemoryStream(cipherTextBytes);
				var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
				byte[] plainTextBytes = new byte[cipherTextBytes.Length];

				int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

				memoryStream.Close();
				cryptoStream.Close();

				return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
			}
			catch (Exception ex)
			{
				// TODO: log
				return null;
			}
		}

		public static int DecryptToInt(string encryptedText, string hash = null) => Convert.ToInt32(DecryptToString(encryptedText, hash));
	}
}
