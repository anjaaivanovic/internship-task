using System.Text.Json;

namespace Tournament.Models
{
    public class TournamentManager
    {
        public List<Group> Groups { get; set; }
        private double MaxDifferential {  get; set; }

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
                        group.Teams[teamIndex].Games = gameTeam.Value;
                        break;
                    }
                }
            }
        }

        public void PrintGroupData()
        {
            foreach (var group in Groups)
            {
                Console.WriteLine(group);
            }
        }

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
    }
}
