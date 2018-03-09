using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using HearthDb.Enums;

namespace DeckPredictor
{
	public class Opponent : IOpponent
	{
		private GameV2 _game;
		private Player _opponent;

		public Opponent(GameV2 game)
		{
			_game = game;
			_opponent = game.Opponent;
		}

		public string Class => _opponent.Class;

		public List<Card> KnownCards => _opponent.OpponentCardList;

		public int AvailableManaNextTurn
		{
			get {
				var opponentEntity = _game.OpponentEntity;
				var crystals = opponentEntity.GetTag(GameTag.RESOURCES);
				var overload = opponentEntity.GetTag(GameTag.OVERLOAD_OWED);
				var coinBonus = _opponent.HasCoin ? 1 : 0;
				// Add one mana for next turn.
				var mana = crystals - overload + coinBonus + 1;
				return Math.Min(Math.Max(mana, 0), 10);
			}
		}
	}
}
