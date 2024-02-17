using BioLib.Streams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using BioLib.Strings;

namespace BioLib {
	/// <summary>
	///  Main library
	/// </summary>
	public static class Bio {
		/// <summary>
		/// If enabled, <see cref="Cout(string, LOG_SEVERITY, bool)"/> calls prepend the current date and time to each message.
		/// </summary>
		public static bool CoutPrintTime = false;

		/// <summary>
		/// If enabled, all messages printed with <see cref="Cout(string, LOG_SEVERITY, bool)"/> are saved internally.<br/>
		/// Use <see cref="CoutGetLog"/> to retrieve the complete log.
		/// </summary>
		public static bool CoutKeepLog = false;

		private const string SEPARATOR = "\n---------------------------------------------------------------------";

		private static readonly Dictionary<string, char> promptSettings = new Dictionary<string, char>();
		private static readonly Random random = new Random();
		private static int lastProgress = -1;
		private static readonly StringBuilder logStringBuilder = new StringBuilder();

		/// <summary>
		/// Test if a byte array contains a specific <paramref name="pattern"/> at position <paramref name="pos"/> by comparing each byte.
		/// </summary>
		/// <param name="array">Input array to search pattern in</param>
		/// <param name="pattern">The pattern to search</param>
		/// <param name="pos">Index to start searching at</param>
		/// <returns></returns>
		public static bool MatchPattern(byte[] array, byte[] pattern, int pos = 0) {
			if (pattern.Length > array.Length - pos) return false;

			for (int i = 0; i < pattern.Length; i++) {
				if (array[pos + i] != pattern[i]) return false;
			}

			return true;
		}

		/// <summary>
		/// Executes an <paramref name="action"/> N <paramref name="times"/>
		/// </summary>
		/// <param name="action">The action to call</param>
		/// <param name="times">The number of executions</param>
		public static void Repeat(Action action, int times) {
			for (var i = 0; i < times; i++) {
				action();
			}
		}

		/// <summary>
		/// Return <paramref name="value"/> clamped to the inclusive range of <paramref name="min"/> and <paramref name="max"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value">The value to clamp</param>
		/// <param name="min">Minimum value (inclusive)</param>
		/// <param name="max">Maximum value (inclusive)</param>
		/// <returns></returns>
		public static T Clamp<T>(T value, T min, T max) where T: IComparable<T> {
			if (value.CompareTo(min) < 0) return min;
			if (value.CompareTo(max) > 0) return max;
			return value;
		}

		/// <summary>
		/// Return a random number between <paramref name="min"/> (inclusive) and <paramref name="max"/> (exclusive)
		/// </summary>
		/// <param name="min">Minimum value (inclusive)</param>
		/// <param name="max">Maximum value (exclusive)</param>
		/// <returns></returns>
		public static int RandomInt(int min, int max) {
			return random.Next(min, max);
		}

		/// <summary>
		/// Create an array of random numbers
		/// </summary>
		/// <typeparam name="T">A numeric data type</typeparam>
		/// <param name="arraySize">The amount of values to generate</param>
		/// <param name="min">Minimum value for each number (inclusive)</param>
		/// <param name="max">Maximum value for each number (inclusive)</param>
		/// <returns>A random array of the specified type</returns>
		public static T[] RandomArray<T>(int arraySize, int min = 0, int max = int.MaxValue) {
			var array = new T[arraySize];

			for (int i = 0; i < arraySize; i++) {
				array[i] = (T) Convert.ChangeType(random.Next(min, max + 1), typeof(T));
			}

			return array;
		}

		/// <summary>
		/// Create a random byte array
		/// </summary>
		/// <param name="arraySize">The amount of values to generate</param>
		/// <param name="min">Minimum value for each number (inclusive)</param>
		/// <param name="max">Maximum value for each number (inclusive)</param>
		/// <returns>A random byte array</returns>
		public static byte[] RandomByteArray(int arraySize, byte min = 0, byte max = byte.MaxValue) {
			return RandomArray<byte>(arraySize, min, max);
		}

		/// <summary>
		/// Create a <see cref="MemoryStream"/> filled with random bytes
		/// </summary>
		/// <param name="length"></param>
		/// <returns></returns>
		public static Stream RandomStream(int length) {
			return new MemoryStream(RandomByteArray(length));
		}

		/// <summary>
		/// Convert a string of hex values to a bytes object
		/// </summary>
		/// <param name="hex">A string of hex values</param>
		/// <returns></returns>
		public static byte[] HexToBytes(string hex) {
			if (hex == null) throw new ArgumentNullException(nameof(hex));

			hex = hex.Replace(" ", "");
			var length = hex.Length;
			var bytes = new byte[length / 2];

			for (int i = 0; i < length; i += 2) {
				bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
			}

			return bytes;
		}

