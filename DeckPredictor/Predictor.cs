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

		private List<Deck> _classDecks;
		private List<Deck> _possibleDecks;
		private Dictionary<string, CardInfo> _possibleCards =
			new Dictionary<string, CardInfo>();
		private List<CardInfo> _predictedCards;
		private List<CardInfo> _nextPredictedCards;
		private IOpponent _opponent;
		private bool _classDetected;
		private CardProximityRanker _proximityRanker;

		public Predictor(IOpponent opponent, ReadOnlyCollection<Deck> metaDecks)
		{
			Log.Debug("Copying possible decks from the meta");
			_classDecks = new List<Deck>(metaDecks);
			_possibleDecks = new List<Deck>(metaDecks);
			_opponent = opponent;
			CheckOpponentClass();
			CheckOpponentMana();
		}

		// We always show a card at this probability or higher.
		public decimal ProbabilityAlwaysInclude { get; set; } = .65m;
		// We show a card at this probability if the opponent has enough mana to play them.
		public decimal ProbabilityIncludeIfPlayable { get; set; } = .50m;
		// We show a card at this probability if the opponent could play it and spend all their mana.
		public decimal ProbabilityIncludeIfOptimal { get; set; } = .30m;

		public int AvailableMana { get; private set; } = 0;
		public int AvailableManaWithCoin { get; private set; } = 0;

		public ReadOnlyCollection<Deck> PossibleDecks =>
			new ReadOnlyCollection<Deck>(_possibleDecks);

		// List of all possible cards that could be in the opponent's deck
		public ICollection<CardInfo> PossibleCards => _possibleCards.Values;

		// Sorted list of most likeley cards to be in opponent's deck, under the deck limit.
		public List<CardInfo> PredictedCards => new List<CardInfo>(_predictedCards);

		// Sorted list of the next most likely cards not in the current prediction.
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
			_classDetected = true;
			_classDecks = _possibleDecks.Where(x => x.Class == _opponent.Class).ToList();
			_proximityRanker = new CardProximityRanker(_classDecks);
			_possibleDecks = new List<Deck>(_classDecks);
			Log.Info(_possibleDecks.Count + " possible decks for class " + _opponent.Class);
			UpdatePredictedCards();
		}

		public void CheckOpponentCards()
		{
			if (!_classDetected)
			{
				Log.Warn("Cannot CheckOpponentCards before opponent class has been detected");
				return;
			}

			// For prediction, we only care about cards that could have started in the opponent's deck.
			var knownCards = _opponent.KnownCards.Where(card => !card.IsCreated && card.Collectible).ToList();
			bool hasNewRanking = _proximityRanker.UpdateCards(knownCards);
			if (!hasNewRanking)
			{
				return;
			}

			_possibleDecks = new List<Deck>(_classDecks);
			// Go through each ranked card and filter the decks down.
			foreach (Card orderedCard in _proximityRanker.RankedCards)
			{
				var possibleDecks = _possibleDecks.Where(possibleDeck =>
					{
						var cardInPossibleDeck =
							possibleDeck.Cards.FirstOrDefault(x => x.Id == orderedCard.Id);
						return cardInPossibleDeck != null && orderedCard.Count <= cardInPossibleDeck.Count;
					}).ToList();
				// If this card filters possidlbe decks down to zero, back up and ignore the remaining cards.
				// Those cards will be considered "off-meta".
				if (possibleDecks.Count == 0)
				{
					break;
				}
				_possibleDecks = possibleDecks;
			}

			UpdatePredictedCards();
		}

		public void CheckOpponentMana()
		{
			var availableMana = _opponent.GetAvailableManaNextTurn(false);
			var availableManaWithCoin = _opponent.GetAvailableManaNextTurn(true);
			if (availableMana != -1 &&
				(availableMana != AvailableMana || availableManaWithCoin != AvailableManaWithCoin))
			{
				Log.Info("Updating Opponent's available mana to " + availableMana +
					"(" + availableManaWithCoin + ")");
				AvailableMana = availableMana;
				AvailableManaWithCoin = availableManaWithCoin;
				if (_classDetected)
				{
					UpdatePredictedCards();
				}
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
			var sortedPossibleCards = _possibleCards.Values
				.OrderByDescending(predictedCard => predictedCard.Probability)
				.ThenBy(predictedCard => predictedCard.Card.Cost)
				.ThenBy(predictedCard => predictedCard.Card.Name)
				.ToList();

			// If our list is greater than the Deck Size, find the probability of the first card that won't
			// make the cut.  All other cards have to be strictly greater than that probability.
			// We do this so none of the top 30 are there for an arbitrary reason.
			decimal insufficientProbability = sortedPossibleCards.Count > DeckSize
				? sortedPossibleCards.ElementAt(DeckSize).Probability
				: 0;
			_predictedCards = sortedPossibleCards
				.TakeWhile(predictedCard => predictedCard.Probability > insufficientProbability &&
					predictedCard.Probability >= ProbabilityAlwaysInclude)
				.ToList();

			// Now go through the remaining possible cards to fill out the deck with speculative picks.
			decimal lastPickProbability = 1;
			_nextPredictedCards = new List<CardInfo>();
			sortedPossibleCards.Skip(_predictedCards.Count).ToList().ForEach(possibleCard =>
				{
					// A speculative card is only added if its probability is high enough and it passes
					// the check based on the opponent's available mana.
					// Cards are playable if they are less than or equal to available mana.
					// Cards are optimal if they are equal to available mana with and without the coin.
					bool isPlayable = possibleCard.Card.Cost <= AvailableManaWithCoin;
					bool playableCheck = isPlayable &&
						possibleCard.Probability >= ProbabilityIncludeIfPlayable;
					bool isOptimal = (possibleCard.Card.Cost == AvailableMana ||
						possibleCard.Card.Cost == AvailableManaWithCoin);
					bool optimalCheck = isOptimal &&
						possibleCard.Probability >= ProbabilityIncludeIfOptimal;
					// Go until the deck is filled, but allow in all valid cards at the same probability.
					if ((playableCheck || optimalCheck) &&
						(_predictedCards.Count < DeckSize || possibleCard.Probability >= lastPickProbability))
					{
						_predictedCards.Add(possibleCard);
						lastPickProbability = possibleCard.Probability;
					}
					else
					{
						_nextPredictedCards.Add(possibleCard);
					}
				});
			_predictedCards = _predictedCards
				.OrderBy(predictedCard => predictedCard.Card.Cost)
				.ThenBy(predictedCard => predictedCard.Card.Name)
				.ToList();

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
