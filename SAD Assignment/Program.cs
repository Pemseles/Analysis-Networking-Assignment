using System;
using System.Collections.Generic;

/*
Thijmen Bouwsema 1008331
*/

namespace SAD_Assignment
{
    class Program
    {
        public static Random rnd = new Random();
        static void Main(string[] args)
        {
            // create log.txt or empties it if it was already there
            Game.EmptyLogFile();

            // initialises players & decks
            PlayerContainer players = new PlayerContainer(10);
            List<Card> deck1 = GenerateDeck(PlayerContainer.Player1);
            List<Card> deck2 = GenerateDeck(PlayerContainer.Player2);

            // add generated decks to players (necessary because cards need a playerId; means players have to be made first)
            PlayerContainer.Player1.AddDeck(deck1);
            PlayerContainer.Player2.AddDeck(deck2);

            // starts game
            Game newGame = new Game(PlayerContainer.Player1, PlayerContainer.Player2);
            newGame.PlayGame();
        }
        /// <summary>
        /// Method <c>GenerateSimulationDeck</c> generates a deck for specified player (for deliverable-purposes only)
        /// </summary>
        public static List<Card> GenerateDeck(Player p) {
            List<Card> staticDeck = new List<Card>();
            if (p.ID == 1) {
                // get player 1's specific cards
                // p1 has: 3 lands (any color), 1 Blue creature (2/2; discard card effect), 1 instant green (buff +3/+3), 1 instant blue (counter)

                // get the 3 lands
                staticDeck.Add(new Land(p.ID, "Unnamed land", "Some description.", Color.Colorless, EffectType.GenerateEnergy));
                staticDeck.Add(new Land(p.ID, "Unnamed land", "Some description.", Color.Colorless, EffectType.GenerateEnergy));
                staticDeck.Add(new Land(p.ID, "Unnamed land", "Some description.", Color.Colorless, EffectType.GenerateEnergy));

                // get the 1 blue creature
                staticDeck.Add(new PermaSpell(2, p.ID, "Unnamed blue creature", "Is able to remove 1 random card from opponent's hand.", Color.Blue, 5, EffectType.ForceDiscardCard, 2, 2));

                // get the green +3/+3 buff & blue counter
                staticDeck.Add(new InstaSpell(3, p.ID, "Green buff", "Is able to buff a creature's attack & defense by +3", Color.Green, EffectType.Buff, 3, 3));
                staticDeck.Add(new InstaSpell(1, p.ID, "Blue Counter", "A blue instant spell that can counter the most recently used card by the opponent.", Color.Blue, EffectType.Counter));
            }
            else {
                // get player 2's specific cards
                // p2 has: 1 land (any color), 1 instant red (counter)

                // get the 1 land & red counter
                staticDeck.Add(new Land(p.ID, "Unnamed land", "Some description.", Color.Colorless, EffectType.GenerateEnergy));
                staticDeck.Add(new InstaSpell(1, p.ID, "Red Counter", "A red instant spell that can counter the most recently used card by the opponent.", Color.Red, EffectType.Counter));
            }
            // fill deck with purposefully useless cards
            while (staticDeck.Count < 30) {
                InstaSpell emptyCard = new InstaSpell(10000, p.ID, "", "", Color.Colorless, EffectType.None);
                emptyCard.Type = Type.Irrelevant;
                staticDeck.Add(emptyCard);
            }
            return staticDeck;
        }
    }
}
