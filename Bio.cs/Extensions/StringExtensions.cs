using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BioLib.Strings {
	/// <summary>
	/// Extension methods for <see cref="string"/>s
	/// </summary>
	public static class StringExtensions {
		/// <summary>
		/// Removes all control characters from the given string.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveControlCharacters(this string input) {
			return new string(input.Where(c => !char.IsControl(c)).ToArray());
		}
	}
}
