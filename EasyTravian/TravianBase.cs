using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using EasyTravian.Types;

namespace EasyTravian
{
    /// <summary>
    /// Legfõbb üzleti osztály
    /// lehet, hogy szét is kéne darabolni
    /// </summary>
    public partial class TravianBase
    {
        private bool pageLoaded;
        public TraviData Data = new TraviData();
        public Dictionary<BuildingType, Dictionary<int, Resources>> BuildingCosts = BuildingCostsFill.Fill();

        private TraviHtmlPath tPath = new TraviHtmlPath();

        private int activeVillage;
        public int ActiveVillage
        {
            get { return GetActiveVillage(); }
        }

        bool tryToLogin = false;

        private bool plusEnabled = false; // 15 aranyos TravianPlus

        public bool PlusEnabled
        {
            get { return plusEnabled; }
            set { plusEnabled = value; }
        }

        public TravianBase()
        {
            Globals.Web.DocumentCompleted += Web_DocumentCompleted;
            Globals.Logger.MinLogType = LogType.ltDebug;
            Globals.Logger.Active = true;
        }

        /// <summary>
        /// Megvárja, míg a DocumentCompleted esemény lefut, azaz a böngészõ letölt mindent
        /// </summary>
        /// <param name="url"></param>
        public bool WaitForBrowser()
        { 
            //while (web.ReadyState == WebBrowserReadyState.Loading)
            Application.UseWaitCursor = true;
            try
            {
                DateTime kezd = DateTime.Now;
                while (!pageLoaded)
                {
                    Thread.Sleep(1);
                    Application.DoEvents();
                    if (DateTime.Now.Subtract(kezd).TotalSeconds > 20)
                        return false;
                }
                return true;
            }
            finally
            {
                Application.UseWaitCursor = false;
            }
        }

        private bool Navigate(VillageData village)
        {
            return Navigate(village.Props.url);
        }

        private bool Navigate(string url)
        {
            int n = 0;
            do
            {
                if (url != null && url.Length > 0)
                {
                    pageLoaded = false;
                    if (url == "back")
                        Globals.Web.GoBack();
                    else
                        if (url.StartsWith("http://"))
                            Globals.Web.Navigate(url);
                        else
                            Globals.Web.Navigate("http://" + Globals.Cfg.Server + "/" + url);
                    if (!WaitForBrowser())
                        Globals.Web.Stop();
                    else
                        return true;
                }
            } while (n++ < 3);

            return false;
        }

        private void Submit(int index)
        {
            if (Globals.Web.Document.Forms[index] != null)
            {
                pageLoaded = false;
                Globals.Web.Document.Forms[index].InvokeMember("Submit");
                WaitForBrowser();
            }
        }

        private void Submit()
        {
            Submit(0);
        }

        /// <summary>
        /// Többfalvas játéknál, kiválaszt egy falut
        /// </summary>
        /// <param name="village"></param>
        private bool ChangeToVillage(VillageData village)
        {
            if (village.Props.url == null)
                return Navigate("dorf1.php");
            else
                return Navigate(village.Props.url);
        }

        /// <summary>
        /// Login adatokat kitölti és megnyomja a gombot
        /// akkor kerül ide a program, ha a navigálás eredményeként a loginablak jön be
        /// </summary>
        void DoLogin()
        {
            //if (xpath.SetAttribute("id('login_form')/tbody/tr[1]/td/input", "value", Globals.Cfg.UserName)
            if (xpath.SetAttribute(GetHTMLElement(TraviHTMLElementId.LoginUserNameInput), "value", Globals.Cfg.UserName)
                &&
               //xpath.SetAttribute("id('login_form')/tbody/tr[2]/td/input", "value", Globals.Cfg.PassWord))
                xpath.SetAttribute(GetHTMLElement(TraviHTMLElementId.LoginPasswordInput), "value", Globals.Cfg.PassWord))
            {
                HtmlElement el;

                // T4 lowres
                // Lassabb internetelérésre optimalizált verzió (Megjegyzés: A térképen nem elérhető minden funkció)
                // id('login_form')/x:tbody/x:tr[3]/x:td/x:input
                switch (Data.TravianServerVersion)
                {
                    case TraviVersion.t40:
                    case TraviVersion.t42:
                        {
                            //xpath.SetAttribute("id('login_form')/x:tbody/x:tr[3]/x:td/x:input", "value", "1");
                            el = xpath.SelectElement(GetHTMLElement(TraviHTMLElementId.SlowNetworkCheckBox));
                            string v = el.GetAttribute("Value");
                            if (v != "1")
                                el.InvokeMember("Click");
                        }
                        break;
                    default:
                        break;
                }
                
                // HtmlElement el = xpath.SelectElement("id('btn_login')");
                el = xpath.SelectElement(GetHTMLElement(TraviHTMLElementId.LoginButton));
                el.InvokeMember("Click");

                try
                {
                    ETService.Service1 auth = new EasyTravian.ETService.Service1();
                    auth.AuthorizeCompleted += new EasyTravian.ETService.AuthorizeCompletedEventHandler(auth_AuthorizeCompleted);
                    auth.AuthorizeAsync(Globals.Cfg.UserName, Globals.Cfg.Server);
                }
                catch 
                {
                    if (Debugger.IsAttached)
                        throw;
                }
            }

        }

