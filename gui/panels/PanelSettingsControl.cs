using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenshinImpact_WishOnStreamGUI
{
    public partial class PanelSettingsControl : UserControl
    {
        static readonly string authUrl = "https://genshin-twitch.sidestreamnetwork.net/auth";
        public UserInfo userInfoTransit;
        public Func<Task<List<string>>> GetCustomRewards;
        public Action<UserInfo, bool, bool> SaveUserSettingsToFile;
        private bool _pendingChanges;
        public event EventHandler PendingChangesChanged;
        public bool PendingChanges
        {
            get => _pendingChanges;
            set
            {
                if (_pendingChanges != value)
                {
                    _pendingChanges = value;
                    PendingChangesChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public PanelSettingsControl()
        {
            InitializeComponent();
            imgTwitchConnect.Image = Images.Load("twitch_connect");
            PendingChangesChanged += (sender, e) =>
            {
                btnSaveUserSettings.Enabled = true;
            };
        }

        #region Functions
        public void ExtractUserInfo()
        {
            userInfoTransit.Redeem = cmbRedeems.Text.Trim();
            userInfoTransit.RedeemEnabled = userInfoTransit.Redeem != "" && chkRedeems.Checked;
            userInfoTransit.TwitchCommandPrefix = txtCommand.Text.Trim();
            userInfoTransit.TwitchCommandEnabled = userInfoTransit.TwitchCommandPrefix != "" && chkCommand.Checked;
            userInfoTransit.Duration = 8000;
            if (txtDuration.Text.Trim() != "")
                if (int.TryParse(txtDuration.Text.Trim(), out int duration))
                    userInfoTransit.Duration = duration;
        }

        public async Task PopulateUserInfo(bool tokenIsStillValid)
        {
            if (InvokeRequired)
            {
                await (Task)Invoke(new Func<Task>(async () =>
                {
                    await PopulateUserInfo(tokenIsStillValid);
                }));

                return;
            }
            txtUsername.Text = userInfoTransit.Name;

            if (userInfoTransit.Name != "")
            {
                if (tokenIsStillValid)
                {
                    imgTwitchConnect.Visible = false;
                    btnCopyAuthLink.Visible = false;
                    lblTokenExpired.Visible = false;
                    btnRevokeToken.Enabled = true;
                    btnUpdateRewards.Enabled = true;
                }
                chkRedeems.Enabled = true;
                chkCommand.Enabled = true;
                txtCommand.Enabled = false;

                cmbRedeems.Enabled = userInfoTransit.RedeemEnabled;
                chkRedeems.Checked = userInfoTransit.RedeemEnabled;
                if (userInfoTransit.Name != "")
                    cmbRedeems.SelectedIndex = cmbRedeems.FindStringExact(userInfoTransit.Redeem);
                else
                    cmbRedeems.Items.Clear();

                txtCommand.Text = userInfoTransit.TwitchCommandPrefix;
                chkCommand.Checked = userInfoTransit.TwitchCommandEnabled;
                txtDuration.Enabled = true;
                txtDuration.Text = userInfoTransit.Duration.ToString();
            }
        }

        public async Task<List<string>> UpdateSettingsRewards()
        {
            labelPullRewards.Visible = true;
            List<string> rewards = new();

            try
            {
                rewards = await GetCustomRewards.Invoke();
                cmbRedeems.Items.Clear();
                foreach (string reward in userInfoTransit.Rewards)
                    cmbRedeems.Items.Add(reward);

                BeginInvoke(new Action(() =>
                {
                    cmbRedeems.Enabled = userInfoTransit.RedeemEnabled;
                    cmbRedeems.SelectedItem = userInfoTransit.Redeem;
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
                throw new UnauthorizedAccessException(ex.Message);
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
            return rewards;
        }
        public void ResetUser(bool fromExpiredToken = false)
        {
            if (InvokeRequired)
            {
                Invoke(() => ResetUser(fromExpiredToken));
                return;
            }
            SaveUserSettings(revoke: true, fromExpiredToken);

            // manually reset the controls, because
            imgTwitchConnect.Visible = true;
            btnCopyAuthLink.Visible = true;
            btnRevokeToken.Enabled = false;
            btnUpdateRewards.Enabled = false;
            chkRedeems.Enabled = false;
            chkCommand.Enabled = false;
            txtCommand.Text = "";
            txtCommand.Enabled = false;
            cmbRedeems.Items.Clear();
            cmbRedeems.Enabled = false;
            txtDuration.Text = "8000";
        }

        public void SaveUserSettings(bool revoke = false, bool fromExpiredToken = false)
        {
            ExtractUserInfo();
            SaveUserSettingsToFile.Invoke(userInfoTransit, revoke, fromExpiredToken);
        }

        public void ShowExpiredNotice(string message)
        {
            lblTokenExpired.Text = message;
            lblTokenExpired.Show();
            imgTwitchConnect.Show();
        }
        #endregion



        #region Buttons
        private async void btnUpdateRewards_Click(object sender, EventArgs e) => await UpdateSettingsRewards();
        private void btnCopyAuthLink_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(authUrl);
            MessageBox.Show("Link copied!", "", MessageBoxButtons.OK);
        }
        private void chkCommand_CheckedChanged(object sender, EventArgs e)
        {
            if (userInfoTransit.TwitchCommandEnabled != chkCommand.Checked)
                PendingChanges = true;
            userInfoTransit.TwitchCommandEnabled = chkCommand.Checked;
            txtCommand.Enabled = chkCommand.Checked;
        }
        private void btnRevokeToken_Click(object sender, EventArgs e) => ResetUser();
        private void imgTwitchConnect_Click(object sender, EventArgs e) => System.Diagnostics.Process.Start(authUrl);
        private void chkRedeems_CheckedChanged(object sender, EventArgs e)
        {
            if (userInfoTransit.RedeemEnabled != chkRedeems.Checked)
                PendingChanges = true;
            userInfoTransit.RedeemEnabled = chkRedeems.Checked;
            cmbRedeems.Enabled = chkRedeems.Checked;
        }
        #endregion

        private void btnSaveUserSettings_Click(object sender, EventArgs e)
        {
            string message = "User settings saved successfully!";
            if (chkCommand.Checked && txtCommand.Text.Trim() == "")
            {
                message += "\n\nCommand string cannot be blank. Setting reverted.";
                chkCommand.Checked = false;
            }

            SaveUserSettings();
            PendingChanges = false;
            MessageBox.Show(message, "User settings saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
