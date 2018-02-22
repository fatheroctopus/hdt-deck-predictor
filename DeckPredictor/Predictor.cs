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

		public Predictor(List<Deck> metaDecks)
		{
			_metaDecks = metaDecks;
		}

		public void GameStart(IGame game)
		{

		}

		public ReadOnlyCollection<Deck> GetPossibleDecks()
		{
			return new ReadOnlyCollection<Deck>(new List<Deck>());
		}
	}
}
