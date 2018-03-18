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

		private void AddDeck(List<string> cardNames)
		{
			var deck = new Deck();
			CardList(cardNames).ForEach(card => deck.Cards.Add(card));
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
			CollectionAssert.AreEqual(CardList(new List<string> {"Alleycat", "Deadly Shot"}),
				ranker.RankCards(cards));
		}

		[TestMethod]
		public void OrderCardWithMatchingDecksBeforeCardWithoutMatch()
		{
			AddDeck(new List<string> {"Deadly Shot"});
			var ranker = new CardProximityRanker(_decks);
			var cards = CardList(new List<string> {"Alleycat", "Deadly Shot"});
			CollectionAssert.AreEqual(CardList(new List<string> {"Deadly Shot", "Alleycat"}),
				ranker.RankCards(cards));
		}

		[TestMethod]
		public void FirstCardHasMostDecksInCommon()
		{
			AddDeck(new List<string> {"Alleycat", "Deadly Shot"});
			AddDeck(new List<string> {"Bear Trap", "Deadly Shot"});
			var ranker = new CardProximityRanker(_decks);
			var rankedCards =
				ranker.RankCards(CardList(new List<string> {"Alleycat", "Bear Trap", "Deadly Shot"}));
			Assert.AreEqual("Deadly Shot", rankedCards[0].Name);
		}

		[TestMethod]
		public void LastCardHasFewestDecksInCommon()
		{
			AddDeck(new List<string> {"Alleycat", "Bear Trap"});
			AddDeck(new List<string> {"Bear Trap", "Tracking", "Deadly Shot"});
			AddDeck(new List<string> {"Tracking", "Deadly Shot"});
			var ranker = new CardProximityRanker(_decks);
			var rankedCards = ranker.RankCards(
				CardList(new List<string> {"Alleycat", "Bear Trap", "Tracking", "Deadly Shot"}));
			Assert.AreEqual("Alleycat", rankedCards[3].Name);
		}

		[TestMethod]
		public void LastCardHasLeastOverlapWithRankedCards()
		{
			AddDeck(new List<string> {"Alleycat", "Bear Trap"});
			AddDeck(new List<string> {"Bear Trap", "Tracking", "Deadly Shot"});
			var ranker = new CardProximityRanker(_decks);
			var rankedCards = ranker.RankCards(
				CardList(new List<string> {"Alleycat", "Bear Trap", "Tracking", "Deadly Shot"}));
			Assert.AreEqual("Alleycat", rankedCards[3].Name);
		}
	}
}
