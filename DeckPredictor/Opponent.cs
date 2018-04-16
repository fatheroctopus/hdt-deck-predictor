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

		public int GetAvailableManaNextTurn(bool considerCoin)
		{
			var opponentEntity = _game.OpponentEntity;
			if (opponentEntity == null)
			{
				Log.Debug("Cannot query mana when opponent entity does not exist");
				return -1;
			}

			bool hasCoin;
			if (_game.GetTurnNumber() == 0)
			{
				// During the mulligan, the coin doesn't exist, but second player will get it next turn.
				hasCoin = !opponentEntity.HasTag(GameTag.FIRST_PLAYER);
			}
			else
			{
				hasCoin = _opponent.HasCoin;
			}

			var crystals = opponentEntity.GetTag(GameTag.RESOURCES);
			var overload = opponentEntity.GetTag(GameTag.OVERLOAD_OWED);
			var coinBonus = (considerCoin && hasCoin) ? 1 : 0;
			// Add one mana for next turn.
			var mana = crystals - overload + coinBonus + 1;
			return Math.Min(Math.Max(mana, 0), 10);
		}
	}
}
