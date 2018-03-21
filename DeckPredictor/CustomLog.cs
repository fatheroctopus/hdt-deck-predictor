using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System;

namespace DeckPredictor
{
	public class CustomLog
	{
		private static readonly string LogDirectory = Path.Combine(DeckPredictorPlugin.DataDirectory, "Logs");
		private static string _logDirectory;
		private string _logFile;

		public static void Initialize(string logDirectory)
		{
			_logDirectory = logDirectory;
			if (!Directory.Exists(LogDirectory))
			{
				Directory.CreateDirectory(LogDirectory);
			}
		}

		public interface ILogProvider
		{
			// Log providers implement this to define the contents of a log.
			// Triggered whenever CustomLog.Write is called.
			void OnWriteLog(TextWriter writer);
		}

		public CustomLog(string logFileName)
		{
			if (_logDirectory == null)
			{
				return;
			}

			_logFile = Path.Combine(_logDirectory, logFileName);
			// Create file if it doesn't exist.
			var fileInfo = new FileInfo(_logFile);
			if (!fileInfo.Exists)
			{
				Log.Info("Creating " + logFileName);
				File.Create(_logFile).Dispose();
			}
		}

		public void Write(ILogProvider provider)
		{
			if (_logDirectory == null)
			{
				return;
			}

			using (StreamWriter sw = new StreamWriter(_logFile, false))
			{
				provider.OnWriteLog(sw);
			}
		}
	}
}
