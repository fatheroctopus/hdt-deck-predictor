#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace DeckPredictor
{
	public partial class PredictionLayout
	{
		// Don't slide the tooltip past the bottom n cards in the list.
		private const int ToolTipCardBuffer = 3;

		public PredictionLayout()
		{
			InitializeComponent();
		}

		public void UpdateCardToolTip(Point mousePos)
		{
			// See if the mouse is inside the stack of card lists
			Point relativePos = CardStackPanel.PointFromScreen(mousePos);
			bool mouseInsideCardList = relativePos.X > 0 && relativePos.X < CardStackPanel.ActualWidth &&
				relativePos.Y > 0 && relativePos.Y < CardStackPanel.ActualHeight;
			if (mouseInsideCardList)
			{
				// Determine the actual card moused over.
				var cardSize = CardList.ActualHeight / CardList.Items.Count;
				var cardIndex = (int)(relativePos.Y / cardSize);
				if (cardIndex < 0 || cardIndex >= CardList.Items.Count)
				{
					CardToolTip.Visibility = Visibility.Collapsed;
					return;
				}
				CardToolTip.SetValue(
					DataContextProperty, CardList.Items.Cast<AnimatedCard>().ElementAt(cardIndex).Card);

				// Set the top margin on the tooltip so it appears next to the card.
				// Keep a buffer on the bottom so we're not changing the height of the main stack.
				var bottomBufferCards = Math.Max(0, CardList.Items.Count - ToolTipCardBuffer);
				var topPos = Math.Min(cardSize * cardIndex, cardSize * bottomBufferCards);
				CardToolTip.Margin = new Thickness(0, topPos, 0, 0);
				CardToolTip.Visibility = Visibility.Visible;
			}
			else
			{
				CardToolTip.Visibility = Visibility.Collapsed;
			}
		}

		public void Update(PredictionInfo prediction)
		{
			// Show each card at its unplayed count.
			// HACK - Because AnimatedCardList seems to reverse the order of created and uncreated cards,
			// we have to reordered by descending while keeping the percentages ordered by ascending,
			// which is enforced by the Controller.  This may need to be readdressed if AnimatedCardList's
			// behavior changes.
			var cards = prediction.PredictedCards
				.Select(cardInfo => cardInfo.GetCardWithUnplayedCount())
				.OrderBy(card => card.Cost)
				.ThenBy(card => card.Name)
				.ThenByDescending(card => card.IsCreated)
				.ToList();
			CardList.Update(cards, true);

			PercentageList.ItemsSource = prediction.PredictedCards.Select(cardInfo =>
				{
					// Lookup probabilities for any cards not already played.
					var nextProbabilities = cardInfo.Probabilities.Skip(cardInfo.NumPlayed).ToList();
					bool alreadyPlayed = (cardInfo.Card.Count - cardInfo.NumPlayed <= 0);
					return new PercentageItem(
						nextProbabilities, cardInfo.Playability, alreadyPlayed, cardInfo.OffMeta);
				}).ToList();

			// Additional stats
			PossibleCards.Text = "Predicting " +
				prediction.NumPredictedCards + " / " + prediction.NumPossibleCards + " Possible Cards";
			PossibleDecks.Text = prediction.NumPossibleDecks.ToString() + " Matching Decks";
		}

		public class PercentageItem
		{
			public PercentageItem(
				List<decimal> probabilities, PlayableType playability, bool alreadyPlayed, bool offMeta)
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

				// Show a star if this card is at opponent's available mana.
				bool showStar = (!alreadyPlayed && playability == PlayableType.AtAvailableMana);
				StarVisibility = showStar ? Visibility.Visible : Visibility.Collapsed;
				// Show a coin if the opponent can play this by using their coin.
				bool showCoin = (!alreadyPlayed && playability == PlayableType.AtAvailableManaWithCoin);
				CoinVisibility = showCoin ? Visibility.Visible : Visibility.Collapsed;
				// Show an X if this card does not fit into this deck according to the current meta.
				bool showX = (offMeta && !showStar && !showCoin);
				XVisibility = showX ? Visibility.Visible : Visibility.Collapsed;

				// Hide stats for cards if there's nothing to show them.
				ItemVisibility = (Percentage == "" && !showStar && !showCoin && !showX)
					? Visibility.Hidden : Visibility.Visible;

				// Dim stats for cards that aren't playable yet or have already been played.
				ItemOpacity = ((playability == PlayableType.AboveAvailableMana || alreadyPlayed) ? .5 : 1);
			}

			public string Percentage { get; private set; }

			public double ItemOpacity { get; private set; }

			public Visibility ItemVisibility { get; private set; }

			public Visibility CoinVisibility { get; private set; }
			public Visibility StarVisibility { get; private set; }
			public Visibility XVisibility { get; private set; }

			private static string DecimalToPercent(decimal value) =>
				Math.Truncate(value * 100).ToString() + "%";
		}
	}
}
