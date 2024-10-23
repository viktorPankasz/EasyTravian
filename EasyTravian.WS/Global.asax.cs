using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using DevExpress.Xpo.DB;
using DevExpress.Xpo;

namespace EasyTravian.WS
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            /*
            Application["provider"] = DevExpress.Xpo.XpoDefault.GetConnectionProvider(
                   DevExpress.Xpo.DB.MSSqlConnectionProvider.GetConnectionString(".","EasyTravian"),
                    DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
             */
            /*
            Application["provider"] = DevExpress.Xpo.XpoDefault.GetConnectionProvider(
                   @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Inetpub\ftproot\Easytravian.WS\App_Data\Et.mdb",
                    DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
            */
            /*
            Application["provider"] = DevExpress.Xpo.XpoDefault.GetConnectionProvider(
                   DevExpress.Xpo.DB.AccessConnectionProvider.GetConnectionString( Server.MapPath("App_Data\\Et.mdb")),
                    DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
             */ 

            
            XpoDefault.DataLayer = XpoDefault.GetDataLayer(
                DevExpress.Xpo.DB.MSSqlConnectionProvider.GetConnectionString("localhost", "EasyTravian"),
                //@"Data Source=2003srv;Initial Catalog=EasyTravian;Integrated Security=True",
                AutoCreateOption.DatabaseAndSchema);
             

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}