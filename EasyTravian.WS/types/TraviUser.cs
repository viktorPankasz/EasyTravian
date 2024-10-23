using System;
using System.ComponentModel;

using DevExpress.Xpo;

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

namespace EasyTravian.WS.types
{
    [DefaultClassOptions]
    public class TraviUser : BaseObject
    {
        public TraviUser(Session session) : base(session) { }

        public TraviUser() { } //WS miatt csak

        public string UserName { get; set; }
        public string Server { get; set; }
        public DateTime LastLogin { get; set; }
    
    }

}
