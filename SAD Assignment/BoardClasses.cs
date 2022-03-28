using System;
using System.Collections.Generic;

/*
Thijmen Bouwsema 1008331
*/

namespace SAD_Assignment
{
    /// <summary>
    /// Class <c>Game</c> defines the game to be played by 2 players w singleton pattern
    /// </summary>
    public sealed class Game {
        // singleton portion
        private static Game _instance = null;
        public static Game GetInstance(Player p1, Player p2) {
            if (_instance == null) {
                _instance = new Game(p1, p2);
            }
            return _instance;
        }
        private Game(Player p1, Player p2) {
            this.TurnNum = 1;
            this.CardActivationStack = new Stack<Card>();
            this.LandsOnBoard = new List<Land>();
            this.ActivePermas = new List<PermaSpell>();
            this.Player1 = p1;
            this.Player2 = p2;
            this.PlayerPriority = -1;
            this.Winner = "Undecided";
        }
        // TurnNum = actual turn number; increments when new turn begins
        private int TurnNum { get; set; }
        // TurnPhase: 0 = Pre-game (shuffle decks), 1 = reset temp effects, 2 = both players draw 1 card (if possible), 
        //            3 = prioritised player plays their cards, 4 = other player plays their cards, 
        //            5 = both players discard cards until they have 7 each (if possible)
        // after this, TurnPhase is reset to 0 when TurnNum is incremented
        private int TurnPhase { get; set; }
        // PlayerPriority decides who goes first during each turn (decided randomly at the start of the game)
        private int PlayerPriority { get; set; }
        // CardActivationStack contains all cards to be played (stored here to be activated after eachother in order later on)
        private Stack<Card> CardActivationStack { get; set; }
        // LandsOnBoard holds all lands currently in play
        private List<Land> LandsOnBoard { get; set; }
        // ActiveSpells holds all permanent spells in play
        private List<PermaSpell> ActivePermas { get; set; }
        private Player Player1 { get; set; }
        private Player Player2 { get; set; }
        // Winner: is "Undecided" while playing, otherwise will say "Player 1", "Player 2" or "Tie" depending on who wins (not used since in case no one is able to win or lose)
        public string Winner { get; set; }
        
