using System;
using System.Collections.Generic;

namespace SAD_Assignment
{
    /// <summary>
    /// Class <c>CardEffect</c> defines an effect
    /// </summary>
    public abstract class CardEffect<T> where T : Card {
        public Target Target { get; set; }
        /// <summary>
        /// Method <c>ActivateEffect</c> activates effect of card
        /// </summary>
        public abstract void ActivateEffect(T param);
        /// <summary>
        /// Method <c>RevertEffect</c> reverts effect of card
        /// </summary>
        public abstract void RevertEffect(T param);
    }
    /// <summary>
    /// Class <c>StatAugmentPerma</c> defines a stat-augment effect used by a creature
    /// </summary>
    public class StatAugmentPerma : CardEffect<PermaSpell> {
        public int HPAugment { get; set; }
        public int AttackAugment { get; set; }
        public StatAugmentPerma(int hpAugment, int attackAugment, Target target) {
            this.HPAugment = hpAugment;
            this.AttackAugment = attackAugment;
            this.Target = target;
        }
        /// <summary>
        /// Method <c>ActivateEffect</c> buffs specified target's attack and defense
        /// </summary>
        public override void ActivateEffect(PermaSpell param)
        {
            // buff target's stats with fields of class
            param.Attack = param.Attack + this.AttackAugment;
            param.HP = param.HP + this.HPAugment;
        }
        /// <summary>
        /// Method <c>RevertEffect</c> reverts buff of specified target's attack and defense
        /// </summary>
        public override void RevertEffect(PermaSpell param)
        {
            // revert stat changes
            param.Attack = param.Attack - this.AttackAugment;
            param.HP = param.HP - this.HPAugment;
        }
    }
    /// <summary>
    /// Class <c>StatAugmentInsta</c> defines a stat-augment effect used by an instant spell
    /// </summary>
    public class StatAugmentInsta : CardEffect<InstaSpell> {
        public int HPAugment { get; set; }
        public int AttackAugment { get; set; }
        public List<PermaSpell> PermaTargets { get; set; }
        public StatAugmentInsta(int hpAugment, int attackAugment, Target target) {
            this.HPAugment = hpAugment;
            this.AttackAugment = attackAugment;
            this.Target = target;
        }
        /// <summary>
        /// Method <c>ActivateEffect</c> buffs targets' attack and defense using instant spell
        /// </summary>
        public override void ActivateEffect(InstaSpell param)
        {
            // buff target's stats with fields of class
            foreach (PermaSpell perma in PermaTargets) {
                perma.Attack = perma.Attack - this.AttackAugment;
                perma.HP = perma.HP - this.HPAugment;
            }
        }
        /// <summary>
        /// Method <c>RevertEffect</c> reverts buff of targets' attack and defense using instant spell
        /// </summary>
        public override void RevertEffect(InstaSpell param)
        {
            // revert stat changes
            foreach (PermaSpell perma in PermaTargets) {
                perma.Attack = perma.Attack + this.AttackAugment;
                perma.HP = perma.HP + this.HPAugment;
            }
        }
        /// <summary>
        /// Method <c>SetTargets</c> sets instant spell's targets to specified list of creatures
        /// </summary>
        public void SetTargets(int targetId, List<PermaSpell> activePermas) {
            foreach (PermaSpell perma in activePermas) {
                if (perma.PlayerId == targetId) {
                    PermaTargets.Add(perma);
                }
            }
        }
    }
    /// <summary>
    /// Class <c>GenerateEnergy</c> defines an energy generating effect used by a land
    /// </summary>
    public class GenerateEnergy : CardEffect<Land> {
        /// <summary>
        /// Method <c>ActivateEffect</c> uses land to generate 1 energy for the player who owns it
        /// </summary>
        public override void ActivateEffect(Land param)
        {
            // check if land is already used or not; if not, continue
            if (param.State != 1 && param.PlayerId == 1) {
                PlayerContainer.Player1.ChangeEnergy(1);
            }
            else if (param.State != 1 && param.PlayerId == 2) {
                PlayerContainer.Player2.ChangeEnergy(1);
            }
        }
        /// <summary>
        /// Method <c>RevertEffect</c> reverts effect of generating energy
        /// </summary>
        public override void RevertEffect(Land param)
        {
            // same as activateEffect but takes energy away instead
            if (param.State != 1 && param.PlayerId == 1) {
                PlayerContainer.Player1.ChangeEnergy(-1);
            }
            else if (param.State != 1 && param.PlayerId == 2) {
                PlayerContainer.Player2.ChangeEnergy(-1);
            }
        }
    }
    /// <summary>
    /// Class <c>CounterSpell</c> defines a counter effect used by an instant spell
    /// </summary>
    public class CounterSpell : CardEffect<InstaSpell> {
        public InstaSpell targetSpell { get; set; }
        /// <summary>
        /// Method <c>ActivateEffect</c> does nothing yet :(
        /// </summary>
        public override void ActivateEffect(InstaSpell param)
        {
            // make targetSpell inactive; possible cascading effect when cancelling a cancelling spell
        }
        /// <summary>
        /// Method <c>RevertEffect</c> does nothing yet D;
        /// </summary>
        public override void RevertEffect(InstaSpell param)
        {
            throw new NotImplementedException();
        }
    }
}