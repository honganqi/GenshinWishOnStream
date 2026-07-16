using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

namespace GenshinImpact_WishOnStreamGUI
{
    public interface ILibraryService
    {
        StarList StarListObject { get; set; }
        List<string> DullBlades { get; set; }
        UserInfo UserInfoObject { get; set; }
        HttpServer HttpServerObject { get; set; }

        // Authentication
        Task HandleAuthSync(HttpServer.AuthPayload payload);

        // Profiles
        void SaveProfile(string selectedProfile, string sourceProfileOfImages);
        Task DeleteProfile(string selectedProfile);
        string[] CheckProfiles();
        bool ActivateProfile(string selectedProfile);

        // Pull Settings
        bool ValidateOrRepairPullSettings(string selectedProfile);

        // Configs
        int GetDefaultCharacterListVersion();
        Task<(bool httpOk, int latestCharacterListVersion)> CheckCharacterListUpdate();
        Task<bool> DownloadDefaultConfigs(List<string> items);
        Task<List<string>> DownloadDefaultImages_Prefetch(bool fullDownload = true);
        Task<bool> DownloadDefaultImages(List<string> paths, IProgress<DownloadProgress> progress = null);

        // User Info
        Task ValidateUserSettingsFromFiles();
        void SaveUserSettingsToFile(bool revoke = false, bool fromExpiredToken = false);
        event Action<UserInfo> UpdateSettingsPanelWithUserInfo;
        void RewriteLocalCreds(HttpServer.AuthPayload payload);
    }

    public class Library : ILibraryService
    {
        static string profilesPath = Path.Combine(Application.StartupPath, "profiles");
        static string jsPath = Path.Combine(Application.StartupPath, "js");
        static string imgPath = Path.Combine(Application.StartupPath, "img");

        public StarList StarListObject { get; set; } = new();
        public List<string> DullBlades { get; set; } = new();

        public UserInfo UserInfoObject { get; set; } = new();
        public HttpServer HttpServerObject { get; set; } = new();
        public event Action<UserInfo> UpdateSettingsPanelWithUserInfo;


        #region Authentication
        public void InitializeApp()
        {
            HttpServer httpServer = new();
        }

        public async Task HandleAuthSync(HttpServer.AuthPayload payload)
        {
            UserInfo tempUser = ReadUserSettingsFromFile();
            UserInfoObject.UpdateUserInfoFromAuthPayload(payload);
            SaveUserSettingsToFile();
            UpdateSettingsPanelWithUserInfo.Invoke(UserInfoObject);
        }
        #endregion

