using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows;
using System;

namespace DeckPredictor
{
	public class DeckPredictorPlugin : IPlugin
	{
		public static readonly string DataDirectory = Path.Combine(Config.AppDataPath, "DeckPredictor");
		public static readonly string PluginDirectory =
			Path.Combine(Config.AppDataPath, "Plugins", "DeckPredictor");
		private static readonly string LogDirectory = Path.Combine(DataDirectory, "Logs");

		private PluginConfig _config;
		private ReadOnlyCollection<Deck> _metaDecks;
		private PredictionController _controller;
		private PredictionView _view;

		private SettingsWindow _settingsWindow;

		public string Author
		{
			get { return "fatheroctopus.bandcamp.com"; }
		}

		public string Description
		{
			get { return "Predicts the contents of the opponent's deck."; }
		}

		public System.Windows.Controls.MenuItem MenuItem
		{
			get { return null; }
		}

		public string Name
		{
			get { return "Deck Predictor"; }
		}

		public string ButtonText
		{
			get { return "Settings"; }
		}

		public void OnButtonPress()
		{
			if (_settingsWindow == null)
			{
				_settingsWindow = new SettingsWindow(_config);
				_settingsWindow.Closed += (sender, args) =>
				{
				    _settingsWindow = null;
				};
				_settingsWindow.Show();
			}
			else
			{
				_settingsWindow.Activate();
			}
		}

		public void OnLoad()
		{
			Log.Initialize();
			Log.Debug("Starting");
			if (!Directory.Exists(DataDirectory))
			{
				Directory.CreateDirectory(DataDirectory);
			}
			CustomLog.Initialize(LogDirectory);

			_config = PluginConfig.Load();

			// Synchronously check and install any updates to this plugin.
			var updaterTask = Task.Run<bool>(async () => await AutoUpdater.CheckAutoUpdate(Version));
			if (updaterTask.Result)
			{
				Log.Info("New version of Plugin installed, restart needed.");

				// Show a dialog to prompt a restart.
				string message =
					"DeckPredictorPlugin has been updated. Restart HDT for changes to take effect?";
				string caption = "DeckPredictorPlugin Updated";
				MessageBoxButtons buttons = MessageBoxButtons.YesNo;
				DialogResult result = System.Windows.Forms.MessageBox.Show(message, caption, buttons);
				if (result == System.Windows.Forms.DialogResult.Yes)
				{
					// Start the new process
					System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);

					// Shutdown the old process
					Hearthstone_Deck_Tracker.API.Core.MainWindow.Close();
					System.Windows.Application.Current.Shutdown();
					return;
				}
			}

			// Synchronously retrieve our meta decks and keep them in memory.
			var metaRetriever = new MetaRetriever();
			var retrieveTask =
				Task.Run<List<Deck>>(async () => await metaRetriever.RetrieveMetaDecks(_config));
			_metaDecks = new ReadOnlyCollection<Deck>(retrieveTask.Result);
			_view = new PredictionView(_config);

			GameEvents.OnGameStart.Add(() =>
				{
					var format = Hearthstone_Deck_Tracker.Core.Game.CurrentFormat;
					var mode = Hearthstone_Deck_Tracker.Core.Game.CurrentGameMode;
					if (format == Format.Standard &&
						(mode == GameMode.Ranked || mode == GameMode.Casual || mode == GameMode.Friendly))
					{
						Log.Info("Enabling DeckPredictor for " + format + " " + mode + " game");
						var opponent = new Opponent(Hearthstone_Deck_Tracker.Core.Game);
						_controller = new PredictionController(opponent, _metaDecks);
						_view.SetEnabled(true);
						_controller.OnPredictionUpdate.Add(_view.OnPredictionUpdate);
					}
					else
					{
						Log.Info("No deck predictions for " + format + " " + mode + " game");
					}
				});
			GameEvents.OnInMenu.Add(() =>
				{
					if (_controller != null)
					{
						_view.SetEnabled(false);
						Log.Debug("Disabling DeckPredictor for end of game");
					}
					_controller = null;
				});
			GameEvents.OnOpponentDraw.Add(() => _controller?.OnOpponentDraw());
			GameEvents.OnTurnStart.Add(activePlayer => _controller?.OnTurnStart(activePlayer));

			// Events that reveal cards need a 100ms delay. This is because HDT takes some extra
			// time to process all the tags we need, but it doesn't wait to send these callbacks.
			int delayMs = 100;
			GameEvents.OnOpponentPlay.Add(async card =>
				{
					await Task.Delay(delayMs);
					_controller?.OnOpponentPlay(card);
				});
			GameEvents.OnOpponentHandDiscard.Add(async card =>
				{
					await Task.Delay(delayMs);
					_controller?.OnOpponentHandDiscard(card);
				});
			GameEvents.OnOpponentDeckDiscard.Add(async card =>
				{
					await Task.Delay(delayMs);
					_controller?.OnOpponentDeckDiscard(card);
				});
			GameEvents.OnOpponentSecretTriggered.Add(async card =>
				{
					await Task.Delay(delayMs);
					_controller?.OnOpponentSecretTriggered(card);
				});
			GameEvents.OnOpponentJoustReveal.Add(async card =>
				{
					await Task.Delay(delayMs);
					_controller?.OnOpponentJoustReveal(card);
				});
			GameEvents.OnOpponentDeckToPlay.Add(async card =>
				{
					await Task.Delay(delayMs);
					_controller?.OnOpponentDeckToPlay(card);
				});
		}

		public void OnUnload()
		{
			if (_settingsWindow != null)
			{
			    if (_settingsWindow.IsVisible)
			    {
			        _settingsWindow.Close();
			    }
			    _settingsWindow = null;
			}
			_config.Save();
			_view.OnUnload();
		}

		public void OnUpdate()
		{
		}

		public Version Version
		{
			get { return new Version(1, 2, 1); }
		}
	}
}
