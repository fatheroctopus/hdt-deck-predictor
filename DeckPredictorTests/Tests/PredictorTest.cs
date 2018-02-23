using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeckPredictor;
using DeckPredictorTests.Mocks;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Hearthstone;
using System.Linq;

namespace DeckPredictorTests.Tests
{
	[TestClass]
	public class PredictorTest
	{
		[TestMethod]
		public void GameStart_EmptyMetaDecks()
		{
			var predictor = new Predictor(new List<Deck>());
			predictor.GameStart(new MockGame());
			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}

		[TestMethod]
		public void GameStart_OneMetaDeckSameClass()
		{
			var metaDecks = new List<Deck>();
			metaDecks.Add(new Deck());
			metaDecks[0].Class = "Hunter";
			var predictor = new Predictor(metaDecks);

			var game = new MockGame();
			game.Opponent.Class = "Hunter";
			predictor.GameStart(game);

			Assert.IsTrue(metaDecks.SequenceEqual(predictor.GetPossibleDecks()));
		}

		[TestMethod]
		public void GameStart_OneMetaDeckDifferentClass()
		{
			var metaDecks = new List<Deck>();
			metaDecks.Add(new Deck());
			metaDecks[0].Class = "Hunter";
			var predictor = new Predictor(metaDecks);

			var game = new MockGame();
			game.Opponent.Class = "Mage";
			predictor.GameStart(game);

			Assert.AreEqual(0, predictor.GetPossibleDecks().Count);
		}

	}
}
