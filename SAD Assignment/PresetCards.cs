using System;
using System.Collections.Generic;

namespace SAD_Assignment
{
    public enum Target { Yours, Opponents }
    public class PresetContainer {
        public static PresetLands Lands { get; set; }
        public static List<Land> LandList { get; set; }
        public static PresetPermas Permas { get; set; }
        public static List<PermaSpell> PermaList { get; set; }
        public static PresetInstas Instas { get; set; }
        public static List<InstaSpell> InstaList { get; set; }
        public PresetContainer(Player p) {
            Lands = new PresetLands(p);
            Permas = new PresetPermas(p);
            Instas = new PresetInstas(p);
        }
    }
    public class PresetLands {
        // turn these into one single list PLEASE
        public static Land Land_Red { get; set; }
        public static Land Land_Blue { get; set; }
        public static Land Land_Yellow { get; set; }
        public static Land Land_Green { get; set; }
        public static Land Land_Black { get; set; }
        public static Land Land_Colorless { get; set; }
        public PresetLands(Player p) {
            Land_Red = new Land(p.ID, "Ancient Volcano", "A red-hot environment which can generate red energy for you.", Color.Red, new GenerateEnergy());
            Land_Blue = new Land(p.ID, "Deep Ocean", "An underwater environment which can generate blue energy for you.", Color.Blue , new GenerateEnergy());
            Land_Yellow = new Land(p.ID, "Sunset Highlands", "A mountainous environment which can generate yellow energy for you.", Color.Yellow , new GenerateEnergy());
            Land_Green = new Land(p.ID, "Lush Jungle", "A tropical rainforest environment which can generate green energy for you.", Color.Green , new GenerateEnergy());
            Land_Black = new Land(p.ID, "Dark City", "A shady megalopolis environment which can generate black energy for you.", Color.Black , new GenerateEnergy());
            Land_Colorless = new Land(p.ID, "Genetic Lab", "A bland laboratory environment which can generate colorless energy for you.", Color.Colorless , new GenerateEnergy());
        }
    }
    public class PresetPermas {
        // turn these into one single list PLEASE
        public static PermaSpell Creature_Red { get; set; }
        public static PermaSpell Creature_Blue { get; set; }
        public static PermaSpell Creature_Yellow { get; set; }
        public static PermaSpell Creature_Green { get; set; }
        public static PermaSpell Creature_Black { get; set; }
        public static PermaSpell Creature_Colorless { get; set; }
        public PresetPermas(Player p) {
            Creature_Red = new PermaSpell(2, p.ID, "Flamebringer", "A simple, fiery creature with lots of attack, but lacking in defenses. Does not have a special ability.", Color.Red, 3, null, 2, 6);
            Creature_Blue = new PermaSpell(1, p.ID, "Slime", "A lackluster but cheap creature, capable of doing light damage but can't take hits very well. Can buff the defense of your creatures by 1.", Color.Blue, 4, new StatAugmentPerma(0, 1, Target.Yours), 2, 2);
            Creature_Yellow = new PermaSpell(3, p.ID, "Bouldersmasher", "A defensive but offensively weak creature, capable of taking many hits. Can debuff opposing creatures' attack by 2.", Color.Yellow, 6, new StatAugmentPerma(-2, 0, Target.Opponents), 10, 1);
            Creature_Green = new PermaSpell(3, p.ID, "Vegemonster", "A balanced creature, capable being both decent offensively and defensively. Does not have a special ability.", Color.Green, 4, null, 5, 5);
            Creature_Black = new PermaSpell(2, p.ID, "Shady Shroom", "A malicious creature with low-ish stats. Can debuff opposing creatures' attack and defense by 1.", Color.Black, 5, new StatAugmentPerma(-1, -1, Target.Opponents), 4, 3);
            Creature_Colorless = new PermaSpell(4, p.ID, "Chemburner", "A strange and unstable creature with decent stats. Can buff your creatures' attack by 2.", Color.Colorless, 3, new StatAugmentPerma(0, 2, Target.Yours), 5, 4);
        }
    }
    public class PresetInstas {
        // turn these into one single list PLEASE
        public static InstaSpell CounterSpell_Red { get; set; }
        public static InstaSpell CounterSpell_Blue { get; set; }
        public static InstaSpell CounterSpell_Yellow { get; set; }
        public static InstaSpell CounterSpell_Green { get; set; }
        public static InstaSpell CounterSpell_Black { get; set; }
        public static InstaSpell CounterSpell_Colorless { get; set; }
        public static InstaSpell BuffSpell_Red { get; set; }
        public static InstaSpell BuffSpell_Blue { get; set; }
        public static InstaSpell BuffSpell_Yellow { get; set; }
        public static InstaSpell BuffSpell_Green { get; set; }
        public static InstaSpell BuffSpell_Black { get; set; }
        public static InstaSpell BuffSpell_Colorless { get; set; }
        public static InstaSpell DebuffSpell_Red { get; set; }
        public static InstaSpell DebuffSpell_Blue { get; set; }
        public static InstaSpell DebuffSpell_Yellow { get; set; }
        public static InstaSpell DebuffSpell_Green { get; set; }
        public static InstaSpell DebuffSpell_Black { get; set; }
        public static InstaSpell DebuffSpell_Colorless { get; set; }
        public PresetInstas(Player p) {
            CounterSpell_Red = new InstaSpell(1, p.ID, "Red Counter", "A red instant spell that can counter the most recently used card by the opponent.", Color.Red, new CounterSpell());
            CounterSpell_Blue = new InstaSpell(1, p.ID, "Blue Counter", "A blue instant spell that can counter the most recently used card by the opponent.", Color.Blue, new CounterSpell());
            CounterSpell_Yellow = new InstaSpell(1, p.ID, "Yellow Counter", "A yellow instant spell that can counter the most recently used card by the opponent.", Color.Yellow, new CounterSpell());
            CounterSpell_Green = new InstaSpell(1, p.ID, "Green Counter", "A green instant spell that can counter the most recently used card by the opponent.", Color.Green, new CounterSpell());
            CounterSpell_Black = new InstaSpell(1, p.ID, "Black Counter", "A black instant spell that can counter the most recently used card by the opponent.", Color.Black, new CounterSpell());
            CounterSpell_Colorless = new InstaSpell(1, p.ID, "Colorless Counter", "A colorless instant spell that can counter the most recently used card by the opponent.", Color.Colorless, new CounterSpell());

            BuffSpell_Red = new InstaSpell(2, p.ID, "Red Buff", "A red instant spell that buffs all your creatures' attack by 1.", Color.Red, new StatAugmentInsta(0, 1, Target.Yours));
            BuffSpell_Blue = new InstaSpell(1, p.ID, "Blue Buff", "A blue instant spell that buffs all your creatures' defense by 1.", Color.Blue, new StatAugmentInsta(1, 0, Target.Yours));
            BuffSpell_Yellow = new InstaSpell(2, p.ID, "Yellow Buff", "A yellow instant spell that buffs all your creatures' defense by 2.", Color.Yellow, new StatAugmentInsta(2, 0, Target.Yours));
            BuffSpell_Green = new InstaSpell(3, p.ID, "Green Buff", "A green instant spell that buffs all your creatures' attack and defense by 1.", Color.Green, new StatAugmentInsta(1, 1, Target.Yours));
            BuffSpell_Black = new InstaSpell(4, p.ID, "Black Buff", "A black instant spell that buffs all your creatures' attack by 1 and defense by 2.", Color.Black, new StatAugmentInsta(2, 1, Target.Yours));
            BuffSpell_Colorless = new InstaSpell(4, p.ID, "Colorless Buff", "A colorless instant spell that buffs all your creatures' attack by 2 and defense by 1.", Color.Colorless, new StatAugmentInsta(1, 2, Target.Yours));

            DebuffSpell_Red = new InstaSpell(2, p.ID, "Red Debuff", "A Red instant spell that debuffs all the opponents' creatures' attack by 1.", Color.Red, new StatAugmentInsta(0, -1, Target.Opponents));
            DebuffSpell_Blue = new InstaSpell(1, p.ID, "Blue Debuff", "A Blue instant spell that debuffs all the opponents' creatures' defense by 1.", Color.Blue, new StatAugmentInsta(-1, 0, Target.Opponents));
            DebuffSpell_Yellow = new InstaSpell(2, p.ID, "Yellow Debuff", "A Yellow instant spell that debuffs all the opponents' creatures' defense by 2.", Color.Yellow, new StatAugmentInsta(-2, 0, Target.Opponents));
            DebuffSpell_Green = new InstaSpell(3, p.ID, "Green Debuff", "A Green instant spell that debuffs all the opponents' creatures' attack and defense by 1.", Color.Green, new StatAugmentInsta(-1, -1, Target.Opponents));
            DebuffSpell_Black = new InstaSpell(4, p.ID, "Black Debuff", "A Black instant spell that debuffs all the opponents' creatures' attack by 1 and defense by 2.", Color.Black, new StatAugmentInsta(-2, -1, Target.Opponents));
            DebuffSpell_Colorless = new InstaSpell(4, p.ID, "Colorless Debuff", "A Colorless instant spell that debuffs all the opponents' creatures' attack by 2 and defense by 1.", Color.Colorless, new StatAugmentInsta(-1, -2, Target.Opponents));
        }
    }
}