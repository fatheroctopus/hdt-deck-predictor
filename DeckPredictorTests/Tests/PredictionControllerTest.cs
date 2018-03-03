using DeckPredictor;
using DeckPredictorTests.Mocks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DeckPredictorTests.Tests
{
	[TestClass]
	public class PredictionControllerTest
	{
		private List<Deck> _metaDecks = new List<Deck>();

		private void AddMetaDeck(string className, List<string> cardNames = null, List<int> counts = null)
		{
			var deck = new Deck();
			deck.Class = className;
			CardList(cardNames, counts).ForEach(card => deck.Cards.Add(card));
			_metaDecks.Add(deck);
		}

		private List<Card> CardList(List<string> cardNames, List<int> counts = null)
		{
			if (cardNames == null)
			{
				return new List<Card>();
			}
			if (counts == null)
			{
				counts = Enumerable.Repeat(1, cardNames.Count).ToList();
			}
			return cardNames.Zip(counts, (cardName, count) =>
				{
					var card = Database.GetCardFromName(cardName);
					card.Count = count;
					return card;
				})
				.OrderBy(card => card.Cost).ThenBy(card => card.Name)
				.ToList();
		}

		private PredictionInfo GetPredictionInfo(PredictionController controller)
		{
			PredictionInfo info = null;
			controller.OnPredictionUpdate.Add(p => info = p);
			controller.OnOpponentDraw();
			return info;
		}

		[TestMethod]
		public void OnOpponentDraw_CallsOnPredictionUpdate()
		{
			var opponent = new MockOpponent("Hunter");
			var controller = new PredictionController(opponent, _metaDecks.AsReadOnly());

			bool called = false;
			controller.OnPredictionUpdate.Add(prediction => called = true);
			controller.OnOpponentDraw();

			Assert.IsTrue(called);
		}

		[TestMethod]
		public void OnOpponentHandDiscard_CallsOnPredictionUpdate()
		{
			var opponent = new MockOpponent("Hunter");
			var controller = new PredictionController(opponent, _metaDecks.AsReadOnly());

			bool called = false;
			controller.OnPredictionUpdate.Add(prediction => called = true);
			controller.OnOpponentHandDiscard(null);

			Assert.IsTrue(called);
		}

		[TestMethod]
		public void OnOpponentPlay_CallsOnPredictionUpdate()
		{
			var opponent = new MockOpponent("Hunter");
			var controller = new PredictionController(opponent, _metaDecks.AsReadOnly());

			bool called = false;
			controller.OnPredictionUpdate.Add(prediction => called = true);
			controller.OnOpponentPlay(null);

			Assert.IsTrue(called);
		}

		[TestMethod]
		public void UpdatesWithMetaDeck()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot"});
			var controller = new PredictionController(opponent, _metaDecks.AsReadOnly());

			var info = GetPredictionInfo(controller);
			Assert.AreEqual(1, info.NumPossibleDecks);
		}

		[TestMethod]
		public void UpdatesWithFullCardList()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"});
			var controller = new PredictionController(opponent, _metaDecks.AsReadOnly());

			var info = GetPredictionInfo(controller);
			CollectionAssert.AreEqual(CardList(new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"}),
				info.UnplayedCards);
		}

		[TestMethod]
		public void UpdatesWithNoDeckIfClassMismatch()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Mage", new List<string> {});
			var controller = new PredictionController(opponent, _metaDecks.AsReadOnly());

			var info = GetPredictionInfo(controller);
			Assert.AreEqual(0, info.NumPossibleDecks);
		}

		[TestMethod]
		public void UpdatesWithMultipleCopies()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter",
				new List<string> {"Alleycat", "Deadly Shot", "Bear Trap"},
				new List<int> {1, 2, 1});
			var controller = new PredictionController(opponent, _metaDecks.AsReadOnly());

			var info = GetPredictionInfo(controller);
			var expectedCardList = CardList(
				new List<string> {"Alleycat", "Deadly Shot", "Bear Trap"},
				new List<int> {1, 2, 1});
			CollectionAssert.AreEqual(expectedCardList, info.UnplayedCards);
		}

		[TestMethod]
		public void RemovesCardsAlreadyPlayed()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"});
			var controller = new PredictionController(opponent, _metaDecks.AsReadOnly());
			opponent.KnownCards = CardList(new List<string> {"Alleycat"});
			var info = GetPredictionInfo(controller);
			CollectionAssert.AreEqual(CardList(new List<string> {"Deadly Shot", "Bear Trap"}),
				info.UnplayedCards);
		}

		[TestMethod]
		public void OnPredictionUpdate_DoesNotRemoveCreatedCards()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"});
			var controller = new PredictionController(opponent, _metaDecks.AsReadOnly());
			opponent.KnownCards = CardList(new List<string> {"Alleycat"});
			opponent.KnownCards[0].IsCreated = true;
			var info = GetPredictionInfo(controller);
			CollectionAssert.AreEqual(CardList(new List<string> {"Alleycat", "Deadly Shot", "Bear Trap"}),
				info.UnplayedCards);
		}

		[TestMethod]
		public void OnPredictionUpdate_DoesNotRemoveJoustedCards()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat", "Bear Trap"});
			var controller = new PredictionController(opponent, _metaDecks.AsReadOnly());
			opponent.KnownCards = CardList(new List<string> {"Alleycat"});
			opponent.KnownCards[0].Jousted = true;
			var info = GetPredictionInfo(controller);
			CollectionAssert.AreEqual(CardList(new List<string> {"Alleycat", "Deadly Shot", "Bear Trap"}),
				info.UnplayedCards);
		}
	}
}
