using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Controls;

namespace DeckPredictor
{
	public class PredictionView : IPredictionView
	{
		private AnimatedCardList _cardList = new AnimatedCardList();

		public PredictionView() {
			Log.Debug("Adding List to OverlayCanvas");
			Core.OverlayCanvas.Children.Add(_cardList);
			Canvas.SetTop(_cardList, Core.OverlayWindow.Height * 5 / 100);
			Canvas.SetLeft(_cardList, Core.OverlayWindow.Width * 80 / 100);
		}

		public void UpdateCards(List<Hearthstone_Deck_Tracker.Hearthstone.Card> cards)
		{
			Log.Debug("Updating view list with " + cards.Count + " cards");
			// _cardList.Visibility = cards.Count <= 0 ? Visibility.Hidden : Visibility.Visible;
			_cardList.Dispatcher.Invoke(() => _cardList.Update(cards, true));
		}
	}
}
