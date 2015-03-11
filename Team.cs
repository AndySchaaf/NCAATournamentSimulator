using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCAATournamentSimulator
{
    class Team
    {
        public string Name;
        public string Pyth;
        public string AdjO;
        public string AdjD;
        public string SOS;
        public double Luck;
        public int Seed;

        public Team(string name, string pyth, string adjo, string adjd, string luck, string sos)
        {
            Name = name;
            Pyth = pyth;
            AdjO = adjo;
            AdjD = adjd;
            SOS = sos;

            string restOfLuck;
            if (luck[0].Equals('+'))
            {
                restOfLuck = luck.Substring(1, 3);
                if (!double.TryParse(restOfLuck, out Luck))
                {
                }
            }
            else if (luck[0].Equals('-'))
            {
                restOfLuck = luck.Substring(1, 3);
                if (double.TryParse(restOfLuck, out Luck))
                {
                    Luck = -1 * Luck;
                }
            }
        }
    }
}
