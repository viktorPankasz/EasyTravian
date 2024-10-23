using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Diagnostics;
using EasyTravian.Types;

namespace EasyTravian
{
    //b0y0
    public partial class TravianBase
    {
        /// <summary>
        /// Összeszedi az alap infókat egy birodalomról.
        /// Épületek és szintek, más nem
        /// </summary>
        public void GetBasicInfo()
        {
            //Data.Villages.Clear();
            // Navigate("dorf1.php");

            if (!LoggedIn)
                return;

            ParseTribe();
            if (Data.Tribe == null)
                ParseTribe();

            Navigate("dorf1.php");

            ParseVillages();

            if (Data.Villages.Count > 1)
            {
                foreach (VillageData village in Data.Villages.Values)
                {
                    ChangeToVillage(village);
                    Navigate("dorf1.php");
                    Navigate("dorf2.php");
                }
            }
            else
            {
                Navigate("dorf1.php");
                Navigate("dorf2.php");
            }

        }

        private void ParseTribe()
        {
            if (Data.Tribe == null)
            {
                try
                {
                    // spieler.php;

                    HtmlElement tribe = xpath.SelectElement(GetHTMLElement(TraviHTMLElementId.ProfilTribe));
                    if (tribe != null)
                    {
                        Data.Tribe = TribeType.Roman; // auuuuuuuu bazd, nem tudta kiolvasni? :)
                        switch (tribe.InnerText)
                        {
                            case "Római":
                                Data.Tribe = TribeType.Roman;
                                break;
                            case "Germán":
                                Data.Tribe = TribeType.Teuton;
                                break;
                            case "Gall":
                                Data.Tribe = TribeType.Gaul;
                                break;
                        }
                    }
                    else
                        Navigate("spieler.php");
                }
                catch (Exception)
                {
                    if (Debugger.IsAttached)
                        throw;
                }
            }
        }

