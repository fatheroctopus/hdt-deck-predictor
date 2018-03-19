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
	public class CardProximityRankerTest
	{
		private List<Deck> _decks = new List<Deck>();

		private void AddDeck(List<string> cardNames, List<int> counts = null)
		{
			var deck = new Deck();
			CardList(cardNames, counts).ForEach(card => deck.Cards.Add(card));
			_decks.Add(deck);
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
				})
				.ToList();
		}

		[TestMethod]
		public void OrderedCardsAreSameAsUpdateCards()
		{
			var ranker = new CardProximityRanker(_decks);
			var cards = CardList(new List<string> {"Alleycat", "Deadly Shot"});
			ranker.UpdateCards(cards);
			CollectionAssert.AreEqual(CardList(new List<string> {"Alleycat", "Deadly Shot"}),
				ranker.RankedCards);
		}

		[TestMethod]
		public void OrderCardWithMatchingDecksBeforeCardWithoutMatch()
		{
			AddDeck(new List<string> {"Deadly Shot"});
			var ranker = new CardProximityRanker(_decks);
			var cards = CardList(new List<string> {"Alleycat", "Deadly Shot"});
			ranker.UpdateCards(cards);
			CollectionAssert.AreEqual(CardList(new List<string> {"Deadly Shot", "Alleycat"}),
				ranker.RankedCards);
		}

		[TestMethod]
		public void FirstCardHasMostDecksInCommon()
		{
			AddDeck(new List<string> {"Alleycat", "Deadly Shot"});
			AddDeck(new List<string> {"Bear Trap", "Deadly Shot"});
			var ranker = new CardProximityRanker(_decks);
			ranker.UpdateCards(CardList(new List<string> {"Alleycat", "Bear Trap", "Deadly Shot"}));
			Assert.AreEqual("Deadly Shot", ranker.RankedCards[0].Name);
		}

		[TestMethod]
		public void LastCardHasFewestDecksInCommon()
		{
			AddDeck(new List<string> {"Alleycat", "Bear Trap"});
			AddDeck(new List<string> {"Bear Trap", "Tracking", "Deadly Shot"});
			AddDeck(new List<string> {"Tracking", "Deadly Shot"});
			var ranker = new CardProximityRanker(_decks);
			ranker.UpdateCards(
				CardList(new List<string> {"Alleycat", "Bear Trap", "Tracking", "Deadly Shot"}));
			Assert.AreEqual("Alleycat", ranker.RankedCards[3].Name);
		}

		[TestMethod]
		public void LastCardHasLeastOverlapWithRankedCards()
		{
			AddDeck(new List<string> {"Alleycat", "Bear Trap"});
			AddDeck(new List<string> {"Bear Trap", "Tracking", "Deadly Shot"});
			var ranker = new CardProximityRanker(_decks);
			ranker.UpdateCards(
				CardList(new List<string> {"Alleycat", "Bear Trap", "Tracking", "Deadly Shot"}));
			Assert.AreEqual("Alleycat", ranker.RankedCards[3].Name);
		}

		[TestMethod]
		public void FirstCardIsFirstCopyIfSameDecksInCommon()
		{
			AddDeck(new List<string> {"Alleycat"}, new List<int> {2});
			var ranker = new CardProximityRanker(_decks);
			ranker.UpdateCards(CardList(new List<string> {"Alleycat"}, new List<int> {2}));
			Assert.AreEqual(1, ranker.RankedCards[0].Count);
		}

		[TestMethod]
		public void FirstCardIsFirstCopyIfMostDecksInCommon()
		{
			AddDeck(new List<string> {"Alleycat"}, new List<int> {2});
			AddDeck(new List<string> {"Alleycat", "Deadly Shot"}, new List<int> {2, 1});
			var ranker = new CardProximityRanker(_decks);
			ranker.UpdateCards(CardList(new List<string> {"Alleycat", "Deadly Shot"}));
			Assert.AreEqual("Alleycat", ranker.RankedCards[0].Name);
		}

		[TestMethod]
		public void UpdateCards_ReturnsFalseIfNoCards()
		{
			var ranker = new CardProximityRanker(_decks);
			Assert.IsFalse(ranker.UpdateCards(new List<Card>()));
		}

		[TestMethod]
		public void UpdateCards_ReturnsTrueIfCardsNonEmpty()
		{
			var ranker = new CardProximityRanker(_decks);
			Assert.IsTrue(ranker.UpdateCards(CardList(new List<string> {"Alleycat"})));
		}

		[TestMethod]
		public void UpdateCards_ReturnsFalseIfCardsUnchangedFromPreviousCall()
		{
			var ranker = new CardProximityRanker(_decks);
			ranker.UpdateCards(CardList(new List<string> {"Alleycat"}));
			Assert.IsFalse(ranker.UpdateCards(CardList(new List<string> {"Alleycat"})));
		}

		[TestMethod]
		public void UpdateCards_ReturnsTrueIfCardsChangedFromPreviousCall()
		{
			var ranker = new CardProximityRanker(_decks);
			ranker.UpdateCards(CardList(new List<string> {"Alleycat"}));
			Assert.IsTrue(ranker.UpdateCards(CardList(new List<string> {"Alleycat", "Deadly Shot"})));
		}
	}
}
