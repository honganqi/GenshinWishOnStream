namespace GenshinImpact_WishOnStreamGUI
{
    partial class PanelSettingsControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTokenExpired = new System.Windows.Forms.Label();
            this.labelPullRewards = new System.Windows.Forms.Label();
            this.btnUpdateRewards = new System.Windows.Forms.Button();
            this.btnCopyAuthLink = new System.Windows.Forms.Label();
            this.labelCommandTip = new System.Windows.Forms.Label();
            this.chkCommand = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtCommand = new System.Windows.Forms.TextBox();
            this.btnRevokeToken = new System.Windows.Forms.Button();
            this.cmbRedeems = new System.Windows.Forms.ComboBox();
            this.labelTokenExpiry = new System.Windows.Forms.Label();
            this.imgTwitchConnect = new System.Windows.Forms.PictureBox();
            this.labelDurationMS = new System.Windows.Forms.Label();
            this.txtDuration = new System.Windows.Forms.TextBox();
            this.labelDuration = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.labelRedeem = new System.Windows.Forms.Label();
            this.labelUsername = new System.Windows.Forms.Label();
            this.labelTitleSettings = new System.Windows.Forms.Label();
            this.chkRedeems = new System.Windows.Forms.CheckBox();
            this.cmbProfiles = new System.Windows.Forms.ComboBox();
            this.btnSaveUserSettings = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.imgTwitchConnect)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTokenExpired
            // 
            this.lblTokenExpired.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblTokenExpired.ForeColor = System.Drawing.Color.Red;
            this.lblTokenExpired.Location = new System.Drawing.Point(401, 87);
            this.lblTokenExpired.Name = "lblTokenExpired";
            this.lblTokenExpired.Size = new System.Drawing.Size(303, 36);
            this.lblTokenExpired.TabIndex = 63;
            this.lblTokenExpired.Text = "expired";
            this.lblTokenExpired.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblTokenExpired.Visible = false;
            // 
            // labelPullRewards
            // 
            this.labelPullRewards.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.labelPullRewards.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPullRewards.Location = new System.Drawing.Point(220, 147);
            this.labelPullRewards.Name = "labelPullRewards";
            this.labelPullRewards.Size = new System.Drawing.Size(227, 13);
            this.labelPullRewards.TabIndex = 49;
            this.labelPullRewards.Text = "Getting Channel Points Rewards...";
            this.labelPullRewards.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelPullRewards.Visible = false;
            // 
            // btnUpdateRewards
            // 
            this.btnUpdateRewards.Enabled = false;
            this.btnUpdateRewards.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUpdateRewards.Location = new System.Drawing.Point(468, 140);
            this.btnUpdateRewards.Name = "btnUpdateRewards";
            this.btnUpdateRewards.Size = new System.Drawing.Size(106, 27);
            this.btnUpdateRewards.TabIndex = 47;
            this.btnUpdateRewards.Text = "Refresh Rewards";
            this.btnUpdateRewards.UseVisualStyleBackColor = true;
            this.btnUpdateRewards.Click += new System.EventHandler(this.btnUpdateRewards_Click);
            // 
            // btnCopyAuthLink
            // 
            this.btnCopyAuthLink.AutoSize = true;
            this.btnCopyAuthLink.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopyAuthLink.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnCopyAuthLink.Location = new System.Drawing.Point(201, 124);
            this.btnCopyAuthLink.Name = "btnCopyAuthLink";
            this.btnCopyAuthLink.Size = new System.Drawing.Size(192, 13);
            this.btnCopyAuthLink.TabIndex = 46;
            this.btnCopyAuthLink.Text = "(click here if using another browser)";
            this.btnCopyAuthLink.Click += new System.EventHandler(this.btnCopyAuthLink_Click);
            // 
            // labelCommandTip
            // 
            this.labelCommandTip.AutoSize = true;
            this.labelCommandTip.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCommandTip.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelCommandTip.Location = new System.Drawing.Point(217, 200);
            this.labelCommandTip.Name = "labelCommandTip";
            this.labelCommandTip.Size = new System.Drawing.Size(314, 13);
            this.labelCommandTip.TabIndex = 45;
            this.labelCommandTip.Text = "include your preferred special symbol (@ and / not allowed)";
            // 
            // chkCommand
            // 
            this.chkCommand.AutoSize = true;
            this.chkCommand.Enabled = false;
            this.chkCommand.Location = new System.Drawing.Point(197, 178);
            this.chkCommand.Name = "chkCommand";
            this.chkCommand.Size = new System.Drawing.Size(15, 14);
            this.chkCommand.TabIndex = 44;
            this.chkCommand.UseVisualStyleBackColor = true;
            this.chkCommand.CheckedChanged += new System.EventHandler(this.chkCommand_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(84, 175);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 17);
            this.label3.TabIndex = 43;
            this.label3.Text = "Twitch Command";
            // 
            // txtCommand
            // 
            this.txtCommand.Enabled = false;
            this.txtCommand.Location = new System.Drawing.Point(218, 172);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.Size = new System.Drawing.Size(179, 25);
            this.txtCommand.TabIndex = 42;
            // 
            // btnRevokeToken
            // 
            this.btnRevokeToken.AutoSize = true;
            this.btnRevokeToken.Enabled = false;
            this.btnRevokeToken.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRevokeToken.Location = new System.Drawing.Point(468, 55);
            this.btnRevokeToken.Name = "btnRevokeToken";
            this.btnRevokeToken.Size = new System.Drawing.Size(128, 27);
            this.btnRevokeToken.TabIndex = 40;
            this.btnRevokeToken.Text = "Disconnect and Reset";
            this.btnRevokeToken.UseVisualStyleBackColor = true;
            this.btnRevokeToken.Click += new System.EventHandler(this.btnRevokeToken_Click);
            // 
            // cmbRedeems
            // 
            this.cmbRedeems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRedeems.Enabled = false;
            this.cmbRedeems.FormattingEnabled = true;
            this.cmbRedeems.Location = new System.Drawing.Point(218, 141);
            this.cmbRedeems.Name = "cmbRedeems";
            this.cmbRedeems.Size = new System.Drawing.Size(244, 25);
            this.cmbRedeems.TabIndex = 39;
            // 
            // labelTokenExpiry
            // 
            this.labelTokenExpiry.AutoSize = true;
            this.labelTokenExpiry.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTokenExpiry.ForeColor = System.Drawing.Color.Red;
            this.labelTokenExpiry.Location = new System.Drawing.Point(194, 84);
            this.labelTokenExpiry.Name = "labelTokenExpiry";
            this.labelTokenExpiry.Size = new System.Drawing.Size(0, 13);
            this.labelTokenExpiry.TabIndex = 38;
            // 
            // imgTwitchConnect
            // 
            this.imgTwitchConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.imgTwitchConnect.Location = new System.Drawing.Point(197, 87);
            this.imgTwitchConnect.Name = "imgTwitchConnect";
            this.imgTwitchConnect.Size = new System.Drawing.Size(200, 36);
            this.imgTwitchConnect.TabIndex = 37;
            this.imgTwitchConnect.TabStop = false;
            this.imgTwitchConnect.Click += new System.EventHandler(this.imgTwitchConnect_Click);
            // 
            // labelDurationMS
            // 
            this.labelDurationMS.AutoSize = true;
            this.labelDurationMS.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDurationMS.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelDurationMS.Location = new System.Drawing.Point(290, 228);
            this.labelDurationMS.Name = "labelDurationMS";
            this.labelDurationMS.Size = new System.Drawing.Size(89, 13);
            this.labelDurationMS.TabIndex = 36;
            this.labelDurationMS.Text = "(in milliseconds)";
            // 
            // txtDuration
            // 
            this.txtDuration.Enabled = false;
            this.txtDuration.Location = new System.Drawing.Point(197, 222);
            this.txtDuration.Name = "txtDuration";
            this.txtDuration.Size = new System.Drawing.Size(87, 25);
            this.txtDuration.TabIndex = 35;
            // 
            // labelDuration
            // 
            this.labelDuration.AutoSize = true;
            this.labelDuration.Location = new System.Drawing.Point(69, 224);
            this.labelDuration.Name = "labelDuration";
            this.labelDuration.Size = new System.Drawing.Size(122, 17);
            this.labelDuration.TabIndex = 34;
            this.labelDuration.Text = "Duration On Screen";
            // 
            // txtUsername
            // 
            this.txtUsername.Enabled = false;
            this.txtUsername.Location = new System.Drawing.Point(197, 56);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(265, 25);
            this.txtUsername.TabIndex = 32;
            // 
            // labelRedeem
            // 
            this.labelRedeem.AutoSize = true;
            this.labelRedeem.Location = new System.Drawing.Point(50, 144);
            this.labelRedeem.Name = "labelRedeem";
            this.labelRedeem.Size = new System.Drawing.Size(141, 17);
            this.labelRedeem.TabIndex = 31;
            this.labelRedeem.Text = "Channel Points Reward";
            // 
            // labelUsername
            // 
            this.labelUsername.AutoSize = true;
            this.labelUsername.Location = new System.Drawing.Point(85, 59);
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(106, 17);
            this.labelUsername.TabIndex = 30;
            this.labelUsername.Text = "Twitch Username";
            // 
            // labelTitleSettings
            // 
            this.labelTitleSettings.AutoSize = true;
            this.labelTitleSettings.Font = new System.Drawing.Font("Segoe UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitleSettings.Location = new System.Drawing.Point(12, 9);
            this.labelTitleSettings.Name = "labelTitleSettings";
            this.labelTitleSettings.Size = new System.Drawing.Size(122, 37);
            this.labelTitleSettings.TabIndex = 29;
            this.labelTitleSettings.Text = "Settings";
            // 
            // chkRedeems
            // 
            this.chkRedeems.AutoSize = true;
            this.chkRedeems.Enabled = false;
            this.chkRedeems.Location = new System.Drawing.Point(197, 147);
            this.chkRedeems.Name = "chkRedeems";
            this.chkRedeems.Size = new System.Drawing.Size(15, 14);
            this.chkRedeems.TabIndex = 41;
            this.chkRedeems.UseVisualStyleBackColor = true;
            this.chkRedeems.CheckedChanged += new System.EventHandler(this.chkRedeems_CheckedChanged);
            // 
            // cmbProfiles
            // 
            this.cmbProfiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProfiles.FormattingEnabled = true;
            this.cmbProfiles.Location = new System.Drawing.Point(1369, 216);
            this.cmbProfiles.Name = "cmbProfiles";
            this.cmbProfiles.Size = new System.Drawing.Size(149, 25);
            this.cmbProfiles.TabIndex = 68;
            // 
            // btnSaveUserSettings
            // 
            this.btnSaveUserSettings.Enabled = false;
            this.btnSaveUserSettings.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveUserSettings.Location = new System.Drawing.Point(197, 272);
            this.btnSaveUserSettings.Name = "btnSaveUserSettings";
            this.btnSaveUserSettings.Size = new System.Drawing.Size(116, 27);
            this.btnSaveUserSettings.TabIndex = 69;
            this.btnSaveUserSettings.Text = "Save User Settings";
            this.btnSaveUserSettings.UseVisualStyleBackColor = true;
            this.btnSaveUserSettings.Click += new System.EventHandler(this.btnSaveUserSettings_Click);
            // 
            // PanelSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.btnSaveUserSettings);
            this.Controls.Add(this.cmbProfiles);
            this.Controls.Add(this.lblTokenExpired);
            this.Controls.Add(this.labelPullRewards);
            this.Controls.Add(this.btnUpdateRewards);
            this.Controls.Add(this.btnCopyAuthLink);
            this.Controls.Add(this.labelCommandTip);
            this.Controls.Add(this.chkCommand);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtCommand);
            this.Controls.Add(this.btnRevokeToken);
            this.Controls.Add(this.cmbRedeems);
            this.Controls.Add(this.labelTokenExpiry);
            this.Controls.Add(this.imgTwitchConnect);
            this.Controls.Add(this.labelDurationMS);
            this.Controls.Add(this.txtDuration);
            this.Controls.Add(this.labelDuration);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.labelRedeem);
            this.Controls.Add(this.labelUsername);
            this.Controls.Add(this.labelTitleSettings);
            this.Controls.Add(this.chkRedeems);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Name = "PanelSettingsControl";
            this.Size = new System.Drawing.Size(759, 302);
            ((System.ComponentModel.ISupportInitialize)(this.imgTwitchConnect)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblTokenExpired;
        private System.Windows.Forms.Label labelPullRewards;
        private System.Windows.Forms.Button btnUpdateRewards;
        private System.Windows.Forms.Label btnCopyAuthLink;
        private System.Windows.Forms.Label labelCommandTip;
        private System.Windows.Forms.CheckBox chkCommand;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtCommand;
        private System.Windows.Forms.Button btnRevokeToken;
        private System.Windows.Forms.ComboBox cmbRedeems;
        private System.Windows.Forms.Label labelTokenExpiry;
        private System.Windows.Forms.PictureBox imgTwitchConnect;
        private System.Windows.Forms.Label labelDurationMS;
        private System.Windows.Forms.TextBox txtDuration;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label labelRedeem;
        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.Label labelTitleSettings;
        private System.Windows.Forms.CheckBox chkRedeems;
        private System.Windows.Forms.ComboBox cmbProfiles;
        private System.Windows.Forms.Button btnSaveUserSettings;
    }
}
