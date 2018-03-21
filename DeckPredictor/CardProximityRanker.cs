using HearthMirror;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System;

namespace DeckPredictor
{
	public class CardProximityRanker : CustomLog.ILogProvider
	{
		private const string LogName = "proximity.txt";
		private List<Deck> _decks;
		private Dictionary<string, CardInfo> _ratedCards = new Dictionary<string, CardInfo>();
		private List<CardInfo> _rankedCards = new List<CardInfo>();
		private CustomLog _proximityLog;

		public CardProximityRanker(List<Deck> decks)
		{
			_decks = decks;
			_proximityLog = new CustomLog(LogName);
			_proximityLog.Write(this);
		}

		// The current list of cards ranked by closest proximity to the other cards in the list.
		// Multiple copies of each card are split up where Card.Count is the copy count.
		public List<Card> RankedCards => _rankedCards.Select(ratedCard => ratedCard.Card).ToList();

		// Updates the current list of cards that should be ranked by proximity.
		// For each card, Card.Count represents the number of copies of that Card.
		// Returns true if this changes the current state of RankedCards.
		public bool UpdateCards(List<Card> cards)
		{
			// Iterate over each card and copy to find the ones we haven't rated yet.
			bool newCardFound = false;
			foreach (Card newCard in cards)
			{
				for (int copyCount = 1; copyCount <= newCard.Count; copyCount++)
				{
					// Check if this card has already been rated.
					string newKey = CardInfo.Key(newCard, copyCount);
					if (!_ratedCards.ContainsKey(newKey))
					{
						Card cardByCopy = Database.GetCardFromId(newCard.Id);
						cardByCopy.Count = copyCount;
						newCardFound = true;
						Log.Debug("Ranking new card: " + cardByCopy);

						// Find the decks that contain this new card.
						var decks = FilterDecksByCard(_decks, newCard);

						// Solo rating is the number of decks it is present in.
						int soloRating = decks.Count;
						var newCardInfo = new CardInfo(cardByCopy, soloRating);

						// Find the new card's proximity to each card already rated.
						foreach (var pair in _ratedCards)
						{
							CardInfo ratedCardInfo = pair.Value;
							// Proximity rating between two cards is how many decks contain both.
							int proximityRating = FilterDecksByCard(decks, ratedCardInfo.Card).Count;
							// Update the dictionary in both CardInfos.
							ratedCardInfo.ProximityByCard[newKey] = proximityRating;
							newCardInfo.ProximityByCard[pair.Key] = proximityRating;
						}

						// This card is now rated.
						_ratedCards[newKey] = newCardInfo;
					}
				}
			}

			// Sort our rated cards first by best proximity rating, then by its solo rating.
			_rankedCards = _ratedCards.Values
				.OrderByDescending(ratedCard => ratedCard.ProximityRating)
				.ThenByDescending(ratedCard => ratedCard.SoloRating)
				.ThenBy(ratedCard => ratedCard.Card.Count)
				.ToList();
			_proximityLog.Write(this);
			return newCardFound;
		}

		public void OnWriteLog(TextWriter writer)
		{
			writer.WriteLine(_decks.Count + " deck(s) in proximity space");
			writer.WriteLine("");
			writer.WriteLine(_rankedCards.Count + " card(s) ranked:");
			foreach (CardInfo cardInfo in _rankedCards)
			{
				writer.WriteLine(cardInfo.ProximityRating + " - " + cardInfo.Card.ToString());
				List<string> statStrings = new List<string>();
				statStrings.Add("Solo: " + cardInfo.SoloRating);
				if (cardInfo.ProximityByCard.Values.Count > 0)
				{
					statStrings.Add(
						"Avg: " + string.Format("{0:0.00}", cardInfo.ProximityByCard.Values.Average()));
					statStrings.Add("Med: " + string.Format("{0:0.00}", cardInfo.MedianProximity));
					statStrings.Add("Min: " + cardInfo.ProximityByCard.Values.Min());
					statStrings.Add("Max: " + cardInfo.ProximityByCard.Values.Max());
				}
				writer.WriteLine("     " + String.Join(" / ", statStrings));
			}
		}

		private static List<Deck> FilterDecksByCard(List<Deck> decks, Card card) =>
			decks.Where(deck =>
				{
					var cardInDeck = deck.Cards.FirstOrDefault(x => x.Id == card.Id);
					return cardInDeck != null && card.Count <= cardInDeck.Count;
				}).ToList();

		private class CardInfo
		{

			public CardInfo(Card card, int soloRating)
			{
				Card = card;
				SoloRating = soloRating;
				ProximityByCard = new Dictionary<string, int>();
			}

			public Card Card { get; }

			public int SoloRating { get; }
			public Dictionary<string, int> ProximityByCard { get; }

			public static string Key(Card card, int copyCount) => card.Id + copyCount;
			public string Key() => Key(Card, Card.Count);

			public int ProximityRating => ProximityByCard.Values.Sum();

			public double MedianProximity
			{
				get
				{
					int count = ProximityByCard.Count();
					var orderedProximities = ProximityByCard.Values.OrderBy(prox => prox).ToList();
					if (count % 2 == 1)
					{
						return orderedProximities[count / 2];
					}
					else
					{
						double median = orderedProximities[count / 2] + orderedProximities[(count - 1) / 2];
						median /= 2;
						return median;
					}
				}
			}
		}
	}
}
