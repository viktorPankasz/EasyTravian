using System;
using System.Threading;
using System.Windows.Forms;

namespace EasyTravian
{
    /// <summary>
    /// Summary description for Class1
    /// </summary>
    public partial class TravianBase
    {
        public void SendMail(string recipient, string subject, string body)
        {
            recipient = recipient.Replace(";", ",");
            string[] traviusers = recipient.Split(',');

            for(int i = 0; i < traviusers.Length; i++)
            {
                Navigate("nachrichten.php?t=1");
                // címzett id('receiver')
                // tárgy: id('subject')
                // body: id('igm')
                if (xpath.ElementExists("id('receiver')")) 
                {
                    if (xpath.SetAttribute("id('receiver')", "value", traviusers[i])
                       &&
                       xpath.SetAttribute("id('subject')", "value", subject)
                       &&
                       xpath.SetAttribute("id('igm')", "value", body))
                    {
                        //HtmlElement el = xpath.SelectElement("id('lmid2')/form/table/tbody/tr[6]/td/input[2]");
                        //el.InvokeMember("Click");
                        Submit();

                        // 500 kevés, kihagy
                        // 1000-nél 3 címzettből 2 kapja meg
                        Thread.Sleep(3000);
                        Navigate("dorf1.php");  // elnavigálás
                    }

                }
            }

       }
    }
}