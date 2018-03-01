using Hearthstone_Deck_Tracker.Hearthstone;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace DeckPredictor
{
	public interface IPredictionView
	{
		// Updates the card list that reflects the current deck prediction.
		void UpdateCards(List<Card> cards);
	}
}
