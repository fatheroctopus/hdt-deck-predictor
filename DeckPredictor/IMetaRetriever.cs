using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace DeckPredictor
{
	public interface IMetaRetriever
	{
		// Returns the list of all decks used in the current meta.
		// If the last cached version is too out of date, it will be retrieved via WebClient.
		Task<List<Deck>> RetrieveMetaDecks(PluginConfig config);
	}
}