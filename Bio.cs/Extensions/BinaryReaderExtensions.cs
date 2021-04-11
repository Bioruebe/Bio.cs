using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace BioLib.Streams {
	/// <summary>
	/// Extension methods for <see cref="BinaryReader"/>
	/// </summary>
	public static class BinaryReaderExtensions {
		/// <summary>
		/// Read a null-terminated ASCII or UTF-8 string
		/// </summary>
		/// <param name="binaryReader"></param>
		/// <param name="utf8">Interpret the string as UTF-8; otherwise ASCII is used</param>
		/// <returns></returns>
		public static string ReadNullTerminatedString(this BinaryReader binaryReader, bool utf8 = false) {
			var bytes = new List<byte>();
			byte read;

			while ((read = binaryReader.ReadByte()) != 0x00) {
				bytes.Add(read);
			}

			var encoding = utf8? Encoding.UTF8: Encoding.ASCII;
			return encoding.GetString(bytes.ToArray());
		}

		/// <summary>
		/// Read backwards from the current position until a null character is encountered and return the string
		/// representation in ASCII or UTF-8.<br/>Only parsing is done backwards, the string is still read from the beginning!
		/// </summary>
		/// <param name="binaryReader"></param>
		/// <param name="utf8">Interpret the string as UTF-8; otherwise ASCII is used</param>
		/// <returns></returns>
		public static string ReadNullTerminatedStringBackwards(this BinaryReader binaryReader, bool utf8 = false) {
			var bytes = new Stack<byte>();
			byte read;

			binaryReader.BaseStream.Skip(-1);
			while ((read = binaryReader.ReadByte()) != 0x00) {
				bytes.Push(read);
				if (!binaryReader.BaseStream.Skip(-2)) break;
			}

			var encoding = utf8 ? Encoding.UTF8 : Encoding.ASCII;
			return encoding.GetString(bytes.ToArray());
		}
	}
}
