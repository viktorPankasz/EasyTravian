using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using EasyTravian.Types;

namespace EasyTravian
{
    public partial class TravianBase
    {
        private void GoRallyPointFarmList()
        {
            // http://tx3.travian.hu/build.php?id=39&tt=99
            Navigate("build.php?id=39&tt=99");
        }

        public FarmList AddFarmlist(VillageData village, string farmListName, int id, string xPathGo)
        {
            FarmList fl = new FarmList();
            try
            {
                fl.Village = village;
                fl.VillageId = village.Id;
                fl.FarmlistName = farmListName;
                fl.Id = id;
                fl.Active = true; // default
                Data.Farmlists.Add(fl);
            }
            catch (Exception)
            {
                if (Debugger.IsAttached)
                    throw;
            }
            return fl;
        }

        public FarmList AddFarmlist()
        {
            FarmList fl = new FarmList();
            Data.Farmlists.Add(fl);
            return fl;
        }

        public void DoFarm(FarmList fl)
        {
            if (fl.Active)
            {
                VillageData vd = Data.Villages[fl.VillageId];
                if (vd != null)
                {
                    // ugrás a falura
                    if (ChangeToVillage(vd))
                    {
                        // Farmlisták
                        GoRallyPointFarmList();

                        HtmlElement elem;

                        // Összes checkbox
                        // id('raidListMarkAll158')
                        elem = xpath.SelectElement(fl.XPathAll);
                        if (elem != null)
                        {
                            elem.InvokeMember("Click");

                            // Rablás indítása
                            // id('list158')/x:form/x:div[2]/x:button
                            elem = xpath.SelectElement(fl.XPathGo);
                            if (elem != null)
                                elem.InvokeMember("Click");
                        }
                    }
                }
            }
        }

        public void DoFarmAll()
        {
            foreach (FarmList fl in Data.Farmlists)
            {
                DoFarm(fl);
            }

        }

        public void DoFarmItemDown(FarmList fl)
        {
            int idxfrom = Data.Farmlists.IndexOf(fl);
            int idxto = Data.Farmlists.IndexOf(fl) + 1;
            if (idxfrom < (Data.Farmlists.Count - 1))
            {
                FarmList tmp = Data.Farmlists[idxto];
                Data.Farmlists[idxto] = fl;
                Data.Farmlists[idxfrom] = tmp;
            }
        }

        public void DoFarmItemUp(FarmList fl)
        {
            int idxfrom = Data.Farmlists.IndexOf(fl);
            int idxto = Data.Farmlists.IndexOf(fl) - 1;
            if (idxfrom > (0))
            {
                FarmList tmp = Data.Farmlists[idxto];
                Data.Farmlists[idxto] = fl;
                Data.Farmlists[idxfrom] = tmp;
            }
        }
    }
}
