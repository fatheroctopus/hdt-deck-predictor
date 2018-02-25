using Hearthstone_Deck_Tracker.Hearthstone;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace DeckPredictor
{
	public interface IOpponent
	{
		// May be empty or null if API has not yet detected the opponent class.
		string Class { get; }

		// List of all cards known to be in the opponent's deck.
		ReadOnlyCollection<Card> KnownCards { get; }
	}
}
