using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Plugins;

namespace DeckPredictor
{
	public class DeckPredictorPlugin : IPlugin
	{
		public static readonly string DataDirectory = Path.Combine(Config.AppDataPath, "DeckPredictor");

		private PluginConfig _config;

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

			var metaRetriever = new MetaRetriever();
			var task = Task.Run<List<Deck>>(async () => await metaRetriever.RetrieveMetaDecks(_config));
			List<Deck> metaDecks = task.Result;
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