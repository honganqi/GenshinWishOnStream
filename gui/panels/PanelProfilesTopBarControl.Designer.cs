namespace GenshinImpact_WishOnStreamGUI.panels
{
    partial class PanelProfilesTopBarControl
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
            this.lblActiveProfileText = new System.Windows.Forms.Label();
            this.lblActiveProfile = new System.Windows.Forms.Label();
            this.cmbProfiles = new System.Windows.Forms.ComboBox();
            this.labelProfiles = new System.Windows.Forms.Label();
            this.btnProfileActivate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblActiveProfileText
            // 
            this.lblActiveProfileText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblActiveProfileText.AutoSize = true;
            this.lblActiveProfileText.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblActiveProfileText.Location = new System.Drawing.Point(523, 12);
            this.lblActiveProfileText.Name = "lblActiveProfileText";
            this.lblActiveProfileText.Size = new System.Drawing.Size(78, 17);
            this.lblActiveProfileText.TabIndex = 84;
            this.lblActiveProfileText.Text = "activeProfile";
            // 
            // lblActiveProfile
            // 
            this.lblActiveProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblActiveProfile.AutoSize = true;
            this.lblActiveProfile.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblActiveProfile.Location = new System.Drawing.Point(434, 12);
            this.lblActiveProfile.Name = "lblActiveProfile";
            this.lblActiveProfile.Size = new System.Drawing.Size(83, 17);
            this.lblActiveProfile.TabIndex = 83;
            this.lblActiveProfile.Text = "Active Profile";
            // 
            // cmbProfiles
            // 
            this.cmbProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProfiles.FormattingEnabled = true;
            this.cmbProfiles.Location = new System.Drawing.Point(69, 9);
            this.cmbProfiles.Name = "cmbProfiles";
            this.cmbProfiles.Size = new System.Drawing.Size(211, 25);
            this.cmbProfiles.TabIndex = 6;
            this.cmbProfiles.DropDown += new System.EventHandler(this.cmbProfiles_DropDown);
            this.cmbProfiles.SelectedIndexChanged += new System.EventHandler(this.cmbProfiles_SelectedIndexChanged);
            // 
            // labelProfiles
            // 
            this.labelProfiles.AutoSize = true;
            this.labelProfiles.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelProfiles.Location = new System.Drawing.Point(18, 12);
            this.labelProfiles.Name = "labelProfiles";
            this.labelProfiles.Size = new System.Drawing.Size(45, 17);
            this.labelProfiles.TabIndex = 80;
            this.labelProfiles.Text = "Profile";
            // 
            // btnProfileActivate
            // 
            this.btnProfileActivate.Font = new System.Drawing.Font("Segoe UI Symbol", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnProfileActivate.Location = new System.Drawing.Point(286, 7);
            this.btnProfileActivate.Name = "btnProfileActivate";
            this.btnProfileActivate.Size = new System.Drawing.Size(108, 27);
            this.btnProfileActivate.TabIndex = 7;
            this.btnProfileActivate.Text = "✔ Set as Active";
            this.btnProfileActivate.UseVisualStyleBackColor = true;
            this.btnProfileActivate.Click += new System.EventHandler(this.btnProfileActivate_Click);
            // 
            // PanelProfilesTopBarControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.lblActiveProfileText);
            this.Controls.Add(this.lblActiveProfile);
            this.Controls.Add(this.btnProfileActivate);
            this.Controls.Add(this.cmbProfiles);
            this.Controls.Add(this.labelProfiles);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Name = "PanelProfilesTopBarControl";
            this.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.Size = new System.Drawing.Size(611, 42);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblActiveProfileText;
        private System.Windows.Forms.Label lblActiveProfile;
        private System.Windows.Forms.ComboBox cmbProfiles;
        private System.Windows.Forms.Label labelProfiles;
        private System.Windows.Forms.Button btnProfileActivate;
    }
}