        void auth_AuthorizeCompleted(object sender, EasyTravian.ETService.AuthorizeCompletedEventArgs e)
        {
            //itt kell majd kikúrni, akinek nincs joga
        }

        public void Login()
        {
            if (Data.Villages.Count > 0)
                Data.Save();
            Data = new TraviData();
            Data.Load();
            //if (TraviBase.Data.Villages.Count == 0)
            try
            {
                Navigate("login.php?del_cookie");
                // TODO TESZT COMMENT
                GetBasicInfo();
                DataPrepare();
            }
            catch
            {
                if (Debugger.IsAttached)
                    throw;
                MessageBox.Show(Globals.Translator["Login failed!"]);
                Application.Exit();
            }

        }

        private void DataPrepare()
        {
            foreach (FarmList farmlist in Data.Farmlists)
            {
                farmlist.Village = Data.Villages[farmlist.VillageId];
            }
        }

        public bool LoggedIn
        {
            get
            {
                HtmlElement el = xpath.SelectElement(GetHTMLElement(TraviHTMLElementId.LoginPasswordInput));
                if (el == null)
                    return false;
                if ((string.Compare(el.Name, "password", true) == 0))
                    return false;
                return true;
                //return !xpath.ElementExists(GetHTMLElement(TraviHTMLElementId.LoginUserNameInput));
            }
        }

        /// <summary>
        /// webbrowser component eventje
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Web_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            GetTravianServerVersion(); // TODO szerver verzió kiírás valahova

            //if (xpath.ElementExists("id('login_form')/tbody/tr[1]/td/input"))
            if (!LoggedIn)
            {
                if (tryToLogin)
                {
                    //Globals.Web.Stop();
                    pageLoaded = true;
                    throw new Exception("cancel");
                }
                DoLogin();
                Thread.Sleep(5);
                tryToLogin = true;
            }
            else
            {
                pageLoaded = true;
                tryToLogin = false;

                // TODO TESZT COMMENT
                ParseAllInfo();
            }
        }

        private void ParseAllInfo()
        {
            int activeVillage = ActiveVillage;
            if (activeVillage == 0) return;

            //ParseTribe();

            ParseProduction(activeVillage);

            switch (Globals.Web.Url.AbsolutePath)
            {
                case "/dorf1.php":
                    ParseUnderContruction(activeVillage);
                    ParseResources(activeVillage);
                    break;
                case "/dorf2.php":
                    ParseUnderContruction(activeVillage);
                    ParseCentre(activeVillage);
                    break;
                case "/build.php":
                    // gyülekezõ
                    if (Helpers.GetURIParameter(Globals.Web.Url.AbsoluteUri, "id") == "39")
                        ParseRally(activeVillage);
                    break;
                default:
                    break;
            }
        }
        
        public int GetVillageIdFromName(string VillageName)
        {
            var res = from v in Data.Villages.Values
                      where v.ToString() == VillageName // Props.Name
                      select v.Props.Id;

            if (res.Count() > 0)
                return res.First();
            else
                return -1;
        }

        /*
        public VillageData GetVillageFromName(string VillageName)
        {
            var res = from v in Data.Villages.Values
                      where v.Props.Name == VillageName
                      select v;
            if (res.Count() > 0)
                return res.First();
            else
                return null;
        }
        */

        public string GetVillageNameByVillageData(CustomVillageData villageData)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(villageData.Props.Name);
            sb.Append(" (");
            sb.Append(villageData.Props.Origin.X);
            sb.Append("|");
            sb.Append(villageData.Props.Origin.Y);
            sb.Append(")");
            return sb.ToString();
        }

        public string GetVillageNameByVillageNameFull(string villageName)
        {
            // STORM (-79|-56)
            return villageName.Substring(0, villageName.IndexOf('(') - 1);
        }

        public Point GetOriginByVillageName(string villageName)
        {
            // STORM (-79|-56)
            Point pt = new Point();
            string coord = villageName.Substring(villageName.IndexOf('('), villageName.Length - villageName.IndexOf('('));
            string xstr = coord.Substring(1, coord.IndexOf('|') - 1);
            string ystr = coord.Substring(coord.IndexOf('|') + 1, coord.IndexOf(')') - coord.IndexOf('|') - 1);
            pt.X = Convert.ToInt32(xstr);
            pt.Y = Convert.ToInt32(ystr);
            return pt;
        }

        public VillageData GetVillageByOrigin(Point origin)
        {
            var res = from v in Data.Villages.Values
                      where v.Props.Origin.X == origin.X && v.Props.Origin.Y == origin.Y
                      select v;
            if (res.Count() > 0)
                return res.First();
            else
                return null;
        }

        public VillageData GetVillageByVillageName(string villageName)
        {
            Point pt = GetOriginByVillageName(villageName);
            return GetVillageByOrigin(pt);

        }

        public void DoCelebrate()
        {
            // http://s6.travian.hu/build.php?gid=24
        }

        public string GetHTMLElement(TraviHTMLElementId elementId)
        {
            return tPath.GetHTMLElement(Data.TravianServerVersion, elementId);
        }
            
    }

}
