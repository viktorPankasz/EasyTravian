using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTravian
{
    /// <summary>
    /// Nyersanyag küldés típusok
    /// </summary>
    public enum TraderType
    {
//        Ha_a_forrásfaluban_több_van_mint,      // if source village higher this amount
        Feltöltés,       // if destination village under this %
        Fölözés          // reaches % of max. stock
    }

    /// <summary>
    /// Nyersanyag küldés
    /// </summary>
    public class TraderItem
    {
        public bool Active { get; set; }

        public int SourceVillageId { get; set; }
        public int DestinationVillageId { get; set; }

        public TraderType Type { get; set; }

        /*
        public string TypeName
        {
            get { return Globals.Translator[Type.ToString()]; }
            set { Type = (TraderType)Enum.Parse(typeof(TraderType), Globals.Translator.DeTranslate(value)); }
        }
         */ 

        public int Value { get; set; }

        public bool ResourcesTypeLumber { get; set; }
        public bool ResourcesTypeClay { get; set; }
        public bool ResourcesTypeIron { get; set; }
        public bool ResourcesTypeCrop { get; set; }

        public bool AllowFullMerchantOnly { get; set; } // allow full merchant only
        public bool MaxDestinationCapacity { get; set; }

        public bool Outside { get; set; }
    }
}
