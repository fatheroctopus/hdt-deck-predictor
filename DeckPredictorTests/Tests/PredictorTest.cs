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
		[TestMethod]
		public void OnGameStart_EmptyMetaDecks()
		{
			var predictor = new Predictor(new MockOpponent("Mage"), new List<Deck>().AsReadOnly());
			predictor.OnGameStart();
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void OnGameStart_OneMetaDeckSameClass()
		{
			var opponent = new MockOpponent("Hunter");
			var metaDecks = new List<Deck>();
			metaDecks.Add(new Deck());
			metaDecks[0].Class = "Hunter";
			var predictor = new Predictor(opponent, metaDecks.AsReadOnly());

			predictor.OnGameStart();
			Assert.IsTrue(metaDecks.SequenceEqual(predictor.GetPossibleDecks()));
		}

		[TestMethod]
		public void OnGameStart_OneMetaDeckDifferentClass()
		{
			var opponent = new MockOpponent("Mage");
			var metaDecks = new List<Deck>();
			metaDecks.Add(new Deck());
			metaDecks[0].Class = "Hunter";
			var predictor = new Predictor(opponent, metaDecks.AsReadOnly());

			predictor.OnGameStart();
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void OnOpponentPlay_MissingCardFiltersDeck()
		{
			var opponent = new MockOpponent("Hunter");
			var metaDecks = new List<Deck>();
			metaDecks.Add(new Deck());
			metaDecks[0].Class = "Hunter";
			var predictor = new Predictor(opponent, metaDecks.AsReadOnly());

			predictor.OnGameStart();
			var card = new Card();
			card.Id = "card_id1";
			opponent.Cards.Add(card);
			predictor.OnOpponentPlay(card);
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void OnOpponentPlay_MatchingCardDoesNotFilter()
		{
			var opponent = new MockOpponent("Hunter");
			var metaDecks = new List<Deck>();
			metaDecks.Add(new Deck());
			metaDecks[0].Class = "Hunter";
			var hunterCard = new Card();
			hunterCard.Id = "card_id1";
			metaDecks[0].Cards.Add(hunterCard);
			var predictor = new Predictor(opponent, metaDecks.AsReadOnly());

			predictor.OnGameStart();
			opponent.Cards.Add(hunterCard);
			predictor.OnOpponentPlay(hunterCard);
			Assert.AreEqual(1, predictor.GetPossibleDecks().Count);
		}

	}
}
