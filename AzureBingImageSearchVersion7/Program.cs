using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBingImageSearchVersion7 {
	class Program {
		static void Main(string[] args) {
			var commandOption = CommandLine.Parser.Default.ParseArguments<Options>(args) as CommandLine.Parsed<Options>;

			var apikey = commandOption.Value.ApiKey;
			var dir = commandOption.Value.SaveDir;
			var term = commandOption.Value.SearchTerm;

			Do(apikey, dir, term).GetAwaiter().GetResult();
		} // End_Methods

		static async Task<bool> Do(string key, string dir, string term) {
			var bing = new BingImageSearch7(key, dir, term);
			return await bing.StartSearchAndDownload();
		} //End_Method
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
