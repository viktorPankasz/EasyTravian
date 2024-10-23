using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Diagnostics;
using EasyTravian.Types;

namespace EasyTravian
{
    public partial class TravianBase
    {
        private void CheckMerchants(VillageData village)
        {
            // faluváltás, ha nem jó helyen van
            if (ActiveVillage != village.Props.Id)
                ChangeToVillage(village);
            // innan már piacon van
            Navigate("build.php?gid=17&t=5"); // T4 2012.4.11-től fülek
            village.MerchantCapacity = GetMerchantCapacity();
            // ez ugrik a dorf3-ba
            village.FreeMerchant = GetFreeMerchant(village.Props.Id);
        }

        /// <summary>
        /// Nyersanyag küldés az aktuális faluból x,y, koordinátára
        /// </summary>
        private void SendResource(int _lumber, int _clay, int _iron, int _crop, int _x, int _y)
        {
            if (_lumber == 0 && _clay == 0 && _iron == 0 && _crop == 0)
                return;
            try
            {
                // belépés a piacra már a koordinátákkal
                // http://s6.travian.hu/build.php?gid=17&x=-83&y=-42
                StringBuilder sb = new StringBuilder();
                sb.Append("build.php?gid=17&t=5");
                sb.Append("&x=");
                sb.Append(_x.ToString());
                sb.Append("&y=");
                sb.Append(_y.ToString());
                Navigate(sb.ToString());

                // Fa:    id('r1')
                if (_lumber != 0)
                    if (!xpath.SetAttribute("id('r1')", "value", _lumber.ToString()))
                        return;
                if (_clay != 0)
                    if (!xpath.SetAttribute("id('r2')", "value", _clay.ToString()))
                        return;
                if (_iron != 0)
                    if (!xpath.SetAttribute("id('r3')", "value", _iron.ToString()))
                        return;
                if (_crop != 0)
                    if (!xpath.SetAttribute("id('r4')", "value", _crop.ToString()))
                        return;

                // TravianPlus esetén az OK gomb előtt van egy checkbox (Kétszer tegye meg az utat)
                // egyenlőre ezt átugorjuk
                // id('lmid2')/form/p[1]/input[1]

                // T3.5 T4
                //Submit();
                // T4 2012.4.11. xpath: id('enabledButton')
                HtmlElement elem = xpath.SelectElement("id('enabledButton')");
                if (elem != null)
                    elem.InvokeMember("Click");
                Thread.Sleep(1000);

                // confirm
                // Submit();
                // T4 2012.4.11. xpath: id('enabledButton')
                elem = xpath.SelectElement("id('enabledButton')");
                if (elem != null)
                    elem.InvokeMember("Click");
                // TODO NEM JÓÓÓÓÓ
/*
                elem = xpath.SelectElement("id('enabledButton')");
                if (elem != null)
                    elem.InvokeMember("Click");
*/
                //Globals.Web.Document.in InvokeScript("marketPlace.sendRessources");


            }
            catch
            {
                if (Debugger.IsAttached)
                    throw;
            }
        }

        /// <summary>
        /// Maximum mennyit küldhet az adott anyagból, az össz nyersi arányában a kereskedő kapacitást figyelembe véve...
        /// </summary>
        /// <param name="nyersiMennyiseg">max anyag ami mehetne</param>
        /// <param name="fullNyersi">összesen</param>
        /// <param name="fullMerchant">össz szabad kereskedő</param>
        /// <returns></returns>
        private int GetMaxCanBeSendByFullMerchant(int nyersiMennyiseg, int fullNyersi, int fullMerchant)
        {
            int result = nyersiMennyiseg;
            double percent = (nyersiMennyiseg * 100) / fullNyersi;
            int tmp = Convert.ToInt32(Helpers.Round((double)fullMerchant / 100 * percent));
            if (tmp < nyersiMennyiseg)
                result = tmp;
            return result;
        }

        private int GetMaxCanBeSendByFreeCapacity(int nyersiMennyiseg, int freeCapacity)
        {
            if (nyersiMennyiseg > freeCapacity)
                return freeCapacity;
            else
                return nyersiMennyiseg;
        }


