using System;
using System.Collections.Generic;

/*
Thijmen Bouwsema 1008331
*/

namespace SAD_Assignment
{
    public enum Color { Red, Blue, Yellow, Green, Black, Colorless } // not every color is required for deliverable's case
    public enum CardType { Land, PermaSpell, InstantSpell, Irrelevant } // Irrelevant type is for deliverable purposes only
    public enum EffectType { None, GenerateEnergy, StatAugment, Counter, Buff, Debuff, ForceDiscard }
    public enum CardState { Inactive, AlreadyUsed, Attacking, Defending }
    /// <summary>
    /// Class <c>Card</c> defines a card
    /// </summary>
    public abstract class Card {
        public int PlayerId { get; set; }
        public string CardName { get; set; }
        public string CardDescription { get; set; }
        public int EnergyCost { get; set; }
        public Color Color { get; set; }
        public CardType CardType { get; set; }
        public EffectType EffectType { get; set; }
        public CardState State { get; set; }
        /// <summary>
        /// Method <c>RevertEffects</c> reverts effects of specified card (never implemented; in deliverable's case this never happens anywhere)
        /// </summary>
        public abstract void RevertEffects();
        /// <summary>
        /// Method <c>PrintAll</c> is used in the composite classes to get information from child elements
        /// </summary>
        public abstract void PrintAll();
        /// <summary>
        /// Method <c>Count</c> is used in the composite classes to get the amount of child elements
        /// </summary>
        public abstract int Count(int currentAmount = 0);
    }
    /// <summary>
    /// Interface <c>IUpdateRecipients</c> is used to have a list of all class objects that will recieve updates
    /// </summary>
    public interface IUpdateRecipients {  }
    /// <summary>
    /// Class <c>Land</c> defines a land card
    /// </summary>
    public class Land : Card {
        public Land(int playerId, string cardName, string cardDescription, Color color) {
            base.EnergyCost = 0;
            base.PlayerId = playerId;
            base.CardName = cardName;
            base.CardDescription = cardDescription;
            base.Color = color;
            base.CardType = CardType.Land;
            base.State = CardState.Inactive;
            base.EffectType = EffectType.GenerateEnergy;
        }
        /// <summary>
        /// Method <c>ResetLand</c> resets land back to active state so it can be used again
        /// </summary>
        public void ResetLand() {
            this.State = CardState.Inactive;
        }
        /// <summary>
        /// Method <c>GenerateEnergy</c> generates 1 energy point for owner player
        /// </summary>
        public void GenerateEnergy() {
            Game game = Game.GetInstance();
            if (this.State == CardState.Inactive && this.PlayerId == 1) {
                // generate energy if owner is player 1
                game.Player1.ChangeEnergy(1);
                this.State = CardState.AlreadyUsed;
            }
            else if (this.State == CardState.Inactive && this.PlayerId == 2) {
                // generate energy if owner is player 2
                game.Player2.ChangeEnergy(1);
                this.State = CardState.AlreadyUsed;
            }
            else {
                Game.LogActivities("Land chosen by player was already used this turn; did not generate any energy.");
            }
        }
        public override void PrintAll() {
            Console.WriteLine($"[Owner = Player {this.PlayerId}] Name = {this.CardName}");
        }
        public override int Count(int currentAmount = 0) {
            return currentAmount + 1;
        }
        public override void RevertEffects() { /* not implemented; deliverable case does not require it's implementation (case doesn't have an instance of reverting effects) */ }
    }
    /// <summary>
    /// Class <c>PermaSpell</c> defines a PermaSpell card
    /// </summary>
    public class PermaSpell : Card, IUpdateRecipients {
        private int _hp;
        public int HP { 
            get { return _hp; }
            set {
                if (_hp != value) {
                    _hp = value;
                    this.Notify(this._hp, this.State);
                }
            }
        }
        public int Attack { get; set; }
        public PermaSpell TargetPerma { get; set; }
        public List<IUpdateRecipients> ObjectsToUpdate = new List<IUpdateRecipients>();
        public int TurnsLeft { get; set; }
        public PermaSpell(int cost, int playerId, string cardName, string cardDescription, Color color, int turnsActive, EffectType effectType, int hp, int attack) {
            this.EnergyCost = cost;
            this.PlayerId = playerId;
            this.CardName = cardName;
            this.CardDescription = cardDescription;
            this.Color = color;
            this.CardType = CardType.PermaSpell;
            this.State = CardState.Inactive;
            this.TurnsLeft = turnsActive;
            this.EffectType = effectType;
            this.HP = hp;
            this.Attack = attack;
            this.TargetPerma = null;
        }
        /// <summary>
        /// Method <c>DoAttack</c> attacks using Perma; if no Perma is defending, it attacks opposing player
        /// </summary>
        public void DoAttack() {
            // attack the given Perma; uses this.attack to reduce it's hp
            Game game = Game.GetInstance();
            
            // update & check targetting; will attack player if target field is null
            if (this.TargetPerma != null) {
                // attack stored target
                Game.LogActivities($"Player {this.PlayerId}'s {this.CardName} has attacked Player {this.TargetPerma.PlayerId}'s {this.TargetPerma.CardName}: HP was reduced from {this.TargetPerma.HP} to {this.TargetPerma.HP - this.Attack}");
                this.TargetPerma.HP = this.TargetPerma.HP - this.Attack;
                this.TargetPerma = null;
            }
            else {
                // attack other player
                if (this.PlayerId == 1) {
                    // attack player 2
                    Game.LogActivities($"Player {this.PlayerId}'s {this.CardName} has attacked Player 2: HP was reduced from {game.Player2.HP} to {game.Player2.HP - this.Attack}");
                    game.Player2.ChangeHP(this.Attack);
                }
                else {
                    // attack player 1
                    Game.LogActivities($"Player {this.PlayerId}'s {this.CardName} has attacked Player 1: HP was reduced from {game.Player1.HP} to {game.Player1.HP - this.Attack}");
                    game.Player1.ChangeHP(this.Attack);
                }
            }
        }
        /// <summary>
        /// Method <c>SetTarget</c> sets target to specified perma
        /// </summary>
        public void SetTarget(PermaSpell perma) {
            if (perma.HP > 0) {
                this.TargetPerma = perma;
            }
        }
        /// <summary>
        /// Method <c>DoDefend</c> sets perma to defending state
        /// </summary>
        public void DoDefend() {
            // perma is defending; any attack from opponent this turn is redirected at it
            if (this.State == CardState.Inactive) {
                this.State = CardState.Defending;
                this.Notify(this._hp, this.State);
            }
        }
        /// <summary>
        /// Method <c>DecrementTurns</c> decreases turnsleft by 1
        /// </summary>
        public void DecrementTurns() {
            if (this.TurnsLeft > 0) {
                this.TurnsLeft--;
                if (this.TurnsLeft == 0) {
                    this.Notify(this._hp, this.State);
                }
            }
        }
        /// <summary>
        /// Method <c>RecieveStatAugment</c> adds given integers to it's HP and Attack stats
        /// </summary>
        public void RecieveStatAugment(int hpAugment, int attackAugment) {
            this.HP = this.HP + hpAugment;
            this.Attack = this.Attack + attackAugment;
        }
        /// <summary>
        /// Method <c>Notify</c> causes every object in this.ObjectsToUpdate to update
        /// </summary>
        public void Notify(int hp, CardState state) {
            if (this.ObjectsToUpdate.Count > 0) {
                foreach (var element in this.ObjectsToUpdate) {
                    if (element is PermaSpell) {
                        PermaSpell perma = (PermaSpell)element;
                        perma.Update(hp, state, element);
                    }
                    else if (element is InstaSpell) {
                        InstaSpell spell = (InstaSpell)element;
                        spell.Update(hp, state);
                    }
                }
            }
            
        }
        /// <summary>
        /// Method <c>Update</c> will check if this.targetPerma is dead; if yes the target will be a player instead
        /// </summary>
        public void Update(int hp, CardState state, IUpdateRecipients elementToRemove) {
            if (state != CardState.Defending || hp <= 0) {
                this.TargetPerma = null;
            }
            if (elementToRemove is PermaSpell) {
                PermaSpell removeMe = (PermaSpell)elementToRemove;
                if (removeMe.TurnsLeft <= 0) {
                    this.ObjectsToUpdate.Remove(elementToRemove);
                }
            }
            if (state == CardState.AlreadyUsed) {
                this.ObjectsToUpdate.Remove(elementToRemove);
            }
        }
        public override void PrintAll() {
            Console.WriteLine($"[Owner = Player {this.PlayerId}] Name = {this.CardName}");
        }
        public override int Count(int currentAmount = 0) {
            return currentAmount + 1;
        }
        public override void RevertEffects() { /* not implemented; deliverable case does not require it's implementation (case doesn't have an instance of reverting effects) */ }
    }
    /// <summary>
    /// Class <c>InstaSpell</c> defines an instant spell card
    /// </summary>
    public class InstaSpell : Card, IUpdateRecipients {
        public int HPAugment { get; set; }
        public int AttackAugment { get; set; }
        public PermaSpell TargetPerma { get; set; }
        public InstaSpell(int cost, int playerId, string cardName, string cardDescription, Color color, EffectType effectType, int hpAugment = 0, int attackAugment = 0) {
            this.EnergyCost = cost;
            this.PlayerId = playerId;
            this.CardName = cardName;
            this.CardDescription = cardDescription;
            this.Color = color;
            this.EffectType = effectType;
            this.CardType = CardType.InstantSpell;
            this.State = CardState.Inactive;
            this.HPAugment = hpAugment;
            this.AttackAugment = attackAugment;
        }
        public override void PrintAll() {
            Console.WriteLine($"[Owner = Player {this.PlayerId}] Name = {this.CardName}");
        }
        public override int Count(int currentAmount = 0) {
            return currentAmount + 1;
        }
        public override void RevertEffects() { /* not implemented; deliverable case does not require it's implementation (case doesn't have an instance of reverting effects) */ }
        public void ActivateEffect() {
            if (this.EffectType == EffectType.Buff || this.EffectType == EffectType.Debuff) {
                this.TargetPerma.RecieveStatAugment(this.HPAugment, this.AttackAugment);
                this.State = CardState.AlreadyUsed;
            }
            this.Update(this.TargetPerma.HP, this.State);
        }
        public void SetTarget(PermaSpell target) {
            if (this.EffectType == EffectType.Buff && target.PlayerId == this.PlayerId) {
                this.TargetPerma = target;
                this.TargetPerma.ObjectsToUpdate.Add(this);
            }
            else if (this.EffectType == EffectType.Debuff && target.PlayerId != this.PlayerId) {
                this.TargetPerma = target;
                this.TargetPerma.ObjectsToUpdate.Add(this);
            }
        }
        public void Update(int permaHp, CardState permaState) {
            if (permaHp <= 0) {
                this.TargetPerma = null;
                this.TargetPerma.ObjectsToUpdate.Remove(this);
            }
        }
    }
    public abstract class CardComponent : Card {
        public string Name;
        public CardComponent(string name) {
            this.Name = name;
        }
        public abstract void Add(Card card);
        public abstract void Remove(Card card);
    }
    /// <summary>
    /// Class <c>CardComposite</c> organises card types into composite groups
    /// </summary>
    public class CardComposite : CardComponent {
        public override void RevertEffects() { /* not implemented; deliverable case does not require it's implementation (case doesn't have an instance of reverting effects) */ }
        public List<Card> Children = new List<Card>();
        // constructor
        public CardComposite(string name) : base(name) {}
        public override void Add(Card card) {
            // adds a new branch (like land- or perma-branch)
            Children.Add(card);
        }
        public override void Remove(Card card) {
            // removes branch
            Children.Remove(card);
        }
        public override int Count(int currentAmount = 0) {
            foreach (Card composite in Children) {
                currentAmount = composite.Count(currentAmount);
            }
            return currentAmount;
        }
        public override void PrintAll() {
            Console.WriteLine($"Composite name = {this.Name}");

            foreach (Card composite in this.Children) {
                composite.PrintAll();
            }
        }
    }
    public class CardLeaf : CardComponent {
        public override void RevertEffects() { /* not implemented; deliverable case does not require it's implementation (case doesn't have an instance of reverting effects) */ }
        public Card StoredCard { get; set; }
        public CardLeaf(Card card) : base(card.CardName) {
            this.StoredCard = card;
        }
        public override void Add(Card card) {
            Console.WriteLine("cannot add to leaf.");
        }
        public override void Remove(Card card)
        {
            Console.WriteLine("cannot remove from leaf.");
        }
        public override void PrintAll() {
            this.StoredCard.PrintAll();
        }
        public override int Count(int currentAmount) {
            return this.StoredCard.Count(currentAmount);
        }
    }

