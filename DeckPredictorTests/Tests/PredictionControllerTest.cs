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
		private List<PredictedCardInfo> PredictedCardList(List<string> cardNames, List<int> copyCounts = null)
		{
			if (copyCounts == null)
			{
				copyCounts = Enumerable.Repeat(1, cardNames.Count).ToList();
			}
			return cardNames.Zip(copyCounts, (cardName, copyCount) =>
				new PredictedCardInfo(Database.GetCardFromName(cardName), copyCount, 1)).ToList();
		}

		private List<Card> CardList(List<string> cardNames, List<int> counts = null)
		{
			if (counts == null)
			{
				counts = Enumerable.Repeat(1, cardNames.Count).ToList();
			}
			return cardNames.Zip(counts, (cardName, count) =>
				{
					var card = Database.GetCardFromName(cardName);
					card.Count = count;
					return card;
				}).ToList();
		}

		[TestMethod]
		public void OnPredictionUpdate_CallsUpdateCards()
		{
			var opponent = new MockOpponent("Hunter");
			var predictor = new MockPredictor();
			var view = new MockPredictionView();

			var controller = new PredictionController(opponent, view);
			controller.OnPredictionUpdate(predictor);
			Assert.IsTrue(view.UpdateCardsCalled);
		}

		[TestMethod]
		public void OnPredictionUpdate_UsesPredictedCardList()
		{
			var opponent = new MockOpponent("Hunter");
			var view = new MockPredictionView();
			var controller = new PredictionController(opponent, view);

			var predictor = new MockPredictor();
			predictor.PredictedCards =
				PredictedCardList(new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"});
			controller.OnPredictionUpdate(predictor);
			CollectionAssert.AreEqual(CardList(new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"}),
				view.Cards);
		}

		[TestMethod]
		public void OnPredictionUpdate_CollapsesMultipleCopiesInPredictedCards()
		{
			var opponent = new MockOpponent("Hunter");
			var view = new MockPredictionView();
			var controller = new PredictionController(opponent, view);

			var predictor = new MockPredictor();
			predictor.PredictedCards = PredictedCardList(
				new List<string> {"Deadly Shot", "Deadly Shot", "Bear Trap"},
				new List<int> {1, 2, 1});
			controller.OnPredictionUpdate(predictor);
			var expectedCardList = CardList(
				new List<string> {"Deadly Shot", "Bear Trap"},
				new List<int> {2, 1});
			CollectionAssert.AreEqual(expectedCardList, view.Cards);
		}

		[TestMethod]
		public void OnPredictionUpdate_RemovesCardsAlreadyPlayed()
		{
			var opponent = new MockOpponent("Hunter");
			var view = new MockPredictionView();
			var controller = new PredictionController(opponent, view);

			var predictor = new MockPredictor();
			predictor.PredictedCards =
				PredictedCardList(new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"});
			opponent.KnownCards = CardList(new List<string> {"Alleycat"});
			controller.OnPredictionUpdate(predictor);
			CollectionAssert.AreEqual(CardList(new List<string> {"Deadly Shot", "Bear Trap"}),
				view.Cards);
		}

		[TestMethod]
		public void OnPredictionUpdate_DoesNotRemoveCreatedCards()
		{
			var opponent = new MockOpponent("Hunter");
			var view = new MockPredictionView();
			var controller = new PredictionController(opponent, view);

			var predictor = new MockPredictor();
			predictor.PredictedCards =
				PredictedCardList(new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"});
			opponent.KnownCards = CardList(new List<string> {"Alleycat"});
			opponent.KnownCards[0].IsCreated = true;
			controller.OnPredictionUpdate(predictor);
			CollectionAssert.AreEqual(CardList(new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"}),
				view.Cards);
		}

		[TestMethod]
		public void OnPredictionUpdate_DoesNotRemoveJoustedCards()
		{
			var opponent = new MockOpponent("Hunter");
			var view = new MockPredictionView();
			var controller = new PredictionController(opponent, view);

			var predictor = new MockPredictor();
			predictor.PredictedCards =
				PredictedCardList(new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"});
			opponent.KnownCards = CardList(new List<string> {"Alleycat"});
			opponent.KnownCards[0].Jousted = true;
			controller.OnPredictionUpdate(predictor);
			CollectionAssert.AreEqual(CardList(new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"}),
				view.Cards);
		}
	}
}
