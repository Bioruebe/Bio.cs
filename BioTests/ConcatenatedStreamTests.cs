using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BioLib;
using BioLib.Streams;

namespace BioTests {
	[TestClass]
	public class ConcatenatedStreamTests {
		private Stream stream1;
		private Stream stream2;
		private long combinedLength;
		private ConcatenatedStream concatenated;
		private byte[] combinedData;

		[TestInitialize]
		public void InitializeTests() {
			stream1 = Bio.RandomStream(32);
			stream2 = Bio.RandomStream(16);
			combinedLength = stream1.Length + stream2.Length;

			combinedData = new byte[combinedLength];
			stream1.Read(combinedData, 0, (int) stream1.Length);
			stream2.Read(combinedData, (int) stream1.Length, (int) stream2.Length);

			stream1.MoveToStart();
			stream2.MoveToStart();

			concatenated = new ConcatenatedStream(stream1, stream2);
		}

		[TestMethod]
		public void Create_Empty() {
			var stream = new ConcatenatedStream();
			Assert.AreEqual(0, stream.Length);
			Assert.AreEqual(0, stream.Position);

			stream = new ConcatenatedStream(new List<Stream>());
			Assert.AreEqual(0, stream.Length);
			Assert.AreEqual(0, stream.Position);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Create_ArgumentNull_ShouldThrowException() {
			new ConcatenatedStream(stream1, null);
		}

		[TestMethod]
		public void Create() {
			Bio.Cout(stream1);
			Bio.Cout(stream2);
			Assert.AreEqual(stream1.Length + stream2.Length, concatenated.Length);
			Assert.AreEqual(0, concatenated.Position);
			Bio.Cout(combinedData);
			Bio.Cout(concatenated.ToByteArray());
			Assert.IsTrue(combinedData.SequenceEqual(concatenated.ToByteArray()));
		}

		[TestMethod]
		public void Seek() {
			concatenated.MoveToStart();
			Assert.AreEqual(0, concatenated.Position);
			Assert.AreEqual(stream1.Length, concatenated.Seek(stream1.Length, SeekOrigin.Current));
			Assert.AreEqual(stream1.Length, concatenated.Position);
			concatenated.Seek(1, SeekOrigin.Current);
			Assert.AreEqual(stream1.Length + 1, concatenated.Position);
			concatenated.Seek(-1, SeekOrigin.Current);
			Assert.AreEqual(stream1.Length, concatenated.Position);
			concatenated.MoveToEnd();
			Assert.AreEqual(combinedLength, concatenated.Position);
		}

		[TestMethod]
		public void Read_BytePerByte() {
			Assert.IsTrue(stream1.IsEqualTo(concatenated.Extract(stream1.Length)));
			Assert.IsTrue(stream2.IsEqualTo(concatenated.ExtractFrom(stream1.Length)));
		}

		[TestMethod]
		public void ReadAfterRead() {
			Read_BytePerByte();
			concatenated.Position = 0;
			Read_BytePerByte();
		}

		[TestMethod]
		public void Read_Bytes_FromSingleStream() {
			Assert.IsTrue(CompareRead(8));
			Assert.IsTrue(CompareRead(8));
		}

		[TestMethod]
		public void Read_Bytes_FromBothStreams() {
			const int READ_LENGTH = 10;
			var offset = (int) stream1.Length - 2;
			concatenated.Position = offset;

			var bytesConcatenated = new byte[READ_LENGTH];
			var bytesOriginal = combinedData.Skip(offset).Take(READ_LENGTH).ToArray();

			concatenated.Position = offset;
			concatenated.Read(bytesConcatenated, 0, READ_LENGTH);

			Assert.IsTrue(CompareBytes(bytesOriginal, bytesConcatenated));
		}

		public bool CompareRead(int length) {
			var bytesConcatenated = new byte[length];
			var bytesOriginal = combinedData.Skip((int) concatenated.Position).Take(length).ToArray();

			concatenated.Read(bytesConcatenated, 0, length);
			
			return CompareBytes(bytesOriginal, bytesConcatenated);
		}

		private bool CompareBytes(byte[] expected, byte[] actual) {
			var isEqual = expected.SequenceEqual(actual);
			if (!isEqual) {
				Bio.Cout(expected);
				Bio.Cout(actual);
			}

			return isEqual;
		}

		[TestMethod]
		public void GetBaseStream() {
			Assert.AreEqual(null, new ConcatenatedStream().GetBaseStream(0));
			Assert.AreEqual(stream1, concatenated.GetBaseStream(0));
			Assert.AreEqual(stream1, concatenated.GetBaseStream(stream1.Length));
			Assert.AreEqual(stream2, concatenated.GetBaseStream(stream1.Length + 1));
			Assert.AreEqual(stream2, concatenated.GetBaseStream(combinedLength));
			Assert.AreEqual(stream2, concatenated.GetBaseStream(combinedLength + 1));
		}

		[TestMethod]
		public void Append() {
			const int APPEND_LENGTH = 16;
			var stream = Bio.RandomStream(APPEND_LENGTH);

			concatenated.Position = stream1.Length;
			concatenated.Append(stream);

			Assert.AreEqual(combinedLength + APPEND_LENGTH, concatenated.Length);
			Assert.AreEqual(concatenated.Position, stream1.Length);
			Assert.IsTrue(stream.IsEqualTo(concatenated.ExtractFrom(combinedLength)));
		}
	}
}
