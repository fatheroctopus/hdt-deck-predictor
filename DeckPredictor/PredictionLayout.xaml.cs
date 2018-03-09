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
			var cardInfos = prediction.PredictedCards;
			List<Card> cards = cardInfos.Select(cardInfo => cardInfo.GetCardWithUnplayedCount()).ToList();
			Visibility = cards.Count <= 0 ? Visibility.Hidden : Visibility.Visible;
			CardList.Update(cards, true);
			var items = cardInfos.Select(cardInfo =>
				{
					var item = new PercentageItem();
					string percentageString = cardInfo.NumPlayed < cardInfo.Probabilities.Count
						? Math.Truncate(cardInfo.Probabilities[cardInfo.NumPlayed] * 100) + "%"
						: "";
					item.Percentage = percentageString;
					return item;
				}).ToList();
			PercentageList.ItemsSource = items;

			// Additional stats
			PossibleCards.Text = "Showing " +
				prediction.NumPredictedCards + " / " + prediction.NumPossibleCards + " Possible Cards";
			PossibleDecks.Text = prediction.NumPossibleDecks.ToString() + " Matching Decks";
		}

		public class PercentageItem
		{
			public string Percentage { get; set; }
		}
	}
}
