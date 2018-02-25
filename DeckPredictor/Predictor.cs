using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace DeckPredictor
{
	public class Predictor
	{
		private List<Deck> _metaDecks;
		private List<Deck> _possibleDecks;
		private IGame _game;

		public Predictor(List<Deck> metaDecks)
		{
			_metaDecks = metaDecks;
		}

		public void OnGameStart(IGame game)
		{
			_game = game;
			// Only want decks for the opponent's class.
			_possibleDecks = _metaDecks.Where(x => x.Class == _game.Opponent.Class).ToList();
		}

		public ReadOnlyCollection<Deck> GetPossibleDecks()
		{
			return new ReadOnlyCollection<Deck>(_possibleDecks);
		}

		public void OnOpponentPlay(Card cardPlayed) {
			_possibleDecks = _possibleDecks
				.Where(deck =>
				{
					foreach (Card card in _game.Opponent.RevealedCards)
					{
						if (deck.Cards.FirstOrDefault(x => x.Id == card.Id) == null)
						{
							return false;
						}
					}
					return true;
				}).ToList();
		}
	}
}
