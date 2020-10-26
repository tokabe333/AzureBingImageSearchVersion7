using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;

namespace AzureBingImageSearchVersion7 {
	class BingImageSearch7 {
		private const string UriBase = "https://api.cognitive.microsoft.com/bing/v7.0/images/search";
		private string SearchTerm = null;
		private string ApiKey = null;
		private string SaveDir = null;
		private int BuffSize = 65536;
		private Queue<string> Que = null;
		private Queue<string> ErrQue = null;

		public struct SearchResult {
			public string JsonResult;
			public Dictionary<string, string> RelevantHeaders;
		} //End_Struct


		public BingImageSearch7(string apiKey, string saveDir, string searchTerm) {
			this.ApiKey = apiKey;
			this.SaveDir = saveDir;
			this.SearchTerm = searchTerm;

			// ディレクトリ作成
			Directory.CreateDirectory(this.SaveDir + "\\" + this.SearchTerm);
			this.SaveDir += "\\" + this.SearchTerm;
		} //End_Constructor

		public async Task<bool> StartSearchAndDownload() {
			int cnt = 1;

			// 検索結果を取得
			var result = this.Search(0);
			dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(result.JsonResult);
			int total = jsonObj["totalEstimatedMatches"];

			// 各画像URLに対してHTTPリクエストして保存する
			while (cnt < total) {
				this.Que = new Queue<string>();
				this.ErrQue = new Queue<string>();
				int startCnt = cnt;
				var tasks = new List<Task<bool>>();
				foreach (var obj in jsonObj["value"]) {
					string url = obj["contentUrl"];
					string format = obj["encodingFormat"];
					this.Que.Enqueue(cnt + " " + format + " " + url);

					// ダウンロードリスト作成
					tasks.Add(DownloadFileAsync(url, format, cnt));
					cnt += 1;
				} //End_Foreach


				// 全部のダウンロードが終わるまで待つ(待機中は表示して遊ぶ)
				bool DisplayProgress(CancellationToken token) {
					string ngo = string.Join("\n", this.Que);
					int displayCnt = 0;
					while (!token.IsCancellationRequested) {
						int finished = tasks.Count(unchi => unchi.IsCompleted);
						Console.SetCursorPosition(0, 0);
						Console.WriteLine("｜／―＼"[displayCnt] + "  " + (startCnt + finished) + "/" + total + " term:" + this.SearchTerm);
						Console.WriteLine(ngo);
						Console.ForegroundColor = ConsoleColor.Magenta;
						Console.WriteLine(string.Join("\n", this.ErrQue));
						Console.ForegroundColor = ConsoleColor.White;
						Thread.Sleep(300);
						displayCnt = displayCnt >= 3 ? 0 : displayCnt += 1;
					} // End_While
					return true;
				} // End_Method
				var tokenSource = new CancellationTokenSource();
				var cancelToken = tokenSource.Token;
				Console.Clear();
				var displayTask = Task.Run(() => DisplayProgress(cancelToken));
				var resultBools = await Task.WhenAll(tasks.ToArray());
				tokenSource.Cancel();
				Thread.Sleep(500);
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine("†††バッチのダウンロード終了†††");

				// 次のオフセット
				string next = jsonObj["nextOffset"];
				Console.WriteLine("NextOffset : " + next);
				result = this.Search(int.Parse(next));
				jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(result.JsonResult);
				total = jsonObj["totalEstimatedMatches"];
			} //End_While

			return true;
		} //End_Method

		public SearchResult Search(int offset) {
			// クエリを生成してリクエストしてJSONレスポンスを得る
			var query = BingImageSearch7.UriBase + "?q=" + Uri.EscapeDataString(this.SearchTerm) + "&offset=" + offset + "&count=" + 20;
			var request = WebRequest.Create(query);
			request.Headers["Ocp-Apim-Subscription-Key"] = this.ApiKey;
			request.Headers["Pragma"] = "no cache";
			//request.Headers["currentOffset"] = "" + offset;
			//request.Headers["offset"] = "" + offset;
			//request.Headers["count"] = "" + 50;
			var response = (HttpWebResponse)request.GetResponseAsync().Result;
			var json = new StreamReader(response.GetResponseStream()).ReadToEnd();

			// JSONを解析して返す
			SearchResult result = new SearchResult() {
				JsonResult = json,
				RelevantHeaders = new Dictionary<string, string>()
			};
			foreach (string header in response.Headers) {
				if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-")) {
					result.RelevantHeaders[header] = response.Headers[header];
				} // End_If
			} // End_Foreach

			return result;
		} // End_Method

		private async Task<bool> DownloadFileAsync(string url, string format, int cnt) {
			try {
				using (var wc = new WebClient()) {
					wc.Credentials = CredentialCache.DefaultNetworkCredentials;
					await wc.DownloadFileTaskAsync(new Uri(url), this.SaveDir + "\\" + cnt + "." + format);
					//wc.DownloadFileAsync(new System.Uri(url), this.SaveDir + "\\" + cnt + "." + format);
					//wc.DownloadFile(new System.Uri(url), this.SaveDir + "\\" + cnt + "." + format);
				} // End_Using
			} catch (Exception e) {
				this.ErrQue.Enqueue("Falied to Download...  No." + cnt + " URL:" + url);
				//Console.ForegroundColor = ConsoleColor.Magenta;
				//Console.WriteLine("Falied to Download...  No." + cnt + " URL:" + url);
				//Console.ForegroundColor = ConsoleColor.White;
				return false;
			} // End_TryCacth
			return true;
		} //End_Method

		private void ClearConsole() {
			Console.SetCursorPosition(0, 0);
			string str = "";
			for (int i = 0; i < 100; ++i) {
				for (int j = 0; j < 400; ++j) str += " ";
				str += "\n";
			} // End_For
			Console.WriteLine(str);
		} // End_Method
	} //End_Class
} //End_Namespace

