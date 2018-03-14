using HearthMirror;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace DeckPredictor
{
	public class PredictionView
	{
		private PredictionLayout _layout = new PredictionLayout();
		private bool _lastHideOpponentCards;
		private bool _enabled;
		private bool _firstUpdateReceived;
		private User32.MouseInput _mouseInput;

		public PredictionView()
		{
			_mouseInput = new User32.MouseInput();
			_mouseInput.LmbUp += MouseInputOnLmbUp;
			// TestView();
		}

		public void SetEnabled(bool enabled)
		{
			_enabled = enabled;
			if (_enabled)
			{
				Log.Debug("Adding Layout to OverlayCanvas");
				Core.OverlayCanvas.Children.Add(_layout);
				Canvas.SetBottom(_layout, Core.OverlayWindow.Height * 20 / 100);
				Canvas.SetLeft(_layout, Core.OverlayWindow.Width * .5 / 100);

				// Turn off the regular Opponent card list and restore the value later.
				_lastHideOpponentCards = Config.Instance.HideOpponentCards;
				Config.Instance.HideOpponentCards = true;
				_firstUpdateReceived = false;
			}
			else
			{
				Log.Debug("Removing List View from OverlayCanvas");
				Core.OverlayCanvas.Children.Remove(_layout);
				Config.Instance.HideOpponentCards = _lastHideOpponentCards;
			}
			UpdateShowing();
		}

		public void OnPredictionUpdate(PredictionInfo prediction)
		{
			_layout.Dispatcher.Invoke(() =>
				{
					_layout.Update(prediction);
				});
			_firstUpdateReceived = true;
			UpdateShowing();
		}

		private void UpdateShowing()
		{
			if (!_enabled)
			{
				return;
			}
			bool shouldShow = _enabled && _firstUpdateReceived && !Reflection.IsFriendsListVisible();
			Log.Debug("Update PredictionLayout visibility: " + shouldShow);
			_layout.Dispatcher.Invoke(() =>
				_layout.Visibility = shouldShow ? Visibility.Visible : Visibility.Hidden);
		}

		private async void MouseInputOnLmbUp(object sender, EventArgs eventArgs)
		{
			await Task.Delay(100);
			UpdateShowing();
		}

		private void TestView()
		{
			// TODO: AnimatedCardList seems to flipping the order of Create/Uncreated items.
			var card1 = Database.GetCardFromName("Greater Healing Potion");
			card1.Count = 2;
			var cardInfo1 = new PredictionInfo.CardInfo(card1, new List<decimal> {1, 1}, 0);
			var card2 = Database.GetCardFromName("Greater Healing Potion");
			card2.Count = 1;
			card2.IsCreated = true;
			var cardInfo2 = new PredictionInfo.CardInfo(card2, 1);
			var card3 = Database.GetCardFromName("Inner Fire");
			var cardList = new List<PredictionInfo.CardInfo> {cardInfo1, cardInfo2};
			cardList = cardList
				.OrderBy(cardInfo => cardInfo.Card.Cost)
				.ThenBy(cardInfo => cardInfo.Card.Name)
				.ThenByDescending(cardInfo => cardInfo.Card.IsCreated)
				.ToList();
			var prediction = new PredictionInfo(1, 2, 3, 4, cardList, new List<PredictionInfo.CardInfo> {});
			SetEnabled(true);
			OnPredictionUpdate(prediction);
		}
	}
}