        public void DoTrade(TraderItem ti)
        {
            int _lumber = 0;
            int _clay = 0;
            int _iron = 0;
            int _crop = 0;

            CustomVillageData v_to = null;
            if (Data.Villages.ContainsKey(ti.DestinationVillageId))
            {
                v_to = Data.Villages[ti.DestinationVillageId];
                ti.Outside = false;
            }
            else
            {
                v_to = Data.VillagesOutside[ti.DestinationVillageId];
                ti.Outside = true;
                ti.Type = TraderType.Fölözés;
                ti.MaxDestinationCapacity = false;
            }
            VillageData v_from = Data.Villages[ti.SourceVillageId];

            switch (ti.Type)
            {
                case TraderType.Feltöltés:
                    // Meg kell nézni a célfaluban melyik anyagból mennyi hiányzik
                    if (ti.ResourcesTypeLumber && (v_to.Productions[ResourcesType.Lumber].FullnessPercent < ti.Value))
                    {
                        _lumber = (int)Math.Floor((double)v_to.Productions[ResourcesType.Lumber].Capacity / 100 * ti.Value) - v_to.Productions[ResourcesType.Lumber].Stock;
                        if (_lumber > v_from.Productions[ResourcesType.Lumber].Stock)
                            _lumber = v_from.Productions[ResourcesType.Lumber].Stock;
                    }
                    if (ti.ResourcesTypeClay && (v_to.Productions[ResourcesType.Clay].FullnessPercent < ti.Value))
                    {
                        _clay = (int)Math.Floor((double)v_to.Productions[ResourcesType.Clay].Capacity / 100 * ti.Value) - v_to.Productions[ResourcesType.Clay].Stock;
                        if (_clay > v_from.Productions[ResourcesType.Clay].Stock)
                            _clay = v_from.Productions[ResourcesType.Clay].Stock;
                    }
                    if (ti.ResourcesTypeIron && (v_to.Productions[ResourcesType.Iron].FullnessPercent < ti.Value))
                    {
                        _iron = (int)Math.Floor((double)v_to.Productions[ResourcesType.Iron].Capacity / 100 * ti.Value) - v_to.Productions[ResourcesType.Iron].Stock;
                        if (_iron > v_from.Productions[ResourcesType.Iron].Stock)
                            _iron = v_from.Productions[ResourcesType.Iron].Stock;
                    }
                    if (ti.ResourcesTypeCrop && (v_to.Productions[ResourcesType.Crop].FullnessPercent < ti.Value))
                    {
                        _crop = (int)Math.Floor((double)v_to.Productions[ResourcesType.Crop].Capacity / 100 * ti.Value) - v_to.Productions[ResourcesType.Crop].Stock;
                        if (_crop > v_from.Productions[ResourcesType.Crop].Stock)
                            _crop = v_from.Productions[ResourcesType.Crop].Stock;
                    }

                    break;
                case TraderType.Fölözés:
                    // mennyit kell átküldeni?
                    if (ti.ResourcesTypeLumber && (v_from.Productions[ResourcesType.Lumber].FullnessPercent > ti.Value))
                        _lumber = v_from.Productions[ResourcesType.Lumber].Stock -
                                  Convert.ToInt32(Helpers.Round((double)v_from.Productions[ResourcesType.Lumber].Capacity / 100 * ti.Value));
                    if (ti.ResourcesTypeClay && (v_from.Productions[ResourcesType.Clay].FullnessPercent > ti.Value))
                        _clay = v_from.Productions[ResourcesType.Clay].Stock -
                                Convert.ToInt32(Helpers.Round((double)v_from.Productions[ResourcesType.Clay].Capacity / 100 * ti.Value));
                    if (ti.ResourcesTypeIron && (v_from.Productions[ResourcesType.Iron].FullnessPercent > ti.Value))
                        _iron = v_from.Productions[ResourcesType.Iron].Stock -
                                  Convert.ToInt32(Helpers.Round((double)v_from.Productions[ResourcesType.Iron].Capacity / 100 * ti.Value));
                    if (ti.ResourcesTypeCrop && (v_from.Productions[ResourcesType.Crop].FullnessPercent > ti.Value))
                        _crop = v_from.Productions[ResourcesType.Crop].Stock -
                                  Convert.ToInt32(Helpers.Round((double)v_from.Productions[ResourcesType.Crop].Capacity / 100 * ti.Value));
                    break;
                default:
                    break;
            }


            // össze kell adni, hogy a bepipált anyagokból mennyit kell összesen küldeni
            // ezeknek a százalékos eloszlását ki kell számolni
            // kerekedő kapacitást kell szummázni (kereskedő kapacitás * szabad kereskedők száma)
            // a fenti aránnyal súlyozni kell a max. kereskedő kapcitást
            // TODO: allowfullmerchantonly
            // TODO: max. a cél tár kapacitásáig (?)

            int osszesen = _lumber + _clay + _crop + _iron;
            if (osszesen != 0)
            {
                CheckMerchants(v_from);
                int fullmerchant = v_from.FreeMerchant * v_from.MerchantCapacity;
                if (fullmerchant != 0)
                {
                    if (_lumber != 0)
                    {
                        _lumber = GetMaxCanBeSendByFullMerchant(_lumber, osszesen, fullmerchant);
                        if (ti.MaxDestinationCapacity)
                            _lumber = GetMaxCanBeSendByFreeCapacity(_lumber, v_to.Productions[ResourcesType.Lumber].FreeCapacity);
                    }
                    if (_clay != 0)
                    {
                        _clay = GetMaxCanBeSendByFullMerchant(_clay, osszesen, fullmerchant);
                        if (ti.MaxDestinationCapacity)
                            _clay = GetMaxCanBeSendByFreeCapacity(_clay, v_to.Productions[ResourcesType.Clay].FreeCapacity);
                    }

                    if (_iron != 0)
                    {
                        _iron = GetMaxCanBeSendByFullMerchant(_iron, osszesen, fullmerchant);
                        if (ti.MaxDestinationCapacity)
                            _iron = GetMaxCanBeSendByFreeCapacity(_iron, v_to.Productions[ResourcesType.Iron].FreeCapacity);
                    }

                    if (_crop != 0)
                    {
                        _crop = GetMaxCanBeSendByFullMerchant(_crop, osszesen, fullmerchant);
                        if (ti.MaxDestinationCapacity)
                            _crop = GetMaxCanBeSendByFreeCapacity(_crop, v_to.Productions[ResourcesType.Crop].FreeCapacity);
                    }

                    if (ti.AllowFullMerchantOnly)
                    {
                        osszesen = _lumber + _clay + _crop + _iron;
                        if (osszesen > v_from.MerchantCapacity)
                        {
                            double _lumberPercent = (_lumber * 100) / osszesen;
                            double _clayPercent = (_clay * 100) / osszesen;
                            double _ironPercent = (_iron * 100) / osszesen;
                            double _cropPercent = (_crop * 100) / osszesen;
                            int tmp = 0;
                            int maradek = osszesen % v_from.MerchantCapacity;
                            if (_lumber != 0)
                            {
                                tmp = Convert.ToInt32(Helpers.Round((double)maradek / 100 * _lumberPercent));
                                _lumber = _lumber - tmp;
                                maradek = maradek - tmp;
                                if ((_clay == 0) && (_iron == 0) && (_crop == 0))
                                {
                                    if (maradek > 0)
                                        _lumber = _lumber - maradek;
                                }
                                else
                                {
                                    osszesen = _clay + _crop + _iron;
                                    _clayPercent = (_clay * 100) / osszesen;
                                    _ironPercent = (_iron * 100) / osszesen;
                                    _cropPercent = (_crop * 100) / osszesen;
                                }
                            }
                            if (_clay != 0)
                            {
                                tmp = Convert.ToInt32(Helpers.Round((double)maradek / 100 * _clayPercent));
                                _clay = _clay - tmp;
                                maradek = maradek - tmp;
                                if ((_iron == 0) && (_crop == 0))
                                {
                                    if (maradek > 0)
                                        _clay = _clay - maradek;
                                }
                                else
                                {
                                    osszesen = _crop + _iron;
                                    _ironPercent = (_iron * 100) / osszesen;
                                    _cropPercent = (_crop * 100) / osszesen;
                                }
                            }
                            if (_iron != 0)
                            {
                                tmp = Convert.ToInt32(Helpers.Round((double)maradek / 100 * _ironPercent));
                                _iron = _iron - tmp;
                                maradek = maradek - tmp;
                                if (_crop == 0)
                                {
                                    if (maradek > 0)
                                        _iron = _iron - maradek;
                                }
                            }
                            if (_crop != 0)
                                _crop = _crop - maradek;
                        }
                        else
                            return;
                    }

                    if (v_to.Props.Origin.X != 0 && v_to.Props.Origin.Y != 0)
                        if (_lumber != 0 || _clay != 0 || _iron != 0 || _crop != 0)
                            SendResource(_lumber, _clay, _iron, _crop, v_to.Props.Origin.X, v_to.Props.Origin.Y);
                }
            }
        }

