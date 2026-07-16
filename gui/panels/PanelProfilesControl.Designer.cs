namespace GenshinImpact_WishOnStreamGUI
{
    partial class PanelProfilesControl
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
            this.panelNewProfile = new System.Windows.Forms.Panel();
            this.btnProfileCreateCancel = new System.Windows.Forms.Button();
            this.lblProfileNewName = new System.Windows.Forms.Label();
            this.txtNewProfile = new System.Windows.Forms.TextBox();
            this.btnProfileCreateSave = new System.Windows.Forms.Button();
            this.lblDownloadImagesStatus = new System.Windows.Forms.Label();
            this.progressDownloadImages = new System.Windows.Forms.ProgressBar();
            this.btnGetImages = new System.Windows.Forms.Button();
            this.btnProfileCreate = new System.Windows.Forms.Button();
            this.btnResetDefaults = new System.Windows.Forms.Button();
            this.cmbProfiles = new System.Windows.Forms.ComboBox();
            this.labelProfiles = new System.Windows.Forms.Label();
            this.labelTitleSettings = new System.Windows.Forms.Label();
            this.btnProfileActivate = new System.Windows.Forms.Button();
            this.lblActiveProfile = new System.Windows.Forms.Label();
            this.lblActiveProfileText = new System.Windows.Forms.Label();
            this.lblSelectedProfile = new System.Windows.Forms.Label();
            this.lblSelectedProfileText = new System.Windows.Forms.Label();
            this.btnDeleteProfile = new System.Windows.Forms.Button();
            this.panelNewProfile.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelNewProfile
            // 
            this.panelNewProfile.Controls.Add(this.btnProfileCreateCancel);
            this.panelNewProfile.Controls.Add(this.lblProfileNewName);
            this.panelNewProfile.Controls.Add(this.txtNewProfile);
            this.panelNewProfile.Controls.Add(this.btnProfileCreateSave);
            this.panelNewProfile.Location = new System.Drawing.Point(22, 187);
            this.panelNewProfile.Name = "panelNewProfile";
            this.panelNewProfile.Size = new System.Drawing.Size(598, 130);
            this.panelNewProfile.TabIndex = 70;
            this.panelNewProfile.Visible = false;
            // 
            // btnProfileCreateCancel
            // 
            this.btnProfileCreateCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnProfileCreateCancel.Location = new System.Drawing.Point(424, 54);
            this.btnProfileCreateCancel.Name = "btnProfileCreateCancel";
            this.btnProfileCreateCancel.Size = new System.Drawing.Size(67, 25);
            this.btnProfileCreateCancel.TabIndex = 67;
            this.btnProfileCreateCancel.Text = " Cancel";
            this.btnProfileCreateCancel.UseVisualStyleBackColor = true;
            this.btnProfileCreateCancel.Click += new System.EventHandler(this.btnProfileCreateCancel_Click);
            // 
            // lblProfileNewName
            // 
            this.lblProfileNewName.AutoSize = true;
            this.lblProfileNewName.Location = new System.Drawing.Point(10, 10);
            this.lblProfileNewName.Name = "lblProfileNewName";
            this.lblProfileNewName.Size = new System.Drawing.Size(114, 17);
            this.lblProfileNewName.TabIndex = 65;
            this.lblProfileNewName.Text = "New Profile Name";
            // 
            // txtNewProfile
            // 
            this.txtNewProfile.Location = new System.Drawing.Point(132, 7);
            this.txtNewProfile.Name = "txtNewProfile";
            this.txtNewProfile.Size = new System.Drawing.Size(286, 25);
            this.txtNewProfile.TabIndex = 64;
            // 
            // btnProfileCreateSave
            // 
            this.btnProfileCreateSave.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnProfileCreateSave.Location = new System.Drawing.Point(424, 6);
            this.btnProfileCreateSave.Name = "btnProfileCreateSave";
            this.btnProfileCreateSave.Size = new System.Drawing.Size(140, 27);
            this.btnProfileCreateSave.TabIndex = 63;
            this.btnProfileCreateSave.Text = " Save as New Profile";
            this.btnProfileCreateSave.UseVisualStyleBackColor = true;
            this.btnProfileCreateSave.Click += new System.EventHandler(this.btnProfileCreateSave_Click);
            // 
            // lblDownloadImagesStatus
            // 
            this.lblDownloadImagesStatus.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.lblDownloadImagesStatus.Location = new System.Drawing.Point(301, 296);
            this.lblDownloadImagesStatus.Name = "lblDownloadImagesStatus";
            this.lblDownloadImagesStatus.Size = new System.Drawing.Size(139, 13);
            this.lblDownloadImagesStatus.TabIndex = 73;
            this.lblDownloadImagesStatus.Text = "Downloading...";
            this.lblDownloadImagesStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDownloadImagesStatus.Visible = false;
            // 
            // progressDownloadImages
            // 
            this.progressDownloadImages.Location = new System.Drawing.Point(301, 268);
            this.progressDownloadImages.Name = "progressDownloadImages";
            this.progressDownloadImages.Size = new System.Drawing.Size(139, 23);
            this.progressDownloadImages.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressDownloadImages.TabIndex = 72;
            this.progressDownloadImages.Visible = false;
            // 
            // btnGetImages
            // 
            this.btnGetImages.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGetImages.Location = new System.Drawing.Point(299, 238);
            this.btnGetImages.Name = "btnGetImages";
            this.btnGetImages.Size = new System.Drawing.Size(141, 27);
            this.btnGetImages.TabIndex = 71;
            this.btnGetImages.Text = "Get Default Images";
            this.btnGetImages.UseVisualStyleBackColor = true;
            this.btnGetImages.Click += new System.EventHandler(this.btnGetImages_Click);
            // 
            // btnProfileCreate
            // 
            this.btnProfileCreate.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.btnProfileCreate.Location = new System.Drawing.Point(151, 188);
            this.btnProfileCreate.Name = "btnProfileCreate";
            this.btnProfileCreate.Size = new System.Drawing.Size(142, 27);
            this.btnProfileCreate.TabIndex = 68;
            this.btnProfileCreate.Text = " Create New Profile";
            this.btnProfileCreate.UseVisualStyleBackColor = true;
            this.btnProfileCreate.Click += new System.EventHandler(this.btnProfileCreate_Click);
            // 
            // btnResetDefaults
            // 
            this.btnResetDefaults.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnResetDefaults.Location = new System.Drawing.Point(151, 238);
            this.btnResetDefaults.Name = "btnResetDefaults";
            this.btnResetDefaults.Size = new System.Drawing.Size(142, 27);
            this.btnResetDefaults.TabIndex = 67;
            this.btnResetDefaults.Text = " Reset Default Profile";
            this.btnResetDefaults.UseVisualStyleBackColor = true;
            this.btnResetDefaults.Click += new System.EventHandler(this.btnResetDefaults_Click);
            // 
            // cmbProfiles
            // 
            this.cmbProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProfiles.FormattingEnabled = true;
            this.cmbProfiles.Location = new System.Drawing.Point(152, 62);
            this.cmbProfiles.Name = "cmbProfiles";
            this.cmbProfiles.Size = new System.Drawing.Size(288, 25);
            this.cmbProfiles.TabIndex = 75;
            this.cmbProfiles.DropDown += new System.EventHandler(this.cmbProfiles_DropDown);
            this.cmbProfiles.SelectedIndexChanged += new System.EventHandler(this.cmbProfiles_SelectedIndexChanged);
            // 
            // labelProfiles
            // 
            this.labelProfiles.AutoSize = true;
            this.labelProfiles.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelProfiles.Location = new System.Drawing.Point(86, 65);
            this.labelProfiles.Name = "labelProfiles";
            this.labelProfiles.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.labelProfiles.Size = new System.Drawing.Size(60, 17);
            this.labelProfiles.TabIndex = 74;
            this.labelProfiles.Text = "Profile";
            // 
            // labelTitleSettings
            // 
            this.labelTitleSettings.AutoSize = true;
            this.labelTitleSettings.Font = new System.Drawing.Font("Segoe UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitleSettings.Location = new System.Drawing.Point(12, 9);
            this.labelTitleSettings.Name = "labelTitleSettings";
            this.labelTitleSettings.Size = new System.Drawing.Size(115, 37);
            this.labelTitleSettings.TabIndex = 76;
            this.labelTitleSettings.Text = "Profiles";
            // 
            // btnProfileActivate
            // 
            this.btnProfileActivate.Font = new System.Drawing.Font("Segoe UI Symbol", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnProfileActivate.Location = new System.Drawing.Point(446, 61);
            this.btnProfileActivate.Name = "btnProfileActivate";
            this.btnProfileActivate.Size = new System.Drawing.Size(108, 27);
            this.btnProfileActivate.TabIndex = 77;
            this.btnProfileActivate.Text = "✔ Set as Active";
            this.btnProfileActivate.UseVisualStyleBackColor = true;
            this.btnProfileActivate.Click += new System.EventHandler(this.btnProfileActivate_Click);
            // 
            // lblActiveProfile
            // 
            this.lblActiveProfile.AutoSize = true;
            this.lblActiveProfile.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblActiveProfile.Location = new System.Drawing.Point(63, 103);
            this.lblActiveProfile.Name = "lblActiveProfile";
            this.lblActiveProfile.Size = new System.Drawing.Size(83, 17);
            this.lblActiveProfile.TabIndex = 78;
            this.lblActiveProfile.Text = "Active Profile";
            // 
            // lblActiveProfileText
            // 
            this.lblActiveProfileText.AutoSize = true;
            this.lblActiveProfileText.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblActiveProfileText.Location = new System.Drawing.Point(152, 103);
            this.lblActiveProfileText.Name = "lblActiveProfileText";
            this.lblActiveProfileText.Size = new System.Drawing.Size(78, 17);
            this.lblActiveProfileText.TabIndex = 79;
            this.lblActiveProfileText.Text = "activeProfile";
            // 
            // lblSelectedProfile
            // 
            this.lblSelectedProfile.AutoSize = true;
            this.lblSelectedProfile.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectedProfile.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblSelectedProfile.Location = new System.Drawing.Point(152, 129);
            this.lblSelectedProfile.Name = "lblSelectedProfile";
            this.lblSelectedProfile.Size = new System.Drawing.Size(216, 13);
            this.lblSelectedProfile.TabIndex = 80;
            this.lblSelectedProfile.Text = "You are currently editing another profile:";
            this.lblSelectedProfile.Visible = false;
            // 
            // lblSelectedProfileText
            // 
            this.lblSelectedProfileText.AutoSize = true;
            this.lblSelectedProfileText.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblSelectedProfileText.Location = new System.Drawing.Point(152, 143);
            this.lblSelectedProfileText.Name = "lblSelectedProfileText";
            this.lblSelectedProfileText.Size = new System.Drawing.Size(93, 17);
            this.lblSelectedProfileText.TabIndex = 81;
            this.lblSelectedProfileText.Text = "selectedProfile";
            this.lblSelectedProfileText.Visible = false;
            // 
            // btnDeleteProfile
            // 
            this.btnDeleteProfile.Font = new System.Drawing.Font("Segoe UI Symbol", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDeleteProfile.Location = new System.Drawing.Point(446, 99);
            this.btnDeleteProfile.Name = "btnDeleteProfile";
            this.btnDeleteProfile.Size = new System.Drawing.Size(108, 27);
            this.btnDeleteProfile.TabIndex = 82;
            this.btnDeleteProfile.Text = "Delete Profile";
            this.btnDeleteProfile.UseVisualStyleBackColor = true;
            this.btnDeleteProfile.Click += new System.EventHandler(this.btnDeleteProfile_Click);
            // 
            // PanelProfilesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.btnDeleteProfile);
            this.Controls.Add(this.panelNewProfile);
            this.Controls.Add(this.lblSelectedProfileText);
            this.Controls.Add(this.lblSelectedProfile);
            this.Controls.Add(this.lblActiveProfileText);
            this.Controls.Add(this.lblActiveProfile);
            this.Controls.Add(this.btnProfileActivate);
            this.Controls.Add(this.labelTitleSettings);
            this.Controls.Add(this.cmbProfiles);
            this.Controls.Add(this.labelProfiles);
            this.Controls.Add(this.lblDownloadImagesStatus);
            this.Controls.Add(this.btnGetImages);
            this.Controls.Add(this.progressDownloadImages);
            this.Controls.Add(this.btnProfileCreate);
            this.Controls.Add(this.btnResetDefaults);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Name = "PanelProfilesControl";
            this.Size = new System.Drawing.Size(623, 320);
            this.panelNewProfile.ResumeLayout(false);
            this.panelNewProfile.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelNewProfile;
        private System.Windows.Forms.Button btnProfileCreateCancel;
        private System.Windows.Forms.Label lblProfileNewName;
        private System.Windows.Forms.TextBox txtNewProfile;
        private System.Windows.Forms.Button btnProfileCreateSave;
        private System.Windows.Forms.Label lblDownloadImagesStatus;
        private System.Windows.Forms.ProgressBar progressDownloadImages;
        private System.Windows.Forms.Button btnGetImages;
        private System.Windows.Forms.Button btnProfileCreate;
        private System.Windows.Forms.Button btnResetDefaults;
        private System.Windows.Forms.ComboBox cmbProfiles;
        private System.Windows.Forms.Label labelProfiles;
        private System.Windows.Forms.Label labelTitleSettings;
        private System.Windows.Forms.Button btnProfileActivate;
        private System.Windows.Forms.Label lblActiveProfile;
        private System.Windows.Forms.Label lblActiveProfileText;
        private System.Windows.Forms.Label lblSelectedProfile;
        private System.Windows.Forms.Label lblSelectedProfileText;
        private System.Windows.Forms.Button btnDeleteProfile;
    }
}
