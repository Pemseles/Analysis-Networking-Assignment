using System;
using System.Collections.Generic;

/*
Thijmen Bouwsema 1008331
*/

namespace SAD_Assignment
{
    class Program
    {
        static void Main(string[] args)
        {
            // create log.txt or empties it if it was already there
            Game.EmptyLogFile();

            // initialises playerContainer singleton & decks
            PlayerContainer players = PlayerContainer.GetInstance(10);
            CardComposite deck1 = GenerateDeck(players.Player1);
            CardComposite deck2 = GenerateDeck(players.Player2);

            // add generated decks to players (necessary because cards need a playerId; means players have to be made first)
            players.Player1.AddDeck(deck1);
            players.Player2.AddDeck(deck2);

            // starts game
            Game newGame = Game.GetInstance(players.Player1, players.Player2);
            newGame.PlayGame();
        }
        /// <summary>
        /// Method <c>GenerateSimulationDeck</c> generates a deck for specified player (for deliverable-purposes only)
        /// </summary>
        public static CardComposite GenerateDeck(Player p) {
            // init composite & card creators
            CardComposite staticDeck = new CardComposite("Root");
            CardCreator[] cardCreators = new CardCreator[] { new LandCreator(), new PermaCreator(), new InstaCreator() };
            int emptyCardsCount = 0;

            if (p.ID == 1) {
                // get player 1's specific cards
                // p1 has: 3 lands (any color), 1 Blue creature (2/2; discard card effect), 1 instant green (buff +3/+3), 1 instant blue (counter)

                // get the 3 lands
                CardComposite lands = new CardComposite("Lands");
                lands.Add(cardCreators[0].CreateCard(p.ID, "Unnamed land", "Some description.", Color.Colorless));
                lands.Add(cardCreators[0].CreateCard(p.ID, "Unnamed land", "Some description.", Color.Colorless));
                lands.Add(cardCreators[0].CreateCard(p.ID, "Unnamed land", "Some description.", Color.Colorless));
                staticDeck.Add(lands);

                // get the 1 blue creature
                CardComposite creatures = new CardComposite("Creatures");
                creatures.Add(cardCreators[1].CreateCard(p.ID, "Unnamed blue creature", "Is able to remove 1 random card from opponent's hand.", Color.Blue, 2, EffectType.ForceDiscard, 2, 2, 2));
                staticDeck.Add(creatures);

                // get the green +3/+3 buff & blue counter
                CardComposite instas = new CardComposite("Insta Spells");
                instas.Add(cardCreators[2].CreateCard(p.ID, "Green buff", "Is able to buff a creature's attack & defense by +3", Color.Green, 3, EffectType.Buff, 3, 3));
                instas.Add(cardCreators[2].CreateCard(p.ID, "Blue Counter", "A blue instant spell that can counter the most recently used card by the opponent.", Color.Blue, 1, EffectType.Counter));
                staticDeck.Add(instas);

                // specify amount of empty cards needing to be added (only necessary due to deliverable's case; is 30 - the amount of cards necessary for case)
                emptyCardsCount = 24;
            }
            else {
                // get player 2's specific cards
                // p2 has: 1 land (any color), 1 instant red (counter)

                // get the 1 land & red counter
                CardComposite lands = new CardComposite("Lands");
                lands.Add(cardCreators[0].CreateCard(p.ID, "Unnamed land", "Some description.", Color.Colorless));
                staticDeck.Add(lands);

                CardComposite instas = new CardComposite("Insta Spells");
                instas.Add(cardCreators[2].CreateCard(p.ID, "Red Counter", "A red instant spell that can counter the most recently used card by the opponent.", Color.Red, 1, EffectType.Counter));
                staticDeck.Add(instas);

                // specify amount of empty cards needing to be added (only necessary due to deliverable's case; is 30 - the amount of cards necessary for case)
                emptyCardsCount = 28;
            }
            // fill deck with purposefully useless cards
            CardComposite emptyCards = new CardComposite("Empty");
            while (emptyCardsCount > 0) {
                InstaSpell emptyCard = new InstaSpell(10000, p.ID, "", "", Color.Colorless, EffectType.None);
                emptyCard.CardType = CardType.Irrelevant;
                emptyCards.Add(emptyCard);
                emptyCardsCount--;
            }
            staticDeck.Add(emptyCards);
            return staticDeck;
        }
    }
}
