using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BioLib;
using System.Collections.Generic;

namespace BioTests {
	[TestClass]
	public class PromptTests {
		[TestMethod]
		public void Prompt_DefaultChoice() {
			var options = new PromptOptions(new List<PromptOption>() {
				new PromptOption("yes"),
				new PromptOption("no")
			}, 'n');

			Assert.AreEqual("[y]es | [N]o", options.ToString());
		}

		[TestMethod]
		public void Prompt_NoDefaultChoice() {
			var options = new PromptOptions(new List<PromptOption>() {
				new PromptOption("yes"),
				new PromptOption("no")
			});

			Assert.AreEqual("[y]es | [n]o", options.ToString());
			Assert.AreEqual(null, options.DefaultOption);
		}

		[TestMethod]
		public void Prompt_KeyNotPartOfName() {
			var options = new PromptOptions(new List<PromptOption>() {
				new PromptOption("ja", 'y'),
				new PromptOption("nein", 'n'),
			});

			Assert.AreEqual("ja [y] | [n]ein", options.ToString());
		}

		[TestMethod]
		public void Prompt_NameUpperCase() {
			var options = new PromptOptions(new List<PromptOption>() {
				new PromptOption("Yes", 'y'),
			});

			Assert.AreEqual("[y]es", options.ToString());
		}

		[TestMethod]
		public void Select_ReturnValue() {
			var options = new PromptOptions(new List<PromptOption>() {
				new PromptOption("yes", () => true)
			});

			Assert.AreEqual(true, options.Select('y'));
		}

		[TestMethod]
		public void Select_ReturnFunctionOutput() {
			var name = "Output";
			var options = new PromptOptions(new List<PromptOption>() {
				new PromptOption("upper case", () => name.ToUpper())
			});

			Assert.AreEqual("[u]pper case", options.ToString());
			Assert.AreEqual("OUTPUT", options.Select('u'));
		}

		[TestMethod]
		public void Select_DefaultChoice() {
			var options = new PromptOptions(new List<PromptOption>() {
				new PromptOption("yes", () => true),
				new PromptOption("no", () => false)
			}, 'n');

			Assert.AreEqual(false, options.Select(PromptInput.ENTER));
		}

		[TestMethod]
		public void Select_InvalidInput() {
			var options = new PromptOptions(new List<PromptOption>() {
				new PromptOption("yes", () => true)
			});

			Assert.AreEqual(PromptOption.NONE, options.Select('n'));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Error_DoubleKey() {
			new PromptOptions(new List<PromptOption>() {
				new PromptOption("no", 'n'),
				new PromptOption("never", 'n')
			});
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Error_NoKeyAvailable() {
			new PromptOptions(new List<PromptOption>() {
				new PromptOption("never"),
				new PromptOption("omit"),
				new PromptOption("no"),
			});
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void Error_InvalidDefaultKey() {
			new PromptOptions(new List<PromptOption>() {
				new PromptOption("yes"),
			}, 'n');
		}
	}
}
