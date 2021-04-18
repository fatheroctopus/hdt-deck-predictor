using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using HearthDb;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Net.Http;
using System.Threading;

namespace DeckPredictor
{
    class MetaRetriever
	{
        private static string siteUrl = "http://metastats.net/hearthstone/class/decks/";
        public static string[] Classes { get; } = { "DemonHunter", "Druid", "Hunter", "Mage", "Paladin", "Priest", "Rogue", "Shaman", "Warlock", "Warrior" };

        public async Task<List<Deck>> RetrieveMetaDecks(PluginConfig config)
        {
            List<Deck> metaDecks = new List<Deck>();

            try
            {
                CancellationTokenSource cancellationToken = new CancellationTokenSource();
                HttpClient httpClient = new HttpClient();
                foreach (var className in Classes)
                {
                    Log.Info(className);
                    HttpResponseMessage request = await httpClient.GetAsync(siteUrl + className);
                    cancellationToken.Token.ThrowIfCancellationRequested();

                    Stream response = await request.Content.ReadAsStreamAsync();
                    cancellationToken.Token.ThrowIfCancellationRequested();

                    HtmlParser parser = new HtmlParser();
                    IHtmlDocument document = parser.ParseDocument(response);

                    metaDecks.AddRange(ParseDoc(document, className));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return metaDecks;
		}

        static private List<Deck> ParseDoc(IHtmlDocument document, string className)
        {
            List<Deck> metaDecks = new List<Deck>();

            IEnumerable<IElement> decks = document.All.Where(x => x.ClassName == "decklist");

            foreach (var deck in decks)
            {
                Deck d = new Deck();
                IElement atag = deck.QuerySelectorAll("a").Eq(0);
                d.Class = className;
                d.Name = atag.Text();
                d.Name = d.Name.Substring(0, d.Name.LastIndexOf("#")).Trim();
                d.Url = atag.GetAttribute("href").Trim();
                IElement divtag = deck.QuerySelectorAll("div").Eq(1);
                var games = divtag.Text().Trim().Replace("#Games: ", "").Trim();
                d.Note = games.Substring(0, games.IndexOf("\n", 0)).Trim(); // used to store the number of games played with the deck
                var cards = deck.QuerySelectorAll(".card-list-item");

                foreach (var card in cards)
                {
                    IElement img = card.QuerySelectorAll("img").Eq(0);
                    String src = img.GetAttribute("src");
                    String name = card.QuerySelectorAll(".hover-img").Eq(0).Text().Trim();
                    var id = src.Substring(src.LastIndexOf("/") + 1).Trim();
                    var quantity = Int32.Parse(card.QuerySelectorAll(".card-quantity").Eq(0).Text().Replace("x", "").Trim());
                    // Looking up key in HearthDb
                    HearthDb.Card HearthDbCard;
                    try {
                        HearthDbCard = Cards.All[id];
                    } catch {
                        // Failed to find card id in database, looking up by name
                        if (name == "Lord Jaraxxus") {
                            // NOTE: Jaraxxus is a special case that fails all other lookups
                            HearthDbCard = Cards.All["CORE_EX1_323"];
                        } else {
                            HearthDbCard = HearthDb.Cards.GetFromName(name, HearthDb.Enums.Locale.enUS);
                            if (HearthDbCard.Id.Substring(0, 4) != "CORE")
                            {
                                HearthDbCard = null;
                                // Failed to find CORE card on name lookup, doing brute force search...
                                var keys = HearthDb.Cards.Collectible.Keys.ToList(); keys.Sort();
                                foreach (var key in keys)
                                {
                                    var val = HearthDb.Cards.Collectible[key];
                                    if (val.Name.Trim() == name && val.Id.Substring(0, 4) == "CORE") {
                                        HearthDbCard = val;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (HearthDbCard == null) {
                        Log.Error("Error: failed to find card id " + id + " with name: " + name);
                    } else {
                        Hearthstone_Deck_Tracker.Hearthstone.Card c = new Hearthstone_Deck_Tracker.Hearthstone.Card(HearthDbCard);
                        c.Count = quantity;
                        d.Cards.Add(c);
                        Log.Info(className + ", " + d.Name + ", " + c.Count + "x " + c.Name);
                    }
                }
                metaDecks.Add(d);
            }
            return metaDecks;
        }

        static public void PrintResults(List<Deck> decks)
        {
            foreach (var deck in decks)
            {
                Log.Info(deck.Name + " " + deck.Url);
                foreach (var card in deck.Cards)
                {
                    Log.Info(card.Id + " " + card.Name);
                }
            }
        }
    }
}
