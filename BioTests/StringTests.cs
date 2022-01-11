using Microsoft.VisualStudio.TestTools.UnitTesting;
using BioLib.Strings;

namespace BioTests {
	[TestClass]
	public class StringTests {

		[TestMethod]
		public void RemoveControlCharacters() {
			Assert.AreEqual("", "".RemoveControlCharacters());
			Assert.AreEqual("", "\0\0\0".RemoveControlCharacters());
			Assert.AreEqual("TestString", "Test\0\tString\n".RemoveControlCharacters());
		}
	}
}