		/// <summary>
		/// Converts a byte array to an UTF-8 string
		/// </summary>
		/// <param name="bytes">The bytes to convert</param>
		/// <param name="utf8">Use UTF-8 instead of ASCII encoding</param>
		/// <param name="ignoreTrailingNullBytes">If true, any null bytes at the end of the buffer are ignored</param>
		/// <returns></returns>
		public static string BytesToString(byte[] bytes, bool utf8 = true, bool ignoreTrailingNullBytes = false) {
			var encoding = utf8? Encoding.UTF8: Encoding.ASCII;

			int lastIndex = ignoreTrailingNullBytes? Array.FindLastIndex(bytes, b => b != 0) + 1: bytes.Length;

			return encoding.GetString(bytes, 0, lastIndex);
		}

		/// <summary>
		/// Perform a XOR operation on two byte arrays
		/// </summary>
		/// <param name="bytesA">The first byte array</param>
		/// <param name="bytesB">The second byte array</param>
		/// <returns>A new byte array with the result of <paramref name="bytesA"/> XOR <paramref name="bytesB"/></returns>
		public static byte[] Xor(byte[] bytesA, byte[] bytesB) {
			var size = bytesA.Length;
			if (bytesB.Length != size) throw new ArgumentException("The byte arrays must be of the same length.");

			var output = new byte[size];

			for (var i = 0; i < size; i++) {
				output[i] = (byte) (bytesA[i] ^ bytesB[i]);
			}

			return output;
		}

		/// <summary>
		/// Creates a file at the given path and returns a new <see cref="FileStream"/>.
		/// A prompt will be shown if the file already exists. The directory structure will be created if necessary.
		/// </summary>
		/// <param name="path">The path to the file</param>
		/// <param name="promptId">A unique ID for the prompt, refer to <see cref="Prompt(string, string, PromptOptions)"/> for more information.</param>
		/// <returns></returns>
		public static FileStream CreateFile(string path, string promptId = null) {
			if (promptId != null) path = EnsureFileDoesNotExist(path, promptId);
			if (path == null) return null;
			
			var fileMode = FileMode.CreateNew;
			CreateDirectoryStructure(path);

			if (File.Exists(path)) fileMode = FileMode.Create;

			return new FileStream(path, fileMode);
		}

		/// <summary>
		/// Convenience function. Ensures a path is valid and does not exist.
		/// If the path already exists, a prompt is displayed asking the user to overwrite or rename.
		/// </summary>
		/// <param name="path">The path to test</param>
		/// <param name="promptId">A unique ID for the prompt, refer to <see cref="Prompt(string, string, PromptOptions)"/> for more information.</param>
		/// <returns>The file path depending on user choice or null indicating the file should not be overwritten.</returns>
		public static string EnsureFileDoesNotExist(string path, string promptId = "") {
			path = PathReplaceInvalidChars(path);
			if (!File.Exists(path)) return path;

			var promptOptions = new PromptOptions(new List<PromptOption>() {
				new PromptOption("yes", () => path, 'y'),
				new PromptOption("no", () => null, 'n'),
				new PromptOption("rename", () => GetRenamedFilePath(path), 'r'),
				new PromptOption("always", () => {
					promptSettings.Add(promptId, 'y');
					return path;
				}),
				new PromptOption("never", () => {
					promptSettings.Add(promptId, 'n');
					return null;
				}),
				new PromptOption("rename all", () => {
					promptSettings.Add(promptId, 'r');
					return GetRenamedFilePath(path);
				}, 'l'),
			}, 'n');

			return (string) Prompt($"\n\nThe file {path} already exists. Overwrite?", promptId, promptOptions);
		}

		/// <summary>
		/// Rename a file Windows Explorer style (by appending ' (&lt;number&gt;)').
		/// This function ensures to return a path, which does not exist already.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetRenamedFilePath(string path) {
			var directory = Path.GetDirectoryName(path);
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			var fileExtension = Path.GetExtension(path);
			
			var i = 2;
			do {
				path = Path.Combine(directory, $"{fileNameWithoutExtension} ({i}){fileExtension}");
				i++;
			}
			while (File.Exists(path));

			return path;
		}

		/// <summary>
		/// Test if a path contains invalid characters
		/// </summary>
		/// <param name="path">The path to test</param>
		/// <returns></returns>
		public static bool PathContainsInvalidChars(string path) {
			if (path == null) return false;

			return path.IndexOfAny(Path.GetInvalidPathChars()) > -1;
		}

		/// <summary>
		/// Replace all invalid characters in a path string
		/// </summary>
		/// <param name="path">The path to sanitize</param>
		/// <param name="by">The string with which invalid characters will be replaced</param>
		/// <param name="isDirectory">If the path points to a directory, file name checks are disabled</param>
		/// <returns></returns>
		public static string PathReplaceInvalidChars(string path, string by = "_", bool isDirectory = false) {
			if (path == null) throw new ArgumentNullException(nameof(path));
			if (by == null) throw new ArgumentNullException(nameof(by));

			path = path.RemoveControlCharacters();
			path = string.Join(by, path.Split(Path.GetInvalidPathChars()));
			if (!isDirectory) path = PathReplaceInvalidFileNameChars(path, by);

			return path;
		}

