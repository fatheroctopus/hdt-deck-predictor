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
	public class PredictionControllerTest
	{
		private List<PredictedCardInfo> PredictedCardList(List<string> cardNames) =>
			CardList(cardNames).Select(card => new PredictedCardInfo(card, 1, 1)).ToList();

		private List<Card> CardList(List<string> cardNames) =>
			cardNames.Select(cardName => Database.GetCardFromName(cardName)).ToList();

		[TestMethod]
		public void OnPredictionUpdate_CallsUpdatePredictedCards()
		{
			var opponent = new MockOpponent("Hunter");
			var controller = new PredictionController(opponent);

			var predictor = new MockPredictor();
			controller.OnPredictionUpdate(predictor);
			Assert.IsTrue(opponent.UpdatePredictedCardsCalled);
		}

		[TestMethod]
		public void OnPredictionUpdate_CallsPredictedCardList()
		{
			var opponent = new MockOpponent("Hunter");
			var controller = new PredictionController(opponent);

			var predictor = new MockPredictor();
			predictor.PredictedCards =
				PredictedCardList(new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"});
			controller.OnPredictionUpdate(predictor);
			CollectionAssert.AreEqual(CardList(new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"}),
				opponent.PredictedCards);
		}

		// [TestMethod]
		// public void OnPredictionUpdate_RemovesCardsAlreadyPlayed()
		// {
		// 	var opponent = new MockOpponent("Hunter");
		// 	var controller = new PredictionController(opponent);

		// 	var predictor = new MockPredictor();
		// 	predictor.PredictedCards =
		// 		PredictedCardList(new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"});
		// 	opponent.KnownCards = CardList(new List<string> {"Alleycat"});
		// 	controller.OnPredictionUpdate(predictor);
		// 	CollectionAssert.AreEqual(CardList(new List<string> {"Deadly Shot", "Bear Trap"}),
		// 		opponent.PredictedCards);
		// }
	}
}
