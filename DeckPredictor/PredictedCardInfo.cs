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
		private int _numOccurrences;
		private int _numPossibleDecks;

		public PredictedCardInfo(Card card, int copyCount, int numPossibleDecks)
		{
			Card = card;
			CopyCount = copyCount;
			_numOccurrences = 0;
			_numPossibleDecks = numPossibleDecks;
		}

		public Card Card { get; }

		// Track each copy of a card separately in the deck.
		// This is 1-indexed to mirror Card.Count
		public int CopyCount { get; }

		public void IncrementNumOccurrences()
		{
			_numOccurrences++;
		}

		public double Probability => (double)_numOccurrences / _numPossibleDecks;

		public string Key() => Key(Card, CopyCount);

		public static string Key(Card card, int copyCount) => card.Id + copyCount;

		public override string ToString()
		{
			return "[" + Card.Cost + "] " +
				Card.Name + "(" + CopyCount + ")" +
				" - " + Math.Truncate(Probability * 100) + "%";
		}
	}
}
