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
            this.TurnNum = 1;
            this.CardQueue = new Queue<Card>();
            this.InstaCardChain = new Stack<InstaSpell>();
            this.LandsOnBoard = new List<Land>();
            this.ActiveCreatures = new List<PermaSpell>();
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
            
            Console.WriteLine("entering StartGame() whileloop");
            // while-loop that keeps game going until winner is decided
            while (this.Winner == "Undecided") {

                // phase 1: revert temporary effects (applies them again at end of this phase), reset lands & remove expired creatures 
                Console.WriteLine("starting turnphase 1");
                this.TurnPhase = 1;

                // revert creature's effects and check if they're expired
                Console.WriteLine($"checking this.ActiveCreatures length = {this.ActiveCreatures.Count}");
                if (this.ActiveCreatures.Count > 0) {
                    this.RevertPermaEffects();
                }
                
                Console.WriteLine($"checking this.LandsOnBoard length = {this.LandsOnBoard.Count}");
                if (this.LandsOnBoard.Count > 0) {
                    for (int i = 0; i < this.LandsOnBoard.Count; i++) {
                        // resets all lands on board so they can be used during later phases
                        this.LandsOnBoard[i].ResetLand();
                    }
                }

                // re-apply creature effects
                if (this.ActiveCreatures.Count > 0) {
                    this.AcivatePermaEffects();
                }

                // phase 2: both players draw cards until they have 7; if their deck is empty they lose
                Console.WriteLine("starting turnphase 2");
                this.TurnPhase = 2;

                this.Player1.FillHand();
                this.Player2.FillHand();
                string winner = this.WinConditionCheck(this.Player1, this.Player2);
                if (winner != "Undecided") {
                    // handle winner; end game
                    Console.WriteLine($"winner is {winner}");
                    this.Winner = winner;
                    continue;
                }
                Console.WriteLine("winner is undecided");

                // phase 3: players play their cards until they end their turn (priority decided earlier applies here; they get to go first)
                Console.WriteLine("starting turnphase 3");
                this.TurnPhase = 3;

                // check priority; player 1 moves first
    	        if (this.PlayerPriority == 0) {
                    // player 1 goes first
                    this.PlayerTurn(this.Player1, this.Player2);

                    // check for winCondition (player might have KOd their opponent)
                    winner = this.WinConditionCheck(this.Player1, this.Player2);
                    if (winner != "Undecided") {
                        // handle winner; end game
                        Console.WriteLine($"winner is {winner}");
                        this.Winner = winner;
                        continue;
                    }
                    Console.WriteLine("winner is undecided");

                    // then player 2 gets to play
                    this.PlayerTurn(this.Player2, this.Player1);

                    // check for winCondition (player might have KOd their opponent)
                    winner = this.WinConditionCheck(this.Player1, this.Player2);
                    if (winner != "Undecided") {
                        // handle winner; end game
                        Console.WriteLine($"winner is {winner}");
                        this.Winner = winner;
                        continue;
                    }
                    Console.WriteLine("winner is undecided");
                }
                // check priority; player 2 moves first
                else {
                    // player 2 goes first
                    this.PlayerTurn(this.Player2, this.Player1);

                    // check for winCondition (player might have KOd their opponent)
                    winner = this.WinConditionCheck(this.Player1, this.Player2);
                    if (winner != "Undecided") {
                        // handle winner; end game
                        Console.WriteLine($"winner is {winner}");
                        this.Winner = winner;
                        continue;
                    }
                    Console.WriteLine("winner is undecided");

                    // then player 1 gets to play
                    this.PlayerTurn(this.Player1, this.Player2);

                    // check for winCondition (player might have KOd their opponent)
                    winner = this.WinConditionCheck(this.Player1, this.Player2);
                    if (winner != "Undecided") {
                        // handle winner; end game
                        Console.WriteLine($"winner is {winner}");
                        this.Winner = winner;
                        continue;
                    }
                    Console.WriteLine("winner is undecided");
                }

                // phase 4: turn has ended; players discard their cards in hand until they have 7 each
                Console.WriteLine("starting turnphase 4");
                this.TurnPhase = 4;

                // players discard random cards in their hands until they have 7 each
                this.Player1.DiscardHand(this.Player1.Hand.Count % 7);
                this.Player2.DiscardHand(this.Player2.Hand.Count % 7);
                Console.WriteLine($"end of turn {this.TurnNum}");
                this.TurnNum++;
                System.Threading.Thread.Sleep(2000);
            }
            Console.WriteLine($"game has ended with {this.Winner} as endresult");
        }
        public void LogActivities(string stringToLog) {
            // write what happens in CardGame() to log.txt (gets called in PrintToConsole())
        }
        public void PrintToConsole(string happening, Player p, string chosenAction) {
            // print what happens in PlayerTurn() to console

            // prints every card in player's hand
            if (happening == "cardsInHand") {
                Console.WriteLine($"\n Cards in Player {p.ID}'s hand (amount = {p.Hand.Count}):");
                int cardNumTracker = 1;
                foreach (Card card in p.Hand) {
                    Console.WriteLine($"[{cardNumTracker}] {card.Type} : {card.CardName} ({card.CardDescription}) | Costs {card.EnergyCost} {card.Color} energy");
                    cardNumTracker++;
                }
            }
            // prints the player's active lands and creatures
            if (happening == "activeLandsAndCreatures") {
                // prints every land on board owned by player
                List<Land> landsOwnedByPlayer = new List<Land>();
                foreach (Land land in this.LandsOnBoard) {
                    if (land.PlayerId == p.ID) {
                        landsOwnedByPlayer.Add(land);
                    }
                }
                Console.WriteLine($"\n Lands on the board owned by Player {p.ID} (amount = {landsOwnedByPlayer.Count}):");
                int cardNumTracker = p.Hand.Count;
                foreach (Land land in landsOwnedByPlayer) {
                    Console.WriteLine($"[{cardNumTracker}] {land.CardName} ({land.CardDescription}) | Color = {land.Color}");
                    cardNumTracker++;
                }

                // prints every active creature owned by player
                List<PermaSpell> creaturesOwnedByPlayer = new List<PermaSpell>();
                foreach (PermaSpell creature in this.ActiveCreatures) {
                    if (creature.PlayerId == p.ID) {
                        creaturesOwnedByPlayer.Add(creature);
                    }
                }
                Console.WriteLine($"\n Active creatures on the board owned by Player {p.ID} (amount = {creaturesOwnedByPlayer.Count})");
                foreach (PermaSpell creature in creaturesOwnedByPlayer) {
                    Console.WriteLine($"[{cardNumTracker}] {creature.CardName} ({creature.CardDescription}) | Attack = {creature.Attack} & Defense = {creature.HP} | Costs {creature.EnergyCost} {creature.Color} energy");
                }
            }
        }
        public void PlayerTurn(Player p, Player otherP) {
            // lets the specified player play their cards until they decide to cancel their turn
            
            // player turn will last until they cancel their turn
            bool playerTurnOngoing = true;
            while (playerTurnOngoing) {
                // print hand (move to PrintToConsole method later)
                PrintToConsole("cardsInHand", p, null);
                PrintToConsole("activeLandsAndCreatures", p, null);

                // present options for player (after moving printing to method, fix hand and activecreatures being conditional & add lands)
                Console.WriteLine($"\n Possible actions: 1-{p.Hand.Count}) Play a card in your hand, {p.Hand.Count + 1}-{(p.Hand.Count) + this.ActiveCreatures.Count}) Play one of your Creatures on the board, 0) End your turn");
                Console.Write("Please select an action: ");

                // reads input & checks if it is able to become an int
                int chosenAction = 0;
                bool choiceIsInt = int.TryParse(Console.ReadLine(), out chosenAction);
                
                // end player's turn if they chose to or if their hand is empty
                if (choiceIsInt == true && (chosenAction == 0 || p.Hand.Count <= 0)) {
                    // ends the player's turn
                    playerTurnOngoing = false;
                    continue;
                }
                // play the player's chosen card
                else if (choiceIsInt == true && (chosenAction >= 1 && chosenAction <= p.Hand.Count)) {
                    // plays the chosen card
                    Console.WriteLine($"play card number {chosenAction - 1} here :) (would be {p.Hand[chosenAction - 1].CardName})");
                    Card chosenCard = p.Hand[chosenAction - 1];

                    // check if chosen card is a land; add to this.LandsOnBoard
                    if (chosenCard.Type == Type.Land) {
                        this.AddLands(chosenCard as Land);
                        p.DiscardHand(chosenCard);
                    }
                    // check if chosen card is instant spell
                    else if (chosenCard.Type == Type.InstantSpell) {
                        Console.WriteLine("make instantspells work here :)");
                    }
                    // check if chosen card is permanent spell; add to this.ActiveCreatures
                    else if (chosenCard.Type == Type.PermanentSpell) {
                        this.AddPermas(chosenCard as PermaSpell);
                        p.DiscardHand(chosenCard);
                    }
                }
                // play one of the player's active creatures
                else if (this.ActiveCreatures.Count > 0 && choiceIsInt == true && (chosenAction > p.Hand.Count && chosenAction <= (p.Hand.Count + 1) + this.ActiveCreatures.Count)) {
                    Console.WriteLine($"play an active creature here :) (creature chosen was {this.ActiveCreatures[chosenAction - 1 - p.Hand.Count].CardName})");
                }
            }
            Console.WriteLine($"Player {p.ID}'s turn has ended");
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
            Console.WriteLine($"Estimated outcome = {outcome}, actual stats are \n Player 1: deck size = {p1.Deck.Count}, HP = {p1.HP} \n Player 2: deck size = {p2.Deck.Count}, HP = {p2.HP}");
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
        public void RevertPermaEffects() {
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
        }
        public void AcivatePermaEffects() {
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
        }
        public void HandleCardQueue() {
            // handles card effects in CardQueue when they need to activate
            if (this.CardQueue.Count > 0) {
                for (int i = 0; i < this.CardQueue.Count; i++) {

                }
            }
        }
    }
}