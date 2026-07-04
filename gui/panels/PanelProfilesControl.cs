using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenshinImpact_WishOnStreamGUI
{
    public partial class PanelProfilesControl : UserControl
    {
        private bool _updatingCmb;
        public string activeProfile;
        public string selectedProfile;

        public Action<string> SaveProfileToFile;
        public Action<string, string> SaveAsNewProfile;
        public Func<string, bool> ActivateProfile;
        public Func<List<string>, Task<bool>> ResetDownloadConfigs;
        public Action DownloadDefaultImages_Ask;
        public Func<string, bool> ChangeProfile;
        public Action UpdateProfileList;

        public PanelProfilesControl()
        {
            InitializeComponent();
        }

        #region Functions

        private async Task ResetConfig(List<string> fileList, string question, Button btn)
        {
            DialogResult exitAsk = MessageBox.Show(question, "Reset File", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            try
            {
                btn.Enabled = false;
                if (exitAsk == DialogResult.Yes)
                {
                    await ResetDownloadConfigs.Invoke(fileList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Download error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn.Enabled = true;
            }
        }

        public void PopulateProfiles(string[] profiles, string selectedProfile = "")
        {
            cmbProfiles.Items.Clear();
            cmbProfiles.Items.AddRange(profiles);
            if (selectedProfile != "")
                cmbProfiles.SelectedIndex = cmbProfiles.FindStringExact(selectedProfile);
        }

        public void SetActiveProfile(string profile)
        {
            activeProfile = profile;
            lblActiveProfileText.Text = activeProfile;
            UpdateProfileText();
        }

        public void SetSelectedProfile(string profile)
        {
            _updatingCmb = true;
            selectedProfile = profile;
            lblSelectedProfileText.Text = selectedProfile;
            cmbProfiles.SelectedIndex = cmbProfiles.FindStringExact(selectedProfile);
            UpdateProfileText();
            _updatingCmb = false;
        }

        private void UpdateProfileText()
        {
            if (selectedProfile != activeProfile)
            {
                lblSelectedProfileText.Text = selectedProfile;
                lblSelectedProfile.Show();
                lblSelectedProfileText.Show();
            }
            else
            {
                lblSelectedProfile.Hide();
                lblSelectedProfileText.Hide();
            }
        }

        private async Task InitializeDownloadDefaultImages() => DownloadDefaultImages_Ask.Invoke();

        public void UpdateProgress(DownloadProgress progress)
        {
            progressDownloadImages.Maximum = progress.FileCount;
            progressDownloadImages.Value = progress.FilesDownloaded;
            lblDownloadImagesStatus.Text = $"{progress.FilesDownloaded} of {progress.FileCount}";
        }

        public void SetDownloadStatus(bool inProgress)
        {
            lblDownloadImagesStatus.Visible = inProgress;
            progressDownloadImages.Visible = inProgress;
        }
        #endregion


        #region Controls
        private void cmbProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updatingCmb)
                return;

            string previousProfile = selectedProfile;
            selectedProfile = cmbProfiles.Text;

            // if files fail to validate, revert
            if (!ChangeProfile.Invoke(selectedProfile))
                selectedProfile = previousProfile;

            UpdateProfileText();
        }
        private void cmbProfiles_DropDownAutoResize(object sender)
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
        private void cmbProfiles_DropDown(object sender, EventArgs e) => cmbProfiles_DropDownAutoResize(sender);
        private async void btnGetImages_Click(object sender, EventArgs e) => await InitializeDownloadDefaultImages();
        private void btnProfileCreate_Click(object sender, EventArgs e) => panelNewProfile.Show();
        private void btnProfileCreateCancel_Click(object sender, EventArgs e)
        {
            panelNewProfile.Hide();
            txtNewProfile.Text = "";
        }
        private void btnProfileCreateSave_Click(object sender, EventArgs e)
        {
            // TO-DO: sanitize new name
            SaveProfileToFile.Invoke(txtNewProfile.Text);
            panelNewProfile.Hide();
            txtNewProfile.Text = "";
            UpdateProfileList.Invoke();
        }
        private async void btnResetDefaults_Click(object sender, EventArgs e)
        {
            List<string> list = ["choices.js", "rates.js"];
            string question = "If you customized the default profile, you might want to create a new profile based on this before you continue.\n\nAre you sure you want to reset the default profile?";
            Button btn = btnResetDefaults;
            await ResetConfig(list, question, btn);
        }
        private void btnProfileActivate_Click(object sender, EventArgs e)
        {
            if (ActivateProfile.Invoke(cmbProfiles.Text))
                SetActiveProfile(cmbProfiles.Text);
        }
        #endregion

    }
}
