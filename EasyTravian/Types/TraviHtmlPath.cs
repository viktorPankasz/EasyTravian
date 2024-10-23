using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyTravian.Types
{
    public enum TraviVersion
    {
        tNone,
        t30,
        t35,
        t40,
        t42
    }

    public enum TraviHTMLElementId
    {
        None = -1,
        LoginUserNameInput,
        LoginPasswordInput,
        LoginButton,
        SlowNetworkCheckBox,
        ParseTribeProfilLink,
        ProfilTribe,
        VillageListId,
        ProfilVillages,
        DorfVillageMap,
        MarketMerchantCapacity,
        MarketFreeMerchant
    }

    public class TraviHTMLElement
    {
        public TraviVersion Version;
        public TraviHTMLElementId ElementId;
        public string Path; // XPath,DOM or grep constant

        internal static List<TraviHTMLElement> FillUp()
        {
            var result = new List<TraviHTMLElement>
             {
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t35,
                         ElementId = TraviHTMLElementId.LoginUserNameInput,
                         Path = "id('login_form')/tbody/tr[1]/td/input"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t40,
                         ElementId = TraviHTMLElementId.LoginUserNameInput,
                         //Path = "id('login_form')/x:tbody/x:tr[1]/x:td/x:input"
                         Path = "id('content')/x:div[1]/x:div[1]/x:form/x:table/x:tbody/x:tr[1]/x:td[2]/x:input"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t42,
                         ElementId = TraviHTMLElementId.LoginUserNameInput,
                         //Path = "id('login_form')/x:tbody/x:tr[1]/x:td/x:input"
                         Path = "id('content')/x:div[1]/x:div[1]/x:form/x:table/x:tbody/x:tr[1]/x:td[2]/x:input"
                     },

                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t35,
                         ElementId = TraviHTMLElementId.LoginPasswordInput,
                         Path = "id('login_form')/tbody/tr[2]/td/input"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t40,
                         ElementId = TraviHTMLElementId.LoginPasswordInput,
                         //Path = "id('login_form')/x:tbody/x:tr[2]/x:td/x:input"
                         Path = "id('content')/x:div[1]/x:div[1]/x:form/x:table/x:tbody/x:tr[2]/x:td[2]/x:input"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t42,
                         ElementId = TraviHTMLElementId.LoginPasswordInput,
                         //Path = "id('login_form')/x:tbody/x:tr[2]/x:td/x:input"
                         Path = "id('content')/x:div[1]/x:div[1]/x:form/x:table/x:tbody/x:tr[2]/x:td[2]/x:input"
                     },

                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t40,
                         ElementId = TraviHTMLElementId.SlowNetworkCheckBox,
                         //Path = "id('login_form')/x:tbody/x:tr[3]/x:td/x:input"
                         Path = "id('content')/x:div[1]/x:div[1]/x:form/x:table/x:tbody/x:tr[3]/x:td[2]/x:input"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t42,
                         ElementId = TraviHTMLElementId.SlowNetworkCheckBox,
                         //Path = "id('login_form')/x:tbody/x:tr[3]/x:td/x:input"
                         Path = "id('content')/x:div[1]/x:div[1]/x:form/x:table/x:tbody/x:tr[3]/x:td[2]/x:input"
                     },

                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t35,
                         ElementId = TraviHTMLElementId.LoginButton,
                         Path = "id('btn_login')"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t40,
                         ElementId = TraviHTMLElementId.LoginButton,
                         Path = "id('s1')"
                         //Path = "id('content')/x:div[1]/x:div[1]/x:form/x:table/x:tbody/x:tr[4]/x:td[2]/x:button"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t42,
                         ElementId = TraviHTMLElementId.LoginButton,
                         Path = "id('s1')"
                         //Path = "id('content')/x:div[1]/x:div[1]/x:form/x:table/x:tbody/x:tr[4]/x:td[2]/x:button"
                     },

                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t35,
                         ElementId = TraviHTMLElementId.ParseTribeProfilLink,
                         Path = "id('side_navi')/p[1]/a[3]"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t40,
                         ElementId = TraviHTMLElementId.ParseTribeProfilLink,
                         Path = "id('side_info')/x:div[2]/x:a"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t42,
                         ElementId = TraviHTMLElementId.ParseTribeProfilLink,
                         Path = "id('outOfGame')/x:li[1]/x:a/x:img"
                     },

                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t35,
                         ElementId = TraviHTMLElementId.ProfilTribe,
                         Path = "id('profile')/tbody/tr[2]/td[1]/table/tbody/tr[2]/td"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t40,
                         ElementId = TraviHTMLElementId.ProfilTribe,
                         Path = "id('details')/x:tbody/x:tr[2]/x:td"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t42,
                         ElementId = TraviHTMLElementId.ProfilTribe,
                         Path = "id('details')/x:tbody/x:tr[2]/x:td"
                     },

                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t35,
                         ElementId = TraviHTMLElementId.VillageListId,
                         Path = "vlist"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t40,
                         ElementId = TraviHTMLElementId.VillageListId,
                         Path = "villageList"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t42,
                         ElementId = TraviHTMLElementId.VillageListId,
                         Path = "sidebarBoxVillagelist"
                     },

                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t35,
                         ElementId = TraviHTMLElementId.ProfilVillages,
                         Path = "id('villages')/x:tbody"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t40,
                         ElementId = TraviHTMLElementId.ProfilVillages,
                         Path = "id('villages')/x:tbody"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t42,
                         ElementId = TraviHTMLElementId.ProfilVillages,
                         Path = "id('villages')/x:tbody"
                     },

                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t35,
                         ElementId = TraviHTMLElementId.DorfVillageMap,
                         Path = "village_map"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t40,
                         ElementId = TraviHTMLElementId.DorfVillageMap,
                         Path = "content"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t42,
                         ElementId = TraviHTMLElementId.DorfVillageMap,
                         Path = "content"
                     },

                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t35,
                         ElementId = TraviHTMLElementId.MarketMerchantCapacity,
                         Path = "id('send_select')/tbody/tr[1]/td[4]/a"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t40,
                         ElementId = TraviHTMLElementId.MarketMerchantCapacity,
                         Path = "id('send_select')/tr[1]/td[4]/a"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t42,
                         ElementId = TraviHTMLElementId.MarketMerchantCapacity,
                         Path = "id('send_select')/tr[1]/td[4]/a"
                     },
                     
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t35,
                         ElementId = TraviHTMLElementId.MarketFreeMerchant,
                         Path = "id('target_select')/tbody/tr[1]/td"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t40,
                         ElementId = TraviHTMLElementId.MarketFreeMerchant,
                         Path = "id('overview')/tbody"
                     },
                 new TraviHTMLElement()
                     {
                         Version = TraviVersion.t42,
                         ElementId = TraviHTMLElementId.MarketFreeMerchant,
                         Path = "id('overview')/tbody"
                     },
             };

            return result;
        }
    }


    public class TraviHtmlPath
    {
        public List<TraviHTMLElement> TraviHTMLElements;

        public TraviHtmlPath()
        {
            TraviHTMLElements = TraviHTMLElement.FillUp();
        }

        public string GetHTMLElement(TraviVersion traviServerVersion, TraviHTMLElementId elementId)
        {
            // TraviHTMLElement
            var result = from e in TraviHTMLElements
                         where (e.Version == traviServerVersion) && (e.ElementId == elementId)
                         select e.Path;

            if (result.Count() > 0)
                return result.First();
            return "";
        }
    }
}