        public TraderItem AddSendResource()
        {
            TraderItem ti = new TraderItem();
            Data.TraderItems.Add(ti);
            ti.Active = false;
            ti.ResourcesTypeClay = true;
            ti.ResourcesTypeCrop = true;
            ti.ResourcesTypeIron = true;
            ti.ResourcesTypeLumber = true;
            ti.Type = TraderType.Fölözés;
            ti.MaxDestinationCapacity = true;
            ti.AllowFullMerchantOnly = true;
            ti.Value = 0;
            return ti;
        }

        private int GetMerchantCapacity()
        {
            // pl. gallnál alap: (750)
            // id('send_select')/tbody/tr[1]/td[4]/a
            int maxval = 1000;
            HtmlElement max = xpath.SelectElement(GetHTMLElement(TraviHTMLElementId.MarketMerchantCapacity));
            if (max != null)
            {
                switch (Data.TravianServerVersion)
                {
                    case TraviVersion.t30:
                    case TraviVersion.t35:
                        if (max.InnerText.Substring(0, 1) == "(")
                            maxval = Convert.ToInt32(max.InnerText.Substring(1, max.InnerText.Length - 2));
                        break;
                    case TraviVersion.t40:
                        // max.InnerText.Split('\');
                        string[] sa = Regex.Split(max.InnerText, "\r\n");
                            // Fa  / 750 \ r\ n Agyag  / 750 \ r\ n Vasérc  / 750 \ r\ n Búza  / 750
                        if (sa.Length > 1)
                        {
                            sa = sa[0].Split('/'); // Fa  / 750
                            if (sa.Length > 1)
                                maxval = int.Parse(sa[1].Trim());
                        }
                        break;
                }
            }
            return maxval;
        }

