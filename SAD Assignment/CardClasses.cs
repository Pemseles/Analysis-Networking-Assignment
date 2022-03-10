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
        public int State { get; set; }
        public int TurnsActive { get; set; }
        public abstract string GetInfo();
    }
    public class Land : Card {
        public CardEffect<Land> Effect { get; set; }
        public Land(int playerId, string cardName, string cardDescription, Color color, CardEffect<Land> effect) {
            this.EnergyCost = 0;
            this.PlayerId = playerId;
            this.CardName = cardName;
            this.CardDescription = cardDescription;
            this.Color = color;
            this.Type = Type.Land;
            this.State = 0;
            this.TurnsActive = -1;
            this.Effect = effect;
        }
        public override string GetInfo()
        {
            return $"[Land Info:] PlayerId={this.PlayerId}, CardName={this.CardName}, CardDescription={this.CardDescription}, EnergyCost={this.EnergyCost}, Color={this.Color}, Type={this.Type}, Effect={this.Effect}, State={this.State}, TurnsActive={this.TurnsActive}";
        }
    }
    public class PermaSpell : Card {
        public CardEffect<PermaSpell> Effect { get; set; }
        public int HP { get; set; }
        public int Attack { get; set; }
        public PermaSpell(int cost, int playerId, string cardName, string cardDescription, Color color, int turnsActive, CardEffect<PermaSpell> effect, int hp, int attack) {
            this.EnergyCost = cost;
            this.PlayerId = playerId;
            this.CardName = cardName;
            this.CardDescription = cardDescription;
            this.Color = color;
            this.Type = Type.PermanentSpell;
            this.State = 0;
            this.TurnsActive = turnsActive;
            this.Effect = effect;
            this.HP = hp;
            this.Attack = attack;
        }
        public override string GetInfo()
        {
            return $"[Perma Info:] PlayerId={this.PlayerId}, CardName={this.CardName}, CardDescription={this.CardDescription}, EnergyCost={this.EnergyCost}, HP={this.HP}, Attack={this.Attack} Color={this.Color}, Type={this.Type}, Effect={this.Effect}, State={this.State}, TurnsActive={this.TurnsActive}";
        }
    }
    public class InstaSpell : Card {
        public CardEffect<InstaSpell> Effect { get; set; }
        public InstaSpell(int cost, int playerId, string cardName, string cardDescription, Color color, CardEffect<InstaSpell> effect) {
            this.EnergyCost = cost;
            this.PlayerId = playerId;
            this.CardName = cardName;
            this.CardDescription = cardDescription;
            this.Color = color;
            this.Effect = effect;
            this.Type = Type.InstantSpell;
            this.State = 0;
            this.TurnsActive = 1;
        }
        public override string GetInfo()
        {
            return $"[Insta Info:] PlayerId={this.PlayerId}, CardName={this.CardName}, CardDescription={this.CardDescription}, EnergyCost={this.EnergyCost}, Color={this.Color}, Type={this.Type}, Effect={this.Effect}, State={this.State}, TurnsActive={this.TurnsActive}";
        }
    }
}