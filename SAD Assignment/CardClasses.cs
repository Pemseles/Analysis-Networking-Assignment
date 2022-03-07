using System;
using System.Collections.Generic;

namespace SAD_Assignment
{
    public enum Color { Red, Blue, Yellow, Green, Black, Colorless }
    public enum Type { Land, PermanentSpell, InstantSpell }

    public abstract class Card {
        public int PlayerId { get; set; }
        public int EnergyCost { get; set; }
        public Color Color { get; set; }
        public Type Type { get; set; }
        public int State { get; set; }
        public int TurnsActive { get; set; }
        public abstract void Activate();
    }
    public class Land : Card {
        public Land(int playerId, Color color) {
            this.EnergyCost = 0;
            this.PlayerId = playerId;
            this.Color = color;
            this.Type = Type.Land;
            this.State = "Available";
            this.TurnsActive = -1;
        }
        public override void Activate(CardEffect effect)
        {
            throw new NotImplementedException();
        }
    }
    public class PermaSpell : Card {
        public int HP { get; set; }
        public int Attack { get; set; }
        public PermaSpell(int cost, int playerId, Color color, int turnsActive) {
            this.EnergyCost = cost;
            this.PlayerId = playerId;
            this.Color = color;
            this.Type = Type.PermanentSpell;
            this.State = "Available";
            this.TurnsActive = turnsActive;
        }
        public override void Activate(CardEffect effect)
        {
            throw new NotImplementedException();
        }
    }
    public class InstaSpell : Card {
        public InstaSpell(int cost, int playerId, Color color, int turnsActive) {
            this.EnergyCost = cost;
            this.PlayerId = playerId;
            this.Color = color;
            this.Type = Type.InstantSpell;
            this.State = "Available";
            this.TurnsActive = 1;
        }
        public override void Activate(CardEffect effect)
        {
            throw new NotImplementedException();
        }
    }
}