using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BioLib;
using BioLib.Streams;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BioTests {
	[TestClass]
	public class OffsetStreamTests {
		private Stream baseStream;
		private OffsetStream offsetStream;
		private const int BASE_LENGTH = 32;
		private const int OFFSET = 8;

		[TestInitialize]
		public void InitializeTests() {
			baseStream = Bio.RandomStream(BASE_LENGTH);
			offsetStream = new OffsetStream(baseStream, OFFSET);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Create_Null_ShouldThrowException() {
			new OffsetStream(null, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Create_NegativeOffset_ShouldThrowException() {
			new OffsetStream(baseStream, -4);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Create_OffsetGreaterThanStreamLength_ShouldThrowException() {
			new OffsetStream(baseStream, BASE_LENGTH + 1);
		}

		[TestMethod]
		public void Create() {
			Assert.AreEqual(BASE_LENGTH - OFFSET, offsetStream.Length);
			Assert.AreEqual(0, offsetStream.Position);
		}

		[TestMethod]
		public void Read() {
			Assert.IsTrue(baseStream.ExtractFrom(OFFSET).IsEqualTo(offsetStream));
		}
	}
}
