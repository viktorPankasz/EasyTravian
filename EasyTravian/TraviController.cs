using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
using EasyTravian.Types;

namespace EasyTravian
{
    /// <summary>
    /// controller
    /// képernyõt köti össze az üzleti klasszokkal
    /// TILOS kikerülni
    /// </summary>
    class TraviController
    {
        private MapPainter mapPainter = new MapPainter();

        /// <summary>
        /// Globál példány az adatokból
        /// </summary>
        private TravianBase TraviBase = new TravianBase();
        /// <summary>
        /// Ez az legutóbb megjelenített térkép középpontja
        /// </summary>
        private Point MapOrigin = new Point();
    
        /// <summary>
        /// Bindingsorce a bányákhoz
        /// </summary>
        public BindingSource bsResources = new BindingSource();
        /// <summary>
        /// Bindingsource a Termeléshez
        /// </summary>
        public BindingSource bsProductions = new BindingSource();
        /// <summary>
        /// BindingSource az épületekhez
        /// </summary>
        public BindingSource bsBuildings = new BindingSource();
        /// <summary>
        /// BindingSource a nyersanyag összesítõhöz
        /// </summary>
        public BindingSource bsResourceOverall = new BindingSource();
        /// <summary>
        /// Aktuális falu neve
        /// </summary>
        public int BuilderGUIActiveVillageId = 0;
        public BindingSource bsConstruction = new BindingSource();
        public BindingSource bsCanBuild = new BindingSource();

        /// <summary>
        /// Bindingsorce a piachoz
        /// </summary>
        public BindingSource bsTraderItems = new BindingSource();

        /// <summary>
        /// Gyülekezõk
        /// </summary>
        public BindingSource bsRallyPoints = new BindingSource();

        /// <summary>
        /// Farmlisták
        /// </summary>
        public BindingSource bsFarmLists = new BindingSource();

        public TraviController()
        {
            bsResources.CurrentChanged += new EventHandler(bsResources_CurrentChanged);
            bsProductions.CurrentChanged += new EventHandler(bsProductions_CurrentChanged);
            bsBuildings.CurrentChanged += new EventHandler(bsBuildings_CurrentChanged);

            bsResourceOverall.DataSource = TraviBase.Data.Overalls.ResourceOverall.Values;
        }
        

        void bsBuildings_CurrentChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void bsProductions_CurrentChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void bsResources_CurrentChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void Refresh( )
        {
            TraviBase.GetBasicInfo();
        }

        /// <summary>
        /// Visszaadja a falvak nevét
        /// </summary>
        /// <returns></returns>
        public string[] GetVillageNames()
        {
            string[] res = new string[TraviBase.Data.Villages.Count];

            var r = from v in TraviBase.Data.Villages.Values
                    select v.ToString(); // Props.Name;

            res = r.ToArray();

            //TraviBase.Data.Villages.Keys.CopyTo(res, 0);
            return res;
        }

        public VillageDataOutside AddVillageOutside(string _villageNameFull)
        {
            VillageDataOutside vdo = new VillageDataOutside();
            try
            {
                Point xy = new Point();
                xy.X = TraviBase.GetOriginByVillageName(_villageNameFull).X;
                xy.Y = TraviBase.GetOriginByVillageName(_villageNameFull).Y;
                int id = Convert.ToInt32(Helpers.RawFromCoords(xy));

                vdo.Props.Name = TraviBase.GetVillageNameByVillageNameFull(_villageNameFull);
                vdo.Props.Id = id;
                vdo.Props.Origin.X = xy.X;
                vdo.Props.Origin.Y = xy.Y;

                TraviBase.Data.VillagesOutside.Add(id, vdo);
            }
            catch
            {
                if (Debugger.IsAttached)
                    throw;
            }
            return vdo;
        }

        public void DeleteVillageOutside(VillageDataOutside _vdo)
        { 
            TraviBase.Data.VillagesOutside.Remove(_vdo.Id);
        }

        public VillageData[] GetVillages()
        {
            var l = from v in TraviBase.Data.Villages.Values
                    orderby v.ToString()
                    select v;

            return l.ToArray();

            //return TraviBase.Data.Villages.Values.ToArray();
        }

        public VillageDataOutside[] GetVillagesOutside()
        {
            var l = from v in TraviBase.Data.VillagesOutside.Values
                    orderby v.ToString()
                    select v;

            return l.ToArray();
        }


        public VillageData GetVillage(int VillageId)
        {
            return TraviBase.Data.Villages[VillageId];
        }

