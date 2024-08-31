using System.Text.Json;
using Tournament.Common;
using static System.Formats.Asn1.AsnWriter;

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

        #region MatchSimulation

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

        private (int, int) SimulateMatch(TeamData team1, TeamData team2, char addedTab = '\t')
        {
            Random random = new Random();

            int team1Score = SimulateScore(team1.Strength);
            int team2Score = SimulateScore(team2.Strength);

            if (team1Score == team2Score)
            {
                var rand = random.Next(0, 2);
                if (rand == 0)
                {
                    team1Score++;
                }
                else
                {
                    team2Score++;
                }
            }
            Console.WriteLine($"\t{addedTab}{team1.Team} - {team2.Team} ({team1Score}:{team2Score})");
            team1.UpdateTeamGames(team2, team1Score, team2Score, team1Score > team2Score);
            team2.UpdateTeamGames(team1, team2Score, team1Score, team2Score > team1Score);

            return (team1Score, team2Score);
        }

        #endregion

        private void GroupPhase()
        {
            int maxRounds = 0;
            int maxGames = 0;

            foreach (var group in Groups)
            {
                group.GeneratePairings();
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

                        var (score1, score2) = SimulateMatch(team1, team2);
                    }

                    group.RankTeams();
                }
                counter += maxGames;
            }
            CalculateTeamStrengths();
        }

        #region Pots

        private Group CreatePots()
        {
            var firstPlaced = new Group { Teams = Groups.Select(g => g.Teams[0]).ToList() };
            var secondPlaced = new Group { Teams = Groups.Select(g => g.Teams[1]).ToList() };
            var thirdPlaced = new Group { Teams = Groups.Select(g => g.Teams[2]).ToList() };

            firstPlaced.RankPotTeams();
            secondPlaced.RankPotTeams();
            thirdPlaced.RankPotTeams();
            thirdPlaced.Teams.RemoveAt(2);

            return new Group
            {
                Name = Constants.Quarterfinals,
                Teams = firstPlaced.Teams.Concat(secondPlaced.Teams).Concat(thirdPlaced.Teams).ToList()
            };
        }

        private void PrintPots()
        {
            var pots = CreatePots();
            Groups = [pots];
            char pot = 'D';
            Console.WriteLine("Sesiri:");

            for (int i = 0; i < pots.Teams.Count; i++)
            {
                if (i % 2 == 0)
                {
                    Console.WriteLine($"\tSesir {pot++}:");
                }
                Console.WriteLine($"\t\t{pots.Teams[i].Team}");
            }
            
            Groups[0].Pairings = GenerateQuarterfinals();
            PrintEliminationPhase(pots);
        }

        private void PrintEliminationPhase(Group pots)
        {
            Console.WriteLine($"\n{Constants.EliminationPhase}:");

            var pairings = Groups[0].Pairings;

            for (int i = 0; i < pairings.Count; i++)
            {
                var team1 = pots.Teams.FirstOrDefault(team => team.ISOCode == pairings[i].Item1)?.Team;
                var team2 = pots.Teams.FirstOrDefault(team => team.ISOCode == pairings[i].Item2)?.Team;

                Console.WriteLine($"\t{team1} - {team2}");
                if (i % 2 == 1) Console.WriteLine();
            }
        }

        private List<(string, string)> GenerateQuarterfinals()
        {
            var teams = Groups[0].Teams;
            var quarterfinals = new List<(string, string)>();

            AddPairing(quarterfinals, teams[0], teams[6], teams[1], teams[7]);
            AddPairing(quarterfinals, teams[2], teams[4], teams[3], teams[5]);

            (quarterfinals[1], quarterfinals[2]) = (quarterfinals[2], quarterfinals[1]);

            return quarterfinals;
        }

        private void AddPairing(List<(string, string)> quarterfinals, TeamData teamA1, TeamData teamA2, TeamData teamB1, TeamData teamB2)
        {
            if (!teamA1.HasPlayedAgainst(teamA2) && !teamB1.HasPlayedAgainst(teamB2))
            {
                quarterfinals.Add((teamA1.ISOCode, teamA2.ISOCode));
                quarterfinals.Add((teamB1.ISOCode, teamB2.ISOCode));
            }
            else
            {
                quarterfinals.Add((teamB1.ISOCode, teamA2.ISOCode));
                quarterfinals.Add((teamA1.ISOCode, teamB2.ISOCode));
            }
        }

        #endregion

        private void Phase(string currentPhase, string nextPhase = "")
        {
            var group = Groups.FirstOrDefault(g => g.Name == currentPhase);
            var pairings = group.Pairings;

            Console.WriteLine($"\n{currentPhase}:");

            var winners = new List<TeamData>();
            var newPairings = new List<(string, string)>();

            foreach (var (team1, team2) in pairings.Select(p => (
                group.Teams.First(t => t.ISOCode == p.Item1),
                group.Teams.First(t => t.ISOCode == p.Item2))))
            {
                var (score1, score2) = SimulateMatch(team1, team2, '\0');
                winners.Add(score1 > score2 ? team1 : team2);
            }

            for (int i = 0; i < winners.Count; i += 2)
            {
                newPairings.Add((winners[i].ISOCode, winners[i + 1].ISOCode));
            }

            Groups.Add(new Group
            {
                Name = nextPhase,
                Teams = winners,
                Pairings = newPairings
            });

            if (nextPhase == Constants.Finals)
            {
                group.Teams.Where(t => !winners.Contains(t)).ToList()
                    .ForEach(t => Groups.First(g => g.Name == nextPhase).Teams.Add(t));
            }

            CalculateTeamStrengths();
        }

        private void LastPhase()
        {
            var group = Groups.FirstOrDefault(g => g.Name == Constants.Finals);
            var teams = group.Teams;
            var medals = new List<string>();

            for (int i = teams.Count - 1; i > 0; i -= 2)
            {
                var team1 = teams[i];
                var team2 = teams[i - 1];

                Console.WriteLine($"\n{(i > 1 ? Constants.ThirdPlaceGame : Constants.Finals)}:");

                var (score1, score2) = SimulateMatch(team1, team2, '\0');
                medals.Add(score1 < score2 ? team1.Team : team2.Team);
                medals.Add(score1 > score2 ? team1.Team : team2.Team);
            }

            medals.RemoveAt(0);
            medals.Reverse();

            Console.WriteLine($"\n{Constants.Medals}:");
            for (int i = 0; i < medals.Count; i++)
            {
                Console.WriteLine($"\t{i + 1}. {medals[i]}");
            }

            CalculateTeamStrengths();
        }

        public void RunTournament()
        {
            CalculateTeamStrengths();
            GroupPhase();
            PrintGroupData();
            CreatePots();
            PrintPots();
            Phase(Constants.Quarterfinals, Constants.Semifinals);
            Phase(Constants.Semifinals, Constants.Finals);
            LastPhase();
        }
    }
}