        #region Profiles
        public void SaveProfile(string selectedProfile, string sourceProfileOfImages = "")
        {
            // create directory if it doesn't exist
            string profileJsPath = GetProfilePath(selectedProfile, "js");
            if (Directory.Exists(profileJsPath))
            {
                DialogResult exitAsk = MessageBox.Show("This will overwrite the profile. Are you sure?", "Confirm overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (exitAsk != DialogResult.Yes)
                    return;
            }
            Directory.CreateDirectory(profileJsPath);
            string pathChoices = Path.Combine(profileJsPath, "choices.js");
            string pathRates = Path.Combine(profileJsPath, "rates.js");

            List<string> messages = new();

            if (StarListObject.Count > 0)
            {
                SortedDictionary<int, int> rates = new();
                Dictionary<string, string> characterElements = new();
                using (StreamWriter writer = new(pathChoices))
                {
                    writer.WriteLine("let choices = [];\n");

                    // iterate over characters and rates
                    foreach (KeyValuePair<int, CharacterListInStar> charListPair in StarListObject)
                    {
                        int starValue = charListPair.Key;
                        CharacterListInStar charList = charListPair.Value;
                        rates.Add(starValue, charList.PullRate);

                        writer.WriteLine("// " + starValue + "-star choices");
                        writer.WriteLine("choices[" + starValue + "] = [");

                        foreach (Character character in charList)
                        {
                            writer.WriteLine("\t{name: \"" + character.CharacterName + "\", element: \"" + character.Element + "\"},");
                            characterElements.Add(character.CharacterName, character.Element);
                        }

                        writer.WriteLine("];\n");
                    }

                    // dull blades
                    if (DullBlades.Count > 0)
                    {
                        writer.WriteLine("\n\n");
                        writer.WriteLine("let dullBlades = [");
                        foreach (string bladeName in DullBlades)
                            writer.WriteLine("\t\"" + bladeName + "\",");
                        writer.WriteLine("];");
                    }
                }

                // process rates
                if (rates.Count > 0)
                {
                    using StreamWriter writer = new(pathRates);

                    writer.WriteLine("let rates = [];\n");
                    writer.WriteLine("// To customize this, the syntax is \"rates[x] = y\"");
                    writer.WriteLine("// where \"x\" is the star value and \"y\" is the pull rate (out of 100)");

                    foreach (KeyValuePair<int, int> ratePair in rates.Reverse())
                    {
                        int starValue = ratePair.Key;
                        int rate = ratePair.Value;
                        writer.WriteLine("rates[" + starValue + "] = " + rate + ";");
                    }

                    messages.Add("Profile saved successfully!");
                }
                else
                {
                    messages.Add("No rates found.");
                }
            }
            else
            {
                messages.Add("There was an error in the Character table data.");
            }

            if (sourceProfileOfImages != "")
            {
                // create directory if it doesn't exist
                string sourceDir = GetProfilePath(sourceProfileOfImages, "img");
                DirectoryInfo imgDir = new(sourceDir);

                string profileImgPath = GetProfilePath(selectedProfile, "img");
                Directory.CreateDirectory(profileImgPath);

                foreach (FileInfo file in imgDir.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    string fileToCopy = file.FullName.Substring(imgDir.FullName.Length + 1);
                    string relativePath = Path.GetDirectoryName(fileToCopy);
                    Directory.CreateDirectory(Path.Combine(profileImgPath, relativePath));
                    file.CopyTo(Path.Combine(profileImgPath, fileToCopy), true);
                }

            }

            if (messages.Count > 0)
                MessageBox.Show(string.Join("\n\n", messages), "Save status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public async Task DeleteProfile(string selectedProfile)
        {
            try
            {
                string profilePath = Path.Combine(profilesPath, selectedProfile);
                if (Directory.Exists(profilePath))
                {
                    Directory.Delete(profilePath, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public string[] CheckProfiles()
        {
            // get profile directories and populate "profiles" combobox
            string[] profiles = Directory.GetDirectories(profilesPath)
                    .Select(Path.GetFileName)
                    .ToArray();

            return profiles;
        }

        public bool ActivateProfile(string selectedProfile)
        {
            // save profile before?
            if (ReadPullSettingsFromFile(selectedProfile))
            {
                string sourceJsPath = GetProfilePath(selectedProfile, "js");
                string sourceImgPath = GetProfilePath(selectedProfile, "img");

                DirectoryInfo jsDir = new(sourceJsPath);
                DirectoryInfo imgDir = new(sourceImgPath);
                DirectoryInfo activeImgDir = new(imgPath);

                foreach (FileInfo file in jsDir.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    string fileToCopy = file.FullName.Substring(jsDir.FullName.Length + 1);
                    file.CopyTo(Path.Combine(jsPath, fileToCopy), true);
                }

                // 1. Delete all files in the root of this directory
                foreach (FileInfo file in activeImgDir.EnumerateFiles())
                {
                    file.Delete();
                }

                // 2. Delete all subdirectories and their contents recursively
                foreach (DirectoryInfo dir in activeImgDir.EnumerateDirectories())
                {
                    dir.Delete(true);
                }

                foreach (FileInfo file in imgDir.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    string fileToCopy = file.FullName.Substring(imgDir.FullName.Length + 1);
                    string dir = Path.GetDirectoryName(file.FullName)
                                    .Replace(sourceImgPath, "")
                                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                    .Replace('\\', '/');
                    Directory.CreateDirectory(Path.Combine(imgPath, dir));
                    file.CopyTo(Path.Combine(imgPath, fileToCopy), true);
                }

                return true;
            }
            return false;
        }

        private string GetProfilePath(string selectedProfile, string dir)
        {
            string path = Path.Combine(profilesPath, selectedProfile, dir);
            return path;
        }
        #endregion

        #region Pull Settings
        private bool ReadPullRatesFile(string selectedProfile)
        {
            string fileToRead = Path.Combine(GetProfilePath(selectedProfile, "js"), "rates.js");
            if (!File.Exists(fileToRead))
                return false;

            using (StreamReader sr = new(fileToRead))
            {
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine().Trim();
                    string searchTerm = "rates[";
                    int strpos = line.IndexOf(searchTerm);
                    if (strpos == 0)
                    {
                        string[] pair = line.Split('=');
                        string indexStr = pair[0].Remove(0, searchTerm.Length).Replace("]", "").Trim();
                        if (!int.TryParse(indexStr, out int index))
                            return false;

                        string rateStr = pair[1].Replace(";", "").Trim();
                        if (!int.TryParse(rateStr, out int rate))
                            return false;

                        StarListObject[index].PullRate = rate;
                    }
                }
            }
            return true;
        }

        private bool ReadCharacterListFile(string selectedProfile)
        {
            string profilePath = GetProfilePath(selectedProfile, "js");
            StarListObject = new();
            DullBlades = new();

            if (!File.Exists(Path.Combine(profilePath, "choices.js")))
                return false;

            using StreamReader sr = new(Path.Combine(profilePath, "choices.js"));
            int currentStarValue = 0;
            bool isInsideCharacterBracket = false;
            bool isInsideDullBladesBracket = false;
            List<CharacterElementPair> charElemPairList = new();

            while (sr.Peek() >= 0)
            {
                string line = sr.ReadLine().Trim();
                string starValueStringStart = "choices[";
                string starValueStringEnd = "];";
                string elementDictionaryStart = "let elementDictionary = {";
                string elementDictionaryEnd = "};";
                string dullBladesStart = "let dullBlades = [";
                string dullBladesEnd = "];";

                int starValueStringIndex = line.IndexOf(starValueStringStart);
                int starValueStringEndIndex = line.IndexOf(starValueStringEnd);
                int elementDictionaryStartIndex = line.IndexOf(elementDictionaryStart);
                int elementDictionaryEndIndex = line.IndexOf(elementDictionaryEnd);
                int dullBladesStartIndex = line.IndexOf(dullBladesStart);
                int dullBladesEndIndex = line.IndexOf(dullBladesEnd);

                if (starValueStringIndex >= 0)
                {
                    isInsideCharacterBracket = true;
                    string[] pair = line.Split('=');
                    string indexStr = pair[0].Remove(0, starValueStringStart.Length).Replace("]", "").Trim();
                    if (!int.TryParse(indexStr, out currentStarValue))
                        return false;

                    StarListObject.AddStar(currentStarValue);
                }
                else if ((starValueStringEndIndex >= 0) && (isInsideCharacterBracket))
                {
                    isInsideCharacterBracket = false;
                }
                else if (isInsideCharacterBracket)
                {
                    CharacterElementPair charElemPair = JsonConvert.DeserializeObject<CharacterElementPair>(line.Trim(','));
                    charElemPairList.Add(charElemPair);
                    StarListObject[currentStarValue].Add(charElemPair.Name);
                    StarListObject[currentStarValue][charElemPair.Name].Star = currentStarValue;
                    StarListObject[charElemPair.Name].Element = charElemPair.Element;
                }
                else if (dullBladesStartIndex >= 0)
                {
                    isInsideDullBladesBracket = true;
                }
                else if ((dullBladesEndIndex >= 0) && (isInsideDullBladesBracket))
                {
                    currentStarValue++;
                    isInsideDullBladesBracket = false;
                }
                else if (isInsideDullBladesBracket)
                {
                    string dullBladeName = line.Replace("\"", "").Replace("\'", "").Replace(",", "").Trim();

                    DullBlades.Add(dullBladeName);
                }
            }
            return true;
        }

        public bool ValidateOrRepairPullSettings(string selectedProfile)
        {
            // check the JS files if they exist
            // TO-DO: also check if valid
            bool filesAreValid = ReadPullSettingsFromFile(selectedProfile);
            if (!filesAreValid)
            {
                List<string> filesToCheck = new()
                {
                    "rates.js",
                    "choices.js",
                };
                List<string> filesNotFound = new();

                foreach (string toCheck in filesToCheck)
                {
                    if (!File.Exists(Path.Combine(GetProfilePath(selectedProfile, "js"), toCheck)))
                    {
                        filesNotFound.Add(toCheck);
                        filesAreValid = false;

                        // copy local config from "defaults" directory if it exists
                        string defaultConfigToCopy = Path.Combine(GetProfilePath("default", "js"), toCheck);
                        string destinationPath = Path.Combine(GetProfilePath(selectedProfile, "js"), toCheck);

                        if (File.Exists(defaultConfigToCopy))
                        {
                            File.Copy(defaultConfigToCopy, destinationPath);
                            filesAreValid = true;
                        }
                    }
                }
                MessageBox.Show(
                    $"The following files are missing from the selected profile: \n  - {String.Join("\n  - ", filesNotFound.ToArray())}\n\nThese files have been restored from the default profile.",
                    "Files missing",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                    );
            }

            return filesAreValid;
        }

        private bool ReadPullSettingsFromFile(string selectedProfile)
        {
            bool first = ReadCharacterListFile(selectedProfile);
            bool second = ReadPullRatesFile(selectedProfile);
            if (ReadCharacterListFile(selectedProfile) && ReadPullRatesFile(selectedProfile))
                return true;

            return false;
        }
        #endregion

        #region Configs
        public int GetDefaultCharacterListVersion()
        {
            int currentVersion = 0;
            // copy local config from "defaults" directory if it exists
            string defaultConfigToCopy = Path.Combine(profilesPath, "default", "js", "choices.js");

            using (StreamReader reader = new(defaultConfigToCopy))
            {
                string firstLine = reader.ReadLine();
                if (firstLine.StartsWith("// version:"))
                    int.TryParse(firstLine.Replace("// version: ", ""), out currentVersion);
            }
            return currentVersion;
        }

        public async Task<(bool httpOk, int latestCharacterListVersion)> CheckCharacterListUpdate()
        {
            string choicesUrl = "https://raw.githubusercontent.com/honganqi/GenshinWishOnStream/main/browser_source/profiles/default/js/choices.js";

            var request = Interwebs.httpClient.GetAsync(choicesUrl);

            Task timeout = Task.Delay(3000);
            await Task.WhenAny(timeout, request);

            try
            {
                System.Net.Http.HttpResponseMessage response = request.Result;
                if (response.IsSuccessStatusCode)
                {
                    var page = response.Content.ReadAsStringAsync();
                    using var reader = new StringReader(page.Result);
                    string choicesVersion = reader.ReadLine();
                    if (choicesVersion.StartsWith("// version:"))
                    {
                        if (int.TryParse(choicesVersion.Replace("// version: ", ""), out int latestCharacterListVersion))
                            return (true, latestCharacterListVersion);
                    }
                    else
                    {
                        return (true, 0);
                    }
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                        throw new Exception("The update file was not found in the Genshin Wisher repository.");
                }
            }
            catch (HttpRequestException)
            {
                throw new HttpRequestException("Could not connect to check the latest character list version.");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }

            return (false, 0);
        }

        public async Task<bool> DownloadDefaultConfigs(List<string> items)
        {
            bool success = false;
            string baseUrl = "https://raw.githubusercontent.com/honganqi/GenshinWishOnStream/refs/heads/main/browser_source/profiles/default/js/";

            foreach (string item in items)
            {
                try
                {
                    string url = Path.Combine(baseUrl, item);
                    string file = await Interwebs.httpClient.GetStringAsync(url);
                    string saveTo = Path.Combine(GetProfilePath("default", "js"), item);
                    File.WriteAllText(saveTo, file);
                    success = true;
                }
                catch (HttpRequestException)
                {
                    throw new Exception($"Unable to download default profile configs.\n\nUnable to connect to {baseUrl}.");
                }
            }
            ReadPullSettingsFromFile("default"); // checks *.js files, updates characters/dullblades panel
            return success;
        }

        public async Task<List<string>> DownloadDefaultImages_Prefetch(bool fullDownload = true)
        {
            List<string> paths = new();
            long totalSize = 0;
            try
            {
                string treeUrl = $"https://api.github.com/repos/honganqi/GenshinWishOnStream/git/trees/main?recursive=1";
                Interwebs.httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GenshinWisherGUI/" + Application.ProductVersion);
                string treeJson = await Interwebs.httpClient.GetStringAsync(treeUrl);
                using var tree = JsonDocument.Parse(treeJson);
                paths = new();
                totalSize = 0;
                List<string> existingFiles = [];
                string checkPath = GetProfilePath("default", "img");

                // if just checking for an update, don't do full download
                // instead, check for existing files, add them to the exclusion list
                if (!fullDownload)
                {
                    foreach (string path in Directory.EnumerateFiles(checkPath, "*.*", SearchOption.AllDirectories))
                    {
                        if (File.Exists(path))
                        {
                            string relativePath = path.Replace(checkPath, "")
                                                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                                                    .Replace('\\', '/');
                            existingFiles.Add(relativePath);
                        }
                    }
                }

                foreach (var entree in tree.RootElement
                         .GetProperty("tree")
                         .EnumerateArray())
                {
                    if (entree.GetProperty("type").GetString() != "blob")
                        continue;

                    string path = entree.GetProperty("path").GetString()!;
                    string targetPath = "browser_source/profiles/default/img/";
                    if (!path.StartsWith(targetPath))
                        continue;

                    // check if the file is excluded
                    string newCheckPath = path.Replace(targetPath, "");
                    if (!existingFiles.Contains(newCheckPath))
                    {
                        paths.Add(path);
                        int fileSize = entree.GetProperty("size").GetInt32();
                        totalSize += fileSize;
                    }
                }

                if (totalSize > 0 && fullDownload)
                {
                    DialogResult ask = MessageBox.Show($"Image count: {paths.Count}\nTotal size: {FormatFileSize(totalSize)}\n\nContinue?", "Confirm download", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (ask != DialogResult.Yes)
                        throw new Exception("Exiting");
                }
                else
                {
                    if (fullDownload)
                        MessageBox.Show("No new images found for download. All images are updated so far.", "Images still updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (HttpRequestException)
            {
                throw new Exception("Unable to download default profile images.");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return paths;
        }

        public async Task<bool> DownloadDefaultImages(List<string> paths, IProgress<DownloadProgress> progress = null)
        {
            bool success = false;
            try
            {
                int downloaded = 0;

                progress?.Report(new DownloadProgress
                {
                    FilesDownloaded = downloaded,
                    FileCount = paths.Count
                });

                foreach (string path in paths)
                {
                    string downloadUrl = $"https://raw.githubusercontent.com/honganqi/GenshinWishOnStream/main/{path}";

                    // store path and preserve source paths but strip the git stuff
                    string localPath = Path.Combine(GetProfilePath("default", "img"), path.Replace("browser_source/profiles/default/img/", ""));

                    // create the directory if needed
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath));

                    // download directly to disk
                    using var response = await Interwebs.httpClient.GetAsync(downloadUrl);
                    response.EnsureSuccessStatusCode();
                    // var statusCode = response.StatusCode // if needed, lazy+tired now so pass
                    using Stream remoteStream = await response.Content.ReadAsStreamAsync();
                    using FileStream localStream = File.Create(localPath);
                    await remoteStream.CopyToAsync(localStream);

                    downloaded++;

                    progress?.Report(new DownloadProgress
                    {
                        FilesDownloaded = downloaded,
                        FileCount = paths.Count
                    });
                }
            }
            catch (HttpRequestException)
            {
                throw new Exception($"Unable to download default images.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return success;
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes == 0)
                return "0 bytes";

            string[] suffixes = { "B", "KB", "MB", "GB" };

            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));

            // limit max to GB, I don't think there will be more than 7,000,000,000,000 Genshin characters which is the only thing this app will be downloading
            place = Math.Min(place, suffixes.Length - 1);

            double num = Math.Round(bytes / Math.Pow(1024, place), 1);

            return $"{num:0.#} {suffixes[place]}";
        }
        #endregion

        #region User Info
        public async Task ValidateUserSettingsFromFiles()
        {
            // check for local_creds.js file, create it if it doesn't exist
            if (!File.Exists(Path.Combine(jsPath, "local_creds.js")))
                SaveUserSettingsToFile(revoke: true);

            // read and validate user settings, will set the userInfo variable if successful
            ReadUserSettingsFromFile();
        }

        private UserInfo ReadUserSettingsFromFile()
        {
            UserInfo _userInfo = new();

            Dictionary<string, string> userSettingsContents = new();
            string pattern = @"var\s+(\w+)\s*=\s*(?:""([^""]*)""|'([^'\\]*(?:\\'[^'\\]*)*)'|(\d+|true|false))\s*;";
            Regex regex = new(pattern);

            List<string> searchTerms = new()
            {
                "channelName",
                "channelID",
                "localToken",
                "redeemTitle",
                "redeemEnabled",
                "twitchCommandPrefix",
                "twitchCommandEnabled",
                "animation_duration"
            };

            using (StreamReader sr = new(Path.Combine(jsPath, "local_creds.js")))
            {
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine().Trim();
                    MatchCollection matches = regex.Matches(line);
                    foreach (Match match in matches)
                    {
                        // verify property exists in the list of allowed properties
                        string property = match.Groups[1].Value;
                        if (searchTerms.Contains(property))
                        {
                            string stringValue = match.Groups[2].Value;
                            string singleQuotedValue = match.Groups[3].Value;
                            string nonQuotedValue = match.Groups[4].Value;
                            string value;

                            if (!string.IsNullOrEmpty(stringValue))
                                value = stringValue; // Double-quoted string value
                            else if (!string.IsNullOrEmpty(singleQuotedValue))
                                value = singleQuotedValue.Replace(@"\'", @"'"); // Single-quoted string value including escaped single quotes
                            else
                                value = nonQuotedValue; // Numeric or boolean value
                            userSettingsContents[property] = value;
                        }
                    }
                }
            }

            if (userSettingsContents.Count > 0)
            {
                string name = userSettingsContents.ContainsKey("channelName") ? userSettingsContents["channelName"] : "";
                string id = userSettingsContents.ContainsKey("channelID") ? userSettingsContents["channelID"] : "";
                UserInfoObject.NewUser(name, id);
                UserInfoObject.Token = userSettingsContents.ContainsKey("localToken") ? userSettingsContents["localToken"] : "";
                UserInfoObject.Redeem = userSettingsContents.ContainsKey("redeemTitle") ? userSettingsContents["redeemTitle"] : "";
                UserInfoObject.RedeemEnabled = (userSettingsContents.ContainsKey("redeemEnabled") && bool.Parse(userSettingsContents["redeemEnabled"])) ? bool.Parse(userSettingsContents["redeemEnabled"]) : false;
                UserInfoObject.TwitchCommandPrefix = userSettingsContents.ContainsKey("twitchCommandPrefix") ? userSettingsContents["twitchCommandPrefix"] : "";
                UserInfoObject.TwitchCommandEnabled = (userSettingsContents.ContainsKey("twitchCommandEnabled") && bool.Parse(userSettingsContents["twitchCommandEnabled"])) ? bool.Parse(userSettingsContents["twitchCommandEnabled"]) : false;
                if (!userSettingsContents.ContainsKey("redeemEnabled") && UserInfoObject.Redeem != "")
                    UserInfoObject.RedeemEnabled = true;
                if (int.TryParse(userSettingsContents["animation_duration"], out int duration))
                    UserInfoObject.Duration = duration;

                _userInfo = UserInfoObject;
            }

            return _userInfo;
        }

        public void SaveUserSettingsToFile(bool revoke = false, bool fromExpiredToken = false)
        {
            string pathSettings = Path.Combine(jsPath, "local_creds.js");
            string errors = "";
            bool freshCredsFile = false;

            if (!File.Exists(pathSettings))
            {
                errors += "The \"local_creds.js\" file was not found in the \"js\" folder.\nOne will be created for you.\n\nAfterwards, please use the \"Connect to Twitch\" button to authenticate to allow you to select the Channel Point Reward and/or the chat command to use.\n\n";
                freshCredsFile = true;
            }


            if (!revoke)
            {
                if (UserInfoObject.Name == "")
                    errors += " - User is not set. Please connect using the Twitch button.\n";
                if (UserInfoObject.Redeem == "")
                    errors += " - The Channel Points Reward is not set. Please set this or make sure you have access to Twitch channel point rewards (Twitch Affiiate, etc.).\n\n";
            }
            else
            {
                UserInfoObject = new();
            }

            if (errors == "")
            {
                if (!fromExpiredToken)
                {
                    if (!revoke)
                    {
                        if (!UserInfoObject.RedeemEnabled && !UserInfoObject.TwitchCommandEnabled && !freshCredsFile)
                            errors += "You have not selected any way for your viewers to wish. Settings saved anyway.";
                        else
                            errors = "User settings saved successfully!";
                    }
                }
                using StreamWriter writer = new(pathSettings);
                writer.WriteLine("var channelName = \'" + UserInfoObject.Name + "\';");
                writer.WriteLine("var channelID = \'" + UserInfoObject.ID + "\';");
                writer.WriteLine("var localToken = \'" + UserInfoObject.Token.Replace("'", @"\'") + "\';");
                writer.WriteLine("var redeemTitle = \'" + UserInfoObject.Redeem + "\';");
                writer.WriteLine("var redeemEnabled = " + (UserInfoObject.RedeemEnabled ? "true" : "false") + ";");
                writer.WriteLine("var twitchCommandPrefix = \'" + UserInfoObject.TwitchCommandPrefix.Replace("'", @"\'") + "\';");
                writer.WriteLine("var twitchCommandEnabled = " + (UserInfoObject.TwitchCommandEnabled ? "true" : "false") + ";");
                writer.WriteLine("var animation_duration = " + UserInfoObject.Duration + ";");
            }

            if (errors != "")
                MessageBox.Show(errors, "Save user", MessageBoxButtons.OK);
        }

        public void RewriteLocalCreds(HttpServer.AuthPayload payload)
        {
            string pathSettings = Path.Combine("js", "local_creds.js");
            string js = File.ReadAllText(pathSettings);

            js = ReplaceVar(js, "channelName", $"'{payload.ChannelName}'");
            js = ReplaceVar(js, "channelID", $"'{payload.ChannelId}'");
            js = ReplaceVar(js, "localToken", $"'{payload.Token.Replace("'", @"\'")}'");

            File.WriteAllText(pathSettings, js);
        }

        string ReplaceVar(string source, string name, string value)
        {
            var pattern = $@"var\s+{name}\s*=\s*.*?;";
            return Regex.Replace(source, pattern, $"var {name} = {value};");
        }
        #endregion
    }

    public class DownloadProgress
    {
        public int FileCount { get; set; }
        public int FilesDownloaded { get; set; }

        public int Percent
        {
            get
            {
                if (FileCount == 0)
                    return 0;

                return (int)(FilesDownloaded * 100 / FileCount);
            }
        }
    }

    public class Character
    {
        public string CharacterName { get; set; }
        public string Element { get; set; }
        public int Star { get; set; }
    }

    public class CharacterListInStar : List<Character>
    {
        public Character this[string name]
        {
            get
            {
                Character character = FindCharacterByName(name);
                return character;
            }
        }
        public int StarValue { get; set; }
        public int PullRate { get; set; }

        public void Add(string name)
        {
            Character character = FindCharacterByName(name);
            if (character == null)
            {
                character = new();
                character.CharacterName = name;
                Add(character);
            }
        }
        public void SortList()
        {
            _characters = this.OrderBy(c => c.CharacterName).ToList();
            Clear();
            foreach (Character character in _characters)
                Add(character);
        }
        private Character FindCharacterByName(string characterName)
        {
            return this.FirstOrDefault(c => c.CharacterName == characterName);
        }

        private List<Character> _characters = new();
    }

    public class StarList : SortedDictionary<int, CharacterListInStar>
    {
        public new CharacterListInStar this[int starValue]
        {
            get
            {
                CharacterListInStar charList = FindCharList(starValue);
                if (charList != null)
                    return charList;
                else
                    throw new Exception("Unable to find star collection");
            }
        }
        public Character this[string characterName]
        {
            get
            {
                Character character = new();

                foreach (KeyValuePair<int, CharacterListInStar> charList in this)
                {
                    CharacterListInStar characters = charList.Value;
                    Character thischar = characters[characterName];
                    if (thischar != null)
                    {
                        character = thischar;
                        break;
                    }
                }
                return character;
            }
        }
        public void AddStar(int starValue)
        {
            if (!ContainsKey(starValue))
            {
                CharacterListInStar charList = new();
                charList.StarValue = starValue;
                Add(starValue, charList);
            }
            _starList = this;
        }

        private CharacterListInStar FindCharList(int starValue)
        {
            return _starList[starValue];
        }
        private SortedDictionary<int, CharacterListInStar> _starList = new();
    }

    public static class Interwebs
    {
        public static readonly HttpClient httpClient = new();
    }

    class Images
    {
        public static Bitmap Load(string imagename)
        {
            Assembly _assembly = Assembly.GetExecutingAssembly();
            Stream stream = _assembly.GetManifestResourceStream("GenshinImpact_WishOnStreamGUI.img." + imagename + ".png");
            Bitmap _bitmap = new(stream);
            return _bitmap;
        }
    }

    // thanks to Nick Charlton
    // https://thoughtbot.com/blog/using-httplistener-to-build-a-http-server-in-csharp
    public class HttpServer
    {
        public const string localhostAddress = "http://localhost:8275/";
        public event Action<AuthPayload> AuthCompleted;
        public event Action<AuthPayload> RewriteLocalCreds;
        private HttpListener _listener;

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(localhostAddress);
            _listener.Start();
            _listener.BeginGetContext(ListenerCallback, null);
        }

        public void Stop()
        {
            if (_listener?.IsListening == true)
            {
                _listener.Stop();
                _listener.Close();
            }
        }

        // thanks to https://www.codeproject.com/Tips/485182/Create-a-local-server-in-Csharp
        private void ListenerCallback(IAsyncResult result)
        {
            if (!_listener.IsListening)
                return;

            HttpListenerContext context = _listener.EndGetContext(result);

            // add CORS headers to allow the browser to send from the remote backend to localhost
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

            // handle CORS preflight
            if (context.Request.HttpMethod == "OPTIONS")
            {
                context.Response.StatusCode = 204;
                context.Response.Close();
                _listener.BeginGetContext(ListenerCallback, null);
                return;
            }

            // only accept POST from /update-creds
            if (context.Request.HttpMethod != "POST" || context.Request.Url.AbsolutePath != "/update-creds")
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                _listener.BeginGetContext(ListenerCallback, null);
                return;
            }

            using StreamReader reader = new(context.Request.InputStream, context.Request.ContentEncoding);
            string body = reader.ReadToEnd();
            JObject jsonObj = JObject.Parse(body);

            AuthPayload payload = new()
            {
                ChannelName = jsonObj["channel_name"]?.ToString(),
                ChannelId = jsonObj["channel_id"]?.ToString(),
                BroadcasterType = jsonObj["broadcaster_type"]?.ToString(),
                Token = jsonObj["access_token"]?.ToString(),
                Redeems = jsonObj["redeems"] != null
                    ? jsonObj["redeems"].ToObject<List<string>>()
                    : []
            };

            // rewrite local_creds.js file with payload
            RewriteLocalCreds?.Invoke(payload);

            // notify listeners
            AuthCompleted?.Invoke(payload);

            byte[] response = Encoding.UTF8.GetBytes("OK");
            context.Response.ContentType = "text/plain";
            context.Response.ContentLength64 = response.Length;
            context.Response.OutputStream.Write(response, 0, response.Length);
            context.Response.Close();

            Stop();
        }



        public class AuthPayload
        {
            public string ChannelName { get; set; }
            public string ChannelId { get; set; }
            public string BroadcasterType { get; set; }
            public string Token { get; set; }
            public List<string> Redeems { get; set; }
        }
    }

    public class UserInfo
    {
        string _name;
        string _id;
        string _token;
        string _redeem;
        bool _redeemEnabled;
        int _duration;
        string _twitchCommandPrefix;
        bool _twitchCommandEnabled;
        string _broadcasterType;
        List<string> _rewards;

        public UserInfo()
        {
            _name = "";
            _id = "";
            _token = "";
            _redeem = "";
            _redeemEnabled = false;
            _duration = 8000;
            _twitchCommandPrefix = "";
            _twitchCommandEnabled = false;
            _broadcasterType = "";
            _rewards = [];
        }
        public string Name => _name;
        public string ID => _id;
        public string Redeem { get => _redeem; set => _redeem = value; }
        public bool RedeemEnabled { get => _redeemEnabled; set => _redeemEnabled = value; }
        public int Duration { get => _duration; set => _duration = value; }
        public string TwitchCommandPrefix { get => _twitchCommandPrefix; set => _twitchCommandPrefix = value; }
        public bool TwitchCommandEnabled { get => _twitchCommandEnabled; set => _twitchCommandEnabled = value; }
        public string BroadcasterType { get => _broadcasterType; set => _broadcasterType = value; }
        public string Token { get => _token; set => _token = value; }
        public List<string> Rewards { get => _rewards; set => _rewards = value;  }
        public void NewUser(string name, string id)
        {
            _name = name;
            _id = id;
            _token = "";
            _redeem = "";
            _redeemEnabled = false;
            _duration = 8000;
            _twitchCommandPrefix = "";
            _twitchCommandEnabled = false;
            _broadcasterType = "";
            _rewards = [];
        }
        public async Task<List<string>> GetCustomRewards()
        {
            List<string> rewards = [];

            Task timeout = Task.Delay(3000);
            string url = $"https://genshin-twitch.sidestreamnetwork.net/api/redemptions?broadcaster_id={_id}";
            Task<HttpResponseMessage> request = Interwebs.httpClient.GetAsync(url);
            await Task.WhenAny(timeout, request);

            HttpResponseMessage response = request.Result;
            if (response.IsSuccessStatusCode)
            {
                string page = await response.Content.ReadAsStringAsync();
                rewards = JsonConvert.DeserializeObject<List<string>>(page);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException($"Unable to get Twitch rewards: User token has expired.\nPlease use the \"Connect to Twitch\" button to reauthorize.");
            }
            else
            {
                string page = await response.Content.ReadAsStringAsync();
                var data = JsonNode.Parse(page);
                string error = (string)data["error"];
                throw new Exception($"Unable to get Twitch rewards:\n{error}");
            }

            Rewards = rewards;
            return rewards;
        }

        public void UpdateUserInfoFromAuthPayload(HttpServer.AuthPayload payload)
        {
            // fetch existing/old variables
            bool _redeemEnabled = RedeemEnabled;
            string _redeem = Redeem;
            int _duration = Duration;
            string _twitchCommandPrefix = TwitchCommandPrefix;
            bool _twitchCommandEnabled = TwitchCommandEnabled;

            // reset the user from the payload
            NewUser(name: payload.ChannelName, id: payload.ChannelId);
            Token = payload.Token;
            Rewards = payload.Redeems;

            // set the old variables back
            Redeem = _redeem;
            RedeemEnabled = _redeemEnabled;
            Duration = _duration;
            TwitchCommandPrefix = _twitchCommandPrefix;
            TwitchCommandEnabled = _twitchCommandEnabled;
        }
    }

    #region JSON classes
    class VersionClass
    {
        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("download_url")]
        public string DownloadURL { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }
    class CharacterElementPair
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("element")]
        public string Element { get; set; }
    }
    #endregion

}
