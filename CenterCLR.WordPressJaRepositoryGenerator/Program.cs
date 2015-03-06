////////////////////////////////////////////////////////////////////////////////////////////////////
//
// CenterCLR.WordPressJaRepositoryGenerator -
//   Auto create and maint WordPress-ja Git repository.
// Copyright (c) Kouji Matsui, All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
// INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
// EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using CenterCLR.Sgml;
using LibGit2Sharp;
using SharpCompress.Common;
using SharpCompress.Reader;
using SharpCompress.Reader.Zip;

namespace CenterCLR.WordPressJaRepositoryGenerator
{
	class Program
	{
		private static async Task<Uri[]> GetWordPressJaDistributionFileUrlsAsync()
		{
			using (var httpClient = new HttpClient())
			{
				using (var stream = await httpClient.GetStreamAsync("http://ja.wordpress.org/releases/").ConfigureAwait(false))
				{
					var document = SgmlReader.Parse(stream);

					XNamespace ns = "http://www.w3.org/1999/xhtml";
					var zipUrls =
						(from html in document.Elements(ns + "html")
						 from body in html.Elements(ns + "body")
						 from divWrapper in body.Elements(ns + "div")
						 let divWrapperClass = divWrapper.Attribute("class")
						 where (divWrapperClass != null) && (divWrapperClass.Value == "wrapper")
						 from divSectionReleases in divWrapper.Elements(ns + "div")
						 let divSectionReleasesClass = divSectionReleases.Attribute("class")
						 where (divSectionReleasesClass != null) && divSectionReleasesClass.Value.Contains("releases")
						 from tableReleasesLatest in divSectionReleases.Elements(ns + "table")
						 let tableReleasesLatestClass = tableReleasesLatest.Attribute("class")
						 where (tableReleasesLatestClass != null) && tableReleasesLatestClass.Value.Contains("releases")
						 from tr in tableReleasesLatest.Elements(ns + "tr")
						 from td in tr.Elements(ns + "td")
						 from a in td.Elements(ns + "a")
						 let href = a.Attribute("href")
						 where href != null
						 let hrefValue = href.Value.Trim()
						 where hrefValue.EndsWith(".zip")
						 select new Uri(hrefValue)).
						Distinct();

					return
						(from url in zipUrls
						 let fileName = Path.GetFileName(url.LocalPath)
						 let split0 = fileName.Split('-')
						 where split0.Length == 3
						 let split1 = split0[1].Split('.')
						 where split1.Length >= 2
						 let o0 = int.Parse(split1[0])
						 let o1 = int.Parse(split1[1])
						 let o2 = (split1.Length == 3) ? int.Parse(split1[2]) : 0
						 orderby o0, o1, o2
						 select url).
						ToArray();
				}
			}
		}

		private static async Task FetchAndExtractWordPressDistributionAsync(Uri url, string path)
		{
			using (var httpClient = new HttpClient())
			{
				using (var stream = await httpClient.GetStreamAsync(url).ConfigureAwait(false))
				{
					using (var zr = ZipReader.Open(stream))
					{
						zr.WriteAllToDirectory(path, ExtractOptions.ExtractFullPath);
					}
				}
			}
		}

		private static bool Clean(string basePath)
		{
			if (basePath.EndsWith(".git"))
			{
				return true;
			}

			var found = false;
			foreach (var folder in Directory.EnumerateDirectories(basePath, "*"))
			{
				if (Clean(folder) == true)
				{
					found = true;
				}
			}

			foreach (var file in Directory.EnumerateFiles(basePath, "*"))
			{
				File.Delete(file);
			}

			if (found == false)
			{
				Directory.Delete(basePath, true);
			}

			return found;
		}

		private static async Task ExecuteAsync()
		{
			var urls = await GetWordPressJaDistributionFileUrlsAsync().ConfigureAwait(false);

			var basePath = "WordPress-ja";
			if (Directory.Exists(basePath) == false)
			{
				Repository.Init(basePath, false);
			}

			using (var r = new Repository(basePath))
			{
				var tags = new HashSet<string>(r.Tags.Select(tag => tag.Name));

				foreach (var url in urls)
				{
					var versionString = string.Join("-", Path.GetFileNameWithoutExtension(url.LocalPath).Split('-').Skip(1));
					if (tags.Contains(versionString) == true)
					{
						Console.WriteLine(url.ToString() + " already fetched.");
						continue;
					}

					Clean(basePath);

					await FetchAndExtractWordPressDistributionAsync(url, basePath).ConfigureAwait(false);

					r.Stage("*");
					var commit = r.Commit(url.ToString());

					r.ApplyTag(versionString);

					Console.WriteLine(url.ToString() + " fetched.");
				}
			}
		}

		static void Main(string[] args)
		{
			Console.WriteLine("CenterCLR.WordPressJaRepositoryGenerator {0} - Auto create and maint WordPress-ja Git repository.",
				typeof(Program).Assembly.GetName().Version);
			Console.WriteLine("Copyright (c) Kouji Matsui, All rights reserved.");
			Console.WriteLine();

			Console.WriteLine("Fetching from WordPress-ja sites...");
			Console.WriteLine();

			ExecuteAsync().Wait();

			Console.WriteLine();

			Console.WriteLine("Done.");
		}
	}
}
