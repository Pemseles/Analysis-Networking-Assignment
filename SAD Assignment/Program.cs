using System;
using System.Collections.Generic;

namespace SAD_Assignment
{
    class Program
    {
        static void Main(string[] args)
        {
            Stack<Card> deck1 = new Stack<Card>();
            Stack<Card> deck2 = new Stack<Card>();

            Player p1 = new Player(1, 20, deck1);
            Player p2 = new Player(2, 20, deck2);

            Game newGame = new Game(p1, p2);
            newGame.StartGame();
        }
        public Stack<Card> GenerateDeck() {
            // randomly choose a pool of cards (like 5 lands, 10 permanents & 15 instas)
            return null;
        }
    }
}
