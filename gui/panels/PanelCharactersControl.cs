using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace GenshinImpact_WishOnStreamGUI
{
    public partial class PanelCharactersControl : UserControl
    {
        // Characters "Panel" variables
        const int columnWidth = 140;
        const int elementColumnWidth = 100;
        const int columnMargin = 25;
        const int columnSplitter = 10;
        const int rowHeight = 30;
        const int initYPos = 50;
        const int initXPos = 20;
        const int extraRows = 5;

        int currentMax = -1;
        int currentMin = -1;

        public bool hasUnsavedChanges = false;

        public PanelCharactersControl()
        {
            InitializeComponent();
        }

        public void InitializeCharactersPanel(StarList starList)
        {
            ClearCharactersPanel();
            int xPos = initXPos;

            int starNum = 1;

            // initialize delete buttons
            foreach (string delPos in new List<string> { "Top", "Bot" })
            {
                Button btnDel = new()
                {
                    Name = "btnDel" + delPos,
                    Text = "\ue10A",
                    Font = new("Segoe UI", 8, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                btnDel.Click += new EventHandler(btnDel_Click);
                Controls.Add(btnDel);
            }

            foreach (KeyValuePair<int, CharacterListInStar> charListPair in starList.Reverse())
            {
                int starValue = charListPair.Key;
                CharacterListInStar charList = charListPair.Value;

                if (starNum == 1)
                    currentMax = starValue;
                else if (starNum == starList.Count)
                    currentMin = starValue;

                AddCharactersColumn(charListPair.Key, xPos, charListPair.Value);

                xPos += columnWidth + columnMargin + elementColumnWidth + columnSplitter;

                starNum++;
            }
        }

        public StarList ExtractDataFromCharactersPanel()
        {
            try
            {
                SortedDictionary<int, List<string>> characters = new();
                Dictionary<string, string> elementDictionary = new();
                int currentTotal = 0;

                StarList starList = new();
                List<string> errors = new();

                foreach (Label labelSearch in Controls.OfType<Label>().Where(l => l.Name.StartsWith("labelHeader_")))
                {
                    string[] pair = labelSearch.Name.Split('_');
                    if (int.TryParse(pair[1], out int starValue))
                    {
                        starList.AddStar(starValue);
                        Control txtbox = Controls["rateBox_" + starValue];
                        if (int.TryParse(txtbox.Text, out int rate))
                        {
                            if (rate <= 0)
                            {
                                errors.Add("Rate cannot be 0");
                            }
                                
                            starList[starValue].PullRate = rate;
                            currentTotal += rate;
                        }

                        int charNum = 0;
                        foreach (TextBox txtChar in Controls.OfType<TextBox>().Where(t => t.Name.StartsWith("txtChar_" + starValue)))
                        {
                            if (txtChar.Text.Trim() != "")
                            {
                                string charName = txtChar.Text.Trim();
                                starList[starValue].Add(charName);

                                Control txtElem = Controls["txtElem_" + starValue + "_" + charNum];
                                string element = txtElem.Text.Trim();
                                starList[charName].Element = element;

                                charNum++;
                            }
                        }
                        if (starList[starValue].Count == 0)
                        {
                            errors.Add($"There are no characters in the {starValue}-star column.");
                        }
                    }
                }

                if (starList.Count == 0)
                    errors.Add("No characters were found.");

                if (currentTotal != 100)
                {
                    starList = new();
                    errors.Add("Please ensure that your rates have a total of 100%. \n\nCurrent total: " + currentTotal);
                }

                if (errors.Count > 0)
                    throw new ArgumentException(string.Join("\n", errors.ToArray()));

                return starList;
            }
            catch (ArgumentException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void AddRow(int xPos, int yPos, int starValue, int charNum, string characterName, string characterElement)
        {
            TextBox txtbox = new()
            {
                Name = "txtChar_" + starValue + "_" + charNum,
                Text = characterName,
                Location = new Point(xPos, yPos),
                Width = columnWidth
            };
            txtbox.TextChanged += ControlChanged;
            Controls.Add(txtbox);

            TextBox elemtxtbox = new()
            {
                Name = "txtElem_" + starValue + "_" + charNum,
                Text = characterElement,
                Location = new Point(xPos + columnWidth + columnSplitter, yPos),
                Width = elementColumnWidth
            };
            elemtxtbox.TextChanged += ControlChanged;
            Controls.Add(elemtxtbox);
        }

        private void AddCharactersColumn(int starValue, int xPos = initXPos, CharacterListInStar characters = null)
        {
            int prevMin = currentMin;
            int prevMax = currentMax;

            if (starValue > 0)
            {
                int yPos = initYPos;

                Label headerLabel = new()
                {
                    Name = "labelHeader_" + starValue,
                    Text = starValue + "-Star",
                    Font = new("Segoe UI", 15, FontStyle.Bold),
                    Location = new Point(xPos, initYPos),
                    AutoSize = true
                };
                Controls.Add(headerLabel);

                TextBox rateBox = new()
                {
                    Name = "rateBox_" + starValue,
                    Text = characters != null ? characters.PullRate.ToString() : "0",
                    Location = new Point(xPos + headerLabel.Width, initYPos),
                    Width = 40
                };
                rateBox.KeyDown += new KeyEventHandler(ValidateRateInput);
                rateBox.TextChanged += ControlChanged;
                Controls.Add(rateBox);

                Label rateLabel = new()
                {
                    Name = "labelRate_" + starValue,
                    Text = "% Pull Rate",
                    Font = new("Segoe UI", 8, FontStyle.Bold),
                    Location = new Point(xPos + headerLabel.Width + rateBox.Width, initYPos),
                    Height = rateBox.Height,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                Controls.Add(rateLabel);

                int charNum = 0;
                if (characters != null)
                {
                    foreach (Character character in characters)
                    {
                        yPos += rowHeight;
                        AddRow(xPos, yPos, starValue, charNum, character.CharacterName, character.Element);
                        charNum++;
                    }
                }

                // create extra rows
                for (int charNumAgain = charNum; charNumAgain < (charNum + extraRows); charNumAgain++)
                {
                    yPos += rowHeight;
                    AddRow(xPos, yPos, starValue, charNumAgain, "", "");
                }

                // logic for delete buttons
                if (starValue > currentMax)
                {
                    foreach (Label labelSearch in Controls.OfType<Label>().Where(l => l.Name.StartsWith("labelHeader_")))
                    {
                        string[] pair = labelSearch.Name.Split('_');
                        if (int.TryParse(pair[1], out int currentValue))
                        {
                            // don't move the one being added
                            if (starValue != currentValue)
                            {
                                Control label = Controls["labelHeader_" + currentValue];
                                MoveCharacterControls(currentValue, 1);
                            }
                        }
                    }
                    currentMax = starValue;
                }
                else if (starValue < currentMin)
                {
                    currentMin = starValue;
                    MoveCharacterControls(starValue, (currentMax - currentMin));
                }

                Control[] findDelTop = Controls.Find("btnDelTop", true);
                Control moveDelTop = findDelTop.Length > 0 ? Controls["btnDelTop"] : Controls["btnDelTop_" + prevMax];
                moveDelTop.Name = "btnDelTop_" + currentMax;

                // reset the button dimensions upon creation after ratebox is created
                // findDelTop/findDelBot only exists on first run
                if (findDelTop.Length > 0)
                {
                    moveDelTop.Height = rateBox.Height;
                    moveDelTop.Width = rateBox.Height;
                    moveDelTop.Location = new Point(xPos + (columnWidth + elementColumnWidth + columnSplitter) - moveDelTop.Width, initYPos);    // Location needs to be set after instantiation to get its Width
                }

                if (currentMin > 0)
                {
                    Control[] findDelBot = Controls.Find("btnDelBot", true);
                    Control moveDelBot = findDelBot.Length > 0 ? Controls["btnDelBot"] : Controls["btnDelBot_" + prevMin];
                    moveDelBot.Name = "btnDelBot_" + currentMin;
                    moveDelBot.Location = new Point(moveDelBot.Location.X + (columnWidth + columnMargin + elementColumnWidth + columnSplitter), initYPos);

                    if (findDelBot.Length > 0)
                    {
                        moveDelBot.Height = rateBox.Height;
                        moveDelBot.Width = rateBox.Height;
                        moveDelBot.Location = new Point(xPos + (columnWidth + elementColumnWidth + columnSplitter) - moveDelBot.Width, initYPos);    // Location needs to be set after instantiation to get its Width
                    }
                }
            }
            else
                MessageBox.Show("The last Star Value of 1 is already present.", "Minimum Star Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void DelCharacterColumn(int starValue)
        {
            Control btnDelBot = btnDelBot = Controls["btnDelBot_" + currentMin];
            if (starValue == currentMin)
            {
                currentMin++;
                btnDelBot.Name = "btnDelBot_" + currentMin;
            }

            if (currentMax > currentMin)
            {
                List<string> controlNames = new()
                {
                    "labelHeader_",
                    "rateBox_",
                    "labelRate_",
                    "txtChar_",
                    "txtElem_",
                };
                List<Control> controls = new();
                foreach (string controlName in controlNames)
                {
                    foreach (Control control in Controls.OfType<Control>().Where(c => c.Name.StartsWith(controlName + starValue)))
                        controls.Add(control);
                }

                foreach (Control control in controls)
                    Controls.Remove(control);

                if (starValue == currentMax)
                {
                    Control btnDelTop = Controls["btnDelTop_" + currentMax];
                    currentMax--;
                    btnDelTop.Name = "btnDelTop_" + currentMax;
                    foreach (Label labelHeader in Controls.OfType<Label>().Where(l => l.Name.StartsWith("labelHeader_")))
                    {
                        string[] pair = labelHeader.Name.Split('_');
                        if (int.TryParse(pair[1], out int currentValue))
                        {
                            MoveCharacterControls(currentValue, -1);
                        }
                    }
                }
                btnDelBot.Location = new Point(btnDelBot.Location.X - (columnWidth + columnMargin + elementColumnWidth + columnSplitter), initYPos);

            }
            else
                MessageBox.Show("Why are you trying to eradicate all hopes and wishes?", "Error deleting column", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void AddStarValue_Higher(object sender, EventArgs e) => AddCharactersColumn(currentMax + 1);

        private void AddStarValue_Lower(object sender, EventArgs e) => AddCharactersColumn(currentMin - 1);

        private void MoveCharacterControls(int starValue, int direction)
        {
            Control label = Controls["labelHeader_" + starValue];
            int xPos = label.Location.X + ((columnWidth + columnMargin + elementColumnWidth + columnSplitter) * direction);
            label.Location = new Point(xPos, label.Location.Y);
            Control rateBox = Controls["rateBox_" + starValue];
            rateBox.Location = new Point(xPos + label.Width, label.Location.Y);
            Control labelRate = Controls["labelRate_" + starValue];
            labelRate.Location = new Point(xPos + label.Width + rateBox.Width, label.Location.Y);

            // move Characters
            foreach (TextBox txtSearch in Controls.OfType<TextBox>().Where(t => t.Name.StartsWith("txtChar_" + starValue)))
            {
                string[] pair = txtSearch.Name.Split('_');
                if (int.TryParse(pair[2], out int currentValue))
                {
                    Control txtChar = Controls["txtChar_" + starValue + "_" + currentValue];
                    xPos = txtChar.Location.X + ((columnWidth + columnMargin + elementColumnWidth + columnSplitter) * direction);
                    txtChar.Location = new Point(xPos, txtChar.Location.Y);
                }
            }

            // move Elements
            foreach (TextBox txtSearch in Controls.OfType<TextBox>().Where(t => t.Name.StartsWith("txtElem_" + starValue)))
            {
                string[] pair = txtSearch.Name.Split('_');
                if (int.TryParse(pair[2], out int currentValue))
                {
                    Control txtChar = Controls["txtElem_" + starValue + "_" + currentValue];
                    xPos = txtChar.Location.X + ((columnWidth + columnMargin + elementColumnWidth + columnSplitter) * direction);
                    txtChar.Location = new Point(xPos, txtChar.Location.Y);
                }
            }
        }

        private void SortCharacterData(StarList starList)
        {
            if (starList.Count > 0)
            {
                int xPos = initXPos;
                int starNum = 1;
                ClearCharactersPanel();
                foreach (KeyValuePair<int, CharacterListInStar> charList in starList.Reverse())
                {
                    int starValue = charList.Key;
                    CharacterListInStar charListInStar = charList.Value;
                    charListInStar.SortList();
                    starList.AddStar(starValue);
                    AddCharactersColumn(charList.Key, xPos, charListInStar);
                    xPos += columnWidth + columnMargin + elementColumnWidth + columnSplitter;
                    starNum++;
                }
            }
        }

        private void ClearCharactersPanel()
        {
            List<string> controlNames = new()
            {
                "labelHeader_",
                "rateBox_",
                "labelRate_",
                "txtChar_",
                "txtElem_",
                "btnDelTop",
                "btnDelBot"
            };
            List<Control> controls = new();
            foreach (string controlName in controlNames)
            {
                foreach (Control control in Controls.OfType<Control>().Where(c => c.Name.StartsWith(controlName)))
                    controls.Add(control);
            }

            foreach (Control control in controls)
                Controls.Remove(control);
        }

        private void ValidateRateInput(object sender, KeyEventArgs e)
        {
            if (e.KeyData != Keys.Back)
                e.SuppressKeyPress = !int.TryParse(Convert.ToString((char)e.KeyData), out int rate);
        }

        private void ControlChanged(object sender, EventArgs e) => hasUnsavedChanges = true;

        #region Controls
        private void btnAddStarValue_Click(object sender, EventArgs e)
        {
            int higher = currentMax + 1;
            int lower = currentMin - 1;
            ContextMenu cm = new();
            MenuItem cmExport = new("Export swap mod");
            cm.MenuItems.Add("Add " + higher + "-star column", new EventHandler(AddStarValue_Higher));
            if (lower > 0)
            {
                cm.MenuItems.Add("Add " + lower + "-star column", new EventHandler(AddStarValue_Lower));
            }
            cm.Show(btnAddStarValue, new Point(btnAddStarValue.Width, 0));
        }

        private void btnSortCharacters_Click(object sender, EventArgs e) => SortCharacterData(ExtractDataFromCharactersPanel());

        private void btnDel_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string[] pair = btn.Name.Split('_');

            if (int.TryParse(pair[1], out int starValue))
            {
                DialogResult dr = MessageBox.Show(
                    "Are you sure you want to delete the entire " + starValue + "-star column? Only Venti can undo this!",
                    "Delete Column",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                    );
                if (dr == DialogResult.Yes)
                    DelCharacterColumn(starValue);
            }
        }
        #endregion
    }
}
