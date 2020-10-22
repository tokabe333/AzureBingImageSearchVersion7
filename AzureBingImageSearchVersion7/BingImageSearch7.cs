﻿using System;
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
				var que = new Queue<string>();
				var tasks = new List<Task<bool>>();
				foreach (var obj in jsonObj["value"]) {
					string url = obj["contentUrl"];
					string format = obj["encodingFormat"];

					// ダウンロードリスト作成
					tasks.Add(DownloadFileAsync(url, format, cnt));

					// 出力
					Console.SetCursorPosition(0, Console.WindowTop);
					Console.WriteLine("｜／―＼".Substring(cnt % 4, 1) + "  " + cnt + "/" + total + " term:" + this.SearchTerm);
					que.Enqueue(cnt + " " + format + " " + url);
					Console.WriteLine(string.Join("\n", que));
					cnt += 1;
				} //End_Foreach

				// 全部のダウンロードが終わるまで待つ
				var resultBools = await Task.WhenAll(tasks.ToArray());
				Console.WriteLine("†††バッチのダウンロード終了†††");

				// 次のオフセット
				Thread.Sleep(500);
				string next = jsonObj["nextOffset"];
				result = this.Search(int.Parse(next));
				jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(result.JsonResult);
				total = jsonObj["totalEstimatedMatches"];
			} //End_While

			return true;
		} //End_Method

		public SearchResult Search(int offset) {
			// クエリを生成してリクエストしてJSONレスポンスを得る
			var query = BingImageSearch7.UriBase + "?q=" + Uri.EscapeDataString(this.SearchTerm);
			var request = WebRequest.Create(query);
			request.Headers["Ocp-Apim-Subscription-Key"] = this.ApiKey;
			request.Headers["Pragma"] = "no cache";
			request.Headers["nextOffset"] = "" + offset;
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
			}catch(Exception e) {
				Console.WriteLine("Falied to Download File:" + url);
				return false;
			} // End_TryCacth
			return true;
		} //End_Method
	} //End_Class
} //End_Namespace

