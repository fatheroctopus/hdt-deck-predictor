using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace DeckPredictor
{
	public class Log
	{
		private static readonly string LogDirectory = Path.Combine(DeckPredictorPlugin.DataDirectory, "Logs");
		private static readonly string LogPrefix = "log";
		private static readonly string LogSuffix = ".txt";
		private static readonly string LogFile = Path.Combine(LogDirectory, LogPrefix + LogSuffix);

		private const int MaxLogFileAge = 2;
		private const int KeepOldLogs = 5;
		private static int _duplicateCount;
		public static string PrevLine { get; private set; }
		public static bool Initialized { get; private set; }

		public static void Initialize()
		{
			if (Initialized)
				return;
			Trace.AutoFlush = true;
			if (!Directory.Exists(LogDirectory))
				Directory.CreateDirectory(LogDirectory);
			else
			{
				try
				{
					var fileInfo = new FileInfo(LogFile);
					if (fileInfo.Exists)
					{
						using (var fs = new FileStream(LogFile, FileMode.Open, FileAccess.Read, FileShare.None))
						{
							//can access log file => no other instance of same installation running
						}
						File.Move(LogFile, LogFile.Replace(LogSuffix, "_" + DateTime.Now.ToUnixTime() + LogSuffix));
						//keep logs from the last 2 days plus 5 before that
						foreach (var file in
							new DirectoryInfo(LogDirectory).GetFiles(LogPrefix + "*")
													 .Where(x => x.LastWriteTime < DateTime.Now.AddDays(-MaxLogFileAge))
													 .OrderByDescending(x => x.LastWriteTime)
													 .Skip(KeepOldLogs))
						{
							try
							{
								File.Delete(file.FullName);
							}
							catch
							{
							}
						}
					}
					else
						File.Create(LogFile).Dispose();
				}
				catch (Exception)
				{
					return;
				}
			}
			Initialized = true;
		}

		public static void WriteLine(string msg, LogType type, [CallerMemberName] string memberName = "",
									 [CallerFilePath] string sourceFilePath = "")
		{
#if (!DEBUG)
			if (type == LogType.Debug && Config.Instance.LogLevel == 0)
				return;
#endif
			var file = sourceFilePath?.Split('/', '\\').LastOrDefault()?.Split('.').FirstOrDefault();
			var line = $"{type}|{file}.{memberName} >> {msg}";

			if (line == PrevLine)
				_duplicateCount++;
			else
			{
				if (_duplicateCount > 0)
					Write($"... {_duplicateCount} duplicate messages");
				PrevLine = line;
				_duplicateCount = 0;
				Write(line);
			}
		}

		private static void Write(string line)
		{
			line = $"{DateTime.Now.ToLongTimeString()}|{line}";
			if (!Initialized)
			{
				return;
			}
			using (StreamWriter sw = new StreamWriter(LogFile, true))
			{
				sw.WriteLine(line);
			}
		}

		public static void Debug(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(msg, LogType.Debug, memberName, sourceFilePath);

		public static void Info(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(msg, LogType.Info, memberName, sourceFilePath);

		public static void Warn(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(msg, LogType.Warning, memberName, sourceFilePath);

		public static void Error(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(msg, LogType.Error, memberName, sourceFilePath);

		public static void Error(Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
			=> WriteLine(ex.ToString(), LogType.Error, memberName, sourceFilePath);
	}
}
