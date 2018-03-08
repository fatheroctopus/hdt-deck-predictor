using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace DeckPredictor
{
	public class PredictionView
	{
		private PredictionLayout _layout = new PredictionLayout();
		private bool _lastHideOpponentCards;

		public PredictionView() {}

		public void SetEnabled(bool enabled)
		{
			if (enabled)
			{
				Log.Debug("Adding Layout to OverlayCanvas");
				Core.OverlayCanvas.Children.Add(_layout);
				Canvas.SetBottom(_layout, Core.OverlayWindow.Height * 20 / 100);
				Canvas.SetLeft(_layout, Core.OverlayWindow.Width * .5 / 100);
				_layout.Dispatcher.Invoke(() => _layout.Visibility = Visibility.Hidden);

				// Turn off the regular Opponent card list and restore the value later.
				_lastHideOpponentCards = Config.Instance.HideOpponentCards;
				Config.Instance.HideOpponentCards = true;
			}
			else
			{
				Log.Debug("Removing List View from OverlayCanvas");
				Core.OverlayCanvas.Children.Remove(_layout);
				Config.Instance.HideOpponentCards = _lastHideOpponentCards;
			}
		}

		public void OnPredictionUpdate(PredictionInfo prediction)
		{
			_layout.Dispatcher.Invoke(() =>
				{
					_layout.Update(prediction);
				});
		}
	}
}
