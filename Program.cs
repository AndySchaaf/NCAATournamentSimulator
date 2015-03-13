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
        public static string BracketWebSite = "http://espn.go.com/mens-college-basketball/bracketology/_/iteration/255"; //Must update to get current bracket
        public static string HTMLFileName = "HTMLdata.txt";
        public static string RegionFileName = "Regions.txt";

        static void Main(string[] args)
        {
            var TeamWithObjects = new Dictionary<String, Team>();
            Dictionary<string, int> TeamsWithRank = new Dictionary<string, int>();

            retreiveOnlineData();
            TeamWithObjects = getTeams();
            TeamsWithRank = getTeamsAndRank();
            TeamWithObjects = fillTeamsInBracket(TeamWithObjects, TeamsWithRank);
            assignRegions(TeamWithObjects);

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

        static Dictionary<string, int> getTeamsAndRank()
        {
            HtmlWeb Website = new HtmlWeb();
            HtmlDocument StatsDoc = Website.Load(BracketWebSite);
            HtmlNodeCollection Team = StatsDoc.DocumentNode.SelectNodes("//div[@class='bracket'] //a[@href]");
            HtmlNodeCollection Rank = StatsDoc.DocumentNode.SelectNodes("//span[@class='rank']");
            List<string> teams = new List<string>();
            List<int> rank = new List<int>();

            for (int i = 0; i < Rank.Count; i++)
            {
                rank.Add(int.Parse(Rank[i].InnerText));
            }
            for (int i = 0; i < Team.Count; i++)
            {
                teams.Add(Team[i].InnerText.ToUpper());
            }
            for (int i = 0; i < teams.Count; i++ )
            {
                teams[i] = ModifyESPNName(teams[i]);
            }
            for (int i = 1; i < rank.Count; i++)
            {
                if (rank[i] == rank[i - 1])
                {
                    rank.RemoveAt(i);
                    teams.RemoveAt(i);
                }
            }

            Dictionary<string, int> Teams = new Dictionary<string, int>();
            for (int i = 0; i < teams.Count; i++)
            {
                Teams.Add(teams[i], rank[i]);
            }
            return Teams;
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

            string[] TeamArray = new string[64];
            string line;
            int ta;
            int count = 0;
            while ((line = file.ReadLine()) != null)
            {
                if (line != "" && line != "WEST" && line != "EAST")
                {
                    if (int.TryParse(line[4].ToString(), out ta))
                    {
                        TeamArray[count] = line;
                        count++;
                    }
                }
            }

            int rank;
            count = 0;
            foreach (string teamName in TeamArray)
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
                                TeamArray[count] = teamName.Substring(i + 1, teamName.Length - (i + 1)).ToUpper();
                            }
                        }
                    }
                }
                else if (int.TryParse(two, out rank))
                {
                    string newName = teamName.Substring(6, teamName.Length - 6).ToUpper();
                    TeamArray[count] = newName;
                }
                else if (int.TryParse(one, out rank))
                {
                    string newName = teamName.Substring(5, teamName.Length - 5).ToUpper();
                    TeamArray[count] = newName;
                }
                count++;
            }
            file.Close();

            return TeamArray.ToList(); 
        }

        static Dictionary<String, Team> fillTeamsInBracket(Dictionary<String, Team> UnorderedTeams, Dictionary<string, int> TeamsWithRank)
        {
            var Teams = new Dictionary<String, Team>();

            UnorderedTeams = ModifyKenPomName(UnorderedTeams);
            foreach (var team in TeamsWithRank)
            {
                foreach (Team teamDic in UnorderedTeams.Values)
                {
                    string dicName = teamDic.Name.ToUpper().Trim();
                    if (team.Key.Equals(dicName))
                    {
                        Teams.Add(team.Key, teamDic);
                    }
                }
            }
            
            List<int> Seeds = new List<int>();
            foreach (int seed in TeamsWithRank.Values)
            {
                Seeds.Add(seed);
            }

            int count = 0;
            foreach(Team team in Teams.Values)
            {
                team.Seed = Seeds[count];
                count++;
            }
            return Teams;
        }

        static void assignRegions(Dictionary<String, Team> Teams)
        {
            int count = 0;
            int i = 0;
            foreach (var team in Teams)
            {
                if (count < 16)
                {
                    Midwest.Teams.Add(team.Key, team.Value);
                }
                else if (count < 32 && count >= 16)
                {
                    West.Teams.Add(team.Key, team.Value);
                }
                else if (count < 48 && count >= 32)
                {
                    East.Teams.Add(team.Key, team.Value);
                }
                else if (count < 64 && count >= 48)
                {
                    South.Teams.Add(team.Key, team.Value);
                }
                count++;
                if (i == 15)
                    i = 0;
                else
                    i++;
            }
        }

        static Dictionary<String, Team> ModifyKenPomName(Dictionary<String, Team> Teams)
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

        static string ModifyESPNName(string teamName)
        {
            if (teamName.Equals("VIRGINIA COMMONWEALTH"))
            {
                teamName = "VCU";
            }
            else if (teamName.Equals("ST. FRANCIS (NY)"))
            {
                teamName = "ST. FRANCIS NY";
            }
            else if (teamName.Equals("UL MONROE"))
            {
                teamName = "LOUISIANA MONROE";
            }
            else if (teamName.Equals("OLE MISS"))
            {
                teamName = "MISSISSIPPI";
            }
            else if (teamName.Equals("NC ST"))
            {
                teamName = "NORTH CAROLINA ST";
            }

            if (teamName.Substring(teamName.Length - 3, 3).Equals(" ST"))
            {
                teamName = teamName.Substring(0, teamName.Length - 2);
                teamName += "STATE";
            }

            return teamName;
        }
    }
}