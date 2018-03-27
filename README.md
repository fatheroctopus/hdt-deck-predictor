# HDT Deck Predictor
A Plugin for Hearthstone Deck Tracker for predicting the opponent's decklist.

## Installation
- [Download and install HDT](https://hsdecktracker.net/).
- [Download the Deck Predictor Plugin](https://github.com/fatheroctopus/hdt-deck-predictor/releases).
- Extract the downloaded archive to your "Plugins" directory.
  - This location can be found from within HDT: `Options -> Tracking -> Plugins -> Plugins Folder`.
  - Your directory structure should look like `%AppData%/HearthstoneDeckTracker/Plugins/DeckPredictor/DeckPredictor.dll`.
- Restart HDT and enable the plugin in `Options -> Tracker -> Plugins`.

## Features
While enabled, DeckPredictor will replace the standard HDT opponent deck list with a new overlay:

![Overlay](Images/overlay.png)

- Chance that this card started in the opponent's decklist.
- Playing this card would spend all the opponent's available mana.
- Playing this card would be possible if the opponent used the coin.
- This played card is an outlier in the current meta and is being ignored by the Predictor.
- Number of predicted cards currently shown in the overlay out of all possible cards that could be in the deck
- Number of decks in the meta that match the opponent's already-played cards.

All stats update in real time as the opponent plays cards.

## How It Works

Deck Predictor was originally inspired by [AdnanC's MetaDetector plugin](https://github.com/AdnanC/HDT.Plugins.MetaDetector) and uses its aggregate data from [MetaStats](http://metastats.net/).

As the opponent plays cards, decks without those cards are filtered out, and each possible card is assigned a probability based on how often it appears in the list of possible decks.

The overlay does not display every possible card, but shows cards based on these heuristics:
 1. Around 30 cards should be shown.
 2. Cards that are sufficiently likely will usually be displayed.
 3. Cards that are playable for the opponent's next turn will often be favored over unplayable cards.
 4. Cards that are "optimal" for the opponent's next turn will often be favored over other playable cards.

The first and second copy of a card are tracked separately, but when showing both copies, the overlay will only display the probability of the first copy.

When a deck goes off-meta, one or more cards are deemed outliers and ignored by the prediction. Cards that appear in fewer decks with the other played cards are more likely to be judged as outliers.

## Known Issues
 - The plugin does not automatically update itself - fixing this is top-priority.
 - There are still a few display bugs floating around when cards go back into the opponent's hand
 - Deck popularity is not currently available from the meta data. If it were, all probabilities would be weighted by popularity.
 - Only works for Standard games at the moment since the meta data has no Wild stats.
 - Predictions do not account for cards with variable mana cost (e.g. Molten Giant, Crystal Lion)
 - Upgraded Spellstones are not correctly identified with the non-upgraded versions of their cards.

## Feedback
I'll respond to any issues opened for bugs or feature requests.
For problems with prediction results, please attach a screenshot of the issue and zip up the Logs directory at `%AppData%/HearthstoneDeckTracker/DeckPredictor/Logs`.
Try to do this immediately after the game finishes.
This is my first time working in C# and WPF, so I'm also interested in any idioms or best practices I may have missed.

Check out my music on [bandcamp](https://fatheroctopus.bandcamp.com).  It's free.
