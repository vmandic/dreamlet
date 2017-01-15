using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using dreamlet.Utilities;

namespace dreamlet.Generic.Tests.Utilities
{
	[TestClass]
	public class DreamletCryptoTests
	{
		static Random rnd = new Random();

		[TestMethod]
		public void Should_encrypt_and_decrypt_100_random_strings()
		{
			// ARRANGE & ACT
			var pairs = Enumerable.Range(1, 100).Select(x => Tuple.Create(x, DreamletCrypto.Encrypt(x))).ToList();

			// ASSERT
			Assert.IsTrue(pairs.TrueForAll(x => x.Item1 == DreamletCrypto.DecryptToInt(x.Item2)));
		}
	}
}
