using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BioLib;
using BioLib.Streams;
using System.Linq;

namespace BioTests {
	[TestClass]
	public class StreamTests {
		private const int STREAM_LENGTH = 1024;

		private static byte[] searchHaystack = Encoding.ASCII.GetBytes("ABRAGADABRAKADABRA");
		private static byte[] searchNeedle = Encoding.ASCII.GetBytes("ABRAKADABRA");
		private static Stream emptyStream;
		private static Stream nullBytesStream;
		private static Stream searchStream;
		private static Stream randomStream;

		[TestInitialize]
		public void InitializeTests() {
			emptyStream = new MemoryStream();

			var data = new byte[STREAM_LENGTH];
			nullBytesStream = new MemoryStream(data);

			// Important: data needs to be a new object, otherwise the nullBytesStream will contain the searchNeedle.
			data = new byte[STREAM_LENGTH];

			searchStream = new MemoryStream(data);
			searchStream.Position = STREAM_LENGTH / 2;
			searchStream.Write(searchNeedle, 0, searchNeedle.Length);
			searchStream.Position = 0;

			randomStream = Bio.RandomStream(STREAM_LENGTH);
		}

		[TestMethod]
		public void MoveToStart() {
			nullBytesStream.Position = 10;
			Assert.AreEqual(10, nullBytesStream.Position);
			nullBytesStream.MoveToStart();
			Assert.AreEqual(0, nullBytesStream.Position);
		}