        /// <summary>
        /// szabad kereskedők száma
        /// </summary>
        /// <returns></returns>
        private int GetFreeMerchant(int villageId)
        {
            int maxval = 20;
            switch (Data.TravianServerVersion)
            {
                case TraviVersion.t30:
                case TraviVersion.t35:
                    /* NINCS MÁR 3.5 szerver, leszarom!
                    // pl. "Kereskedő 18/20"
                    // id('target_select')/tbody/tr[1]/td
                    HtmlElement max = xpath.SelectElement(GetHTMLElement(TraviHTMLElementId.MarketFreeMerchant));
                    if (max != null)
                    {
                        string[] arr = max.InnerText.Split('/');
                        if (arr.Length > 0)
                            maxval = Convert.ToInt32(arr[0].Substring(arr[0].Length - 2, 2));
                    }
                    break;
                    */
                case TraviVersion.t40:
                    Navigate("dorf3.php");
                    HtmlElement elem = xpath.SelectElement(GetHTMLElement(TraviHTMLElementId.MarketFreeMerchant));
                    HtmlElement subelem;
                    elem = elem.FirstChild;
                    while (elem != null)
                    {
                        subelem = elem.FirstChild; // td
                        
                        // a href="dorf1.php?newdid=103"
                        string ss = subelem.FirstChild.GetAttribute("href");
                        string[] sa = ss.Split('=');
                        if (int.Parse(sa[1]) == villageId)
                        {

                            subelem = subelem.NextSibling;
                            while (subelem != null)
                            {
                                if (string.Compare(subelem.GetAttribute("className"), "tra lc", true) == 0)
                                {
                                    subelem = subelem.FirstChild;
                                    ss = subelem.InnerHtml;
                                    sa = ss.Split('/');
                                    maxval = int.Parse(sa[0]);
                                    break;
                                }
                                subelem = subelem.NextSibling;
                            }
                            break;
                        }
                        elem = elem.NextSibling;
                    }
                    break;
            }
            return maxval;
        }

        public void SendAllMerchant()
        {
            foreach (TraderItem item in Data.TraderItems)
            {
                if (item.Active)
                {
                    // küldés előtt frissíteni kell a készletet
                    ChangeToVillage(Data.Villages[item.SourceVillageId]);
                    if (!item.Outside)
                        item.Outside = Data.VillagesOutside.ContainsKey(item.DestinationVillageId);
                    if (!item.Outside)
                        ChangeToVillage(Data.Villages[item.DestinationVillageId]);

                    DoTrade(item);
                }
            }
            Navigate("dorf1.php");  //mert ha ottmarad akkor a javascript frissiti az oldalt, az meg kerdez egyet
        }

        public void DoTradeItemDown(TraderItem ti)
        {
            int idxfrom = Data.TraderItems.IndexOf(ti);
            int idxto = Data.TraderItems.IndexOf(ti) + 1;
            if (idxfrom < (Data.TraderItems.Count - 1))
            {
                TraderItem titmp = Data.TraderItems[idxto];
                Data.TraderItems[idxto] = ti;
                Data.TraderItems[idxfrom] = titmp;
            }
        }

        public void DoTradeItemUp(TraderItem ti)
        {
            int idxfrom = Data.TraderItems.IndexOf(ti);
            int idxto = Data.TraderItems.IndexOf(ti) - 1;
            if (idxfrom > (0))
            {
                TraderItem titmp = Data.TraderItems[idxto];
                Data.TraderItems[idxto] = ti;
                Data.TraderItems[idxfrom] = titmp;
            }
        }

    }
}
