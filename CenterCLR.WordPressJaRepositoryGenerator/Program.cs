using System;
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
			if (Directory.Exists(basePath) == true)
			{
				Directory.Delete(basePath, true);
			}

			Repository.Init(basePath, false);

			using (var r = new Repository(basePath))
			{
				foreach (var url in urls)
				{
					await FetchAndExtractWordPressDistributionAsync(url, basePath).ConfigureAwait(false);

					r.Stage("*");
					r.Commit(url.ToString());

					Console.WriteLine(url.ToString());

					Clean(basePath);
				}
			}
		}

		static void Main(string[] args)
		{
			ExecuteAsync().Wait();
		}
	}
}