    /// <summary>
    /// Class <c>CardCreator</c> creates cards with factory design pattern
    /// </summary>
    abstract class CardCreator {
        /// <summary>
        /// Method <c>CreateCard</c> is the factory method used for creating card instances
        /// </summary>
        public abstract Card CreateCard(int playerId, string cardName, string cardDescription, Color color, int cost = 0, EffectType effectType = EffectType.None, int hp = 0, int attack = 0, int turnsActive = 0);
    }
    /// <summary>
    /// Class <c>LandCreator</c> creates lands using CreateCard
    /// </summary>
    class LandCreator : CardCreator {
        public override Card CreateCard(int playerId, string cardName, string cardDescription, Color color, int cost = 0, EffectType effectType = EffectType.None, int hp = 0, int attack = 0, int turnsActive = 0)
        {
            return new Land(playerId, cardName, cardDescription, color);
        }
    }
    /// <summary>
    /// Class <c>PermaCreator</c> creates perma using CreateCard
    /// </summary>
    class PermaCreator : CardCreator {
        public override Card CreateCard(int playerId, string cardName, string cardDescription, Color color, int cost, EffectType effectType, int hp, int attack, int turnsActive)
        {
            return new PermaSpell(cost, playerId, cardName, cardDescription, color, turnsActive, effectType, hp, attack);
        }
    }
    /// <summary>
    /// Class <c>InstaCreator</c> creates Instant Spells using CreateCard
    /// </summary>
    class InstaCreator : CardCreator {
        public override Card CreateCard(int playerId, string cardName, string cardDescription, Color color, int cost, EffectType effectType, int hp, int attack, int turnsActive = 0)
        {
            return new InstaSpell(cost, playerId, cardName, cardDescription, color, effectType, hp, attack);
        }
    }
}