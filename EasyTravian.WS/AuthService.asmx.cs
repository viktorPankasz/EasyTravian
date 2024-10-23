using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using DevExpress.Xpo;
using EasyTravian.WS.types;
using DevExpress.Data.Filtering;
using System.Linq;

namespace EasyTravian.WS
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class Service1 : System.Web.Services.WebService
    {

        [WebMethod]
        public bool Authorize( string user, string server )
        {
            UnitOfWork uow = new UnitOfWork(XpoDefault.DataLayer);
            uow.UpdateSchema();
            TraviUser u = uow.FindObject<TraviUser>(CriteriaOperator.Parse("UserName='" + user + "' and Server='" + server + "'"));
            if (u == null)
            {
                u = new TraviUser(uow);
                u.UserName = user;
                u.Server = server;
            }
            u.LastLogin = DateTime.Now;
            uow.CommitChanges();

            return true; //egyelore :)
        }

        [WebMethod]
        public TraviUser[] GetAllUsers()
        {
            UnitOfWork uow = new UnitOfWork(XpoDefault.DataLayer);
            uow.UpdateSchema();
            XPCollection<TraviUser> users = new XPCollection<TraviUser>();

            var us = from u in users
                     select u;

            return us.ToArray();

        }
    }
}
