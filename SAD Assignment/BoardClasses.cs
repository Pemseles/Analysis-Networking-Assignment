using System;
using System.Threading;
using System.Collections.Generic;

namespace SAD_Assignment
{
    /// <summary>
    /// Class <c>Game</c> defines the game to be played by 2 players
    /// </summary>
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
        // CardActivationStack contains all cards to be played (stored here to be activated after eachother in order later on)
        public Stack<Card> CardActivationStack { get; set; }
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
            this.CardActivationStack = new Stack<Card>();
            this.LandsOnBoard = new List<Land>();
            this.ActiveCreatures = new List<PermaSpell>();
            this.Player1 = p1;
            this.Player2 = p2;
            this.PlayerPriority = -1;
            this.Winner = "Undecided";
        }
        /// <summary>
        /// Method <c>StartGame</c> is the main method simulating the card game
        /// </summary>
        public void StartGame() {
            // main method
            LogActivities("Starting game...\n");
            // Pre-game prep (PlayerPriority is determined, Decks are shuffled & hands are filled)
            this.TurnPhase = 0;
            this.PlayerPriority = this.rnd.Next(2);
            LogActivities($"Player priority decided. Player {this.PlayerPriority + 1} moves first.");
            this.Player1.ShuffleDeck();
            this.Player1.FillHand();
            LogActivities($"Player 1 deck has been shuffled & hand has been filled with 7 cards.");
            this.Player2.ShuffleDeck();
            this.Player2.FillHand();
            LogActivities($"Player 2 deck has been shuffled & hand has been filled with 7 cards.\n");
            
            // while-loop that keeps game going until winner is decided
            while (this.Winner == "Undecided") {
                LogActivities($"Starting Turn {this.TurnNum}.\n-------------------\n");

                // phase 1: revert temporary effects (applies them again at end of this phase), reset lands & remove expired creatures 
                LogActivities("Starting TurnPhase 1.");
                this.TurnPhase = 1;

                // revert creature's effects and check if they're expired
                if (this.ActiveCreatures.Count > 0) {
                    LogActivities("Reverting active creatures' effects.");
                    this.RevertPermaEffects();
                }
                
                if (this.LandsOnBoard.Count > 0) {
                    LogActivities("Resetting lands so they can be used again.");
                    for (int i = 0; i < this.LandsOnBoard.Count; i++) {
                        // resets all lands on board so they can be used during later phases
                        this.LandsOnBoard[i].ResetLand();
                    }
                }

                // re-apply creature effects
                if (this.ActiveCreatures.Count > 0) {
                    LogActivities("Re-Activating active creatures' effects.");
                    this.AcivatePermaEffects();
                }

                // phase 2: both players draw cards until they have 7; if their deck is empty they lose
                LogActivities("\nStarting TurnPhase 2.\n");
                this.TurnPhase = 2;

                LogActivities("Adding 1 card to each player's hand.");
                this.Player1.FillHand(1);
                this.Player2.FillHand(1);
                LogActivities("Checking win conditions.");
                string winner = this.WinConditionCheck(this.Player1, this.Player2);
                if (winner != "Undecided") {
                    // handle winner; end game
                    LogActivities($"Winner has been decided: result is {winner}.");
                    this.Winner = winner;
                    continue;
                }
                LogActivities("Winner is undecided; game continues.");

                // phase 3: players play their cards until they end their turn (priority decided earlier applies here; they get to go first)
                LogActivities("\nStarting Turnphase 3.");
                this.TurnPhase = 3;

                // check priority; player 1 moves first
    	        if (this.PlayerPriority == 0) {
                    // player 1 goes first
                    LogActivities("\nStarting Player 1's turn...");
                    Thread.Sleep(700);
                    this.PlayerTurn(this.Player1, this.Player2);

                    // check for winCondition (player might have KOd their opponent)
                    LogActivities("Checking win conditions.");
                    winner = this.WinConditionCheck(this.Player1, this.Player2);
                    if (winner != "Undecided") {
                        // handle winner; end game
                        LogActivities($"Winner has been decided: result is {winner}.");
                        this.Winner = winner;
                        continue;
                    }
                    LogActivities("Winner is undecided; game continues.");

                    // then player 2 gets to play
                    LogActivities("\nStarting Player 2's turn...");
                    Thread.Sleep(700);
                    this.PlayerTurn(this.Player2, this.Player1);

                    // check for winCondition (player might have KOd their opponent)
                    LogActivities("Checking win conditions.");
                    winner = this.WinConditionCheck(this.Player1, this.Player2);
                    if (winner != "Undecided") {
                        // handle winner; end game
                        LogActivities($"Winner has been decided: result is {winner}.");
                        this.Winner = winner;
                        continue;
                    }
                    LogActivities("Winner is undecided; game continues.");
                }
                // check priority; player 2 moves first
                else {
                    // player 2 goes first
                    LogActivities("\nStarting Player 2's turn...");
                    Thread.Sleep(700);
                    this.PlayerTurn(this.Player2, this.Player1);

                    // check for winCondition (player might have KOd their opponent)
                    LogActivities("Checking win conditions.");
                    winner = this.WinConditionCheck(this.Player1, this.Player2);
                    if (winner != "Undecided") {
                        // handle winner; end game
                        LogActivities($"Winner has been decided: result is {winner}.");
                        this.Winner = winner;
                        continue;
                    }
                    LogActivities("Winner is undecided; game continues.");

                    // then player 1 gets to play
                    LogActivities("\nStarting Player 1's turn...");
                    Thread.Sleep(700);
                    this.PlayerTurn(this.Player1, this.Player2);

                    // check for winCondition (player might have KOd their opponent)
                    LogActivities("Checking win conditions.");
                    winner = this.WinConditionCheck(this.Player1, this.Player2);
                    if (winner != "Undecided") {
                        // handle winner; end game
                        LogActivities($"Winner has been decided: result is {winner}.");
                        this.Winner = winner;
                        continue;
                    }
                    LogActivities("Winner is undecided; game continues.");
                }
                Thread.Sleep(700);

                // phase 4: turn has ended; players discard their cards in hand until they have 7 each
                LogActivities("\nStarting TurnPhase 4.");
                this.TurnPhase = 4;

                // players discard random cards in their hands until they have 7 each
                if (this.Player1.Hand.Count > 7) {
                    LogActivities("\nPlayer 1 discards random cards until they have 7.");
                    this.Player1.DiscardHand(this.Player1.Hand.Count - 7);
                }
                if (this.Player2.Hand.Count > 7) {
                    LogActivities("Player 2 discards random cards until they have 7.");
                    this.Player2.DiscardHand(this.Player2.Hand.Count - 7);
                }

                // mark the end of the turn by incrementing this.TurnNum
                LogActivities($"\nend of turn {this.TurnNum}.\n");

                // log end-of-turn standings to file
                LogActivities($"End of turn {this.TurnNum} standings:\n");

                LogActivities($"Player 1: Amount in hand = {this.Player1.Hand.Count}, Deck size = {this.Player1.Deck.Count}, Amount of energy = {this.Player1.Energy}, Amount of HP = {this.Player1.HP}");
                LogActivities($"Player 2: Amount in hand = {this.Player2.Hand.Count}, Deck size = {this.Player2.Deck.Count}, Amount of energy = {this.Player2.Energy}, Amount of HP = {this.Player2.HP}");

                LogActivities($"\nAmount of lands on board = {this.LandsOnBoard.Count}");
                if (this.LandsOnBoard.Count > 0) {
                    foreach (Land land in this.LandsOnBoard) {
                        LogActivities($"[Owner = Player {land.PlayerId}] : {land.CardName}");
                    }
                }

                LogActivities($"\nAmount of creatures on board = {this.ActiveCreatures.Count}");
                if (this.ActiveCreatures.Count > 0) {
                    foreach (PermaSpell creature in this.ActiveCreatures) {
                        LogActivities($"[Owner = Player {creature.PlayerId}] : {creature.CardName} has {creature.Attack} Attack, {creature.HP} HP & has {creature.TurnsLeft} turns left");
                        creature.DecrementTurns();
                    }
                }

                this.TurnNum++;
                Thread.Sleep(700);
            }
            LogActivities($"----------------\ngame has ended with {this.Winner} as endresult.\n----------------");
        }
        /// <summary>
        /// Method <c>LogActivities</c> writes specified string to log.txt
        /// </summary>
        public static void EmptyLogFile() {
            System.IO.File.WriteAllText("./log.txt", "");
        }
        public static void LogActivities(string stringToLog) {
            using (System.IO.StreamWriter sw = System.IO.File.AppendText("./log.txt")) {
                sw.WriteLine(stringToLog);
            }
        }
        /// <summary>
        /// Method <c>PrintToConsole</c> prints to console based on given parameters
        /// </summary>
        public static void PrintToConsole(string happening, Player p, List<Land> lands = null, List<PermaSpell> creatures = null, List<InstaSpell> counters = null, string chosenAction = null) {
            // print what happens in PlayerTurn() to console

            // prints every card in player's hand
            if (happening == "cardsInHand") {
                Console.WriteLine($"\nCards in Player {p.ID}'s hand (amount = {p.Hand.Count}):");
                int cardNumTracker = 1;
                foreach (Card card in p.Hand) {
                    Console.WriteLine($"[{cardNumTracker}] {card.Type} : {card.CardName} ({card.CardDescription}) | Costs {card.EnergyCost} energy");
                    cardNumTracker++;
                }
            }
            // prints the player's active lands and creatures
            else if (happening == "activeLandsAndCreatures") {
                // prints every land on board owned by player
                Console.WriteLine($"\nLands on the board owned by Player {p.ID} (amount = {lands.Count}):");
                int cardNumTracker = p.Hand.Count + 1;
                foreach (Land land in lands) {
                    Console.WriteLine($"[{cardNumTracker}] {land.CardName} ({land.CardDescription}) | Color = {land.Color}");
                    cardNumTracker++;
                }

                // prints every active creature owned by player
                Console.WriteLine($"\nActive creatures on the board owned by Player {p.ID} (amount = {creatures.Count})");
                foreach (PermaSpell creature in creatures) {
                    string creatureAction = "Do nothing";
                    if (creature.State == 2) {
                        // creature is going to be attacking
                        creatureAction = "Attack";
                    }
                    else if (creature.State == 3) {
                        // creature is going to be defending
                        creatureAction = "Defend";
                    }
                    Console.WriteLine($"[{cardNumTracker}] {creature.CardName} ({creature.CardDescription}) | Attack = {creature.Attack}, Defense = {creature.HP}, Turns left = {creature.TurnsLeft} | Is going to {creatureAction}");
                    cardNumTracker++;
                }
            }
            // prints the base options presented to the player
            else if (happening == "presentOptions") {
                Console.WriteLine("\n[0] End your turn");
                Console.WriteLine("[-1] Forfeit the game\n");

                string actionsStr = "Possible actions: ";
                if (p.Hand.Count > 0) {
                    if (p.Hand.Count == 1) {
                        actionsStr = actionsStr + $"1) Play card in your hand, ";
                    }
                    else {
                        actionsStr = actionsStr + $"1-{p.Hand.Count}) Play card in your hand, ";
                    }
                }
                if (lands.Count > 0) {
                    if (lands.Count == 1) {
                        actionsStr = actionsStr + $"{p.Hand.Count + 1}) Generate energy with land, ";
                    }
                    else {
                        actionsStr = actionsStr + $"{p.Hand.Count + 1}-{p.Hand.Count + lands.Count}) Generate energy with land, ";
                    }
                }
                if (creatures.Count > 0) {
                    if (creatures.Count == 1) {
                        actionsStr = actionsStr + $"{p.Hand.Count + 1 + lands.Count}) Attack/Defend with creature, ";
                    }
                    else {
                        actionsStr = actionsStr + $"{p.Hand.Count + 1 + lands.Count}-{p.Hand.Count + lands.Count + creatures.Count}) Attack/Defend with creature, ";
                    }
                }
                actionsStr = actionsStr + "0) End turn, -1) Forfeit game.";
                Console.WriteLine($"Available energy: {p.Energy}");
                Console.WriteLine($"{actionsStr}");
                Console.Write("Please choose the number associated with desired action: ");
            }
            else if (happening == "playCreature") {
                Console.WriteLine("\n[1] Attack with chosen creature");
                Console.WriteLine("[2] Do nothing");
                Console.Write("Please choose the number associated with desired action: ");
            }
            else if (happening == "defendCreature") {
                Console.WriteLine($"\nIt's Player {p.ID}'s turn to decide if they wish to defend the incoming attack!");
                int cardNumTracker = 1;
                foreach (PermaSpell creature in creatures) {
                    if (creature.State < 2) {
                        Console.WriteLine($"[{cardNumTracker}] {creature.CardName} ({creature.CardDescription}) | Attack = {creature.Attack}, Defense = {creature.HP}, Turns left = {creature.TurnsLeft}");
                        cardNumTracker++;
                    }
                }
                Console.WriteLine("[0] Do nothing");
                Console.Write("Please choose the number associated with desired action: ");
            }
            // prints counter options for player
            else if (happening == "counterOptions") {
                Console.WriteLine($"It's Player {p.ID}'s turn to counter their opponent's incoming move!");
                Console.WriteLine($"\nLands on the board owned by Player {p.ID} (amount = {lands.Count}):");
                int cardNumTracker = 1;
                foreach (Land land in lands) {
                    Console.WriteLine($"[{cardNumTracker}] {land.CardName} ({land.CardDescription}) | Color = {land.Color}");
                    cardNumTracker++;
                }
                Console.WriteLine($"\nCounter spells in Player {p.ID}'s hand (amount = {p.Hand.Count}):");
                foreach (Card card in p.Hand) {
                    if (card.EffectType == EffectType.Counter) {
                        Console.WriteLine($"[{cardNumTracker}] {card.Type} : {card.CardName} ({card.CardDescription}) | Costs {card.EnergyCost} energy");
                        cardNumTracker++;    
                    }
                }
                Console.WriteLine("\n[0] Do nothing");
                Console.WriteLine($"\nAvailable energy: {p.Energy}");
                Console.Write("Please choose the number associated with desired action: ");
            }
        }
        /// <summary>
        /// Method <c>PlayerTurn</c> lets specified player play their cards until they end their turn
        /// </summary>
        public void PlayerTurn(Player p, Player otherP) {
            // player turn will last until they cancel their turn
            bool playerTurnOngoing = true;
            bool playerForfeited = false;

            while (playerTurnOngoing) {
                // establish list of lands & creatures owned by current player
                List<Land> ownedLands = this.GetPlayerLands(p);
                List<Land> opponentLands = this.GetPlayerLands(otherP);
                List<PermaSpell> ownedCreatures = this.GetPlayerCreatures(p);
                List<PermaSpell> opponentCreatures = this.GetPlayerCreatures(otherP);

                // print relevant information (cards in hand, lands/creatures on board and possible actions)
                PrintToConsole("cardsInHand", p, ownedLands, ownedCreatures);
                PrintToConsole("activeLandsAndCreatures", p, ownedLands, ownedCreatures);
                PrintToConsole("presentOptions", p, ownedLands, ownedCreatures);

                LogActivities($"Player {p.ID} is choosing an action.");
                // reads input & checks if it is able to become an int
                int chosenAction = 0;
                bool choiceIsInt = int.TryParse(Console.ReadLine(), out chosenAction);
                
                // end player's turn if they chose to or if their hand is empty
                if (choiceIsInt && chosenAction == 0) {
                    // ends the player's turn
                    LogActivities($"Player {p.ID} has chosen to end their turn.");
                    playerTurnOngoing = false;
                    continue;
                }
                else if (choiceIsInt && chosenAction == -1) {
                    // player forfeited, other player wins
                    LogActivities($"Player {p.ID} has chosen to forfeit the game.");
                    playerTurnOngoing = false;
                    playerForfeited = true;
                    this.Winner = $"Player {p.ID} forfeit";
                    continue;
                }
                // play the player's chosen card from their hand
                else if (choiceIsInt && (chosenAction >= 1 && chosenAction <= p.Hand.Count)) {
                    // get card from hand
                    Card chosenCard = p.Hand[chosenAction - 1];

                    // check if chosen card is a land; add to this.LandsOnBoard
                    if (chosenCard.Type == Type.Land) {
                        LogActivities($"Player {p.ID} has chosen to play a land card.");
                        this.AddLands(chosenCard as Land);
                        p.DiscardHand(chosenCard);
                    }
                    // check if chosen card is instant spell
                    else if (chosenCard.Type == Type.InstantSpell) {
                        LogActivities($"Player {p.ID} has chosen to play an instant spell card.");
                        if (p.Energy >= chosenCard.EnergyCost) {
                            // player is able to play card; play card
                            this.AddToCardStack(chosenCard);
                            p.DiscardHand(chosenCard);
                            p.ChangeEnergy(chosenCard.EnergyCost * -1);

                            // let other player interject with their counterspells
                            this.InterruptPlayerTurn(otherP, p, "counterSpell");
                            // wait for teacher to confirm if turn ends or not here.
                        }
                        else {
                            // player is unable to play card
                            LogActivities($"Player {p.ID} cannot play their card; insufficient energy.");
                            Console.WriteLine("Unable to play chosen card; insufficient energy");
                        }
                    }
                    // check if chosen card is permanent spell; add to this.ActiveCreatures if player has enough energy
                    else if (chosenCard.Type == Type.PermanentSpell) {
                        LogActivities($"Player {p.ID} has chosen to play a creature card.");
                        if (p.Energy >= chosenCard.EnergyCost) {
                            // player is able to play card; play card

                            // check if creature has discard card effect; activate effect
                            if (chosenCard.EffectType == EffectType.ForceDiscardCard) {
                                otherP.DiscardHand(1);
                            }
                            this.AddPermas(chosenCard as PermaSpell, p);
                            p.DiscardHand(chosenCard);
                        }
                        else {
                            // player is unable to play card
                            LogActivities($"Player {p.ID} cannot play their card; insufficient energy.");
                            Console.WriteLine("Unable to play chosen card; insufficient energy");
                        }
                    }
                }
                // play one of the player's lands on the board
                else if (choiceIsInt && (chosenAction > p.Hand.Count && chosenAction <= p.Hand.Count + ownedLands.Count)) {
                    LogActivities($"Player {p.ID} has chosen to use one of their active lands to generate an energy point.");
                    Console.WriteLine($"Player chose to harvest land; Estimated index = {chosenAction - p.Hand.Count - 1}");
                    Land chosenLand = ownedLands[chosenAction - p.Hand.Count - 1];
                    chosenLand.ActivateEffect(chosenLand);
                }
                // play one of the player's active creatures on the board
                else if (choiceIsInt && (chosenAction > p.Hand.Count + ownedLands.Count && chosenAction <= p.Hand.Count + ownedLands.Count + ownedCreatures.Count)) {
                    LogActivities($"Player {p.ID} has chosen to play one of their active creatures; is deciding whether to attack, defend or cancel.");
                    // make user decide between attacking & defending here 
                    // adds play to the card queue
                    PrintToConsole("playCreature", p, ownedLands, ownedCreatures);
                    PermaSpell chosenCreature = ownedCreatures[chosenAction - p.Hand.Count - 1 - ownedLands.Count];
                    chosenAction = 0;
                    choiceIsInt = int.TryParse(Console.ReadLine(), out chosenAction);
                    bool canPerformAction = true;
                    Console.WriteLine($"player chose to play creature; estimated index = {chosenAction - p.Hand.Count - 1 - ownedLands.Count}");

                    // check if one of the player's own creatures is already doing something
                    if (choiceIsInt && chosenAction != 2) {
                        foreach (PermaSpell creature in ownedCreatures) {
                            if (creature.State == 2 || creature.State == 3) {
                                canPerformAction = false;
                            }
                        }
                    }
                    if (canPerformAction && chosenAction == 1) {
                        // apply attacking to creature & add it to cardstack
                        LogActivities($"Player {p.ID} has chosen to attack with their {chosenCreature.CardName}.");
                        chosenCreature.State = 2;
                        this.AddToCardStack(chosenCreature);

                        // let other player interject by choosing to defend with one of their own creatures (if they have one on the board)
                        if (opponentCreatures.Count > 0) {
                            this.InterruptPlayerTurn(otherP, p, "opponentAttacks", chosenCreature);
                        }
                        else {
                            LogActivities("Opponent is unable to defend against the incoming attack.");
                        }
                    }
                }
                Thread.Sleep(700);
            }
            Console.WriteLine($"Player {p.ID}'s turn has ended");
            if (playerForfeited) {
                Console.WriteLine($"Player {p.ID} decided to forfeit, the winner is Player {otherP.ID}");
            }
        }
        /// <summary>
        /// Method <c>InterruptPlayerTurn</c> lets specified player interrupt the specified other player's turn and play their own cards
        /// </summary>
        public void InterruptPlayerTurn(Player p, Player otherP, string happening, PermaSpell attackingCreature = null) {
            if (happening == "opponentAttacks") {
                // player turn will last until they cancel their turn
                bool playerTurnOngoing = true;
                List<PermaSpell> ownedCreatures = new List<PermaSpell>();
                if (this.ActiveCreatures.Count > 0) {
                    foreach (PermaSpell creature in this.ActiveCreatures) {
                        if (creature.PlayerId == p.ID && creature.State < 2) {
                            ownedCreatures.Add(creature);
                        }
                        else if (creature.PlayerId == p.ID && creature.State == 3) {
                            playerTurnOngoing = false;
                        }
                    }
                }
                if (!playerTurnOngoing) {
                    LogActivities($"Player {p.ID} is already defending with one of their creatures this turn.");
                }

                // interrupt by giving player option to defend against incoming attack
                while (playerTurnOngoing) {
                    // print options to console
                    PrintToConsole("defendCreature", p, null, ownedCreatures);

                    LogActivities($"Player {p.ID} is choosing their defense.");
                    // reads input & checks if it is able to become an int
                    int chosenAction = 0;
                    bool choiceIsInt = int.TryParse(Console.ReadLine(), out chosenAction);

                    if (choiceIsInt && chosenAction == 0) {
                        // player did nothing
                        LogActivities($"Player {p.ID} has decided to not defend.");
                        playerTurnOngoing = false;
                        continue;
                    }
                    else if (choiceIsInt && (chosenAction > 0 && chosenAction <= ownedCreatures.Count)) {
                        // player is defending w their creature
                        LogActivities($"Player {p.ID} has decided to defend with one of their creatures.");
                        PermaSpell chosenCreature = ownedCreatures[chosenAction];
                        chosenCreature.DoDefend();
                        attackingCreature.TargetCreature = chosenCreature;

                        // end turn
                        playerTurnOngoing = false;
                        continue;
                    }
                }
            }
            else if (happening == "counterSpell") {
                // player turn will last until they select a valid action
                bool playerTurnOngoing = true;

                // get list of only player's counter spells
                List<InstaSpell> ownedCounters = new List<InstaSpell>();
                if (p.Hand.Count > 0) {
                    foreach (Card card in p.Hand) {
                        if (card.EffectType == EffectType.Counter) {
                            ownedCounters.Add(card as InstaSpell);
                        }
                    }
                }
                // get list of owned lands
                List<Land> ownedLands = this.GetPlayerLands(p);

                // interrupt by giving player option to counter spell that was last added to CardStack
                while (playerTurnOngoing) {
                    // print counter options to console
                    PrintToConsole("counterOptions", p, ownedLands, null, ownedCounters);

                    LogActivities($"Player {p.ID} is choosing a counter action.");
                    // reads input & checks if it is able to become an int
                    int chosenAction = 0;
                    bool choiceIsInt = int.TryParse(Console.ReadLine(), out chosenAction);

                    if (choiceIsInt && chosenAction == 0) {
                        // player did nothing
                        LogActivities($"Player {p.ID} has decided to not perform a counter action.");
                        playerTurnOngoing = false;
                        continue;
                    }
                    else if (ownedLands.Count > 0 && choiceIsInt && (chosenAction > 0 && chosenAction <= ownedLands.Count)) {
                        // player decided to get energy from land
                        LogActivities($"Player {p.ID} has chosen to use one of their active lands to generate an energy point.");
                        Console.WriteLine($"Player chose to harvest land; Estimated index = {chosenAction - 1}");
                        Land chosenLand = ownedLands[chosenAction - 1];
                        chosenLand.ActivateEffect(chosenLand);
                    }
                    else if (ownedCounters.Count > 0 && choiceIsInt && (chosenAction > ownedLands.Count && chosenAction <= ownedCounters.Count + ownedLands.Count)) {
                        // player chose to use a counter spell
                        LogActivities($"Player {p.ID} has chosen to use one of their counter spells.");
                        Console.WriteLine($"Counterspell estimated index = {chosenAction - ownedLands.Count - 1}");
                        InstaSpell chosenCounter = ownedCounters[chosenAction - ownedLands.Count - 1];

                        // check energy
                        if (p.Energy >= chosenCounter.EnergyCost) {
                            // player is able to play card; play card
                            this.AddToCardStack(chosenCounter);
                            p.DiscardHand(chosenCounter);
                            p.ChangeEnergy(chosenCounter.EnergyCost * -1);

                            // let other player cancel the counter
                            this.InterruptPlayerTurn(otherP, p, "counterSpell");

                            // end interrupt turn
                            LogActivities($"Player {p.ID}'s interruption of Player {otherP.ID}'s turn is over; returning to Player {otherP.ID}'s turn.");
                            playerTurnOngoing = false;
                            continue;
                        }
                        else {
                            // player is unable to play card
                            LogActivities($"Player {p.ID} cannot play their card; insufficient energy.");
                            Console.WriteLine("Unable to play chosen card; insufficient energy");
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Method <c>WinConditionCheck</c> checks if a win condition has been satisfied
        /// </summary>
        public string WinConditionCheck(Player p1, Player p2) {
            // checks the win conditions at various points during a turn
            // win conditions are: 1. a player having an empty deck, 2. a player losing all of their HP
            // players tie if both happen at the same time to both players
            // players can also forfeit during their turn

            // check for forfeits
            if (this.Winner == "Player 1 forfeit") {
                return "Player 2";
            }
            else if (this.Winner == "Player 2 forfeit") {
                return "Player 1";
            }

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
        /// <summary>
        /// Method <c>AddLands</c> adds specified land to list of active lands
        /// </summary>
        public void AddLands(Land landToAdd) {
            // add lands to LandsOnBoard
            LogActivities("A land has been played and added to the list of active lands.");
            this.LandsOnBoard.Add(landToAdd);
        }
        /// <summary>
        /// Method <c>AddPermas</c> adds specified PermaSpell (creature) to list of active creatures
        /// </summary>
        public void AddPermas(PermaSpell creatureToAdd, Player p) {
            // add creature to ActiveSpells & subtract energy from player
            LogActivities("A creature has been played and added to the list of active creatures.");
            this.ActiveCreatures.Add(creatureToAdd);
            p.ChangeEnergy(creatureToAdd.EnergyCost * -1);
        }
        /// <summary>
        /// Method <c>AddToCardStack</c> adds specified card to stack of cards to be activated
        /// </summary>
        public void AddToCardStack(Card cardToAdd) {
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
        public List<PermaSpell> GetPlayerCreatures(Player p) {
            List<PermaSpell> returnList = new List<PermaSpell>();
            foreach (PermaSpell creature in this.ActiveCreatures) {
                if (creature.PlayerId == p.ID) {
                    returnList.Add(creature);
                }
            }
            return returnList;
        }
        /// <summary>
        /// Method <c>SetUpTurn2Scenaio</c> sets the game's values to the required turn 2
        /// </summary>
        public void SetUpTurn2Scenario() {

        }
        /// <summary>
        /// Method <c>RevertPermaEffects</c> reverts effects of active creatures and removes expired cards from board
        /// </summary>
        public void RevertPermaEffects() {
            for (int i = 0; i < this.ActiveCreatures.Count; i++) {
                // reverts effects if they were activated during last turn
                this.ActiveCreatures[i].State = 0;
                if (this.ActiveCreatures[i].Effect != null) {
                    foreach (PermaSpell perma in this.ActiveCreatures) {
                        if ((this.ActiveCreatures[i].PlayerId == Player1.ID && this.ActiveCreatures[i].Effect.Target == Target.Yours) && perma.PlayerId == Player1.ID) {
                            // reverts self-buffing effects for player 1's creatures
                            Console.WriteLine($"removing {this.ActiveCreatures[i].PlayerId}'s {this.ActiveCreatures[i].CardName} effect on {perma.PlayerId}'s {perma.CardName}");
                            this.ActiveCreatures[i].RevertEffects(perma);
                        }
                        else if ((this.ActiveCreatures[i].PlayerId == Player2.ID && this.ActiveCreatures[i].Effect.Target == Target.Yours) && perma.PlayerId == Player2.ID) {
                            // reverts self-buffing effects for player 2's creatures
                            Console.WriteLine($"removing {this.ActiveCreatures[i].PlayerId}'s {this.ActiveCreatures[i].CardName} effect on {perma.PlayerId}'s {perma.CardName}");
                            this.ActiveCreatures[i].RevertEffects(perma);
                        }
                        else if ((this.ActiveCreatures[i].PlayerId == Player1.ID && this.ActiveCreatures[i].Effect.Target == Target.Opponents) && perma.PlayerId == Player2.ID) {
                            // reverts debuffing effect caused by player 1 on player 2's creatures
                            Console.WriteLine($"removing {this.ActiveCreatures[i].PlayerId}'s {this.ActiveCreatures[i].CardName} effect on {perma.PlayerId}'s {perma.CardName}");
                            this.ActiveCreatures[i].RevertEffects(perma);
                        }
                        else if ((this.ActiveCreatures[i].PlayerId == Player2.ID && this.ActiveCreatures[i].Effect.Target == Target.Opponents) && perma.PlayerId == Player1.ID) {
                            // reverts debuffing effect caused by player 2 on player 1's creatures
                            Console.WriteLine($"removing {this.ActiveCreatures[i].PlayerId}'s {this.ActiveCreatures[i].CardName} effect on {perma.PlayerId}'s {perma.CardName}");
                            this.ActiveCreatures[i].RevertEffects(perma);
                        }
                    }
                }

                // checks if the card has any turns left; if not, it gets removed from play
                if (this.ActiveCreatures[i].TurnsLeft == 0 && this.ActiveCreatures[i].PlayerId == 1) {
                    // removes card from play; it's turns are over so it gets discarded
                    Console.WriteLine("Creature has ran out of active turns; is getting discarded");
                    this.ActiveCreatures.RemoveAt(i);
                }
                else if (this.ActiveCreatures[i].TurnsLeft == 0 && this.ActiveCreatures[i].PlayerId == 2) {
                    // removes card from play; it's turns are over so it gets discarded
                    Console.WriteLine("Creature has ran out of active turns; is getting discarded");
                    this.ActiveCreatures.RemoveAt(i);
                }
            }
        }
        /// <summary>
        /// Method <c>ActivatePermaEffects</c> activates effects of active creatures
        /// </summary>
        public void AcivatePermaEffects() {
            for (int i = 0; i < this.ActiveCreatures.Count; i++) {
                // reverts effects if they were activated during last turn
                if (this.ActiveCreatures[i].Effect != null) {
                    foreach (PermaSpell perma in this.ActiveCreatures) {
                        if ((this.ActiveCreatures[i].PlayerId == Player1.ID && this.ActiveCreatures[i].Effect.Target == Target.Yours) && perma.PlayerId == Player1.ID) {
                            // reverts self-buffing effects for player 1's creatures
                            Console.WriteLine($"re-adding {this.ActiveCreatures[i].PlayerId}'s {this.ActiveCreatures[i].CardName} effect on {perma.PlayerId}'s {perma.CardName}");
                            this.ActiveCreatures[i].Effect.ActivateEffect(perma);
                        }
                        else if ((this.ActiveCreatures[i].PlayerId == Player2.ID && this.ActiveCreatures[i].Effect.Target == Target.Yours) && perma.PlayerId == Player2.ID) {
                            // reverts self-buffing effects for player 2's creatures
                            Console.WriteLine($"re-adding {this.ActiveCreatures[i].PlayerId}'s {this.ActiveCreatures[i].CardName} effect on {perma.PlayerId}'s {perma.CardName}");
                            this.ActiveCreatures[i].Effect.ActivateEffect(perma);
                        }
                        else if ((this.ActiveCreatures[i].PlayerId == Player1.ID && this.ActiveCreatures[i].Effect.Target == Target.Opponents) && perma.PlayerId == Player2.ID) {
                            // reverts debuffing effect caused by player 1 on player 2's creatures
                            Console.WriteLine($"re-adding {this.ActiveCreatures[i].PlayerId}'s {this.ActiveCreatures[i].CardName} effect on {perma.PlayerId}'s {perma.CardName}");
                            this.ActiveCreatures[i].Effect.ActivateEffect(perma);
                        }
                        else if ((this.ActiveCreatures[i].PlayerId == Player2.ID && this.ActiveCreatures[i].Effect.Target == Target.Opponents) && perma.PlayerId == Player1.ID) {
                            // reverts debuffing effect caused by player 2 on player 1's creatures
                            Console.WriteLine($"re-adding {this.ActiveCreatures[i].PlayerId}'s {this.ActiveCreatures[i].CardName} effect on {perma.PlayerId}'s {perma.CardName}");
                            this.ActiveCreatures[i].Effect.ActivateEffect(perma);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Method <c>HandleCardQueue</c> handles queue of cards to be activated
        /// </summary>
        public void HandleCardQueue() {
            // handles card effects in CardQueue when they need to activate
            // if card is a counter; removes next item in stack
            if (this.CardActivationStack.Count > 0) {
                for (int i = 0; i < this.CardActivationStack.Count; i++) {

                }
            }
        }
    }
}