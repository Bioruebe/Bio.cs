using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BioLib;

namespace BioTests {
	[TestClass]
	public class BioTests {

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
	}
}
