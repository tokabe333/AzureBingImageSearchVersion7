using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace AzureBingImageSearchVersion7 {
	class BingImageSearch7 {
		private const string UriBase = "https://api.cognitive.microsoft.com/bing/v7.0/images/search";
		private string SearchTerm = null;
		private string ApiKey = null;

		public struct SearchResult {
			public string JsonResult;
			public Dictionary<string, string> RelevantHeaders;
		} //End_Struct
		

		public BingImageSearch7(string apiKey, string searchTerm) {
			this.ApiKey = apiKey;
			this.SearchTerm = searchTerm;
		} //End_Constructor
		
		public bool StartSearchAndDownload() {
			return true;
		} //End_Method

		public SearchResult BingImageSearch() {
			// クエリを生成してリクエストしてJSONレスポンスを得る
			var query = BingImageSearch7.UriBase + "?q=" + Uri.EscapeDataString(this.SearchTerm);
			var request = WebRequest.Create(query);
			request.Headers["Ocp-Apim-Subscription-Key"] = this.ApiKey;
			var response = (HttpWebResponse)request.GetResponseAsync().Result;
			var json = new StreamReader(response.GetResponseStream()).ReadToEnd();

			// JSONを解析して返す
			SearchResult result = new SearchResult() {
				JsonResult = json,
				RelevantHeaders = new Dictionary<string, string>()
			};
			foreach(string header in response.Headers) {
				if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-")) {
					result.RelevantHeaders[header] = response.Headers[header];
				} // End_If
			} // End_Foreach

			return result;
		} //End_Method
		
	} //End_Class
} //End_Namespace

