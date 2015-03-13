using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCAATournamentSimulator
{
    class Region
    {
        public static Random random = new Random();
        public Dictionary<String, Team> Teams = new Dictionary<String, Team>();

        public Team getWinner()
        {
            Team[] TeamArray = Teams.Values.ToArray();

            int numOfTeams = 16;
            for (int i = 0; i < 4; i++ )
            {
                TeamArray = PlayRound(TeamArray, numOfTeams);

                //Each time a round is is played, the number of teams is halved 
                numOfTeams /= 2;
            }

            return TeamArray[0];
        }

        public Team[] PlayRound(Team[] TeamArray, int numOfTeams)
        {
            foreach (Team team in TeamArray)
            {
                Console.WriteLine(" " + team.Seed + "\t" + team.Name);
            }

            //There will always be half as many winners as the number of teams
            Team[] winners = new Team[numOfTeams/2];

            int count = 0;
            for (int i = 1; i < numOfTeams + 2; i++)
            {
                //Every two so that teams can be accessed in groups of two
                if(i % 2 == 0) 
                {
                    Team team1 = TeamArray[i - 2];
                    Team team2 = TeamArray[i - 1];

                    Team winner = SimulateGame(team1, team2);
                    winners[count] = winner;
                    count++;
                }
            }
            Console.WriteLine();
            return winners;
        }

        public Team SimulateGame(Team team1, Team team2)
        {
            double p1 = 1;
            double p2 = 1;
            
            p1 += (16 - team1.Seed);
            if (team1.AdjD < team1.AdjO)
            {
                p1 += (team1.AdjO - team1.AdjD);
            }
            p1 *= team1.SOS;
            p1 *= team1.Pyth;

            p2 += (16 - team2.Seed);
            if(team2.AdjD < team2.AdjO)
            {
                p2 += (team2.AdjO - team2.AdjD);
            }
            p2 *= team2.SOS;
            p2 *= team2.Pyth;

            double total = p1 + p2;

            var WinningTeam = new Dictionary<Team, int>();
            int amountOfSims = 15;
            for (int i = 0; i < amountOfSims; i++)
            {
                int randomNumber = randomNum(random, (int)total);

                if (randomNumber < p1)
                {
                    if (!WinningTeam.ContainsKey(team1))
                    {
                        WinningTeam.Add(team1, 1);
                    }
                    else
                    {
                        WinningTeam[team1]++;
                    }
                }
                else
                {
                    if (!WinningTeam.ContainsKey(team2))
                    {
                        WinningTeam.Add(team2, 1);
                    }
                    else
                    {
                        WinningTeam[team2]++;
                    }
                }
            }
            return getMostCommonValue(WinningTeam);            
        }

        Team getMostCommonValue(Dictionary<Team, int> dic)
        {
            KeyValuePair<Team, int> max = new KeyValuePair<Team, int>();
            foreach (var kvp in dic)
            {
                if (kvp.Value > max.Value)
                    max = kvp;
            }
            return max.Key;
        }

        int randomNum(Random random, int highValue)
        {
            return random.Next(1, highValue);
        }
    }
}
