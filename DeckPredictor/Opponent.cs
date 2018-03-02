using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace DeckPredictor
{
	public class Opponent : IOpponent
	{
		private Player _opponent;

		public Opponent(Player opponent)
		{
			_opponent = opponent;
		}

		public string Class => _opponent.Class;

		public List<Card> KnownCards => _opponent.OpponentCardList;
	}
}
