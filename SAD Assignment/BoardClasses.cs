using System;
using System.Collections.Generic;

namespace SAD_Assignment
{
    public class Game {
        public int TurnNum { get; set; }
        public int TurnPhase { get; set; }
        public int PlayerPriority { get; set; }
        public Stack<Card> CardQueue { get; set; }
        public Stack<Card> InstaCardChain { get; set; }
        public List<Land> LandsOnBoard { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
    }
}