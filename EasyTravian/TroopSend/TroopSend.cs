using System;
using System.Collections.Generic;
using System.Text;
using EasyTravian.Types;
using System.Drawing;


namespace EasyTravian
{
    public partial class TravianBase
    {
        

        public void SendOne(TroopSendParameters info)
        {
            //küddés

            Data.TroopSends.Remove(info);    
        }

        public void Send()
        {
            DateTime Nextdate = DateTime.MaxValue;
            TroopSendParameters NextSend = null;
            foreach (TroopSendParameters info in Data.TroopSends)
            {
                if (info.Start < Nextdate)
                {
                    Nextdate = info.Start;
                    NextSend = info;
                }
            }

            if (NextSend.Start.Subtract(new TimeSpan(0, 1, 0)) < Data.GetServerTime)  //akkor megvárjuk
            {
                while (NextSend.Start.Subtract(new TimeSpan(0, 0, 1)) < Data.GetServerTime )
                {
                    Navigate(NextSend.From.Props.url);
                    System.Threading.Thread.Sleep(100);
                }

                SendOne(NextSend);
            }
        }

    }

    public class TroopSendParameters
    {
        public VillageData From;
        public Point To;
        public Troop[] Troops;
        public Boolean Hero;
        public Building Target1;
        public Building Target2;
        public SendArmyAttackType AttackType;
        public DateTime Start;

        //calc
        public DateTime Arrival
        {
            get
            {
                return DateTime.MinValue;
            }
        }


    }
}
