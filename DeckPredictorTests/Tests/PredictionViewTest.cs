using DeckPredictor;
using DeckPredictorTests.Mocks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DeckPredictorTests.Tests
{
	[TestClass]
	public class PredictionViewTest
	{
		[TestMethod]
		public void OnPredictionUpdate_CallsUpdatePredictedCards()
		{
			var opponent = new MockOpponent("Hunter");
			var view = new PredictionView(opponent);

			var predictor = new MockPredictor();
			view.OnPredictionUpdate(predictor);
			Assert.IsTrue(opponent.UpdatePredictedCardsCalled);
		}
	}
}
