namespace Tournament.Models
{
    public class Group
    {
        public string Name { get; set; }
        public List<TeamData> Teams { get; set; }
        public List<(string, string)> Pairings { get; set; }

        #region Ranking

        public void RankTeams()
        {
            Teams.Sort((team1, team2) => team2.Points.CompareTo(team1.Points));

            for (int i = 0; i < Teams.Count - 1; i++)
            {
                int j = i;
                while (j < Teams.Count - 1 && Teams[j].Points == Teams[j + 1].Points)
                {
                    j++;
                }

                if (j > i)
                {
                    var tiedTeams = Teams.GetRange(i, j - i + 1);

                    if (tiedTeams.Count == 2)
                    {
                        ResolveHeadToHead(tiedTeams);
                    }
                    else
                    {
                        ResolveCircleTiebreaker(tiedTeams);
                    }

                    Teams.RemoveRange(i, tiedTeams.Count);
                    Teams.InsertRange(i, tiedTeams);
                }

                i = j;
            }
        }

        private void ResolveHeadToHead(List<TeamData> tiedTeams)
        {
            tiedTeams.Sort((team1, team2) =>
            {
                var gameResult = team1.Games.FirstOrDefault(game => game.Opponent == team2.ISOCode)?.Result;

                if (gameResult != null)
                {
                    var team1Score = int.Parse(gameResult.Split("-")[0]);
                    var team2Score = int.Parse(gameResult.Split("-")[1]);

                    return team2Score.CompareTo(team1Score);
                }

                return 0;
            });
        }

        private void ResolveCircleTiebreaker(List<TeamData> tiedTeams)
        {
            var pointDifferentials = new Dictionary<TeamData, int>();

            foreach (var team in tiedTeams)
            {
                pointDifferentials[team] = 0;
            }

            foreach (var team in tiedTeams)
            {
                foreach (var opponent in tiedTeams)
                {
                    if (team == opponent) continue;

                    var gameResult = team.Games.FirstOrDefault(game => game.Opponent == opponent.ISOCode)?.Result;

                    if (gameResult != null)
                    {
                        var teamScore = int.Parse(gameResult.Split("-")[0]);
                        var opponentScore = int.Parse(gameResult.Split("-")[1]);

                        pointDifferentials[team] += teamScore - opponentScore;
                    }
                }
            }

            tiedTeams.Sort((team1, team2) => pointDifferentials[team2].CompareTo(pointDifferentials[team1]));
        }

        #endregion

        public override string? ToString()
        {
            var str = $"Grupa {Name} (Ime - pobede/porazi/bodovi/postignuti koševi/primljeni koševi/koš razlika):\n";
            var standing = 1;

            foreach (var team in Teams)
            {
                str += $"\t{standing++}. {team.ToString()}\n";
            }

            return str;
        }
    }
}