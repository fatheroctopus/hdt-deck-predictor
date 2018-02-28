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
	public class MockPredictor : IPredictor
	{
		public MockPredictor() {
			PredictedCards = new List<PredictedCardInfo>();
		}

		public List<PredictedCardInfo> PredictedCards { get; set; }
	}
}
