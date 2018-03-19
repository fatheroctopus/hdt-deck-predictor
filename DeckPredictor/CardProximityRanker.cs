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
			_proximityLog = new CustomLog(LogName, this);
			_proximityLog.Write();
		}

		// Ranks the provided cards by their proximity to each other based on possible decks and returns
		// a list ordered by this ranking. (Closest proximity first)
		// In the parameter list, Card.Count represents the number of copies of that Card.
		// In the return list, multiple copies of each card are split up where Card.Count is the copy count.
		public List<Card> RankCards(List<Card> cards)
		{
			// Iterate over each card and copy to find the ones we haven't rated yet.
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
			_proximityLog.Write();
			return _rankedCards.Select(ratedCard => ratedCard.Card).ToList();
		}

		public void OnWrite(TextWriter writer)
		{
			writer.WriteLine(_decks.Count + " decks in proximity space");
			writer.WriteLine("");
			writer.WriteLine(_rankedCards.Count + " cards ranked:");
			foreach (CardInfo cardInfo in _rankedCards)
			{
				writer.WriteLine(cardInfo.ProximityRating + " - " + cardInfo.Card.ToString());
				List<string> statStrings = new List<string>();
				statStrings.Add("Solo: " + cardInfo.SoloRating);
				if (cardInfo.ProximityByCard.Values.Count > 0)
				{
					statStrings.Add(
						"Avg: " + string.Format("{0:0.00}", cardInfo.ProximityByCard.Values.Average()));
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
		}
	}
}
