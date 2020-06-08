using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BioLib;

namespace CommandLineTest {
	class Program {
		static void Main(string[] args) {
			Bio.Header("BioLib Command Line Test Program", "1.0.0", "2020", "");

			//TestOutputByteArray();
			//TestOverwritePrompt();

			Bio.Pause();
		}

		static void TestOutputByteArray() {
			var randomArray = Bio.RandomArray<byte>(256, 0, byte.MaxValue);
			Bio.Cout(randomArray);
		}

		static void TestOverwritePrompt() {
			var path = Bio.GetProgramPath();

			var renamed = Bio.EnsureFileDoesNotExist(path, "overwrite_prompt");
			Bio.Cout(renamed);
			
			renamed = Bio.EnsureFileDoesNotExist(path, "overwrite_prompt");
			Bio.Cout(renamed);
		}
	}
}
