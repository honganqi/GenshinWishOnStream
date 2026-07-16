using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
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

        int lastItemY = 0;
        int currentMax = -1;
        int currentMin = -1;

        public PanelCharactersControl()
        {
            InitializeComponent();
        }

        public void InitializeCharactersPanel(StarList starList)
        {
            ClearCharactersPanel();
            int xPos = initXPos;
            lastItemY = 0;

            int starNum = 1;

            foreach (KeyValuePair<int, CharacterListInStar> charListPair in starList.Reverse())
            {
                int yPos = initYPos;

                int starValue = charListPair.Key;
                CharacterListInStar charList = charListPair.Value;

                if (starNum == 1)
                    currentMax = starValue;
                else if (starNum == starList.Count)
                    currentMin = starValue;

                Label headerLabel = new()
                {
                    Name = "labelHeader_" + starValue,
                    Text = starValue.ToString() + "-Star",
                    Font = new("Segoe UI", 15, FontStyle.Bold),
                    Location = new Point(xPos, initYPos),
                    AutoSize = true
                };
                Controls.Add(headerLabel);

                TextBox rateBox = new()
                {
                    Name = "rateBox_" + starValue,
                    Text = charListPair.Value.PullRate.ToString(),
                    Location = new Point(xPos + headerLabel.Width, initYPos),
                    Width = 40
                };
                rateBox.KeyDown += new KeyEventHandler(ValidateRateInput);
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

                if ((starNum == 1) || (starNum == starList.Count))
                {
                    Button btnDel = new();
                    if (starNum == 1)
                        btnDel.Name = "btnDelTop_" + starValue;
                    else
                        btnDel.Name = "btnDelBot_" + starValue;
                    btnDel.Text = "\ue10A";
                    btnDel.Font = new("Segoe UI", 8, FontStyle.Bold);
                    btnDel.Height = rateBox.Height;
                    btnDel.Width = rateBox.Height;
                    btnDel.Location = new Point(xPos + columnWidth + elementColumnWidth + columnSplitter - btnDel.Width, initYPos);
                    btnDel.TextAlign = ContentAlignment.MiddleCenter;
                    btnDel.Click += new EventHandler(btnDel_Click);
                    Controls.Add(btnDel);
                }



                int charNum = 0;
                foreach (Character character in charList)
                {
                    string nameToSearch = character + ".webp";
                    yPos += rowHeight;
                    TextBox txtbox = new()
                    {
                        Name = "txtChar_" + starValue + "_" + charNum,
                        Text = character.CharacterName,
                        Location = new Point(xPos, yPos),
                        Width = columnWidth
                    };
                    Controls.Add(txtbox);

                    TextBox elemtxtbox = new()
                    {
                        Name = "txtElem_" + starValue + "_" + charNum,
                        Text = character.Element,
                        Location = new Point(xPos + columnWidth + columnSplitter, yPos),
                        Width = elementColumnWidth
                    };
                    Controls.Add(elemtxtbox);

                    charNum++;
                }

                for (int charNumAgain = charNum; charNumAgain < (charNum + extraRows); charNumAgain++)
                {
                    yPos += rowHeight;
                    TextBox txtbox = new()
                    {
                        Name = "txtChar_" + starValue + "_" + charNumAgain,
                        Location = new Point(xPos, yPos),
                        Width = columnWidth
                    };
                    Controls.Add(txtbox);

                    TextBox elemtxtbox = new()
                    {
                        Name = "txtElem_" + starValue + "_" + charNumAgain,
                        Location = new Point(xPos + columnWidth + columnSplitter, yPos),
                        Width = elementColumnWidth
                    };
                    Controls.Add(elemtxtbox);

                    if ((yPos + rowHeight) > lastItemY)
                        lastItemY = (yPos + rowHeight);
                }

                xPos += columnWidth + columnMargin + elementColumnWidth + columnSplitter;

                starNum++;
            }
        }

        public StarList ExtractDataFromCharactersPanel()
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
                MessageBox.Show(string.Join("\n", errors.ToArray()));

            return starList;
        }

        private void AddCharactersColumn(int starValue, int position, CharacterListInStar characters = null)
        {
            if (starValue > 0)
            {
                if (starValue > currentMax)
                {
                    foreach (Label labelSearch in Controls.OfType<Label>().Where(l => l.Name.StartsWith("labelHeader_")))
                    {
                        string[] pair = labelSearch.Name.Split('_');
                        if (int.TryParse(pair[1], out int currentValue))
                        {
                            Control label = Controls["labelHeader_" + currentValue];
                            MoveCharacterControls(currentValue, 1);
                        }
                    }
                    Control moveDel = Controls["btnDelTop_" + currentMax];
                    currentMax = starValue;
                    moveDel.Name = "btnDelTop_" + currentMax;
                }
                else if (starValue < currentMin)
                {
                    Control moveDel = Controls["btnDelBot_" + currentMin];
                    currentMin = starValue;
                    moveDel.Name = "btnDelBot_" + currentMin;
                }




                int xPos = initXPos;
                if (position == -1)
                    foreach (Label labelSearch in Controls.OfType<Label>().Where(l => l.Name.StartsWith("labelHeader_")))
                        xPos += columnWidth + columnMargin + elementColumnWidth + columnSplitter;
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
                    Text = "0"
                };
                if (characters != null)
                    rateBox.Text = characters.PullRate.ToString();
                rateBox.Location = new Point(xPos + headerLabel.Width, initYPos);
                rateBox.Width = 40;
                rateBox.KeyDown += new KeyEventHandler(ValidateRateInput);
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
                        string characterName = character.CharacterName;
                        yPos += rowHeight;
                        TextBox txtbox = new()
                        {
                            Name = "txtChar_" + starValue + "_" + charNum,
                            Text = characterName,
                            Location = new Point(xPos, yPos),
                            Width = columnWidth
                        };
                        Controls.Add(txtbox);

                        TextBox elemtxtbox = new()
                        {
                            Name = "txtElem_" + starValue + "_" + charNum,
                            Text = character.Element,
                            Location = new Point(xPos + columnWidth + columnSplitter, yPos),
                            Width = elementColumnWidth
                        };
                        Controls.Add(elemtxtbox);

                        if ((yPos + rowHeight) > lastItemY)
                            lastItemY = (yPos + rowHeight);
                        charNum++;
                    }
                }

                for (int charNumAgain = charNum; charNumAgain < (charNum + extraRows); charNumAgain++)
                {
                    yPos += rowHeight;
                    TextBox txtbox = new()
                    {
                        Name = "txtChar_" + starValue + "_" + charNumAgain,
                        Location = new Point(xPos, yPos),
                        Width = columnWidth
                    };
                    Controls.Add(txtbox);

                    TextBox elemtxtbox = new()
                    {
                        Name = "txtElem_" + starValue + "_" + charNumAgain,
                        Location = new Point(xPos + columnWidth + columnSplitter, yPos),
                        Width = elementColumnWidth
                    };
                    Controls.Add(elemtxtbox);

                    if ((yPos + rowHeight) > lastItemY)
                        lastItemY = (yPos + rowHeight);
                }

                Control btnDelBot = Controls["btnDelBot_" + currentMin];
                btnDelBot.Location = new Point(btnDelBot.Location.X + (columnWidth + columnMargin + elementColumnWidth + columnSplitter), initYPos);
            }
            else
                MessageBox.Show("The last Star Value of 1 is already present.", "Minimum Star Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void DelCharacterColumn(int starValue)
        {


            Control btnDelBot = Controls["btnDelBot_" + currentMin];
            btnDelBot.Location = new Point(btnDelBot.Location.X - (columnWidth + columnMargin + elementColumnWidth + columnSplitter), initYPos);
            if (starValue == currentMin)
                currentMin++;
            btnDelBot.Name = "btnDelBot_" + currentMin;
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
                    btnDelTop.Name = "btnDelTop_" + currentMax;
                    foreach (Label labelHeader in Controls.OfType<Label>().Where(l => l.Name.StartsWith("labelHeader_")))
                    {
                        string[] pair = labelHeader.Name.Split('_');
                        if (int.TryParse(pair[1], out int currentValue))
                        {
                            Control label = Controls["labelHeader_" + currentValue];
                            MoveCharacterControls(currentValue, -1);
                        }
                    }
                    currentMax--;
                }

            }
            else
                MessageBox.Show("Why are you trying to eradicate all hopes and wishes?", "Error deleting column", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnAddStarValue_Click(object sender, EventArgs e)
        {
            int higher = currentMax + 1;
            int lower = currentMin - 1;
            ContextMenu cm = new();
            MenuItem cmExport = new("Export swap mod");
            cm.MenuItems.Add("Add " + higher + "-star column", new EventHandler(AddStarValue_Higher));
            cm.MenuItems.Add("Add " + lower + "-star column", new EventHandler(AddStarValue_Lower));
            cm.Show(btnAddStarValue, new Point(btnAddStarValue.Width, 0));
        }

        private void AddStarValue_Higher(object sender, EventArgs e) => AddCharactersColumn(currentMax + 1, 1);
        private void AddStarValue_Lower(object sender, EventArgs e) => AddCharactersColumn(currentMin - 1, -1);

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
                ClearCharactersPanel();
                foreach (KeyValuePair<int, CharacterListInStar> charList in starList.Reverse())
                {
                    int starValue = charList.Key;
                    CharacterListInStar charListInStar = charList.Value;
                    charListInStar.SortList();
                    starList.AddStar(starValue);
                    AddCharactersColumn(charList.Key, -1, charListInStar);
                }
            }
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

        private void ClearCharactersPanel()
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
    }
}
