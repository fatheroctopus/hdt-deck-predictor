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
		private const int DeckSize = 30;
		// We never show cards below this probability.
		private const decimal ProbabilityHardCutoff = .4m;

		private List<Deck> _possibleDecks;
		private Dictionary<string, CardInfo> _possibleCards =
			new Dictionary<string, CardInfo>();
		private List<CardInfo> _predictedCards;
		private List<CardInfo> _nextPredictedCards;
		private IOpponent _opponent;
		private bool _classDetected;

		public Predictor(IOpponent opponent, ReadOnlyCollection<Deck> metaDecks)
		{
			Log.Debug("Copying possible decks from the current meta");
			_possibleDecks = new List<Deck>(metaDecks);
			_opponent = opponent;
			CheckOpponentClass();
		}

		public ReadOnlyCollection<Deck> PossibleDecks =>
			new ReadOnlyCollection<Deck>(_possibleDecks);

		// List of all possible cards that could be in the opponent's deck
		public ICollection<CardInfo> PossibleCards => _possibleCards.Values;

		// Sorted list of most likeley cards to be in opponent's deck, under the deck limit.
		public List<CardInfo> PredictedCards => new List<CardInfo>(_predictedCards);

		// Sorted list of the next most likely cards after the top 30 cutoff.
		public ReadOnlyCollection<CardInfo> GetNextPredictedCards(int numCards) =>
			new ReadOnlyCollection<CardInfo>(_nextPredictedCards.Take(numCards).ToList());

		// Returns null if the given card and copyCount have no chance to be in the opponent's deck.
		public CardInfo GetPredictedCard(Card card, int copyCount)
		{
			string key = CardInfo.Key(card, copyCount);
			return GetPredictedCard(key);
		}

		public CardInfo GetPredictedCard(string key)
		{
			if (_possibleCards.ContainsKey(key))
			{
				return _possibleCards[key];
			}
			return null;
		}

		public void CheckOpponentClass()
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

		public void CheckOpponentCards()
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
						var key = CardInfo.Key(card, copyCount);
						if (!_possibleCards.ContainsKey(key))
						{
							var predictedCard = new CardInfo(card, copyCount, _possibleDecks.Count);
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
			decimal insufficientProbability = _nextPredictedCards.Count > DeckSize
				? _nextPredictedCards.ElementAt(DeckSize).Probability
				: 0;
			_predictedCards = _nextPredictedCards
				.Take(DeckSize)
				.TakeWhile(predictedCard => predictedCard.Probability > insufficientProbability &&
					predictedCard.Probability >= ProbabilityHardCutoff)
				.OrderBy(predictedCard => predictedCard.Card.Cost)
				.ThenBy(predictedCard => predictedCard.Card.Name)
				.ToList();

			// Shave off the cards we moved to _predictedCards from our _nextPredictedCards
			_nextPredictedCards.RemoveRange(0, _predictedCards.Count);

			Log.Debug(_possibleCards.Count + " possible cards");
			Log.Debug(_predictedCards.Count + " predicted cards");
		}

		public class CardInfo
		{
			private int _numOccurrences;
			private int _numPossibleDecks;

			public CardInfo(Card card, int copyCount, int numPossibleDecks)
			{
				Card = card;
				CopyCount = copyCount;
				_numOccurrences = 0;
				_numPossibleDecks = numPossibleDecks;
			}

			public Card Card { get; }

			// Track each copy of a card separately in the deck.
			// This is 1-indexed to mirror Card.Count
			public int CopyCount { get; }

			public void IncrementNumOccurrences()
			{
				_numOccurrences++;
			}

			public decimal Probability => (decimal)_numOccurrences / _numPossibleDecks;

			public string Key() => Key(Card, CopyCount);

			public static string Key(Card card, int copyCount) => card.Id + copyCount;
		}
	}

}
