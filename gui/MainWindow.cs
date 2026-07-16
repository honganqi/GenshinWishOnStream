using GenshinImpact_WishOnStreamGUI.panels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenshinImpact_WishOnStreamGUI
{
    public partial class MainWindow : Form
    {
        ILibraryService _library = new Library();

        public Assembly imageAssembly = Assembly.GetExecutingAssembly();

        public bool mouseDown;
        public Point lastLocation;


        readonly string jsPath = Path.Combine(Application.StartupPath, "js");
        string activeProfile;

        public bool updateAvailable = false;
        readonly string updateurl = "https://raw.githubusercontent.com/honganqi/GenshinWishOnStream/main/latest.json";
        string downloadURL = "";

        PanelSettingsControl panelSettingsControl = new();
        PanelCharactersControl panelCharactersControl = new();
        PanelDullBladesControl panelDullBladesControl = new();
        PanelProfilesControl panelProfilesControl = new();
        PanelProfilesTopBarControl panelProfilesTopBarControl = new();


        public MainWindow()
        {
            InitializeComponent();

            panelSettings.Controls.Clear();
            panelSettingsControl.Dock = DockStyle.Fill;
            panelSettings.Controls.Add(panelSettingsControl);
            panelCharacters.Controls.Clear();
            panelCharactersControl.Dock = DockStyle.Fill;
            panelCharacters.Controls.Add(panelCharactersControl);
            panelDullBlades.Controls.Clear();
            panelDullBladesControl.Dock = DockStyle.Fill;
            panelDullBlades.Controls.Add(panelDullBladesControl);
            panelProfiles.Controls.Clear();
            panelProfilesControl.Dock = DockStyle.Fill;
            panelProfiles.Controls.Add(panelProfilesControl);
            panelProfilesTop.Controls.Clear();
            panelProfilesTopBarControl.Dock = DockStyle.Top;
            panelProfilesTop.Controls.Add(panelProfilesTopBarControl);

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
            btnSaveProfile.BackgroundImage = Images.Load("save");
            btnProfiles.BackgroundImage = Images.Load("check");
            btnPanelCharacters.BackgroundImage = Images.Load("character");
            btnPanelDullBlades.BackgroundImage = Images.Load("dullblade");

            // user info
            UserInfo userInfo = _library.UserInfoObject;

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
            panelProfilesControl.SetActiveProfile(activeProfile);
            panelProfilesTopBarControl.SetActiveProfile(activeProfile);





            // action delegates
            #region Profile Delegates
            panelProfilesControl.SaveProfileToFile = async (newProfile, sourceProfile) => SaveProfile(newProfile, sourceProfile);

            panelProfilesControl.ResetDownloadConfigs = async fileList =>
            {
                bool success = false;
                try
                {
                    success = await _library.DownloadDefaultConfigs(fileList);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
                return success;
            };

            panelProfilesControl.DownloadDefaultImages_Ask = async () =>
            {
                try
                {
                    List<string> paths = await _library.DownloadDefaultImages_Prefetch(true);
                    panelProfilesControl.SetDownloadStatus(true);
                    var progress = new Progress<DownloadProgress>(p =>
                    {
                        panelProfilesControl.UpdateProgress(p);
                    });
                    await _library.DownloadDefaultImages(paths, progress);
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show(ex.Message, "Download error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception)
                {
                    // exit/cancel download
                    //MessageBox.Show("Cancelled download", "Download cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                finally
                {
                    panelProfilesControl.SetDownloadStatus(false);
                }
            };

            panelProfilesControl.ChangeProfile = profile =>
            {
                bool filesAreValid = ChangeProfile(profile);
                panelProfilesTopBarControl.SetSelectedProfile(profile);
                return filesAreValid;
            };

            panelProfilesTopBarControl.ChangeProfile = profile =>
            {
                bool filesAreValid = ChangeProfile(profile);
                panelProfilesControl.SetSelectedProfile(profile);
                return filesAreValid;
            };

            panelProfilesControl.ActivateProfile = profile =>
            {
                bool activateSuccess = ActivateProfile(profile);
                return activateSuccess;
            };

            panelProfilesTopBarControl.ActivateProfile = profile =>
            {
                bool activateSuccess = ActivateProfile(profile);
                return activateSuccess;
            };

            panelProfilesControl.DeleteProfile = async profile =>
            {
                try
                {
                    await _library.DeleteProfile(profile);
                    bool filesAreValid = ChangeProfile("default");
                    panelProfilesControl.SetSelectedProfile("default");
                    ActivateProfile("default");
                    UpdateProfileList();
                    MessageBox.Show($"The \"{profile}\" profile has been successfully deleted.", "Profile deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Delete failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            };

            panelProfilesControl.UpdateProfileList = () => UpdateProfileList();
            #endregion

            #region Settings Delegates
            _library.HttpServerObject.AuthCompleted += async payload =>
            {
                BeginInvoke(async () =>
                {
                    await _library.HandleAuthSync(payload);

                    panelSettingsControl.userInfoTransit = _library.UserInfoObject;
                    await panelSettingsControl.UpdateSettingsRewards();
                    await panelSettingsControl.PopulateUserInfo(true);
                });
            };

            _library.HttpServerObject.RewriteLocalCreds += payload => _library.RewriteLocalCreds(payload);

            panelSettingsControl.GetCustomRewards = async () =>
            {
                List<string> rewards = new();
                try
                {
                    rewards = await _library.UserInfoObject.GetCustomRewards();
                }
                catch (UnauthorizedAccessException ex)
                {
                    SwitchPanel(panelSettings);
                    throw new UnauthorizedAccessException(ex.Message);
                }
                catch (Exception ex)
                {
                    SwitchPanel(panelSettings);
                    throw new Exception(ex.Message);
                }
                return rewards;
            };

            panelSettingsControl.SaveUserSettingsToFile = async (userInfo, revoke, fromExpiredToken) =>
            {
                _library.UserInfoObject = userInfo;
                _library.SaveUserSettingsToFile(revoke, fromExpiredToken);
            };

            _library.UpdateSettingsPanelWithUserInfo += async userInfo =>
            {
                panelSettingsControl.userInfoTransit = userInfo;
                await panelSettingsControl.PopulateUserInfo(true);
            };
            #endregion
        }

        private void SwitchPanel(Panel panelname)
        {
            string actualPanelName = panelname.Name;
            switch (actualPanelName)
            {
                case "panelCharacters":
                case "panelDullBlades":
                    panelProfilesTop.Show();
                    break;
                default:
                    panelProfilesTop.Hide();
                    break;
            }
            panelname.BringToFront();
        }

        private void SaveWindowSettings()
        {
            Properties.Settings.Default.windowSize = Size;
            Properties.Settings.Default.windowStartLocation = Location;
            Properties.Settings.Default.windowState = WindowState.ToString();
            Properties.Settings.Default.activeProfile = panelProfilesTopBarControl.activeProfile;
            Properties.Settings.Default.Save();
        }

        public void DisplayConnectionErrors(List<string> errors)
        {
            if (errors.Count > 0)
                MessageBox.Show(string.Join("\n", errors), "Unable to connect", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private bool ChangeProfile(string profile)
        {
            // TO-DO: before switching, check if there are unsaved changes and ask
            bool filesAreValid = _library.ValidateOrRepairPullSettings(profile);
            panelCharactersControl.InitializeCharactersPanel(_library.StarListObject);
            panelDullBladesControl.InitializeDullBladesPanel(_library.DullBlades);
            return filesAreValid;
        }

        private bool ActivateProfile(string profile)
        {
            // TO-DO: save AND activate?
            bool activateSuccess = _library.ActivateProfile(profile);
            activeProfile = profile;
            panelProfilesControl.SetActiveProfile(activeProfile);
            panelProfilesTopBarControl.SetActiveProfile(activeProfile);
            panelCharactersControl.InitializeCharactersPanel(_library.StarListObject);
            panelDullBladesControl.InitializeDullBladesPanel(_library.DullBlades);
            return activateSuccess;
        }

        private void UpdateProfileList()
        {
            string[] profiles = _library.CheckProfiles();
            panelProfilesControl.PopulateProfiles(profiles, activeProfile);
            panelProfilesTopBarControl.PopulateProfiles(profiles, activeProfile);
        }



        #region File Read-Write
        void SaveProfile(string profile, string sourceProfileofImages = "")
        {
            // TO-DO: check if saving active or selected/editing profile
            StarList starList = panelCharactersControl.ExtractDataFromCharactersPanel();
            List<string> dullBlades = panelDullBladesControl.ExtractDataFromDullBladesPanel();
            _library.StarListObject = starList;
            _library.DullBlades = dullBlades;
            _library.SaveProfile(profile, sourceProfileofImages);
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
                HttpResponseMessage response = request.Result;
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
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }

        public async Task GetUpdate_Ask()
        {
            DialogResult exitAsk = MessageBox.Show("Are you sure you want to update? This will overwrite this program but will retain your settings and profiles.\n\nYes: download and overwrite\nNo: visit the change log to preview changes\nCancel: cancel download", "Confirm update", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (exitAsk == DialogResult.Cancel)
                return;
            if (exitAsk == DialogResult.No)
            {
                string changeLogUrl = "https://github.com/honganqi/GenshinWishOnStream/blob/main/CHANGELOG.md";
                Process.Start(changeLogUrl);
                return;
            }

            await GetUpdate();
        }

        public async Task GetUpdate()
        {
            string downloadUrl = $"https://github.com/honganqi/GenshinWishOnStream/releases/latest/download/GenshinWishOnStream.exe";
            string fullOriginalPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string fileName = Path.GetFileName(fullOriginalPath);
            string directoryName = Path.GetDirectoryName(fullOriginalPath);
            string tempFile = fullOriginalPath + ".old";

            if (File.Exists(tempFile))
                File.Delete(tempFile);

            try
            {
                File.Move(fullOriginalPath, tempFile);
                byte[] byteArray = await Interwebs.httpClient.GetByteArrayAsync(downloadUrl);
                File.WriteAllBytes(Path.Combine(directoryName, fileName), byteArray);

                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = fullOriginalPath,
                    UseShellExecute = true
                });

                Environment.Exit(0);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to download the latest version.", "Download error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnGetLatestCharacterList_Click(object sender, EventArgs e)
        {
            List<string> fileList = ["choices.js"];
            try
            {
                List<string> downloadList = await _library.DownloadDefaultImages_Prefetch(false);
                if (downloadList.Count > 0)
                {
                    DialogResult ask = MessageBox.Show(
                        $"{downloadList.Count} new images found.\n\n" +
                        $"Do you want to download the images along with the default profile configs?\n\n" + 
                        $"Yes: Configs and images\n" + 
                        $"No: Configs only\n" +
                        $"Cancel: Don't download anything",
                        "Confirm download",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question
                        );
                    if (ask == DialogResult.Yes)
                        await _library.DownloadDefaultImages(downloadList);
                    if (ask == DialogResult.Cancel)
                        throw new Exception("cancelled");
                }
                await _library.DownloadDefaultConfigs(fileList);
                MessageBox.Show("Default profile updated successfully!", "Downloaded Default Profile", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lblLatestCharacterList.Hide();
                btnGetLatestCharacterList.Hide();
            }
            catch (Exception ex)
            {
                if (ex.Message != "cancelled")
                    MessageBox.Show(ex.Message, "Download error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }
        #endregion


        #region Form Controls
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            string additionalQuestion = "";
            if (panelProfilesControl.selectedProfile != activeProfile)
                additionalQuestion = "\n\nReminder: The selected profile is not the active one.";
            DialogResult exitAsk = MessageBox.Show($"You sure? {additionalQuestion}", "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (exitAsk == DialogResult.Yes)
            {
                _library.HttpServerObject.Stop();
                SaveWindowSettings();
            }
            else
                e.Cancel = true;
        }

        private async void MainWindow_Load(object sender, EventArgs e)
        {
            // get profile directories and populate "profiles" combobox
            string[] profiles = _library.CheckProfiles();
            panelProfilesTopBarControl.PopulateProfiles(profiles, activeProfile);
            panelProfilesControl.PopulateProfiles(profiles, activeProfile);

            await _library.ValidateUserSettingsFromFiles();
            if (_library.UserInfoObject.Name != "")
            {
                panelSettingsControl.userInfoTransit = _library.UserInfoObject;

                bool tokenIsStillValid = false;
                try
                {
                    List<string> rewards = await panelSettingsControl.UpdateSettingsRewards();
                    SwitchPanel(panelCharacters);
                    tokenIsStillValid = true;
                }
                catch (UnauthorizedAccessException)
                {
                    SwitchPanel(panelSettings);
                }
                await panelSettingsControl.PopulateUserInfo(tokenIsStillValid);
            } else
            {
                SwitchPanel(panelSettings);
            }

            _library.HttpServerObject.Start();

            // check for GUI app updates
            await CheckUpdate(updateurl);
            int characterListVersion = _library.GetDefaultCharacterListVersion();
            try
            {
                (bool httpOk, int latestCharacterListVersion) = await _library.CheckCharacterListUpdate();

                // if header version number is missing, characterListVersion == 0 
                if (characterListVersion < latestCharacterListVersion || characterListVersion == 0)
                {
                    lblLatestCharacterList.Visible = true;
                    btnGetLatestCharacterList.Show();
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
        private void btnPanelProfiles_Click(object sender, EventArgs e) => SwitchPanel(panelProfiles);
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

        private async void btnSave_Click(object sender, EventArgs e) => SaveProfile(panelProfilesControl.selectedProfile);

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

        private async void btnUpdateNotif_Click(object sender, EventArgs e) => await GetUpdate_Ask();
        #endregion
    }
}