        /// <summary>
        /// dorf2 parszolása
        /// épületek és szintjeik kimojolása
        /// </summary>
        private void ParseCentre(int activeVillage)
        {
            HtmlElement f = Globals.Web.Document.GetElementById("content");

            int n;

            //buildings
            switch (Data.TravianServerVersion)
            {
                case TraviVersion.t35:
                    f = f.GetElementsByTagName("div")[0];
                    foreach (HtmlElement e in f.GetElementsByTagName("img"))
                    {
                        try
                        {
                            string eclass = e.GetAttribute("className");
                            string[] eclassarr = eclass.Split(' ');

                            if (eclassarr.Length == 3)
                            {
                                int pid = int.Parse(eclassarr[1].Substring(1, eclassarr[1].Length - 1));
                                if (eclassarr[2] != "iso")
                                {
                                    // ha építés folyamatban, akkor mögé tesz egy "b" betűt
                                    // class="building d11 g10b"
                                    int bid;
                                    if (eclassarr[2].Substring(eclassarr[2].Length - 1, 1) == "b")
                                    {
                                        bid = int.Parse(eclassarr[2].Substring(1, eclassarr[2].Length - 2));
                                        Data.Villages[activeVillage].Buildings[pid].UnderConstruction = true;
                                    }
                                    else
                                        bid = int.Parse(eclassarr[2].Substring(1, eclassarr[2].Length - 1));
                                    Data.Villages[activeVillage].Buildings[pid].Type = (BuildingType)bid - 1;
                                }
                                else
                                {
                                    if (Data.Villages[activeVillage].Buildings[pid].Level != 0)
                                        Data.Villages[activeVillage].Buildings[pid].Type = BuildingType.None;
                                }
                            }
                            else
                            {
                                //gyulekezoter
                                if (eclassarr[0] == "dx1")
                                {
                                    if (eclassarr[1] != "iso")
                                    {
                                        //valamiért tesz egy e betűt a végére, ha nincs ott semmi talán akkor
                                        if (eclassarr[1].Substring(eclassarr[1].Length - 1, 1) == "e")
                                            Data.Villages[activeVillage].Buildings[21].Level = 0;
                                        else
                                            Data.Villages[activeVillage].Buildings[21].Level =
                                            int.Parse(eclassarr[1].Substring(1, eclassarr[1].Length - 1));
                                    }
                                    else
                                        Data.Villages[activeVillage].Buildings[21].Level = 0;
                                }
                            }
                        }
                        catch
                        {
                            if (Debugger.IsAttached)
                                throw;
                        }

                    }

                    //wall - ezt lehetne okosabban
                    HtmlElement wallEl = Globals.Web.Document.GetElementById("map1").GetElementsByTagName("area")[0];
                    //HtmlElement wallEl = f.GetElementsByTagName("map1")[0].GetElementsByTagName("area")[0];
                    Data.Villages[activeVillage].Buildings[22].Level = GetLevel(wallEl);

                    switch (Data.Tribe)
                    {
                        case TribeType.Roman:
                            Data.Villages[activeVillage].Buildings[22].Type = BuildingType.City_wall;
                            break;
                        case TribeType.Gaul:
                            Data.Villages[activeVillage].Buildings[22].Type = BuildingType.Palisade;
                            break;
                        case TribeType.Teuton:
                            Data.Villages[activeVillage].Buildings[22].Type = BuildingType.Earth_wall;
                            break;
                    }

                    //levels
                    n = 0;

                    foreach (HtmlElement e in Globals.Web.Document.GetElementById("levels").GetElementsByTagName("div"))
                    {
                        try
                        {
                            string classname = e.GetAttribute("className");

                            if (classname == "l39")  // Gyülekező
                                n = 21;
                            else if (classname == "l40")  // Fal
                                n = 22;
                            else
                                n = int.Parse(classname.Substring(1, classname.Length - 1));

                            Data.Villages[activeVillage].Buildings[n].Level = int.Parse(e.InnerText);
                        }
                        catch
                        {
                            if (Debugger.IsAttached)
                                throw;
                        }
                    }

                    break;
                case TraviVersion.t40:
                case TraviVersion.t42:
                    f = f.GetElementsByTagName("div")[0]; // village_map
                    
                    // clear
                    foreach (KeyValuePair<int, Building> b in Data.Villages[activeVillage].Buildings)
                    {
                        if (b.Value.Target == 0)
                            (b.Value).Type = BuildingType.None;
                        (b.Value).Level = 0;
                    }

                    int ii = 0;
                    try
                    {
                        foreach (HtmlElement e in f.GetElementsByTagName("img"))
                        {
                            ii++;
                            string elem = e.GetAttribute("className");
                            if (!elem.Contains(' ')) // Plus
                                continue;

                            string[] elemarr = elem.Split(' ');

                            if (ii < 23) // a fal 2 példányban van
                            {
                                Data.Villages[activeVillage].Buildings[ii].UnderConstruction = false;
                                if (elemarr[1] != "iso") // Építési terület
                                {
                                    int bid = 0;
                                    if (elemarr[1].Length > 4) // 4, mert UnderConstruction is lehet
                                    {
                                        // FAL
                                        // class="wall g32Top", class="wall g32Bottom"
                                        // germán: bid = 32;
                                        switch (Data.Tribe)
                                        {
                                            case TribeType.Roman:
                                                bid = (int)BuildingType.City_wall;
                                                break;
                                            case TribeType.Gaul:
                                                bid = (int)BuildingType.Palisade;
                                                break;
                                            case TribeType.Teuton:
                                                bid = (int)BuildingType.Earth_wall;
                                                break;
                                        }
                                    }
                                    else
                                        if (elemarr[1].Substring(elemarr[1].Length - 1, 1) == "b")
                                        {
                                            // ha építés folyamatban, akkor mögé tesz egy "b" betűt
                                            // class="building d11 g10b"
                                            bid = int.Parse(elemarr[1].Substring(1, elemarr[1].Length - 2));
                                            bid--;
                                            Data.Villages[activeVillage].Buildings[ii].UnderConstruction = true;
                                        }
                                        else
                                        {
                                            string bb = elemarr[1].Replace("g", "").Replace("e", "");
                                            //if (elemarr[1].Length == 3) // 3, mert "g16e" FAL basszameg
                                            //{
                                                bid = int.Parse(bb);
                                                bid--;
                                            //}
                                        }
                                    Data.Villages[activeVillage].Buildings[ii].Type = (BuildingType)bid;
                                }
                            }
                        }


                        // Épület szintek // levels
                        foreach (HtmlElement e in Globals.Web.Document.GetElementById("levels").GetElementsByTagName("div"))
                        {
                            try
                            {
                                string classname = e.GetAttribute("className"); // class="aid19"
                                classname = classname.Replace("aid", "");
                                // "29 underConstruction"
                                bool underConstruction = false;
                                if (classname.Contains(' '))
                                {
                                    string[] classname_arr = classname.Split(' ');
                                    classname = classname_arr[0];
                                    underConstruction = (classname_arr[1].ToUpper() == "underConstruction".ToUpper());
                                }
                                n = int.Parse(classname) - 18; // 18 bánya, mert 19-től kezdődik
                                int level = int.Parse(e.InnerText);
                                if (underConstruction)
                                    Data.Villages[activeVillage].Buildings[n].UnderConstruction = true;
                                if (Data.Villages[activeVillage].Buildings[n].UnderConstruction)
                                    level++;
                                Data.Villages[activeVillage].Buildings[n].Level = level;
                            }
                            catch
                            {
                                if (Debugger.IsAttached)
                                    throw;
                            }
                        }
                    }
                    catch
                    {
                        if (Debugger.IsAttached)
                            throw;
                    }
                    break;
            }
        }

        /// <summary>
        /// kimarja az épület szintjét
        /// egyelőre sajnos a hint-ből
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private int GetLevel(HtmlElement e)
        {
            string title = e.GetAttribute("title");
            string[] tt = title.Split(' ');
            int level;
            try
            {
                level = int.Parse(tt[tt.Length - 1]);
            }
            catch (Exception)
            {
                level = 0;
            }
            return level;
        }

