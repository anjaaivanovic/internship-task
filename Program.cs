using Tournament.Models;

namespace Tournament
{
    public class Program
    {
        static void Main(string[] args)
        {
            var tournament = TournamentManager.Instance;

            tournament.CalculateTeamStrengths();
            tournament.RunTournament();
        }
    }
}