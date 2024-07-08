using System.Text;

using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    public class PlayerCardDisplay
    {
        public PlayerCardDisplay()
        {
            PlayerCard = new PlayerCard();
            //ImageFound = true;
        }

        public PlayerCardDisplay(PlayerCard playerCard) : this()
        {
            PlayerCard = playerCard;
        }

        private PlayerCard PlayerCard
        {
            get;
            init;
        }

        public string GetIntroduction(int width = 0)
        {
            int styleWidth = width == 0 ? 200 : width;
            StringBuilder sb = new();
            sb.AppendLine($"<div style=\"font-size:1.2em; border: solid black 1px; display: flex; padding: 5px; width:668px; margin: 5px;\">")
              .AppendLine($"<div style=\"display:flex; align-items:center; padding-right: 5px; max-width: 100%;\">")
              .AppendLine($"<img style=\"width:{styleWidth}px\" src={ImageSrc} alt=\"{PlayerDisplayName}\" />")
              .AppendLine($"</div>")
              .AppendLine($"<div style=\"display: flex; flex-direction:column; text-align:left; vertical-align:top; margin-left:10px\">")
              .AppendLine($"<h3 style=\"color:Firebrick; margin-bottom:.75em; font-weight:500; font-size:1.3em\">Introducing {PlayerDisplayName}</h3>")
              .AppendLine($"{PlayerFirstName} played in {NumLeagues.NumDesc("league")}, and")
              .AppendLine($"played for {NumTeams.NumDesc("team")} in {NumGames.NumDesc("game")} during the {Season} season.");
            if (NumLeagues > 0)
            {
                sb.AppendLine($"<div style=\"margin-top:.75em;\">")
                  .AppendLine($"The tables are comparative stats for {PlayerFirstName} and a summary of all players")
                  .AppendLine($"that played in the same games and on the same teams as {PlayerFirstName} during this season.")
                  .AppendLine($"Table data includes all roster, substitute and replacement data. Many players")
                  .AppendLine($"are on multiple teams in multiple leagues and substitute also.")
                  .AppendLine($"</div>")
                  .AppendLine($"</div>")
                  .AppendLine($"</div>")
                  .AppendLine($"<div style=\"margin-top:.75em; color:Firebrick; font-size:1em\">")
                  .AppendLine($"Click anywhere on the table heading <span style=\"font-weight:bold;\">text</span> to")
                  .AppendLine(" collapse (▲) or expand (▼) the table.")
                  .AppendLine($"</div>");
            }

            return sb.ToString();
        }

        public string Season => PlayerCard.Season;

        public string PlayerName => PlayerCard.PlayerName;
        public string PlayerDisplayName => PlayerName.BuildDisplayName();
        public string PlayerFirstName => PlayerName.Split(',')[1].Trim();

        public string ImageSrc => GetPlayerImageTag(PlayerName).Substring("<img src=", "/>", false, false);

        //public bool ImageFound
        //{
        //    get;
        //    set;
        //}

        public int NumLeagues => PlayerCard.Leagues.Count();
        public int NumGames => PlayerCard.Games.Count();
        public int NumTeams => PlayerCard.Teams.Count();



        public static string GetPlayerImageTag(string playerName)
        {

            return PlayerPhotos.GetPlayerImageTag(playerName);
        }


        public List<PlayerCardDataSummary> PlayerDataDisplays = [];

        public List<PlayerCardDataSummary> Add(PlayerCardDataSummary playerCardDataSummary)
        {
            PlayerDataDisplays.Add(playerCardDataSummary);
            return PlayerDataDisplays;
        }

        public string Display(int width = 0, int collapseTo = 1)
        {
            string html = string.Empty;
            using (HtmlGenerator generator = new())
            {
                generator.WriteRawHtml(GetIntroduction(width));
                foreach (var dataDisplay in PlayerDataDisplays)
                {
                    generator.WriteRootTable(dataDisplay.DisplaySummary(), ds => dataDisplay.Header);
                }

                html = generator.DumpHtml(collapseTo: collapseTo);
            }

            return html;
        }
    }

}
