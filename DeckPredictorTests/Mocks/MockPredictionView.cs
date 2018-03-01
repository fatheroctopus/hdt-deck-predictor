﻿using System;
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
	public class MockPredictionView : IPredictionView
	{
		public List<Card> Cards { get; set; }

		public bool UpdateCardsCalled => Cards != null;

		public void UpdateCards(List<Card> cards)
		{
			Cards = cards;
		}
	}
}
