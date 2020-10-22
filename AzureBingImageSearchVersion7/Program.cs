﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBingImageSearchVersion7 {
	class Program {
		static void Main(string[] args) {
			var apikey = args[0];
			var dir = args[1];
			var term = args[2];
			var bing = new BingImageSearch7(apikey, dir, term);
			bing.StartSearchAndDownload();
		} // End_Methods
	} // End_Class
} // End_Namespace
