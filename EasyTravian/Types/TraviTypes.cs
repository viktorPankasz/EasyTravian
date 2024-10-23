using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;

namespace EasyTravian
{
    /// <summary>
    /// Nyersanyag típusok
    /// </summary>
    public enum ResourcesType
	{
	    Lumber=4,
        Clay=3,
        Iron=2,
        Crop=1
	}  

    /// <summary>
    /// Épület típusok - Bányák és épületek egyben
    /// pl. Fõépület: http://s6.travian.hu/build.php?gid=15
    /// Lóitató http://tx3.travian.hu/build.php?gid=41
    /// </summary>
    public enum BuildingType
    {
        None = -1,
        Woodcutter,
        Clay_pit,
        Iron_mine,
        Cropland,
        Sawmill,
        Brickyard,
        Iron_foundry,
        Grain_mill,
        Bakery,
        Warehouse,
        Granary,
        Blacksmith,
        Armoury,
        Tournament_square,
        Main_building,
        Rally_point,
        Marketplace,
        Embassy,
        Barracks,
        Stable,
        Workshop,
        Academy,
        Cranny,
        Townhall,
        Residence,
        Palace,
        Treasury,
        Trade_office,
        Great_barracks,
        Great_stable,
        City_wall,
        Earth_wall,
        Palisade,
        Stonemason,
        Brewery,
        Trapper,
        Heros_mansion, 
        Great_Warehouse, 
        Great_Granary, 
        Wonder_of_the_World, 
        Horse_Drinking_Trough
    }

    public static class TypeHelpers
    {
        public static int GetBuildCategory(BuildingType buildingType)
        {
            /*
            Infrastruktúra
            http://tx3.travian.hu/build.php?id=30&category=1

            Katonaság
            http://tx3.travian.hu/build.php?id=30&category=2

            nyersi
            http://tx3.travian.hu/build.php?id=30&category=3
            */
            switch (buildingType)
            {
                case BuildingType.Woodcutter:
                case BuildingType.Clay_pit:
                case BuildingType.Iron_mine:
                case BuildingType.Cropland:
                    return 0;
                case BuildingType.Warehouse:
                case BuildingType.Granary:
                case BuildingType.Main_building:
                case BuildingType.Marketplace:
                case BuildingType.Cranny:
                case BuildingType.Residence:
                case BuildingType.Palace:
                case BuildingType.Trade_office:
                case BuildingType.Stonemason:
                case BuildingType.Great_Warehouse:
                case BuildingType.Great_Granary:
                case BuildingType.Wonder_of_the_World:
                case BuildingType.Embassy:
                case BuildingType.Treasury:
                case BuildingType.Brewery:
                case BuildingType.Townhall:
                    return 1;
                case BuildingType.Blacksmith:
                case BuildingType.Armoury:
                case BuildingType.Tournament_square:
                case BuildingType.Rally_point:
                case BuildingType.Barracks:
                case BuildingType.Stable:
                case BuildingType.Workshop:
                case BuildingType.Academy:
                case BuildingType.Great_barracks:
                case BuildingType.Great_stable:
                case BuildingType.City_wall:
                case BuildingType.Earth_wall:
                case BuildingType.Palisade:
                case BuildingType.Trapper:
                case BuildingType.Heros_mansion:
                case BuildingType.Horse_Drinking_Trough:
                    return 2;
                case BuildingType.Sawmill:
                case BuildingType.Brickyard:
                case BuildingType.Iron_foundry:
                case BuildingType.Grain_mill:
                case BuildingType.Bakery:
                    return 3;
                default:
                    return 0;
            }
        }

        public static bool Contains(this Enum keys, Enum flag)
        {
            if (keys.GetType() != flag.GetType())
                throw new ArgumentException("Type Mismatch");
            return (Convert.ToUInt64(keys) & Convert.ToUInt64(flag)) != 0;
        }
    }

    /// <summary>
    /// Törzsek. Natar nemtom minek kell...
    /// </summary>
    public enum TribeType
    {
        Roman, Gaul, Teuton, Natar
    }

