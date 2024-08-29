namespace Tournament.Models
{
    public class TeamData
    {
        public string Team { get; set; }
        public string ISOCode { get; set; }
        public int FIBARanking { get; set; }
        public List<Game> Games { get; set; }
        
        #region helperProperties
        
        public double Strength { get; private set; } = 0;
        public double PointDifferential { get; private set; } = 0;
        private int RecentGames { get; set; }
        private int RecentWins { get; set; }
        private int TotalPointsScored { get; set; }
        private int TotalPointsConceded { get; set; }

        #endregion

        public TeamData(string team, string iSOCode, int fIBARanking)
        {
            Team = team;
            ISOCode = iSOCode;
            FIBARanking = fIBARanking;
        }

        public void GetStats()
        {
            RecentGames = Games.Count;
            RecentWins = Games.Count(game => int.Parse(game.Result.Split("-")[0]) > int.Parse(game.Result.Split("-")[1]));
            TotalPointsScored = Games.Sum(game => int.Parse(game.Result.Split("-")[0]));
            TotalPointsConceded = Games.Sum(game => int.Parse(game.Result.Split("-")[1]));
            PointDifferential = (double)(TotalPointsScored - TotalPointsConceded) / RecentGames;
        }

        private double CalculateFormStrength(double maxDifferential)
        {
            double winLossStrength = (double)RecentWins / RecentGames;
            double pointDiffStrength = (PointDifferential >= 0)? PointDifferential / maxDifferential: 0;

            return (winLossStrength * Constants.WinLossWeight) + (pointDiffStrength * Constants.PointDiffWeight);
        }

        public void CalculateOverallTeamStrength(double maxDifferential)
        {
            double rankingStrength = 1.0 / (1.0 + Constants.Alpha * (FIBARanking - 1));
            double formStrength = CalculateFormStrength(maxDifferential);

            Strength = (rankingStrength * Constants.RankingWeight) + (formStrength * Constants.FormWeight);
        }


        public override string? ToString()
        {
            return $"  - {Team} ({ISOCode}), FIBA Ranking: {FIBARanking}, Strength: {Strength:F2}";
        }
    }
}