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
			Cards = new List<Card>();
		}

		// Manipulate this in tests to affect KnownCards.
		public List<Card> Cards {get; set;}

		public string Class { get; set; }
		public ReadOnlyCollection<Card> KnownCards => Cards.AsReadOnly();
	}
}
