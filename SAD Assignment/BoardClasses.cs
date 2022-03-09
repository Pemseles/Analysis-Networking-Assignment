using System;
using System.Collections.Generic;

namespace SAD_Assignment
{
    public class Game {
        public Random rnd = new Random();
        // TurnNum = actual turn number; increments when new turn begins
        public int TurnNum { get; set; }
        // TurnPhase: 0 = Pre-game (shuffle decks), 1 = reset temp effects, 2 = both players draw 1 card (if possible), 
        //            3 = prioritised player plays their cards, 4 = other player plays their cards, 
        //            5 = both players discard cards until they have 7 each (if possible)
        // after this, TurnPhase is reset to 0 when TurnNum is incremented
        public int TurnPhase { get; set; }
        // PlayerPriority decides who goes first during each turn (decided randomly at the start of the game)
        public int PlayerPriority { get; set; }
        // CardQueue contains all cards to be played (stored here to be activated after eachother in order later on)
        public Queue<Card> CardQueue { get; set; }
        // InstaCardChain holds all insta's that players are countering eachother with, to then be activated FIFO-style
        public Stack<InstaSpell> InstaCardChain { get; set; }
        // LandsOnBoard holds all lands currently in play
        public List<Land> LandsOnBoard { get; set; }
        // ActiveSpells holds all permanent spells or 'creatures' in play
        public List<PermaSpell> ActiveSpells { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public string Winner { get; set; }
        public Game(Player p1, Player p2) {
            this.TurnNum = this.TurnPhase = 0;
            this.CardQueue = new Queue<Card>();
            this.InstaCardChain = new Stack<InstaSpell>();
            this.LandsOnBoard = new List<Land>();
            this.Player1 = p1;
            this.Player2 = p2;
            this.PlayerPriority = -1;
            this.Winner = "Undecided";
        }
        public void StartGame() {
            // main method

            // player priority is decided at the start of the game (stays this way for entire duration of the game)
            this.PlayerPriority = this.rnd.Next(2);
            
            // while-loop that keeps game going until winner is decided
            while (this.Winner == "Undecided") {
                // do stuff
            }
        }
        public void LogActivities() {
            // write what happens in CardGame() to log.txt
        }
        public void PrintToConsole() {
            // print what happens in CardGame() to console
        }
        public void AddLands(Land landToAdd) {
            // add lands to LandsOnBoard
        }
        public void AddToCardQueue(Card cardToAdd) {
            // add card to CardQueue
        }
        public void HandleCardQueue() {
            // handles card effects in CardQueue when they need to activate
        }
    }
}