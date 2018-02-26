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

		private void AddMetaDeck(string className)
		{
			var deck = new Deck();
			deck.Class = className;
			_metaDecks.Add(deck);
		}

		[TestMethod]
		public void OnGameStart_EmptyMetaDecks()
		{
			var predictor = new Predictor(new MockOpponent("Mage"), _metaDecks.AsReadOnly());
			predictor.OnGameStart();
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void OnGameStart_OneMetaDeckSameClass()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			Assert.IsTrue(_metaDecks.SequenceEqual(predictor.GetPossibleDecks()));
		}

		[TestMethod]
		public void OnGameStart_OneMetaDeckDifferentClass()
		{
			var opponent = new MockOpponent("Mage");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void OnOpponentPlay_MissingCardFiltersDeck()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			var card = new Card();
			card.Id = "card_id1";
			opponent.Cards.Add(card);
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void OnOpponentPlay_MatchingCardDoesNotFilter()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var hunterCard = new Card();
			hunterCard.Id = "card_id1";
			_metaDecks[0].Cards.Add(hunterCard);
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			opponent.Cards.Add(hunterCard);
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(1, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void OnOpponentPlay_MissingCreatedCardDoesNotFilter()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			var hunterCard = new Card();
			hunterCard.Id = "card_id1";
			hunterCard.IsCreated = true;
			opponent.Cards.Add(hunterCard);
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(1, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void OnOpponentHandDiscard_MissingCardFiltersDeck()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			var card = new Card();
			card.Id = "card_id1";
			opponent.Cards.Add(card);
			predictor.OnOpponentHandDiscard(null);
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}
	}
}