        /// <summary>
        /// Beállítja aktívnak a falut
        /// </summary>
        /// <param name="VillageName">Falunak a neve</param>
        public void SetActiveVillage( int VillageId )
        {
            RefreshBindings(VillageId);

            BuilderGUIActiveVillageId = VillageId;
        }

        private void RefreshBindings(int VillageId)
        {
            VillageData village = null;
            if (TraviBase.Data.Villages.TryGetValue(VillageId, out village))
            {
                bsResources.DataSource = village.Resources.Values;
                bsProductions.DataSource = village.Productions.Values;
                bsBuildings.DataSource = village.Buildings.Values;
                bsConstruction.DataSource = village.Constructing;
                bsCanBuild.DataSource = village.CanBuild;
            }
            bsResourceOverall.DataSource = null;
            bsResourceOverall.DataSource = TraviBase.Data.Overalls.ResourceOverall.Values;
            //bsResourceOverall.ResetBindings(false);
            bsConstruction.ResetBindings(false);
            bsBuildings.ResetBindings(false);
            bsResources.ResetBindings(false);
            bsResourceOverall.ResetBindings(false);
            bsCanBuild.ResetBindings(false);
            bsProductions.ResetBindings(false);

            bsTraderItems.DataSource = TraviBase.Data.TraderItems;
            bsTraderItems.ResetBindings(false);

            bsRallyPoints.DataSource = TraviBase.Data.RallyPointsItems;
            bsRallyPoints.ResetBindings(false);

            bsFarmLists.DataSource = TraviBase.Data.Farmlists;
            bsFarmLists.ResetBindings(false);
        }

        public void CheckTrials()
        {
            TraviBase.CheckTrials();
        }

        public void SetActiveVillageByName(string VillageName)
        {
            SetActiveVillage( TraviBase.GetVillageIdFromName(VillageName) );
        }

        /// <summary>
        /// Betölti az adatokat
        /// lehet, hogy nem kéne mindent.
        /// Térkép talán lehetne külön fileban is
        /// </summary>
        public bool Login()
        {
            try
            {
                TraviBase.Login();
                RefreshBindings(BuilderGUIActiveVillageId);
                return true;
            }
            catch 
            {
                if (Debugger.IsAttached)
                    throw;
                return false;
            }
        }

        /// <summary>
        /// Elmenti az adatokat
        /// lehet, hogy nem kéne mindent.
        /// Térkép talán lehetne külön fileban is
        /// </summary>
        public void SaveData()
        {
            TraviBase.Data.Save();
        }

        /// <summary>
        /// Kitalálja, hogy mit kell építeni és meg is építi
        /// </summary>
        public void Build()
        {
            try
            {
                TraviBase.Build();
            }
            catch 
            {
            }
            RefreshBindings(BuilderGUIActiveVillageId);
        }

        /// <summary>
        /// Szervertõl elkéri a legfrisebb térképet
        /// </summary>
        public void RefreshMap()
        {
            TraviBase.RefreshMap();
        }

        /// <summary>
        /// Kirajzolja az aktuális térképet
        /// </summary>
        /// <param name="graphics">Ide rajzol</param>
        /// <param name="origin">Központban lévõ falu koordinátái</param>
        /// <param name="zoom">zoom mértéke</param>
        public void DrawActMap( Graphics graphics, Point origin, MapPainterProps props )
        {
            if (BuilderGUIActiveVillageId == -1) return;

            Dictionary<Point, MapElement> map = TraviBase.ActMap();

            if (origin.X == 1000 && origin.Y == 1000)
            {
                if (MapOrigin.X == 0 && MapOrigin.Y == 0)
                    MapOrigin = TraviBase.Data.Villages[BuilderGUIActiveVillageId].Props.Origin;
            }
            else
                MapOrigin = origin;

            mapPainter.Paint(graphics, map, TraviBase.Data.Terrain, MapOrigin, props);
        }

        /// <summary>
        /// Megkeresi, hogy az egér mutató alatt melyik falu van
        /// </summary>
        /// <param name="size">Térkép mérete</param>
        /// <param name="point">egérpozició</param>
        /// <param name="zoom">zoom mértéke</param>
        /// <returns></returns>
        internal MapElement GetMapInfo(Size size, Point point, int zoom)
        {

            MapElement me = mapPainter.GetMapInfo(size, point, MapOrigin, TraviBase.ActMap(), zoom);
            if (me != null)
            {
                TerrainType tt = TerrainType.NA;
                TraviBase.Data.Terrain.TryGetValue(new Point(me.X, me.Y), out tt);
                if (tt != TerrainType.NA)
                    me.Terrain = tt.ToString();
            }

            return me;
        }

