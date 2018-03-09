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
		List<Card> KnownCards { get; }

		// Return how much mana the opponent will have on the next turn.
		// Takes overload into account, and whether the opponent still has the coin.
		// Returns -1 if API has not yet initialized the opponent state.
		// Valid values will always be in [0, 10]
		int AvailableManaNextTurn { get; }
	}
}
