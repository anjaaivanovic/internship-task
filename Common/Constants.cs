namespace Tournament.Common
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
        public static readonly string EliminationPhase = "Eliminaciona faza";
        public static readonly string Quarterfinals = "Cetvrtfinale";
        public static readonly string Semifinals = "Polufinale";
        public static readonly string Finals = "Finale";
        public static readonly string ThirdPlaceGame = "Utakmica za 3. mesto";
        public static readonly string Medals = "Medalje";
    }
}