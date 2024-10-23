using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace EasyTravian.Types
{
    public class FarmList
    {
        //private int id = 0;
        //private string xPathAll;

        public bool Active { get; set; }
        public int VillageId { get; set; }
        //public int Id { get { return id; } }
        public int Id { get; set; }
        public string FarmlistName { get; set; }

        // id('raidListMarkAll158')             // T4.2012.01
        [XmlIgnore()]
        public string XPathAll 
        { 
            get
            {
                //return xPathAll;
                return string.Format("id('raidListMarkAll{0}')", Id);
            }
            //set 
            //{
            //    xPathAll = value;
            //    int pos = xPathAll.IndexOf("raidListMarkAll") + "raidListMarkAll".Length;
            //    string idstr = xPathAll.Substring(pos, xPathAll.Length - pos);
            //    idstr = idstr.Substring(0, idstr.IndexOf("'"));
            //    id = int.Parse(idstr);
            //}
        } 

        // id('list158')/x:form/x:div[2]/x:button
        [XmlIgnore()]
        public string XPathGo 
        { 
            get 
            {
                return string.Format("id('list{0}')/x:form/x:div[2]/x:button", Id);
            } 
        } 

        [NonSerialized]
        [XmlIgnore()]
        public VillageData Village = new VillageData();

        public override string ToString()
        {
            return Village.Nev + " - " + FarmlistName;
        }
    }
}
