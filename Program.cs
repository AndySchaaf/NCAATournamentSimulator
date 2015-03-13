using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
namespace NCAATournamentSimulator
{
    class Program
    {     
        public static Region Midwest = new Region();
        public static Region South = new Region();
        public static Region East = new Region();
        public static Region West = new Region();
        public static List<Region> Regions = new List<Region>(new Region[] { Midwest, South, East, West});
        public static List<Team> FinalFour = new List<Team>();
        public static string StatsWebSite = "http://kenpom.com";
        public static string HTMLFileName = "HTMLdata.txt";
        public static string RegionFileName = "Regions.txt";

        static void Main(string[] args)
        {
            var Teams = new Dictionary<String, Team>();
            List<string> TeamList = new List<string>();

            retreiveOnlineData();
            Teams = getTeams();
            TeamList = getBracket();
            Teams = fillTeamsInBracket(Teams, TeamList);
            assignRegions(Teams);

            foreach (Region region in Regions)
            {
                FinalFour.Add(region.getWinner());
                Console.WriteLine("--------------------------");
                Console.WriteLine();
            }

            foreach (Team team in FinalFour)
            {
                Console.WriteLine(" " + team.Seed + "\t" + team.Name);
            }
            Console.ReadKey();
        }

        static void retreiveOnlineData()
        {
            HtmlWeb Website = new HtmlWeb();
            HtmlDocument StatsDoc = Website.Load(StatsWebSite);
            HtmlNodeCollection Node = StatsDoc.DocumentNode.SelectNodes("//div[@id='data-area'] //td");
            StreamWriter File = new StreamWriter(HTMLFileName);

            int count = 1;
            for (int i = 0; i < Node.Count; i++)
            {
                string data = Node[i].InnerText;
                if (Node[i].OuterHtml.Equals("<td>" + count + "</td>") || Node[i].OuterHtml.Equals("<td class=\"bold-bottom\">" + count + "</td>"))
                {
                    string team = Node[i + 1].InnerText;
                    string Pyth = Node[i + 4].InnerText;
                    string AdjO = Node[i + 5].InnerText;
                    string AdjD = Node[i + 7].InnerText;
                    string Luck = Node[i + 11].InnerText;
                    string SOS = Node[i + 13].InnerText;
                    File.WriteLine(team + "," + Pyth + "," + AdjO + "," + AdjD + "," + Luck + "," + SOS);
                    count++;
                }
            }
            File.Close();
        }
        static Dictionary<String, Team> getTeams()
        {
            StreamReader file = new StreamReader(HTMLFileName);
            var Teams = new Dictionary<String, Team>();

            string line;
            while ((line = file.ReadLine()) != null)
            {
                List<string> TeamList = new List<string>();
                foreach (string word in line.Split(','))
                {
                    TeamList.Add(word);
                }
                for (int i = 0; i < 3; i++)
                {
                    if (i == 0)
                    {
                        Teams.Add(TeamList[i], new Team(TeamList[i], TeamList[i + 1], TeamList[i + 2], TeamList[i + 3], TeamList[i + 4], TeamList[i + 5]));
                    }
                }
            }
            file.Close();
            return Teams;
        }
        static List<string> getBracket()
        {
            StreamReader file = new StreamReader(RegionFileName);

            string[] teams = new string[64];
            string line;
            int ta;
            int count = 0;
            while ((line = file.ReadLine()) != null)
            {
                if (line != "" && line != "WEST" && line != "EAST")
                {
                    if (int.TryParse(line[4].ToString(), out ta))
                    {
                        teams[count] = line;
                        count++;
                    }
                }
            }
            int rank;
            int c = 0;
            foreach (string teamName in teams)
            {
                string two = teamName.Substring(4, 2);
                string one = teamName[4].ToString();
                if (teamName.Contains("/"))
                {
                    int numCount = 0;
                    for (int i = 0; i < teamName.Length; i++)
                    {
                        if (int.TryParse(teamName[i].ToString(), out rank))
                        {
                            numCount++;
                            if (numCount == 4)
                            {
                                teams[c] = teamName.Substring(i + 1, teamName.Length - (i + 1)).ToUpper();
                            }
                        }
                    }
                }
                else if (int.TryParse(two, out rank))
                {
                    string newName = teamName.Substring(6, teamName.Length - 6).ToUpper();
                    teams[c] = newName;
                }
                else if (int.TryParse(one, out rank))
                {
                    string newName = teamName.Substring(5, teamName.Length - 5).ToUpper();
                    teams[c] = newName;
                }
                c++;
            }
            file.Close();

            return teams.ToList(); 
        }
        static Dictionary<String, Team> fillTeamsInBracket(Dictionary<String, Team> uoTeams, List<string> TeamArray)
        {
            var Teams = new Dictionary<String, Team>();
            TeamArray = modifyESPNSchoolNames(TeamArray);
            uoTeams = modifyKenPomSchoolNames(uoTeams);
            foreach (string teamName in TeamArray)
            {
                foreach (Team teamDic in uoTeams.Values)
                {
                    string dicName = teamDic.Name.ToUpper().Trim();
                    if (teamName.Equals(dicName))
                    {
                        Teams.Add(teamName, teamDic);
                    }
                }
            }
            return Teams;
        }
        static void assignRegions(Dictionary<String, Team> Teams)
        {
            int[] seeds = { 1, 16, 8, 9, 5, 12, 4, 13, 6, 11, 3, 14, 7, 10, 2, 15 };
            int count = 0;
            int i = 0;
            foreach (var team in Teams)
            {
                if (count < 16)
                {
                    team.Value.Seed = seeds[i];
                    Midwest.Teams.Add(team.Key, team.Value);
                }
                else if (count < 32 && count >= 16)
                {
                    team.Value.Seed = seeds[i];
                    West.Teams.Add(team.Key, team.Value);
                }
                else if (count < 48 && count >= 32)
                {
                    team.Value.Seed = seeds[i];
                    East.Teams.Add(team.Key, team.Value);
                }
                else if (count < 64 && count >= 48)
                {
                    team.Value.Seed = seeds[i];
                    South.Teams.Add(team.Key, team.Value);
                }
                count++;
                if (i == 15)
                    i = 0;
                else
                    i++;
            }
        }