		/// <summary>
		/// Replace all invalid characters in a file name. This function only tests the file part of a path string,
		/// it does not guarantee that the whole path is valid. Use <see cref="PathReplaceInvalidChars(string, string, bool)"/> instead.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="by"></param>
		/// <returns></returns>
		private static string PathReplaceInvalidFileNameChars(string path, string by = "_") {
			if (path == null) throw new ArgumentNullException(nameof(path));
			if (by == null) throw new ArgumentNullException(nameof(by));

			var fileName = Path.GetFileName(path);
			fileName = fileName.RemoveControlCharacters();
			fileName = string.Join(by, fileName.Split(Path.GetInvalidFileNameChars()));
			return Path.Combine(Path.GetDirectoryName(path), fileName);
		}

		/// <summary>
		/// Combine a (relative or absolute) file path and an output directory.
		/// The returned path is guaranteed to be valid and inside the <paramref name="outputDirectory"/>.
		/// If a valid path could not be created, a <see cref="SecurityException"/> is thrown.
		/// </summary>
		/// <param name="outputDirectory"></param>
		/// <param name="filePath">Relative or absolute path to be merged with <paramref name="outputDirectory"/></param>
		/// <returns></returns>
		public static string GetSafeOutputPath(string outputDirectory, string filePath) {
			if (string.IsNullOrWhiteSpace(outputDirectory)) throw new ArgumentNullException(nameof(outputDirectory));
			if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(filePath));

			outputDirectory = Path.GetFullPath(outputDirectory);
			filePath = PathReplaceInvalidChars(filePath);
			filePath = PathRemoveRelativeParts(filePath).Replace(":", "");
			
			var combined = Path.Combine(outputDirectory, filePath);
			combined = Path.GetFullPath(combined);
			if (!combined.Contains(outputDirectory)) throw new SecurityException("The combined path is outside the output directory");