		[TestMethod]
		public void MoveToEnd() {
			nullBytesStream.MoveToEnd();
			Assert.AreEqual(STREAM_LENGTH, nullBytesStream.Position);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void KeepPosition_NullArgument_ShouldThrowException() {
			nullBytesStream.KeepPosition(null);
			Assert.AreEqual(0, nullBytesStream.Position);
		}

		[TestMethod]
		public void KeepPosition_EmptyAction() {
			Assert.AreEqual(0, nullBytesStream.Position);
			nullBytesStream.KeepPosition(() => {});
			Assert.AreEqual(0, nullBytesStream.Position);
		}

		[TestMethod]
		public void KeepPosition() {
			Assert.AreEqual(0, nullBytesStream.Position);
			nullBytesStream.KeepPosition(() => nullBytesStream.MoveToEnd());
			Assert.AreEqual(0, nullBytesStream.Position);

			nullBytesStream.KeepPosition(() => nullBytesStream.Position = 4);
			Assert.AreEqual(0, nullBytesStream.Position);

			nullBytesStream.KeepPosition(() => nullBytesStream.Skip(2));
			Assert.AreEqual(0, nullBytesStream.Position);
		}

		[TestMethod]
		public void IsEqualTo_SameObject() {
			Assert.IsTrue(emptyStream.IsEqualTo(emptyStream));
			Assert.IsTrue(nullBytesStream.IsEqualTo(nullBytesStream));
			Assert.IsTrue(randomStream.IsEqualTo(randomStream));
		}

		[TestMethod]
		public void IsEqualTo_Empty_Empty() {
			var other = new MemoryStream();
			Assert.IsTrue(emptyStream.IsEqualTo(other));
		}

		[TestMethod]
		public void IsEqualTo_DifferentLength() {
			Assert.IsFalse(emptyStream.IsEqualTo(nullBytesStream));
			Assert.IsFalse(randomStream.IsEqualTo(randomStream.Extract(4)));
		}

		[TestMethod]
		public void IsEqualTo_DifferentContent() {
			Assert.IsFalse(randomStream.IsEqualTo(nullBytesStream));
			Assert.IsFalse(randomStream.IsEqualTo(searchStream));
		}

		[TestMethod]
		public void IsEqualTo_PositionDoesNotChange() {
			const int POSITION = 8;
			randomStream.Position = POSITION;

			Assert.IsFalse(randomStream.IsEqualTo(nullBytesStream));
			Assert.AreEqual(POSITION, randomStream.Position);
			Assert.AreEqual(0, nullBytesStream.Position);
		}

		[TestMethod]
		public void Skip_Zero_PositionShouldNotChange() {
			Assert.IsTrue(nullBytesStream.Skip(0));
			Assert.AreEqual(0, nullBytesStream.Position);
		}

		[TestMethod]
		public void Skip_One() {
			Assert.IsTrue(nullBytesStream.Skip(1));
			Assert.AreEqual(1, nullBytesStream.Position);
		}

		[TestMethod]
		public void Skip_Multiple() {
			Assert.IsTrue(nullBytesStream.Skip(10));
			Assert.AreEqual(10, nullBytesStream.Position);
		}

		[TestMethod]
		public void Skip_OneUsingDefaultParameter() {
			Assert.IsTrue(nullBytesStream.Skip());
			Assert.AreEqual(1, nullBytesStream.Position);
		}

		[TestMethod]
		public void Skip_NegativeOffset_ShouldSkipBackwards() {
			nullBytesStream.Position = 1;
			Assert.IsTrue(nullBytesStream.Skip(-1));
			Assert.AreEqual(0, nullBytesStream.Position);
		}

		[TestMethod]
		public void Skip_BeforeStart_PositionShouldNotChange() {
			Assert.IsFalse(nullBytesStream.Skip(-1));
			Assert.AreEqual(0, nullBytesStream.Position);
		}

		[TestMethod]
		public void Skip_AfterEnd_PositionShouldNotChange() {
			nullBytesStream.MoveToEnd();
			Assert.AreEqual(STREAM_LENGTH, nullBytesStream.Position);
			Assert.IsFalse(nullBytesStream.Skip(1));
			Assert.AreEqual(STREAM_LENGTH, nullBytesStream.Position);
		}

		[TestMethod]
		public void Skip_BeforeLowerLimit_PositionShouldNotChange() {
			const int LIMIT = 5;
			nullBytesStream.Position = LIMIT;
			Assert.IsFalse(nullBytesStream.Skip(-1, LIMIT));
			Assert.AreEqual(LIMIT, nullBytesStream.Position);
		}

		[TestMethod]
		public void Skip_AfterUpperLimit_PositionShouldNotChange() {
			const int LIMIT = 5;
			nullBytesStream.Position = LIMIT;
			Assert.IsFalse(nullBytesStream.Skip(1, 0, LIMIT));
			Assert.AreEqual(LIMIT, nullBytesStream.Position);
		}

		[TestMethod]
		public void Copy() {
			var copy = nullBytesStream.Copy();
			Assert.AreNotEqual(copy, nullBytesStream);
			Assert.IsTrue(copy.IsEqualTo(nullBytesStream));

			copy = randomStream.Copy();
			Assert.AreNotEqual(copy, randomStream);
			Assert.IsTrue(copy.IsEqualTo(randomStream));
		}

		[TestMethod]
		public void Copy_ShouldIgnorePosition() {
			randomStream.Position = STREAM_LENGTH / 2;
			var copy = randomStream.Copy();
			Assert.AreNotEqual(copy, randomStream);
			Assert.AreEqual(STREAM_LENGTH, copy.Length);
			Assert.IsTrue(copy.IsEqualTo(randomStream));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CopyBytes_OutputStreamIsNull_ShouldThrowException() {
			randomStream.Copy(null);
		}

		[TestMethod]
		public void CopyBytes_Zero() {
			var stream = nullBytesStream.Copy();
			randomStream.Copy(stream, 0);
			Assert.IsTrue(stream.IsEqualTo(nullBytesStream));
		}

		[TestMethod]
		public void CopyBytes_EndOfStream_ShouldCopyZeroBytes() {
			var stream = nullBytesStream.Copy();
			randomStream.MoveToEnd();
			randomStream.Copy(stream, 16);
			Assert.IsTrue(stream.IsEqualTo(nullBytesStream));
		}

		[TestMethod]
		public void CopyBytes_All() {
			var stream = nullBytesStream.Copy();
			randomStream.Copy(stream);
			Assert.IsTrue(stream.IsEqualTo(randomStream));
		}

		[TestMethod]
		public void CopyBytes_Some() {
			const int POSITION = 32;
			const int BYTES = 16;

			var stream = nullBytesStream.Copy();
			var copiedBytes = randomStream.Extract(BYTES);
			randomStream.MoveToStart();

			stream.Position = POSITION;
			randomStream.Copy(stream, BYTES);

			Assert.IsFalse(stream.IsEqualTo(nullBytesStream));
			Assert.IsFalse(stream.IsEqualTo(randomStream));

			stream.MoveToStart();
			Assert.IsTrue(stream.Find(copiedBytes.ToArray()));
			Assert.AreEqual(POSITION, stream.Position);
		}

		[TestMethod]
		public void Extract_Empty() {
			var extracted = emptyStream.Extract(2);
			Assert.AreEqual(0, extracted.Length);
			Assert.IsTrue(extracted.IsEqualTo(emptyStream));
		}

		[TestMethod]
		public void Extract_ZeroBytes() {
			var extracted = randomStream.Extract(0);
			Assert.AreEqual(0, extracted.Length);
			Assert.IsTrue(extracted.IsEqualTo(emptyStream));
		}

		[TestMethod]
		public void Extract_Everything() {
			var extracted = randomStream.Extract();
			Assert.IsTrue(extracted.IsEqualTo(randomStream));
		}

		[TestMethod]
		public void Extract_Byte_FromStart() {
			var extracted = randomStream.Extract(1);
			randomStream.MoveToStart();
			Assert.AreEqual(0, extracted.Position);
			Assert.AreEqual(1, extracted.Length);
			Assert.AreEqual(randomStream.ReadByte(), extracted.ReadByte());
		}

		[TestMethod]
		public void Extract_Byte_FromPosition() {
			const int POSITION = 16;
			randomStream.Position = POSITION;

			var extracted = randomStream.Extract(1);
			randomStream.Skip(-1);
			Assert.AreEqual(1, extracted.Length);
			Assert.AreEqual(randomStream.ReadByte(), extracted.ReadByte());
		}

		[TestMethod]
		public void ExtractFrom_Byte() {
			const int POSITION = 16;
			var extracted = randomStream.ExtractFrom(POSITION, 1);
			randomStream.Skip(-1);
			Assert.AreEqual(1, extracted.Length);
			Assert.AreEqual(randomStream.ReadByte(), extracted.ReadByte());
		}

		[TestMethod]
		public void ExtractFrom_All() {
			var extracted = new MemoryStream(searchHaystack).ExtractFrom(7);
			Assert.IsTrue(extracted.IsEqualTo(new MemoryStream(searchNeedle)));
		}

		[TestMethod]
		public void Split_Empty() {
			var split = emptyStream.Split(16);
			Assert.AreEqual(0, split.ToArray().Length);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Split_InvalidPartLength_ShouldThrowException() {
			randomStream.Split(0).ToArray();
		}

		[TestMethod]
		public void Split_IntoOnePart() {
			var parts = randomStream.Split(randomStream.Length).ToArray();
			Assert.AreEqual(1, parts.Length);
			Assert.AreEqual(randomStream.Length, parts[0].Length);
		}

		[TestMethod]
		public void Split_IntoMultipleEqualParts() {
			const int PARTS = 8;
			const int PART_LENGTH = STREAM_LENGTH / PARTS;

			var i = 0;
			foreach (var part in randomStream.Split(PART_LENGTH)) {
				Assert.AreEqual(PART_LENGTH, part.Length);
				i++;
			}

			Assert.AreEqual(PARTS, i);
		}

		[TestMethod]
		public void Split_IntoMultipleParts() {
			const int PARTS = 10;
			const int PART_LENGTH = STREAM_LENGTH / (PARTS - 1);
			const int LAST_PART_LENGTH = STREAM_LENGTH - (PARTS - 1) * PART_LENGTH;

			var parts = randomStream.Split(PART_LENGTH).ToArray();
			for (var i = 0; i < parts.Length - 1; i++) {
				Assert.AreEqual(PART_LENGTH, parts[i].Length);
			}

			Assert.AreEqual(LAST_PART_LENGTH, parts[parts.Length - 1].Length);
			Assert.AreEqual(PARTS, parts.Length);
		}

		[TestMethod]
		public void Split_WithEndOffset() {
			var parts = randomStream.Split(STREAM_LENGTH, 2).ToArray();
			
			Assert.AreEqual(2, parts[0].Length);
			Assert.AreEqual(1, parts.Length);

			randomStream.MoveToStart();
			parts = randomStream.Split(1, 4).ToArray();

			Assert.AreEqual(1, parts[0].Length);
			Assert.AreEqual(4, parts.Length);

			randomStream.MoveToStart();
			parts = randomStream.Split(1, 0).ToArray();

			Assert.AreEqual(0, parts.Length);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Concatenate_Null_ShouldThrowException() {
			randomStream.Concatenate((Stream) null);
		}

		[TestMethod]
		public void Concatenate() {
			var concatenated = randomStream.Concatenate(nullBytesStream);
			Assert.AreEqual(typeof(ConcatenatedStream), concatenated.GetType());
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Search_PatternIsNull_ShouldThrowException() {
			nullBytesStream.Find(null);
		}

		[TestMethod]
		public void Search_NoMatch() {
			Assert.IsFalse(nullBytesStream.Find(searchNeedle));
			Assert.AreEqual(0, nullBytesStream.Position);
		}

		[TestMethod]
		public void Search_BytePattern() {
			Assert.IsTrue(searchStream.Find(searchNeedle));
			Assert.AreEqual(STREAM_LENGTH / 2, searchStream.Position);
			searchStream.Skip();
			Assert.IsFalse(searchStream.Find(searchNeedle));
		}

		[TestMethod]
		public void Search_StringBytes() {
			var stream = new MemoryStream(searchHaystack);

			Assert.IsTrue(stream.Find(searchNeedle));
			Assert.AreEqual(7, stream.Position);
			stream.Skip();
			Assert.IsFalse(stream.Find(searchNeedle));
		}

		[TestMethod]
		public void Search_WithLimit() {
			var limit = (int)(STREAM_LENGTH * 0.1);
			Assert.IsFalse(searchStream.Find(searchNeedle, limit));
			Assert.AreEqual(0, searchStream.Position);

			limit = (int)(STREAM_LENGTH * 0.8);
			Assert.IsTrue(searchStream.Find(searchNeedle, limit));
		}

		[TestMethod]
		public void SearchBackwards_NoMatch() {
			nullBytesStream.MoveToEnd();
			Assert.IsFalse(nullBytesStream.FindBackwards(searchNeedle));
			Assert.AreEqual(STREAM_LENGTH, nullBytesStream.Position);
		}

		[TestMethod]
		public void SearchBackwards_StringBytes() {
			var stream = new MemoryStream(searchHaystack);
			stream.MoveToEnd();

			Assert.IsTrue(stream.FindBackwards(searchNeedle));
			Assert.AreEqual(7, stream.Position);
			Assert.IsFalse(stream.FindBackwards(searchNeedle));
			Assert.AreEqual(7, stream.Position);
		}

		[TestMethod]
		public void SearchBackwards_BytePattern() {
			searchStream.MoveToEnd();
			Assert.IsTrue(searchStream.FindBackwards(searchNeedle));
			Assert.AreEqual(STREAM_LENGTH / 2, searchStream.Position);
			Assert.IsFalse(searchStream.FindBackwards(searchNeedle));
		}

		[TestMethod]
		public void SearchBackwards_WithLimit() {
			var limit = (int)(STREAM_LENGTH * 0.8);
			searchStream.MoveToEnd();
			Assert.IsFalse(searchStream.FindBackwards(searchNeedle, limit));
			Assert.AreEqual(STREAM_LENGTH, searchStream.Position);

			limit = (int)(STREAM_LENGTH * 0.1);
			Assert.IsTrue(searchStream.FindBackwards(searchNeedle, limit));
		}

		[TestMethod]
		public void Stream_Split_NoSplittingNecessary() {
			CreateTestCase_Split(16, 32);
		}

		[TestMethod]
		public void Stream_Split_Exact() {
			CreateTestCase_Split(64, 16);
		}

		[TestMethod]
		public void Stream_Split_WithRest() {
			CreateTestCase_Split(64, 15);
		}

		private void CreateTestCase_Split(int streamLength, int partLength) {
			var expectedPartCount = (int) Math.Ceiling((decimal) streamLength / partLength);

			using (var stream = Bio.RandomStream(streamLength)) {
				var i = 0;
				foreach (var part in stream.Split(partLength)) {
					i++;

					var expected = partLength;
					if (i == expectedPartCount) {
						var rest = streamLength % partLength;
						if (rest > 0) expected = rest;
					}

					Assert.AreEqual(expected, part.Length);
				}

				Assert.AreEqual(expectedPartCount, i);
			}
		}
	}
}
