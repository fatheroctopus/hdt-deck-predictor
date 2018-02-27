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
	public class PredictionLog
	{
		private static readonly string LogDirectory = Path.Combine(DeckPredictorPlugin.DataDirectory, "Logs");
		private static readonly string LogFile = Path.Combine(LogDirectory, "prediction.txt");

		public PredictionLog()
		{
			if (!Directory.Exists(LogDirectory))
			{
				Directory.CreateDirectory(LogDirectory);
			}

			// Create prediction file if it doesn't exist.
			var fileInfo = new FileInfo(LogFile);
			if (!fileInfo.Exists)
			{
				Log.Info("Creating prediction.txt");
				File.Create(LogFile).Dispose();
			}
		}

		public void OnPredictionUpdate(Predictor predictor)
		{
			using (StreamWriter sw = new StreamWriter(LogFile, false))
			{
				sw.WriteLine(predictor.PossibleDecks.Count + " possible decks");
				sw.WriteLine(predictor.PredictedCards + " predicted cards");
				foreach (PredictedCardInfo predictedCard in predictor.PredictedCards)
				{
					sw.WriteLine(predictedCard.ToString());
				}
			}
		}
	}
}
