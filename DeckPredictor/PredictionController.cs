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
			// Make CardInfos for all cards that have already been played
			var cardInfos = _opponent.KnownCards
				.Where(card => !card.Jousted) // Jousted cards have not yet been played
				.Select(card =>
				{
					var playedCard = Database.GetCardFromId(card.Id);
					playedCard.Count = card.Count;
					playedCard.IsCreated = card.IsCreated;
					return new PredictionInfo.CardInfo(playedCard, playedCard.Count);
				}).ToList();

			// Get the top predicted cards from the original deck list and group them together by id.
			// Then find the ones that have already been played and update their probabilities.
			// Otherwise, make a new CardInfo with the predicted cards.
			_predictor.PredictedCards
				.GroupBy(predictedCard => predictedCard.Card.Id, predictedCard => predictedCard)
				.ToList()
				.ForEach(group =>
				{
					// Find a played card that started in the original deck
					var playedCardInfo = cardInfos.FirstOrDefault(cardInfo => cardInfo.Card.Id == group.Key
						&& cardInfo.Card.Collectible && !cardInfo.Card.IsCreated);
					var probabilities = group.Select(predictedCard => predictedCard.Probability).ToList();
					int numPredictedCards = probabilities.Count;
					if (playedCardInfo != null)
					{
						playedCardInfo.Card.Count = numPredictedCards;
						playedCardInfo.Probabilities.AddRange(probabilities);
					} else
					{
						// This predicted card hasn't been played yet.
						var card = Database.GetCardFromId(group.Key);
						card.Count = numPredictedCards;
						cardInfos.Add(new PredictionInfo.CardInfo(card, probabilities, 0));
					}
				});

			var predictionInfo =
				new PredictionInfo(_predictor.PossibleDecks.Count, _predictor.PossibleCards.Count, cardInfos
					.OrderBy(cardInfo => cardInfo.Card.Cost)
					.ThenBy(cardInfo => cardInfo.Card.Name)
					.ToList());
			OnPredictionUpdate.ForEach(callback => callback.Invoke(predictionInfo));
		}

	}
}
