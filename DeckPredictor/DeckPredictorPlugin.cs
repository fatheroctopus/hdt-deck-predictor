using System;
using System.IO;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Plugins;

namespace DeckPredictor
{
	public class DeckPredictorPlugin : IPlugin
	{
		private static string DataDirectory = Path.Combine(Config.AppDataPath, "DeckPredictor");
		private static string ConfigPath = Path.Combine(DataDirectory, "config.xml");

		private PluginConfig config_;

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
			if (!Directory.Exists(DataDirectory))
			{
				Directory.CreateDirectory(DataDirectory);
			}

			LoadConfig();
			SaveConfig();
		}

		public void OnUnload()
		{
		}

		public void OnUpdate()
		{
		}

		public Version Version
		{
			get { return new Version(0, 1, 1); }
		}

		private void LoadConfig()
		{
			if (File.Exists(ConfigPath))
			{
				config_ = PluginConfig.Load(new StreamReader(ConfigPath));
			}
			else
			{
				config_ = new PluginConfig();
			}
		}

		private void SaveConfig()
		{
			config_.Save(new StreamWriter(ConfigPath));
		}
	}
}