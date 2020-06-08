using System;
using System.Collections.Generic;
using System.Linq;

namespace BioLib {
	/// <summary>
	/// A collection of <see cref="PromptOption"/>s to be displayed in a prompt
	/// </summary>
	public class PromptOptions {
		private Dictionary<char, PromptOption> options = new Dictionary<char, PromptOption>();
		
		/// <summary>
		/// The option to select if the user presses enter
		/// </summary>
		public PromptOption DefaultOption {
			get {
				return options.Values.FirstOrDefault((option) => option.isDefaultChoice);
			}
		}

		/// <summary>
		/// Create a new PromptOptions object from an <see cref="IEnumerable{T}"/> of <see cref="PromptOption"/>s
		/// </summary>
		/// <param name="promptOptions">The <see cref="PromptOption"/>s to display</param>
		/// <param name="defaultKey">The key used as default, if the user presses enter. An option with this key must exist!</param>
		public PromptOptions(IEnumerable<PromptOption> promptOptions, char defaultKey = '\0') {
			foreach (var option in promptOptions) {
				Add(option);
			}

			try {
				if (defaultKey != '\0') options[defaultKey].isDefaultChoice = true;
			}
			catch (KeyNotFoundException) {
				throw new ArgumentException("Invalid default key: no choice exists with key '" + defaultKey + "'. Valid keys are " + string.Join("|", options.Keys));
			}
		}

		/// <summary>
		/// Add a new <see cref="PromptOption"/>
		/// </summary>
		/// <param name="option">The option to add</param>
		public void Add(PromptOption option) {
			if (option.key == '\0') {
				foreach (var character in option.name) {
					if (options.ContainsKey(character)) continue;

					option.key = character;
					break;
				}

				if (option.key == '\0') throw new ArgumentException("Invalid prompt option " + option.name + ": no key specified and all characters of the name are already in use");
			}
			else if (options.ContainsKey(option.key)){
				throw new ArgumentException("Invalid prompt option " + option.name + ": key is already in use");
			}

			options.Add(option.key, option);
		}

		/// <summary>
		/// Selects the <see cref="PromptOption"/> with the key <paramref name="input"/>.
		/// Throws an <see cref="ArgumentException"/> if no option with the given key exists.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public object Select(char input) {
			PromptOption option;
			if (input == Bio.CR) {
				option = DefaultOption;
			}
			else {
				options.TryGetValue(input, out option);
			}

			if (option == null) throw new ArgumentException();

			return option.Select();
		}

		/// <summary>
		/// Return the string representation of the options
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return string.Join(" | ", options.Values);
		}
	}

	/// <summary>
	/// A single option used in a prompt.
	/// </summary>
	public class PromptOption {
		/// <summary>
		/// The name of the option
		/// </summary>
		public string name;
		/// <summary>
		/// The key to press to select the option
		/// </summary>
		public char key;
		/// <summary>
		/// True if this is the option to select if the user presses enter
		/// </summary>
		public bool isDefaultChoice = false;
		/// <summary>
		/// The function to execute when selected
		/// </summary>
		public Func<object> action;

		/// <summary>
		/// Create a new option
		/// </summary>
		/// <param name="name">The name of the option</param>
		public PromptOption(string name) {
			this.name = name;
		}

		/// <summary>
		/// Create a new option
		/// </summary>
		/// <param name="name">The name of the option</param>
		/// <param name="key">The key to select this option</param>
		public PromptOption(string name, char key) {
			this.name = name;
			this.key = key;
		}

		/// <summary>
		/// Create a new option
		/// </summary>
		/// <param name="name">The name of the option</param>
		/// <param name="action">A function to execute if the option is selected</param>
		public PromptOption(string name, Func<object> action) {
			this.name = name;
			this.action = action;
		}

		/// <summary>
		/// Create a new option
		/// </summary>
		/// <param name="name">The name of the option</param>
		/// <param name="action">A function to execute if the option is selected</param>
		/// <param name="key">The key to select this option</param>
		public PromptOption(string name, Func<object> action, char key) {
			this.name = name;
			this.action = action;
			this.key = key;
		}

		/// <summary>
		/// Select an option and execute the function bound to it
		/// </summary>
		/// <returns></returns>
		public object Select() {
			return action();
		}

		/// <summary>
		/// Return the string representation of the option
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			for (var i = 0; i < name.Length; i++) {
				if (char.ToLower(name[i]) != key) continue;

				return string.Format("{0}[{1}]{2}", name.Substring(0, i), isDefaultChoice? char.ToUpper(key): key, name.Substring(i + 1));
			}

			return name + $" [{key}]";
		}
	}
}
