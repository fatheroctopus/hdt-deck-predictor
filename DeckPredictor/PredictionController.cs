using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System;

namespace DeckPredictor
{
	public class PredictionController
	{
		private IOpponent _opponent;
		private IPredictionView _view;

		public PredictionController(IOpponent opponent, IPredictionView view)
		{
			_opponent = opponent;
			_view = view;
		}

		public void OnPredictionUpdate(IPredictor predictor)
		{
			Log.Debug("Collapsing duplicates from prediction.");
			var playedCards = _opponent.KnownCards
				.Where(card => !card.IsCreated && card.Collectible && !card.Jousted);
			List<Card> cardList = predictor.PredictedCards
				.GroupBy(predictedCard => predictedCard.Card.Id, predictedCard => predictedCard.Card)
				.Select(group =>
				{
					var card = Database.GetCardFromId(group.Key);
					card.Count = group.Count();
					var playedCard = playedCards.FirstOrDefault(c => c.Id == card.Id);
					if (playedCard != null)
					{
						Log.Debug("Removing played card: " + playedCard);
						card.Count -= playedCard.Count;
					}
					return card;
				})
				.Where(card => card.Count > 0)
				.ToList();
			_view.UpdateCards(cardList);
		}
	}
}