    /// <summary>
    /// Egy nyersanyag termelésének a tulajdonságai
    /// </summary>
    public class Production
    {
        private ResourcesType type;
        private int capacity;
        private int stock;
        private int producing;
        public int TargetPercent { get; set; }
        public int ActPercent { get; set; }
        public int FullnessPercent 
        {
            get
            {
                return (int)Math.Floor(((double)stock / (double)capacity * 100));
            }
        }

        public int FreeCapacity { get { return Capacity - Stock; } }

        public string TypeName
        {
            get { return Globals.Translator[type.ToString()]; }
        }

        public ResourcesType Type
        {
            get { return type; }
            set { type = value; }
        }

        public int Capacity
        {
            get { return capacity; }
            set { capacity = value; }
        }

        public int Stock
        {
            get { return stock; }
            set { stock = value; }
        }

        public int Producing
        {
            get { return producing; }
            set { producing = value; }
        }

    }

    /// <summary>
    /// Egy épület vagy bánya tulajdonságai
    /// </summary>
    public class Building
    {
        public int Id;

        public BuildingType type;
        public BuildingType Type 
        {
            get
            {
                return type;
            }
            set
            {
                if (type != BuildingType.None && target > 0 && value == BuildingType.None)
                    type = value;

            	type = value;
            } 
        }

        [NonSerialized]
        [XmlIgnore()]
        private int level;

        private int target;
        public string href;
        public int BuildId;

        [NonSerialized]
        [XmlIgnore()]
        public TimeSpan BuildTime;

        [NonSerialized]
        public string BuildHref;

        public ResourcesType? Producing
        {
            get 
            {
                switch (Type)
                {
                    case BuildingType.Woodcutter:
                        return ResourcesType.Lumber;
                    case BuildingType.Clay_pit:
                        return ResourcesType.Clay;
                    case BuildingType.Iron_mine:
                        return ResourcesType.Iron;
                    case BuildingType.Cropland:
                        return ResourcesType.Crop;
                    default:
                        return null;
                }
            }
        }

        public int BuildPriority = 0;
        //public SerializableDictionary<ResourcesType, int> NextLevelCost = new SerializableDictionary<ResourcesType, int>();

        [NonSerialized()]
        [XmlIgnore()]
        public Resources NextLevelCost = new Resources(0, 0, 0, 0);

        [XmlIgnore()]
        public string Name
        {
            get { return Globals.Translator[Type.ToString()]; }
            set { Type = (BuildingType)Enum.Parse(typeof(BuildingType), Globals.Translator.DeTranslate(value)); }
        }

        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        public int Target
        {
            get { return target; }
            set { target = value; }
        }

        [field: NonSerializedAttribute()]
        [XmlIgnore()]
        public bool UnderConstruction;

        public bool Demolition = false;
    }

    /// <summary>
    /// Egy térkép elem tulajdonságai
    /// </summary>
    public class MapElement
    {
        public DateTime TimeStamp;
        public int Id;
        public int X;
        public int Y;
        public int Tid;
        public int Vid;
        public string Village;
        public int Uid;
        public string Player;
        public int Aid;
        public string Alliance;
        public int Population;
        public string Terrain;
    }

    /// <summary>
    /// Egy falu taljdonságai
    /// </summary>
    public class VillageProps
    {
        public int Id;
        public string Name;
        public string url;
        public Point Origin;
    }

    public enum TerrainType
    {
        NA,
        field_9crop,             // f1
        field_5iron,             // f2
        field_4all,              // f3
        field_5clay,             // f4    
        field_5lumber,           // f5
        field_15crop,            // f6
        oase_lumber25,           // o1, o2
        oase_lumber25_crop25,    // o3
        oase_clay25,             // o4, o5
        oase_clay25_crop25,      // o6
        oase_iron25,             // o7, o8
        oase_iron25_crop25,      // o9
        oase_crop25,             // o10,o11
        oase_crop50,             // o12
    }

