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
		public static readonly int DeckSize = 30;
		private static readonly double Epsilon = .00001;

		private List<Deck> _possibleDecks;
		private Dictionary<string, PredictedCardInfo> _possibleCards =
			new Dictionary<string, PredictedCardInfo>();
		private List<PredictedCardInfo> _predictedCards;
		private List<PredictedCardInfo> _nextPredictedCards;
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

		// List of all possible cards that could be in the opponent's deck
		public ICollection<PredictedCardInfo> PossibleCards => _possibleCards.Values;

		// Sorted list of most likeley cards to be in opponent's deck, under the deck limit.
		public ReadOnlyCollection<PredictedCardInfo> PredictedCards =>
			new ReadOnlyCollection<PredictedCardInfo>(_predictedCards);

		// Sorted list of the next most likely cards after the top 30 cutoff.
		public ReadOnlyCollection<PredictedCardInfo> GetNextPredictedCards(int numCards) =>
			new ReadOnlyCollection<PredictedCardInfo>(_nextPredictedCards.Take(numCards).ToList());

		// Returns null if the given card and copyCount have no chance to be in the opponent's deck.
		public PredictedCardInfo GetPredictedCard(Card card, int copyCount)
		{
			string key = PredictedCardInfo.Key(card, copyCount);
			return GetPredictedCard(key);
		}

		public PredictedCardInfo GetPredictedCard(string key)
		{
			if (_possibleCards.ContainsKey(key))
			{
				return _possibleCards[key];
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
			// Determine which cards are possible.
			_possibleCards.Clear();
			foreach (Deck deck in _possibleDecks)
			{
				foreach (Card card in deck.Cards)
				{
					for (int copyCount = 1; copyCount <= card.Count; copyCount++)
					{
						var key = PredictedCardInfo.Key(card, copyCount);
						if (!_possibleCards.ContainsKey(key))
						{
							var predictedCard = new PredictedCardInfo(card, copyCount, _possibleDecks.Count);
							_possibleCards[key] = predictedCard;
						}
						_possibleCards[key].IncrementNumOccurrences();
					}
				}
			}

			// Prediction
			// First sort possible cards by probability
			_nextPredictedCards = _possibleCards.Values
				.OrderByDescending(predictedCard => predictedCard.Probability)
				.ToList();
			// If our list is greater than the Deck Size, take the probability of the first card that won't
			// make the cut.  All other cards we predict have to be strictly greater than that probability.
			// We do this so none of the top 30 are there for an arbitrary reason.
			double cutOffProbability = _nextPredictedCards.Count > DeckSize
				? _nextPredictedCards.ElementAt(DeckSize).Probability + Epsilon
				: 0;
			_predictedCards = _nextPredictedCards
				.Take(DeckSize)
				.TakeWhile(predictedCard => predictedCard.Probability > cutOffProbability)
				.OrderBy(predictedCard => predictedCard.Card.Cost)
				.ToList();

			// Shave off the cards we moved to _predictedCards from our _nextPredictedCards
			_nextPredictedCards.RemoveRange(0, _predictedCards.Count);

			Log.Info(_possibleCards.Count + " possible cards");
			Log.Info(_predictedCards.Count + " predicted cards");
			foreach (Action<Predictor> callback in OnPredictionUpdate)
			{
				callback.Invoke(this);
			}
		}

	}

}
