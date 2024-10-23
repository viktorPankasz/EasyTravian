using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using EasyTravian.Types;

namespace EasyTravian
{
    public partial class TravianBase
    {
        /// <summary>
        /// Megkeres és megépít egy épületet
        /// lehetne még rajta reszelni...
        /// </summary>
        public void Build()
        {
            Globals.Logger.Log("=====================================", LogType.ltReport);
            Globals.Logger.Log("Build...", LogType.ltDebug);
            foreach (VillageData village in Data.Villages.Values)
            {
                DemolishInVillage(village);

                village.CleanConstructions();
                foreach (Construction c in village.Constructing)
                    Globals.Logger.Log("constr> " + c.Name + ' ' + c.Level + ' ' + c.Ends, LogType.ltReport);
                Globals.Logger.Log(village.Props.Name, LogType.ltReport);
                Globals.Logger.Log("Stocks:" + village.Stock, LogType.ltDebug);

                List<Building> buildings;

                if (Data.Tribe == TribeType.Roman)
                {
                    if (village.NeedToBuildResource())
                    {
                        ChangeToVillage(village);
                        buildings = village.Resources.Values.ToList();
                        UpgradeBuilding(buildings, village);
                        if (!village.ResourceConstructionInProgress())
                            BuildBuilding(buildings, village);
                    }

                    if (village.NeedToBuildBuilding())
                    {
                        ChangeToVillage(village);
                        if (!NewBuilding(village))
                        {
                            buildings = village.Buildings.Values.ToList();
                            UpgradeBuilding(buildings, village);
                            if (!village.BuildingConstructionInProgress())
                                BuildBuilding(buildings, village);
                        }
                    }
                }
                else //nem római // plus?
                {
                    if ((village.NeedToBuildBuilding() || village.NeedToBuildResource()))
                    {
                        ChangeToVillage(village);
                        if (!NewBuilding(village))
                        {
                            buildings = village.Buildings.Values.Union(village.Resources.Values).ToList();
                            UpgradeBuilding(buildings, village);
                            if (!village.BuildingConstructionInProgress() && !village.ResourceConstructionInProgress())
                                BuildBuilding(buildings, village);
                        }
                    }
                }


            }
        }

