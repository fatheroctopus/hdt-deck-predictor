using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;

namespace DeckPredictor
{
	public class Predictor
	{
		private List<Deck> _possibleDecks;
		private Dictionary<string, PredictedCardInfo> _predictedCards =
			new Dictionary<string, PredictedCardInfo>();
		private List<PredictedCardInfo> _predictedCardsByProbablity;
		private IOpponent _opponent;
		private bool _classDetected;

		public Predictor(IOpponent opponent, ReadOnlyCollection<Deck> metaDecks)
		{
			Log.Debug("Copying possible decks from the current meta");
			_possibleDecks = new List<Deck>(metaDecks);
			_opponent = opponent;
			UpdatePredictedCards();
		}

		public List<Action<Predictor>> OnPredictionUpdate = new List<Action<Predictor>>();

		public ReadOnlyCollection<Deck> PossibleDecks =>
			new ReadOnlyCollection<Deck>(_possibleDecks);

		public ReadOnlyCollection<PredictedCardInfo> PredictedCards =>
			new ReadOnlyCollection<PredictedCardInfo>(_predictedCardsByProbablity);

		// Returns null if the given card and copyCount are not predicted to be in the opponent's deck.
		public PredictedCardInfo GetPredictedCard(Card card, int copyCount)
		{
			string key = PredictedCardInfo.Key(card, copyCount);
			return GetPredictedCard(key);
		}

		public PredictedCardInfo GetPredictedCard(string key)
		{
			if (_predictedCards.ContainsKey(key))
			{
				return _predictedCards[key];
			}
			return null;
		}

		public void OnGameStart()
		{
			CheckOpponentClass();
		}

		public void OnOpponentDraw()
		{
			CheckOpponentClass();
		}

		public void OnOpponentPlay(Card cardPlayed)
		{
			Log.Debug("cardPlayed: " + cardPlayed);
			FilterAllRevealedCards();
		}

		public void OnOpponentHandDiscard(Card cardDiscarded)
		{
			Log.Debug("cardDiscarded: " + cardDiscarded);
			FilterAllRevealedCards();
		}

		public void OnOpponentDeckDiscard(Card cardDiscarded)
		{
			Log.Debug("cardDiscarded: " + cardDiscarded);
			FilterAllRevealedCards();
		}

		public void OnOpponentSecretTriggered(Card secretTriggered)
		{
			Log.Debug("secretTriggered: " + secretTriggered);
			FilterAllRevealedCards();
		}

		public void OnOpponentJoustReveal(Card cardRevealed)
		{
			Log.Debug("cardRevealed: " + cardRevealed);
			FilterAllRevealedCards();
		}

		public void OnOpponentDeckToPlay(Card cardPlayed)
		{
			Log.Debug("cardPlayed: " + cardPlayed);
			FilterAllRevealedCards();
		}

		private void CheckOpponentClass()
		{
			if (_classDetected)
			{
				return;
			}
			if (string.IsNullOrEmpty(_opponent.Class))
			{
				return;
			}
			// Only want decks for the opponent's class.
			_possibleDecks = _possibleDecks.Where(x => x.Class == _opponent.Class).ToList();
			_classDetected = true;
			Log.Info(_possibleDecks.Count + " possible decks for class " + _opponent.Class);
			UpdatePredictedCards();
		}

		private void FilterAllRevealedCards()
		{
			var missingCards = new HashSet<Card>();
			var insufficientCards = new HashSet<Card>();
			_possibleDecks = _possibleDecks
				.Where(possibleDeck =>
				{
					foreach (Card knownCard in _opponent.KnownCards)
					{
						if (knownCard.IsCreated)
						{
							continue;
						}
						if (!knownCard.Collectible)
						{
							continue;
						}
						var cardInPossibleDeck =
							possibleDeck.Cards.FirstOrDefault(x => x.Id == knownCard.Id);
						if (cardInPossibleDeck == null)
						{
							missingCards.Add(knownCard);
							return false;
						}
						if (knownCard.Count > cardInPossibleDeck.Count)
						{
							insufficientCards.Add(knownCard);
							return false;
						}
					}
					return true;
				}).ToList();
			// If PossibleDecks have changed.
			if (missingCards.Any() || insufficientCards.Any())
			{
				foreach (Card card in missingCards)
				{
					Log.Debug("Filtering out decks missing card: " + card);
				}
				foreach (Card card in insufficientCards)
				{
					Log.Debug("Filtering out decks that don't run enough copies of "+ card);
				}
				Log.Info(_possibleDecks.Count + " possible decks");
				UpdatePredictedCards();
			}
		}

		private void UpdatePredictedCards()
		{
			_predictedCards.Clear();
			foreach (Deck deck in _possibleDecks)
			{
				foreach (Card card in deck.Cards)
				{
					for (int copyCount = 1; copyCount <= card.Count; copyCount++)
					{
						var key = PredictedCardInfo.Key(card, copyCount);
						if (!_predictedCards.ContainsKey(key))
						{
							var predictedCard = new PredictedCardInfo(card, copyCount, _possibleDecks.Count);
							_predictedCards[key] = predictedCard;
						}
						_predictedCards[key].IncrementNumOccurrences();
					}
				}
			}

			_predictedCardsByProbablity = _predictedCards.Values
				.OrderByDescending(predictedCard => predictedCard.Probability)
				.ThenBy(predictedCard => predictedCard.Card.Cost)
				.ToList();

			Log.Info(_predictedCards.Count + " predicted cards");
			foreach (Action<Predictor> callback in OnPredictionUpdate)
			{
				callback.Invoke(this);
			}
		}

	}

}
