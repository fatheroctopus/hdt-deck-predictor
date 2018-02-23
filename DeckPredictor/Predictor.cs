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
		private IGame _game;

		public Predictor(List<Deck> metaDecks)
		{
			_metaDecks = metaDecks;
		}

		public void GameStart(IGame game)
		{
			_game = game;
		}

		public ReadOnlyCollection<Deck> GetPossibleDecks()
		{
			var classDecks = _metaDecks.Where(x => x.Class == _game.Opponent.Class).ToList();
			return new ReadOnlyCollection<Deck>(classDecks);
		}
	}
}