        static Dictionary<String, Team> modifyKenPomSchoolNames(Dictionary<String, Team> Teams)
        {
            foreach (Team team in Teams.Values)
            {
                team.Name = team.Name.Trim().ToUpper();
                string name = team.Name;
                if (team.Name.Substring(team.Name.Length - 3, 3).Equals("ST."))
                {
                    team.Name = team.Name.Substring(0, team.Name.Length - 3);
                    team.Name += "STATE";
                }
            }
            return Teams;
        }
        static List<string> modifyESPNSchoolNames(List<string> TeamArray)
        {
            for (int i = 0; i < TeamArray.Count; i++)
            {
                if (TeamArray[i].Equals("VIRGINIA COMMONWEALTH"))
                {
                    TeamArray[i] = "VCU";
                }
                else if (TeamArray[i].Equals("ST. FRANCIS (NY)"))
                {
                    TeamArray[i] = "ST. FRANCIS NY";
                }
                else if (TeamArray[i].Equals("UL MONROE"))
                {
                    TeamArray[i] = "LOUISIANA MONROE";
                }
                else if (TeamArray[i].Equals("OLE MISS"))
                {
                    TeamArray[i] = "MISSISSIPPI";
                }
                else if (TeamArray[i].Equals("NC ST"))
                {
                    TeamArray[i] = "NORTH CAROLINA ST";
                }
                if (TeamArray[i].Substring(TeamArray[i].Length - 3, 3).Equals(" ST"))
                {
                    TeamArray[i] = TeamArray[i].Substring(0, TeamArray[i].Length - 2);
                    TeamArray[i] += "STATE";
                }
            }
            return TeamArray;
        }
    }
}