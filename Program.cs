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

        static void Main(string[] args)
        {
            var TeamWithObjects = new Dictionary<String, Team>();
            Dictionary<string, int> TeamsWithRank = new Dictionary<string, int>();

            TeamWithObjects = buildTeams();
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

        static Dictionary<String, Team> buildTeams()
        {
            HtmlWeb Website = new HtmlWeb();
            HtmlDocument StatsDoc = Website.Load(StatsWebSite);
            HtmlNodeCollection Node = StatsDoc.DocumentNode.SelectNodes("//div[@id='data-area'] //td");
            Dictionary<String, Team> Teams = new Dictionary<String, Team>();

            int count = 1;
            for (int i = 0; i < Node.Count; i++)
            {
                string data = Node[i].InnerText;
                if (Node[i].OuterHtml.Equals("<td>" + count + "</td>") || Node[i].OuterHtml.Equals("<td class=\"bold-bottom\">" + count + "</td>"))
                {         
                    Teams.Add(Node[i + 1].InnerText, new Team(Node[i + 1].InnerText, Node[i + 4].InnerText, Node[i + 5].InnerText, Node[i + 7].InnerText, Node[i + 11].InnerText, Node[i + 13].InnerText));
                    count++;
                }
            }
            return Teams;
        }

        static Dictionary<String, Team> fillTeamsInBracket(Dictionary<String, Team> UnorderedTeams, Dictionary<string, int> TeamsWithRank)
        {
            Dictionary<String, Team> Teams = new Dictionary<String, Team>();

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
            assignSeeds(Teams, TeamsWithRank);
            
            return Teams;
        }

        static void assignSeeds(Dictionary<String, Team> Teams, Dictionary<string, int> TeamsWithRank)
        {
            List<int> Seeds = new List<int>();
            foreach (int seed in TeamsWithRank.Values)
            {
                Seeds.Add(seed);
            }

            int count = 0;
            foreach (Team team in Teams.Values)
            {
                team.Seed = Seeds[count];
                count++;
            }
        }

        static void assignRegions(Dictionary<String, Team> Teams)
        {
            int count = 0;
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

        static string ModifyESPNName(string Name)
        {
            if (Name.Equals("VIRGINIA COMMONWEALTH"))
            {
                Name = "VCU";
            }
            else if (Name.Equals("ST. FRANCIS (NY)"))
            {
                Name = "ST. FRANCIS NY";
            }
            else if (Name.Equals("UL MONROE"))
            {
                Name = "LOUISIANA MONROE";
            }
            else if (Name.Equals("OLE MISS"))
            {
                Name = "MISSISSIPPI";
            }
            else if (Name.Equals("NC ST"))
            {
                Name = "NORTH CAROLINA ST";
            }

            if (Name.Substring(Name.Length - 3, 3).Equals(" ST"))
            {
                Name = Name.Substring(0, Name.Length - 2);
                Name += "STATE";
            }

            return Name;
        }
    }
}