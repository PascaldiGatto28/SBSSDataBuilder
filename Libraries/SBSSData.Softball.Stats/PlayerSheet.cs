namespace SBSSData.Softball.Stats
{
    public class PlayerSheet
    {
        public PlayerSheet() 
        {
            PlayerName = string.Empty;
            LeagueNames = Enumerable.Empty<LeagueName>();
            Games = Enumerable.Empty<Game>();
            Teams = Enumerable.Empty<Team>();
            Season = string.Empty;
        }

        public string PlayerName { get; set; }

        public IEnumerable<LeagueName> LeagueNames { get; set; }

        public IEnumerable<Game> Games { get; set; }

        public IEnumerable<Team> Teams { get; set; }

        public string Season { get; set; }

        private void Initialize(Query query)
        {
            IEnumerable<Game> playedGames = query.GetPlayedGames();
            IEnumerable<Game> playerPlayedGames = playedGames.Where(g => g.Teams.SelectMany(t => t.Players)
                                                                                .Where(p => p.Name == PlayerName)
                                                                                .Any());
            Games = playerPlayedGames;

            IEnumerable<LeagueDescription> leagueDescriptions = playerPlayedGames.Select(g => g.GameInformation).Select(i => new LeagueDescription()
            {
                LeagueCategory = i.LeagueCategory,
                LeagueDay = i.LeagueDay,
                Season = i.Season,
                Year = i.Year,
            }).Distinct();

            IEnumerable<LeagueName> leagueNames = leagueDescriptions.Select(l => new LeagueName(l));

            //IEnumerable<Tuple<string, string>> leagueNames = Games.Select(g => Tuple.Create(g.GameInformation.LeagueCategory, g.GameInformation.LeagueDay)).Distinct();
            //IEnumerable<LeagueDescription> descriptions = query.GetLeagueDescriptions().Where(l => leagueNames.Contains(Tuple.Create(l.LeagueCategory, l.LeagueDay)));

            LeagueNames = leagueNames;

            IEnumerable<Team> teams = Games.SelectMany(g => g.Teams.Where(t => t.Players.Select(p => p.Name).Contains(PlayerName)));
            IEnumerable<IGrouping<string, Team>> groups = teams.GroupBy(t => t.Name);

            IEnumerable<Team> teamGroups = groups.Select(g => new TeamSummary(g));

            Teams = teamGroups;

        }

    }
}
