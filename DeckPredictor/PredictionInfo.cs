using Hearthstone_Deck_Tracker.Hearthstone;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace DeckPredictor
{
	public class PredictionInfo
	{
		public PredictionInfo(int numPossibleDecks, int numPossibleCards, int availableMana,
			int availableManaWithCoin, List<CardInfo> predictedCards, List<CardInfo> runnerUpCards)
		{
			NumPossibleDecks = numPossibleDecks;
			NumPossibleCards = numPossibleCards;
			PredictedCards = predictedCards;
			RunnerUpCards = runnerUpCards;
			// Set all the cards' Playability values based on availableMana.
			PredictedCards.Concat(RunnerUpCards).ToList().ForEach(cardInfo =>
				{
					cardInfo.Playability =
						cardInfo.Card.Cost < availableMana ? PlayableType.BelowAvailableMana :
						(cardInfo.Card.Cost == availableMana ? PlayableType.AtAvailableMana :
						(cardInfo.Card.Cost == availableManaWithCoin ? PlayableType.AtAvailableManaWithCoin :
						PlayableType.AboveAvailableMana));
				});
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
			public PlayableType Playability { get; set; }

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

			public bool OffMeta => !Card.IsCreated && Card.Collectible && Card.Count > Probabilities.Count;

			public override string ToString()
			{
				string playabilityChar = "";
				switch (Playability)
				{
					case PlayableType.BelowAvailableMana:
						playabilityChar = "-";
						break;
					case PlayableType.AtAvailableMana:
						playabilityChar = "*";
						break;
					case PlayableType.AtAvailableManaWithCoin:
						playabilityChar = "o";
						break;
					case PlayableType.AboveAvailableMana:
						playabilityChar = "+";
						break;
				}

				List<string> probStrings = new List<string>();
				for (int n = 0; n < Probabilities.Count || n < NumPlayed; n++)
				{
					if (n < Probabilities.Count)
					{
						string probString = Math.Truncate(Probabilities[n] * 100) + "%";
						string playedString = n < NumPlayed ? "(P)" : "";
						probStrings.Add(probString + playedString);
					}
					else if (OffMeta)
					{
						// Off-meta
						probStrings.Add("XX");
					}
				}
				string percentageString = String.Join(" / ", probStrings);

				string costString = "[" + Card.Cost + playabilityChar + "] ";
				string createdString = Card.IsCreated || !Card.Collectible ? "[C]" : "";
				return costString + Card.Name + "(" + Card.Count + ")" +
					createdString + " - " + percentageString;
			}
		}
	}

	public enum PlayableType {
		BelowAvailableMana, AtAvailableMana, AtAvailableManaWithCoin, AboveAvailableMana };
}
