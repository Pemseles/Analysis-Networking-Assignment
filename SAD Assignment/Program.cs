using System;
using System.Collections.Generic;
using System.Linq;

namespace SAD_Assignment
{
    class Program
    {
        public static Random rnd = new Random();
        static void Main(string[] args)
        {
            PlayerContainer players = new PlayerContainer(20);

            Stack<Card> deck1 = GenerateDeck(PlayerContainer.Player1);
            Stack<Card> deck2 = GenerateDeck(PlayerContainer.Player2);

            PlayerContainer.Player1.AddDeck(deck1);
            PlayerContainer.Player2.AddDeck(deck2);

            Game newGame = new Game(PlayerContainer.Player1, PlayerContainer.Player2);
            newGame.StartGame();
            
        }
        public static Stack<Card> GenerateDeck(Player p) {
            // randomly choose a pool of cards (like 5 lands, 10 permanents & 15 instas)

            // generate presetCards & instantiate empty deck
            PresetContainer presetCards = new PresetContainer(p);
            List<Card> newDeck = new List<Card>();

            // fill deck w 5 lands
            while (newDeck.Count < 5) {
                // generate the list of duplicates & pick a new card from list of presets
                IEnumerable<Card> dupes = DuplicateCheck(newDeck);
                Card newCard = PresetContainer.Lands.PresetLandsList[rnd.Next(PresetContainer.Lands.PresetLandsList.Count)];

                // checks if there is already 3 of chosen card in deck
                if (!dupes.Contains(newCard)) {
                   newDeck.Add(newCard); 
                }
            }
            // fill deck w 10 creatures
            while (newDeck.Count < 15) {
                // generate the list of duplicates & pick a new card from list of presets
                IEnumerable<Card> dupes = DuplicateCheck(newDeck);
                Card newCard = PresetContainer.Permas.PresetPermasList[rnd.Next(PresetContainer.Permas.PresetPermasList.Count)];

                // checks if there is already 3 of chosen card in deck
                if (!dupes.Contains(newCard)) {
                   newDeck.Add(newCard); 
                }
            }
            // fill deck w 15 instant spells
            while (newDeck.Count < 30) {
                // generate the list of duplicates & pick a new card from list of presets
                IEnumerable<Card> dupes = DuplicateCheck(newDeck);
                Card newCard = PresetContainer.Instas.PresetInstasList[rnd.Next(PresetContainer.Instas.PresetInstasList.Count)];

                // checks if there is already 3 of chosen card in deck
                if (!dupes.Contains(newCard)) {
                   newDeck.Add(newCard); 
                }
            }
            
            return new Stack<Card>(newDeck);
        }
        public static IEnumerable<Card> DuplicateCheck(List<Card> cardList) {
            // returns an IEnumerable with items in it that are > 3x in the param list
            // used to check if too many of a specific card are in a player's deck
            return cardList.GroupBy(x => x).Where(g => g.Count() > 3).Select(x => x.Key);
        }
    }
}
