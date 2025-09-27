using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace GenshinImpact_WishOnStreamGUI
{
    public partial class MainWindow : Form
    {
        public Assembly imageAssembly = Assembly.GetExecutingAssembly();

        public bool mouseDown;
        public Point lastLocation;

        public string wisherPath = "";

        SortedDictionary<int, int> rates = new();
        StarList starList = new();
        List<string> dullBlades = new();

        public bool updateAvailable = false;
        string updateurl = "https://raw.githubusercontent.com/honganqi/GenshinWishOnStream/main/latest.json";
        string downloadURL = "";
        HttpServer httpServer = new();
        public AuthThings authVar;
        UserInfo userInfo;
        public bool userTokenized = false;


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

            authVar = new(this);

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

            string pathCheck = "";

            // set path if exists
            if (Properties.Settings.Default.path != "")
            {
                if (Directory.Exists(Properties.Settings.Default.path))
                    pathCheck = Properties.Settings.Default.path;
                else
                    MessageBox.Show("The previously specified folder does not exist (anymore): \n" + Properties.Settings.Default.path + "\n\nPlease point this app to where the \"Genshin_Wish.html\" file is.");
                    SwitchPanel(panelSettings);
            }
            else
                pathCheck = Directory.GetCurrentDirectory();

            // check user and settings
            List<string> failedFiles = CheckSettings(pathCheck);
            CheckFiles(failedFiles, pathCheck);
            string userInitError = userInfo.CheckUser();
            if ((failedFiles.Count == 0) && (userInitError == ""))
                SwitchPanel(panelCharacters);
            else
                if (userInitError != "")
                MessageBox.Show(userInitError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SwitchPanel(panelSettings);

            // set window state
            if (Properties.Settings.Default.windowState == "Maximized")
            {
                WindowState = FormWindowState.Maximized;
                btnMaximize.BackgroundImage = Images.Load("restore");
            }

            httpServer.Start(this);

            // check for updates
            CheckUpdate(updateurl);
        }

        private void SwitchPanel(Panel panelname)
        {
            if ((wisherPath != "") || ((wisherPath == "") && (panelname == panelSettings)))
                panelname.BringToFront();
            else
                MessageBox.Show("The path to the Genshin Wisher has not been set yet. Please point this app to where the \"Genshin_Wish.html\" file is.", "Setup needed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void SaveWindowSettings()
        {
            Properties.Settings.Default.windowSize = Size;
            Properties.Settings.Default.windowStartLocation = Location;
            Properties.Settings.Default.windowState = WindowState.ToString();
            if ((wisherPath != "") && Directory.Exists(wisherPath))
                Properties.Settings.Default.path = wisherPath;
            Properties.Settings.Default.Save();
        }

        public void RevokeToken()
        {
            authVar.RevokeToken();
            cmbRedeems.Items.Clear(); 
        }

        public void UpdateSettingsPanel(UserInfo userTransit)
        {
            userInfo = userTransit;
            SetUserInfo(userInfo);
        }

        public void UpdateSettingsRewards()
        {
            SetUserInfo(userInfo);
        }

        public void DisplayConnectionErrors(List<string> errors)
        {
            if (errors.Count > 0)
                MessageBox.Show(string.Join("\n", errors), "Unable to connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        #region Fetchers
        private bool GetLocalUser()
        {
            Dictionary<string, string> userSettingsContents = new();
            string pattern = @"var\s+(\w+)\s*=\s*(?:""([^""]*)""|'([^'\\]*(?:\\'[^'\\]*)*)'|(\d+|true|false))\s*;";
            Regex regex = new(pattern);

            List<string> searchTerms = new()
            {
                "channelName",
                "channelID",
                "localToken",
                "localTokenExpiry",
                "redeemTitle",
                "redeemEnabled",
                "twitchCommandPrefix",
                "twitchCommandEnabled",
                "animation_duration"
            };


            using (StreamReader sr = new(Path.Combine(wisherPath, "js/local_creds.js")))
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
                userTokenized = true;
                userInfo.Redeem = userSettingsContents.ContainsKey("redeemTitle") ? userSettingsContents["redeemTitle"] : "";
                userInfo.RedeemEnabled = (userSettingsContents.ContainsKey("redeemEnabled") && bool.Parse(userSettingsContents["redeemEnabled"])) ? bool.Parse(userSettingsContents["redeemEnabled"]) : false;
                userInfo.TwitchCommandPrefix = userSettingsContents.ContainsKey("twitchCommandPrefix") ? userSettingsContents["twitchCommandPrefix"] : "";
                userInfo.TwitchCommandEnabled = (userSettingsContents.ContainsKey("twitchCommandEnabled") && bool.Parse(userSettingsContents["twitchCommandEnabled"])) ? bool.Parse(userSettingsContents["twitchCommandEnabled"]) : false;
                if (!userSettingsContents.ContainsKey("redeemEnabled") && userInfo.Redeem != "")
                    userInfo.RedeemEnabled = true;
                if (userSettingsContents.ContainsKey("localTokenExpiry") && int.TryParse(userSettingsContents["localTokenExpiry"], out int expiry))
                {
                    userInfo.TokenExpiry = expiry;
                    if (int.TryParse(userSettingsContents["animation_duration"], out int duration))
                        userInfo.Duration = duration;
                }

                authVar.user = userInfo;

                // get new token if user exists
                if (userInfo.Token != "")
                {
                    SetUserInfo(userInfo);
                    authVar.GetUserInfo(userInfo.Token);
                }

                return true;
            }

            return false;
        }
        private bool GetRates()
        {
            using (StreamReader sr = new(Path.Combine(wisherPath, "js/rates.js")))
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
            starList = new();
            using StreamReader sr = new(Path.Combine(wisherPath, "js/choices.js"));
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
                    //string name = line.Replace("\"", "").Replace("\'", "").Replace(",", "").Trim();
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
        #endregion


        #region File Read-Write
        public List<string> CheckSettings(string pathCheck)
        {
            List<string> filesToCheck = new()
            {
                "rates.js",
                "choices.js",
            };
            List<string> failedFiles = new();
            if (!File.Exists(Path.Combine(pathCheck, "Genshin_Wish.html")))
                failedFiles.Add("Genshin_Wish.html");

            // check for local_creds.js file, create it if it doesn't exist
            if (!File.Exists(Path.Combine(pathCheck, "js/", "local_creds.js")))
                authVar.SaveCreds(userInfo: new(), saveToFile: true, revoke: true);

            foreach (string toCheck in filesToCheck)
            {
                if (!File.Exists(Path.Combine(pathCheck, "js/", toCheck)))
                    failedFiles.Add(toCheck);
            }


            return failedFiles;
        }

        public void CheckFiles(List<string> failedFiles, string pathCheck)
        {
            if (failedFiles.Count > 0)
            {
                string errorMessage = "The following required files are missing: ";
                foreach (string fail in failedFiles)
                    errorMessage += "\n - " + fail;
                MessageBox.Show(errorMessage, "Required Files Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                wisherPath = pathCheck;
                authVar.wisherPath = wisherPath;
                SetPath(wisherPath);

                ReadSettings();

                btnPanelDullBlades.Show();
                btnSave.Show();
                btnCheck.Show();
                btnPanelCharacters.Show();
                btnPanelSettings.Show();

                Properties.Settings.Default.path = pathCheck;
                Properties.Settings.Default.Save();
            }
        }

        private void ReadSettings()
        {
            List<string> errors = new();
            if (!GetChoices())
                errors.Add("choices.js");
            if (!GetRates())
                errors.Add("rates.js");
            if (!GetLocalUser())
                errors.Add("local_creds.js");
            if (errors.Count > 0)
                MessageBox.Show("There were errors reading the following files:\n\n   - " + string.Join("\n   - ", errors) + "\n\nThese are probably syntax errors. Kindly check your files or download the JS files again.");
            else
            {
                InitializeCharactersPanel(starList);
                InitializeDullBladesPanel(dullBlades);
            }
        }

        public void SaveData()
        {
            List<string> messages = new();
            StarList starList = ExtractDataFromCharactersPanel();
            if (starList.Count > 0)
            {
                rates = new();
                Dictionary<string, string> characterElements = new();
                string pathChoices = Path.Combine(wisherPath, "js/choices.js");

                if (File.Exists(pathChoices))
                {
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
                        string pathRates = Path.Combine(wisherPath, "js/rates.js");
                        if (File.Exists(pathRates))
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
                            messages.Add("\"rates.js\" not found in the \"" + wisherPath + "\"js folder.");
                        }

                    }
                    else
                    {
                        messages.Add("No rates found.");
                    }
                }
                else
                {
                    messages.Add("\"choices.js\" not found in the \"" + wisherPath + "\"js folder.");
                }
            }
            else
            {
                messages.Add("There was an error in the Character table data.");
            }
            ExtractUserInfo(ref userInfo);
            string credMessages = authVar.SaveCreds(userInfo, true);
            messages.Add(credMessages);

            if (messages.Count > 0)
                MessageBox.Show(string.Join("\n\n", messages), "Save status", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }
        #endregion


        #region Updates
        public async void CheckUpdate(string url)
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (wisherPath != "")
                SaveData();
            else
                MessageBox.Show("The path to the Genshin Wisher has not been set yet. Please point this app to where the \"Genshin_Wish.html\" file is.", "Setup needed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (wisherPath != "")
                ExtractDataFromCharactersPanel();
            else
                MessageBox.Show("The path to the Genshin Wisher has not been set yet. Please point this app to where the \"Genshin_Wish.html\" file is.", "Setup needed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        #endregion









        #region Characters Panel Functions
        public void InitializeCharactersPanel(StarList starList)
        {
            ClearCharactersPanel();
            int xPos = initXPos;
            lastItemY = 0;

            int starNum = 1;
            string[] allCharacters = Directory.GetFiles(Path.Combine(wisherPath, "img", "characters")).Select(file => Path.GetFileName(file)).ToArray();
            string[] allElements = Directory.GetFiles(Path.Combine(wisherPath, "img", "elements")).Select(file => Path.GetFileName(file)).ToArray();

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
        public void SetPath(string newpath)
        {
            cmbRedeems.Enabled = true;
            txtDuration.Enabled = true;
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

        public void SetUserInfo(UserInfo userInfo)
        {
            if (ControlInvokeRequired(txtUsername, () => SetUserInfo(userInfo))) return;
            txtUsername.Text = userInfo.Name;

            // set redeems before selecting
            if (cmbRedeems.InvokeRequired)
                cmbRedeems.Invoke(new MethodInvoker(() => SetRewards()));
            else
                SetRewards();

            chkRedeems.Checked = userInfo.RedeemEnabled;

            if (userInfo.BroadcasterType != "affiliate" && userInfo.BroadcasterType != "partner")
            {
                chkRedeems.Enabled = false;
                cmbRedeems.Enabled = false;
                cmbRedeems.Text = "";
            } else
            {
                chkRedeems.Enabled = true;
                cmbRedeems.Enabled = true;
                if (userInfo.Name != "")
                    cmbRedeems.SelectedIndex = cmbRedeems.FindStringExact(userInfo.Redeem);
                else
                    cmbRedeems.Items.Clear();
            }

            txtCommand.Text = userInfo.TwitchCommandPrefix;

            chkCommand.Checked = userInfo.TwitchCommandEnabled;

            txtDuration.Text = userInfo.Duration.ToString();

            long rightNow = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (userInfo.TokenExpiry > 0)
            {
                if (rightNow < userInfo.TokenExpiry)
                {
                    DateTime unixtime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    DateTime expiryTime = unixtime.AddSeconds(userInfo.TokenExpiry).ToLocalTime();
                    labelTokenExpiry.Text = "Token expires on: " + expiryTime;
                    btnRevokeToken.Enabled = true;
                }
                else
                {
                    labelTokenExpiry.Text = "Token expired.";
                }
            }
            else
            {
                labelTokenExpiry.Text = "Please acquire a token by clicking on the button below.";
            }
        }

        public void SetRewards()
        {
            cmbRedeems.Items.Clear();
            foreach (string reward in userInfo.Rewards)
                cmbRedeems.Items.Add(reward);
        }


        private static string Nonce(int length)
        {
            Random random = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string pathCheck = dialog.SelectedPath;
                List<string> failedFiles = CheckSettings(pathCheck);
                CheckFiles(failedFiles, dialog.SelectedPath);
            }
        }

        private void imgTwitchConnect_Click(object sender, EventArgs e)
        {
            if (wisherPath != "")
            {
                string clientId = AuthThings.CLIENT_ID;
                string redirectURI = HttpServer.localhostAddress.TrimEnd('/');
                string state = Nonce(15);
                string scope = System.Web.HttpUtility.UrlEncode("channel:read:redemptions chat:read");

                string url = "https://id.twitch.tv/oauth2/authorize" +
                    "?response_type=token" +
                    "&client_id=" + clientId +
                    "&redirect_uri=" + redirectURI +
                    "&state=" + state +
                    "&scope=" + scope +
                    "&force_verify=true";
                System.Diagnostics.Process.Start(url);
            }
            else
            {
                MessageBox.Show("The path to the Genshin Wisher has not been set yet. Please point this app to where the \"Genshin_Wish.html\" file is.", "Setup needed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRevokeToken_Click(object sender, EventArgs e) => RevokeToken();

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
    }


}