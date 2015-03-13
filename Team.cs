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
        public double Luck;
        public double Seed;
        public double Pyth;
        public double AdjO;
        public double AdjD;
        public double SOS;

        public Team(string name, string pyth, string adjo, string adjd, string luck, string sos)
        {
            Name = name;
            Pyth = double.Parse(pyth);
            AdjO = double.Parse(adjo);
            AdjD = double.Parse(adjd);
            SOS = double.Parse(sos);

            string restOfLuck;
            if (luck[0].Equals('+'))
            {
                restOfLuck = luck.Substring(1, 3);
                Luck = double.Parse(restOfLuck);
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
