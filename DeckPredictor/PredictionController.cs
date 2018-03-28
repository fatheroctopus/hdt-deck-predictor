using Hearthstone_Deck_Tracker.Enums;
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
		private const string LogName = "prediction.txt";
		private IOpponent _opponent;
		private Predictor _predictor;
		private bool _firstOpponentDraw = true;
		private CustomLog _predictionLog;

		public PredictionController(IOpponent opponent, ReadOnlyCollection<Deck> metaDecks)
		{
			_opponent = opponent;
			_predictor = new Predictor(opponent, metaDecks);
			_predictionLog = new CustomLog(LogName);
		}

		public List<Action<PredictionInfo>> OnPredictionUpdate = new List<Action<PredictionInfo>>();

		public void OnOpponentDraw()
		{
			if (_firstOpponentDraw)
			{
				// This draw is mulligan, so check the values that weren't initialized at Game Start.
				// (i.e. - hero class, mana crystals)
				_predictor.CheckOpponentClass();
				_predictor.CheckOpponentMana();
				UpdatePrediction();
				_firstOpponentDraw = false;
			}
		}

		public void OnTurnStart(ActivePlayer player)
		{
			Log.Info("OnTurnStart: " + player);
			// At the beginning of the player turn, update the opponent's available mana for the next turn.
			if (player == ActivePlayer.Player)
			{
				_predictor.CheckOpponentMana();
				UpdatePrediction();
			}
		}

		public void OnOpponentPlay(Card cardPlayed)
		{
			Log.Info("cardPlayed: " + cardPlayed);
			_predictor.CheckOpponentCards();
			UpdatePrediction();
		}

		public void OnOpponentHandDiscard(Card cardDiscarded)
		{
			Log.Info("cardDiscarded: " + cardDiscarded);
			_predictor.CheckOpponentCards();
			UpdatePrediction();
		}

		public void OnOpponentDeckDiscard(Card cardDiscarded)
		{
			Log.Info("cardDiscarded: " + cardDiscarded);
			_predictor.CheckOpponentCards();
			UpdatePrediction();
		}

		public void OnOpponentSecretTriggered(Card secretTriggered)
		{
			Log.Info("secretTriggered: " + secretTriggered);
			_predictor.CheckOpponentCards();
			UpdatePrediction();
		}

		public void OnOpponentJoustReveal(Card cardRevealed)
		{
			Log.Info("cardRevealed: " + cardRevealed);
			_predictor.CheckOpponentCards();
			UpdatePrediction();
		}

		public void OnOpponentDeckToPlay(Card cardPlayed)
		{
			Log.Info("cardPlayed: " + cardPlayed);
			_predictor.CheckOpponentCards();
			UpdatePrediction();
		}

		public void UpdatePrediction()
		{
			// Make CardInfos for all cards that have already been played
			var cardInfos = _opponent.KnownCards
				.Select(card =>
				{
					var playedCard = Database.GetCardFromId(card.Id);
					playedCard.Count = card.Count;
					playedCard.IsCreated = card.IsCreated;
					var numPlayed = card.Jousted ? 0 : playedCard.Count;
					return new PredictionInfo.CardInfo(playedCard, numPlayed);
				}).ToList();

			// Get the predicted cards from the original deck list and group them together by id.
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
						playedCardInfo.Card.Count = Math.Max(numPredictedCards, playedCardInfo.Card.Count);
						playedCardInfo.Probabilities.AddRange(probabilities);
					}
					else
					{
						// This predicted card hasn't been played yet.
						var card = Database.GetCardFromId(group.Key);
						card.Count = numPredictedCards;
						cardInfos.Add(new PredictionInfo.CardInfo(card, probabilities, 0));
					}
				});

			var predictedCards = cardInfos
					.OrderBy(cardInfo => cardInfo.Card.Cost)
					.ThenBy(cardInfo => cardInfo.Card.Name)
					.ThenBy(cardInfo => cardInfo.Card.IsCreated)
					.ToList();
			var runnerUps = _predictor.GetNextPredictedCards(30).Select(cardInfo =>
				{
					// Don't group runnerUps, they all should have a count of 1 and are unplayed.
					var card = Database.GetCardFromId(cardInfo.Card.Id);
					var probabilities = new List<decimal> {cardInfo.Probability};
					return new PredictionInfo.CardInfo(card, probabilities, 0);
				}).ToList();

			var predictionInfo = new PredictionInfo(
				_predictor.PossibleDecks.Count, _predictor.PossibleCards.Count,
				_predictor.AvailableMana, _predictor.AvailableManaWithCoin, predictedCards, runnerUps);
			_predictionLog.Write(predictionInfo);
			OnPredictionUpdate.ForEach(callback => callback.Invoke(predictionInfo));
		}

	}
}
