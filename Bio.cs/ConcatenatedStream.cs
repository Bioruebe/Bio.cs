using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BioLib.Streams {
    /// <summary>
    /// A class that maps between two or more base streams to emulate a bigger merged stream without actually concatenating the byte data. This is especially useful when working with file streams as reading is an expensive operation.
    /// <b>Warning</b>: This class directly wraps the passed streams, therefore most methods modify their positions! Any changes to the underlying streams can lead to unspecified behaviour!
    /// </summary>
	public class ConcatenatedStream: Stream {
        private List<Stream> streams;
        private int currentStreamIndex;
        private long[] streamOffsets;

        private long _Position;
        private long _Length;

        /// <summary>
        /// Always true.
        /// </summary>
        public override bool CanRead {
            get => true;
        }

        /// <summary>
        /// Always true.
        /// </summary>
        public override bool CanSeek {
            get => true;
        }

        /// <summary>
        /// Always false. Writing is currently not supported.
        /// </summary>
        public override bool CanWrite {
            get => false;
        }

        /// <summary>
        /// Gets the length of the stream in bytes.<br/><br/>
        /// <b>Warning</b>: cached. Does not reflect changes to the underlying streams.
        /// </summary>
        public override long Length {
            get => _Length;
        }

        /// <summary>
        /// Gets or sets the current position of this stream.<br/><br/>
        /// <b>Warning</b>: setting this property directly changes the positions of the underlying streams
        /// </summary>
        public override long Position {
            get => _Position;
            set {
                _Position = value;
                var newIndex = GetBaseStreamIndex(value);
                if (newIndex < currentStreamIndex) ResetBaseStreams(newIndex + 1);

                currentStreamIndex = newIndex;
                CurrentStream.Position = value - streamOffsets[currentStreamIndex];
            }
        }

        /// <summary>
        /// Returns the mapped base stream for the current position.
        /// </summary>
        private Stream CurrentStream {
            get => currentStreamIndex < 0? null: streams[currentStreamIndex];
		}

        /// <summary>
        /// Concatenates the given <paramref name="streams"/> in the order they are passed.<br/><br/>
        /// <b>Warning</b>: This class directly wraps the passed <paramref name="streams"/>. Actions performed on the <see cref="ConcatenatedStream"/> modify their position! Any changes to the underlying <paramref name="streams"/> can lead to unspecified behaviour! See <see cref="ConcatenatedStream"/>.
        /// </summary>
        /// <param name="streams">The streams to concatenate</param>
        public ConcatenatedStream(params Stream[] streams) :
            this(streams.ToList()) { }

        /// <summary>
        /// Concatenates the given <paramref name="streams"/> in the order they are passed.<br/><br/>
        /// <b>Warning</b>: This class directly wraps the passed <paramref name="streams"/>. Actions performed on the <see cref="ConcatenatedStream"/> modify their position! Any changes to the underlying <paramref name="streams"/> can lead to unspecified behaviour! See <see cref="ConcatenatedStream"/>.
        /// </summary>
        /// <param name="streams">The stream(s) to concatenate</param>
        public ConcatenatedStream(List<Stream> streams) {
            if (streams.Any((s) => s == null)) throw new ArgumentException("Base streams cannot be null");

            this.streams = streams;
            CalculateOffsets();
            ResetBaseStreams();
            Seek(0, SeekOrigin.Begin);
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero if the end of the stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count) {
            if (streams.Count < 1) return 0;

            var bytesRead = 0;
            while (_Position < Length && bytesRead < count && currentStreamIndex < streams.Count) {
                var r = CurrentStream.Read(buffer, offset + bytesRead, count - bytesRead);
                //Bio.Debug($"Read {r} bytes @ {_Position} | stream {currentStreamIndex} @ {CurrentStream.Position}");
                bytesRead += r;
                _Position += r;

                if (r < 1) currentStreamIndex++;
            }

            return bytesRead;
        }

        /// <summary>
        /// Sets the position within the current stream
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin) {
            if (streams.Count < 1) return 0;

            switch (origin) {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    if (offset == 0) return _Position;

                    Position = _Position + offset;
                    break;

                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }

            return Position;
        }

        /// <summary>
        /// Returns the index of the mapped base stream for a given <paramref name="position"/>.
        /// </summary>
        /// <param name="position">The position in the concatenated stream to retrieve the base stream for</param>
        /// <returns></returns>
        private int GetBaseStreamIndex(long position) {
            if (streams.Count < 1) return -1;

            for (var i = 0; i < streams.Count; i++) {
                position -= streams[i].Length;
                if (position <= 0) return i;
            }

            return streams.Count - 1;
        }

        /// <summary>
        /// Returns the mapped base stream for a given <paramref name="position"/>.
        /// </summary>
        /// <param name="position">The position in the concatenated stream to retrieve the base stream for</param>
        /// <returns>The mapped base stream</returns>
        public Stream GetBaseStream(long position) {
            var index = GetBaseStreamIndex(position);
            return (index < 0)? null: streams[index];
        }

        /// <summary>
        /// Sets the position of all base streams to 0.
        /// </summary>
        /// <param name="startIndex">The index from which resetting should start</param>
        private void ResetBaseStreams(int startIndex = 0) {
            for (var i = startIndex; i < streams.Count; i++) {
                streams[i].Position = 0;
            }
        }

        /// <summary>
        /// Calculates the position of each base stream within the <see cref="ConcatenatedStream"/>.<br/>
        /// This is a performance opimisation as retrieving the length of a <see cref="FileStream"/> is very slow.
        /// </summary>
        private void CalculateOffsets() {
            streamOffsets = new long[streams.Count];

            var sum = 0L;
            for (var i = 0; i < streams.Count; i++) {
                streamOffsets[i] = sum;
                sum += streams[i].Length;
			}

            _Length = sum;
		}

        /// <summary>
        /// Appends the passed <paramref name="streams"/> to the <see cref="ConcatenatedStream"/>
        /// </summary>
        /// <param name="streams">The streams to append</param>
        public void Append(IEnumerable<Stream> streams) {
            this.streams.AddRange(streams);
            CalculateOffsets();
		}

        /// <summary>
        /// Appends the passed <paramref name="streams"/> to the <see cref="ConcatenatedStream"/>
        /// </summary>
        /// <param name="streams">The streams to append</param>
        public void Append(params Stream[] streams) {
            Append(streams.ToList());
        }

        /// <summary>
        /// Not supported. Do not use.
        /// </summary>
        public override void Flush() => throw new NotSupportedException();

        /// <summary>
        /// Not supported. Do not use.
        /// </summary>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <summary>
        /// Not supported. Do not use.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
