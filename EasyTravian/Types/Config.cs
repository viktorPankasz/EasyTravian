using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using System.Diagnostics;
using System.Drawing;

namespace EasyTravian
{
    public class Config
    {
        public string Server = "Server";
        public string UserName = "UserName";
        [NonSerialized]
        [XmlIgnore()]
        public string PassWord = "Password";
        public int Language = 0;

        public int AutoBuildInterval = 1;
        public int AutoTradeInterval = 60;
        public int AutoFarmInterval = 60;

        public static DirectoryInfo DataBaseDir
        {
            get
            {
                DirectoryInfo di = new DirectoryInfo(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EasyTravian\\Data\\");
                if (!di.Exists)
                    di.Create();
                return di;
            }
        }

        public static Config LoadConfig()
        {
            string fileName = DataBaseDir + "config.xml";
            if (File.Exists(fileName))
            {
                using (TextReader r = new StreamReader(fileName)) 
                {
                    XmlSerializer ser = new XmlSerializer(typeof(Config));
                    try
                    {
                        return (Config)ser.Deserialize(r);
                    }
                    catch
                    {
                        return new Config();
                    }
                }
            }

            Config cfg = new Config();
            if (Globals.Translator.GetLanguages().Where(l => l.LCID == CultureInfo.CurrentCulture.LCID).Count() > 0)
                cfg.Language = CultureInfo.CurrentCulture.LCID;
            return cfg;
        }

        [XmlIgnore]
        private Dictionary<string, Color> ColorsList = new Dictionary<string, Color>();

        public Config()
        {
            ColorsList.Add("Demolish", Color.FromArgb(200, 50, 50));
            ColorsList.Add("Feltoles", Color.FromArgb(255, 243, 151));
            ColorsList.Add("Folozes", Color.FromArgb(150, 255, 217));
            ColorsList.Add("Inactive", Color.Gray);
            ColorsList.Add("Empty", Color.White); // FromArgb(120, 245, 207));
            ColorsList.Add("ReadOnly", Color.DarkGray);
            ColorsList.Add("Input", Color.FromArgb(251, 244, 147));
            ColorsList.Add("Lower", Color.FromArgb(251, 213, 153));
            ColorsList.Add("Higher", Color.FromArgb(214, 229, 254));
            ColorsList.Add("DefaultBackground", Color.White);
        }

        ~Config()
        {
            string fileName = DataBaseDir + "config.xml";
            using (TextWriter wri = new StreamWriter(fileName))
            {
                XmlSerializer ser = new XmlSerializer(typeof(Config));
                ser.Serialize(wri, this);
            }
        }

        public Color Colors(string key)
        {
            Color retval = ColorsList[key];
            if (retval == null)
                retval = ColorsList["DefaultBackground"];
            return retval;
        }

        public bool Debugging
        {
            get
            {
                #if DEBUG
                    return true;
                #else
                    return false;
                #endif
            }
        }


        //public Color Color[string colorName]
        //{
        //    get
        //    {
        //        return list[Type];
        //    }
        //    set
        //    {
        //        list[Type] = value;
        //    }
        //}

    }
}
