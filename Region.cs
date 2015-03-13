﻿using System;
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
        public static List<Team> UpsetTeams = new List<Team>();

        public Team getWinner()
        {
            Team[] TeamArray = Teams.Values.ToArray();

            int numOfTeams = 16;
            for (int i = 0; i < 4; i++ )
            {
                TeamArray = playRound(TeamArray, numOfTeams);
                numOfTeams /= 2;
            }
            return TeamArray[0];
        }

        public Team[] playRound(Team[] TeamArray, int numOfTeams)
        {
            foreach (Team team in TeamArray)
            {
                Console.WriteLine(" " + team.Seed + "\t" + team.Name);
            }

            Team[] winners = new Team[numOfTeams/2];
            Team team1;
            Team team2;
            Team winner;

            int count = 0;
            for (int i = 1; i < numOfTeams + 2; i++)
            {
                if(i % 2 == 0) //Every two
                {
                    team1 = TeamArray[i - 2];
                    team2 = TeamArray[i - 1];

                    winner = game(team1, team2);
                    winners[count] = winner;
                    count++;
                }
            }
            Console.WriteLine();
            return winners;
        }

        /*
         * This is where the games are simulated. There are many ways to do this depending on your philosophy. 
         * Currently it's just using KenPom's "Pyth" state which, here, is weighted to get realistic probabilities.
         * However, offensive stats, defensive stats, schedule strength, luck, and seed are all held in the Team object 
         * and can easily be accessed here. 
         */
        public Team game(Team team1, Team team2)
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
            
            //p1 /= total;
            //p2 /= total;

            /*
             * After probabilities are set (pX/total=proability) games are simulated by a random number which picks the winning team.
             * This is done for a set amount of sims declared in the amountOfSims variable.
             * The team that gets the most wins out of that will be returned.
            */
            var WinningTeam = new Dictionary<Team, int>();
            int amountOfSims = 20;
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

            foreach(var team in WinningTeam)
            {
                if(team.Value > 45 && team.Value < 55)
                {
                    UpsetTeams.Add(team.Key);
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

        public void printCloseGames()
        {
            foreach(Team team in UpsetTeams)
            {
                Console.WriteLine(team.Name);
            }
        }
    }
}
