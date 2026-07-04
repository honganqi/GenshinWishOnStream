namespace GenshinImpact_WishOnStreamGUI
{
    partial class PanelDullBladesControl
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
            this.btnSortDullBlades = new System.Windows.Forms.Button();
            this.labelTitleDullBlades = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnSortDullBlades
            // 
            this.btnSortDullBlades.AutoSize = true;
            this.btnSortDullBlades.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSortDullBlades.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSortDullBlades.Location = new System.Drawing.Point(188, 11);
            this.btnSortDullBlades.Name = "btnSortDullBlades";
            this.btnSortDullBlades.Size = new System.Drawing.Size(54, 25);
            this.btnSortDullBlades.TabIndex = 16;
            this.btnSortDullBlades.Text = " Sort";
            this.btnSortDullBlades.UseVisualStyleBackColor = true;
            this.btnSortDullBlades.Click += new System.EventHandler(this.btnSortDullBlades_Click);
            // 
            // labelTitleDullBlades
            // 
            this.labelTitleDullBlades.AutoSize = true;
            this.labelTitleDullBlades.Font = new System.Drawing.Font("Segoe UI", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitleDullBlades.Location = new System.Drawing.Point(12, 3);
            this.labelTitleDullBlades.Name = "labelTitleDullBlades";
            this.labelTitleDullBlades.Size = new System.Drawing.Size(160, 37);
            this.labelTitleDullBlades.TabIndex = 17;
            this.labelTitleDullBlades.Text = "Dull Blades";
            // 
            // PanelDullBladesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.btnSortDullBlades);
            this.Controls.Add(this.labelTitleDullBlades);
            this.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.Name = "PanelDullBladesControl";
            this.Size = new System.Drawing.Size(245, 40);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSortDullBlades;
        private System.Windows.Forms.Label labelTitleDullBlades;
    }
}
