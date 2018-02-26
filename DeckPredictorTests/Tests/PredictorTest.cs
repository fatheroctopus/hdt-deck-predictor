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
			opponent.Cards.Add(Database.GetCardFromName("Deadly Shot"));
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void OnOpponentHandDiscard_MissingCardFiltersDeck()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			opponent.Cards.Add(Database.GetCardFromName("Deadly Shot"));
			predictor.OnOpponentHandDiscard(null);
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void MissingSecondCardFiltersDeck()
		{
			var opponent = new MockOpponent("Hunter");
			var hunterCard = Database.GetCardFromName("Deadly Shot");
			AddMetaDeck("Hunter");
			_metaDecks[0].Cards.Add(hunterCard);
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			opponent.Cards.Add(Database.GetCardFromName("Deadly Shot"));
			opponent.Cards.Add(Database.GetCardFromName("Alleycat"));
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void MatchingCardDoesNotFilter()
		{
			var opponent = new MockOpponent("Hunter");
			var hunterCard = Database.GetCardFromName("Deadly Shot");
			AddMetaDeck("Hunter");
			_metaDecks[0].Cards.Add(hunterCard);
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			opponent.Cards.Add(hunterCard);
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(1, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void MissingCreatedCardDoesNotFilter()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			var hunterCard = Database.GetCardFromName("Deadly Shot");
			hunterCard.IsCreated = true;
			opponent.Cards.Add(hunterCard);
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(1, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void MissingSecondCardAfterCreatedCardFiltersDeck()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			var hunterCard = Database.GetCardFromName("Deadly Shot");
			hunterCard.IsCreated = true;
			opponent.Cards.Add(hunterCard);
			opponent.Cards.Add(Database.GetCardFromName("Deadly Shot"));
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void MissingNonCollectibleCardDoesNotFilter()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			var hunterCard = Database.GetCardFromName("Greater Emerald Spellstone");
			opponent.Cards.Add(hunterCard);
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(1, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void MissingSecondCopyFiltersDeck()
		{
			var opponent = new MockOpponent("Hunter");
			AddMetaDeck("Hunter");
			_metaDecks[0].Cards.Add(Database.GetCardFromName("Deadly Shot"));
			var predictor = new Predictor(opponent, _metaDecks.AsReadOnly());

			predictor.OnGameStart();
			var hunterCard2Copies = Database.GetCardFromName("Deadly Shot");
			hunterCard2Copies.Count = 2;
			opponent.Cards.Add(hunterCard2Copies);
			predictor.OnOpponentPlay(null);
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}
	}
}
