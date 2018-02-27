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
		private HashSet<Card> _possibleCards = new HashSet<Card>();
		private IOpponent _opponent;
		private bool _classDetected;

		public Predictor(IOpponent opponent, ReadOnlyCollection<Deck> metaDecks)
		{
			Log.Debug("Copying possible decks from the current meta");
			_possibleDecks = new List<Deck>(metaDecks);
			_opponent = opponent;
			UpdatePossibleCards(false);
		}

		public void OnGameStart()
		{
			CheckOpponentClass();
		}

		public void OnOpponentDraw()
		{
			CheckOpponentClass();
		}

		public ReadOnlyCollection<Deck> PossibleDecks =>
			new ReadOnlyCollection<Deck>(_possibleDecks);

		public ISet<Card> PossibleCards => new HashSet<Card>(_possibleCards);

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
			UpdatePossibleCards();
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
				UpdatePossibleCards();
			}
		}

		public void UpdatePossibleCards(bool logEachCard = true)
		{
			_possibleCards.Clear();
			foreach (Deck deck in _possibleDecks)
			{
				foreach (Card card in deck.Cards)
				{
					// Remove an existing card with a lower card count than this one.
					var alreadyExistingCard = _possibleCards.FirstOrDefault(x => x.Id == card.Id);
					if (alreadyExistingCard != null && alreadyExistingCard.Count < card.Count)
					{
						_possibleCards.Remove(alreadyExistingCard);
					}
					_possibleCards.Add(card);
				}
			}
			Log.Info(_possibleCards.Count + " possible different cards");
			if (logEachCard)
			{
				foreach (Card possibleCard in _possibleCards)
				{
					Log.Debug(possibleCard.ToString());
				}
			}
		}
	}

}
