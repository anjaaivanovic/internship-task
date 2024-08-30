using System.Text.Json;
using Tournament.Common;

namespace Tournament.Models
{
    public class TournamentManager
    {
        public List<Group> Groups { get; set; } = [];
        private double MaxDifferential { get; set; } = 15;

        private static TournamentManager? _instance = null;

        private TournamentManager()
        {
            LoadGroupData();
            LoadExibitionData();
        }

        public static TournamentManager Instance
        {
            get
            {
                if (_instance == null) _instance = new TournamentManager();
                return _instance;
            }
        }

        #region LoadData

        private void LoadGroupData()
        {
            try
            {
                string json = File.ReadAllText(Constants.GroupsFilePath);

                var groupsDictionary = JsonSerializer.Deserialize<Dictionary<string, List<TeamData>>>(json);

                Groups = new List<Group>();

                foreach (var entry in groupsDictionary)
                {
                    Groups.Add(new Group
                    {
                        Name = entry.Key,
                        Teams = entry.Value
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void LoadExibitionData()
        {
            string json = File.ReadAllText(Constants.ExibitionsFilePath);

            var gamesDictionary = JsonSerializer.Deserialize<Dictionary<string, List<Game>>>(json);

            foreach (var gameTeam in gamesDictionary)
            {
                foreach (var group in Groups)
                {
                    var teamIndex = group.Teams.FindIndex(team => team.ISOCode == gameTeam.Key);
                    if (teamIndex != -1)
                    {
                        group.Teams[teamIndex].Exibitions = gameTeam.Value;
                        break;
                    }
                }
            }
        }

        #endregion

        public void PrintGroupData()
        {
            Console.WriteLine();

            foreach (var group in Groups)
            {
                Console.WriteLine(group);
            }
        }

        #region Calculations

        private void CalculateStats()
        {
            foreach (var group in Groups)
            {
                foreach (var team in group.Teams)
                {
                    team.GetStats();
                }
            }
        }

        private void CalculateMaxDifferential()
        {
            foreach (var group in Groups)
            {
                var maxDiffGroup = group.Teams.Max(team => team.PointDifferential);

                if (maxDiffGroup > MaxDifferential)
                {
                    MaxDifferential = maxDiffGroup;
                }
            }
        }

        public void CalculateTeamStrengths()
        {
            CalculateStats();
            CalculateMaxDifferential();

            foreach (var group in Groups)
            {
                foreach (var team in group.Teams)
                {
                    team.CalculateOverallTeamStrength(MaxDifferential);
                }
            }
        }

        #endregion

        private List<(string, string)> GeneratePairings(Group group)
        {
            var pairings = new List<(string, string)>();
            int numTeams = group.Teams.Count;

            for (int round = 0; round < numTeams - 1; round++)
            {
                for (int i = 0; i < numTeams / 2; i++)
                {
                    var team1 = group.Teams[i];
                    var team2 = group.Teams[numTeams - 1 - i];

                    if (team1 != null && team2 != null)
                    {
                        pairings.Add((team1.ISOCode, team2.ISOCode));
                    }
                }

                var lastTeam = group.Teams[numTeams - 1];
                group.Teams.RemoveAt(numTeams - 1);
                group.Teams.Insert(1, lastTeam);
            }

            return pairings;
        }

        public int SimulateScore(double teamStrength)
        {
            Random rand = new Random();

            int baseScore = rand.Next(65, 75);
            int randomPoints = rand.Next(25, 51);
            int strengthMultiplier = (int)(randomPoints * teamStrength);

            int simulatedScore = baseScore + strengthMultiplier;

            int finalRandomness = rand.Next(0, 10);
            simulatedScore += finalRandomness;

            return simulatedScore;
        }

        private (int, int) SimulateMatch(TeamData team1, TeamData team2)
        {
            Random random = new Random();

            int team1Score = SimulateScore(team1.Strength);
            int team2Score = SimulateScore(team2.Strength);

            return (team1Score, team2Score);
        }

        private void UpdateTeamGames(TeamData team1, TeamData team2, int pointsScored, int pointsConceded, bool won)
        {
            team1.Games.Add(new Game {
                Opponent = team2.ISOCode,
                Result = $"{pointsScored}-{pointsConceded}" 
            });

            if (pointsScored > pointsConceded)
            {
                team1.Points += 2;
            }
            else
            {
                team1.Points++;
            }
        }

        public void RunTournament()
        {
            int maxRounds = 0;
            int maxGames = 0;

            foreach (var group in Groups)
            {
                group.Pairings = GeneratePairings(group);
                maxGames = group.Teams.Count / 2;
                maxRounds = group.Pairings.Count / maxGames;
            }

            int counter = 0;

            for (int i = 0; i < maxRounds; i++)
            {    
                Console.WriteLine($"{i + 1}. Kolo:");

                foreach (var group in Groups)
                {
                    Console.WriteLine($"\tGrupa {group.Name}:");

                    for (int j = 0; j < maxGames; j++)
                    {
                        var team1 = group.Teams.FirstOrDefault(team => team.ISOCode == group.Pairings[counter + j].Item1);
                        var team2 = group.Teams.FirstOrDefault(team => team.ISOCode == group.Pairings[counter + j].Item2);

                        var (team1Score, team2Score) = SimulateMatch(team1, team2);

                        Console.WriteLine($"\t\t{team1.Team} - {team2.Team} ({team1Score}:{team2Score})");

                        UpdateTeamGames(team1, team2, team1Score, team2Score, team1Score > team2Score);
                        UpdateTeamGames(team2, team1, team2Score, team1Score, team2Score > team1Score);
                    }

                    group.RankTeams();
                }
                counter += maxGames;
            }

            CalculateTeamStrengths();
            PrintGroupData();
        }
    }
}