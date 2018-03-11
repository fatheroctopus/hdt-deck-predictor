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
			Visibility = cards.Count <= 0 ? Visibility.Hidden : Visibility.Visible;
			CardList.Update(cards, true);

			PercentageList.ItemsSource = prediction.PredictedCards.Select(cardInfo =>
				{
					// Lookup probabilities for any cards not already played.
					var nextProbabilities = cardInfo.Probabilities.Skip(cardInfo.NumPlayed).ToList();
					// Hide cards that no longer can be played, dim ones that aren't playable yet.
					var opacity = cardInfo.Card.Count - cardInfo.NumPlayed <= 0 ? 0 :
						(cardInfo.IsPlayable ? 1 : .5);
					return new PercentageItem(nextProbabilities, opacity);
				}).ToList();

			// Additional stats
			PossibleCards.Text = "Showing " +
				prediction.NumPredictedCards + " / " + prediction.NumPossibleCards + " Possible Cards";
			PossibleDecks.Text = prediction.NumPossibleDecks.ToString() + " Matching Decks";
		}

		public class PercentageItem
		{
			public PercentageItem(List<decimal> probabilities, double opacity)
			{
				if (probabilities.Count == 0)
				{
					Percentage = "";
				}
				else if (probabilities.All(prob => prob == probabilities[0]))
				{
					// All instances are at the same probability, just show one number.
					Percentage = DecimalToPercent(probabilities[0]);
				}
				else
				{
					Percentage = String.Join(" / ", probabilities.Select(prob => DecimalToPercent(prob)));
				}

				Opacity = opacity;
			}

			public string Percentage { get; private set; }

			public double Opacity { get; private set; }

			private static string DecimalToPercent(decimal value) =>
				Math.Truncate(value * 100).ToString() + "%";
		}
	}
}
