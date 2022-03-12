using System;
using System.Collections.Generic;
using System.Linq;

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
        public static Random rnd = new Random();
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

            // init of new shuffled Deck and a copy of current unshuffled deck
            Stack<Card> shuffledDeck = new Stack<Card>();
            List<Card> cardList = new List<Card>(this.Deck);

            while (cardList.Count != 0) {
                // generate random index; item at index in unshuffled deck is moved to new deck
                int cardListIndex = rnd.Next(cardList.Count);

                // guarantees at least 1 land in the first 7 cards (is for when filling the player's hand, they can actually do something with their hand)
                if (shuffledDeck.Count == 6 && !shuffledDeck.Any(c => c.Type == Type.Land) && cardList[cardListIndex].Type != Type.Land) {
                    for (int i = 0; i < cardList.Count; i++) {
                        if (cardList[i].Type == Type.Land) {
                            cardListIndex = i;
                        }
                    }
                }
                shuffledDeck.Push(cardList[cardListIndex]);
                cardList.RemoveAt(cardListIndex);
            }
            // deck is inverted; otherwise the 'at least 1 land' condition won't work 
            // (elements are added on top and taken from the top; would mean the 'guaranteed land' gets buried if not inverted)
            shuffledDeck = new Stack<Card>(shuffledDeck);
            this.Deck = shuffledDeck;
        }
        public void DiscardHand(int amount) {
            // take cards from Hand to DiscardPile (equal to amount)
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