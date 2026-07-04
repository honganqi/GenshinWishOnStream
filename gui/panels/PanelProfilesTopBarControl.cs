using System;
using System.Drawing;
using System.Windows.Forms;

namespace GenshinImpact_WishOnStreamGUI.panels
{
    public partial class PanelProfilesTopBarControl : UserControl
    {
        private bool _updatingCmb;

        public Func<string, bool> ActivateProfile;
        public Func<string, bool> ChangeProfile;

        public string activeProfile;
        public string selectedProfile;

        public PanelProfilesTopBarControl()
        {
            InitializeComponent();
        }

        public void SetActiveProfile(string profile)
        {
            activeProfile = profile;
            lblActiveProfileText.Text = activeProfile;
        }

        public void PopulateProfiles(string[] profiles, string selectedProfile = "")
        {
            cmbProfiles.Items.Clear();
            cmbProfiles.Items.AddRange(profiles);
            if (selectedProfile != "")
                cmbProfiles.SelectedIndex = cmbProfiles.FindStringExact(selectedProfile);
        }
        public void SetSelectedProfile(string profile)
        {
            _updatingCmb = true;
            selectedProfile = profile;
            cmbProfiles.SelectedIndex = cmbProfiles.FindStringExact(selectedProfile);
            _updatingCmb = false;
        }

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

            cmbProfiles.SelectedIndex = cmbProfiles.FindStringExact(selectedProfile);
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
        private void btnProfileActivate_Click(object sender, EventArgs e)
        {
            if (ActivateProfile.Invoke(cmbProfiles.Text))
                SetActiveProfile(cmbProfiles.Text);
        }
        #endregion

    }
}
