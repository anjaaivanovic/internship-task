namespace Tournament.Models
{
    public static class Constants
    {
        public static readonly string GroupsFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "groups.json");
        public static readonly string ExibitionsFilePath = Path.Combine(AppContext.BaseDirectory, "Data", "exibitions.json");
        public static readonly double RankingWeight = 0.6;
        public static readonly double FormWeight = 0.4;
        public static readonly double Alpha = 0.1;
        public static readonly double WinLossWeight = 0.7;
        public static readonly double PointDiffWeight = 0.3;
    }
}