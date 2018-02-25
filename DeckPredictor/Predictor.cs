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
		private bool _classDetected;

		public Predictor(IGame game, List<Deck> metaDecks)
		{
			_metaDecks = metaDecks;
			_game = game;
		}

		public void OnGameStart()
		{
			Log.Debug("Copying possible decks from the current meta");
			_possibleDecks = new List<Deck>(_metaDecks);
			CheckOpponentClass();
		}

		public void OnOpponentDraw()
		{
			CheckOpponentClass();
		}

		public ReadOnlyCollection<Deck> GetPossibleDecks()
		{
			return new ReadOnlyCollection<Deck>(_possibleDecks);
		}

		public void OnOpponentPlay(Card cardPlayed)
		{
			Log.Debug("cardPlayed: " + cardPlayed);
			foreach (Card card in _game.Opponent.RevealedCards)
			{
				Log.Debug("revealed card: " + card);
			}
			_possibleDecks = _possibleDecks
				.Where(deck =>
				{
					foreach (Card card in _game.Opponent.RevealedCards)
					{
						if (deck.Cards.FirstOrDefault(x => x.Id == card.Id) == null)
						{
							Log.Info("Filtering out a deck missing card: " + card);
							return false;
						}
					}
					return true;
				}).ToList();
			Log.Debug(_possibleDecks.Count + " possible decks");
		}

		private void CheckOpponentClass()
		{
			if (_classDetected)
			{
				return;
			}
			if (string.IsNullOrEmpty(_game.Opponent.Class))
			{
				return;
			}
			// Only want decks for the opponent's class.
			_possibleDecks = _possibleDecks.Where(x => x.Class == _game.Opponent.Class).ToList();
			_classDetected = true;
			Log.Info(_possibleDecks.Count + " possible decks for class " + _game.Opponent.Class);
		}
	}

}
