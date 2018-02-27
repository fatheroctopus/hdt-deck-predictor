using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System;

namespace DeckPredictor
{
	public class DeckPredictorPlugin : IPlugin
	{
		public static readonly string DataDirectory = Path.Combine(Config.AppDataPath, "DeckPredictor");

		private PluginConfig _config;
		private Predictor _predictor;
		private ReadOnlyCollection<Deck> _metaDecks;
		private PredictionLog _predictionLog;

		public string Author
		{
			get { return "gpitsch"; }
		}

		public string ButtonText
		{
			get { return "Settings"; }
		}

		public string Description
		{
			get { return "Predicts the contents of the opponent's deck."; }
		}

		public MenuItem MenuItem
		{
			get { return null; }
		}

		public string Name
		{
			get { return "Deck Predictor"; }
		}

		public void OnButtonPress()
		{
		}

		public void OnLoad()
		{
			Log.Initialize();
			Log.Debug("Starting");
			if (!Directory.Exists(DataDirectory))
			{
				Directory.CreateDirectory(DataDirectory);
			}

			_config = PluginConfig.Load();
			_predictionLog = new PredictionLog();

			// Synchronously retrieve our meta decks and keep them in memory.
			var metaRetriever = new MetaRetriever();
			var retrieveTask =
				Task.Run<List<Deck>>(async () => await metaRetriever.RetrieveMetaDecks(_config));
			_metaDecks = new ReadOnlyCollection<Deck>(retrieveTask.Result);

			GameEvents.OnGameStart.Add(() =>
				{
					Log.Debug("Creating a new Predictor");
					var opponent = new Opponent(Hearthstone_Deck_Tracker.Core.Game.Opponent);
					_predictor = new Predictor(opponent, _metaDecks);
					_predictor.OnPredictionUpdate.Add(_predictionLog.OnPredictionUpdate);
					_predictor.OnGameStart();
				});
			GameEvents.OnOpponentDraw.Add(() => _predictor.OnOpponentDraw());

			// Events that reveal cards need a 100ms delay. This is because HDT takes some extra
			// time to process all the tags we need, but it doesn't wait to send these callbacks.
			int delayMs = 100;
			GameEvents.OnOpponentPlay.Add(card =>
				Task.Delay(delayMs)
					.ContinueWith(_ => _predictor.OnOpponentPlay(card))
					.Start());
			GameEvents.OnOpponentHandDiscard.Add(card =>
				Task.Delay(delayMs)
					.ContinueWith(_ => _predictor.OnOpponentHandDiscard(card))
					.Start());
			GameEvents.OnOpponentDeckDiscard.Add(card =>
				Task.Delay(delayMs)
					.ContinueWith(_ => _predictor.OnOpponentDeckDiscard(card))
					.Start());
			GameEvents.OnOpponentSecretTriggered.Add(card =>
				Task.Delay(delayMs)
					.ContinueWith(_ => _predictor.OnOpponentSecretTriggered(card))
					.Start());
			GameEvents.OnOpponentJoustReveal.Add(card =>
				Task.Delay(delayMs)
					.ContinueWith(_ => _predictor.OnOpponentJoustReveal(card))
					.Start());
			GameEvents.OnOpponentDeckToPlay.Add(card =>
				Task.Delay(delayMs)
					.ContinueWith(_ => _predictor.OnOpponentDeckToPlay(card))
					.Start());
		}

		public void OnUnload()
		{
			_config.Save();
		}

		public void OnUpdate()
		{
		}

		public Version Version
		{
			get { return new Version(0, 1, 1); }
		}
	}
}
