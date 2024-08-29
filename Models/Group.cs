namespace Tournament.Models
{
    public class Group
    {
        public string Name { get; set; }
        public List<TeamData> Teams { get; set; }

        public override string? ToString()
        {
            var str = $"Group {Name}:\n";

            foreach (var team in Teams)
            {
                str += team.ToString() + "\n";
            }

            return str;
        }
    }
}