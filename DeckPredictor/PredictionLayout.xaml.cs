#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace DeckPredictor
{
	public partial class PredictionLayout
	{
		public PredictionLayout()
		{
			InitializeComponent();
		}

		public void Update(PredictionInfo prediction)
		{
			// Show each card at its unplayed count.
			var cards =
				prediction.PredictedCards.Select(cardInfo => cardInfo.GetCardWithUnplayedCount()).ToList();
			CardList.Update(cards, true);

			PercentageList.ItemsSource = prediction.PredictedCards.Select(cardInfo =>
				{
					// Lookup probabilities for any cards not already played.
					var nextProbabilities = cardInfo.Probabilities.Skip(cardInfo.NumPlayed).ToList();
					bool alreadyPlayed = (cardInfo.Card.Count - cardInfo.NumPlayed <= 0);
					return new PercentageItem(nextProbabilities, cardInfo.Playability, alreadyPlayed);
				}).ToList();

			// Additional stats
			PossibleCards.Text = "Showing " +
				prediction.NumPredictedCards + " / " + prediction.NumPossibleCards + " Possible Cards";
			PossibleDecks.Text = prediction.NumPossibleDecks.ToString() + " Matching Decks";
		}

		public class PercentageItem
		{
			public PercentageItem(List<decimal> probabilities, PlayableType playability, bool alreadyPlayed)
			{
				// TODO: setting?
				bool onlyShowFirst = true;
				if (probabilities.Count == 0)
				{
					Percentage = "";
				}
				else if (onlyShowFirst || probabilities.All(prob => prob == probabilities[0]))
				{
					// All instances are at the same probability, just show one number.
					Percentage = DecimalToPercent(probabilities[0]);
				}
				else
				{
					Percentage = String.Join(" / ", probabilities.Select(prob => DecimalToPercent(prob)));
				}

				// Hide cards that have already been played.
				ItemVisibility = (alreadyPlayed ? Visibility.Hidden : Visibility.Visible);

				// Dim cards that aren't playable yet.
				ItemOpacity = (playability == PlayableType.AboveAvailableMana ? .5 : 1);

				// Show a star if this card is at opponent's available mana.
				StarVisibility =
					(playability == PlayableType.AtAvailableMana ? Visibility.Visible : Visibility.Collapsed);
				// Show a coin if the opponent can play this by using their coin.
				CoinVisibility = (playability == PlayableType.AtAvailableManaWithCoin ?
					Visibility.Visible : Visibility.Collapsed);
			}

			public string Percentage { get; private set; }

			public double ItemOpacity { get; private set; }

			public Visibility ItemVisibility { get; private set; }

			public Visibility CoinVisibility { get; private set; }
			public Visibility StarVisibility { get; private set; }

			private static string DecimalToPercent(decimal value) =>
				Math.Truncate(value * 100).ToString() + "%";
		}
	}
}
