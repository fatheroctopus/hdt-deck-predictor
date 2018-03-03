using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace DeckPredictor
{
	public class PredictionInfo
	{
		private List<CardInfo> _cardInfos { get; } = new List<CardInfo>();

		public PredictionInfo(int numPossibleDecks, int numPossibleCards)
		{
			NumPossibleDecks = numPossibleDecks;
			NumPossibleCards = numPossibleCards;
		}

		public int NumPossibleDecks { get; }

		public int NumPossibleCards { get; }

		public int NumPredictedCards => _cardInfos.Sum(cardInfo => cardInfo.Probabilities.Count);

		public void AddCardInfo(Card card, List<double> probabilities, int numPlayed) =>
			_cardInfos.Add(new CardInfo(card, probabilities, numPlayed));

		public List<Card> UnplayedCards =>
			_cardInfos.Select(cardInfo =>
				{
					var card = Database.GetCardFromId(cardInfo.Card.Id);
					card.Count = cardInfo.Probabilities.Count - cardInfo.NumPlayed;
					return card;
				})
				.Where(card => card.Count > 0)
				.ToList();

		public void WritePrediction(TextWriter writer)
		{
			writer.WriteLine(NumPossibleDecks + " possible decks");
			writer.WriteLine(NumPossibleCards + " possible cards");
			writer.WriteLine("");

			writer.WriteLine(NumPredictedCards + " predicted cards:");
			_cardInfos.ForEach(cardInfo => writer.WriteLine(cardInfo.ToString()));
			// writer.WriteLine("");

			// var nextPredictedCards = predictor.GetNextPredictedCards(3);
			// writer.WriteLine("Next " + nextPredictedCards.Count + " most likely cards:");
			// foreach (PredictedCardInfo predictedCard in nextPredictedCards)
			// {
			// 	writer.WriteLine(predictedCard.ToString());
			// }
		}

		private class CardInfo
		{
			public Card Card { get; }
			public List<double> Probabilities { get; }
			public int NumPlayed { get; }

			public CardInfo(Card card, List<double> probabilities, int numPlayed)
			{
				Card = card;
				Probabilities = probabilities;
				NumPlayed = numPlayed;
			}

			public override string ToString()
			{
				List<string> probStrings = new List<string>();
				for (int n = 0; n < Probabilities.Count; n++)
				{
					probStrings.Add(n + 1 <= NumPlayed ? "XX"
						: Math.Truncate(Probabilities[n] * 100) + "%");
				}

				return "[" + Card.Cost + "] " +
					Card.Name + "(" + Probabilities.Count + ")" +
					" - " + String.Join(" / ", probStrings);
			}
		}
	}
}
