using System;
using System.Collections.Generic;

namespace SAD_Assignment
{
    public enum Color { Red, Blue, Yellow, Green, Black, Colorless }
    public enum Type { Land, PermanentSpell, InstantSpell, Irrelevant } // Irrelevant type is for deliverable purposes only
    public enum EffectType { None, GenerateEnergy, StatAugment, Counter, Buff, Debuff, ForceDiscardCard }
    public enum Target { Yours, Opponents }

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

        // State: 0 = Card is any; has not been played this turn, can be activated (initial state)
        //        1 = Card is Perma; has not been played this turn (did not attack but effect was activated; gets reset to 0 at beginning of turn)
        //        2 = Card is Perma; is considered 'attacking' (has no effect or effect is already activated; gets reset to 0 at beginning of turn)
        //        3 = Card is Perma; is considered 'defending'; any attacks coming from opponent are redirected to it (can only be one Perma per turn; gets reset to 0 at beginning of turn)
        //        4 = Card is Land; has generated energy this turn and becomes inactive
        //        5 = Card is Insta; has been used this turn; (because of it's TurnsLeft always being 1 the card is discarded at end of turn anyway)
        public int State { get; set; }
        public int TurnsLeft { get; set; }
        /// <summary>
        /// Method <c>RevertEffects</c> reverts effects of specified card
        /// </summary>
        public abstract void RevertEffects(Card givenCard);
    }
    /// <summary>
    /// Class <c>Land</c> defines a land card
    /// </summary>
    public class Land : Card {
        public CardEffect<Land> Effect { get; set; }
        public Land(int playerId, string cardName, string cardDescription, Color color, CardEffect<Land> effect, EffectType effectType) {
            this.EnergyCost = 0;
            this.PlayerId = playerId;
            this.CardName = cardName;
            this.CardDescription = cardDescription;
            this.Color = color;
            this.Type = Type.Land;
            this.State = 0;
            this.TurnsLeft = -1;
            this.Effect = effect;
            this.EffectType = effectType;
        }
        /// <summary>
        /// Method <c>ActivateEffect</c> activates effect of specified land
        /// </summary>
        public void ActivateEffect(Land landParam) {
            if (this.State == 0) {
                this.Effect.ActivateEffect(landParam);
                this.State = 4;
            }
            else {
                Console.WriteLine("Chosen land was already used this turn");
                Game.LogActivities("Land chosen by player was already used this turn; did not generate any energy.");
            }
        }
        /// <summary>
        /// Method <c>ResetLand</c> resets land back to active state so it can be used again
        /// </summary>
        public void ResetLand() {
            this.State = 0;
        }
        public override void RevertEffects(Card givenCard)
        {
            if (this.State != 0) {
                this.Effect.RevertEffect(givenCard as Land);
                this.State = 0;                
            }
        }
    }
    /// <summary>
    /// Class <c>Land</c> defines a creature card
    /// </summary>
    public class PermaSpell : Card {
        public int HP { get; set; }
        public int Attack { get; set; }
        public PermaSpell TargetCreature { get; set; }
        public CardEffect<PermaSpell> Effect { get; set; }
        public PermaSpell(int cost, int playerId, string cardName, string cardDescription, Color color, int turnsActive, CardEffect<PermaSpell> effect, EffectType effectType, int hp, int attack) {
            this.EnergyCost = cost;
            this.PlayerId = playerId;
            this.CardName = cardName;
            this.CardDescription = cardDescription;
            this.Color = color;
            this.Type = Type.PermanentSpell;
            this.State = 0;
            this.TurnsLeft = turnsActive;
            this.Effect = effect;
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
            
            // check targetting; will attack player if target field is null

        }
        /// <summary>
        /// Method <c>DoDefend</c> sets creature to defending state
        /// </summary>
        public void DoDefend() {
            // creature is defending; any attack from opponent this turn is redirected at it
            if (this.State < 2) {
                this.State = 3;
            }
        }
        public void RecieveStatAugment(int hpAmount, int attackAmount) {
            this.HP = this.HP + hpAmount;
            this.Attack = this.Attack + attackAmount;
        }
        /// <summary>
        /// Method <c>DecrementTurns</c> decreases turnsleft by 1
        /// </summary>
        public void DecrementTurns() {
            if (this.TurnsLeft > 0) {
                this.TurnsLeft--;
            }
        }
        public override void RevertEffects(Card givenCard)
        {
            if (this.State > 1 && this.Effect != null) {
                this.Effect.RevertEffect(givenCard as PermaSpell);
                this.State = 0;
            }
        }
    }
    /// <summary>
    /// Class <c>Land</c> defines an instant spell card
    /// </summary>
    public class InstaSpell : Card {
        public CardEffect<InstaSpell> Effect { get; set; }
        public InstaSpell(int cost, int playerId, string cardName, string cardDescription, Color color, CardEffect<InstaSpell> effect, EffectType effectType) {
            this.EnergyCost = cost;
            this.PlayerId = playerId;
            this.CardName = cardName;
            this.CardDescription = cardDescription;
            this.Color = color;
            this.Effect = effect;
            this.EffectType = effectType;
            this.Type = Type.InstantSpell;
            this.State = 0;
            this.TurnsLeft = 1;
        }
        public override void RevertEffects(Card givenCard)
        {
            this.Effect.RevertEffect(givenCard as InstaSpell);
            this.State = 0;
        }
    }
}