        private void DemolishInVillage(VillageData village)
        {
            foreach (Building b in village.Buildings.Values)
            {
                if (b.Demolition)
                {
                    if (b.Level == 0)
                        b.Demolition = false;
                    else
                    {
                        DoDemolishBuilding(village, b);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// új épületek felvétele listára
        /// ez azért speciális, mert a helyük még üres
        /// ilyen esetben kapunk egy épületlistát és abból kell választani
        /// </summary>
        /// <param name="village"></param>
        /// <returns></returns>
        private bool NewBuilding(VillageData village)
        {
            Globals.Logger.Log("NewBuilding...", LogType.ltDebug);

            var news = from b in village.Buildings.Values
                       where b.Type != BuildingType.None
                          && b.Level == 0
                          && b.Target > 0
                       select b;

            int villageId = ActiveVillage;

            foreach (Building b in news.ToList())
            {
                Globals.Logger.Log("try: " + b.Name + '@' + b.Id, LogType.ltDebug);
                Navigate("build.php?id=" + b.BuildId);

                int id;

                switch (Data.TravianServerVersion)
                {
                    case TraviVersion.t30:
                    case TraviVersion.t35:
                        Regex rex1 = new Regex(@"dorf[1-2]\.php\?a=\d+&id=\d+&c=(.{6})");
                        MatchCollection matches1 = rex1.Matches(Globals.Web.Document.GetElementById("content").InnerHtml.Replace("&amp;", "&"));

                        //Regex rex2 = new Regex(@"\d+ \| \d+ \| \d+ \| \d+ \| \d+ \|  \d+\:\d+\:\d+");
                        Regex rex2 = new Regex(@"\d+ \| \d+ \| \d+ \| \d+ \| \d+ \| \d+:\d+:\d+");
                        MatchCollection matches2 = rex2.Matches(Globals.Web.Document.GetElementById("content").InnerText);

                        if (matches1.Count == matches2.Count)
                        {
                            for (int i = 0; i < matches1.Count; i++)
                            {
                                id = int.Parse(matches1[i].Value.Split('?')[1].Split('&')[0].Split('=')[1]);

                                // megvan az épület
                                if (id == ((int)b.Type) + 1)
                                {
                                    string[] sa = matches2[i].Value.Split('|');
                                    sa = sa[5].Split(':');
                                    b.BuildTime =
                                        TimeSpan.FromHours(int.Parse(sa[0])) +
                                        TimeSpan.FromMinutes(int.Parse(sa[1])) +
                                        TimeSpan.FromSeconds(int.Parse(sa[2]));

                                    Globals.Logger.Log("NewBuild: " + b.Name, LogType.ltReport);
                                    Globals.Logger.Log("Href: " + matches1[i].Value, LogType.ltDebug);
                                    Navigate(matches1[i].Value);
                                    b.Level++;
                                    b.BuildHref = "";
                                    b.NextLevelCost.Clear();
                                    AddToConstructionList(villageId, b);
                                    return true;
                                }

                            }
                        }
                        else
                        {
                            Regex rex = new Regex(@"dorf[1-2]\.php\?a=" + ((int)b.Type + 1).ToString() + "&id=" + b.BuildId + "&c=(.{6})");
                            MatchCollection matches = rex.Matches(Globals.Web.Document.GetElementById("content").InnerHtml.Replace("&amp;", "&"));
                            if (matches.Count > 0)
                            {
                                Globals.Logger.Log("NewBuild: " + b.Name, LogType.ltReport);
                                Globals.Logger.Log("Href: " + matches[0].Value, LogType.ltDebug);
                                Navigate(matches[0].Value);
                                b.Level++;
                                b.BuildHref = "";
                                b.NextLevelCost.Clear();
                                AddToConstructionList(villageId, b);
                                return true;
                            }
                        }
                        break;
                    case TraviVersion.t40:
                        // http://tx3.travian.hu/build.php?id=30&category=2
                        int category = TypeHelpers.GetBuildCategory(b.type);
                        if (category > 1)
                            Navigate("build.php?id=" + b.BuildId + "&category=" + category.ToString());

                        //dorf2.php?a=23&id=20&c=4699a7
                        Regex rexi = new Regex(@"dorf[1-2]\.php\?a=" + ((int)b.Type + 1).ToString() + "&id=" + b.BuildId + "&c=(.{6})");
                        MatchCollection matchesi = rexi.Matches(Globals.Web.Document.GetElementById("content").InnerHtml.Replace("&amp;", "&"));
                        if (matchesi.Count > 0)
                        {
                            Globals.Logger.Log("NewBuild: " + b.Name, LogType.ltReport);
                            Globals.Logger.Log("Href: " + matchesi[0].Value, LogType.ltDebug);
                            Navigate(matchesi[0].Value);
                            b.Level++;
                            b.BuildHref = "";
                            b.NextLevelCost.Clear();
                            AddToConstructionList(villageId, b);
                            return true;
                        }

                        break;
                        /*
                        HtmlElement div = Globals.Web.Document.GetElementById("build");

                        HtmlElement elem;
                        HtmlElement subelem;
                        HtmlElement subelem2;

                        elem = div.FirstChild; // h2

                        while (elem != null)
                        {
                            elem = elem.NextSibling; // build_desc
                            if (elem != null)
                            {
                                subelem = elem.FirstChild; // a
                                // return Travian.iPopup(23,4);
                                // string ss = subelem.GetAttribute("onclick"); // System.__ComObject
                                if (subelem != null)
                                {
                                    subelem = subelem.FirstChild; // img class="building big white g23"
                                    if ((subelem != null) && (string.Compare(subelem.TagName, "img", true) == 0))
                                    {
                                        string ss = subelem.GetAttribute("className");
                                        string[] sa = ss.Split(' ');
                                        if (sa.Length > 3)
                                            ss = sa[3].Replace('g', ' ').Trim();
                                        else
                                            ss = sa[2].Replace('g', ' ').Trim();

                                        id = int.Parse(ss);
                                        elem = elem.NextSibling; // contract
                                        if (id == ((int)b.Type) + 1)
                                        {
                                            subelem = elem.FirstChild; // contractText
                                            subelem = subelem.NextSibling; // contractCosts
                                            subelem2 = subelem.FirstChild; // showCosts
                                            subelem2 = subelem2.FirstChild; // span class="resources r1
                                            while (subelem2 != null)
                                            {
                                                if (string.Compare(subelem2.TagName, "span", true) == 0)
                                                    if (string.Compare(subelem2.GetAttribute("className"), "clocks", true) == 0)
                                                    {
                                                        ss = subelem2.InnerHtml;
                                                        sa = ss.Split('>'); // 0:03:30
                                                        sa = sa[1].Split(':');
                                                        b.BuildTime =
                                                            TimeSpan.FromHours(int.Parse(sa[0])) +
                                                            TimeSpan.FromMinutes(int.Parse(sa[1])) +
                                                            TimeSpan.FromSeconds(int.Parse(sa[2]));

                                                        // gombnyomás
                                                        subelem = subelem.NextSibling; // contractLink
                                                        subelem = subelem.FirstChild; // button
                                                        if (string.Compare(subelem.TagName, "button", true) == 0)
                                                            subelem.InvokeMember("Click"); // onclick

                                                        b.Level++;
                                                        b.BuildHref = "";
                                                        b.NextLevelCost.Clear();
                                                        // AddToConstructionList(villageId, b); // lol, ez miért volt kikommentezve? // mert később kiparszoljuk és bekerül
                                                        return true;
                                                    }
                                                subelem2 = subelem2.NextSibling;
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                            //
                            elem = elem.NextSibling; // clear
                            elem = elem.NextSibling; // hr
                            elem = elem.NextSibling; // h2
                        }
                        break;
                        */ 
                }
            }
            return false;
        }

        /// <summary>
        /// frissíti a need to build listát
        /// </summary>
        /// <param name="buildings"></param>
        /// <param name="village"></param>
        private void UpgradeBuilding(List<Building> buildings, VillageData village)
        {
            Globals.Logger.Log("Upgrade... ", LogType.ltDebug);

            //ezt itt azért, hogy frissek legyenek az épületszintek
            Navigate("dorf1.php");
            Navigate("dorf2.php");

            foreach (Building building in buildings)
                    ParseConstruction(building, false);
        }

        /// <summary>
        /// a need to build lista alapján kiértékeli, hogy mit építsen
        /// majd meghívja az építést
        /// </summary>
        /// <param name="buildings"></param>
        /// <param name="village"></param>
        private void BuildBuilding(List<Building> buildings, VillageData village)
        {
            bool done = false;
            //nyersanyagok közül választani kell
            //ebben a sorrendben kéne építeni
            var prodOrd =
                from p in village.Productions.Values
                orderby p.ActPercent - p.TargetPercent
                select p;

            foreach (Production prod in prodOrd)
                Globals.Logger.Log(prod.TypeName + ' ' + prod.Producing + ' ' + prod.TargetPercent + ' ' + prod.ActPercent, LogType.ltDebug);


            //végigskera, hogy van-e az olcsók között ilyen
            foreach (Production prod in prodOrd)
            {
                var nowBuild =
                    from b in buildings
                    where b.Producing == prod.Type
                    && b.Level < b.Target
                    && b.NextLevelCost < village.Stock
                    && b.BuildId < 19
                    orderby b.NextLevelCost.Sum
                    select b;

                if (nowBuild.Count() > 0)
                {
                    if (!done)
                    {
                        BuildIt(nowBuild.First());
                        done = true;
                    }
                }
            }

            //ha nyersanyagot nem kellet akkor hátha épületet
            if (!done)
            {
                var Buildables = 
                    from b in buildings
                    where b.BuildId > 18
                    && b.Level > 0
                    && b.Level < b.Target
                    && b.NextLevelCost < village.Stock
                    orderby b.NextLevelCost.Sum
                    select b;

                if (Buildables.Count() > 0)
                {
                    BuildIt(Buildables.First());
                    done = true;
                }
            }
        }

        /// <summary>
        /// egy épület megépítése
        /// </summary>
        /// <param name="b"></param>
        private void BuildIt(Building b)
        {
            Globals.Logger.Log( "Try Upgrade: " + b.Name + '@' + b.Id + ' ' + b.NextLevelCost, LogType.ltDebug);
            
            Navigate("build.php?id=" + b.BuildId);
            ParseConstruction(b, true);

            Regex rex = new Regex(@"dorf[1-2]\.php\?a=.*&c=(.{6})");
            MatchCollection matches = rex.Matches(Globals.Web.Document.GetElementById("content").InnerHtml.Replace("&amp;", "&"));
            if (matches.Count > 0)
                b.BuildHref = matches[matches.Count - 1].Value;

            if (b.BuildHref != null && b.BuildHref.Length != 0)
            {
                Globals.Logger.Log("Upgrade: " + b.BuildHref, LogType.ltDebug);
                Navigate(b.BuildHref);
                //b.Level++;
                b.BuildHref = "";
                b.NextLevelCost.Clear();
                //AddToConstructionList(b);
                Navigate("dorf1.php"); //hogy frissüljön a construction lista
            }
             
              
        }
        private void ClearConstructionList(int activeVillage)
        {
            if (Data.Villages.ContainsKey(activeVillage)) 
                Data.Villages[activeVillage].Constructing.Clear();
        }

        private void AddToConstructionList(int villageId, Building b)
        {
            Construction c = new Construction();
            c.Building = b.Type;
            c.Started = DateTime.Now;
            if (b.BuildTime == TimeSpan.Zero)
                c.Ends = c.Started.AddMinutes(2);
            else
                c.Ends = c.Started.Add(b.BuildTime);
            c.Level = b.Level;
            c.CancelUrl = "";
            Data.Villages[villageId].Constructing.Add(c);
        }

        public void AddToConstructionList(int activeVillage, string buildName, int level, DateTime endTime, string cancelUrl)
        {
            Construction c = new Construction();
            try
            {
                c.Building = GetBuildingTypeByBuildName(buildName);
            }
            catch { }
            c.Started = DateTime.Now;
            c.Ends = endTime;
            c.Level = level;
            c.CancelUrl = cancelUrl;
            Data.Villages[activeVillage].Constructing.Add(c);
        }

        internal void CheckTrials()
        {
            /*
            if (!Globals.Register.IsRegistered(TraviModule.Builder))
            {
                foreach(VillageData v in Data.Villages.Values)
                {
                    foreach (Building b in v.Buildings.Values.Union(v.Resources.Values))
                    {
                        if (b.Target > 1)
                            b.Target = 1;
                    }
                }
            }
             */ 
        }

        internal void CopyBuildingsFrom(VillageData from, VillageData to)
        {
            if (to.Props.Id != from.Props.Id)
            {
                // VillageData to = Data.Villages[ActiveVillageId];
                foreach (KeyValuePair<int, Building> b in from.Buildings)
                {
                    to.Buildings[b.Key].Type = b.Value.Type;
                    to.Buildings[b.Key].Target = b.Value.Level;
                }
            }
        }
        private BuildingType GetBuildingTypeByBuildName(string buildName)
        {
            return (BuildingType)Enum.Parse(typeof(BuildingType), Globals.Translator.DeTranslate(buildName));
        }

        internal void Demolish(Building building)
        {
            building.Demolition = !building.Demolition;
        }

        internal void AllResourcesTo10(VillageData village)
        {
            foreach (KeyValuePair<int, Building> b in village.Resources)
                b.Value.Target = 10;
        }

        /// <summary>
        /// épületet létét leellenőrizzük, és javítjuk az adatbázist, ha kell
        /// </summary>
        /// <param name="village"></param>
        /// <param name="building"></param>
        /// <returns></returns>
        private bool PrepareBuilding(VillageData village, Building building)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("dorf2.php?newdid=");
            sb.Append(village.Props.Id);
            Navigate(sb.ToString());
            HtmlElement vm = Globals.Web.Document.GetElementById("village_map");
            foreach (HtmlElement e in vm.GetElementsByTagName("img"))
            {
                try
                {
                    string eclass = e.GetAttribute("className");
                    string[] eclassarr = eclass.Split(' '); 

                    switch (Data.TravianServerVersion)
                    {
                        case TraviVersion.t30:
                        case TraviVersion.t35:
                            if (eclassarr.Length == 3)
                            {
                                int pid = int.Parse(eclassarr[1].Substring(1, eclassarr[1].Length - 1));
                                // class="building d11 iso" üres hely
                                if (building.Id == pid)
                                {
                                    if ((eclassarr[2] == "iso") && (building.Level != 0))
                                    {
                                        building.Level = 0;
                                        building.Demolition = false;
                                        return true;
                                    }
                                    return false;
                                }
                            }
                            break;
                        case TraviVersion.t40:
                            // "building iso" "building g8"
                            if (eclassarr.Length < 2)
                                continue;
                            if (eclassarr[1] == "iso") // üres hely
                                continue;
                            if (eclassarr.Length == 2)
                            {
                                int pid = 0;
                                int.TryParse(eclassarr[1].Substring(1, eclassarr[1].Length - 1), out pid);
                                if (pid == 0)
                                    continue;
                                if (building.BuildId == pid)
                                {
                                    // TODO ellenőrizni a tényleges szintet
                                    if ((eclassarr[1] == "iso") && (building.Level != 0))
                                    {
                                        building.Level = 0;
                                        building.Demolition = false;
                                        return true;
                                    }
                                    return false;
                                }
                            }
                            break;
                    }
                }
                catch
                {
                    if (Debugger.IsAttached)
                        throw;
                }
            }
            return false;
        }

        /// <summary>
        /// TODO T4
        /// </summary>
        /// <param name="village"></param>
        /// <param name="building"></param>
        internal void DoDemolishBuilding(VillageData village, Building building)
        {
            if (building.Demolition)
            {
                building.Target = 0;

                if (PrepareBuilding(village, building)) return;

                // Főépület keresése
                // http://s6.travian.hu/build.php?gid=15
                int gid = (Array.IndexOf(Enum.GetValues(typeof(BuildingType)), BuildingType.Main_building)) + 1;

                StringBuilder sb = new StringBuilder();
                sb.Append("build.php?newdid=");
                sb.Append(village.Props.Id);
                sb.Append("&gid=");
                sb.Append(gid.ToString());
                Navigate(sb.ToString());

                // bontás combobox; helyek: 19-40
                // <select name="abriss" class="dropdown">
                // <option value="19">19. Nagy raktár 20</option>
                // id('build')/form/select

                HtmlElement form = Globals.Web.Document.Forms[0];

                foreach (HtmlElement input in (form.GetElementsByTagName("select"))) // Globals.Web.Document
                {
                    if (string.Compare(input.Name, "abriss", true) == 0)
                    {
                        input.SetAttribute("value", building.BuildId.ToString());
                        Submit();
                        building.Level--;
                        break;
                    }
                    
                }

                Navigate("dorf1.php");  // mert ha ottmarad akkor a javascript frissiti az oldalt, az meg kerdez egyet

                // TODO: bontás beírása az undercontruction gridbe
            }
        }

    }
}
