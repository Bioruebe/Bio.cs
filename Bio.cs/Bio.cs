using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BioLib {
	/// <summary>
	///  Main library
	/// </summary>
	public static class Bio {
		public const char DEFAULT_CHAR = '\u0000';
		public const char NULL_CHAR = '\0';
		public const char CR = '\r';
		public const string SEPERATOR = "\n---------------------------------------------------------------------";

		private static readonly Dictionary<string, char> promptSettings = new Dictionary<string, char>();

		/// <summary>
		/// Copy N <paramref name="bytes"/> from <paramref name="input"/> to <paramref name="output"/> stream.
		/// </summary>
		/// <param name="input">Input stream</param>
		/// <param name="output">Output stream</param>
		/// <param name="bytes">Amount of bytes to copy or -1 to copy all</param>
		/// <param name="keepPosition">If true, resets the input stream position after copying</param>
		/// <param name="bufferSize">The size of the internal buffer to use for copying</param>
		public static void CopyStream(Stream input, Stream output, int bytes = -1, bool keepPosition = true, int bufferSize = 1024) {
			var buffer = new byte[bufferSize];
			long initialPosition = 0;
			if (keepPosition) initialPosition = input.Position;
			int read;
			if (bytes < 1) bytes = (int)(input.Length - input.Position);

			while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(bufferSize, bytes))) > 0) {
				output.Write(buffer, 0, read);
				bytes -= read;
			}

			if (keepPosition) input.Seek(initialPosition, SeekOrigin.Begin);
		}

		/// <summary>
		/// Test if a byte array contains a specific <paramref name="pattern"/> at position <paramref name="pos"/> by comparing each byte.
		/// Useful to verify a file magic.
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
		/// Returns <paramref name="value"/> clamped to the inclusive range of <paramref name="min"/> and <paramref name="max"/>.
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
		/// Create an array of random numbers
		/// </summary>
		/// <typeparam name="T">A numeric data type</typeparam>
		/// <param name="arraySize">The amount of values to generate</param>
		/// <param name="min">Minimum value for each number (inclusive)</param>
		/// <param name="max">Maximum value for each number (inclusive)</param>
		/// <returns></returns>
		public static T[] RandomArray<T>(int arraySize, int min = 0, int max = int.MaxValue) {
			Random random = new Random();
			var array = new T[arraySize];

			for (int i = 0; i < arraySize; i++) {
				array[i] = (T) Convert.ChangeType(random.Next(min, max + 1), typeof(T));
			}

			return array;
		}

		/// <summary>
		/// Convenience function. Ensures a path is valid and does not exist.
		/// If the path already exists, a prompt is displayed asking the user to overwrite or rename.
		/// </summary>
		/// <param name="path">The path to test</param>
		/// <param name="promptId">A unique ID for the prompt, refer to <see cref="Prompt(string, string, PromptOptions)"/> for more information.</param>
		/// <returns>The file path depending on user choice or null indicating the file should not be overwritten.</returns>
		public static string EnsureFileDoesNotExist(string path, string promptId = "") {
			path = FileReplaceInvalidChars(path);
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
		/// This function ensures to only return a path, which does not already exist.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetRenamedFilePath(string path) {
			var directory = Path.GetDirectoryName(path);
			var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			var fileExtension = Path.GetExtension(path);
			
			var i = 2;
			do {
				path = Path.Combine(directory, string.Format($"{fileNameWithoutExtension} ({i}){fileExtension}"));
				i++;
			}
			while (File.Exists(path));

			return path;
		}

		/// <summary>
		/// Replace all invalid characters in a file name.
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="by"></param>
		/// <returns></returns>
		public static string FileReplaceInvalidChars(string filename, string by = "_") {
			return string.Join(by, filename.Split(Path.GetInvalidPathChars()));
		}

		/// <summary>
		/// Print a standard command line program header.
		/// </summary>
		/// <param name="name">The name of the program</param>
		/// <param name="version">The program version</param>
		/// <param name="year">Development year(s)</param>
		/// <param name="description">Short description of the basic program functionality</param>
		/// <param name="usage">Program usage information</param>
		/// <param name="license">Program license</param>
		public static void Header(string name, string version, string year, string description = "", string usage = "", string license = "BSD 3-Clause") {
			var header = string.Format($"{name} by Bioruebe (https://bioruebe.com), {year}, Version {version}, Released under a {license} style license\n\n{description}");
			if (usage != null) header += "\n\nUsage: " + GetProgramName() + " " + usage;
			
			Console.WriteLine(header + "\n" + SEPERATOR);
		}

		/// <summary>
		/// Convenience function to check if the command line arguments contain one of the valid help switches -h, --help, /?, -?, and /h.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static bool HasCommandlineSwitchHelp(string[] args) {
			return args.Contains("-h") || args.Contains("--help") || args.Contains("/?") || args.Contains("-?") || args.Contains("/h");
		}

		/// <summary>
		/// Print a seperator line.
		/// </summary>
		public static void Seperator() {
			Console.WriteLine(SEPERATOR + "\n");
		}

		/// <summary>
		/// Return the name of the currently running program.
		/// </summary>
		/// <returns></returns>
		public static string GetProgramName() {
			return Path.GetFileNameWithoutExtension(GetProgramPath());
		}

		/// <summary>
		/// Return the path of the currently running program.
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
			object result = new ArgumentException();
			if (setting == NULL_CHAR) {
				Cout($"{message} {promptOptions}");
			}
			else {
				result = promptOptions.Select(setting);
			}

			while (result is ArgumentException) {
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
			Cout(string.Format($"[{current}/{total}] {msg}"));
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
		public static void PrintNumbers(byte[] array, int endIndex = -1, string formatString = "", string formatStringOffset = "", uint valuesPerLine = 16, uint separatorPosition = 8, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE) {
			var output = "";
			endIndex = Clamp(endIndex, 0, array.Length);

			for (int i = 0; i < endIndex; i++) {
				if (i % valuesPerLine == 0) {
					Cout(output, logSeverity);
					var offset = i / valuesPerLine;
					output = offset.ToString(formatStringOffset) + "\t" + array[i].ToString(formatString);
				}
				else {
					if (i % separatorPosition == 0) output += "  ";
					output += " " + array[i].ToString(formatString);
				}
			}

			Cout(output, logSeverity);
			Cout();
		}

		/// <summary>
		/// Print a message to stdout (and stderr depending on the <paramref name="logSeverity"/>).
		/// </summary>
		/// <param name="msg">The message to print</param>
		/// <param name="logSeverity">Affects how the message will be displayed. Refer to the <see cref="LOG_SEVERITY"/> documentation.</param>
		public static void Cout(string msg, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE) {
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

			if (logSeverity != LOG_SEVERITY.MESSAGE) msg = string.Format("[{0}] {1}", logSeverity, msg);

			switch (logSeverity) {
				case LOG_SEVERITY.ERROR:
				case LOG_SEVERITY.CRITICAL:
					Console.Error.WriteLine();
					Console.WriteLine(msg);
					break;
				default:
					Console.WriteLine(msg);
					break;
			}
		}

		/// <summary>
		/// Print the string representation of each object in an enumerable to stdout along with its index.
		/// </summary>
		/// <param name="enumerable"></param>
		/// <param name="logSeverity"></param>
		public static void Cout(IEnumerable enumerable, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE) {
			var i = 0;
			foreach (var item in enumerable) {
				Cout($"\t[{i++}] " + item, logSeverity);
			}
		}

		/// <summary>
		/// Pretty print a byte array.
		/// Convenience function, which calls <see cref="PrintNumbers(byte[], int, string, string, uint, uint, LOG_SEVERITY)"/>
		/// </summary>
		/// <param name="array"></param>
		/// <param name="logSeverity"></param>
		public static void Cout(byte[] array, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE) {
			PrintNumbers(array, 256, "X2", "X4", 16, 8, logSeverity);
		}

		/// <summary>
		/// Print empty line to stdout.
		/// </summary>
		public static void Cout() {
			Console.WriteLine();
		}

		/// <summary>
		/// Print the string representation of an object to stdout. See <see cref="Cout(string, LOG_SEVERITY)"/>.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="logSeverity"></param>
		public static void Cout(object msg, LOG_SEVERITY logSeverity = LOG_SEVERITY.MESSAGE) {
			Cout(msg == null? "null": msg.ToString(), logSeverity);
		}

		/// <summary>
		/// Print debug message.
		/// This is a convenience method to be used instead of <see cref="Cout(object, LOG_SEVERITY)"/> with severity <see cref="LOG_SEVERITY.DEBUG"/>
		/// </summary>
		/// <param name="msg"></param>
		public static void Debug(object msg) {
			Cout(msg, LOG_SEVERITY.DEBUG);
		}

		/// <summary>
		/// Print warning message.
		/// This is a convenience method to be used instead of <see cref="Cout(object, LOG_SEVERITY)"/> with severity <see cref="LOG_SEVERITY.WARNING"/>
		/// </summary>
		/// <param name="msg"></param>
		public static void Warn(object msg) {
			Cout(msg, LOG_SEVERITY.WARNING);
		}

		/// <summary>
		/// Print error message and exit if an exit code is specified
		/// </summary>
		/// <param name="msg">The message to print</param>
		/// <param name="exitCode"></param>
		public static void Error(object msg, EXITCODE exitCode = EXITCODE.NONE) {
			Error(msg, (int) exitCode);
		}

		/// <summary>
		/// Print error message and exit if an exit code is specified
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="exitCode"></param>
		private static void Error(object msg, int exitCode = -1) {
			Cout(msg, LOG_SEVERITY.ERROR);
#if DEBUG
			Console.ReadKey(false);
#endif
			if (exitCode > -1) Environment.Exit(exitCode);
		}

		/// <summary>
		/// Delay program termination until a key is pressed.
		/// This function will only work in the debug version of BioLib!
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