			return combined;
		}

		/// <summary>
		/// Remove relative parts (/./ or /../) of a <paramref name="path"/>
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string PathRemoveRelativeParts(string path) {
			if (path == null) throw new ArgumentNullException(nameof(path));

			var split = path.Split('/', '\\');
			var filtered = split.Where((part) => part != ".." && part != ".");
			return string.Join(Path.DirectorySeparatorChar.ToString(), filtered);
		}

		/// <summary>
		/// Test whether a path points to an existing file or directory.
		/// </summary>
		/// <param name="path">The path to test</param>
		/// <returns></returns>
		public static bool PathExists(string path) {
			return File.Exists(path) || Directory.Exists(path);
		}

		/// <summary>
		/// Return the directory part of a given path.<br/><br/>
		/// Similar to <see cref="Path.GetDirectoryName(string)"/>, but works correctly if the path is an existing directory.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string PathGetDirectory(string path) {
			path = Path.GetFullPath(path);
			return IsDirectory(path)? path: Path.GetDirectoryName(path);
		}

		/// <summary>
		/// Append a path separator character to a directory path if necessary
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string PathAppendSeparator(string path) {
			if (path == null) return null;
			if (path.EndsWith(Path.DirectorySeparatorChar.ToString()) || !IsDirectory(path)) return path;

			return path + Path.DirectorySeparatorChar;
		}

		/// <summary>
		/// Remove path separator characters from the beginning of the <paramref name="path"/>
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string PathRemoveLeadingSeparator(string path) {
			if (path == null) return null;
			return path.TrimStart(new char[] { Path.DirectorySeparatorChar });
		}

		/// <summary>
		/// Remove path separator characters from the end of the <paramref name="path"/>
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string PathRemoveTrailingSeparator(string path) {
			if (path == null) return null;
			return path.TrimEnd(new char[] { Path.DirectorySeparatorChar });
		}

		/// <summary>
		/// Create a new or overwrite an existing file.
		/// Creates the directory structure if it doesn't exist.
		/// </summary>
		/// <param name="path">The file path</param>
		/// <returns></returns>
		public static FileStream FileCreate(string path) {
			CreateDirectoryStructure(path);
			return FileOpen(path, FileMode.Create);
		}

		/// <summary>
		/// Open a file and handle exceptions
		/// </summary>
		/// <param name="path"></param>
		/// <param name="fileMode"></param>
		/// <param name="fileAccess"></param>
		/// <returns></returns>
		public static FileStream FileOpen(string path, FileMode fileMode, FileAccess fileAccess = FileAccess.ReadWrite) {
			try {
				return File.Open(path, fileMode, fileAccess);
			}
			catch (FileNotFoundException) {
				Error("The input file does not exist", EXITCODE.IO_ERROR);
			}
			catch (Exception e) {
				Error("Failed to read input file: " + e.Message, EXITCODE.IO_ERROR);
			}

			return null;
		}

		/// <summary>
		/// Move a file from <paramref name="from"/> to <paramref name="to"/> making sure the paths are valid.
		/// If <paramref name="promptId"/> is not null, an overwrite prompt is displayed if <paramref name="to"/> already exists.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="promptId"></param>
		/// <returns>True if the operation succeeded, otherwise false</returns>
		public static bool FileMove(string from, string to, string promptId = null) {
			if (!File.Exists(from)) return false;
			to = PathReplaceInvalidChars(to);

			//Debug($"Moving {from} to {to}");
			if (promptId != null) {
				to = EnsureFileDoesNotExist(to, promptId);
				if (to == null) return false;
			}

			CreateDirectoryStructure(to);

			try {
				File.Delete(to);
				File.Move(from, to);
			}
			catch (Exception e) {
				Warn("Failed to move file: " + e.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Delete a file and handle exceptions
		/// </summary>
		/// <param name="path">The path to the file to delete</param>
		/// <returns>True on success, else false</returns>
		public static bool FileDelete(string path) {
			if (!File.Exists(path)) return true;

			try {
				File.Delete(path);
				return true;
			}
			catch (Exception e) {
				Error($"Failed to delete file {Path.GetFileName(path)}: {e}", EXITCODE.NONE);
				return false;
			}
		}

		/// <summary>
		/// Shortcut function to set creation, last access and modification time of a file 
		/// </summary>
		/// <param name="filePath">The file path</param>
		/// <param name="creationTime">Creation time to set</param>
		/// <param name="accessTime">Access time to set</param>
		/// <param name="writeTime">Write time to set</param>
		/// <returns>True if the operation succeeded, false if the file does not exist</returns>
		public static bool FileSetTimes(string filePath, DateTime? creationTime = null, DateTime? accessTime = null, DateTime? writeTime = null) {
			if (!File.Exists(filePath)) return false;

			if (creationTime != null) File.SetCreationTime(filePath, (DateTime) creationTime);
			if (accessTime != null) File.SetLastAccessTime(filePath, (DateTime) accessTime);
			if (writeTime != null) File.SetLastWriteTime(filePath, (DateTime) writeTime);

			return true;
		}

		/// <summary>
		/// Search a relative path in <paramref name="basePath"/> or its parents.
		/// </summary>
		/// <param name="basePath">The path to search in</param>
		/// <param name="relativePath">The path to search for</param>
		/// <param name="searchDepth">Determines how many levels to go up</param>
		/// <returns>The full path if found, otherwise null</returns>
		public static string FileFindBySubDirectory(string basePath, string relativePath, int searchDepth = 3) {
			var directory = PathGetDirectory(basePath);

			for (var i = 0; i < searchDepth; i++) {
				var combinedPath = Path.Combine(directory, relativePath);
				Debug($"Searching {relativePath} in {combinedPath}");

				if (File.Exists(combinedPath)) return Path.GetFullPath(combinedPath);

				directory = Path.Combine(directory, "..");
			}

			return null;
		}

		/// <summary>
		/// Creates the directory structure for a <paramref name="path"/>
		/// </summary>
		/// <param name="path">The path to create the directory structure for</param>
		public static void CreateDirectoryStructure(string path) {
			Directory.CreateDirectory(PathGetDirectory(path));
		}

		/// <summary>
		/// Test whether a path is a directory or not <br/><br/>
		/// Warning: The path to test <b>must exist</b>!<br/>
		/// This function accesses the file system and therefore might have an impact on performance if used extensively.
		/// </summary>
		/// <param name="path">The path to test</param>
		/// <returns></returns>
		public static bool IsDirectory(string path) {
			try {
				return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
			}
			catch (Exception) {
				return false;
			}
		}

		/// <summary>
		/// Returns is a directory is empty or does not exist
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool DirectoryIsEmpty(string path) {
			if (!Directory.Exists(path)) return true;

			IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
			using (IEnumerator<string> en = items.GetEnumerator()) {
				return !en.MoveNext();
			}
		}

		/// <summary>
		/// Print a standard command line program header
		/// </summary>
		/// <param name="name">The name of the program</param>
		/// <param name="version">The program version</param>
		/// <param name="year">Development year(s)</param>
		/// <param name="description">Short description of the basic program functionality</param>
		/// <param name="usage">Program usage information</param>
		/// <param name="license">Program license</param>
		public static void Header(string name, string version, string year, string description = "", string usage = "", string license = "BSD 3-Clause") {
			var header = $"{name} by Bioruebe (https://bioruebe.com), {year}, Version {version}, Released under a {license} style license\n\n{description}";
			if (usage != null) header += "\n\nUsage: " + GetProgramName() + " " + usage;
			
			Console.WriteLine(header + "\n" + SEPARATOR);
		}

		/// <summary>
		/// Convenience function to check if the command line arguments contain one of the valid help switches -h, --help, /?, -?, and /h
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static bool HasCommandlineSwitchHelp(string[] args) {
			return args.Contains("-h") || args.Contains("--help") || args.Contains("/?") || args.Contains("-?") || args.Contains("/h");
		}

		/// <summary>
		/// Print a seperator line
		/// </summary>
		public static void Separator() {
			Console.WriteLine(SEPARATOR + "\n");
		}

		/// <summary>
		/// Return the name of the currently running program
		/// </summary>
		/// <returns></returns>
		public static string GetProgramName() {
			return Path.GetFileNameWithoutExtension(GetProgramPath());
		}

		/// <summary>
		/// Return the path of the currently running program
		/// </summary>
		/// <returns></returns>
		public static string GetProgramPath() {
			return System.Reflection.Assembly.GetEntryAssembly().Location;
		}

		/// <summary>
		/// Print a prompt message, wait for valid user input and return the selected option.
		/// Keeps an internal settings object to allow options like 'always' or 'never'.
		/// </summary>
		/// <param name="message">The message to print</param>
		/// <param name="promptId">
		///		A unique ID for this prompt, e.g. when the user selects 'always',<br/>subsequent calls 
		///		to <see cref="Prompt(string, string, PromptOptions)"/> with the same ID automatically return true
		///	</param>
		/// <param name="promptOptions">A <see cref="PromptOptions"/> object</param>
		/// <returns></returns>
		public static object Prompt(string message, string promptId = "", PromptOptions promptOptions = null) {
			if (promptOptions == null) {
				promptOptions = new PromptOptions(new List<PromptOption>() {
					new PromptOption("yes", () => true),
					new PromptOption("no", () => false),
					new PromptOption("always", () => {
						promptSettings.Add(promptId, 'y');
						return true;
					}),
					new PromptOption("never", () => {
						promptSettings.Add(promptId, 'n');
						return false;
					})
				}, 'n');
			}

			// Check setting from previous function calls
			promptSettings.TryGetValue(promptId, out var setting);

			char input;
			object result = PromptOption.NONE;
			if (setting == PromptInput.NULL_CHAR) {
				Cout($"{message} {promptOptions}");
			}
			else {
				result = promptOptions.Select(setting);
			}

			while (result == PromptOption.NONE) {
				input = Console.ReadKey().KeyChar;
				Console.WriteLine();
				result = promptOptions.Select(input);
			}

			return result;
		}

		/// <summary>
		/// Print a simple progress message, e.g.
		/// <code>[1/10] Processing file file1.txt</code>
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="current"></param>
		/// <param name="total"></param>
		public static void Progress(string msg, int current, int total) {
			Cout($"[{current}/{total}] {msg}");
		}

		/// <summary>
		/// Print a simple progress message, e.g.
		/// <code>[1/10] Processing file file1.txt</code><br/>
		/// This function does not add a newline character at the end,
		/// to allow appending additional information later.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="current"></param>
		/// <param name="total"></param>
		public static void ProgressPartial(string msg, int current, int total) {
			Cout($"[{current}/{total}] {msg}", LOG_SEVERITY.MESSAGE, false);
		}

		/// <summary>
		/// Print a simple progress message, e.g.
		/// <code>[1/10] Processing file file1.txt</code>
		/// This function saves the last <paramref name="current"/> value and does not print anything for subsequent calls
		/// with the same <paramref name="current"/> value.<br/><see cref="ProgressWithoutDuplicatesReset"/> must be called
		/// after finishing to ensure the next call does output a message.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="current"></param>
		/// <param name="total"></param>
		public static void ProgressWithoutDuplicates(string msg, int current, int total) {
			//Cout(current + "\t" + lastProgress);
			if (current == lastProgress) return;

			lastProgress = current;

			Cout($"[{current}/{total}] {msg}");
		}

		/// <summary>
		/// Reset the last progress value. See <seealso cref="ProgressWithoutDuplicates(string, int, int)"/>
		/// </summary>
		public static void ProgressWithoutDuplicatesReset() {
			lastProgress = -1;
		}

		/// <summary>
		/// Convert a byte array to a formatted string
		/// </summary>
		/// <param name="array">The array to format</param>
		/// <param name="endIndex">An optional index to stop at</param>
		/// <param name="formatString">The format string to use for each number</param>
		/// <param name="formatStringOffset">The format string to use for the offset</param>
		/// <param name="valuesPerLine">The number of values per line</param>
		/// <param name="separatorPosition">The position at which a separator (space) should be put</param>
		/// <returns></returns>
		private static string FormatNumbers(byte[] array, int endIndex = -1, string formatString = "", string formatStringOffset = "", int valuesPerLine = 16, int separatorPosition = 8) {
			if (array.Length < 1) return null;

			var output = new StringBuilder();
			endIndex = endIndex < 0? array.Length: Clamp(endIndex, 0, array.Length);

			for (int i = 0; i < endIndex; i += valuesPerLine) {
				output.Append(i.ToString(formatStringOffset) + " ");
				FormatNumbersLine(output, array, i, valuesPerLine, formatString, separatorPosition);
			}

			return output.ToString();
		}

		private static void FormatNumbersLine(StringBuilder output, byte[] array, int start, int count, string formatString, int separatorPosition) {
			var end = start + count;

			for (int i = start; i < end; i ++) {
				if (i % separatorPosition == 0) output.Append("  ");
				output.Append(" " + array[i].ToString(formatString));
			}

			output.Append("   ");
			byte b;
			for (int i = start; i < end; i++) {
				b = array[i];
				output.Append(b < 31 || b >= 127? '.': Convert.ToChar(b));
			}

			output.AppendLine();
		}

		/// <summary>
		/// Print an array of numbers in a format that fits numeric values better than the generic <see cref="Cout(IEnumerable, LOG_SEVERITY)"/>
		/// </summary>
		/// <param name="array">The array to print</param>
		/// <param name="endIndex">The amount of values to print</param>
		/// <param name="formatString">The format string to pass to <see cref="byte.ToString(string)"/> for each number</param>
		/// <param name="formatStringOffset">The format string to pass to <see cref="byte.ToString(string)"/> for the offset at the beginning of each line</param>
		/// <param name="valuesPerLine">The maximum amount of numbers per line</param>
		/// <param name="separatorPosition">Position at which a larger gap should be inserted</param>
		/// <param name="logSeverity">The <see cref="LOG_SEVERITY"/> for the output</param>
		public static void PrintNumbers(byte[] array, int endIndex = -1, string formatString = "", string formatStringOffset = "", int valuesPerLine = 16, int separatorPosition = 8, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE) {
			var formatted = FormatNumbers(array, endIndex, formatString, formatStringOffset, valuesPerLine, separatorPosition);
			
			if (formatted == null) {
				Cout();
				Cout("<empty>\n");
				return;
			}

			Cout(formatted, logSeverity);
			Cout();
		}

		/// <summary>
		/// Print the current time to stdout
		/// </summary>
		/// <param name="formatString"></param>
		public static void PrintTime(string formatString = "yyyy-MM-dd HH:mm:ss:fff") {
			var msg = DateTime.Now.ToString(formatString) + " ";
			Console.Write(msg);

			if (CoutKeepLog) logStringBuilder.Append(msg);
		}

		/// <summary>
		/// Print a byte array in the form of a hex dump
		/// </summary>
		/// <param name="array"></param>
		/// <param name="endIndex"></param>
		/// <param name="logSeverity"></param>
		public static void HexDump(byte[] array, int endIndex = -1, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE) {
			PrintNumbers(array, endIndex, "X2", "X4", 16, 8, logSeverity);
		}

		/// <summary>
		/// Print a stream's content in the form of a hex dump
		/// </summary>
		/// <param name="stream">The stream to dump</param>
		/// <param name="bytesToDump">The amount of bytes to dump. The actual output might be less than this number, if the streams end was reached.</param>
		/// <param name="logSeverity"></param>
		public static void HexDump(Stream stream, int bytesToDump, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE) {
			var bytesLeft = stream.Length - stream.Position;
			bytesToDump = Clamp(bytesToDump, 0, (int) bytesLeft);
			byte[] buffer = new byte[bytesToDump];
			stream.KeepPosition(() => stream.Read(buffer, 0, bytesToDump));
			HexDump(buffer, -1, logSeverity);
			if (stream.IsAtEnd()) Cout("-- end of stream reached\n");
		}

		/// <summary>
		/// Print a message to stdout (and stderr depending on the <paramref name="logSeverity"/>)
		/// </summary>
		/// <param name="msg">The message to print</param>
		/// <param name="logSeverity">Affects how the message will be displayed. Refer to the <see cref="LOG_SEVERITY"/> documentation.</param>
		/// <param name="appendNewLine">If enabled, a newline character is appended to the message.</param>
		public static void Cout(string msg, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE, bool appendNewLine = true) {
#if !DEBUG
			if (logSeverity == LOG_SEVERITY.DEBUG) return;
#endif
			if (msg == null) {
				msg = "null";
			}
			else if (msg.StartsWith("\n")) {
				Console.WriteLine();
				msg = msg.Substring(1);
			}

			if (logSeverity != LOG_SEVERITY.MESSAGE) msg = $"[{logSeverity}] {msg}";
			if (CoutPrintTime) PrintTime();
			if (appendNewLine) msg += "\n";
			if (CoutKeepLog) logStringBuilder.Append(msg);

			switch (logSeverity) {
				case LOG_SEVERITY.ERROR:
				case LOG_SEVERITY.CRITICAL:
					//Console.Error.WriteLine(msg);
					Console.Write(msg);
					break;
				default:
					Console.Write(msg);
					break;
			}
		}

		/// <summary>
		/// Print the string representation of each object in an enumerable to stdout along with its index
		/// </summary>
		/// <param name="enumerable"></param>
		/// <param name="logSeverity">Affects how the message will be displayed. Refer to the <see cref="LOG_SEVERITY"/> documentation.</param>
		public static void Cout(IEnumerable enumerable, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE) {
			var i = 0;
			foreach (var item in enumerable) {
				Cout($"\t[{i++}] " + item, logSeverity);
			}
		}

		/// <summary>
		/// Pretty print a byte array.
		/// Convenience function, which calls <see cref="HexDump(byte[], int, LOG_SEVERITY)"/>
		/// </summary>
		/// <param name="array"></param>
		/// <param name="bytesToDump">The amount of bytes to print</param>
		/// <param name="logSeverity">Affects how the message will be displayed. Refer to the <see cref="LOG_SEVERITY"/> documentation.</param>
		public static void Cout(byte[] array, int bytesToDump = 256, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE) {
#if !DEBUG
			if (logSeverity == LOG_SEVERITY.DEBUG) return;
#endif

			HexDump(array, bytesToDump, logSeverity);
		}

		/// <summary>
		/// Pretty print a dictionary's content.
		/// </summary>
		/// <param name="dictionary"></param>
		public static void Cout(IDictionary dictionary) {
			foreach (var key in dictionary.Keys) {
				Cout($"\t{key} = {dictionary[key]}");
			}
		}

		/// <summary>
		/// Pretty print a stream's content.
		/// Convenience function, which calls <see cref="HexDump(byte[], int, LOG_SEVERITY)"/>
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="bytesToDump">The amount of bytes to print</param>
		/// <param name="logSeverity">Affects how the message will be displayed. Refer to the <see cref="LOG_SEVERITY"/> documentation.</param>
		public static void Cout(Stream stream, int bytesToDump = 256, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE) {
#if !DEBUG
			if (logSeverity == LOG_SEVERITY.DEBUG) return;
#endif

			Cout(stream + " @ " + stream.Position + ":", logSeverity);

			HexDump(stream, bytesToDump, logSeverity);
		}

		/// <summary>
		/// Prints an empty line to stdout
		/// </summary>
		public static void Cout() {
			Console.WriteLine();
		}

		/// <summary>
		/// Print the string representation of an object to stdout. See <see cref="Cout(string, LOG_SEVERITY, bool)"/>.
		/// </summary>
		/// <param name="msg">The message to print</param>
		/// <param name="logSeverity">Affects how the message will be displayed. Refer to the <see cref="LOG_SEVERITY"/> documentation.</param>
		/// <param name="appendNewLine">If enabled, a newline character is appended to the message.</param>
		public static void Cout(object msg, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE, bool appendNewLine = true) {
			Cout(msg == null? "null": msg.ToString(), logSeverity, appendNewLine);
		}

		/// <summary>
		/// Prints the current time to stdout. Alias for <see cref="PrintTime(string)"/>/>
		/// </summary>
		public static void Tout() {
			PrintTime();
		}

		/// <summary>
		/// Prints the current time and <paramref name="msg"/> to stdout. See <see cref="Bio.Cout(string, LOG_SEVERITY, bool)"/>
		/// </summary>
		/// <param name="msg">The message to print</param>
		/// <param name="logSeverity">Affects how the message will be displayed. Refer to the <see cref="LOG_SEVERITY"/> documentation.</param>
		/// <param name="appendNewLine">If enabled, a newline character is appended to the message.</param>
		public static void Tout(object msg, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE, bool appendNewLine = true) {
			PrintTime();
			Cout(msg, logSeverity, appendNewLine);
		}

		/// <summary>
		/// Print a newline.
		/// </summary>
		public static void Debug() {
#if DEBUG
			Console.WriteLine();
#endif
		}

		/// <summary>
		/// Print a debug message.
		/// This is a convenience method to be used instead of <see cref="Cout(object, LOG_SEVERITY, bool)"/> with severity <see cref="LOG_SEVERITY.DEBUG"/>
		/// </summary>
		/// <param name="msg">The message to print</param>
		public static void Debug(object msg) {
			Cout(msg, LOG_SEVERITY.DEBUG);
		}

		/// <summary>
		/// Print a debug message.
		/// This is a convenience method to be used instead of <see cref="Cout(object, LOG_SEVERITY, bool)"/> with severity <see cref="LOG_SEVERITY.DEBUG"/>
		/// </summary>
		/// <param name="msg">The message(s) to print</param>
		public static void Debug(params object[] msg) {
			Cout(string.Join(" ", msg), LOG_SEVERITY.DEBUG);
		}

		/// <summary>
		/// Print a debug message.
		/// This is a convenience method to be used instead of <see cref="Cout(object, LOG_SEVERITY, bool)"/> with severity <see cref="LOG_SEVERITY.DEBUG"/>
		/// </summary>
		/// <param name="bytes">The byte array to print</param>
		/// <param name="bytesToDump">The number of bytes to dump</param>
		public static void Debug(byte[] bytes, int bytesToDump = 256) {
			Cout(bytes, bytesToDump, LOG_SEVERITY.DEBUG);
		}

		/// <summary>
		/// Print a debug message.
		/// This is a convenience method to be used instead of <see cref="Cout(object, LOG_SEVERITY, bool)"/> with severity <see cref="LOG_SEVERITY.DEBUG"/>
		/// </summary>
		/// <param name="dictionary"></param>
		public static void Debug(IDictionary dictionary) {
			Cout(dictionary);
		}

		/// <summary>
		/// Print a warning message.
		/// This is a convenience method to be used instead of <see cref="Cout(object, LOG_SEVERITY, bool)"/> with severity <see cref="LOG_SEVERITY.WARNING"/>
		/// </summary>
		/// <param name="msg">The message to print</param>
		public static void Warn(object msg) {
			Cout(msg, LOG_SEVERITY.WARNING);
		}

		/// <summary>
		/// Print an error message and exit if an exit code is specified
		/// </summary>
		/// <param name="msg">The message to print</param>
		/// <param name="exitCode"></param>
		public static void Error(object msg, EXITCODE exitCode = EXITCODE.NONE) {
			Error(msg, (int) exitCode);
		}

		/// <summary>
		/// Print an error message and exit if an exit code is specified
		/// </summary>
		/// <param name="msg">The message to print</param>
		/// <param name="exitCode"></param>
		private static void Error(object msg, int exitCode = -1) {
			Cout(msg, LOG_SEVERITY.ERROR);
#if DEBUG
			Console.ReadKey(false);
#endif
			if (exitCode > -1) Environment.Exit(exitCode);
		}

		/// <summary>
		/// Returns a log of all messages printed by <see cref="Cout(object, LOG_SEVERITY, bool)"/>.<br/>
		/// Requires <see cref="CoutKeepLog"/> option set to true to work.
		/// </summary>
		/// <returns></returns>
		public static string CoutGetLog() {
			return logStringBuilder.ToString();
		}

		/// <summary>
		/// Delay program termination until a key is pressed.<br/>
		/// <b>This function will only work in the debug version of BioLib!</b>
		/// </summary>
		public static void Pause() {
#if DEBUG
			Console.ReadKey();
#endif
		}

		/// <summary>
		/// The severity of a log message
		/// </summary>
		public enum LOG_SEVERITY {
			/// <summary>
			/// Debug message. Will only be printed in the debug version of BioLib.
			/// </summary>
			DEBUG,
			/// <summary>
			/// Info message.
			/// </summary>
			INFO,
			/// <summary>
			/// Warning message.
			/// </summary>
			WARNING,
			/// <summary>
			/// Error message. Will also print the message to stderr.
			/// </summary>
			ERROR,
			/// <summary>
			/// Unexpected errors. Will also print the message to stderr.
			/// </summary>
			CRITICAL,
			/// <summary>
			/// Normal console output. Will not add the log severity as message prefix.
			/// </summary>
			MESSAGE
		}

		/// <summary>
		/// Possible exit codes for a program
		/// </summary>
		public enum EXITCODE {
			/// <summary>
			/// Program does not exit
			/// </summary>
			NONE = -1,
			/// <summary>
			/// Program terminated without any errors
			/// </summary>
			SUCCESS,
			/// <summary>
			/// General input/output error, i.e. 'File not found' or 'Permission denied'
			/// </summary>
			IO_ERROR,
			/// <summary>
			/// The program received invalid input, e.g. a file with an unsupported type
			/// </summary>
			INVALID_INPUT,
			/// <summary>
			/// The user specified an invalid parameter, e.g. a non-existant command line switch or an out-of-bounds value
			/// </summary>
			INVALID_PARAMETER,
			/// <summary>
			/// The program does not support the action initialized by the user
			/// </summary>
			NOT_SUPPORTED,
			/// <summary>
			/// A required file could not be found
			/// </summary>
			MISSING_FILE,
			/// <summary>
			/// Generic error at runtime
			/// </summary>
			RUNTIME_ERROR,
			/// <summary>
			/// Interrupt signal received (Ctrl+C)
			/// </summary>
			SIGINT = 130
		}
	}
}
