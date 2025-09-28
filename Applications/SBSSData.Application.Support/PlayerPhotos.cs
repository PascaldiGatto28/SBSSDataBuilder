using HtmlAgilityPack;

using SBSSData.Softball;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    /// <summary>
    /// Provides utilities for locating, downloading, mapping and generating HTML tags for player photos
    /// used by the SBSS data tooling.
    /// 
    /// This class contains helpers to:
    /// - Build a mapping between roster player names and photo file name prefixes.
    /// - Create or refresh local copies of photo image files from the SBSS website.
    /// - Serialize/deserialize the player-to-image map to/from the configured data store.
    /// - Produce HTML image tags for a player by resolving the mapped image resource.
    /// 
    /// The class is implemented as a static helper and expects certain project-specific
    /// extension methods and types (e.g. serialization helpers, DataStoreContainer, Query,
    /// and EmbeddedImageResourceToHtml) to be available in the consuming solution.
    /// </summary>
    public static class PlayerPhotos
    {
        /// <summary>
        /// Mapping between player (roster) names and the file name prefix (no extension) of the their photo images.
        /// These are explicit overrides or known-corrections that are merged into the generated map.
        /// </summary>
        private static readonly Dictionary<string, string> updatePlayerName2PhotoName = new()
        {
            { "Weems, Russ", "Weems_Russell" },
            { "O'Donnell, Sue", "Odonnell_Sue" },
            { "O’Donnell, Sue", "Odonnell_Sue" },
            { "St Jules, Barb", "St-Jules_Barbara" },
            { "St Jules, David", "St-Jules_David" },
            { "Carbone, Bobby", "Carbonne_Bobby" },
            { "Takacs, Jim", "Talacs_Jim" },
            { "Schvartzberg, Yvette", "Schwartzberg_Yvette" },
            { "Jones, Mike", "Available_Photo-Not" }
        };
        private static readonly string urlPrefix = "https://saddlebrookesoftball.com/wp-content/gallery/player-pictures/";
        private static readonly string nlpUrlPrefix = "https://saddlebrookesoftball.com/wp-content/gallery/no-longer-playing/";

        /// <summary>
        /// Path to the on-disk data store used to persist the player name to image name mapping.
        /// This is a publicly-readable field to allow other parts of the application to access
        /// the configured location for the persisted mapping file.
        /// </summary>
        public static readonly string dataStorePath = @"J:\SBSSDataStore\";
        private static readonly string playerPhotosClassPath = @"J:\SBSSDataVS\";
        //private static readonly string playerPhotosClassPath = @"D:\Users\Richard\Documents\Visual Studio 2022\Github Projects\SBSS\";

        // TODO: This should be the the PlayerPhotos directory in the J:\SBSSDataStore\HTML directory.
        private static readonly string playerPhotosPath = $@"{playerPhotosClassPath}SBSSDataBuilder\Applications\SBSSData.Application.Support\PlayerPhotos\";
        private static Dictionary<string, string> playerNameToImageNameMap = [];

        /// <summary>
        /// Builds a fresh mapping between player roster names and the image file name prefixes discovered
        /// in the configured player photos folders (including "no longer playing"). The returned map is
        /// also persisted to the configured data store as JSON.
        /// </summary>
        /// <returns>
        /// A dictionary where the key is the roster player name (e.g. "Last, First") and the value
        /// is the photo image file name prefix (without extension), e.g. "Last_First" or "Available_Photo-Not".
        /// </returns>
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

        /// <summary>
        /// Loads the persisted player name to image name mapping from the configured data store if present,
        /// otherwise triggers a build of the mapping.
        /// </summary>
        /// <returns>
        /// The in-memory dictionary mapping roster player names to image file name prefixes.
        /// </returns>
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

        /// <summary>
        /// Persists the provided player name to image name mapping to the configured data store and
        /// reloads the saved map into the static in-memory field.
        /// </summary>
        /// <param name="map">The map to persist. If null, no write is performed but the persisted map is still reloaded.</param>
        /// <returns>The mapping that was saved and reloaded from disk.</returns>
        public static Dictionary<string, string> UpdatePlayerName2ImageNameMap(Dictionary<string, string> map)
        {
            string filePath = $"{dataStorePath}PlayerName2ImageNameMap.json";
            map?.Serialize(filePath);

            // Return the saved map, which sets the static property.
            return GetPlayerName2ImageNameMap();

        }

        /// <summary>
        /// Produces an HTML image tag for the requested <paramref name="playerName"/> by resolving the mapped
        /// image resource. If no image is found, a default "photo not available" image is used.
        /// The produced HTML is returned as a string suitable for embedding in generated HTML.
        /// </summary>
        /// <param name="playerName">The roster player name (e.g. "Last, First") to resolve.</param>
        /// <returns>An HTML string containing an image tag for the resolved resource.</returns>
        public static string GetPlayerImageTag(string playerName)
        {
            bool found = GetPlayerName2ImageNameMap().TryGetValue(playerName, out string? imageName);
            string resource = found ? @$"PlayerPhotos\{imageName}.jpg" : @"PlayerPhotos\Available_Photo-Not.jpg";

            return resource.EmbeddedImageResourceToHtml<HtmlGenerator>();
        }

        /// <summary>
        /// Creates a mapping between roster player names and discovered image file name prefixes based on
        /// the provided enumerable of image display names (where underscores have been replaced with comma+space).
        /// The method inspects all league data files in the data store to discover active player names.
        /// </summary>
        /// <param name="imageNames">An enumerable of image display names discovered in the photos folder (e.g. "Last, First").</param>
        /// <returns>
        /// A dictionary mapping roster player names to the image file name prefix (underscores instead of comma+space).
        /// Missing entries map to "Available_Photo-Not".
        /// </returns>
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
                IEnumerable<string> playerNames = query.GetActivePlayers().Select(p => p.Name).OrderBy(p => p).Distinct();
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

        /// <summary>
        /// Returns the last name portion of a roster-style name string using a comma separator.
        /// Example: "Smith, John" => "Smith".
        /// </summary>
        /// <param name="name">The roster formatted name.</param>
        /// <returns>The substring before the first comma.</returns>
        private static string LastName(string name)
        {
            return name.Split(',')[0];
        }

        /// <summary>
        /// Ensures the destination photos directory exists, downloads all active and
        /// "no longer playing" player photos from the configured remote gallery locations,
        /// and writes the files into the provided photos path.
        /// </summary>
        /// <param name="photosPath">The local directory where photo files will be written.</param>
        /// <returns>An ordered distinct enumerable of discovered player image file names (with extensions).</returns>
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

        /// <summary>
        /// Downloads and parses the HTML index page at the specified URL and returns the set of
        /// anchor text values representing player image file names discovered in the table body.
        /// </summary>
        /// <param name="url">The gallery URL to load and parse for image file names.</param>
        /// <returns>An enumerable of file names (as they appear on the remote index), filtered to valid photo entries.</returns>
        public static IEnumerable<string> GetPlayerImageFileNames(string url)
        {
            Uri uri = new(url);
            HtmlDocument htmlDocument = PageContentUtilities.GetPageHtmlDocument(uri);
            HtmlNode root = htmlDocument.DocumentNode.SelectSingleNode("//body/table");
            IEnumerable<string> playerImageFileNames = root.SelectNodes("tr/td/a").Select(n => n.InnerText).Where(s => (s.Contains('_') && !s.Contains("_backup", StringComparison.CurrentCulture) && !s.Contains("&gt;", StringComparison.CurrentCulture)));
            return playerImageFileNames;
        }

        /// <summary>
        /// Convenience wrapper that returns the active player photo file names from the main gallery URL.
        /// </summary>
        /// <returns>An enumerable of active player image file names discovered on the active players gallery page.</returns>
        public static IEnumerable<string> GetActivePlayerPhotoFileNames() => GetPlayerImageFileNames(urlPrefix);

        /// <summary>
        /// Downloads the binary content for each provided player URL name and writes it to the target directory.
        /// The method constructs the full remote URL by concatenating the provided URL prefix and the player URL name value.
        /// </summary>
        /// <param name="playerPhotosPath">Local output directory where files will be written.</param>
        /// <param name="urlPrefix">The remote gallery URL prefix used to form the full download URL.</param>
        /// <param name="playerUrlNames">A sequence of file names (as presented by the remote index) to download.</param>
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
