using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using EasyTravian.Types;
using System.Drawing;

namespace EasyTravian
{
    public partial class TravianBase
    {
        /// <summary>
        /// Gyülekező parszolása
        /// </summary>
        private void ParseRally(int activeVillage)
        {
            return;
            // TODO bejövő támadásnál elszáll
            // TODO t42

            try
            {
                // TODO előtte ezt a switchet be kell nyomni (ha van)
                // <p class="switch">
				// <a href="build.php?id=39&k">Összes megjelenítése (23)</a></p>

                HtmlElement valami = Globals.Web.Document.GetElementsByTagName("h4")[0];
                RallySection rs = RallySection.Into;
                while (valami != null)
                {
                    if (string.Compare(valami.TagName, "h4", true) == 0)
                    {
                        foreach (int res in Enum.GetValues(typeof(RallySection)))
	                    {
                		    rs = (RallySection)res;
                            if (valami.InnerText.StartsWith(EnumHelpers.RallySection2Text(rs)))
                                break;
                        }
                    }
                    else if (string.Compare(valami.TagName, "TABLE", true) == 0)
                    {
                        if (string.Compare(valami.GetAttribute("className"), "troop_details", true) == 0)
                        {
                            Data.RallyPointsItems.Add(ParseUnitsTable(valami, rs));
                        }
                    }
                    valami = valami.NextSibling;
                }
            }
            catch
            {
                if (Debugger.IsAttached)
                    throw;
            }
        }

        /// <summary>
        /// gyülekező
        /// TODO T4
        /// </summary>
        /// <param name="unitsTable"></param>
        /// <param name="rallySection"></param>
        /// <returns></returns>
        public static ArmyItem ParseUnitsTable(HtmlElement unitsTable, RallySection rallySection)
        {
            ArmyItem RetVal = new ArmyItem(rallySection);

            string ArrivalSpan = "", ArrivalText = "";

            HtmlElement child = unitsTable.FirstChild;

            //vertical ingress
            while (child != null && child != unitsTable)
            {
                //Source village
                if (String.Compare(child.GetAttribute("className"), "role") == 0)
                {
                    HtmlElement a = child.FirstChild;
                    RetVal.SourceVillageName = a.InnerText;
                    RetVal.SourceVillageURL = a.GetAttribute("href");
                    RetVal.SourceVillageCoord = Helpers.CoordsFromRaw(
                                                    Helpers.GetURIParameter(RetVal.SourceVillageURL, "d"));
                }

                //Destination and movement type
                if (string.Compare(child.GetAttribute("colspan"), "10") == 0 &&
                  ((child.FirstChild != null) ? String.Compare(child.FirstChild.TagName, "A") == 0 : false))
                {
                    HtmlElement a = child.FirstChild;
                    RetVal.DestinationVillageName = "";

                    foreach (int res in Enum.GetValues(typeof(ArmyState)))
                    {
                        ArmyState ast = (ArmyState)res;
                        if (a.InnerText.StartsWith(EnumHelpers.ArmyState2Text(ast)))
                        {
                            RetVal.State = ast;
                            RetVal.DestinationVillageName =
                                a.InnerText.Substring(EnumHelpers.ArmyState2Text(ast).Length + 1, 
                                                      a.InnerText.Length - 
                                                      EnumHelpers.ArmyState2Text(ast).Length - 
                                                      EnumHelpers.ArmyState2TextPost(ast).Length - 2);
                            break;
                        }
                    }
                    RetVal.DestinationVillageURL = a.GetAttribute("href");
                    RetVal.DestinationVillageCoord = Helpers.CoordsFromRaw(
                                                        Helpers.GetURIParameter(RetVal.DestinationVillageURL, "d"));
                }

                //Travel time
                if (String.Compare(child.TagName.ToUpper(), "SPAN") == 0 && child.Id.StartsWith("timer"))
                {
                    ArrivalSpan = child.InnerText;
                }

                //Arrival time
                if (String.Compare(child.TagName.ToUpper(), "DIV") == 0 && String.Compare(child.GetAttribute("className"), "at") == 0)
                {
                    ArrivalText = child.InnerText.Substring(0, child.InnerText.IndexOf(' '));
                }

                if (child.FirstChild != null)
                {
                    child = child.FirstChild;
                }
                else
                {
                    if (child.NextSibling != null)
                    {
                        child = child.NextSibling;
                    }
                    else
                    {
                        while (((child != null) ? child.NextSibling : child) == null && child != unitsTable)
                            child = child.Parent;

                        if (child != unitsTable)
                            child = child.NextSibling;
                    }
                }
            }

            if (ArrivalSpan.Length > 0 && ArrivalText.Length > 0)
                RetVal.SetArrival(ArrivalSpan, ArrivalText);

            return RetVal;
        }