        /// <summary>
        /// kiparszolja a profilban a falvakat
        /// hozzáadja az információkat a már meglévő listához
        /// </summary>
        private void ParseVillagesByProfil()
        {
            try
            {
                // profil linkje
                //HtmlElement profil = xpath.SelectElement("id('side_navi')/p[1]/a[3]");
                HtmlElement profil = xpath.SelectElement(GetHTMLElement(TraviHTMLElementId.ParseTribeProfilLink));
                if (profil != null)
                {
                    string href = profil.GetAttribute("href");
                    if (href != null)
                    {
                        //Navigate(href);
                        Navigate("spieler.php");

                        // HtmlElement tribe = xpath.SelectElement("id('content')/table[1]/tbody/tr[5]/td[2]");
                        HtmlElement div = xpath.SelectElement(GetHTMLElement(TraviHTMLElementId.ProfilVillages));
                        if (div == null)
                            return;
                        HtmlElement tr = div.FirstChild;
                        while (tr != null)
                        {
                            HtmlElement td = tr.FirstChild; // td name
                            HtmlElement elem = td.FirstChild; // <a href="karte.php?d=340006">01. Smallville</a> <span class="mainVillage">(Főfalu)</span>
                            string villageName = elem.InnerHtml;

                            td = td.NextSibling; // td oases
                            td = td.NextSibling; // td inhabitants
                            td = td.NextSibling; // td coords
                            //elem = td.FirstChild; // <div class="aligned_coords ">
                            elem = td.FirstChild; // <a href="karte.php?x=-19&amp;y=-24"><span class="cox">(-19</span><span class="pi">|</span><span class="coy">-24)</span></a>
                            string ss = elem.GetAttribute("href");
                            
                            ss = ss.Replace("&amp;", "&"); // karte.php?x=-19&y=-24
                            
                            string s2 = ss.Substring(ss.IndexOf("x=") + 2, ss.IndexOf("&") - ss.IndexOf("x=") - 2);
                            int xx = int.Parse(s2);
                            s2 = ss.Substring(ss.IndexOf("y=") + 2, ss.Length - ss.IndexOf("y=") - 2);
                            int yy = int.Parse(s2);

                            var res = from v in Data.Villages.Values
                                      where v.Props.Name == villageName
                                      select v;
                            // ez nem lehet több, mint 1, különben nem egyedi a falunév
                            if (res.Count() == 1)
                            {
                                VillageData vd = res.First();
                                vd.Props.Origin.X = xx;
                                vd.Props.Origin.Y = yy;
                            }
                            tr = tr.NextSibling;
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    throw;
            }
        }

        /// <summary>
        /// összenézi hány falu van a birodalomban
        /// </summary>
        private void ParseVillages()
        {
            //többfalus (Falvak:)
            // 3.1: if (xpath.ElementExists("id('lright1')/a/span"))
            // 3.5:  "id('vlist')/a"

            //if (xpath.ElementExists(GetHTMLElement(TraviHTMLElementId.VillageListId)))
            HtmlElement div = Globals.Web.Document.GetElementById(GetHTMLElement(TraviHTMLElementId.VillageListId));
            if (div != null)
            {
                switch (Data.TravianServerVersion)
                {
                    case TraviVersion.t30:
                    case TraviVersion.t35:
                        {
                            Regex rex = new Regex(@"newdid=\d+.*");
                            MatchCollection matches = rex.Matches(div.InnerHtml.Replace("&amp;", "&"));

                            Regex rexx = new Regex(@"class=cox>.*");
                            MatchCollection matchesx = rexx.Matches(div.InnerHtml.Replace("&amp;", "&"));

                            Regex rexy = new Regex(@"class=coy>.*");
                            MatchCollection matchesy = rexy.Matches(div.InnerHtml.Replace("&amp;", "&"));

                            for (int i = 0; i < matches.Count; i++)
                            {
                                string ss = matches[i].Value.Substring(matches[i].Value.IndexOf('=') + 1,
                                                                       matches[i].Value.IndexOf('\"') - matches[i].Value.IndexOf('=') - 1);
                                int id = int.Parse(ss);
                                int xx = int.Parse(matchesx[i].Value.Substring(11, matchesx[i].Value.IndexOf('<') - 11));
                                int yy = int.Parse(matchesy[i].Value.Substring(10, matchesy[i].Value.IndexOf(')') - 10));
                                if (!Data.Villages.ContainsKey(id))
                                {
                                    Data.Villages[id] = new VillageData();
                                    Data.Villages[id].Props.Id = id;
                                    Data.Villages[id].Props.Origin = new Point(xx, yy);
                                }
                                else
                                {
                                    Data.Villages[id].Props.Origin.X = xx;
                                    Data.Villages[id].Props.Origin.Y = yy;
                                }

                                // mert amikor 2. falu kész van...
                                if (Data.Villages.ContainsKey(0))
                                {
                                    //eh... át kéne másolni az adatokat
                                    Data.Villages.Remove(0);
                                }

                                Data.Villages[id].Props.url = "dorf1.php?newdid=" + Data.Villages[id].Props.Id;
                                Data.Villages[id].Props.Name = matches[i].Value.Substring(
                                    matches[i].Value.IndexOf('>') + 1,
                                    matches[i].Value.IndexOf('<') - matches[i].Value.IndexOf('>') - 1);

                            }
                        }
                        break;
                    case TraviVersion.t40:
                        {
                            div = div.FirstChild; // head
                            div = div.NextSibling; // list
                            div = div.FirstChild; // ul
                            div = div.FirstChild; // li
                            while (div != null)
                            {
                                HtmlElement aa = div.FirstChild;
                                string ss;
                                if (aa != null)
                                {
                                    string link = aa.GetAttribute("href"); // href="?newdid=103"
                                    //if (link.Contains("newdid"))
                                    ss = link.Substring(link.IndexOf("=") + 1, link.Length - link.IndexOf("=") - 1);
                                    int id = int.Parse(ss);

                                    // NINCS title! bazzz, a forrásban meg ott van!
                                    //string villageName = aa.GetAttribute("title"); // title="01. Smallville (-19|-24)"
                                    //Point origin = GetOriginByVillageName(villageName);

                                    if (!Data.Villages.ContainsKey(id))
                                    {
                                        Data.Villages[id] = new VillageData();
                                        Data.Villages[id].Props.Id = id;
                                        //Data.Villages[id].Props.Origin = new Point(origin.X, origin.Y);
                                    }

                                    Data.Villages[id].Props.url = link; // "dorf1.php" + link
                                    Data.Villages[id].Props.Name = aa.InnerHtml;

                                    // mert amikor 2. falu kész van...
                                    if (Data.Villages.ContainsKey(0))
                                    {
                                        //eh... át kéne másolni az adatokat
                                        Data.Villages.Remove(0);
                                    }
                                }
                                div = div.NextSibling;
                            }

                            // profil a koordináták és egyebek miatt
                            ParseVillagesByProfil();
                        }
                        break;
                    case TraviVersion.t42:
                        {
                            //<div id="sidebarBoxVillagelist" class="sidebarBox toggleable collapsed ">
                            // T42 active village
                            div = div.FirstChild; // sidebarBoxBaseBox
                            div = div.NextSibling; // sidebarBoxInnerBox
                            div = div.FirstChild; // innerBox header
                            div = div.NextSibling; // innerBox content
                            div = div.FirstChild; // ul
                            div = div.FirstChild; // li
                            while (div != null)
                            {
                                HtmlElement aa = div.FirstChild;
                                string ss;
                                if (aa != null)
                                {
                                    string link = aa.GetAttribute("href"); // href="?newdid=103"
                                    //if (link.Contains("newdid"))
                                    ss = link.Substring(link.IndexOf("=") + 1, link.Length - link.IndexOf("=") - 1);
                                    int id = int.Parse(ss);

                                    // NINCS title! bazzz, a forrásban meg ott van!
                                    //string villageName = aa.GetAttribute("title"); // title="01. Smallville (-19|-24)"
                                    //Point origin = GetOriginByVillageName(villageName);

                                    if (!Data.Villages.ContainsKey(id))
                                    {
                                        Data.Villages[id] = new VillageData();
                                        Data.Villages[id].Props.Id = id;
                                        //Data.Villages[id].Props.Origin = new Point(origin.X, origin.Y);
                                    }

                                    Data.Villages[id].Props.url = link; // "dorf1.php" + link
                                    string[] nameArray = aa.InnerText.Trim().Split('(');
                                    Data.Villages[id].Props.Name = nameArray[0];
                                    //Data.Villages[id].Props.Name = aa.InnerText.Trim();

                                    // mert amikor 2. falu kész van...
                                    if (Data.Villages.ContainsKey(0))
                                    {
                                        //eh... át kéne másolni az adatokat
                                        Data.Villages.Remove(0);
                                    }
                                }
                                div = div.NextSibling;
                            }

                            // profil a koordináták és egyebek miatt
                            ParseVillagesByProfil();
                        }
                        break;
                }

            }
            //egyfalus
            else
            {
                // 2.0: string name = xpath.SelectElement("id('lmid2')/div[1]/h1").InnerText;
                string name = xpath.SelectElement("id('content')/h1").InnerText;
                if (!Data.Villages.ContainsKey(0))
                {

                    Data.Villages[0] = new VillageData();
                    Data.Villages[0].Props.Name = name;
                    Navigate("karte.php");
                    Data.Villages[0].Props.Origin = ParseMapOrigin();
                }
            }

        }

        /// <summary>
        /// bányákat parszol
        /// </summary>
        /// <param name="VillageId"></param>
        private void ParseResources(int VillageId)
        {
            if (!Data.Villages.ContainsKey(VillageId))
                return;

                //HtmlElement f = Globals.Web.Document.GetElementById("content").GetElementsByTagName("div")[1];
            //if (f.InnerText.Trim() != "") //hűség szarság... <div id="dzp" class="b f7  c3">Hűség 57%</div>
            //    f = Globals.Web.Document.GetElementById("content").GetElementsByTagName("div")[2];

            //resource map
            HtmlElement f = Globals.Web.Document.GetElementById("village_map");
            // <div id="village_map" class="f11">
            string ss = f.GetAttribute("className");
            ss = ss.Substring(1, ss.Length - 1);
            int r = int.Parse(ss);
            for (int t = 1; t < 19; t++)
                Data.Villages[VillageId].Resources[t].Type = ResourceMaps.Map[r - 1][t - 1];

            bool[] x = {false, false, false, false, false, false, 
                   false, false, false, false, false, false,
                   false, false, false, false, false, false };

            //built level
            switch (Data.TravianServerVersion)
            {
                case TraviVersion.t35:
                    foreach (HtmlElement e in f.GetElementsByTagName("img"))
                    {
                        string eclass = e.GetAttribute("className");
                        string[] eclassarr = eclass.Split(' ');

                        if (eclassarr.Length > 1)
                        {
                            int id = int.Parse(eclassarr[1].Substring(2, eclassarr[1].Length - 2));
                            int lvl = int.Parse(eclassarr[2].Substring(5, eclassarr[2].Length - 5));

                            x[id - 1] = true;
                            if (lvl != Data.Villages[VillageId].Resources[id].Level)
                            {
                                Data.Villages[VillageId].Resources[id].Level = lvl;
                                Data.Villages[VillageId].Resources[id].NextLevelCost.Clear();
                            }
                        }
                    }
                    break;
                case TraviVersion.t40:
                case TraviVersion.t42:
                    f = Globals.Web.Document.GetElementById("content");
                    f = f.FirstChild; // map

                    int i = 0;
                    f = f.FirstChild; // <AREA href=\"build.php?id=1\" shape=circle coords=190,88,28 _extendedTip=\"true\">"
                    while (f != null)
                    {
                        ss = f.GetAttribute("href");
                        string[] sa = ss.Split('=');

                        if (sa.Length > 1)
                        {
                            int id = int.Parse(sa[1]);

                            HtmlElement f2 = Globals.Web.Document.GetElementById("village_map");
                            int j = 0;
                            f2 = f2.FirstChild;
                            while (f2 != null)
                            {
                                if (j == i)
                                {
                                    int lvl = 0;
                                    ss = f2.InnerText;
                                    int.TryParse(ss, out lvl);

                                    x[id - 1] = true;
                                    if (lvl != Data.Villages[VillageId].Resources[id].Level)
                                    {
                                        Data.Villages[VillageId].Resources[id].Level = lvl;
                                        Data.Villages[VillageId].Resources[id].NextLevelCost.Clear();
                                    }
                                    
                                    break;
                                }
                                f2 = f2.NextSibling;
                                j++;
                            }
                        }
                        i++;
                        f = f.NextSibling;
                    }
                    break;
            }

            for (int i = 0; i < x.Count(); i++)
            {
                if (!x[i])
                {
                    Data.Villages[VillageId].Resources[i + 1].Level = 0;
                    //Data.Villages[VillageId].Resources[i + 1].Target = 0;
                    Data.Villages[VillageId].Resources[i + 1].NextLevelCost.Clear();
                }
            }
        }

        /// <summary>
        /// termelést parszol
        /// </summary>
        /// <param name="VillageId"></param>
        private void ParseProduction(int VillageId)
        {
            int sum = 0;
            foreach (int res in Enum.GetValues(typeof(ResourcesType)))
            {
                int idx = res;
                //if (Data.TravianServerVersion == TraviVersion.t40) // + TraviVersion.t42
                
                //var e1 = TraviVersion.t40 | TraviVersion.t42;
                //if (e1.Contains((Data.TravianServerVersion))
                if ((TraviVersion.t40 | TraviVersion.t42).Contains((Data.TravianServerVersion)))
                    if (idx == 1) idx = 4;
                    else if (idx == 2) idx = 3;
                    else if (idx == 3) idx = 2;
                    else if (idx == 4) idx = 1;

                // TODO t40 t42
                sum += GetProduction(res, idx, VillageId);
            }

            if (Data.Villages.ContainsKey(VillageId)) 
                foreach (Production prod in Data.Villages[VillageId].Productions.Values)
                    if (prod.Producing != 0)
                        prod.ActPercent = prod.Producing * 100 / sum;
            
        }

        private int GetProduction(int res, int idx, int VillageId)
        {
            ResourcesType rt = (ResourcesType)res;

            HtmlElement el = xpath.SelectElement("id('l" + idx + "')"); // <SPAN id=l4 class=\"value \">2132/4000</SPAN>
            string[] s = el.InnerText.Split('/');

            if (!Data.Villages.ContainsKey(VillageId)) return 0;

            string ss = "";

            //if (Data.TravianServerVersion == TraviVersion.t40)
            if ((TraviVersion.t40 | TraviVersion.t42).Contains((Data.TravianServerVersion)))
            {
                el = el.Parent; // p
                el = el.Parent; // li
                // \r\n<LI class=r4 _extendedTip=\"true\"><P><IMG alt=Búza src=\"img/x.gif\"> <SPAN id=l4 class=\"value \">2132/4000</SPAN> </P>\r\n<DIV class=bar-bg>\r\n<DIV style=\"BACKGROUND-COLOR: #006900; WIDTH: 53%\" id=lbar4 class=bar></DIV></DIV>"
    
                ss = el.GetAttribute("title"); // TODO T4: title NINCS a DOMban
                if (ss != "")
                {
                    string[] s2 = ss.Split(':');
                    Data.Villages[VillageId].Productions[rt].Producing = int.Parse(s2[1]);
                }
            } 
            else
                Data.Villages[VillageId].Productions[rt].Producing = int.Parse(el.GetAttribute("title"));

            ss = s[0].Replace(".", "");
            Data.Villages[VillageId].Productions[rt].Stock = int.Parse(ss); // int.Parse(s[0])
            // TODO t42
            if (Data.TravianServerVersion != TraviVersion.t42)
                Data.Villages[VillageId].Productions[rt].Capacity = int.Parse(s[1]);
            return Data.Villages[VillageId].Productions[rt].Producing;
        }

        /// <summary>
        /// megnézi, hogy egy épület építhető-e és mennyibe kerül
        /// lehetne reszelni még rajta: pacik, katonák
        /// </summary>
        /// <param name="building"></param>
        private void ParseConstruction(Building building, bool Forced)
        {
            try
            {
                if (building.Type != BuildingType.None && building.Level < building.Target)
                {
                    if (Forced || building.NextLevelCost.Sum == 0)
                    {
                        Navigate("build.php?id=" + building.BuildId);

                        //build href
                        /*
                        building.BuildHref = "";
                        Regex rex = new Regex(@"dorf[1-2]\.php\?a=.*&c=(.{3})");
                        MatchCollection matches = rex.Matches(Globals.Web.Document.GetElementById("lmid2").InnerHtml.Replace("&amp;", "&"));
                        if (matches.Count > 0)
                            building.BuildHref = matches[matches.Count - 1].Value;
                        */

                        building.NextLevelCost.Clear();

                        string[] sa;

                        //cost
                        switch (Data.TravianServerVersion)
                        {
                            case TraviVersion.t30:
                            case TraviVersion.t35:
                                //Regex rex = new Regex(@"\d+ \| \d+ \| \d+ \| \d+ \| \d+ \|  \d+\:\d+\:\d+");
                                Regex rex = new Regex(@"\d+ \| \d+ \| \d+ \| \d+ \| \d+ \| \d+:\d+:\d+");
                                MatchCollection matches = rex.Matches(Globals.Web.Document.GetElementById("content").InnerText);
                                if (matches.Count > 0)
                                {
                                    sa = matches[matches.Count - 1].Value.Split('|');
                                    building.NextLevelCost[ResourcesType.Lumber] = int.Parse(sa[0]);
                                    building.NextLevelCost[ResourcesType.Clay] = int.Parse(sa[1]);
                                    building.NextLevelCost[ResourcesType.Iron] = int.Parse(sa[2]);
                                    building.NextLevelCost[ResourcesType.Crop] = int.Parse(sa[3]);

                                    sa = sa[5].Split(':');
                                    building.BuildTime =
                                        TimeSpan.FromHours(int.Parse(sa[0])) +
                                        TimeSpan.FromMinutes(int.Parse(sa[1])) +
                                        TimeSpan.FromSeconds(int.Parse(sa[2]));
                                }
                                break;
                            case TraviVersion.t40:
                            case TraviVersion.t42:
                                // id('contract')/div[2]/div
                                // showCosts
                                HtmlElement div = xpath.SelectElement("id('contract')/div[2]/div");

                                // fa
                                HtmlElement elem = div.FirstChild;
                                building.NextLevelCost[ResourcesType.Lumber] = GetBuildingCost(elem);
                                // agyag
                                elem = elem.NextSibling;
                                building.NextLevelCost[ResourcesType.Clay] = GetBuildingCost(elem);
                                // vas
                                elem = elem.NextSibling;
                                building.NextLevelCost[ResourcesType.Iron] = GetBuildingCost(elem);
                                // búza
                                elem = elem.NextSibling;
                                building.NextLevelCost[ResourcesType.Crop] = GetBuildingCost(elem);

                                elem = elem.NextSibling; // Élelemfelhasználás
                                elem = elem.NextSibling; // clear dummy

                                // clocks Időtartam
                                //elem = elem.NextSibling;
                                string ss = elem.InnerText; // InnerHtml
                                //sa = ss.Split('>');
                                //sa = sa[1].Split(':');
                                sa = ss.Split(':');
                                building.BuildTime =
                                    TimeSpan.FromHours(int.Parse(sa[0])) +
                                    TimeSpan.FromMinutes(int.Parse(sa[1])) +
                                    TimeSpan.FromSeconds(int.Parse(sa[2]));

                                // contractLink
                                // div = xpath.SelectElement("id('contract')/div[3]");

                                break;
                        }
                    }
                }
            }
            catch 
            {
                if (Debugger.IsAttached)
                    throw;
            }
        }

        private int GetBuildingCost(HtmlElement elem)
        {
            //string res;
            //string[] resa;
            int mennyiseg;
            /*
            res = elem.InnerHtml;
            resa = res.Split('>');
            mennyiseg = int.Parse(resa[1]);
            */
            mennyiseg = int.Parse(elem.InnerText);
            return mennyiseg;
        }

        private int GetActiveVillage()
        {
            HtmlElement div = Globals.Web.Document.GetElementById(GetHTMLElement(TraviHTMLElementId.VillageListId));
            if (div != null)
            {
                switch (Data.TravianServerVersion)
                {
                    case TraviVersion.t30:
                    case TraviVersion.t35:
                        // sárga pötty
                        // <tr><td class="dot hl"> // hl jelzi az aktív falut
                        // href="?newdid=22430"
                        // href="?newdid=159229&uid=7208

                        HtmlElement child = div.FirstChild;
                        while (child != null)
                        {
                            if (string.Compare(child.TagName, "TBODY") == 0)
                            {
                                HtmlElement child2 = child.FirstChild;  // TR-ek
                                while (child2 != null)
                                {
                                    if ((child2.FirstChild != null) && (child2.FirstChild.TagName == "TD"))
                                        if (string.Compare(child2.FirstChild.GetAttribute("className"), "dot hl") == 0)
                                            if (child2.FirstChild.NextSibling != null)                           // 2. TD
                                                if (child2.FirstChild.NextSibling.FirstChild != null)            // <A
                                                {
                                                    //<A href="?newdid=138639">5</A>
                                                    string ss = child2.FirstChild.NextSibling.FirstChild.InnerHtml;

                                                    int kezd = ss.IndexOf("newdid=") + 7;
                                                    int veg = ss.IndexOf("\"", kezd);

                                                    ss = ss.Substring(kezd, veg - kezd);

                                                    if (ss.IndexOf("&") > 0)
                                                    {
                                                        veg = ss.IndexOf("&");
                                                        ss = ss.Substring(0, veg);
                                                    }
                                                    //ss = child2.FirstChild.NextSibling.FirstChild.GetAttribute("href");
                                                    if (ss != "")
                                                    {
                                                        //ss = Helpers.GetURIParameter(ss, "newdid");
                                                        int ii = int.Parse(ss);
                                                        if (ii != activeVillage)
                                                            activeVillage = int.Parse(ss);
                                                    }
                                                    break;
                                                }
                                    child2 = child2.NextSibling;
                                }
                                break;
                            }
                            child = child.NextSibling;
                        }
                        break;
                    case TraviVersion.t40:
                        // T4 active village
                        div = div.FirstChild; // head
                        div = div.NextSibling; // list
                        div = div.FirstChild; // ul
                        div = div.FirstChild; // li
                        while (div != null)
                        {
                            // <a href="?newdid=40837"
                            HtmlElement aa = div.FirstChild;
                            string ss;
                            if (aa != null)
                            {
                                string link = aa.GetAttribute("href"); // href="?newdid=103"
                                //if (link.Contains("newdid"))
                                ss = link.Substring(link.IndexOf("=") + 1, link.Length - link.IndexOf("=") - 1);
                                int id = int.Parse(ss.Split('&')[0]); //ilyen is van néha: 94820&uid=2141

                                ss = aa.GetAttribute("className"); // <li class="entry active"
                                if (string.Compare(ss, "active", true) == 0)
                                {
                                    activeVillage = id;
                                    break;
                                }
                            }
                            div = div.NextSibling;
                        }
                        break;
                    case TraviVersion.t42:
                        //<div id="sidebarBoxVillagelist" class="sidebarBox toggleable collapsed ">
                        // T4 active village
                        div = div.FirstChild; // sidebarBoxBaseBox
                        div = div.NextSibling; // sidebarBoxInnerBox
                        div = div.FirstChild; // innerBox header
                        div = div.NextSibling; // innerBox content
                        div = div.FirstChild; // ul
                        div = div.FirstChild; // li
                        while (div != null)
                        {
                            // <a href="?newdid=40837" class="active">
                            HtmlElement aa = div.FirstChild;
                            string ss;
                            if (aa != null)
                            {
                                string link = aa.GetAttribute("href"); // href="?newdid=103"
                                //if (link.Contains("newdid"))
                                ss = link.Substring(link.IndexOf("=") + 1, link.Length - link.IndexOf("=") - 1);
                                int id = int.Parse(ss.Split('&')[0]); //ilyen is van néha: 94820&uid=2141

                                ss = aa.GetAttribute("className"); // <li class="entry active"
                                if (string.Compare(ss, "active", true) == 0)
                                {
                                    activeVillage = id;
                                    break;
                                }
                            }
                            div = div.NextSibling;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return activeVillage;
        }

        /// <summary>
        /// kiparszolja a folyamatban lévő építkezéseket az éppen aktív faluban
        /// </summary>
        private void ParseUnderContruction(int activeVillage)
        {
            try
            {
                ClearConstructionList(activeVillage);

                string buildName = "";
                int level = 0;
                DateTime endTime = DateTime.Now;
                string cancelUrl;

                HtmlElement bc = Globals.Web.Document.GetElementById("building_contract");
                if (bc != null)
                {
                    HtmlElement child = bc.FirstChild; // T40: thead
                    //if (Data.TravianServerVersion == TraviVersion.t40)
                    if ((TraviVersion.t40 | TraviVersion.t42).Contains((Data.TravianServerVersion)))
                        child = bc.FirstChild;
                    while (child != null)
                    {
                        if (string.Compare(child.TagName, "TBODY") == 0)
                        {
                            HtmlElement tr = child.FirstChild;  // TR-ek
                            while (tr != null)
                            {
                                if (tr.DomElement == null)
                                    break;
                                if ((tr.FirstChild != null) && (tr.FirstChild.TagName == "TD"))
                                {
                                    HtmlElement td = tr.FirstChild; // ico

                                    // href="?d=13145101&amp;a=0&amp;c=e00"
                                    cancelUrl = td.FirstChild.GetAttribute("href");

                                    // pl. Magtár (Szint 17)
                                    td = td.NextSibling;
                                    if (td != null)
                                    {
                                        string ss;
                                        string[] sa;
                                        //if (Data.TravianServerVersion == TraviVersion.t40)
                                        if ((TraviVersion.t40 | TraviVersion.t42).Contains((Data.TravianServerVersion)))
                                        {

                                            ss = td.InnerHtml.Trim(); // T4: "Főépület <span>Szint 7</span>"
                                            buildName = ss.Substring(0, ss.IndexOf("<") - 1);

                                            ss = ss.Substring(ss.IndexOf(">") + 1, ss.IndexOf("</") - ss.IndexOf(">") - 1);
                                            sa = ss.Split(' ');
                                            ss = sa[1];
                                        }
                                        else
                                        {
                                            ss = td.InnerText.Trim();
                                            buildName = ss.Substring(0, ss.IndexOf("(") - 1);

                                            ss = ss.Substring(ss.Length - 3, 2);
                                            ss = ss.Trim();
                                        }

                                        level = int.Parse(ss);

                                        // mennyi van vissza
                                        td = td.NextSibling;
                                        // <td><span id="timer1">2:13:29</span> óra</td>
                                        if (td != null)
                                        {
                                            if ((td.FirstChild != null) && (td.FirstChild.TagName == "SPAN"))
                                            { 
                                                ss = td.FirstChild.InnerText.Trim();
                                                sa = ss.Split(':');
                                                endTime = DateTime.Now +
                                                    (TimeSpan)(TimeSpan.FromHours(int.Parse(sa[0])) +
                                                               TimeSpan.FromMinutes(int.Parse(sa[1])) +
                                                               TimeSpan.FromSeconds(int.Parse(sa[2])));
                                            }

                                            // td = td.NextSibling;
                                            // Kész: 22:33 óra
                                            // if (td != null)
                                            // ss = ss.Substring(ss.IndexOf(":") + 2, 5); // HH:mm

                                            if (buildName != "")
                                                AddToConstructionList(activeVillage, buildName, level, endTime, cancelUrl);
                                        }
                                    }
                                }
                                tr = tr.NextSibling;
                            }
                            break;
                        }
                        child = child.NextSibling;
                    }
                }
            }
            catch
            {
                if (Debugger.IsAttached)
                    throw;
            }
        }

        private void GetTravianServerVersion()
        {
            //Data.ServerVersion =
            // <script....   
            // Travian.version = '4.0';
            // Travian.Game.version = '4.0';
            if (Data.TravianServerVersion != TraviVersion.tNone)
                return;

            // default T35
            Data.TravianServerVersion = TraviVersion.t35;

            // T4 vizsgálat
            try
            {
                HtmlElement head = Globals.Web.Document.GetElementsByTagName("head")[0];

                HtmlElementCollection scriptColl = head.GetElementsByTagName("script");
                if (scriptColl == null) return;

                HtmlElement scriptEl = scriptColl[1];
                if (scriptEl == null) return;

                string ss = scriptEl.InnerHtml;
                int ii = ss.IndexOf("Travian.Game.version");
                if (ii > 0)
                {
                    ii = ss.IndexOf("'", ii);
                    int jj = ss.IndexOf("'", ii + 1);
                    ss = ss.Substring(ii + 1, jj - ii - 1);
                    if (ss == "4.0")
                    {
                        Data.TravianServerVersion = TraviVersion.t40;
                        //return;
                    }
                }
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    throw;
            }

            // T4.2 vizsgálat
            // <link href="gpack/travian_Travian_4.2_Himmelsstuermer/lang/hu/compact.css?0a774" rel="stylesheet" type="text/css" /><link href="gpack/travian_Travian_4.2_Himmelsstuermer/lang/hu/lang.css?0a774" rel="stylesheet" type="text/css" /><link href="img/travian_basics.css" rel="stylesheet" type="text/css" />
            try
            {
                HtmlElement head = Globals.Web.Document.GetElementsByTagName("head")[0];

                HtmlElementCollection collection = head.GetElementsByTagName("link");
                if (collection == null) return;

                HtmlElement link1 = collection[1];
                if (link1 == null) return;

                string href = link1.GetAttribute("href");

                if (href.IndexOf("Travian_4.2") > 0)
                {
                    Data.TravianServerVersion = TraviVersion.t42;
                    return;
                }
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    throw;
            }
        }
    }
}
