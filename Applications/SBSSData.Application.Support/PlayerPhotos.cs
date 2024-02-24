using HtmlAgilityPack;

using SBSSData.Softball;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    public static class PlayerPhotos
    {
        /// <summary>
        /// Mapping between player (roster) names and the file name prefix (no extension) of the their photo images.
        /// </summary>
        private static readonly Dictionary<string, string> updatePlayerName2PhotoName = new()
        {
            { "Kennedy, Summer", "Available_Photo-Not"},
            { "Weems, Russ", "Weems_Russell" },
            { "O'Donnell, Sue", "Odonnell_Sue" },
            { "O’Donnell, Sue", "Odonnell_Sue" },
            { "St Jules, Barb", "St-Jules_Barbara" },
            { "St Jules, David", "St-Jules_David" },
            { "Carbone, Bobby", "Carbonne_Bobby" },
            { "Takacs, Jim", "Talacs_Jim" },
            { "Schvartzberg, Yvette", "Schwartzberg_Yvette" },
            { "Jones, Mike", "Available_Photo-Not" },
            { "Levaro, Richard", "Levaro_Richard" }
        };
        private static readonly string urlPrefix = "https://saddlebrookesoftball.com/wp-content/gallery/player-pictures/";
        private static readonly string nlpUrlPrefix = "https://saddlebrookesoftball.com/wp-content/gallery/no-longer-playing/";
        public static readonly string dataStorePath = @"J:\SBSSDataStore\";
        private static readonly string playerPhotosPath = @"D:\Users\Richard\Documents\Visual Studio 2022\Github Projects\SBSS\SBSSDataBuilder\Applications\SBSSData.Application.Support\PlayerPhotos\";
        private static Dictionary<string, string> playerNameToImageNameMap = [];

        public static Dictionary<string, string> Build()
        {
            IEnumerable<string> playerImageFilesNames = CreateImages(playerPhotosPath);
            IEnumerable<string> imageNames = Directory.GetFiles(playerPhotosPath)
                                                      .Select(p => Path.GetFileNameWithoutExtension(p))
                                                      .Select(p => p.Replace("_", ", "))
                                                      .OrderBy(p => p);

            Dictionary<string, string> map = CreatePlayer2ImageMap(imageNames);

            // Now add the entries which we know are correct but don't find them programatically
            foreach (string key in updatePlayerName2PhotoName.Keys)
            {
                map[key] = updatePlayerName2PhotoName[key];
            }


            playerNameToImageNameMap = map.OrderBy(x => x.Key).ToDictionary<string, string>();
            File.WriteAllText($"{dataStorePath}PlayerName2ImageNameMap.json", playerNameToImageNameMap.ToJsonString());

            return playerNameToImageNameMap;
        }

        public static Dictionary<string, string> GetPlayerName2ImageNameMap()
        {

            string filePath = $"{dataStorePath}PlayerName2ImageNameMap.json";
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                playerNameToImageNameMap = filePath.Deserialize<Dictionary<string, string>>() ?? [];
            }
            else
            {
                playerNameToImageNameMap = Build();
            }

            return playerNameToImageNameMap;
        }

        public static Dictionary<string, string> UpdatePlayerName2ImageNameMap(Dictionary<string, string> map)
        {
            string filePath = $"{dataStorePath}PlayerName2ImageNameMap.json";
            map?.Serialize(filePath);

            // Return the saved map, which sets the static property.
            return GetPlayerName2ImageNameMap();

        }

        public static string GetPlayerImageTag(string playerName)
        {
            bool found = GetPlayerName2ImageNameMap().TryGetValue(playerName, out string? imageName);
            string resource = found ? @$"PlayerPhotos\{imageName}.jpg" : @"PlayerPhotos\Available_Photo-Not.jpg";

            return resource.EmbeddedImageResourceToHtml<HtmlGenerator>();
        }

        public static Dictionary<string, string> CreatePlayer2ImageMap(IEnumerable<string> imageNames)
        {
            Dictionary<string, string> namesMap = [];
            List<string> playersNotFound = [];
            foreach (string filePath in Directory.GetFiles(dataStorePath).Where(f => f.EndsWith("LeaguesData.json")))
            {
                using DataStoreContainer dsContainer = DataStoreContainer.Instance(filePath);
                //Console.WriteLine("\r\n\r\nProcessing {filePath} season +++++++++++++++++++++++++++++++++++++++++");
                Query query = new(dsContainer);
                //Console.WriteLine($"{query}");
                IEnumerable<string> playerNames = query.Players.Select(p => p.Name).OrderBy(p => p).Distinct();
                foreach (string playerName in playerNames)
                {
                    string? imageName = imageNames.SingleOrDefault(n => string.Equals(n, playerName, StringComparison.OrdinalIgnoreCase));
                    if (string.IsNullOrEmpty(imageName))
                    {
                        //Console.WriteLine($"{player} resource cannot be found.");
                        string playerLastName = LastName(playerName);
                        IEnumerable<string> imageFileNames = imageNames.Where(n => LastName(n) == playerLastName);
                        if (imageFileNames.Any())
                        {
                            string foundLastNames = imageFileNames.Select(n => n.Replace(", ", "_")).ToString("; ");
                            //Console.WriteLine($"{playerName} not found, but the name {foundLastNames} found.");
                            if (namesMap.TryGetValue(playerName, out string? value))
                            {
                                if (string.IsNullOrEmpty(value))
                                {
                                    namesMap[playerName] = foundLastNames;
                                }
                            }
                            else
                            {
                                namesMap.TryAdd(playerName, foundLastNames);
                            }
                        }
                        else
                        {
                            //Console.WriteLine($"{playerName} not found");
                            playersNotFound.Add(playerName);
                            namesMap.TryAdd(playerName, "Available_Photo-Not");
                        }
                    }
                    else
                    {
                        namesMap.TryAdd(playerName, imageName.Replace(", ", "_"));
                    }
                }
            }

            return namesMap;
        }

        private static string LastName(string name)
        {
            return name.Split(',')[0];
        }

        public static IEnumerable<string> CreateImages(string photosPath)
        {
            Directory.CreateDirectory(photosPath);

            List<string> playerImageFileNames = GetPlayerImageFileNames(urlPrefix).ToList();
            BuildPhotoFiles(photosPath, urlPrefix, playerImageFileNames);
            List<string> nlpPlayerImageFileNames = GetPlayerImageFileNames(nlpUrlPrefix).Where(p => !playerImageFileNames.Contains(p)).ToList();
            BuildPhotoFiles(photosPath, nlpUrlPrefix, nlpPlayerImageFileNames);

            playerImageFileNames.AddRange(nlpPlayerImageFileNames);

            return playerImageFileNames.OrderBy(p => p).Distinct();
        }

        public static IEnumerable<string> GetPlayerImageFileNames(string url)
        {
            Uri uri = new(url);
            HtmlDocument htmlDocument = PageContentUtilities.GetPageHtmlDocument(uri);
            HtmlNode root = htmlDocument.DocumentNode.SelectSingleNode("//body/table");
            IEnumerable<string> playerImageFileNames = root.SelectNodes("tr/td/a").Select(n => n.InnerText).Where(s => (s.Contains('_') && !s.Contains("_backup", StringComparison.CurrentCulture) && !s.Contains("&gt;", StringComparison.CurrentCulture)));
            return playerImageFileNames;
        }

        public static IEnumerable<string> GetActivePlayerPhotoFileNames() => GetPlayerImageFileNames(urlPrefix);

        public static void BuildPhotoFiles(string playerPhotosPath, string urlPrefix, IEnumerable<string> playerUrlNames)
        {
            using HttpClient client = new();
            foreach (string playerUrlName in playerUrlNames)
            {
                string playerUrl = $"{urlPrefix}{playerUrlName}";
                byte[] pageData = client.GetByteArrayAsync(playerUrl).Result;
                string outputPath = $"{playerPhotosPath}{playerUrlName}";
                File.WriteAllBytes(outputPath, pageData);
            }
        }
    }
}
