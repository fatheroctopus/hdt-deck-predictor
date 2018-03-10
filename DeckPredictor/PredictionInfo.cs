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
		public PredictionInfo(int numPossibleDecks, int numPossibleCards,
			List<CardInfo> predictedCards, List<CardInfo> runnerUpCards)
		{
			NumPossibleDecks = numPossibleDecks;
			NumPossibleCards = numPossibleCards;
			PredictedCards = predictedCards;
			RunnerUpCards = runnerUpCards;
		}

		public int NumPossibleDecks { get; }

		public int NumPossibleCards { get; }

		public List<CardInfo> PredictedCards { get; }

		public List<CardInfo> RunnerUpCards { get; }

		public int NumPredictedCards => PredictedCards.Sum(cardInfo => cardInfo.Probabilities.Count);

		public void WritePrediction(TextWriter writer)
		{
			writer.WriteLine(NumPossibleDecks + " possible decks");
			writer.WriteLine(NumPossibleCards + " possible cards");
			writer.WriteLine("");

			writer.WriteLine(NumPredictedCards + " predicted cards:");
			PredictedCards.ForEach(cardInfo => writer.WriteLine(cardInfo.ToString()));
			writer.WriteLine("");

			writer.WriteLine("Next " + RunnerUpCards.Count + " most likely cards:");
			RunnerUpCards.ForEach(cardInfo => writer.WriteLine(cardInfo.ToString()));
		}

		public class CardInfo
		{
			public Card Card { get; }
			public List<decimal> Probabilities { get; }
			public int NumPlayed { get; }

			public CardInfo(Card card, List<decimal> probabilities, int numPlayed)
			{
				Card = card;
				Probabilities = probabilities;
				NumPlayed = numPlayed;
			}

			public CardInfo(Card card, int numPlayed) : this(card, new List<decimal>(), numPlayed) {}

			public Card GetCardWithUnplayedCount()
			{
				var card = Database.GetCardFromId(Card.Id);
				card.Count = Card.Count - NumPlayed;
				card.IsCreated = Card.IsCreated;
				return card;
			}

			public override string ToString()
			{
				var createdString = Card.IsCreated ? "[C]" : "";
				return "[" + Card.Cost + "] " +
					Card.Name + "(" + Card.Count + ")" +
					createdString + " - " + GetPercentageString();
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
