using System;
using System.Collections.Generic;
using System.Text;

namespace EasyTravian
{
    /// <summary>
    /// Itt van le�rva, hogy a k�lnb�z� t�pusu falvakban, hol �s mijen b�ny�k �p�thet�k
    /// </summary>
    class ResourceMaps
    {
        public static BuildingType[][] Map = new BuildingType[][]
            {
                //f1
                new BuildingType[] { BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Iron_mine,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                    }, 
                //f2
                new BuildingType[] {BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Iron_mine,
                                     BuildingType.Clay_pit,
                                     BuildingType.Clay_pit,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Iron_mine,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                    }, 
                //f3
                new BuildingType[] {BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Iron_mine,
                                     BuildingType.Clay_pit,
                                     BuildingType.Clay_pit,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Iron_mine,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                    }, 
                //f4 = 4536
                new BuildingType[] {BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                     BuildingType.Clay_pit,
                                     BuildingType.Clay_pit,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Iron_mine,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                    }, 
                //f5 = 5346
                new BuildingType[] {BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Iron_mine,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Iron_mine,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                    }, 
                //f6
                new BuildingType[] {BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                    }, 
                //f7 = 4437
                new BuildingType[] {BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                     BuildingType.Clay_pit,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Iron_mine,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                    }, 
                //f8 = 3447
                new BuildingType[] {BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                     BuildingType.Clay_pit,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Iron_mine,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                    }, 
                //f9 = 4347
                new BuildingType[] {BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Iron_mine,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                    }, 
                //f10 = 3546
                new BuildingType[] {BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                     BuildingType.Clay_pit,
                                     BuildingType.Clay_pit,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Iron_mine,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                    }, 
                // f11 = 4256
                new BuildingType[] {BuildingType.Iron_mine,
                                     BuildingType.Woodcutter,
                                     BuildingType.Woodcutter,
                                     BuildingType.Iron_mine,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Iron_mine,
                                     BuildingType.Iron_mine,
                                     BuildingType.Clay_pit,
                                     BuildingType.Clay_pit,
                                     BuildingType.Iron_mine,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                    }, 
                //f12 = 5436
                new BuildingType[] {BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                     BuildingType.Clay_pit,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Iron_mine,
                                     BuildingType.Iron_mine,
                                     BuildingType.Cropland,
                                     BuildingType.Cropland,
                                     BuildingType.Woodcutter,
                                     BuildingType.Cropland,
                                     BuildingType.Clay_pit,
                                     BuildingType.Woodcutter,
                                     BuildingType.Clay_pit,
                                    }, 
            };

    }
}
