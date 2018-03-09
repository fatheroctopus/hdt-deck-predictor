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
			var cards = new List<Card>();
			var percentages = new List<PercentageItem>();
			prediction.PredictedCards.ForEach(cardInfo =>
				{
					// Card with unplayed count on the card list.
					cards.Add(cardInfo.GetCardWithUnplayedCount());

					// Percentages on the percentage list.
					var nextProbabilities = cardInfo.Probabilities.Skip(cardInfo.NumPlayed).ToList();
					string percentageString;
					if (nextProbabilities.Count == 0)
					{
						// All cards have been played already.
						percentageString = "";
					}
					else
					{
						if (nextProbabilities.All(prob => prob == nextProbabilities[0]))
						{
							// All instances are at the same probability, just show one number.
							nextProbabilities = new List<decimal> { nextProbabilities[0] };
						}
						percentageString = String.Join(" / ",
							nextProbabilities.Select(prob => Math.Truncate(prob * 100).ToString() + "%"));
					}

					var item = new PercentageItem(percentageString);
					percentages.Add(item);
				});
			Visibility = cards.Count <= 0 ? Visibility.Hidden : Visibility.Visible;
			CardList.Update(cards, true);
			PercentageList.ItemsSource = percentages;

			// Additional stats
			PossibleCards.Text = "Showing " +
				prediction.NumPredictedCards + " / " + prediction.NumPossibleCards + " Possible Cards";
			PossibleDecks.Text = prediction.NumPossibleDecks.ToString() + " Matching Decks";
		}

		public class PercentageItem
		{
			public PercentageItem(string percentage)
			{
				Percentage = percentage;
			}

			public string Percentage { get; private set; }
		}
	}
}
