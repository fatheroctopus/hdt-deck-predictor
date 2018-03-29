using Hearthstone_Deck_Tracker.Utility;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;

namespace DeckPredictor
{
	class AutoUpdater
	{
		private const string GitHubUser = "fatheroctopus";
		private const string GitHubRepo = "hdt-deck-predictor";
		private static readonly string TempDirectory = Path.Combine(DeckPredictorPlugin.DataDirectory, "tmp");
		private static readonly string TempPluginDirectory = Path.Combine(TempDirectory, "DeckPredictor");

		public static async Task<bool> CheckAutoUpdate(Version currentVersion)
		{
			Log.Debug("Checking for new DeckPredictor version.");
			var release = await GitHub.CheckForUpdate(GitHubUser, GitHubRepo, currentVersion);
			if (release == null)
			{
				Log.Debug("DeckPredictor is up-to-date.");
				return false;
			}

			if (!Directory.Exists(TempDirectory))
			{
				Log.Debug("Creating temp directory: " + TempDirectory);
				Directory.CreateDirectory(TempDirectory);
			}

			try
			{
				Log.Info("Downloading new DeckPredictor Version: " + release.Tag);
				var path = await GitHub.DownloadRelease(release, TempDirectory);
				if (path == null)
				{
					Log.Error("Download failed, check hdt log for error.");
					return false;
				}
				Log.Info("Downloaded to: " + path);

				var zipFile = Path.Combine(TempDirectory, release.Assets[0].Name);
				Log.Info("Extracting " + zipFile + "...");
				ZipFile.ExtractToDirectory(zipFile, TempDirectory);

				Log.Info("Copying over new DeckPredictor...");
				Log.Debug("From: " + TempPluginDirectory + " To: " + DeckPredictorPlugin.PluginDirectory);
				// TODO
				// CopyFiles(TempPluginDirectory, DeckPredictorPlugin.PluginDirectory);
			}
			catch (Exception e)
			{
				Log.Error("Exception while installing update: " + e);
				return false;
			}
			finally
			{
				// Delete temp directory
				if (Directory.Exists(TempDirectory))
				{
					Directory.Delete(TempDirectory, true);
				}
			}

			return true;
		}

		private static void CopyFiles(string sourceDirName, string destDirName)
		{
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			// If the source directory does not exist, throw an exception.
			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			// If the destination directory does not exist, create it.
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}

			// Get the file contents of the directory to copy.
			FileInfo[] files = dir.GetFiles();

			foreach (FileInfo file in files)
			{
				// Create the path to the new copy of the file.
				string tempPath = Path.Combine(destDirName, file.Name);

				// Copy the file.
				file.CopyTo(tempPath, false);
			}

			// Copy the subdirectories.
			foreach (DirectoryInfo subdir in dirs)
			{
				// Create the subdirectory.
				string tempPath = Path.Combine(destDirName, subdir.Name);

				// Copy the subdirectories.
				CopyFiles(subdir.FullName, tempPath);
			}
		}
	}
}
