using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BioLib;

namespace BioTests {
	[TestClass]
	public class BioTests {
		private const int ARRAY_LENGTH = 32;
		private readonly byte[] nullBytes = new byte[ARRAY_LENGTH];
		private readonly byte[] randomBytes = Bio.RandomByteArray(ARRAY_LENGTH);

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void HexToBytes_Null() {
			Bio.HexToBytes(null);
		}

		[TestMethod]
		public void HexToBytes_EmptyString() {
			var result = Bio.HexToBytes("");
			Assert.AreEqual(0, result.Length);
			Assert.IsTrue(new byte[0].SequenceEqual(result));
		}

		[TestMethod]
		public void HexToBytes() {
			var hexString = "010203FF1A";
			var expected = new byte[] { 0x01, 0x02, 0x03, 0xFF, 0x1A };
			var result = Bio.HexToBytes(hexString);
			Assert.AreEqual(expected.Length, result.Length);
			Assert.IsTrue(expected.SequenceEqual(result));
		}

		[TestMethod]
		public void HexToBytes_ShouldIgnoreSpaces() {
			var hexString = "01 02 03 FF 1A";
			var expected = new byte[] { 0x01, 0x02, 0x03, 0xFF, 0x1A };
			var result = Bio.HexToBytes(hexString);
			Assert.AreEqual(expected.Length, result.Length);
			Assert.IsTrue(expected.SequenceEqual(result));
		}

		[TestMethod]
		public void Xor_NullBytes_ShouldEqualInput() {
			var result = Bio.Xor(nullBytes, randomBytes);
			CollectionAssert.AreEqual(randomBytes, result);
		}

		[TestMethod]
		public void Xor_Identity_ShouldEqualZero() {
			var result = Bio.Xor(randomBytes, randomBytes);
			CollectionAssert.AreEqual(nullBytes, result);
		}

		[TestMethod]
		public void Xor_Twice_ShouldEqualIdentity() {
			var key = Bio.RandomByteArray(ARRAY_LENGTH);
			var once = Bio.Xor(randomBytes, key);
			var twice = Bio.Xor(once, key);
			CollectionAssert.AreEqual(randomBytes, twice);
		}

		[TestMethod]
		public void Repeat() {
			var i = 0;
			Bio.Repeat(() => i++, 5);
			Assert.AreEqual(i, 5);
		}

		[TestMethod]
		public void Repeat_ZeroTimes() {
			var i = 0;
			Bio.Repeat(() => i++, 0);
			Assert.AreEqual(i, 0);
		}
	}
}
