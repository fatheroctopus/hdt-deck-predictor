using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DeckPredictor;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace DeckPredictorTests.Mocks
{
	public class MockOpponent : IOpponent
	{
		public MockOpponent(string className)
		{
			Class = className;
			KnownCards = new List<Card>();

			// Default to all cards being playable.
			AvailableManaNextTurn = 10;
		}

		public string Class { get; set; }

		public List<Card> KnownCards { get; set; }

		public int AvailableManaNextTurn { get; set; }

	}
}
