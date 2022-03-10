using System;
using System.Collections.Generic;

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
            //newGame.StartGame();
        }
        public static Stack<Card> GenerateDeck(Player p) {
            // randomly choose a pool of cards (like 5 lands, 10 permanents & 15 instas)

            Array colors = typeof(Color).GetEnumValues();
            Stack<Card> newDeck = new Stack<Card>();

            while (newDeck.Count < 5) {
                // change how it makes lands, maybe make a sample of preset cards
                Color randomColor = (Color)colors.GetValue(rnd.Next(colors.Length));
                Land newLand = new Land(p.ID, "emptyName", "emptyDescription", randomColor, new GenerateEnergy());
                newDeck.Push(newLand);
            }
            while (newDeck.Count < 15) {
                // change how it makes permas, maybe make a sample of preset cards
                Color randomColor = (Color)colors.GetValue(rnd.Next(colors.Length));
                PermaSpell newPerma = new PermaSpell(rnd.Next(1, 4), p.ID, "emptyName", "emptyDescription", randomColor, rnd.Next(3, 16), null, rnd.Next(5, 16), rnd.Next(5, 16));
                newDeck.Push(newPerma);
            }
            while (newDeck.Count < 30) {
                // change how it makes instas, maybe make a sample of preset cards
                Color randomColor = (Color)colors.GetValue(rnd.Next(colors.Length));
                InstaSpell newInsta = new InstaSpell(rnd.Next(1, 4), p.ID, "emptyName", "emptyDescription", randomColor, null);
                newDeck.Push(newInsta);
            }

            foreach(Card item in newDeck) {
                Console.WriteLine(item.GetInfo());
            }

            return newDeck;
        }
    }
}
