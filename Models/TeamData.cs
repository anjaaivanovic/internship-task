﻿using Tournament.Common;

namespace Tournament.Models
{
    public class TeamData
    {
        public string Team { get; set; }
        public string ISOCode { get; set; }
        public int FIBARanking { get; set; }
        public List<Game> Games { get; set; }
        public List<Game> Exibitions { get; set; }
        
        #region helperProperties
        
        public double Strength { get; private set; } = 0;
        public double PointDifferential { get; private set; } = 0;
        public int Points { get; set; } = 0;
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
            Games = [];
            Exibitions = [];
        }

        public void GetStats()
        {
            if (!Games.Any()) return;

            RecentGames = Games.Count;
            RecentWins = Games.Count(game => int.Parse(game.Result.Split("-")[0]) > int.Parse(game.Result.Split("-")[1]));
            TotalPointsScored = Games.Sum(game => int.Parse(game.Result.Split("-")[0]));
            TotalPointsConceded = Games.Sum(game => int.Parse(game.Result.Split("-")[1]));
            PointDifferential = (double)(TotalPointsScored - TotalPointsConceded) / RecentGames;
        }

        private double CalculateFormStrength(double maxDifferential)
        {
            var totalGames = Exibitions.Count + RecentGames;
            var totalWins = Exibitions.Count(game => int.Parse(game.Result.Split("-")[0]) > int.Parse(game.Result.Split("-")[1])) + RecentWins;
            var totalPointsScored = Exibitions.Sum(game => int.Parse(game.Result.Split("-")[0])) + TotalPointsScored;
            var totalPointsConceded = Exibitions.Sum(game => int.Parse(game.Result.Split("-")[1])) + TotalPointsConceded;
            var totalPointDifferential = (double)(totalPointsScored - totalPointsConceded) / totalGames;

            double winLossStrength = (double)totalWins / totalGames;
            double pointDiffStrength = (totalPointDifferential >= 0)? totalPointDifferential / maxDifferential: 0;

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
            return $" {Team}\t {RecentWins} / {RecentGames - RecentWins} / {Points} / {TotalPointsScored} / {TotalPointsConceded} / {TotalPointsScored - TotalPointsConceded}";
        }
    }
}