using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace DeckPredictor
{
	class MetaRetriever
	{
		// How many days we wait before updating the meta since the last download.
		private const double RecentDownloadTimeoutDays = 1;
		private const string MetaVersionUrl = "http://metastats.net/metadetector/metaversion.php";
		private const string MetaFileUrl = "https://s3.amazonaws.com/metadetector/metaDecks.xml.gz";
		private static readonly string MetaFilePath =
				Path.Combine(DeckPredictorPlugin.DataDirectory, @"metaDecks.xml");
		private static readonly string MetaArchivePath = MetaFilePath + ".gz";

		public async Task<List<Deck>> RetrieveMetaDecks(PluginConfig config)
		{
			// First check if we need to download the meta file.
			string newMetaVersion = "";
			if (!File.Exists(MetaFilePath))
			{
				Log.Info("No meta file found.");
				using (WebClient client = new WebClient())
				{
					newMetaVersion = await client.DownloadStringTaskAsync(MetaVersionUrl);
				}
			}
			else
			{
				double daysSinceLastDownload = (DateTime.Now - config.CurrentMetaFileDownloadTime).TotalDays;
				if (daysSinceLastDownload > RecentDownloadTimeoutDays)
				{
					Log.Info(daysSinceLastDownload +
							" days since meta file has been updated, checking for new version.");
					using (WebClient client = new WebClient())
					{
						newMetaVersion = await client.DownloadStringTaskAsync(MetaVersionUrl);
					}
					if (newMetaVersion.Trim() != "" && newMetaVersion != config.CurrentMetaFileVersion)
					{
						Log.Info("New version detected: " + newMetaVersion +
								", old version: " + config.CurrentMetaFileVersion);
					}
					else
					{
						Log.Debug("Newest version of meta file matches cached version: " + newMetaVersion);
						newMetaVersion = "";
					}
				}
				else
				{
					Log.Debug("Cached meta file is only " + daysSinceLastDownload + " days old.");
				}
			}

			if (newMetaVersion != "")
			{
				Log.Info("Downloading new meta file.");
				using (WebClient client = new WebClient())
				{
					await client.DownloadFileTaskAsync(MetaFileUrl, MetaArchivePath);
				}

				Log.Info("Meta file downloaded, unzipping...");
				FileInfo archiveFile = new FileInfo(MetaArchivePath);

				using (FileStream archiveFileStream = archiveFile.OpenRead())
				{
					using (FileStream unzippedFileStream = File.Create(MetaFilePath))
					{
						using (GZipStream unzipStream =
								new GZipStream(archiveFileStream, CompressionMode.Decompress))
						{
							unzipStream.CopyTo(unzippedFileStream);
						}
					}
				}

				config.CurrentMetaFileVersion = newMetaVersion;
				config.CurrentMetaFileDownloadTime = DateTime.Now;
				config.Save();
			}

			Log.Debug("Loading meta file");
			List<Deck> metaDecks = XmlManager<List<Deck>>.Load(MetaFilePath);
			Log.Info("Meta retrieved, " + metaDecks.Count + " decks loaded.");
			return metaDecks;
		}
	}
}
