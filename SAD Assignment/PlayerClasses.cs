using System;
using System.Collections.Generic;

namespace SAD_Assignment
{
    public class PlayerContainer {
        public static Player Player1 { get; set; }
        public static Player Player2 { get; set; }
        public PlayerContainer(int playerHP) {
            Player1 = new Player(1, playerHP);
            Player2 = new Player(2, playerHP);
        }
    }
    public class Player {
        public int ID { get; }
        public Stack<Card> Deck { get; set; }
        public List<Card> Hand { get; set; }
        public Stack<Card> DiscardPile { get; set; }
        public int HP { get; set; }
        public Dictionary<string, int> EnergyReserve { get; set; }
        public Player(int id, int hp) {
            this.ID = id;
            this.EnergyReserve = new Dictionary<string, int>(){
                {"Red", 0},
                {"Blue", 0},
                {"Yellow", 0},
                {"Green", 0},
                {"Black", 0},
                {"Colorless", 0}
            };
            this.HP = hp;
            this.Deck = null;
            this.Hand = new List<Card>();
            this.DiscardPile = new Stack<Card>();
        }
        public void AddDeck(Stack<Card> deck) {
            this.Deck = deck;
        }
        public void ShuffleDeck() {
            // randomises order of Cards in Deck
        }
        public void DiscardHand(int amount) {
            // take cards from Deck to DiscardPile (equal to amount)
        }
        public void ChangeHP(int amount) {
            // (amount > 0) = damage; (amount < 0) = healing

        }
        public void ChangeEnergy(int amount, Color color) {
            // adds {amount} energy of specified color to energy reserve (if < 0 it subtracts)
            this.EnergyReserve[color.ToString()] = this.EnergyReserve[color.ToString()] + amount;
        }
        public void DrawCards(int amount) {
            // take cards from Deck to Hand (equal to amount)
        }
        public void PlayCard(Card cardToPlay) {
            // activate effect of card (depending on effect, move to DiscardPile or keep in hand)
        }
    }
}