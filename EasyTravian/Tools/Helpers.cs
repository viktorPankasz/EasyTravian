using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace EasyTravian
{
    class Helpers
    {
        public static string GetURIParameter(string _uri, string _name)
        {
            if (_name != "id")
                return ParseURIParameters(new Uri(_uri).Query)[_name];
            return "";
        }

        public static Dictionary<string, string> ParseURIParameters(string ParamStr)
        {
            Dictionary<string, string> ParamDic = new Dictionary<string, string>();
            string[] Items = ParamStr.Split('&');
            foreach (string s in Items)
            {
                string[] Pair = (s.StartsWith("?") ? s.Substring(1) : s).Split('=');
                ParamDic.Add(Pair[0], (Pair.Length > 1 ? Pair[1] : ""));
            }
            return ParamDic;
        }

        // Zónapontból koordináta pontokat ad vissza egy kételemű tömbben.
        public static Point CoordsFromRaw(string RawCoord)
        {
            // function id2x($id){ return (($id - 1) % 801 - 400); }
            // function id2y($id){ return (400 - (int)(($id - 1) / 801)); } 
            Int32 D = Convert.ToInt32(RawCoord);
            int y = (400 - (int)((D - 1) / 801));
            int x = (D - 1) % 801 - 400;
            return new Point(x, y);
        }

        // Koordinátákból zóna pontot számol.
        public static string RawFromCoords(int x, int y)
        {
            // function xy2id($x, $y){ return 1 + ($x + 400) + (801 * abs($y - 400)); }
            // return ( 1 + ( parseInt ( x ) + 400 ) + ( 801 * Math.abs ( parseInt ( y ) - 400 ) ) );
            Int32 D = 1 + (x + 400) + (801 * Math.Abs(y - 400));
            return D.ToString();
        }

        public static string RawFromCoords(Point Coord)
        {
            return RawFromCoords(Coord.X, Coord.Y);
        }

        public static decimal Round(object obj, int dec)
        {
            // fujj: ((int)Math.Floor((double)(maradek / 100 * _lumberPercent)))
            decimal result = 0;
            if (obj == null)
                return 0;
            if (obj.GetType() == typeof(Decimal))
                result = Convert.ToDecimal(obj);
            else
                if (obj.GetType() == typeof(Int32))
                    result = Convert.ToDecimal(obj);
                else
                    if (obj.GetType() == typeof(double))
                        result = Convert.ToDecimal(obj);
            return Math.Round(result, dec);
        }

        public static decimal Round(object obj)
        {
            return Round(obj, 0);
        }
    }
}
