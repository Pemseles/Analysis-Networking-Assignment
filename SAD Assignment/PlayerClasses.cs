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
            if (this.Deck.Count < 1) {
                return;
            }
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
            // (elements are added on top and taken from the top of shuffledDeck; would mean the 'guaranteed land' gets buried if not inverted)
            shuffledDeck = new Stack<Card>(shuffledDeck);
            this.Deck = shuffledDeck;
        }
        // overloads; no params is used to generate initial hand; int param is used in other specific situations
        public void FillHand() {
            // fills this.hand until there's 7 cards; moves cards from deck to hand
            while (this.Hand.Count < 7 && this.Deck.Count > 0) {
                this.Hand.Add(this.Deck.Pop());
            }
        }
        public void FillHand(int amount) {
            // fills this.hand with specified amount of cards; moves cards from deck to hand
            while (amount > 0 && this.Deck.Count > 0) {
                this.Hand.Add(this.Deck.Pop());
                amount--;
            }
        }
        public void DiscardHand(int amount) {
            // take cards from Hand to DiscardPile (equal to amount)
            // selects random cards to move from hand to discard pile;
            while (amount > 0 && this.Hand.Count > 0) {
                int handIndex = rnd.Next(this.Hand.Count);
                this.DiscardPile.Push(this.Hand[handIndex]);
                this.Hand.RemoveAt(handIndex);
                amount--;
            }
        }
        public void DiscardHand(Card specificCard) {
            // discards specific card from hand
            if (this.Hand.Count < 1) {
                return;
            }
            for (int i = 0; i < this.Hand.Count; i++) {
                if (specificCard.CardName == this.Hand[i].CardName && specificCard.State == this.Hand[i].State) {
                    this.DiscardPile.Push(this.Hand[i]);
                    this.Hand.RemoveAt(i);
                    return;
                }
            }
        }
        public void ChangeHP(int amount) {
            // (amount > 0) = damage; (amount < 0) = healing
            int newHP = this.HP - amount;
            if (newHP < 0) {
                this.HP = 0;
            }
            else {
                this.HP = newHP;
            }
        }
        public void ChangeEnergy(int amount, Color color) {
            // adds {amount} energy of specified color to energy reserve (if < 0 it subtracts)
            int newEnergyCount = this.EnergyReserve[color.ToString()] + amount;
            if (newEnergyCount < 0) {
                this.EnergyReserve[color.ToString()] = 0;
            }
            else {
                this.EnergyReserve[color.ToString()] = newEnergyCount;
            }
        }
        public void PlayCard<T1>(T1 card, T1 targetCard) {
            // activate effect of card (depending on effect, move to DiscardPile or keep in hand)
            if (typeof(T1) == typeof(Land)) {
                Land cardToPlay = card as Land;
                Land targetCardToPlay = card as Land;
                cardToPlay.Effect.ActivateEffect(targetCardToPlay);
            }
            else if (typeof(T1) == typeof(PermaSpell)) {
                PermaSpell cardToPlay = card as PermaSpell;
                PermaSpell targetCardToPlay = targetCard as PermaSpell;
                cardToPlay.Effect.ActivateEffect(targetCardToPlay);
            }
            else {
                InstaSpell cardToPlay = card as InstaSpell;
                InstaSpell targetCardToPlay = targetCard as InstaSpell;
                cardToPlay.Effect.ActivateEffect(targetCardToPlay);
            }
        }
    }
}