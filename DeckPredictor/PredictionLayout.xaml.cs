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
		// Don't slide the tooltip past the bottom cards in the list.
		private const int ToolTipBottomBuffer = 100;
		private const int CardHeight = 32;
		private const double CardListHeightToScreenRatio = .5;
		private PluginConfig _config;

		public PredictionLayout(PluginConfig config)
		{
			_config = config;
			InitializeComponent();
		}

		public void UpdateCardToolTip(Point mousePos)
		{
			// See if the mouse is inside the stack of card lists
			Point relativePos = CardView.PointFromScreen(mousePos);
			bool mouseInsideCardList = relativePos.X > 0 && relativePos.X < CardView.ActualWidth &&
				relativePos.Y > 0 && relativePos.Y < CardView.ActualHeight;
			if (mouseInsideCardList)
			{
				// Determine the actual card moused over.
				var cardSize = CardView.ActualHeight / CardList.Items.Count;
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
				var cardTopPos = cardSize * cardIndex;
				var toolTipTopPos = Math.Min(cardTopPos, CardView.ActualHeight - ToolTipBottomBuffer);
				CardToolTip.Margin = new Thickness(0, toolTipTopPos, 0, 0);
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
			// HACK - Because AnimatedCardList seems to reverse the order of cards with the same ID,
			// we have to reverse their order here but preserve the original order for the percentages.
			// This may need to be readdressed if AnimatedCardList's behavior changes.
			var cards = prediction.PredictedCards
				.GroupBy(cardInfo => cardInfo.Card.Id, cardInfo => cardInfo.GetCardWithUnplayedCount())
				.Select(group => group.Reverse())
				.SelectMany(x => x)
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

			// Enforce a maximum height on the Viewbox that contains the list of cards.
			// (If the option is set in the config)
			double maxHeight = SystemParameters.PrimaryScreenHeight * CardListHeightToScreenRatio;
			if (_config.FitDeckListToDisplay && cards.Count * CardHeight > maxHeight)
			{
				CardView.Height = maxHeight;
			}
			else
			{
				CardView.Height = Double.NaN;
			}
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

				// Show the optimal icon if this card is at opponent's available mana.
				bool showOptimal = (!alreadyPlayed && playability == PlayableType.AtAvailableMana);
				OptimalVisibility = showOptimal ? Visibility.Visible : Visibility.Collapsed;
				// Show a coin if the opponent can play this by using their coin.
				bool showCoin = (!alreadyPlayed && playability == PlayableType.AtAvailableManaWithCoin);
				CoinVisibility = showCoin ? Visibility.Visible : Visibility.Collapsed;
				// Show an X if this card does not fit into this deck according to the current meta.
				bool showX = (offMeta && !showOptimal && !showCoin);
				XVisibility = showX ? Visibility.Visible : Visibility.Collapsed;

				// Hide stats for cards if there's nothing to show them.
				ItemVisibility = (Percentage == "" && !showOptimal && !showCoin && !showX)
					? Visibility.Hidden : Visibility.Visible;

				// Dim stats for cards that aren't playable yet or have already been played.
				ItemOpacity = ((playability == PlayableType.AboveAvailableMana || alreadyPlayed) ? .3 : 1);
			}

			public string Percentage { get; private set; }

			public double ItemOpacity { get; private set; }

			public Visibility ItemVisibility { get; private set; }

			public Visibility CoinVisibility { get; private set; }
			public Visibility OptimalVisibility { get; private set; }
			public Visibility XVisibility { get; private set; }

			public int CardHeight => PredictionLayout.CardHeight;

			private static string DecimalToPercent(decimal value) =>
				Math.Truncate(value * 100).ToString() + "%";
		}
	}
}
