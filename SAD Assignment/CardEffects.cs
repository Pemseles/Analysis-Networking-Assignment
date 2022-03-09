using System;
using System.Collections.Generic;

namespace SAD_Assignment
{
    public abstract class CardEffect {
        public abstract void ActivateEffect();
    }
    public class StatAugmentEffect : CardEffect {
        public int HPAugment { get; set; }
        public int AttackAugment { get; set; }
        public StatAugmentEffect(int hpAugment, int attackAugment) {
            this.HPAugment = hpAugment;
            this.AttackAugment = attackAugment;
        }
        public override void ActivateEffect()
        {
            // reduce target's stats with fields of class
        }
    }
}