        /// <summary>
        /// Térképre kattintás
        /// Bepozícionálja a kattintott falut
        /// </summary>
        /// <param name="size">Térkép mérete</param>
        /// <param name="point">ide kattintottak</param>
        /// <param name="zoom">zoom mértéke</param>
        internal void MapClicked(Size size, Point point, int zoom)
        {
            MapElement me = mapPainter.GetMapInfo(size, point, MapOrigin, TraviBase.ActMap(), zoom);
            if (me != null)
                MapOrigin = new Point(me.X, me.Y);
        }

        internal void ReadMap()
        {
            TraviBase.ParseMap();
        }

        public List<string> ActiveClans()
        {
            return TraviBase.ActiveClans();
        }

        public void SendMail2CSRecipients(string recipients, string subject, string body)
        {
            TraviBase.SendMail(recipients, subject, body);
        }

        /*
        public void SendResource(string sourceVillageName, string destinationVillageName, int _lumber, int _clay, int _iron, int _crop)
        {
            TraviBase.SendResource(sourceVillageName, destinationVillageName, _lumber, _clay, _iron, _crop);
        }
        */

        public TraderItem AddSendResource()
        {
            return TraviBase.AddSendResource();
        }

        public void DoTrade(TraderItem ti)
        {
            TraviBase.DoTrade(ti);
        }

        public void DoTradeAll()
        {
            TraviBase.SendAllMerchant();
        }

        public bool PlusEnabled 
        { 
            get      
            {
                return TraviBase.PlusEnabled;
            }
        }

        /*
        public VillageData GetVillageFromName(string VillageName)
        { 
            return TraviBase.GetVillageFromName(VillageName);
        }
        */

        internal void CopyBuildingsFrom(VillageData from, VillageData to)
        {
            TraviBase.CopyBuildingsFrom(from, to);
        }

        internal void BuildingDemolition()
        {
            TraviBase.Demolish((Building)bsBuildings.Current);
        }

        internal void ResourceDemolition()
        {
            TraviBase.Demolish((Building)bsResources.Current);
        }

        internal void AllResourcesTo10()
        {
            TraviBase.AllResourcesTo10(GetVillage(BuilderGUIActiveVillageId));
        }

        internal void RefreshRallyPoints()
        {
            TraviBase.RefreshRallyPoints();
        }

        internal void SendTroopsAll(int villageIdFrom, Point coord)
        {
            TraviBase.SaveTroopsAll(villageIdFrom, coord);
        }

        public Troop[] GetTroops()
        {
            var l = from tr in TraviBase.Data.Troops.Values
                    where tr.Tribe == TraviBase.Data.Tribe
                    select tr;

            return l.ToArray();
        }

        public FarmList AddFarmlist()
        {
            return TraviBase.AddFarmlist();
        }

        public void DeleteFarmlist(FarmList _fl)
        {
            TraviBase.Data.Farmlists.Remove(_fl);
        }

        public FarmList[] GetFarmlist()
        {
            var l = from fl in TraviBase.Data.Farmlists
                    orderby fl.ToString()
                    select fl;

            return l.ToArray();
        }

        public void DoFarm(FarmList fl)
        {
            TraviBase.DoFarm(fl);
        }

        public void DoFarmAll()
        {
            TraviBase.DoFarmAll();
        }


        internal void DoTradeDown()
        {
            TraderItem ti = (TraderItem)bsTraderItems.Current;
            TraviBase.DoTradeItemDown(ti);
            bsTraderItems.Position = bsTraderItems.IndexOf(ti);
            bsTraderItems.ResetBindings(false);
        }

        public void DoTadeItemUp()
        {
            TraderItem ti = (TraderItem)bsTraderItems.Current;
            TraviBase.DoTradeItemUp(ti);
            bsTraderItems.Position = bsTraderItems.IndexOf(ti);
            bsTraderItems.ResetBindings(false);
        }

        public void DoFarmItemDown()
        {
            FarmList fl = (FarmList) bsFarmLists.Current;
            TraviBase.DoFarmItemDown(fl);
            bsFarmLists.Position = bsFarmLists.IndexOf(fl);
            bsFarmLists.ResetBindings(false);
        }

        public void DoFarmItemUp()
        {
            FarmList fl = (FarmList)bsFarmLists.Current;
            TraviBase.DoFarmItemUp(fl);
            bsFarmLists.Position = bsFarmLists.IndexOf(fl);
            bsFarmLists.ResetBindings(false);
        }

    }
}