        /// <summary>
        /// Method <c>PlayGame</c> will play out the required turns with no player interactivity (has rigid order of instructions to match assignment case)
        /// </summary>
        public void PlayGame() {
            // start of game-simulation
            LogActivities("Starting game...\n");
            // Pre-game prep (Decks are shuffled & hands are filled)
            this.Player1.FillHand();
            LogActivities($"Player 1 deck has been shuffled & hand has been filled with 7 cards.");
            this.Player2.FillHand();
            LogActivities($"Player 2 deck has been shuffled & hand has been filled with 7 cards.\n");

            while (this.TurnNum < 4) {
                LogActivities($"-------------------\nStarting Turn {this.TurnNum}.\n-------------------");

                if (this.TurnNum == 1) {
                    // player turn 1 happens

                    // Player 1 goes first
                    LogActivities("\nStarting Player 1's turn...");
                    LogActivities("\nAdding 1 card Player 1's hand.");
                    this.Player1.FillHand(1);
                    PlayerTurn(this.Player1, this.Player2, this.TurnNum);

                    // then Player 2 goes
                    LogActivities("\nStarting Player 2's turn...");
                    LogActivities("\nAdding 1 card Player 2's hand.");
                    this.Player2.FillHand(1);
                    PlayerTurn(this.Player2, this.Player1, this.TurnNum);
                }
                else if (this.TurnNum == 2) {
                    // player turn 2 happens

                    // Player 1 goes first
                    LogActivities("\nStarting Player 1's turn...");
                    LogActivities("\nAdding 1 card Player 1's hand.");
                    this.Player1.FillHand(1);
                    PlayerTurn(this.Player1, this.Player2, this.TurnNum);

                    // then Player 2 goes
                    LogActivities("\nStarting Player 2's turn...");
                    LogActivities("\nAdding 1 card Player 2's hand.");
                    this.Player2.FillHand(1);
                    PlayerTurn(this.Player2, this.Player1, this.TurnNum);
                }
                else {
                    // player turn 3 happens

                    // Player 1 goes first
                    LogActivities("\nStarting Player 1's turn...");
                    LogActivities("\nAdding 1 card Player 1's hand.");
                    this.Player1.FillHand(1);
                    PlayerTurn(this.Player1, this.Player2, this.TurnNum);

                    // Player 2 does not go afterwards (is not in deliverable scenario for some reason)
                }

                // mark the end of the turn by incrementing this.TurnNum
                LogActivities($"\nEnd of turn {this.TurnNum}.");

                // log end-of-turn standings to file
                LogActivities($"Turn {this.TurnNum} standings:\n");

                LogActivities($"Player 1: Amount in hand = {this.Player1.Hand.Count}, Deck size = {this.Player1.Deck.Count()}, Amount of energy = {this.Player1.Energy} & Amount of HP = {this.Player1.HP}.");
                LogActivities($"Player 2: Amount in hand = {this.Player2.Hand.Count}, Deck size = {this.Player2.Deck.Count()}, Amount of energy = {this.Player2.Energy} & Amount of HP = {this.Player2.HP}.");

                LogActivities($"\nAmount of lands on board = {this.LandsOnBoard.Count}:");
                if (this.LandsOnBoard.Count > 0) {
                    foreach (Land land in this.LandsOnBoard) {
                        LogActivities($"[Owner = Player {land.PlayerId}]");
                    }
                }

                LogActivities($"\nAmount of permas on board = {this.ActivePermas.Count}:");
                if (this.ActivePermas.Count > 0) {
                    foreach (PermaSpell perma in this.ActivePermas) {
                        LogActivities($"[Owner = Player {perma.PlayerId}] : {perma.CardName} has {perma.Attack} Attack & {perma.HP} HP.");
                        perma.DecrementTurns();
                    }
                }
                this.TurnNum++;
            }
            Console.WriteLine("Sample game finished; find results in log.txt");
        }
        /// <summary>
        /// Method <c>SimulatedPlayerTurn</c> simulates a player playing a turn (not interactive since the assignment description apparantly says so)
        /// </summary>
        private void PlayerTurn(Player p, Player otherP, int currentTurn) {
            // reset lands at the beginning of the turn
            if (this.LandsOnBoard.Count > 0) {
                for (int i = 0; i < this.LandsOnBoard.Count; i++) {
                    // resets all lands on board so they can be used during later phases
                    this.LandsOnBoard[i].ResetLand();
                }
            }

            if (currentTurn == 1) {
                // turn 1 happenings
                if (p.ID == 1) {
                    // player 1 plays 2 lands & nothing else
                    LogActivities($"Player {p.ID} has chosen to play a land card.");
                    this.AddLand(p.Hand[0] as Land);
                    p.DiscardHand(p.Hand[0]);

                    LogActivities($"Player {p.ID} has chosen to play a land card.");
                    this.AddLand(p.Hand[0] as Land);
                    p.DiscardHand(p.Hand[0]);

                    LogActivities($"Player {p.ID}'s turn has ended.");
                }
                else {
                    // player 2 plays 1 land & nothing else
                    LogActivities($"Player {p.ID} has chosen to play a land card.");
                    this.AddLand(p.Hand[0] as Land);
                    p.DiscardHand(p.Hand[0]);

                    LogActivities($"Player {p.ID}'s turn has ended.");
                }
            }
            else if (currentTurn == 2) {
                // turn 2 happenings
                if (p.ID == 1) {
                    // player 1 plays 1 land, harvests 2 lands, casts the blue perma (which removes 1 card from opponent's hand)

                    // play 1 land
                    LogActivities($"Player {p.ID} has chosen to play a land card.");
                    this.AddLand(p.Hand[0] as Land);
                    p.DiscardHand(p.Hand[0]);

                    // harvest energy from 2 lands
                    LogActivities($"Player {p.ID} has chosen to use one of their active lands to generate an energy point.");
                    this.LandsOnBoard[0].GenerateEnergy();

                    LogActivities($"Player {p.ID} has chosen to use one of their active lands to generate an energy point.");
                    this.LandsOnBoard[1].GenerateEnergy();
                    
                    // cast blue creature (and discard 1 opponent card)
                    LogActivities($"Player {p.ID} has chosen to play a creature card.");
                    PermaSpell creature = p.Hand[0] as PermaSpell;
                    // check if creature has discard card effect; activate effect
                    if (creature.EffectType == EffectType.ForceDiscard) {
                        LogActivities($"Player {p.ID}'s creature has forced Player {otherP.ID} to discard 1 random card from their hand.");
                        otherP.DiscardHand(1);
                    }
                    this.AddPerma(creature, p);
                    p.DiscardHand(p.Hand[0]);

                    LogActivities($"Player {p.ID}'s turn has ended.");
                }
                else {
                    // player 2 does nothing :)
                    LogActivities($"Player {p.ID}'s turn has ended, but they did not perform any actions.");
                }
            }
            else {
                // turn 3 happenings

                // player 1 harvests 3 energy from lands, sets creature to attacking state & uses +3/+3 green buff on it
                // player 2 counters green buff by casting red counter (harvests 1 energy first)
                // player 1 counters red counter by casting blue counter (harvests 2 energy first)

                // harvest 3 energy
                List<Land> player1Lands = GetPlayerLands(p);
                foreach (Land land in player1Lands) {
                    LogActivities($"Player {p.ID} has chosen to use one of their active lands to generate an energy point.");
                    land.GenerateEnergy();
                }

                // set creature to attack & add to cardstack
                this.ActivePermas[0].State = CardState.Attacking;
                this.AddToCardStack(this.ActivePermas[0]);

                // set creature target if there is a defending creature active (should not be the case in example)
                List<PermaSpell> player2Creatures = GetPlayerPermas(otherP);
                foreach (PermaSpell creature in player2Creatures) {
                    if (creature.State == CardState.Defending) {
                        this.ActivePermas[0].TargetPerma = creature;
                    }
                }

                // buff creature (remove from hand & add to stack)
                this.AddToCardStack(p.Hand[0]);
                p.DiscardHand(p.Hand[0]);

                // Player 2 interjects; harvests 1 energy then uses red counter
                LogActivities($"\nPlayer {otherP.ID} has decided to interject!");
                this.InterruptPlayerTurn(otherP, p);

                // Player 1 interjects; harvests 2 energy then uses blue counter
                LogActivities($"\nPlayer {p.ID} has decided to interject!");
                this.InterruptPlayerTurn(p, otherP);

                LogActivities($"Player {p.ID}'s turn has ended.");
            }
            // handle stack of cards
            this.HandleActivationStack();
        }
        /// <summary>
        /// Method <c>SimulatedPlayerInterrupt</c> lets specified player interrupt the specified other player's turn (simulation of)
        /// </summary>
        private void InterruptPlayerTurn(Player p, Player otherP) {
            // reset lands at the beginning of the turn
            if (this.LandsOnBoard.Count > 0) {
                for (int i = 0; i < this.LandsOnBoard.Count; i++) {
                    // resets all lands on board so they can be used during later phases
                    this.LandsOnBoard[i].ResetLand();
                }
            }
            // get current player's lands
            List<Land> playerLands = GetPlayerLands(p);
            
            if (p.ID == 1) {
                // player 1 harvests 2 energy, then uses blue counter

                // harvests 2 energy
                LogActivities($"Player {p.ID} has chosen to use one of their active lands to generate an energy point.");
                playerLands[0].GenerateEnergy();
                LogActivities($"Player {p.ID} has chosen to use one of their active lands to generate an energy point.");
                playerLands[1].GenerateEnergy();

                // uses blue counter
                LogActivities($"Player {p.ID} has chosen to use one of their counter spells.");
                this.AddToCardStack(p.Hand[0]);
                p.DiscardHand(p.Hand[0]);
            }
            else if (p.ID == 2) {
                // player 2 harvests 1 energy, then uses red counter

                // harvests 1 energy
                foreach (Land land in playerLands) {
                    LogActivities($"Player {p.ID} has chosen to use one of their active lands to generate an energy point.");
                    land.GenerateEnergy();
                }

                // uses red counter
                LogActivities($"Player {p.ID} has chosen to use one of their counter spells.");
                this.AddToCardStack(p.Hand[0]);
                p.DiscardHand(p.Hand[0]);
            }
        }
        private void HandleActivationStack() {
            // handles card effects in CardQueue when they need to activate
            if (this.CardActivationStack.Count > 0) {
                LogActivities($"\nActivating the card stack.");
            }

            while (this.CardActivationStack.Count > 0) {
                Card stackCard = this.CardActivationStack.Pop();

                // check if card is a counter; if so remove next item (if there is another in there)
                if (this.CardActivationStack.Count > 0 && stackCard.EffectType == EffectType.Counter) {
                    LogActivities($"Player {stackCard.PlayerId}'s {stackCard.CardName} has been activated; removed {this.CardActivationStack.Peek().CardName} from card stack.");
                    this.CardActivationStack.Pop();
                }
                // check if card is a buff; buff next item in stack (if next item exists & is a perma)
                else if (this.CardActivationStack.Count > 0 && stackCard.EffectType == EffectType.Buff && this.CardActivationStack.Peek().CardType == CardType.PermaSpell) {
                    // get relevant cards
                    PermaSpell permaToBuff = this.CardActivationStack.Peek() as PermaSpell;
                    InstaSpell currentBuff = stackCard as InstaSpell;
                    LogActivities($"Player {stackCard.PlayerId}'s {stackCard.CardName} has been activated; buffing {permaToBuff.CardName} by +{currentBuff.HPAugment}/+{currentBuff.AttackAugment}");

                    // set target & activate
                    currentBuff.SetTarget(permaToBuff);
                    currentBuff.ActivateEffect();
                }
                // attack with perma (if target is null; attack opposing player)
                else if (stackCard.CardType == CardType.PermaSpell) {
                    PermaSpell attackingPerma = stackCard as PermaSpell;
                    LogActivities($"Player {stackCard.PlayerId}'s {attackingPerma.CardName} is going to attack it's opponent (Creature has {attackingPerma.Attack} Attack & {attackingPerma.HP} HP).");
                    if (attackingPerma.State == CardState.Attacking) {
                        // update target (if stored target is dead; will attack player instead)
                        attackingPerma.DoAttack();
                    }
                }
            }
            // remove dead permas from play
            for (int i = 0; i < this.ActivePermas.Count; i++) {
                if (this.ActivePermas[i].HP <= 0) {
                    this.ActivePermas.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// Method <c>EmptyLogFile</c> Is used to create/overwrite log.txt at the beginning of a new game
        /// </summary>
        public static void EmptyLogFile() {
            System.IO.File.WriteAllText("./log.txt", "");
        }
        /// <summary>
        /// Method <c>LogActivities</c> writes specified string to log.txt
        /// </summary>
        public static void LogActivities(string stringToLog) {
            using (System.IO.StreamWriter sw = System.IO.File.AppendText("./log.txt")) {
                sw.WriteLine(stringToLog);
            }
        }
        /// <summary>
        /// Method <c>AddLand</c> adds specified land to list of active lands
        /// </summary>
        private void AddLand(Land landToAdd) {
            // add lands to LandsOnBoard
            LogActivities("A land has been played and added to the list of active lands.");
            landToAdd.State = CardState.AlreadyUsed;
            this.LandsOnBoard.Add(landToAdd);
        }
        /// <summary>
        /// Method <c>AddPerma</c> adds specified PermaSpell to list of active permas
        /// </summary>
        private void AddPerma(PermaSpell permaToAdd, Player p) {
            // add perma to ActiveSpells & subtract energy from player
            LogActivities("A creature has been played and added to the list of active creatures.");
            this.ActivePermas.Add(permaToAdd);
            p.ChangeEnergy(permaToAdd.EnergyCost * -1);
        }
        /// <summary>
        /// Method <c>AddToCardStack</c> adds specified card to stack of cards to be activated
        /// </summary>
        private void AddToCardStack(Card cardToAdd) {
            // add card to CardStack
            LogActivities($"A {cardToAdd.CardName} has been added to the card stack.");
            this.CardActivationStack.Push(cardToAdd);
        }
        /// <summary>
        /// Method <c>GetPlayerLands</c> gives list of active lands owned by specified player
        /// </summary>
        public List<Land> GetPlayerLands(Player p) {
            List<Land> returnList = new List<Land>();
            foreach (Land land in this.LandsOnBoard) {
                if (land.PlayerId == p.ID) {
                    returnList.Add(land);
                }
            }
            return returnList;
        }
        /// <summary>
        /// Method <c>GetPlayerCreatures</c> gives list of active creatures owned by specified player
        /// </summary>
        public List<PermaSpell> GetPlayerPermas(Player p) {
            List<PermaSpell> returnList = new List<PermaSpell>();
            foreach (PermaSpell perma in this.ActivePermas) {
                if (perma.PlayerId == p.ID) {
                    returnList.Add(perma);
                }
            }
            return returnList;
        }

        // unused methods in required implementation (necessary for class diagram)


        /// <summary>
        /// Method <c>PrintToConsole</c> prints to console based on given parameters
        /// </summary>
        private static void PrintToConsole(string happening, Player p, List<Land> lands = null, List<PermaSpell> perma = null, List<InstaSpell> counters = null) {
            // not implemented; deliverable case does not require it's implementation (case does not require a GUI)
        }
        /// <summary>
        /// Method <c>WinConditionCheck</c> checks if a win condition has been satisfied
        /// </summary>
        private string WinConditionCheck() {
            // not implemented; deliverable case does not require it's implementation (no win condition is satisfied in example)
            return null;
        }
        /// <summary>
        /// Method <c>RevertPermaEffects</c> reverts effects of active permas and removes expired cards from board
        /// </summary>
        private void RevertPermaEffects() {
            // not implemented; deliverable case does not require it's implementation (no effects to revert)
        }
        /// <summary>
        /// Method <c>AcivatePermaEffects</c> activates effects of active permas
        /// </summary>
        private void AcivatePermaEffects() {
            // not implemented; deliverable case does not require it's implementation (no effects to re-activate after RevertPermaEffects())
        }
        /// <summary>
        /// Method <c>HandleActivationStack</c> handles queue of cards to be activated
        /// </summary>
    }
}