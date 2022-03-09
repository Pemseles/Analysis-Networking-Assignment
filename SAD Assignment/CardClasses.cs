using System;
using System.Collections.Generic;

namespace SAD_Assignment
{
    public enum Color { Red, Blue, Yellow, Green, Black, Colorless }
    public enum Type { Land, PermanentSpell, InstantSpell }

    public abstract class Card {
        public int PlayerId { get; set; }
        public string CardName { get; set; }
        public string CardDescription { get; set; }
        public int EnergyCost { get; set; }
        public Color Color { get; set; }
        public Type Type { get; set; }
        // State: 0 = able to be played, 1 = not able to be played (for now)
        public CardEffect Effect { get; set; }
        public int State { get; set; }
        public int TurnsActive { get; set; }
    }
    public class Land : Card {
        public Land(int playerId, string cardname, string carddescription, Color color) {
            this.EnergyCost = 0;
            this.PlayerId = playerId;
            this.CardName = cardname;
            this.CardDescription = carddescription;
            this.Color = color;
            this.Type = Type.Land;
            this.State = 0;
            this.TurnsActive = -1;
        }
    }
    public class PermaSpell : Card {
        public int HP { get; set; }
        public int Attack { get; set; }
        public PermaSpell(int cost, int playerId, Color color, int turnsActive, int hp, int attack) {
            this.EnergyCost = cost;
            this.PlayerId = playerId;
            this.Color = color;
            this.Type = Type.PermanentSpell;
            this.State = 0;
            this.TurnsActive = turnsActive;
            this.HP = hp;
            this.Attack = attack;
        }
    }
    public class InstaSpell : Card {
        public InstaSpell(int cost, int playerId, Color color, int turnsActive) {
            this.EnergyCost = cost;
            this.PlayerId = playerId;
            this.Color = color;
            this.Type = Type.InstantSpell;
            this.State = 0;
            this.TurnsActive = 1;
        }
    }
}