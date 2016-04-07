using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Models
{
    public class UserSetting
    {
        public Dictionary<ulong, List<ulong>> HiddenChannels = new Dictionary<ulong, List<ulong>>();
        

        //Shoot Game
        public uint kills;
        public uint deaths;
        public decimal kdRatio
        {
            get
            {
                if (kills == 0 || deaths == 0)
                    return -1;

                return Math.Round((decimal)kills / (decimal)deaths, 5);
            }
        }
    }
}
