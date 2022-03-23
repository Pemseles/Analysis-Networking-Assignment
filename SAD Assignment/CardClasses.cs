using System;

/*
Thijmen Bouwsema 1008331
*/

namespace SAD_Assignment
{
    public enum Color { Red, Blue, Yellow, Green, Black, Colorless }
    public enum Type { Land, PermanentSpell, InstantSpell, Irrelevant } // Irrelevant type is for deliverable purposes only
    public enum EffectType { None, GenerateEnergy, StatAugment, Counter, Buff, Debuff, ForceDiscardCard }
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
        public Type Type { get; set; }
        public EffectType EffectType { get; set; }
        public CardState State { get; set; }
        public int TurnsLeft { get; set; }
        /// <summary>
        /// Method <c>RevertEffects</c> reverts effects of specified card
        /// </summary>
        public abstract void RevertEffects();
    }
    /// <summary>
    /// Class <c>Land</c> defines a land card
    /// </summary>
    public class Land : Card {
        public Land(int playerId, string cardName, string cardDescription, Color color, EffectType effectType) {
            this.EnergyCost = 0;
            this.PlayerId = playerId;
            this.CardName = cardName;
            this.CardDescription = cardDescription;
            this.Color = color;
            this.Type = Type.Land;
            this.State = CardState.Inactive;
            this.TurnsLeft = -1;
            this.EffectType = effectType;
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
            PlayerContainer players = PlayerContainer.GetInstance();
            if (this.State == CardState.Inactive && this.PlayerId == 1) {
                // generate energy if owner is player 1
                players.Player1.ChangeEnergy(1);
                this.State = CardState.AlreadyUsed;
            }
            else if (this.State == CardState.Inactive && this.PlayerId == 2) {
                // generate energy if owner is player 2
                players.Player2.ChangeEnergy(1);
                this.State = CardState.AlreadyUsed;
            }
            else {
                Game.LogActivities("Land chosen by player was already used this turn; did not generate any energy.");
            }
        }
        public override void RevertEffects()
        {
            // not implemented; deliverable case does not require it's implementation (case doesn't have an instance of reverting effects)
        }
    }
    /// <summary>
    /// Class <c>Land</c> defines a creature card
    /// </summary>
    public class PermaSpell : Card {
        public int HP { get; set; }
        public int Attack { get; set; }
        public PermaSpell TargetCreature { get; set; }
        public PermaSpell(int cost, int playerId, string cardName, string cardDescription, Color color, int turnsActive, EffectType effectType, int hp, int attack) {
            this.EnergyCost = cost;
            this.PlayerId = playerId;
            this.CardName = cardName;
            this.CardDescription = cardDescription;
            this.Color = color;
            this.Type = Type.PermanentSpell;
            this.State = CardState.Inactive;
            this.TurnsLeft = turnsActive;
            this.EffectType = effectType;
            this.HP = hp;
            this.Attack = attack;
            this.TargetCreature = null;
        }
        /// <summary>
        /// Method <c>DoAttack</c> attacks using creature; if no creature is defending, it attacks opposing player
        /// </summary>
        public void DoAttack() {
            // attack the given Creature; uses this.attack to reduce it's hp
            PlayerContainer players = PlayerContainer.GetInstance();
            
            // update & check targetting; will attack player if target field is null
            this.UpdateTarget();
            if (this.TargetCreature != null) {
                // attack stored target
                Game.LogActivities($"Player {this.PlayerId}'s {this.CardName} has attacked Player {this.TargetCreature.PlayerId}'s {this.TargetCreature.CardName}: HP was reduced from {this.TargetCreature.HP} to {this.TargetCreature.HP - this.Attack}");
                this.TargetCreature.HP = this.TargetCreature.HP - this.Attack;
                this.TargetCreature = null;
            }
            else {
                // attack other player
                if (this.PlayerId == 1) {
                    // attack player 2
                    Game.LogActivities($"Player {this.PlayerId}'s {this.CardName} has attacked Player 2: HP was reduced from {players.Player2.HP} to {players.Player2.HP - this.Attack}");
                    players.Player2.ChangeHP(this.Attack);
                }
                else {
                    // attack player 1
                    Game.LogActivities($"Player {this.PlayerId}'s {this.CardName} has attacked Player 1: HP was reduced from {players.Player1.HP} to {players.Player1.HP - this.Attack}");
                    players.Player1.ChangeHP(this.Attack);
                }
            }
        }
        /// <summary>
        /// Method <c>UpdateTarget</c> checks if target is still actually in play; if not sets target to null
        /// </summary>
        public void UpdateTarget() {
            if (this.TargetCreature != null && this.TargetCreature.HP <= 0) {
                this.TargetCreature = null;
            }
        }
        /// <summary>
        /// Method <c>SetTarget</c> sets target to specified creature
        /// </summary>
        public void SetTarget(PermaSpell creature) {
            if (creature.HP > 0) {
                this.TargetCreature = creature;
            }
        }
        /// <summary>
        /// Method <c>DoDefend</c> sets creature to defending state
        /// </summary>
        public void DoDefend() {
            // creature is defending; any attack from opponent this turn is redirected at it
            if (this.State == CardState.Inactive) {
                this.State = CardState.Defending;
            }
        }
        /// <summary>
        /// Method <c>DecrementTurns</c> decreases turnsleft by 1
        /// </summary>
        public void DecrementTurns() {
            if (this.TurnsLeft > 0) {
                this.TurnsLeft--;
            }
        }
        /// <summary>
        /// Method <c>RecieveStatAugment</c> adds given integers to it's HP and Attack stats
        /// </summary>
        public void RecieveStatAugment(int hpAugment, int attackAugment) {
            this.HP = this.HP + hpAugment;
            this.Attack = this.Attack + attackAugment;
        }
        public override void RevertEffects()
        {
            // not implemented; deliverable case does not require it's implementation (case doesn't have an instance of reverting effects)
        }
    }
    /// <summary>
    /// Class <c>Land</c> defines an instant spell card
    /// </summary>
    public class InstaSpell : Card {
        public int HPAugment { get; set; }
        public int AttackAugment { get; set; }
        public PermaSpell TargetCreature { get; set; }
        public InstaSpell(int cost, int playerId, string cardName, string cardDescription, Color color, EffectType effectType, int hpAugment = 0, int attackAugment = 0) {
            this.EnergyCost = cost;
            this.PlayerId = playerId;
            this.CardName = cardName;
            this.CardDescription = cardDescription;
            this.Color = color;
            this.EffectType = effectType;
            this.Type = Type.InstantSpell;
            this.State = CardState.Inactive;
            this.TurnsLeft = 1;
            this.HPAugment = hpAugment;
            this.AttackAugment = attackAugment;
        }
        public override void RevertEffects()
        {
            // not implemented; deliverable case does not require it's implementation (case doesn't have an instance of reverting effects)
        }
        public void ActivateEffect() {
            if (this.EffectType == EffectType.Buff || this.EffectType == EffectType.Debuff) {
                this.TargetCreature.RecieveStatAugment(this.HPAugment, this.AttackAugment);
            }
        }
        public void SetTarget(PermaSpell target) {
            if (target.PlayerId == this.PlayerId) {
                this.TargetCreature = target;
            }
        }
        public void UpdateTarget() {
            if (this.TargetCreature.HP <= 0) {
                this.TargetCreature = null;
            }
        }
    }
}