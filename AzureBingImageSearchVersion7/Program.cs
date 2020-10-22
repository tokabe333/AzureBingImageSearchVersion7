using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBingImageSearchVersion7 {
	class Program {
		static void Main(string[] args) {
			var commandOption = CommandLine.Parser.Default.ParseArguments<Options>(args) as CommandLine.Parsed<Options>;

			var apikey = args[0];
			var dir = args[1];
			var term = args[2];
			var bing = new BingImageSearch7(apikey, dir, term);
			bing.StartSearchAndDownload();
		} // End_Methods
	} // End_Class

	class Options {
		[CommandLine.Option('k', "key", HelpText = "AzureBingImageSearchのAPIキー")]
		public string ApiKey { get; set; }

		[CommandLine.Option('d', "dir", HelpText = "保存先ディレクトリ(この直下に検索名でディレクトリ作成")]
		public string SaveDir { get; set; }

		[CommandLine.Option('t', "term", HelpText = "検索したい画像名")]
		public string SearchTerm { get; set; }
	} //End_Class
} // End_Namespace
