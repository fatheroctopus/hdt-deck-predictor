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
		public PredictionInfo(int numPossibleDecks, int numPossibleCards, List<CardInfo> cardInfos)
		{
			NumPossibleDecks = numPossibleDecks;
			NumPossibleCards = numPossibleCards;
			CardInfos = cardInfos;
		}

		public int NumPossibleDecks { get; }

		public int NumPossibleCards { get; }

		public List<CardInfo> CardInfos { get; }

		public int NumPredictedCards => CardInfos.Sum(cardInfo => cardInfo.Probabilities.Count);

		public void WritePrediction(TextWriter writer)
		{
			writer.WriteLine(NumPossibleDecks + " possible decks");
			writer.WriteLine(NumPossibleCards + " possible cards");
			writer.WriteLine("");

			writer.WriteLine(NumPredictedCards + " predicted cards:");
			CardInfos.ForEach(cardInfo => writer.WriteLine(cardInfo.ToString()));
			// writer.WriteLine("");

			// var nextPredictedCards = predictor.GetNextPredictedCards(3);
			// writer.WriteLine("Next " + nextPredictedCards.Count + " most likely cards:");
			// foreach (PredictedCardInfo predictedCard in nextPredictedCards)
			// {
			// 	writer.WriteLine(predictedCard.ToString());
			// }
		}

		public class CardInfo
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

			public CardInfo(Card card, int numPlayed) : this(card, new List<double>(), numPlayed) {}

			public Card GetCardWithUnplayedCount()
			{
				var card = Database.GetCardFromId(Card.Id);
				card.Count = Probabilities.Count - NumPlayed;
				return card;
			}

			public override string ToString()
			{
				return "[" + Card.Cost + "] " +
					Card.Name + "(" + Probabilities.Count + ")" +
					" - " + GetPercentageString();
			}

			private string GetPercentageString()
			{
				List<string> probStrings = new List<string>();
				for (int n = 0; n < Probabilities.Count; n++)
				{
					probStrings.Add(n + 1 <= NumPlayed ? "XX"
						: Math.Truncate(Probabilities[n] * 100) + "%");
				}
				return String.Join(" / ", probStrings);
			}
		}
	}
}
