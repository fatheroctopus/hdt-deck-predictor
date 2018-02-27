using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace DeckPredictor
{
	public class PredictedCardInfo
	{
		public PredictedCardInfo(Card card, int copyCount)
		{
			Card = card;
			CopyCount = copyCount;
		}

		public Card Card { get; }

		// Track each copy of a card separately in the deck.
		// This is 1-indexed to mirror Card.Count
		public int CopyCount { get; }

		public string Key() => Key(Card, CopyCount);

		public static string Key(Card card, int copyCount) => card.Id + copyCount;

		public override string ToString()
		{
			return Card.Name + "(" + CopyCount + ")";
		}
	}
}
