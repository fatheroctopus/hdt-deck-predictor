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
	public class PredictorTest
	{
		private List<Deck> _metaDecks = new List<Deck>();

		private void AddMetaDeck(string className, List<string> cardNames = null)
		{
			var deck = new Deck();
			deck.Class = className;
			if (cardNames != null)
			{
				foreach (string cardName in cardNames)
				{
					deck.Cards.Add(Database.GetCardFromName(cardName));
				}
			}
			_metaDecks.Add(deck);
		}

		private string Key(string cardName, int copyCount)
		{
			var card = Database.GetCardFromName(cardName);
			return PredictedCardInfo.Key(card, copyCount);
		}

		[TestMethod]
		public void PossibleDecks_EmptyByDefault()
		{
			var predictor = new Predictor(new MockOpponent("Mage"), _metaDecks.AsReadOnly());
			Assert.AreEqual(0, predictor.PossibleDecks.Count);
		}

		[TestMethod]
		public void OnGameStart_OneMetaDeckSameClass()
		{
			AddMetaDeck("Hunter");
			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			Assert.IsTrue(_metaDecks.SequenceEqual(predictor.PossibleDecks));
		}

		[TestMethod]
		public void OnGameStart_OneMetaDeckDifferentClass()
		{
			AddMetaDeck("Hunter");
			var predictor = new Predictor(new MockOpponent("Mage"), _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			Assert.AreEqual(0, predictor.PossibleDecks.Count);
		}

		[TestMethod]
		public void OnGameStart_CallsOnPredictionUpdate()
		{
			AddMetaDeck("Hunter");
			var predictor = new Predictor(new MockOpponent("Mage"), _metaDecks.AsReadOnly());
			bool called = false;
			Action<Predictor> callback = x => { called = true; };
			predictor.OnPredictionUpdate.Add(callback);

			predictor.OnGameStart();
			Assert.IsTrue(called);
		}

		[TestMethod]
		public void OnOpponentPlay_MissingCardFiltersDeck()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			opponent.Cards.Add(Database.GetCardFromName("Deadly Shot"));
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(0, predictor.PossibleDecks.Count);
		}

		[TestMethod]
		public void OnOpponentHandDiscard_MissingCardFiltersDeck()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			opponent.Cards.Add(Database.GetCardFromName("Deadly Shot"));
			predictor.OnOpponentHandDiscard(null);
			Assert.AreEqual(0, predictor.PossibleDecks.Count);
		}

		[TestMethod]
		public void GetPossibleDecks_MissingSecondCardFiltersDeck()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot"});
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			opponent.Cards.Add(Database.GetCardFromName("Deadly Shot"));
			opponent.Cards.Add(Database.GetCardFromName("Alleycat"));
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(0, predictor.PossibleDecks.Count);
		}

		[TestMethod]
		public void GetPossibleDecks_MatchingCardDoesNotFilter()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot"});
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			var hunterCard = Database.GetCardFromName("Deadly Shot");
			opponent.Cards.Add(hunterCard);
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(1, predictor.PossibleDecks.Count);
		}

		[TestMethod]
		public void GetPossibleDecks_MissingCreatedCardDoesNotFilter()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			var hunterCard = Database.GetCardFromName("Deadly Shot");
			hunterCard.IsCreated = true;
			opponent.Cards.Add(hunterCard);
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(1, predictor.PossibleDecks.Count);
		}

		[TestMethod]
		public void GetPossibleDecks_MissingSecondCardAfterCreatedCardFiltersDeck()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			var hunterCard = Database.GetCardFromName("Deadly Shot");
			hunterCard.IsCreated = true;
			opponent.Cards.Add(hunterCard);
			opponent.Cards.Add(Database.GetCardFromName("Deadly Shot"));
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(0, predictor.PossibleDecks.Count);
		}

		[TestMethod]
		public void GetPossibleDecks_MissingNonCollectibleCardDoesNotFilter()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			var hunterCard = Database.GetCardFromName("Greater Emerald Spellstone");
			opponent.Cards.Add(hunterCard);
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(1, predictor.PossibleDecks.Count);
		}

		[TestMethod]
		public void GetPossibleDecks_MissingSecondCopyFiltersDeck()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot"});
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			var hunterCard2Copies = Database.GetCardFromName("Deadly Shot");
			hunterCard2Copies.Count = 2;
			opponent.Cards.Add(hunterCard2Copies);
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(0, predictor.PossibleDecks.Count);
		}

		[TestMethod]
		public void GetPredictedCards_EmptyByDefault()
		{
			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());
			Assert.AreEqual(0, predictor.PredictedCards.Count);
		}

		[TestMethod]
		public void GetPredictedCards_SameAsSingleMetaDeck()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});

			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());
			Assert.AreEqual(2, predictor.PredictedCards.Count);
		}

		[TestMethod]
		public void GetPredictedCards_EmptyAfterClassFiltered()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});

			var predictor = new Predictor(new MockOpponent("Mage"), _metaDecks.AsReadOnly());
			predictor.OnGameStart();
			Assert.AreEqual(0, predictor.PredictedCards.Count);
		}

		[TestMethod]
		public void GetPredictedCards_EmptyAfterDeckFiltered()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});

			var opponent = new MockOpponent("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());
			opponent.Cards.Add(Database.GetCardFromName("Savannah Highmane"));
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(0, predictor.PredictedCards.Count);
		}

		[TestMethod]
		public void GetPredictedCards_CombinesContentsOfTwoDecks()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});
			AddMetaDeck("Hunter", new List<string> {"Arcane Shot", "Bear Trap"});

			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());
			Assert.AreEqual(4, predictor.PredictedCards.Count);
		}

		[TestMethod]
		public void GetPredictedCards_UnionOfTwoDecks()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});
			AddMetaDeck("Hunter", new List<string> {"Alleycat", "Bear Trap"});

			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());
			Assert.AreEqual(3, predictor.PredictedCards.Count);
			// First copy of Alleycat.
			Assert.IsNotNull(predictor.GetPredictedCard(Key("Alleycat", 1)));
			// No second copy of Alleycat.
			Assert.IsNull(predictor.GetPredictedCard(Key("Alleycat", 2)));
		}

		[TestMethod]
		public void GetPredictedCards_SameAsFirstDeckAfterSecondFiltered()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});
			AddMetaDeck("Hunter", new List<string> {"Alleycat", "Bear Trap"});

			var opponent = new MockOpponent("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());
			opponent.Cards.Add(Database.GetCardFromName("Deadly Shot"));
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(2, predictor.PredictedCards.Count);
			Assert.IsNotNull(predictor.GetPredictedCard(Key("Alleycat", 1)));
			Assert.IsNotNull(predictor.GetPredictedCard(Key("Deadly Shot", 1)));
		}

		[TestMethod]
		public void GetPredictedCards_UnionTakesHigherCardCount()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});
			AddMetaDeck("Hunter", new List<string> {"Alleycat", "Bear Trap"});
			_metaDecks[1].Cards[0].Count = 2;

			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());
			Assert.AreEqual(4, predictor.PredictedCards.Count);
			Assert.IsNotNull(predictor.GetPredictedCard(Key("Alleycat", 2)));
		}

		[TestMethod]
		public void GetPredictedCards_SortedByDescendingProbability()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});
			AddMetaDeck("Hunter", new List<string> {"Alleycat", "Bear Trap"});
			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());
			PredictedCardInfo firstPredictedCard = predictor.PredictedCards.ElementAt(0);
			Assert.AreEqual("Alleycat", firstPredictedCard.Card.Name);
		}

		[TestMethod]
		public void GetPredictedCards_SortedSecondaryByLowerManaCost()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});
			AddMetaDeck("Hunter", new List<string> {"Alleycat", "Bear Trap"});
			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());
			PredictedCardInfo firstPredictedCard = predictor.PredictedCards.ElementAt(1);
			Assert.AreEqual("Bear Trap", firstPredictedCard.Card.Name);
		}

		[TestMethod]
		public void GetPredictedCards_ReturnsNoMoreThanDeckSize()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot"});
			_metaDecks[0].Cards[0].Count = 30;
			_metaDecks[1].Cards[0].Count = 30;

			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());
			Assert.AreEqual(30, predictor.PredictedCards.Count);
		}

		[TestMethod]
		public void GetPredictedCards_LessThanDeckSizeIfAtSameProbability()
		{
			AddMetaDeck("Hunter", new List<string> {"Alleycat"});
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});
			_metaDecks[1].Cards[0].Count = 40;

			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());
			// All the Deadly Shots are at the same probability, so don't include any of them.
			Assert.AreEqual(1, predictor.PredictedCards.Count);
		}

		[TestMethod]
		public void GetPredictedCard_ProbabilityIsOneForSinglePossibleDeck()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});
			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());
			Assert.AreEqual(1, predictor.GetPredictedCard(Key("Deadly Shot", 1)).Probability, .01);
		}

		[TestMethod]
		public void GetPredictedCard_ProbabilityIsHalfForOneOfTwoDecks()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});
			AddMetaDeck("Hunter", new List<string> {"Alleycat", "Bear Trap"});
			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());
			Assert.AreEqual(.5, predictor.GetPredictedCard(Key("Deadly Shot", 1)).Probability, .01);
		}

		[TestMethod]
		public void GetPredictedCard_ProbabilityIsOneWhenInBothDecks()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});
			AddMetaDeck("Hunter", new List<string> {"Alleycat", "Bear Trap"});
			var predictor = new Predictor(new MockOpponent("Hunter"), _metaDecks.AsReadOnly());
			Assert.AreEqual(1, predictor.GetPredictedCard(Key("Alleycat", 1)).Probability, .01);
		}

		[TestMethod]
		public void GetPredictedCard_ProbabilityReturnsToOneAfterSecondDeckFiltered()
		{
			AddMetaDeck("Hunter", new List<string> {"Deadly Shot", "Alleycat"});
			AddMetaDeck("Hunter", new List<string> {"Alleycat", "Bear Trap"});
			var opponent = new MockOpponent("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			opponent.Cards.Add(Database.GetCardFromName("Deadly Shot"));
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(1, predictor.GetPredictedCard(Key("Deadly Shot", 1)).Probability, .01);
		}
	}
}
