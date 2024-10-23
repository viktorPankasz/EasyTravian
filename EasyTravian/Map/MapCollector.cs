using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace EasyTravian
{
    /// <summary>
    /// Térképészeti osztály
    /// </summary>
    class MapCollector
    {

        /// <summary>
        /// Beszerzi a legfrisebb térképet, szétparszolja, és elrakja
        /// ToDo: Csak akkor kéne elrakni, ha különbözik az előzőtől
        /// Ijen egy sor:
        /// INSERT INTO `x_world` VALUES (188644,8, 165,1,45770,'Mr G faluja',30778,'Mr G',0,'',3);
        ///                               247034,-75,92,1,78348,'Répa,Retek,Mogyró',14265,'Borbás Zsuzsa',0,'',41
        /// </summary>
        /// <param name="url">Innen töltike lefelé</param>
        /// <param name="Data">Ide kell elrakni</param>
        public static void Collect(string url, TraviData Data)
        {

            System.Net.WebClient Client = new System.Net.WebClient();
            Stream strm = Client.OpenRead(url);
            StreamReader sr = new StreamReader(strm);
            string line;
            string[] details;
            DateTime now = DateTime.Now;
            do
            {
                line = sr.ReadLine();
                if (line != null)
                {
                    line = line.Substring(30, line.Length - 32);
                    details = line.Split(',');
                    MapElement me = new MapElement();
                    me.TimeStamp = now;
                    if (details.Length > 11)
                        continue;
                    try
                    {
                        me.Id = int.Parse(details[0]);
                        me.X = int.Parse(details[1]);
                        me.Y = int.Parse(details[2]);
                        me.Tid = int.Parse(details[3]);
                        me.Vid = int.Parse(details[4]);
                        me.Village = details[5].Trim('\'');
                        me.Uid = int.Parse(details[6]);
                        me.Player = details[7].Trim('\'');
                        me.Aid = int.Parse(details[8]);
                        me.Alliance = details[9].Trim('\'');
                        me.Population = int.Parse(details[10]);

                    }
                    catch
                    {
                        if (Debugger.IsAttached)
                            throw;
                    }
                    Data.Map.Add(me);
                }

            }
            while (line != null);
            strm.Close();


        }

    }
}