    public class Resources
    {
        public Resources()
        {
        }
        public Resources(int Lumber, int Clay, int Iron, int Crop)
        {
            list[ResourcesType.Lumber] = Lumber;
            list[ResourcesType.Clay] = Clay;
            list[ResourcesType.Iron] = Iron;
            list[ResourcesType.Crop] = Crop;
        }

        public SerializableDictionary<ResourcesType, int> list = new SerializableDictionary<ResourcesType, int>();

        public int this[ResourcesType Type]
        {
            get
            {
                return list[Type];
            }
            set
            {
                list[Type] = value;
            }
        }

        public int Sum
        {
            get
            {
                try
                {
                    return
                        list[ResourcesType.Lumber] +
                        list[ResourcesType.Clay] +
                        list[ResourcesType.Iron] +
                        list[ResourcesType.Crop];
                }
                catch
                {
                    return 0;
                }
            }
        }

        public void Clear()
        {
            list[ResourcesType.Lumber] = 0;
            list[ResourcesType.Clay] = 0;
            list[ResourcesType.Iron] = 0;
            list[ResourcesType.Crop] = 0;
        }

        public static bool operator >( Resources res1, Resources res2 )
        {
            return res1[ResourcesType.Lumber] > res2[ResourcesType.Lumber]
                   ||
                   res1[ResourcesType.Clay] > res2[ResourcesType.Clay]
                   ||
                   res1[ResourcesType.Iron] > res2[ResourcesType.Iron]
                   ||
                   res1[ResourcesType.Crop] > res2[ResourcesType.Crop];
        }
        public static bool operator <(Resources res1, Resources res2)
        {
            return res1[ResourcesType.Lumber] <= res2[ResourcesType.Lumber]
                   &&
                   res1[ResourcesType.Clay] <= res2[ResourcesType.Clay]
                   &&
                   res1[ResourcesType.Iron] <= res2[ResourcesType.Iron]
                   &&
                   res1[ResourcesType.Crop] <= res2[ResourcesType.Crop];
        }

        public static Resources operator +(Resources res1, Resources res2)
        {
            return new Resources(res1[ResourcesType.Lumber] + res2[ResourcesType.Lumber],
                                  res1[ResourcesType.Clay] + res2[ResourcesType.Clay],
                                  res1[ResourcesType.Iron] + res2[ResourcesType.Iron],
                                  res1[ResourcesType.Crop] + res2[ResourcesType.Crop]);
        }


        override public string ToString()
        {
            return
                list[ResourcesType.Lumber].ToString() + '|' +
                list[ResourcesType.Clay].ToString() + '|' +
                list[ResourcesType.Iron].ToString() + '|' +
                list[ResourcesType.Crop].ToString();
        }
    }

    public class Construction
    {
        public BuildingType Building { get; set; }
        public string Name 
        {
            get
            {
                return Globals.Translator[Building.ToString()];
            }
        }
        public int Level { get; set; }
        public DateTime Started { get; set; }
        public DateTime Ends { get; set; }
        public string CancelUrl { get; set; }
    }

    public class BuildingDisplay
    {
        public BuildingDisplay(BuildingType type, int level, Resources cost, bool buildable)
        {
            Type = type;
            Level = level;
            Cost = cost;
            Buildable = buildable;
        }

        public BuildingDisplay()
        {

        }

        public BuildingType Type;
        public string Name
        {
            get { return Globals.Translator[Type.ToString()]; }
        }
        public int Level { get; set; }
        public Resources Cost { get; set; }

        public int Lumber { get { return Cost[ResourcesType.Lumber]; } }
        public int Clay { get { return Cost[ResourcesType.Clay]; } }
        public int Iron { get { return Cost[ResourcesType.Iron]; } }
        public int Crop { get { return Cost[ResourcesType.Crop]; } }

        public bool Buildable { get; set; }
    }

    public class Units
    {
      public TribeType Tribe { get; set; }

      private int[] UnitCount;

      public Units()
      {
        UnitCount = new int[10];
        for (int i = 0; i < UnitCount.Length; i++)
          UnitCount[i] = 0;
      }

    };

    public enum SendArmyAttackType { Reinforcement, NormalAttack, Raid }
}
