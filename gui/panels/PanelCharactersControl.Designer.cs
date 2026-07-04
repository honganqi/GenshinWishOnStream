namespace GenshinImpact_WishOnStreamGUI
{
    partial class PanelCharactersControl
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
            this.btnSortCharacters = new System.Windows.Forms.Button();
            this.btnAddStarValue = new System.Windows.Forms.Button();
            this.labelTitleCharacters = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnSortCharacters
            // 
            this.btnSortCharacters.AutoSize = true;
            this.btnSortCharacters.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSortCharacters.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSortCharacters.Location = new System.Drawing.Point(314, 11);
            this.btnSortCharacters.Name = "btnSortCharacters";
            this.btnSortCharacters.Size = new System.Drawing.Size(54, 25);
            this.btnSortCharacters.TabIndex = 18;
            this.btnSortCharacters.Text = " Sort";
            this.btnSortCharacters.UseVisualStyleBackColor = true;
            this.btnSortCharacters.Click += new System.EventHandler(this.btnSortCharacters_Click);
            // 
            // btnAddStarValue
            // 
            this.btnAddStarValue.AutoSize = true;
            this.btnAddStarValue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddStarValue.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddStarValue.Location = new System.Drawing.Point(188, 11);
            this.btnAddStarValue.Name = "btnAddStarValue";
            this.btnAddStarValue.Size = new System.Drawing.Size(108, 25);
            this.btnAddStarValue.TabIndex = 17;
            this.btnAddStarValue.Text = " Add Star Value";
            this.btnAddStarValue.UseVisualStyleBackColor = true;
            this.btnAddStarValue.Click += new System.EventHandler(this.btnAddStarValue_Click);
            // 
            // labelTitleCharacters
            // 
            this.labelTitleCharacters.AutoSize = true;
            this.labelTitleCharacters.Font = new System.Drawing.Font("Segoe UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitleCharacters.Location = new System.Drawing.Point(12, 3);
            this.labelTitleCharacters.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTitleCharacters.Name = "labelTitleCharacters";
            this.labelTitleCharacters.Size = new System.Drawing.Size(153, 37);
            this.labelTitleCharacters.TabIndex = 16;
            this.labelTitleCharacters.Text = "Characters";
            // 
            // PanelCharactersControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.btnSortCharacters);
            this.Controls.Add(this.btnAddStarValue);
            this.Controls.Add(this.labelTitleCharacters);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Name = "PanelCharactersControl";
            this.Size = new System.Drawing.Size(371, 40);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSortCharacters;
        private System.Windows.Forms.Button btnAddStarValue;
        private System.Windows.Forms.Label labelTitleCharacters;
    }
}
