using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System;

namespace DeckPredictor
{
	public class PredictionController
	{
		private IOpponent _opponent;
		private Predictor _predictor;

		public PredictionController(IOpponent opponent, ReadOnlyCollection<Deck> metaDecks)
		{
			_opponent = opponent;
			_predictor = new Predictor(opponent, metaDecks);
		}

		public List<Action<PredictionInfo>> OnPredictionUpdate = new List<Action<PredictionInfo>>();

		public void OnOpponentDraw()
		{
			_predictor.CheckOpponentClass();
			UpdatePrediction();
		}

		public void OnOpponentPlay(Card cardPlayed)
		{
			Log.Debug("cardPlayed: " + cardPlayed);
			_predictor.CheckOpponentCards();
			UpdatePrediction();
		}

		public void OnOpponentHandDiscard(Card cardDiscarded)
		{
			Log.Debug("cardDiscarded: " + cardDiscarded);
			_predictor.CheckOpponentCards();
			UpdatePrediction();
		}

		public void OnOpponentDeckDiscard(Card cardDiscarded)
		{
			Log.Debug("cardDiscarded: " + cardDiscarded);
			_predictor.CheckOpponentCards();
			UpdatePrediction();
		}

		public void OnOpponentSecretTriggered(Card secretTriggered)
		{
			Log.Debug("secretTriggered: " + secretTriggered);
			_predictor.CheckOpponentCards();
			UpdatePrediction();
		}

		public void OnOpponentJoustReveal(Card cardRevealed)
		{
			Log.Debug("cardRevealed: " + cardRevealed);
			_predictor.CheckOpponentCards();
			UpdatePrediction();
		}

		public void OnOpponentDeckToPlay(Card cardPlayed)
		{
			Log.Debug("cardPlayed: " + cardPlayed);
			_predictor.CheckOpponentCards();
			UpdatePrediction();
		}

		private void UpdatePrediction()
		{
			var playedCards = _opponent.KnownCards
				.Where(card => !card.IsCreated && card.Collectible && !card.Jousted);
			var predictionInfo =
				new PredictionInfo(_predictor.PossibleDecks.Count, _predictor.PossibleCards.Count);
			_predictor.PredictedCards
				.GroupBy(predictedCard => predictedCard.Card.Id, predictedCard => predictedCard)
				.ToList()
				.ForEach(group =>
				{
					var card = Database.GetCardFromId(group.Key);
					var playedCard = playedCards.FirstOrDefault(c => c.Id == card.Id);
					var playedCount = playedCard != null ? playedCard.Count : 0;
					List<double> probabilities =
						group.Select(predictedCard => predictedCard.Probability).ToList();
					predictionInfo.AddCardInfo(card, probabilities, playedCount);
				});

			OnPredictionUpdate.ForEach(callback => callback.Invoke(predictionInfo));
		}

	}
}
