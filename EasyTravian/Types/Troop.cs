using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace EasyTravian.Types
{
    public class Troop
    {
        public int Id { get; set; }
        public TribeType? Tribe { get; set; }
        public string Name { get; set; }
        public string DisplayName { get { return Globals.Translator[Name]; } }
        public int Velocity { get; set; }
        public int Carry { get; set; }
        public int Attack { get; set; }
        public int DefenseInfantry { get; set; }
        public int DefenseCavalry { get; set; }
        public int Eating { get; set; }



        internal static SerializableDictionary<int, Troop> FillTroops()
        {
            SerializableDictionary<int, Troop> ret = new SerializableDictionary<int, Troop>();
            ret.Add(1, new Troop() { Id = 1, Tribe = TribeType.Roman, Name = "Legionnaire", Attack = 40, DefenseInfantry = 35, DefenseCavalry = 50, Velocity = 6, Carry = 50, Eating = 1 }); //Légió
            ret.Add(2, new Troop() { Id = 2, Tribe = TribeType.Roman, Name = "Praetorian", Attack = 30, DefenseInfantry = 65, DefenseCavalry = 35, Velocity = 5, Carry = 20, Eating = 1 }); // Testőrség
            ret.Add(3, new Troop() { Id = 3, Tribe = TribeType.Roman, Name = "Imperian", Attack = 70, DefenseInfantry = 40, DefenseCavalry = 25, Velocity = 7, Carry = 50, Eating = 1 }); // Birodalmi
            ret.Add(4, new Troop() { Id = 4, Tribe = TribeType.Roman, Name = "Equites Legati", Attack = 0, DefenseInfantry = 20, DefenseCavalry = 10, Velocity = 16, Carry = 0, Eating = 2 });
            ret.Add(5, new Troop() { Id = 5, Tribe = TribeType.Roman, Name = "Equites Imperatoris", Attack = 120, DefenseInfantry = 65, DefenseCavalry = 50, Velocity = 14, Carry = 100, Eating = 3 });
            ret.Add(6, new Troop() { Id = 6, Tribe = TribeType.Roman, Name = "Equites Caesaris", Attack = 180, DefenseInfantry = 80, DefenseCavalry = 105, Velocity = 10, Carry = 70, Eating = 4 });
            ret.Add(7, new Troop() { Id = 7, Tribe = TribeType.Roman, Name = "Battering Ram", Attack = 60, DefenseInfantry = 30, DefenseCavalry = 75, Velocity = 4, Carry = 0, Eating = 3 }); // Faltörő kos
            ret.Add(8, new Troop() { Id = 8, Tribe = TribeType.Roman, Name = "Fire Catapult", Attack = 75, DefenseInfantry = 60, DefenseCavalry = 10, Velocity = 3, Carry = 0, Eating = 6 }); // Tűzkatapult
            ret.Add(9, new Troop() { Id = 9, Tribe = TribeType.Roman, Name = "Senator", Attack = 50, DefenseInfantry = 40, DefenseCavalry = 30, Velocity = 4, Carry = 0, Eating = 5 }); // Szenátor
            ret.Add(10, new Troop() { Id = 10, Tribe = TribeType.Roman, Name = "Settler", Attack = 0, DefenseInfantry = 80, DefenseCavalry = 80, Velocity = 5, Carry = 3000, Eating = 1 }); // Telepes

            ret.Add(11, new Troop() { Id = 11, Tribe = TribeType.Teuton, Name = "Clubswinger", Attack = 40, DefenseInfantry = 20, DefenseCavalry = 50, Velocity = 7, Carry = 60, Eating = 1 }); // Buzogányos
            ret.Add(12, new Troop() { Id = 12, Tribe = TribeType.Teuton, Name = "Spearman", Attack = 10, DefenseInfantry = 35, DefenseCavalry = 60, Velocity = 7, Carry = 40, Eating = 1 }); // Lándzsás
            ret.Add(13, new Troop() { Id = 13, Tribe = TribeType.Teuton, Name = "Axeman", Attack = 60, DefenseInfantry = 30, DefenseCavalry = 30, Velocity = 6, Carry = 50, Eating = 1 }); // Csatabárdos
            ret.Add(14, new Troop() { Id = 14, Tribe = TribeType.Teuton, Name = "Scout", Attack = 0, DefenseInfantry = 10, DefenseCavalry = 5, Velocity = 9, Carry = 0, Eating = 1 }); // Felderítő
            ret.Add(15, new Troop() { Id = 15, Tribe = TribeType.Teuton, Name = "Paladin", Attack = 55, DefenseInfantry = 100, DefenseCavalry = 40, Velocity = 10, Carry = 110, Eating = 2 });
            ret.Add(16, new Troop() { Id = 16, Tribe = TribeType.Teuton, Name = "Teutonic Knight", Attack = 150, DefenseInfantry = 75, DefenseCavalry = 50, Velocity = 9, Carry = 80, Eating = 3 }); // Teuton lovag
            ret.Add(17, new Troop() { Id = 17, Tribe = TribeType.Teuton, Name = "Ram", Attack = 65, DefenseInfantry = 30, DefenseCavalry = 80, Velocity = 4, Carry = 0, Eating = 3 }); // Faltörő kos
            ret.Add(18, new Troop() { Id = 18, Tribe = TribeType.Teuton, Name = "Catapult", Attack = 50, DefenseInfantry = 60, DefenseCavalry = 10, Velocity = 3, Carry = 0, Eating = 6 }); // Katapult
            ret.Add(19, new Troop() { Id = 19, Tribe = TribeType.Teuton, Name = "Chief", Attack = 40, DefenseInfantry = 60, DefenseCavalry = 40, Velocity = 4, Carry = 0, Eating = 1 }); // Törzsi vezető
            ret.Add(20, new Troop() { Id = 20, Tribe = TribeType.Teuton, Name = "Settler", Attack = 10, DefenseInfantry = 80, DefenseCavalry = 80, Velocity = 5, Carry = 3000, Eating = 1 }); // Telepes

            ret.Add(21, new Troop() { Id = 21, Tribe = TribeType.Gaul, Name = "Phalanx", Attack = 15, DefenseInfantry = 40, DefenseCavalry = 50, Velocity = 7, Carry = 35, Eating = 1 });
            ret.Add(22, new Troop() { Id = 22, Tribe = TribeType.Gaul, Name = "Swordsman", Attack = 65, DefenseInfantry = 35, DefenseCavalry = 20, Velocity = 6, Carry = 45, Eating = 1 });
            ret.Add(23, new Troop() { Id = 23, Tribe = TribeType.Gaul, Name = "Pathfinder", Attack = 0, DefenseInfantry = 20, DefenseCavalry = 10, Velocity = 17, Carry = 0, Eating = 2 });
            ret.Add(24, new Troop() { Id = 24, Tribe = TribeType.Gaul, Name = "Theutates Thunder", Attack = 90, DefenseInfantry = 25, DefenseCavalry = 40, Velocity = 19, Carry = 75, Eating = 2 }); // Theutat Villám
            ret.Add(25, new Troop() { Id = 25, Tribe = TribeType.Gaul, Name = "Druidrider", Attack = 45, DefenseInfantry = 115, DefenseCavalry = 55, Velocity = 16, Carry = 35, Eating = 2 }); // Druida lovas
            ret.Add(26, new Troop() { Id = 26, Tribe = TribeType.Gaul, Name = "Haeduan", Attack = 140, DefenseInfantry = 50, DefenseCavalry = 165, Velocity = 13, Carry = 65, Eating = 3 });
            ret.Add(27, new Troop() { Id = 27, Tribe = TribeType.Gaul, Name = "Ram", Attack = 50, DefenseInfantry = 30, DefenseCavalry = 105, Velocity = 4, Carry = 0, Eating = 3 });
            ret.Add(28, new Troop() { Id = 28, Tribe = TribeType.Gaul, Name = "Trebuchet", Attack = 70, DefenseInfantry = 45, DefenseCavalry = 10, Velocity = 3, Carry = 0, Eating = 6 }); // Harci-katapult
            ret.Add(29, new Troop() { Id = 29, Tribe = TribeType.Gaul, Name = "Chieftain", Attack = 40, DefenseInfantry = 50, DefenseCavalry = 50, Velocity = 5, Carry = 0, Eating = 4 }); // Főnök
            ret.Add(30, new Troop() { Id = 30, Tribe = TribeType.Gaul, Name = "Settler", Attack = 0, DefenseInfantry = 80, DefenseCavalry = 80, Velocity = 5, Carry = 3000, Eating = 1 });

            // nature
            // natar
            // http://help.travian.com/index.php?type=faq&mod=440

            return ret;
        }
    }

    /// <summary>
    /// "Érkező csapatok", "Faluban lévő csapatok", "Mozgó csapatok", "Más falvakban levő csapatok"
    /// todo: a völgyekben lévőket is kellene
    /// </summary>
    public enum RallySection { Incoming, Into, Outgoing, NoHome }
    public enum ArmyState { Attack, Raid, Return, Reinforcement, Self, HomeTrap, OutTrap }

    public static class EnumHelpers
    {
        // Bozsó, ezt hogy kell ? (enum - string párosok)
        public static string RallySection2Text(RallySection rs)
        {
            string retval = "";
            switch (rs)
            {
                case RallySection.Incoming:
                    retval = Globals.Translator["Érkező csapatok"];
                    break;
                case RallySection.Into:
                    retval = Globals.Translator["Faluban lévő csapatok"];
                    break;
                case RallySection.Outgoing:
                    retval = Globals.Translator["Mozgó csapatok"];
                    break;
                case RallySection.NoHome:
                    retval = Globals.Translator["Más falvakban levő csapatok"];
                    break;
                default:
                    break;
            }
            return retval;
        }
        public static string ArmyState2Text(ArmyState ast)
        {
            string retval = "";
            switch (ast)
            {
                case ArmyState.Attack:
                    retval = Globals.Translator["Támadás"];
                    break;
                case ArmyState.Raid:
                    retval = Globals.Translator["Fosztogatás"];
                    break;
                case ArmyState.Return:
                    retval = Globals.Translator["Visszatérés"];
                    break;
                case ArmyState.Reinforcement:
                    retval = Globals.Translator["Támogatás"];
                    break;
                case ArmyState.Self:
                    retval = Globals.Translator["Saját csapatok"];
                    break;
                case ArmyState.HomeTrap:
                    retval = Globals.Translator[""];
                    break;
                case ArmyState.OutTrap:
                    retval = Globals.Translator["Csapdába ejtett katonák("];
                    break;
                default:
                    break;
            }
            return retval;
        }
        public static string ArmyState2TextPost(ArmyState ast)
        {
            string retval = "";
            switch (ast)
            {
                case ArmyState.Attack:
                    retval = Globals.Translator["falu ellen"];
                    break;
                case ArmyState.Raid:
                    retval = Globals.Translator["faluban"];
                    break;
                case ArmyState.Return:
                    retval = Globals.Translator["faluból"];
                    break;
                case ArmyState.Reinforcement:
                    retval = Globals.Translator["falunak"];
                    break;
                case ArmyState.Self:
                    retval = Globals.Translator[""];
                    break;
                case ArmyState.HomeTrap:
                    retval = Globals.Translator["csapatai"];
                    break;
                case ArmyState.OutTrap:
                    retval = Globals.Translator["csapatai)"];
                    break;
                default:
                    break;
            }
            return retval;
        }

    }

    public class ArmyItem
    {
        public RallySection Section { get; set; }
        public ArmyState State { get; set; }

        public string SourceVillageName { get; set; }
        public string SourceVillageURL { get; set; }
        public Point SourceVillageCoord { get; set; }

        public string DestinationVillageName { get; set; }
        public string DestinationVillageURL { get; set; }
        public Point DestinationVillageCoord { get; set; }

        public DateTime Identify { get; set; } // észlelés
        public DateTime Arrival { get; set; } // érkezés

        public Units Units;

        public ArmyItem(RallySection rallySection)
        {
            SourceVillageName = "";
            DestinationVillageName = "";

            Section = rallySection;
            Identify = DateTime.Now;
        }

        public void SetArrival(string ArrivalSpan, string ArrivalText)
        {
            TimeSpan TravelTime = TimeSpan.Parse(ArrivalSpan);
            Arrival = Identify + TravelTime;

            Arrival = Arrival.AddHours(-Arrival.Hour);
            Arrival = Arrival.AddMinutes(-Arrival.Minute);
            Arrival = Arrival.AddSeconds(-Arrival.Second);
            Arrival = Arrival.AddMilliseconds(-Arrival.Millisecond);

            TimeSpan ArivalTime = TimeSpan.Parse(ArrivalText);
            Arrival = Arrival + ArivalTime;
        }

    };

}
