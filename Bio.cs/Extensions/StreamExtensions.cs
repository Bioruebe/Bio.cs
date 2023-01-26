using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace BioLib.Streams {
    /// <summary>
    /// Extension methods for generic <see cref="Stream"/>s
    /// </summary>
    public static class StreamExtensions {
        /// <summary>
        /// The size of the internal buffer used for most of the functions
        /// </summary>
        public static int bufferSize = 1024;

        /// <summary>
        /// The magic number to detect MZ executables
        /// </summary>
        private static readonly byte[] MAGIC_EXE = { 0x4D, 0x5A };

        /// <summary>
        /// Return the remaining length of the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>The remaining length of the stream</returns>
        public static long RemainingLength(this Stream stream) {
            return stream.Length - stream.Position;
        }

        /// <summary>
        /// Ensure the <paramref name="stream"/>'s position is not changed by the <paramref name="action"/> passed as parameter.
        /// <br/><br/>Usage:
        /// <code>myStream.KeepPosition(() => myStream.Copy(otherStream));</code>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="action"></param>
        public static void KeepPosition(this Stream stream, Action action) {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var position = stream.Position;
            action();
            stream.Position = position;
        }

        /// <summary>
        /// Compare two Streams by content. Does not change the streams' positions.
        /// </summary>
        /// <param name="stream">First stream to compare</param>
        /// <param name="other">Second stream to compare</param>
        /// <returns>True if the streams' contents are equal, otherwise false</returns>
        public static bool IsEqualTo(this Stream stream, Stream other) {
            if (stream.Equals(other)) return true;
            if (stream.Length != other.Length) return false;

            var streamPos = stream.Position;
            var otherPos = other.Position;

            stream.MoveToStart();
            other.MoveToStart();

            var isEqual = true;
            int read;
            while ((read = stream.ReadByte()) != -1) {
                var readOther = other.ReadByte();
                //Bio.Cout(read + "\t" + readOther);
                if (read == readOther) continue;

                isEqual = false;
                break;
            }

            stream.Position = streamPos;
            other.Position = otherPos;

            return isEqual;
        }

        /// <summary>
        /// Check if the stream's position is zero
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>True if the stream's position is zero, otherwise false</returns>
        public static bool IsAtStart(this Stream stream) {
            return stream.Position == 0;
        }

        /// <summary>
        /// Check if the stream's position is the end of the stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>True if the stream's position is the end of the stream, otherwise false</returns>
        public static bool IsAtEnd(this Stream stream) {
            return stream.Position == stream.Length;
        }

        /// <summary>
        /// Test whether the data at the stream's current position matches the given <paramref name="pattern"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pattern">The pattern to test</param>
        /// <param name="advancePosition">Whether to reset the stream's position after comparision or not.</param>
        /// <returns>True or false</returns>
        public static bool Matches(this Stream stream, byte[] pattern, bool advancePosition = false) {
            if (stream.RemainingLength() < pattern.Length) return false;

            var buffer = new byte[pattern.Length];

            if (advancePosition) {
                stream.Read(buffer, 0, pattern.Length);
            }
            else {
                stream.KeepPosition(() => stream.Read(buffer, 0, pattern.Length));
            }

            return Bio.MatchPattern(buffer, pattern);
        }

        /// <summary>
        /// Determine whether the data at the current is an MZ executable.
        /// Does not advance the stream's position.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>True or false</returns>
        public static bool IsExecutable(this Stream stream) {
            return stream.Matches(MAGIC_EXE);
        }

        /// <summary>
        /// Move to the beginning of the stream
        /// </summary>
        /// <param name="stream"></param>
        public static void MoveToStart(this Stream stream) {
            stream.Position = 0;
        }

        /// <summary>
        /// Move to the end of the stream
        /// </summary>
        /// <param name="stream"></param>
        public static void MoveToEnd(this Stream stream) {
            stream.Position = stream.Length;
        }

        /// <summary>
        /// Decrease the stream's position by N <paramref name="bytes"/> while staying in bounds.<br/><br/>
        /// Convenience function for calling <see cref="Skip(Stream, long, long, long)"/> with a negative number.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes">Amount of bytes to move backwards. Can be negative.</param>
        /// <param name="lowerLimit">Optional lower limit, defaults to 0</param>
        /// <param name="upperLimit">Optional upper limt, defaults to the stream's length</param>
        /// <returns>True, if the position could be set; false, if the new position would have been out of bounds</returns>
        /// <returns></returns>
        public static bool Back(this Stream stream, long bytes = 1, long lowerLimit = 0, long upperLimit = -1) {
            return Skip(stream, -bytes, lowerLimit, upperLimit);
        }

        /// <summary>
        /// Advance the stream's position by N <paramref name="bytes"/> while staying in bounds
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes">Amount of bytes to move. Can be negative.</param>
        /// <param name="lowerLimit">Optional lower limit, defaults to 0</param>
        /// <param name="upperLimit">Optional upper limt, defaults to the stream's length</param>
        /// <returns>True, if the position could be set; false, if the new position would have been out of bounds</returns>
        public static bool Skip(this Stream stream, long bytes = 1, long lowerLimit = 0, long upperLimit = -1) {
            var newPosition = stream.Position + bytes;
            if (upperLimit < 0) upperLimit = stream.Length - 1;
            if (newPosition < lowerLimit || newPosition > upperLimit) return false;

            stream.Position = newPosition;
            return true;
        }

        /// <summary>
        /// Create a copy of a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>A <see cref="MemoryStream"/> with a copy of the stream's content</returns>
        public static MemoryStream Copy(this Stream stream) {
            return stream.ExtractFrom(0);
        }

        /// <summary>
        /// Copy N <paramref name="bytes"/> from <paramref name="input"/> to <paramref name="output"/> stream
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <param name="output">Output stream</param>
        /// <param name="bytes">Amount of bytes to copy or -1 to copy all</param>
        public static void Copy(this Stream input, Stream output, long bytes = -1) {
            if (output == null) throw new ArgumentNullException(nameof(output));

            var buffer = new byte[bufferSize];
            int read;
            if (bytes < 0) bytes = input.Length - input.Position;

            //Bio.Debug($"Copying {bytes} bytes from position {input.Position}");
            while (bytes > 0 && (read = input.Read(buffer, 0, (int)Math.Min(bufferSize, bytes))) > 0) {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }

        /// <summary>
        /// Copy all bytes from the current position until <paramref name="endPosition"/> to <paramref name="output"/> stream
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <param name="output">Output stream</param>
        /// <param name="endPosition">The end offset (exclusive)</param>
        public static void CopyUntil(this Stream input, Stream output, long endPosition) {
            input.Copy(output, (int)(endPosition - input.Position));
        }

        /// <summary>
        /// Extract N bytes from the current position and return them as a new <see cref="MemoryStream"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes">The amount of bytes to extract</param>
        /// <returns>A new <see cref="MemoryStream"/> with the extracted part of the stream</returns>
        public static MemoryStream Extract(this Stream stream, long bytes = -1) {
            var memoryStream = new MemoryStream();
            stream.Copy(memoryStream, bytes);
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Extract N bytes beginning at a given <paramref name="startOffset"/> and return them as a new <see cref="MemoryStream"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="startOffset"></param>
        /// <param name="bytes"></param>
        /// <returns>A new <see cref="MemoryStream"/> with the extracted part of the stream</returns>
        public static MemoryStream ExtractFrom(this Stream stream, long startOffset, int bytes = -1) {
            stream.Position = startOffset;
            return Extract(stream, bytes);
        }

        /// <summary>
        /// Extract the bytes between the current position and the <paramref name="endOffset"/> and return them as a new <see cref="MemoryStream"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="endOffset"></param>
        /// <returns>A new <see cref="MemoryStream"/> with the extracted part of the stream</returns>
        public static MemoryStream ExtractUntil(this Stream stream, long endOffset) {
            var memoryStream = new MemoryStream();
            stream.CopyUntil(memoryStream, endOffset);
            return memoryStream;
        }

        /// <summary>
        /// Extract the bytes between <paramref name="startOffset"/> and <paramref name="endOffset"/> and return them as a new <see cref="MemoryStream"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="startOffset"></param>
        /// <param name="endOffset"></param>
        /// <returns>A new <see cref="MemoryStream"/> with the extracted part of the stream</returns>
        public static MemoryStream ExtractFromUntil(this Stream stream, long startOffset, long endOffset) {
            stream.Seek(startOffset, SeekOrigin.Begin);
            return ExtractUntil(stream, endOffset);
        }

        /// <summary>
        /// Split <paramref name="stream"/> into <see cref="MemoryStream"/>s of <paramref name="length"/> <see langword="null"/>,
        /// beginning at the current position of the stream.<br/>
        /// This returns data on demand (as an <see cref="IEnumerable{MemoryStream}"/>), so it should be efficient
        /// even for large input streams.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="length">The length of each part (in bytes)</param>
        /// <param name="endOffset">Position at which to stop splitting or -1 to continue until the end</param>
        /// <returns></returns>
        public static IEnumerable<MemoryStream> Split(this Stream stream, long length, long endOffset = -1) {
            if (length < 1) throw new ArgumentOutOfRangeException(nameof(length));
            if (endOffset < 0) endOffset = stream.Length;

            while (stream.Position < endOffset) {
                yield return Extract(stream, Math.Min(length, endOffset - stream.Position));
            }
        }

        /// <summary>
        /// Appends <paramref name="other"/> to this stream. This is a convenience function, which creates a new instance of <see cref="ConcatenatedStream"/>.<br/><br/>
        /// <b>Warning</b>: Actions performed on the <see cref="ConcatenatedStream"/> modify the underlying streams directly! Any changes to <paramref name="stream"/> or <paramref name="other"/> can lead to unspecified behaviour! Make sure to read the documentation of <see cref="ConcatenatedStream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static ConcatenatedStream Concatenate(this Stream stream, Stream other) {
            if (other == null) throw new ArgumentNullException(nameof(other));

            return new ConcatenatedStream(stream, other);
        }

        /// <summary>
        /// Appends <paramref name="other"/> to this stream. This is a convenience function, which creates a new instance of <see cref="ConcatenatedStream"/>.<br/><br/>
        /// <b>Warning</b>: Actions performed on the <see cref="ConcatenatedStream"/> modify the underlying streams directly! Any changes to <paramref name="stream"/> or <paramref name="other"/> can lead to unspecified behaviour! Make sure to read the documentation of <see cref="ConcatenatedStream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static ConcatenatedStream Concatenate(this Stream stream, List<Stream> other) {
            if (other == null) throw new ArgumentNullException(nameof(other));
            other.Insert(0, stream);

            return new ConcatenatedStream(other);
        }

        /// <summary>
        /// Find the given <paramref name="pattern"/> in the stream and set the stream's position to the position at which it was found.
        /// If the <paramref name="pattern"/> was not found, the position does not change.
        /// <br/><br/>
        /// <b>Warning</b>: This function uses naive search, reading and comparing a single byte at a time. This can be very slow for big streams!
        /// Only use this function if you expect the pattern to be found and/or set the <paramref name="endOffset"/>. Otherwise your programm may freeze.
        /// </summary>
        /// <param name="stream">The search stream</param>
        /// <param name="pattern">The byte pattern to search for</param>
        /// <param name="endOffset">An optional index at which the search should stop, defaults to the end of the stream</param>
        /// <returns>True if the pattern was found, otherwise false</returns>
        public static bool Find(this Stream stream, byte[] pattern, long endOffset = -1) {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            var patternPosition = 0;
            var patternLength = pattern.Length;
            var initialStreamPosition = stream.Position;
            int read;

            while ((read = stream.ReadByte()) != -1) {
                //Bio.Cout($"{stream.Position}| {read} <-> {pattern[patternPosition]} ({patternPosition})");
                if (read == pattern[patternPosition]) {
                    patternPosition++;
                    if (patternPosition == patternLength) {
                        stream.Skip(-patternLength);
                        return true;
                    }
                }
                else {
                    patternPosition = 0;
                }

                if (endOffset > -1 && stream.Position >= endOffset) break;
            }

            stream.Position = initialStreamPosition;
            return false;
        }

        /// <summary>
        /// Find the given <paramref name="pattern"/> in the stream and set the stream's position to the position at which it was found.
        /// If the <paramref name="pattern"/> was not found, the position does not change.
        /// <br/><br/>
        /// <b>Warning</b>: This function uses naive search, reading and comparing a single byte at a time. This can be very slow for big streams!
        /// Only use this function if you expect the pattern to be found and/or set the <paramref name="endOffset"/>. Otherwise your programm may freeze.
        /// </summary>
        /// <param name="stream">The search stream</param>
        /// <param name="pattern">The string pattern to search for</param>
        /// <param name="endOffset">An optional index at which the search should stop, defaults to the end of the stream</param>
        /// <param name="utf8">Whether the pattern is UTF-8 or not</param>
        /// <returns>True if the pattern was found, otherwise false</returns>
        public static bool Find(this Stream stream, string pattern, long endOffset = -1, bool utf8 = true) {
            var encoding = utf8? Encoding.UTF8: Encoding.ASCII;
            var bytePattern = encoding.GetBytes(pattern);

            return Find(stream, bytePattern, endOffset);
        }

        /// <summary>
        /// Find the given <paramref name="pattern"/> in the stream using backwards search and set the stream's position to the position at which it was found.
        /// If the <paramref name="pattern"/> was not found, the position does not change.
        /// <br/><br/>
        /// <b>Warning</b>: This function uses naive search, reading and comparing a single byte at a time. This can be very slow for big streams!
        /// Only use this function if you expect the pattern to be found and/or set the <paramref name="endOffset"/>. Otherwise your programm may freeze.
        /// </summary>
        /// <param name="stream">The search stream</param>
        /// <param name="pattern">The byte pattern to search for</param>
        /// <param name="endOffset">An optional index at which the search should stop, defaults to the beginning of the stream</param>
        /// <returns>True if the pattern was found, otherwise false</returns>
        public static bool FindBackwards(this Stream stream, byte[] pattern, long endOffset = 0) {
            var patternMaxIndex = pattern.Length - 1;
            var patternPosition = patternMaxIndex;
            var initialStreamPosition = stream.Position;

            if (stream.Position >= stream.Length) stream.Position = stream.Length - 1;

            int read;

            while ((read = stream.ReadByte()) != -1) {
                //Bio.Cout($"{stream.Position}| {read:X2} <-> {pattern[patternPosition]:X2} ({patternPosition})");
                if (read == pattern[patternPosition]) {
                    patternPosition--;
                    if (patternPosition < 0) {
                        stream.Skip(-1);
                        return true;
                    }
                }
                else {
                    patternPosition = patternMaxIndex;
                }

                if (!stream.Skip(-2, endOffset)) {
                    stream.Skip(-1);
                    break;
                }
            }

            stream.Position = initialStreamPosition;
            return false;
        }

        /// <summary>
        /// Find all occurences of the given <paramref name="pattern"/> in the stream. The stream's position will not be modified.
        /// <br/><br/>
        /// <b>Warning</b>: This function uses naive search, reading and comparing a single byte at a time. This can be very slow for big streams!
        /// Only use this function if you expect the pattern to be found and/or set the <paramref name="endOffset"/>. Otherwise your programm may freeze.
        /// </summary>
        /// <param name="stream">The search stream</param>
        /// <param name="pattern">The byte pattern to search for</param>
        /// <param name="endOffset">An optional index at which the search should stop, defaults to the end of the stream</param>
        /// <returns>A list of positions at which the pattern was found</returns>
        public static List<long> FindAll(this Stream stream, byte[] pattern, long endOffset = -1) {
            var offsets = new List<long>();

            stream.KeepPosition(() => {
                stream.MoveToStart();

                while (stream.Find(pattern, endOffset)) {
                    offsets.Add(stream.Position);
                    if (!stream.Skip(pattern.Length)) break;
                }
            });

            return offsets;
        }

        /// <summary>
        /// Write the complete content of a stream to a file. Does not advance the stream's position.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="path">Path to the file</param>
        /// <param name="promptId"></param>
        /// <param name="copyFunction"></param>
        /// <returns>True if the operation succeeded, otherwise false. Exceptions might be thrown depending on the <paramref name="copyFunction"/>.</returns>
        public static bool WriteToFile(this Stream input, string path, string promptId = null, Action<Stream, Stream> copyFunction = null) {
            if (copyFunction == null) copyFunction = (inputStream, outputStream) => inputStream.Copy(outputStream);

            return WriteToFileRelative(input, path, promptId, (inputStream, outputStream) => {
                input.MoveToStart();
                copyFunction(input, outputStream);
            });
        }

        /// <summary>
        /// Write the content of a stream (from the current position to the end) to a file. Does not advance the stream's position.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="path">Path to the file</param>
        /// <param name="promptId"></param>
        /// <param name="copyFunction"></param>
        /// <returns>True if the operation succeeded, otherwise false. Exceptions might be thrown depending on the <paramref name="copyFunction"/>.</returns>
        public static bool WriteToFileRelative(this Stream input, string path, string promptId = null, Action<Stream, Stream> copyFunction = null) {
            if (copyFunction == null) copyFunction = (inputStream, outputStream) => inputStream.Copy(outputStream);
            
            using (var fileStream = Bio.CreateFile(path, promptId)) {
                if (fileStream == null) return true; // User decided to not overwrite existing file
                input.KeepPosition(() => copyFunction(input, fileStream));
            }

            return true;
        }

        /// <summary>
        /// Reads the <paramref name="stream"/> from beginning to end and returns its content as a byte array.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this Stream stream) {
            return stream.Copy().ToArray();
        }

        /// <summary>
        /// Converts the stream to an UTF-8 string
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>An UTF-8 string</returns>
        public static string ToUtf8String(this Stream stream) {
            return Bio.BytesToString(stream.ToByteArray());
        }
    }
}