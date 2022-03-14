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
        public List<PermaSpell> ActiveCreatures { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        // Winner: is "Undecided" while playing, otherwise will say "Player 1", "Player 2" or "Tie" depending on who wins
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

            // Pre-game prep (PlayerPriority is determined, Decks are shuffled & hands are filled)
            this.TurnPhase = 0;
            this.PlayerPriority = this.rnd.Next(2);
            this.Player1.ShuffleDeck();
            this.Player1.FillHand();
            this.Player2.ShuffleDeck();
            this.Player2.FillHand();
            
            // while-loop that keeps game going until winner is decided
            while (this.Winner == "Undecided") {

                // phase 1: revert temporary effects (applies them again at end of this phase), reset lands & remove expired creatures 
                this.TurnPhase = 1;

                for (int i = 0; i < this.ActiveCreatures.Count; i++) {
                    // reverts effects if they were activated during last turn
                    if (this.ActiveCreatures[i].Effect != null) {
                        foreach (PermaSpell perma in this.ActiveCreatures) {
                            if ((this.ActiveCreatures[i].PlayerId == Player1.ID && this.ActiveCreatures[i].Effect.Target == Target.Yours) && perma.PlayerId == Player1.ID) {
                                // reverts self-buffing effects for player 1's creatures
                                this.ActiveCreatures[i].RevertEffects(perma);
                            }
                            else if ((this.ActiveCreatures[i].PlayerId == Player2.ID && this.ActiveCreatures[i].Effect.Target == Target.Yours) && perma.PlayerId == Player2.ID) {
                                // reverts self-buffing effects for player 2's creatures
                                this.ActiveCreatures[i].RevertEffects(perma);
                            }
                            else if ((this.ActiveCreatures[i].PlayerId == Player1.ID && this.ActiveCreatures[i].Effect.Target == Target.Opponents) && perma.PlayerId == Player2.ID) {
                                // reverts debuffing effect caused by player 1 on player 2's creatures
                                this.ActiveCreatures[i].RevertEffects(perma);
                            }
                            else if ((this.ActiveCreatures[i].PlayerId == Player2.ID && this.ActiveCreatures[i].Effect.Target == Target.Opponents) && perma.PlayerId == Player1.ID) {
                                // reverts debuffing effect caused by player 2 on player 1's creatures
                                this.ActiveCreatures[i].RevertEffects(perma);
                            }
                        }
                    }

                    // checks if the card has any turns left; if not, it gets removed from play
                    if (this.ActiveCreatures[i].TurnsLeft == 0 && this.ActiveCreatures[i].PlayerId == 1) {
                        // removes card from play; it's turns are over so it gets discarded
                        this.Player1.DiscardHand(this.ActiveCreatures[i]);
                        this.ActiveCreatures.RemoveAt(i);
                    }
                    else if (this.ActiveCreatures[i].TurnsLeft == 0 && this.ActiveCreatures[i].PlayerId == 2) {
                        // removes card from play; it's turns are over so it gets discarded
                        this.Player2.DiscardHand(this.ActiveCreatures[i]);
                        this.ActiveCreatures.RemoveAt(i);
                    }
                }
                for (int i = 0; i < this.LandsOnBoard.Count; i++) {
                    // resets all lands on board so they can be used during later phases
                    this.LandsOnBoard[i].ResetLand();
                }

                // re-apply creature effects
                for (int i = 0; i < this.ActiveCreatures.Count; i++) {
                    // reverts effects if they were activated during last turn
                    if (this.ActiveCreatures[i].Effect != null) {
                        foreach (PermaSpell perma in this.ActiveCreatures) {
                            if ((this.ActiveCreatures[i].PlayerId == Player1.ID && this.ActiveCreatures[i].Effect.Target == Target.Yours) && perma.PlayerId == Player1.ID) {
                                // reverts self-buffing effects for player 1's creatures
                                this.ActiveCreatures[i].Effect.ActivateEffect(perma);
                            }
                            else if ((this.ActiveCreatures[i].PlayerId == Player2.ID && this.ActiveCreatures[i].Effect.Target == Target.Yours) && perma.PlayerId == Player2.ID) {
                                // reverts self-buffing effects for player 2's creatures
                                this.ActiveCreatures[i].Effect.ActivateEffect(perma);
                            }
                            else if ((this.ActiveCreatures[i].PlayerId == Player1.ID && this.ActiveCreatures[i].Effect.Target == Target.Opponents) && perma.PlayerId == Player2.ID) {
                                // reverts debuffing effect caused by player 1 on player 2's creatures
                                this.ActiveCreatures[i].Effect.ActivateEffect(perma);
                            }
                            else if ((this.ActiveCreatures[i].PlayerId == Player2.ID && this.ActiveCreatures[i].Effect.Target == Target.Opponents) && perma.PlayerId == Player1.ID) {
                                // reverts debuffing effect caused by player 2 on player 1's creatures
                                this.ActiveCreatures[i].Effect.ActivateEffect(perma);
                            }
                        }
                    }
                }

                // phase 2: both players draw cards until they have 7; if their deck is empty they lose
                this.TurnPhase = 2;

                this.Player1.FillHand();
                this.Player2.FillHand();
                if (this.WinConditionCheck(this.Player1, this.Player2) != "Undecided") {
                    // handle winner; end game
                }

                // phase 3: players play their cards until they end their turn (priority decided earlier applies here; they get to go first)
                this.TurnPhase = 3;
            }
        }
        public void LogActivities(int turnNumber, int turnPhase, string happening) {
            // write what happens in CardGame() to log.txt
        }
        public void PrintToConsole(int turnNumber, int turnPhase, string happening) {
            // print what happens in CardGame() to console
        }
        public string WinConditionCheck(Player p1, Player p2) {
            // checks the win conditions at various points during a turn
            // win conditions are: 1. a player having an empty deck, 2. a player losing all of their HP
            // players tie if both happen at the same time to both players

            string outcome = "Undecided";
            if ((p1.HP == 0 && p2.HP == 0) || (p1.Deck.Count == 0 && p2.Deck.Count == 0)) {
                // tie; both players have either 0 hp or empty decks
                outcome = "Tie";
            }
            else {
                // one player might win here
                if ((p1.HP == 0 && p2.HP > 0) || (p1.Deck.Count == 0 && p2.Deck.Count > 0)) {
                    // player 2 wins; either because other player has 0 hp or an empty deck
                    outcome = "Player 2";
                }
                else if ((p2.HP == 0 && p1.HP > 0) || (p2.Deck.Count == 0 && p1.Deck.Count > 0)) {
                    // player 1 wins; either because other player has 0 hp or an empty deck
                    outcome = "Player 1";
                }
            }
            return outcome;
        }
        public void AddLands(Land landToAdd) {
            // add lands to LandsOnBoard
            this.LandsOnBoard.Add(landToAdd);
        }
        public void AddPermas(PermaSpell creatureToAdd) {
            // add creature to ActiveSpells
            this.ActiveCreatures.Add(creatureToAdd);
        }
        public void AddToCardQueue(Card cardToAdd) {
            // add card to CardQueue
            this.CardQueue.Enqueue(cardToAdd);
        }
        public void HandleCardQueue() {
            // handles card effects in CardQueue when they need to activate
            for (int i = 0; i < this.CardQueue.Count; i++) {

            }
        }
    }
}