using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenshinImpact_WishOnStreamGUI
{
    public partial class MainWindow : Form
    {
        public Assembly imageAssembly = Assembly.GetExecutingAssembly();

        public bool mouseDown;
        public Point lastLocation;

        SortedDictionary<int, int> rates = new();
        StarList starList = new();
        List<string> dullBlades = new();

        string jsPath = Path.Combine(Application.StartupPath, "js");
        string imgPath = Path.Combine(Application.StartupPath, "img");
        string activeProfile;
        int currentChoicesVersion;

        public bool updateAvailable = false;
        readonly string updateurl = "https://raw.githubusercontent.com/honganqi/GenshinWishOnStream/main/latest.json";
        string downloadURL = "";
        HttpServer httpServer = new();
        readonly string authUrl = "https://genshin-twitch.sidestreamnetwork.net/auth";
        UserInfo userInfo;


        // Characters "Panel" variables
        const int columnWidth = 140;
        const int elementColumnWidth = 100;
        const int columnMargin = 25;
        const int columnSplitter = 10;
        const int rowHeight = 30;
        const int initYPos = 50;
        const int initXPos = 20;
        const int extraRows = 5;

        int lastItemY = 0;
        int currentMax = -1;
        int currentMin = -1;
        int lastItemYDullBlade = 0;


        public MainWindow()
        {
            InitializeComponent();

            ControlBox = false;
            Text = string.Empty;

            imgBMCSupport.Image = Images.Load("bmc");
            imgSF.Image = Images.Load("github");
            imgYoutube.Image = Images.Load("youtube");
            imgTwitch.Image = Images.Load("twitch");
            btnClose.BackgroundImage = Images.Load("exit");
            btnMinimize.BackgroundImage = Images.Load("min");
            btnMaximize.BackgroundImage = Images.Load("max");
            btnPanelSettings.BackgroundImage = Images.Load("settings");
            btnSave.BackgroundImage = Images.Load("save");
            btnCheck.BackgroundImage = Images.Load("check");
            btnPanelCharacters.BackgroundImage = Images.Load("character");
            btnPanelDullBlades.BackgroundImage = Images.Load("dullblade");
            imgTwitchConnect.Image = Images.Load("twitch_connect");

            // set version number on titlebar
            string currentVerString = Application.ProductVersion;
            List<string> currentVersionSplit = currentVerString.Split('.').ToList();
            if (currentVersionSplit[3] == "0") currentVersionSplit.RemoveAt(3);
            if (currentVersionSplit[2] == "0") currentVersionSplit.RemoveAt(2);
            labelVerNum.Text = "v" + string.Join(".", currentVersionSplit) + " by honganqi";

            // set window state
            if (Properties.Settings.Default.windowState == "Maximized")
            {
                WindowState = FormWindowState.Maximized;
                btnMaximize.BackgroundImage = Images.Load("restore");
            }

            // set active profile
            if (
                Properties.Settings.Default.PropertyValues["activeProfile"] == null ||
                Properties.Settings.Default.activeProfile == "" ||
                !Directory.Exists(Path.Combine(jsPath, "profiles", Properties.Settings.Default.activeProfile))
                )
                Properties.Settings.Default.activeProfile = "default";
            activeProfile = Properties.Settings.Default.activeProfile;
        }

        private void SwitchPanel(Panel panelname)
        {
            if (userInfo.Name != "" || panelname == panelSettings)
                panelname.BringToFront();
        }

        private void SaveWindowSettings()
        {
            Properties.Settings.Default.windowSize = Size;
            Properties.Settings.Default.windowStartLocation = Location;
            Properties.Settings.Default.windowState = WindowState.ToString();
            Properties.Settings.Default.activeProfile = cmbProfiles.Text;
            Properties.Settings.Default.Save();
        }

        public void ResetUser(bool fromExpiredToken = false)
        {
            if (InvokeRequired)
            {
                Invoke(() => ResetUser(fromExpiredToken));
                return;
            }
            imgTwitchConnect.Visible = true;
            btnCopyAuthLink.Visible = true;
            btnRevokeToken.Enabled = false;
            btnUpdateRewards.Enabled = false;
            chkRedeems.Enabled = false;
            chkCommand.Enabled = false;
            txtCommand.Text = "";
            txtCommand.Enabled = false;
            cmbRedeems.Items.Clear();
            SwitchPanel(panelSettings);
        }

        public void UpdateSettingsRewards()
        {
            labelPullRewards.Visible = true;

            Task.Run(async () =>
            {
                try
                {
                    userInfo.Rewards = await userInfo.GetCustomRewards();
                    BeginInvoke(new Action(() =>
                    {
                        cmbRedeems.Enabled = userInfo.RedeemEnabled;
                        SetRewards();
                        cmbRedeems.SelectedItem = userInfo.Redeem;
                    }));
                }
                catch (UnauthorizedAccessException ex)
                {
                    ResetUser(fromExpiredToken: true);
                    lblTokenExpired.Invoke(new Action(() =>
                    {
                        lblTokenExpired.Text = ex.Message;
                        lblTokenExpired.Visible = true;
                    }));
                }
                catch (Exception ex)
                {
                    lblTokenExpired.Text = ex.Message;
                    lblTokenExpired.Visible = true;
                }
                finally
                {
                    BeginInvoke(new Action(() =>
                    {
                        labelPullRewards.Visible = false;
                    }));
                }
            });
        }

        public void DisplayConnectionErrors(List<string> errors)
        {
            if (errors.Count > 0)
                MessageBox.Show(string.Join("\n", errors), "Unable to connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        #region Fetchers
        private async Task ReadUserSettingsFromFile()
        {
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

            using (StreamReader sr = new(Path.Combine("js", "local_creds.js")))
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
                userInfo = new(name, id);
                userInfo.Token = userSettingsContents.ContainsKey("localToken") ? userSettingsContents["localToken"] : "";
                userInfo.Redeem = userSettingsContents.ContainsKey("redeemTitle") ? userSettingsContents["redeemTitle"] : "";
                userInfo.RedeemEnabled = (userSettingsContents.ContainsKey("redeemEnabled") && bool.Parse(userSettingsContents["redeemEnabled"])) ? bool.Parse(userSettingsContents["redeemEnabled"]) : false;
                userInfo.TwitchCommandPrefix = userSettingsContents.ContainsKey("twitchCommandPrefix") ? userSettingsContents["twitchCommandPrefix"] : "";
                userInfo.TwitchCommandEnabled = (userSettingsContents.ContainsKey("twitchCommandEnabled") && bool.Parse(userSettingsContents["twitchCommandEnabled"])) ? bool.Parse(userSettingsContents["twitchCommandEnabled"]) : false;
                if (!userSettingsContents.ContainsKey("redeemEnabled") && userInfo.Redeem != "")
                    userInfo.RedeemEnabled = true;
                if (int.TryParse(userSettingsContents["animation_duration"], out int duration))
                    userInfo.Duration = duration;

                await UpdateUIPanelSettingsWithUserInfo(userInfo);
            }
        }
        private bool GetRates()
        {
            string profilePath = Path.Combine(jsPath, "profiles", activeProfile);
            if (!File.Exists(Path.Combine(profilePath, "rates.js")))
            {
                MessageBox.Show($"Could not find the file {Path.Combine(profilePath, "rates.js")}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            using (StreamReader sr = new(Path.Combine(profilePath, "rates.js")))
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

                        starList[index].PullRate = rate;
                        if (!rates.ContainsKey(index))
                            rates.Add(index, rate);
                    }
                }
            }
            return true;
        }

        private bool GetChoices()
        {
            string profilePath = Path.Combine(jsPath, "profiles", activeProfile);
            starList = new();
            if (!File.Exists(Path.Combine(profilePath, "choices.js")))
            {
                MessageBox.Show($"Could not find {Path.Combine(profilePath, "choices.js")}");
                return false;
            }
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

                    starList.AddStar(currentStarValue);
                }
                else if ((starValueStringEndIndex >= 0) && (isInsideCharacterBracket))
                {
                    isInsideCharacterBracket = false;
                }
                else if (isInsideCharacterBracket)
                {
                    CharacterElementPair charElemPair = JsonConvert.DeserializeObject<CharacterElementPair>(line.Trim(','));
                    charElemPairList.Add(charElemPair);
                    starList[currentStarValue].Add(charElemPair.Name);
                    starList[currentStarValue][charElemPair.Name].Star = currentStarValue;
                    starList[charElemPair.Name].Element = charElemPair.Element;
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

                    dullBlades.Add(dullBladeName);
                }
            }
            return true;
        }

        public string FormatFileSize(long bytes)
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


        #region File Read-Write
        public async Task CheckPullSettings()
        {
            List<string> filesToCheck = new()
            {
                "rates.js",
                "choices.js",
            };
            List<string> filesNotFound = new();

            foreach (string toCheck in filesToCheck)
            {
                if (!File.Exists(Path.Combine(jsPath, "profiles", activeProfile, toCheck)))
                {
                    filesNotFound.Add(toCheck);

                    // copy local config from "defaults" directory if it exists
                    string defaultConfigToCopy = Path.Combine(jsPath, "profiles", "default", toCheck);
                    string destinationPath = Path.Combine(jsPath, toCheck);

                    if (File.Exists(defaultConfigToCopy))
                    {
                        File.Copy(defaultConfigToCopy, destinationPath);
                        filesNotFound.Remove(toCheck);
                    }
                }
            }

            // download defaults if both active and local default configs don't exist
            if (filesNotFound.Count > 0) {
                await DownloadDefaultConfigs(filesNotFound);
            }

            List<string> errors = new();
            if (!GetChoices())
                errors.Add("choices.js");
            if (!GetRates())
                errors.Add("rates.js");
            if (errors.Count > 0)
            {
                activeProfile = "default";
                cmbProfiles.SelectedIndex = cmbProfiles.FindStringExact(activeProfile);
                MessageBox.Show("There were errors reading the following files:\n\n   - " + string.Join("\n   - ", errors) + "\n\nThese are probably syntax errors. Kindly check your files or download the JS files again.");
            }
            else
            {
                InitializeCharactersPanel(starList);
                InitializeDullBladesPanel(dullBlades);
            }
        }

        public async Task CheckUserSettings()
        {
            // check for local_creds.js file, create it if it doesn't exist
            if (!File.Exists(Path.Combine(jsPath, "local_creds.js")))
                await SaveUserSettingsToFile(userInfo: new(), revoke: true);
        }

        public async Task ValidateUserSettingsFromFiles()
        {
            // check for local_creds.js file, create it if it doesn't exist
            if (!File.Exists(Path.Combine(jsPath, "local_creds.js")))
                await SaveUserSettingsToFile(userInfo: new(), revoke: true);

            // read and validate user settings, will set the userInfo variable if successful
            await ReadUserSettingsFromFile();

            if (userInfo.Name != "")
                TogglePanelVisibility(true);

            Properties.Settings.Default.Save();
        }

        private bool ReadPullSettingsFromFile()
        {
            List<string> errors = new();
            if (!GetChoices())
                errors.Add("choices.js");
            if (!GetRates())
                errors.Add("rates.js");
            if (errors.Count > 0)
                MessageBox.Show("There were errors reading the following files:\n\n   - " + string.Join("\n   - ", errors) + "\n\nThese are probably syntax errors. Kindly check your files or download the JS files again.");
            else
            {
                InitializeCharactersPanel(starList);
                InitializeDullBladesPanel(dullBlades);
                return true;
            }
            return false;
        }

        public void SetProfileAsActive(string profile)
        {
            List<string> filesToCopy = new()
            {
                "rates.js",
                "choices.js",
            };

            foreach (string file in filesToCopy)
            {
                string filePath = Path.Combine(jsPath, "profiles", profile, file);
                string destinationPath = Path.Combine(jsPath, file);
                if (File.Exists(filePath))
                    File.Copy(filePath, destinationPath, true);
            }
            Properties.Settings.Default.activeProfile = profile;
        }

        public async Task SaveAllSettingsToFile()
        {
            List<string> messages = new();

            SaveProfile();
            SetProfileAsActive(cmbProfiles.Text);

            ExtractUserInfo(ref userInfo);
            string credMessages = await SaveUserSettingsToFile(userInfo);
            messages.Add(credMessages);

            if (messages.Count > 0)
                MessageBox.Show(string.Join("\n\n", messages), "Save status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public async Task<string> SaveUserSettingsToFile(UserInfo userInfo, bool revoke = false, bool fromExpiredToken = false)
        {
            string pathSettings = Path.Combine("js", "local_creds.js");
            string errors = "";
            bool freshCredsFile = false;

            if (!File.Exists(pathSettings))
            {
                MessageBox.Show("The \"local_creds.js\" file was not found in the \"js\" folder.\nOne will be created for you.\n\nPlease use the \"Connect to Twitch\" button to authenticate to allow you to select the Channel Point Reward and/or the chat command to use.");
                freshCredsFile = true;
            }
                

            if (!revoke)
            {
                if (userInfo.Name == "")
                    errors += " - Username was blank. Please connect using the Twitch button.\n";
                if (userInfo.Redeem == "")
                    errors += " - The Channel Point Redeem is not set. Please set this or make sure you have access to Twitch channel point rewards (Twitch Affiiate, etc.).";
            }
            else
            {
                userInfo = new();
            }

            if (errors == "")
            {
                await UpdateUIPanelSettingsWithUserInfo(userInfo);
                if (!fromExpiredToken)
                {
                    if (!userInfo.RedeemEnabled && !userInfo.TwitchCommandEnabled && !freshCredsFile)
                        MessageBox.Show("You have not selected any way for your viewers to wish. Settings saved anyway.", "No option selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    errors = "User settings saved successfully!";
                }
                using StreamWriter writer = new(pathSettings);
                writer.WriteLine("var channelName = \'" + userInfo.Name + "\';");
                writer.WriteLine("var channelID = \'" + userInfo.ID + "\';");
                writer.WriteLine("var localToken = \'" + userInfo.Token.Replace("'", @"\'") + "\';");
                writer.WriteLine("var redeemTitle = \'" + userInfo.Redeem + "\';");
                writer.WriteLine("var redeemEnabled = " + (userInfo.RedeemEnabled ? "true" : "false") + ";");
                writer.WriteLine("var twitchCommandPrefix = \'" + userInfo.TwitchCommandPrefix.Replace("'", @"\'") + "\';");
                writer.WriteLine("var twitchCommandEnabled = " + (userInfo.TwitchCommandEnabled ? "true" : "false") + ";");
                writer.WriteLine("var animation_duration = " + userInfo.Duration + ";");
            }
            else
            {
                errors = "User Settings errors:\n" + errors;
            }

            return errors;
        }

        void UpdateUserInfoFromAuthPayload(HttpServer.AuthPayload payload)
        {
            userInfo = new (name: payload.ChannelName, id: payload.ChannelId);
            userInfo.Token = payload.Token;
            userInfo.Rewards = payload.Redeems;
        }

        private async Task DownloadDefaultConfigs(List<string> items)
        {
            string baseUrl = "https://raw.githubusercontent.com/honganqi/GenshinWishOnStream/refs/heads/main/browser_source/js/profiles/default/";

            foreach (string item in items)
            {
                string url = Path.Combine(baseUrl, item);
                string file = await Interwebs.httpClient.GetStringAsync(url);
                string saveTo = Path.Combine(jsPath, "profiles", "default", item);
                File.WriteAllText(saveTo, file);
            }
            lblLatestChoices.Visible = false;
            btnGetLatestChoices.Hide();
            ReadPullSettingsFromFile();
            MessageBox.Show("The default profile has been updated to the latest version.", "Defaults updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private async Task DownloadDefaultImages()
        {
            progressDownloadImages.Show();
            lblDownloadImagesStatus.Text = "";
            try
            {
                string treeUrl = $"https://api.github.com/repos/honganqi/GenshinWishOnStream/git/trees/main?recursive=1";
                Interwebs.httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GenshinWisherGUI/" + Application.ProductVersion);
                string treeJson = await Interwebs.httpClient.GetStringAsync(treeUrl);
                using var tree = JsonDocument.Parse(treeJson);
                List<string> paths = new();
                int totalSize = 0;

                foreach (var entree in tree.RootElement
                         .GetProperty("tree")
                         .EnumerateArray())
                {
                    if (entree.GetProperty("type").GetString() != "blob")
                        continue;

                    string path = entree.GetProperty("path").GetString()!;
                    string targetPath = "browser_source/img/profiles/default";
                    if (!path.StartsWith(targetPath))
                        continue;

                    paths.Add(path);
                    int fileSize = entree.GetProperty("size").GetInt32();
                    totalSize += fileSize;
                }

                DialogResult ask = MessageBox.Show($"Image count: {paths.Count}\nTotal size: {FormatFileSize(totalSize)}\n\nContinue?", "Confirm download", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ask != DialogResult.Yes)
                    return;

                progressDownloadImages.Maximum = paths.Count;
                int downloaded = 0;

                foreach (string path in paths)
                {
                    string downloadUrl = $"https://raw.githubusercontent.com/honganqi/GenshinWishOnStream/main/{path}";

                    // store path and preserve source paths but strip the git stuff
                    string localPath = Path.Combine(imgPath, path.Replace("browser_source/img/", ""));

                    // create the directory if needed
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath));

                    // check if file exists, download if not
                    if (!File.Exists(localPath))
                    {
                        // download directly to disk
                        using var response = await Interwebs.httpClient.GetAsync(downloadUrl);
                        response.EnsureSuccessStatusCode();
                        // var statusCode = response.StatusCode // if needed, lazy+tired now so pass
                        using Stream remoteStream = await response.Content.ReadAsStreamAsync();
                        using FileStream localStream = File.Create(localPath);
                        await remoteStream.CopyToAsync(localStream);
                    }

                    progressDownloadImages.Value = ++downloaded;

                    if (downloaded >= paths.Count)
                        lblDownloadImagesStatus.Text = "Download complete!";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                progressDownloadImages.Hide();
                lblDownloadImagesStatus.Show();
            }
        }
        private async Task ResetConfig(List<string> fileList, string question, Button btn)
        {
            DialogResult exitAsk = MessageBox.Show(question, "Reset File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            try
            {
                btn.Enabled = false;
                if (exitAsk == DialogResult.Yes)
                {
                    await DownloadDefaultConfigs(fileList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading file(s):\n{ex.Message}");
            }
            finally
            {
                btn.Enabled = true;
            }
        }

        private void CheckDefaultProfile()
        {
            // copy local config from "defaults" directory if it exists
            string defaultConfigToCopy = Path.Combine(jsPath, "profiles", "default", "choices.js");

            using (StreamReader reader = new(defaultConfigToCopy))
            {
                string firstLine = reader.ReadLine();
                if (firstLine.StartsWith("// version:"))
                {
                    int.TryParse(firstLine.Replace("// version: ", ""), out currentChoicesVersion);
                }
            }
        }

        private void CheckProfiles()
        {
            cmbProfiles.Items.Clear();

            string[] profiles = Directory.GetDirectories(Path.Combine(jsPath, "profiles"))
                                    .Select(Path.GetFileName)
                                    .ToArray();

            cmbProfiles.Items.AddRange(profiles);

            cmbProfiles.SelectedIndex = cmbProfiles.FindStringExact(activeProfile);
        }

        private void SaveProfile()
        {
            // create directory if it doesn't exist
            string selectedProfile = cmbProfiles.Text;
            string profilePath = Path.Combine(jsPath, "profiles", selectedProfile);
            if (Directory.Exists(profilePath))
            {
                DialogResult exitAsk = MessageBox.Show("This will overwrite the profile. Are you sure?", "Confirm overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (exitAsk != DialogResult.Yes)
                    return;
            }
            Directory.CreateDirectory(profilePath);
            string pathChoices = Path.Combine(profilePath, "choices.js");
            string pathRates = Path.Combine(profilePath, "rates.js");

            List<string> messages = new();
            StarList starList = ExtractDataFromCharactersPanel();

            if (starList.Count > 0)
            {
                rates = new();
                Dictionary<string, string> characterElements = new();
                using (StreamWriter writer = new(pathChoices))
                {
                    writer.WriteLine("let choices = [];\n");

                    // iterate over characters and rates
                    foreach (KeyValuePair<int, CharacterListInStar> charListPair in starList)
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
                    if (dullBlades.Count > 0)
                    {
                        writer.WriteLine("\n\n");
                        writer.WriteLine("let dullBlades = [");
                        foreach (string bladeName in dullBlades)
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

                    messages.Add("Pull settings saved successfully!");
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

            if (messages.Count > 0)
                MessageBox.Show(string.Join("\n\n", messages), "Save status", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CopyProfile()
        {
            string sourceProfilePath = Path.Combine(jsPath, "profiles", cmbProfileCopyFrom.Text);
            string newProfilePath = Path.Combine(jsPath, "profiles", txtNewProfile.Text);
            if (Directory.Exists(newProfilePath))
            {
                DialogResult exitAsk = MessageBox.Show("It looks like the profile already exists. This will overwrite the profile. Are you sure?", "Confirm overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (exitAsk != DialogResult.Yes)
                    return;
            }
            Directory.CreateDirectory(newProfilePath);
            File.Copy(Path.Combine(sourceProfilePath, "choices.js"), Path.Combine(newProfilePath, "choices.js"), true);
            File.Copy(Path.Combine(sourceProfilePath, "rates.js"), Path.Combine(newProfilePath, "rates.js"), true);
        }
        #endregion


        #region Updates
        public async Task CheckUpdate(string url)
        {
            List<string> onlineVer = new();
            List<string> currentVer = new();
            var request = Interwebs.httpClient.GetAsync(url);

            Task timeout = Task.Delay(3000);
            await Task.WhenAny(timeout, request);

            try
            {
                System.Net.Http.HttpResponseMessage response = request.Result;
                if (response.IsSuccessStatusCode)
                {
                    var page = response.Content.ReadAsStringAsync();
                    VersionClass queryResult = JsonConvert.DeserializeObject<VersionClass>(page.Result);

                    if ((queryResult != null) && (queryResult.ReleaseDate != null))
                    {
                        DateTime releaseDate = DateTime.Parse(queryResult.ReleaseDate).ToUniversalTime();
                        string onlineVerString = queryResult.Version;
                        string currentVerString = Application.ProductVersion;
                        downloadURL = queryResult.DownloadURL;
                        if (onlineVerString.CompareTo(currentVerString) > 0)
                        {
                            List<string> versionSplit = onlineVerString.Split('.').ToList();
                            if (versionSplit[3] == "0") versionSplit.RemoveAt(3);
                            if (versionSplit[2] == "0") versionSplit.RemoveAt(2);
                            onlineVer.Add(string.Join(".", versionSplit));
                            onlineVer.Add(releaseDate.ToLocalTime().ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern));
                            btnUpdateNotif.Text = "v" + onlineVer[0] + " is now available!\nGET IT NOW!";
                            if (queryResult.Description != "")
                            {
                                ToolTip updateTooltip = new();
                                updateTooltip.SetToolTip(btnUpdateNotif, "Download from: " + queryResult.DownloadURL + "\n\n" + queryResult.Description);
                            }

                            versionSplit = new(currentVerString.Split('.').ToList());
                            if (versionSplit[3] == "0") versionSplit.RemoveAt(3);
                            if (versionSplit[2] == "0") versionSplit.RemoveAt(2);
                            currentVer.Add(string.Join(".", versionSplit));
                            currentVer.Add(releaseDate.ToLocalTime().ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern));
                            btnUpdateNotif.Show();
                        }
                    }

                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            throw new Exception("The update file was not found on the server.");
                        case HttpStatusCode.BadRequest:
                            throw new Exception("");
                        case HttpStatusCode.InternalServerError:
                            throw new Exception("");
                        case HttpStatusCode.MethodNotAllowed:
                            throw new Exception("");
                        case HttpStatusCode.Forbidden:
                            throw new Exception("");
                    }
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                throw;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        public void GetUpdate()
        {
            if (downloadURL != "" && downloadURL != null) System.Diagnostics.Process.Start(downloadURL);
        }

        public async Task CheckChoicesUpdate()
        {
            string choicesUrl = "https://raw.githubusercontent.com/honganqi/GenshinWishOnStream/main/browser_source/js/profiles/default/choices.js";

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
                        int.TryParse(choicesVersion.Replace("// version: ", ""), out int latestChoicesVersion);
                        if (currentChoicesVersion < latestChoicesVersion)
                        {
                            lblLatestChoices.Visible = true;
                            btnGetLatestChoices.Show();
                        }
                    }
                }
                else
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            throw new Exception("The update file was not found on the server.");
                        case HttpStatusCode.BadRequest:
                            throw new Exception("");
                        case HttpStatusCode.InternalServerError:
                            throw new Exception("");
                        case HttpStatusCode.MethodNotAllowed:
                            throw new Exception("");
                        case HttpStatusCode.Forbidden:
                            throw new Exception("");
                    }
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                throw;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }
        #endregion


        #region Form Functions
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult exitAsk = MessageBox.Show("You sure?", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (exitAsk == DialogResult.Yes)
            {
                httpServer.Stop();
                SaveWindowSettings();
            }
            else
                e.Cancel = true;
        }

        private async void MainWindow_Shown(object sender, EventArgs e)
        {
            try
            {
                // update UI after authentication
                httpServer.AuthCompleted += async payload =>
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke(new Action(async () =>
                        {
                            await HandleAuthSync(payload);
                        }));
                    }
                    else
                    {
                        await HandleAuthSync(payload);
                    }
                };

                // check active pull settings files, will set default as active or download it if "default" profile directory doesn't exist
                await CheckPullSettings();

                // check default profile
                CheckDefaultProfile();

                // check profile directories in /js/profiles and add them to the Profileslist
                CheckProfiles();

                // check and validate stored user settings, will create empty file if it doesn't exist
                // if valid, this function will proceed to:
                // 1. update the panels with user info
                // 2. attempt to fetch the user's Twitch rewards
                // 3. authenticate using the user's token
                // if #3 fails, userInfo.Name will be empty and will trigger the "expired" notification and inform the user to re-authenticate
                await ValidateUserSettingsFromFiles();
                var panel = panelSettings;
                if ((userInfo.Name != ""))
                {
                    panel = panelCharacters;
                    if (userInfo.ID != "")
                        UpdateSettingsRewards();
                }
                SwitchPanel(panel);

                httpServer.Start();

                // check for GUI app updates
                await CheckUpdate(updateurl);
                await CheckChoicesUpdate();
            }
            catch (Exception ex)
            {

            }

        }

        private async Task HandleAuthSync(HttpServer.AuthPayload payload)
        {
            UpdateUserInfoFromAuthPayload(payload);
            await UpdateUIPanelSettingsWithUserInfo(userInfo);
        }

        private void btnMaximize_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
            else
                WindowState = FormWindowState.Maximized;
        }
        private void btnMinimize_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;
        private void btnClose_Click(object sender, EventArgs e) => Close();
        private void imgBMCSupport_Click(object sender, EventArgs e) => System.Diagnostics.Process.Start("https://buymeacoffee.com/honganqi");
        private void imgYoutube_Click(object sender, EventArgs e) => System.Diagnostics.Process.Start("https://youtube.com/honganqi");
        private void imgTwitch_Click(object sender, EventArgs e) => System.Diagnostics.Process.Start("https://twitch.tv/honganqi");
        private void imgSF_Click(object sender, EventArgs e) => System.Diagnostics.Process.Start("https://sourceforge.net/u/honganqi/profile/");
        private void btnPanelSettings_Click(object sender, EventArgs e) => SwitchPanel(panelSettings);
        private void btnPanelCharacters_Click(object sender, EventArgs e) => SwitchPanel(panelCharacters);
        private void btnPanelDullBlades_Click(object sender, EventArgs e) => SwitchPanel(panelDullBlades);
        private void MainWindow_MouseDown(object sender, MouseEventArgs e) => Form_MouseDown(sender, e);
        private void MainWindow_MouseMove(object sender, MouseEventArgs e) => Form_MouseMove(sender, e);
        private void MainWindow_MouseUp(object sender, MouseEventArgs e) => Form_MouseUp(sender, e);
        private void panelTop_DoubleClick(object sender, EventArgs e) => btnMaximize_Click(sender, e);
        private void labelTitle_DoubleClick(object sender, EventArgs e) => btnMaximize_Click(sender, e);
        private void labelVerNum_DoubleClick(object sender, EventArgs e) => btnMaximize_Click(sender, e);
        public void Form_MouseUp(object sender, MouseEventArgs e) => mouseDown = false;

        public void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Clicks == 1)
            {
                mouseDown = true;
                lastLocation = e.Location;
            }
        }

        public void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                Location = new Point(
                    (Location.X - lastLocation.X) + e.X, (Location.Y - lastLocation.Y) + e.Y);
                Update();
            }
        }

        private async void btnSave_Click(object sender, EventArgs e) => await SaveAllSettingsToFile();

        private void btnCheck_Click(object sender, EventArgs e)
        {
            StarList listCheck = ExtractDataFromCharactersPanel();
            if (listCheck.Count > 0)
                MessageBox.Show("No errors found.", "All clear!", MessageBoxButtons.OK);
        }

        protected override void WndProc(ref Message m)
        {
            FormWindowState org = WindowState;
            base.WndProc(ref m);
            if (WindowState != org)
                OnFormWindowStateChanged(EventArgs.Empty);
        }

        protected virtual void OnFormWindowStateChanged(EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
                btnMaximize.BackgroundImage = Images.Load("restore");
            else
                btnMaximize.BackgroundImage = Images.Load("max");
        }

        private void btnUpdateNotif_Click(object sender, EventArgs e) => GetUpdate();

        private void chkCommand_CheckedChanged(object sender, EventArgs e)
        {
            userInfo.TwitchCommandEnabled = chkCommand.Checked;
            txtCommand.Enabled = chkCommand.Checked;
        }

        private void chkRedeems_CheckedChanged(object sender, EventArgs e)
        {
            userInfo.RedeemEnabled = chkRedeems.Checked;
            cmbRedeems.Enabled = chkRedeems.Checked;
        }

        #endregion









        #region Characters Panel Functions
        public void InitializeCharactersPanel(StarList starList)
        {
            ClearCharactersPanel();
            int xPos = initXPos;
            lastItemY = 0;

            int starNum = 1;
            string[] allCharacters = Directory.GetFiles(Path.Combine("img", "characters")).Select(file => Path.GetFileName(file)).ToArray();
            string[] allElements = Directory.GetFiles(Path.Combine("img", "elements")).Select(file => Path.GetFileName(file)).ToArray();

            foreach (KeyValuePair<int, CharacterListInStar> charListPair in starList.Reverse())
            {
                int yPos = initYPos;

                int starValue = charListPair.Key;
                CharacterListInStar charList = charListPair.Value;

                if (starNum == 1)
                    currentMax = starValue;
                else if (starNum == starList.Count)
                    currentMin = starValue;

                Label headerLabel = new()
                {
                    Name = "labelHeader_" + starValue,
                    Text = starValue.ToString() + "-Star",
                    Font = new("Segoe UI", 15, FontStyle.Bold),
                    Location = new Point(xPos, initYPos),
                    AutoSize = true
                };
                panelCharacters.Controls.Add(headerLabel);

                TextBox rateBox = new()
                {
                    Name = "rateBox_" + starValue,
                    Text = charListPair.Value.PullRate.ToString(),
                    Location = new Point(xPos + headerLabel.Width, initYPos),
                    Width = 40
                };
                rateBox.KeyDown += new KeyEventHandler(ValidateRateInput);
                panelCharacters.Controls.Add(rateBox);

                Label rateLabel = new()
                {
                    Name = "labelRate_" + starValue,
                    Text = "% Pull Rate",
                    Font = new("Segoe UI", 8, FontStyle.Bold),
                    Location = new Point(xPos + headerLabel.Width + rateBox.Width, initYPos),
                    Height = rateBox.Height,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                panelCharacters.Controls.Add(rateLabel);

                if ((starNum == 1) || (starNum == starList.Count))
                {
                    Button btnDel = new();
                    if (starNum == 1)
                        btnDel.Name = "btnDelTop_" + starValue;
                    else
                        btnDel.Name = "btnDelBot_" + starValue;
                    btnDel.Text = "X";
                    btnDel.Font = new("Segoe UI", 8, FontStyle.Bold);
                    btnDel.Height = rateBox.Height;
                    btnDel.Width = rateBox.Height;
                    btnDel.Location = new Point(xPos + columnWidth + elementColumnWidth + columnSplitter - btnDel.Width, initYPos);
                    btnDel.TextAlign = ContentAlignment.MiddleCenter;
                    btnDel.Click += new EventHandler(btnDel_Click);
                    panelCharacters.Controls.Add(btnDel);
                }



                int charNum = 0;
                foreach (Character character in charList)
                {
                    string nameToSearch = character + ".webp";
                    yPos += rowHeight;
                    TextBox txtbox = new()
                    {
                        Name = "txtChar_" + starValue + "_" + charNum,
                        Text = character.CharacterName,
                        Location = new Point(xPos, yPos),
                        Width = columnWidth
                    };
                    panelCharacters.Controls.Add(txtbox);

                    TextBox elemtxtbox = new()
                    {
                        Name = "txtElem_" + starValue + "_" + charNum,
                        Text = character.Element,
                        Location = new Point(xPos + columnWidth + columnSplitter, yPos),
                        Width = elementColumnWidth
                    };
                    panelCharacters.Controls.Add(elemtxtbox);

                    charNum++;
                }

                for (int charNumAgain = charNum; charNumAgain < (charNum + extraRows); charNumAgain++)
                {
                    yPos += rowHeight;
                    TextBox txtbox = new()
                    {
                        Name = "txtChar_" + starValue + "_" + charNumAgain,
                        Location = new Point(xPos, yPos),
                        Width = columnWidth
                    };
                    panelCharacters.Controls.Add(txtbox);

                    TextBox elemtxtbox = new()
                    {
                        Name = "txtElem_" + starValue + "_" + charNumAgain,
                        Location = new Point(xPos + columnWidth + columnSplitter, yPos),
                        Width = elementColumnWidth
                    };
                    panelCharacters.Controls.Add(elemtxtbox);

                    if ((yPos + rowHeight) > lastItemY)
                        lastItemY = (yPos + rowHeight);
                }

                xPos += columnWidth + columnMargin + elementColumnWidth + columnSplitter;

                starNum++;
            }
        }

        private void AddCharactersColumn(int starValue, int position, CharacterListInStar characters = null)
        {
            if (starValue > 0)
            {
                if (starValue > currentMax)
                {
                    foreach (Label labelSearch in panelCharacters.Controls.OfType<Label>().Where(l => l.Name.StartsWith("labelHeader_")))
                    {
                        string[] pair = labelSearch.Name.Split('_');
                        if (int.TryParse(pair[1], out int currentValue))
                        {
                            Control label = panelCharacters.Controls["labelHeader_" + currentValue];
                            MoveCharacterControls(currentValue, 1);
                        }
                    }
                    Control moveDel = panelCharacters.Controls["btnDelTop_" + currentMax];
                    currentMax = starValue;
                    moveDel.Name = "btnDelTop_" + currentMax;
                }
                else if (starValue < currentMin)
                {
                    Control moveDel = panelCharacters.Controls["btnDelBot_" + currentMin];
                    currentMin = starValue;
                    moveDel.Name = "btnDelBot_" + currentMin;
                }




                int xPos = initXPos;
                if (position == -1)
                    foreach (Label labelSearch in panelCharacters.Controls.OfType<Label>().Where(l => l.Name.StartsWith("labelHeader_")))
                        xPos += columnWidth + columnMargin + elementColumnWidth + columnSplitter;
                int yPos = initYPos;

                Label headerLabel = new()
                {
                    Name = "labelHeader_" + starValue,
                    Text = starValue + "-Star",
                    Font = new("Segoe UI", 15, FontStyle.Bold),
                    Location = new Point(xPos, initYPos),
                    AutoSize = true
                };
                panelCharacters.Controls.Add(headerLabel);

                TextBox rateBox = new()
                {
                    Name = "rateBox_" + starValue,
                    Text = "0"
                };
                if (characters != null)
                    rateBox.Text = characters.PullRate.ToString();
                rateBox.Location = new Point(xPos + headerLabel.Width, initYPos);
                rateBox.Width = 40;
                rateBox.KeyDown += new KeyEventHandler(ValidateRateInput);
                panelCharacters.Controls.Add(rateBox);

                Label rateLabel = new()
                {
                    Name = "labelRate_" + starValue,
                    Text = "% Pull Rate",
                    Font = new("Segoe UI", 8, FontStyle.Bold),
                    Location = new Point(xPos + headerLabel.Width + rateBox.Width, initYPos),
                    Height = rateBox.Height,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                panelCharacters.Controls.Add(rateLabel);

                int charNum = 0;
                if (characters != null)
                {
                    foreach (Character character in characters)
                    {
                        string characterName = character.CharacterName;
                        yPos += rowHeight;
                        TextBox txtbox = new()
                        {
                            Name = "txtChar_" + starValue + "_" + charNum,
                            Text = characterName,
                            Location = new Point(xPos, yPos),
                            Width = columnWidth
                        };
                        panelCharacters.Controls.Add(txtbox);

                        TextBox elemtxtbox = new()
                        {
                            Name = "txtElem_" + starValue + "_" + charNum,
                            Text = character.Element,
                            Location = new Point(xPos + columnWidth + columnSplitter, yPos),
                            Width = elementColumnWidth
                        };
                        panelCharacters.Controls.Add(elemtxtbox);

                        if ((yPos + rowHeight) > lastItemY)
                            lastItemY = (yPos + rowHeight);
                        charNum++;
                    }
                }

                for (int charNumAgain = charNum; charNumAgain < (charNum + extraRows); charNumAgain++)
                {
                    yPos += rowHeight;
                    TextBox txtbox = new()
                    {
                        Name = "txtChar_" + starValue + "_" + charNumAgain,
                        Location = new Point(xPos, yPos),
                        Width = columnWidth
                    };
                    panelCharacters.Controls.Add(txtbox);

                    TextBox elemtxtbox = new()
                    {
                        Name = "txtElem_" + starValue + "_" + charNumAgain,
                        Location = new Point(xPos + columnWidth + columnSplitter, yPos),
                        Width = elementColumnWidth
                    };
                    panelCharacters.Controls.Add(elemtxtbox);

                    if ((yPos + rowHeight) > lastItemY)
                        lastItemY = (yPos + rowHeight);
                }

                Control btnDelBot = panelCharacters.Controls["btnDelBot_" + currentMin];
                btnDelBot.Location = new Point(btnDelBot.Location.X + (columnWidth + columnMargin + elementColumnWidth + columnSplitter), initYPos);
            }
            else
                MessageBox.Show("The last Star Value of 1 is already present.", "Minimum Star Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void DelCharacterColumn(int starValue)
        {


            Control btnDelBot = panelCharacters.Controls["btnDelBot_" + currentMin];
            btnDelBot.Location = new Point(btnDelBot.Location.X - (columnWidth + columnMargin + elementColumnWidth + columnSplitter), initYPos);
            if (starValue == currentMin)
                currentMin++;
            btnDelBot.Name = "btnDelBot_" + currentMin;
            if (currentMax > currentMin)
            {
                List<string> controlNames = new()
                {
                    "labelHeader_",
                    "rateBox_",
                    "labelRate_",
                    "txtChar_",
                    "txtElem_",
                };
                List<Control> controls = new();
                foreach (string controlName in controlNames)
                {
                    foreach (Control control in panelCharacters.Controls.OfType<Control>().Where(c => c.Name.StartsWith(controlName + starValue)))
                        controls.Add(control);
                }

                foreach (Control control in controls)
                    panelCharacters.Controls.Remove(control);

                if (starValue == currentMax)
                {
                    Control btnDelTop = panelCharacters.Controls["btnDelTop_" + currentMax];
                    btnDelTop.Name = "btnDelTop_" + currentMax;
                    foreach (Label labelHeader in panelCharacters.Controls.OfType<Label>().Where(l => l.Name.StartsWith("labelHeader_")))
                    {
                        string[] pair = labelHeader.Name.Split('_');
                        if (int.TryParse(pair[1], out int currentValue))
                        {
                            Control label = panelCharacters.Controls["labelHeader_" + currentValue];
                            MoveCharacterControls(currentValue, -1);
                        }
                    }
                    currentMax--;
                }

            }
            else
                MessageBox.Show("Why are you trying to eradicate all hopes and wishes?", "Error deleting column", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnAddStarValue_Click(object sender, EventArgs e)
        {
            int higher = currentMax + 1;
            int lower = currentMin - 1;
            ContextMenu cm = new();
            MenuItem cmExport = new("Export swap mod");
            cm.MenuItems.Add("Add " + higher + "-star column", new EventHandler(AddStarValue_Higher));
            cm.MenuItems.Add("Add " + lower + "-star column", new EventHandler(AddStarValue_Lower));
            cm.Show(btnAddStarValue, new Point(btnAddStarValue.Width, 0));
        }

        private void AddStarValue_Higher(object sender, EventArgs e) => AddCharactersColumn(currentMax + 1, 1);
        private void AddStarValue_Lower(object sender, EventArgs e) => AddCharactersColumn(currentMin - 1, -1);


        private void MoveCharacterControls(int starValue, int direction)
        {
            Control label = panelCharacters.Controls["labelHeader_" + starValue];
            int xPos = label.Location.X + ((columnWidth + columnMargin + elementColumnWidth + columnSplitter) * direction);
            label.Location = new Point(xPos, label.Location.Y);
            Control rateBox = panelCharacters.Controls["rateBox_" + starValue];
            rateBox.Location = new Point(xPos + label.Width, label.Location.Y);
            Control labelRate = panelCharacters.Controls["labelRate_" + starValue];
            labelRate.Location = new Point(xPos + label.Width + rateBox.Width, label.Location.Y);

            // move Characters
            foreach (TextBox txtSearch in panelCharacters.Controls.OfType<TextBox>().Where(t => t.Name.StartsWith("txtChar_" + starValue)))
            {
                string[] pair = txtSearch.Name.Split('_');
                if (int.TryParse(pair[2], out int currentValue))
                {
                    Control txtChar = panelCharacters.Controls["txtChar_" + starValue + "_" + currentValue];
                    xPos = txtChar.Location.X + ((columnWidth + columnMargin + elementColumnWidth + columnSplitter) * direction);
                    txtChar.Location = new Point(xPos, txtChar.Location.Y);
                }
            }

            // move Elements
            foreach (TextBox txtSearch in panelCharacters.Controls.OfType<TextBox>().Where(t => t.Name.StartsWith("txtElem_" + starValue)))
            {
                string[] pair = txtSearch.Name.Split('_');
                if (int.TryParse(pair[2], out int currentValue))
                {
                    Control txtChar = panelCharacters.Controls["txtElem_" + starValue + "_" + currentValue];
                    xPos = txtChar.Location.X + ((columnWidth + columnMargin + elementColumnWidth + columnSplitter) * direction);
                    txtChar.Location = new Point(xPos, txtChar.Location.Y);
                }
            }
        }

        public StarList ExtractDataFromCharactersPanel()
        {
            SortedDictionary<int, List<string>> characters = new();
            Dictionary<string, string> elementDictionary = new();
            int currentTotal = 0;

            StarList starList = new();
            List<string> errors = new();

            foreach (Label labelSearch in panelCharacters.Controls.OfType<Label>().Where(l => l.Name.StartsWith("labelHeader_")))
            {
                string[] pair = labelSearch.Name.Split('_');
                if (int.TryParse(pair[1], out int starValue))
                {
                    starList.AddStar(starValue);
                    Control txtbox = panelCharacters.Controls["rateBox_" + starValue];
                    if (int.TryParse(txtbox.Text, out int rate))
                    {
                        starList[starValue].PullRate = rate;
                        currentTotal += rate;
                    }

                    int charNum = 0;
                    foreach (TextBox txtChar in panelCharacters.Controls.OfType<TextBox>().Where(t => t.Name.StartsWith("txtChar_" + starValue)))
                    {
                        if (txtChar.Text.Trim() != "")
                        {
                            string charName = txtChar.Text.Trim();
                            starList[starValue].Add(charName);

                            Control txtElem = panelCharacters.Controls["txtElem_" + starValue + "_" + charNum];
                            string element = txtElem.Text.Trim();
                            starList[charName].Element = element;

                            charNum++;
                        }
                    }
                }
            }

            if (starList.Count == 0)
                errors.Add("No characters were found.");

            if (currentTotal != 100)
            {
                starList = new();
                errors.Add("Please ensure that your rates have a total of 100%. \n\nCurrent total: " + currentTotal);
            }

            if (errors.Count > 0)
                MessageBox.Show(string.Join("\n", errors.ToArray()));

            return starList;
        }

        private void SortCharacterData(StarList starList)
        {
            if (starList.Count > 0)
            {
                ClearCharactersPanel();
                foreach (KeyValuePair<int, CharacterListInStar> charList in starList.Reverse())
                {
                    int starValue = charList.Key;
                    CharacterListInStar charListInStar = charList.Value;
                    charListInStar.SortList();
                    starList.AddStar(starValue);
                    AddCharactersColumn(charList.Key, -1, charListInStar);
                }
            }
        }

        private void btnSortCharacters_Click(object sender, EventArgs e) => SortCharacterData(ExtractDataFromCharactersPanel());

        private void btnDel_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string[] pair = btn.Name.Split('_');

            if (int.TryParse(pair[1], out int starValue))
            {
                DialogResult dr = MessageBox.Show(
                    "Are you sure you want to delete the entire " + starValue + "-star column? Only Venti can undo this!",
                    "Delete Column",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                    );
                if (dr == DialogResult.Yes)
                    DelCharacterColumn(starValue);
            }
        }

        private void ClearCharactersPanel()
        {
            List<string> controlNames = new()
            {
                "labelHeader_",
                "rateBox_",
                "labelRate_",
                "txtChar_",
                "txtElem_",
            };
            List<Control> controls = new();
            foreach (string controlName in controlNames)
            {
                foreach (Control control in panelCharacters.Controls.OfType<Control>().Where(c => c.Name.StartsWith(controlName)))
                    controls.Add(control);
            }

            foreach (Control control in controls)
                panelCharacters.Controls.Remove(control);
        }

        private void ValidateRateInput(object sender, KeyEventArgs e)
        {
            if (e.KeyData != Keys.Back)
                e.SuppressKeyPress = !int.TryParse(Convert.ToString((char)e.KeyData), out int rate);
        }
        #endregion











        #region Settings Panel Functions
        public void TogglePanelVisibility(bool showPanels)
        {
            if (showPanels)
            {
                btnPanelDullBlades.Show();
                btnSave.Show();
                btnCheck.Show();
                btnPanelCharacters.Show();
                btnPanelSettings.Show();
                labelProfiles.Show();
                cmbProfiles.Show();
            } else
            {
                btnPanelDullBlades.Hide();
                btnSave.Hide();
                btnCheck.Hide();
                btnPanelCharacters.Hide();
                btnPanelSettings.Hide();
            }
        }

        public void ExtractUserInfo(ref UserInfo info)
        {
            info.Redeem = cmbRedeems.Text.Trim();
            info.RedeemEnabled = info.Redeem != "" && chkRedeems.Checked;
            info.TwitchCommandPrefix = txtCommand.Text.Trim();
            info.TwitchCommandEnabled = info.TwitchCommandPrefix != "" && chkCommand.Checked;
            info.Duration = 8000;
            if (txtDuration.Text.Trim() != "")
                if (int.TryParse(txtDuration.Text.Trim(), out int duration))
                    info.Duration = duration;
        }

        public async Task UpdateUIPanelSettingsWithUserInfo(UserInfo userInfo)
        {
            if (InvokeRequired)
            {
                await (Task)Invoke(new Func<Task>(async () =>
                {
                    await UpdateUIPanelSettingsWithUserInfo(userInfo);
                }));

                return;
            }
            txtUsername.Text = userInfo.Name;
            if (userInfo.Name != "")
            {
                imgTwitchConnect.Visible = false;
                btnCopyAuthLink.Visible = false;
                btnRevokeToken.Enabled = true;
                btnUpdateRewards.Enabled = true;
                chkRedeems.Enabled = true;
                chkCommand.Enabled = true;
                txtCommand.Enabled = false;

                // set redeems before selecting
                if (cmbRedeems.InvokeRequired)
                    cmbRedeems.Invoke(new MethodInvoker(() => SetRewards()));
                else
                    SetRewards();

                chkRedeems.Checked = userInfo.RedeemEnabled;
                cmbRedeems.Enabled = userInfo.RedeemEnabled;
                if (userInfo.Name != "")
                    cmbRedeems.SelectedIndex = cmbRedeems.FindStringExact(userInfo.Redeem);
                else
                    cmbRedeems.Items.Clear();

                txtCommand.Text = userInfo.TwitchCommandPrefix;
                chkCommand.Checked = userInfo.TwitchCommandEnabled;
                txtDuration.Enabled = true;
                txtDuration.Text = userInfo.Duration.ToString();

                lblTokenExpired.Visible = false;
            }
        }

        public void SetRewards()
        {
            cmbRedeems.Items.Clear();
            foreach (string reward in userInfo.Rewards)
                cmbRedeems.Items.Add(reward);
        }

        private void imgTwitchConnect_Click(object sender, EventArgs e) => System.Diagnostics.Process.Start(authUrl);

        private void btnCopyAuthLink_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(authUrl);
            MessageBox.Show("Link copied!", "", MessageBoxButtons.OK);
        }
        private void btnRevokeToken_Click(object sender, EventArgs e) => ResetUser();

        public bool ControlInvokeRequired(Control c, Action a)
        {
            if (c.InvokeRequired) c.Invoke(new MethodInvoker(delegate { a(); }));
            else return false;

            return true;
        }
        #endregion










        #region Dull Blades Panel Functions
        public void InitializeDullBladesPanel(List<string> dullBlades)
        {
            ClearDullBladesPanel();
            int xPos = initXPos;
            int yPos = initYPos;
            lastItemYDullBlade = 0;

            int rownum = 0;

            foreach (string bladeName in dullBlades)
            {
                yPos += rowHeight;
                TextBox txtbox = new();
                txtbox.Name = "txtDull_" + rownum++;
                txtbox.Text = bladeName;
                txtbox.Location = new Point(xPos, yPos);
                txtbox.Width = columnWidth;
                panelDullBlades.Controls.Add(txtbox);
            }

            for (int rowNumAgain = rownum; rowNumAgain < (rownum + extraRows); rowNumAgain++)
            {
                yPos += rowHeight;
                TextBox txtbox = new();
                txtbox.Name = "txtDull_" + rowNumAgain;
                txtbox.Location = new Point(xPos, yPos);
                txtbox.Width = columnWidth;
                panelDullBlades.Controls.Add(txtbox);

                if ((yPos + rowHeight) > lastItemYDullBlade)
                    lastItemYDullBlade = (yPos + rowHeight);
            }
        }

        public List<string> ExtractDataFromDullBladesPanel()
        {
            List<string> dullBlades = new();
            List<string> errors = new();

            foreach (TextBox dullSearch in panelDullBlades.Controls.OfType<TextBox>().Where(l => l.Name.StartsWith("txtDull_")))
            {
                string[] pair = dullSearch.Name.Split('_');
                if (dullSearch.Text.Trim() != "")
                    dullBlades.Add(dullSearch.Text.Trim());
            }

            return dullBlades;
        }

        private void SortDullBladesData(List<string> dullBlades)
        {
            if (dullBlades.Count > 0)
            {
                ClearDullBladesPanel();
                dullBlades.Sort();
                InitializeDullBladesPanel(dullBlades);
            }
        }
        private void btnSortDullBlades_Click(object sender, EventArgs e) => SortDullBladesData(ExtractDataFromDullBladesPanel());
        private void ClearDullBladesPanel()
        {
            List<Control> controls = new();
            foreach (Control control in panelDullBlades.Controls.OfType<Control>().Where(c => c.Name.StartsWith("txtDull_")))
                controls.Add(control);

            foreach (Control control in controls)
                panelDullBlades.Controls.Remove(control);
        }
        #endregion

        private async void btnResetDefaults_Click(object sender, EventArgs e)
        {
            List<string> list = ["choices.js", "rates.js"];
            string question = "If you customized the default profile, you might want to create a new profile based on this before you continue.\n\nAre you sure you want to reset the default profile?";
            Button btn = btnResetDefaults;
            await ResetConfig(list, question, btn);
        }

        private void btnUpdateRewards_Click(object sender, EventArgs e)
        {
            UpdateSettingsRewards();
        }

        private void cmbProfiles_DropDown(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            int maxWidth = cb.Width;

            // get length of profile name based on the font
            using (Graphics g = cb.CreateGraphics())
            {
                foreach (object item in cb.Items)
                {
                    if (item != null)
                    {
                        int itemWidth = (int)g.MeasureString(item.ToString(), cb.Font).Width;
                        if (itemWidth > maxWidth)
                            maxWidth = itemWidth;
                    }
                }
            }

            // padding for scroll bar
            cb.DropDownWidth = maxWidth + SystemInformation.VerticalScrollBarWidth;
        }

        private async void btnProfileCreate_Click(object sender, EventArgs e)
        {
            cmbProfileCopyFrom.Items.Clear();
            cmbProfileCopyFrom.Items.AddRange(cmbProfiles.Items.Cast<object>().ToArray());
            cmbProfileCopyFrom.SelectedIndex = cmbProfiles.SelectedIndex;
            panelNewProfile.Show();
            //await SaveProfile();
            //await CheckProfiles();
        }

        private void btnProfileCreateSave_Click(object sender, EventArgs e)
        {
            CopyProfile();
        }

        private void btnProfileCreateCancel_Click(object sender, EventArgs e)
        {
            panelNewProfile.Hide();
            txtNewProfile.Text = "";
        }

        private void cmbProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            string previousProfile = activeProfile;
            activeProfile = cmbProfiles.Text;
            if (!ReadPullSettingsFromFile())
            {
                activeProfile = previousProfile;
                cmbProfiles.SelectedIndex = cmbProfiles.FindStringExact(activeProfile);
            }
        }

        private async void btnGetLatestChoices_Click(object sender, EventArgs e)
        {
            List<string> fileList= ["choices.js"];
            await DownloadDefaultConfigs(fileList);
        }

        private async void btnGetImages_Click(object sender, EventArgs e)
        {
            await DownloadDefaultImages();
        }
    }
}