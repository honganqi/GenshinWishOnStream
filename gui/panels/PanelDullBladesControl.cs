using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GenshinImpact_WishOnStreamGUI
{
    public partial class PanelDullBladesControl : UserControl
    {
        // Characters "Panel" variables
        const int columnWidth = 140;
        const int rowHeight = 30;
        const int initYPos = 20;
        const int initXPos = 20;
        const int extraRows = 5;

        int lastItemYDullBlade = 0;

        public PanelDullBladesControl()
        {
            InitializeComponent();
        }

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
                Controls.Add(txtbox);
            }

            for (int rowNumAgain = rownum; rowNumAgain < (rownum + extraRows); rowNumAgain++)
            {
                yPos += rowHeight;
                TextBox txtbox = new();
                txtbox.Name = "txtDull_" + rowNumAgain;
                txtbox.Location = new Point(xPos, yPos);
                txtbox.Width = columnWidth;
                Controls.Add(txtbox);

                if ((yPos + rowHeight) > lastItemYDullBlade)
                    lastItemYDullBlade = (yPos + rowHeight);
            }
        }

        public List<string> ExtractDataFromDullBladesPanel()
        {
            List<string> dullBlades = new();
            List<string> errors = new();

            foreach (TextBox dullSearch in Controls.OfType<TextBox>().Where(l => l.Name.StartsWith("txtDull_")))
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
            foreach (Control control in Controls.OfType<Control>().Where(c => c.Name.StartsWith("txtDull_")))
                controls.Add(control);

            foreach (Control control in controls)
                Controls.Remove(control);
        }
    }
}
