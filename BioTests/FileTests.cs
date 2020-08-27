using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BioLib;

namespace BioTests {
	[TestClass]
	public class FileTests {
		private static string BASE_PATH = Path.GetFullPath(".");
		private const string PATH_TRAVERSAL_PATH = @".\..\..\..\file.txt";
		private const string RELATIVE_FILE = @".\file.txt";
		private const string ABSOLUTE_FILE = @"C:\file.txt";
		private const string INVALID_PATH = @"C:\<file>.txt";
		private const string INVALID_PATH_FILE_NAME = @"C:\file?.txt";
		private const string FILE_NAME = "file.txt";
		private string combinedPath = BASE_PATH + Path.DirectorySeparatorChar + FILE_NAME;

		[TestInitialize]
		public void InitializeTests() {
			
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PathRemoveRelativeParts_Null_ShouldThrowException() {
			Bio.PathRemoveRelativeParts(null);
		}

		[TestMethod]
		public void PathRemoveRelativeParts_EmptyString() {
			Assert.AreEqual("", Bio.PathRemoveRelativeParts(""));
			Assert.AreEqual("", Bio.PathRemoveRelativeParts(string.Empty));
		}

		[TestMethod]
		public void PathRemoveRelativeParts_AbsolutePath() {
			Assert.AreEqual(ABSOLUTE_FILE, Bio.PathRemoveRelativeParts(ABSOLUTE_FILE));
		}

		[TestMethod]
		public void PathRemoveRelativeParts_RelativePath_NoReplacementsNecessary() {
			Assert.AreEqual(FILE_NAME, Bio.PathRemoveRelativeParts(FILE_NAME));
		}

		[TestMethod]
		public void PathRemoveRelativeParts_RelativePath() {
			Assert.AreEqual(FILE_NAME, Bio.PathRemoveRelativeParts(RELATIVE_FILE));
		}

		[TestMethod]
		public void PathRemoveRelativeParts_RelativePath_PathTraversal() {
			Assert.AreEqual(FILE_NAME, Bio.PathRemoveRelativeParts(PATH_TRAVERSAL_PATH));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PathReplaceInvalidChars_NullArgument_ShouldThrowException() {
			Assert.AreEqual(null, Bio.PathReplaceInvalidChars(null));
		}

		[TestMethod]
		public void PathReplaceInvalidChars_NoInvalidChars_ShouldNotChange() {
			Assert.AreEqual(ABSOLUTE_FILE, Bio.PathReplaceInvalidChars(ABSOLUTE_FILE));
		}

		[TestMethod]
		public void PathReplaceInvalidChars_InvalidChars() {
			Assert.AreEqual(ABSOLUTE_FILE, Bio.PathReplaceInvalidChars(INVALID_PATH, ""));

			var expected = INVALID_PATH.Replace("<", "_").Replace(">", "_");
			Assert.AreEqual(expected, Bio.PathReplaceInvalidChars(INVALID_PATH));
		}

		[TestMethod]
		public void PathReplaceInvalidChars_InvalidFileName() {
			Assert.AreEqual(ABSOLUTE_FILE, Bio.PathReplaceInvalidChars(INVALID_PATH_FILE_NAME, ""));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetSafeOutputPath_Null_ShouldThrowException() {
			Bio.GetSafeOutputPath(null, FILE_NAME);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void GetSafeOutputPath_Empty_ShouldThrowException() {
			Bio.GetSafeOutputPath(BASE_PATH, "");
		}

		[TestMethod]
		public void GetSafeOutputPath_AbsolutePath() {
			var expected = Path.Combine(BASE_PATH, "C", FILE_NAME);
			Assert.AreEqual(expected, Bio.GetSafeOutputPath(BASE_PATH, ABSOLUTE_FILE));
		}

		[TestMethod]
		public void GetSafeOutputPath_PathTraversal() {
			Assert.AreEqual(combinedPath, Bio.GetSafeOutputPath(BASE_PATH, PATH_TRAVERSAL_PATH));
		}

		[TestMethod]
		public void GetSafeOutputPath_Valid() {
			Assert.AreEqual(combinedPath, Bio.GetSafeOutputPath(BASE_PATH, FILE_NAME));
		}
	}
}
