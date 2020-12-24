using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BioLib.Streams {
	/// <summary>
	/// A wrapper that maps a base stream by a specified offset.<br/><br/>
	/// <b>Warning</b>: This class directly operates on the base stream, i.e. changing its position after read actions.<br/>
	/// Any changes to the underlying stream can lead to unspecified behaviour!
	/// </summary>
	public class OffsetStream : Stream {
		private Stream baseStream;
		private long offset;

		/// <summary>
		/// Gets a value indicating whether the current stream supports reading.
		/// </summary>
		public override bool CanRead => baseStream.CanRead;

		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking.
		/// </summary>
		public override bool CanSeek => baseStream.CanSeek;

		/// <summary>
		/// Gets a value indicating whether the current stream supports writing.
		/// </summary>
		public override bool CanWrite => baseStream.CanWrite;

		/// <summary>
		/// Gets the length of the stream in bytes. Calculated as the length of the base stream minus the offset.
		/// </summary>
		public override long Length => baseStream.Length - offset;

		/// <summary>
		/// Gets or sets the position within the current stream.
		/// </summary>
		public override long Position {
			get => baseStream.Position - offset;
			set => baseStream.Position = value + offset;
		}

		/// <summary>
		/// Creates a stream that wraps the <paramref name="baseStream"/> ignoring the first <paramref name="offset"/> bytes of the original.
		/// </summary>
		/// <param name="baseStream">The stream to offset</param>
		/// <param name="offset">The offset in bytes</param>
		public OffsetStream(Stream baseStream, long offset) {
			if (baseStream == null) throw new ArgumentNullException(nameof(baseStream));
			if (offset < 1 || offset >= baseStream.Length - 1) throw new ArgumentOutOfRangeException(nameof(offset), offset, $"The offset exceeds the allowed range [0; {baseStream.Length - 2}]");

			this.baseStream = baseStream;
			this.offset = offset;
			Position = 0;
		}

		/// <summary>
		/// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero if the end of the stream has been reached.</returns>
		public override int Read(byte[] buffer, int offset, int count) {
			return baseStream.Read(buffer, offset, count);
		}

		/// <summary>
		/// Sets the position within the current stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
		/// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
		/// <returns>The new position within the current stream.</returns>
		public override long Seek(long offset, SeekOrigin origin) {
			if (origin == SeekOrigin.Begin) {
				offset += this.offset;
			}
			else if (origin == SeekOrigin.End) {
				offset = Math.Max(this.offset, Length + offset);
			}

			return baseStream.Seek(offset, SeekOrigin.Begin);
		}

		/// <summary>
		/// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
		/// </summary>
		public override void Flush() => baseStream.Flush();

		/// <summary>
		/// Sets the length of the current stream.
		/// </summary>
		/// <param name="value">The desired length of the current stream in bytes.</param>
		public override void SetLength(long value) => baseStream.SetLength(value + offset);

		/// <summary>
		/// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
		/// <param name="count">The number of bytes to be written to the current stream.</param>
		public override void Write(byte[] buffer, int offset, int count) => baseStream.Write(buffer, offset, count);
	}
}