        internal void RefreshRallyPoints()
        {
            // ha így hívjuk, akkor newdid falu gyülekezőjébe kötünk ki, kibontva az össevonásokat
            // http://s6.travian.hu/build.php?newdid=89172&gid=16&id=39&j&k
            foreach (VillageData falu in Data.Villages.Values)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("build.php?newdid=");
                sb.Append(falu.Props.Id);
                sb.Append("&gid=16&id=39&j&k");
                Navigate(sb.ToString());
            }
        }

        public void SendTroops(int villageIdFrom, Point coord, int[] sereg, SendArmyAttackType sat)
        {
            // from
            // a2b.php? + getActiveVillage()
            // http://s6.travian.hu/a2b.php?newdid=22686

            // to
            // z = RawFromCoords(X, Y)
            // &z=355161
            // http://s6.travian.hu/a2b.php?newdid=22686&z=355161

            StringBuilder uri = new StringBuilder("http://s6.travian.hu/a2b.php?newdid=");
            uri.Append(villageIdFrom.ToString());
            uri.Append("&z=");
            uri.Append(Helpers.RawFromCoords(coord.X, coord.Y));
            Navigate(uri.ToString());
            
            // what
            // xpath: első egység"id('troops')/tbody/tr[1]/td[1]/span"
            // vagy bejárás xdommal
            // submit,submit

            HtmlElement input = null;
            bool vanMitKuldeni = false;

            try
            {
                HtmlElement tabla = Globals.Web.Document.GetElementById("troops");
                if (tabla != null)
                {
                    if (string.Compare(tabla.FirstChild.TagName, "tbody", true) == 0)
                        tabla = tabla.FirstChild;
                    int i = 0;
                    string db = "";

                    HtmlElement tr = tabla.FirstChild;
                    while (tr != null)
                    {
                        HtmlElement td = tr.FirstChild;
                        while (td != null)
                        {
                            foreach (HtmlElement item in td.Children)
	                        {
                                if (item != null)
                                {
                                    if (string.Compare(item.TagName, "img", true) == 0)
                                    {} else
                                    if (string.Compare(item.TagName, "input", true) == 0)
                                    {
                                        input = item;
                                    } else
                                    if (string.Compare(item.TagName, "a", true) == 0)
                                    {
                                        if ((sereg != null) && (sereg.Length >= i))
                                            db = sereg[i].ToString();
                                        else
                                            db = item.InnerText.Substring(1, item.InnerText.Length - 2);
                                    }
                                }
                            }
                            if ((!string.IsNullOrEmpty(db)) && (db != "0") && (input != null))
                            {
                                input.SetAttribute("value", db);
                                db = "";
                                input = null;
                                vanMitKuldeni = true;
                            }
                            i++;
                            td = td.NextSibling;
                        }
                        tr = tr.NextSibling;
                    }

                }
                tabla = Globals.Web.Document.GetElementById("coords");
                if (tabla != null)
                {
                    if (string.Compare(tabla.FirstChild.TagName, "tbody", true) == 0)
                        tabla = tabla.FirstChild;
                    input = null;
                    HtmlElement tr = tabla.FirstChild;
                    while (tr != null)
                    {
                        HtmlElement td = tr.FirstChild;
                        while (td != null)
                        {
                            HtmlElement valami = td.FirstChild;  // label
                            if (valami != null)
                            {
                                // <input type="radio" class="radio" name="c" value="2" checked />
                                valami = valami.FirstChild;  // input
                                if ((valami != null) &&
                                    (string.Compare(valami.GetAttribute("type"), "radio", true) == 0) &&
                                    (string.Compare(valami.GetAttribute("value"), "2", true) == 0)
                                   )
                                { 

                                }
                            }
                            td = td.NextSibling;
                        }
                        tr = tr.NextSibling;
                    }
                }

                if (vanMitKuldeni)
                {
                    Submit();
                    Submit();
                }
            }
            catch
            {
                if (Debugger.IsAttached)
                    throw;
            }
        }

        public void SaveTroopsAll(int villageIdFrom, Point coord)
        {
            SendTroops(villageIdFrom, coord, null, SendArmyAttackType.Reinforcement);
        }

    }
}
