using System;
using System.Collections.Generic;

namespace SAD_Assignment
{
    public class EnergyContainer {
        public int RedEnergy { get; set; }
        public int BlueEnergy { get; set; }
        public int YellowEnergy { get; set; }
        public int GreenEnergy { get; set; }
        public int BlackEnergy { get; set; }
        public int ColorlessEnergy { get; set; }
        public EnergyContainer() {
            this.RedEnergy = this.BlueEnergy = this.YellowEnergy = 0;
            this.GreenEnergy = this.BlackEnergy = this.ColorlessEnergy = 0;
        }
    }
    public class Player {
        public int ID { get; }
        public Stack<Card> Deck { get; set; }
        public List<Card> Hand { get; set; }
        public Stack<Card> DiscardPile { get; set; }
        public int HP { get; set; }
        public EnergyContainer EnergyReserve { get; set; }
        public Player(string id, int hp, Stack<Card> deck) {
            this.ID = id;
            this.EnergyReserve = new EnergyContainer();
            this.HP = hp;
            this.Deck = deck;
            this.Hand = new List<Card>();
            this.DiscardPile = new Stack<Card>();
        }
    }
}