using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace DeckPredictor
{
	public class Opponent : IOpponent
	{
		private Player _opponent;

		public Opponent(Player opponent)
		{
			_opponent = opponent;
		}

		public string Class => _opponent.Class;

		public ReadOnlyCollection<Card> KnownCards => _opponent.OpponentCardList.AsReadOnly();

		public void UpdatePredictedCards(List<Card> cards)
		{
			// Clear the current "Precitions" and add the new List.
			_opponent.InDeckPrecitions.Clear();
			foreach (Card card in cards)
			{
				_opponent.InDeckPrecitions.Add(new PredictedCard(card.Id, 0));
			}
		}
	}
}
