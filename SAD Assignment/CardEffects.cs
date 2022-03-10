using System;
using System.Collections.Generic;

namespace SAD_Assignment
{
    public abstract class CardEffect<T> where T : Card {
        // Target: 0 = Your side of the field, 1 = Opponent's side of the field
        public int Target { get; set; }
        public abstract void ActivateEffect(T param);
    }
    public class StatAugmentPerma : CardEffect<PermaSpell> {
        public int HPAugment { get; set; }
        public int AttackAugment { get; set; }
        public StatAugmentPerma(int hpAugment, int attackAugment, Target target) {
            this.HPAugment = hpAugment;
            this.AttackAugment = attackAugment;
            this.Target = (int)target;
        }
        public override void ActivateEffect(PermaSpell param)
        {
            // buff target's stats with fields of class
            param.Attack = param.Attack + this.AttackAugment;
            param.HP = param.HP + this.HPAugment;
        }
    }
    public class StatAugmentInsta : CardEffect<InstaSpell> {
        public int HPAugment { get; set; }
        public int AttackAugment { get; set; }
        public List<PermaSpell> PermaTargets { get; set; }
        public StatAugmentInsta(int hpAugment, int attackAugment, Target target) {
            this.HPAugment = hpAugment;
            this.AttackAugment = attackAugment;
            this.Target = (int)target;
        }
        public override void ActivateEffect(InstaSpell param)
        {
            // buff target's stats with fields of class
            foreach (PermaSpell perma in PermaTargets) {
                perma.Attack = perma.Attack - this.AttackAugment;
                perma.HP = perma.HP - this.HPAugment;
            }
        }
        public void SetTargets(int targetId, List<PermaSpell> activePermas) {
            foreach (PermaSpell perma in activePermas) {
                if (perma.PlayerId == targetId) {
                    PermaTargets.Add(perma);
                }
            }
        }
    }
    public class GenerateEnergy : CardEffect<Land> {
        public override void ActivateEffect(Land param)
        {
            // check if land is already used or not; if not, continue
            if (param.State != 1 && param.PlayerId == 1) {
                PlayerContainer.Player1.ChangeEnergy(1, param.Color);
            }
            else if (param.State != 1 && param.PlayerId == 2) {
                PlayerContainer.Player2.ChangeEnergy(1, param.Color);
            }
        }
    }
    public class CounterSpell : CardEffect<InstaSpell> {
        public InstaSpell targetSpell { get; set; }
        public override void ActivateEffect(InstaSpell param)
        {
            // make targetSpell inactive; possible cascading effect when cancelling a cancelling spell
        }
    }
}