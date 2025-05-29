using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace Escarez_Finals_Mental_Health_Tracker
{
    public partial class MoodHistory : Form
    {

        private string DbConnectionString = ConfigurationManager.ConnectionStrings["moodmate"].ConnectionString;

        public string empty = null;

        private int? editingEntryId = null;


        public MoodHistory()
        {
            InitializeComponent();
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            TypeWriterEffect();
            TypeWriterEffectTitle();
            PanelCheckerIfEmpty();

            LoadJournalEntries();
        }

    

        private async Task TypeWriterEffect()
        {
            MoodTitle.Text = "";
            SubContent.Text = "";

            string content = "Mood History";
            string Subcontent = "A Daily Check-In With Yourself";

            foreach (char type in content)
            {
                MoodTitle.Text += type;
                await Task.Delay(100);
            }
            await Task.Delay(300);
            foreach (char type in Subcontent)
            {
                SubContent.Text += type;
                await Task.Delay(80);
            }


        }



        private async Task TypeWriterEffectTitle()
        {

            string content = "You haven’t done anything yet.";

            while (true) // Infinite loop
            {
                // Type text
                JournalTitle.Text = "";
                foreach (char c in content)
                {
                    JournalTitle.Text += c;
                    await Task.Delay(90);
                }

                // Wait bago mag-delete
                await Task.Delay(2000);

                // Delete text one character at a time
                while (JournalTitle.Text.Length > 0)
                {
                    JournalTitle.Text = JournalTitle.Text.Substring(0, JournalTitle.Text.Length - 1);
                    await Task.Delay(60);
                }

                // Wait bago magsimulang muli
                await Task.Delay(500);
            }
        }


        #region PanelCard
        private void PanelCheckerIfEmpty()
        {
            if (JournalDataPanel.Controls.Count > 0)
            {
                JournalTitle.Visible = false;
            }
            else
            {
                JournalTitle.Visible = true;
            }
        }

        private void LoadJournalEntries()
        {
            // Example: Load entries from the database or manual data
            List<MoodEntry> entries = GetEntriesFromDatabase();  // Replace with actual DB call

            foreach (var entry in entries)
            {
                // Add journal entry cards to the FlowLayoutPanel
                AddJournalCard(entry);
            }
        }




        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse
          );




        private void AddJournalCard(MoodEntry entry)
        {
            Panel card = new Panel();
            card.Width = 400;
            card.BackColor = GetRandomNoteColor();
            card.Margin = new Padding(9);
            card.Padding = new Padding(9);
            card.AutoSize = false;
            card.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // Mood Label (Title)
            Label lblMood = new Label();
            lblMood.Text = entry.Mood;
            lblMood.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblMood.ForeColor = Color.FromArgb(64, 64, 64);
            lblMood.MaximumSize = new Size(card.Width - card.Padding.Horizontal - 20, 0);
            lblMood.AutoSize = true;

            // Date Label
            Label lblDate = new Label();
            lblDate.Text = entry.Date;
            lblDate.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lblDate.ForeColor = Color.FromArgb(64, 64, 64);
            lblDate.MaximumSize = new Size(card.Width - card.Padding.Horizontal - 20, 0);
            lblDate.AutoSize = true;

            // Content Label
            Label lblContent = new Label();
            lblContent.Text = entry.Content;
            lblContent.Font = new Font("Segoe UI", 13);
            lblContent.ForeColor = Color.Black;
            lblContent.MaximumSize = new Size(card.Width - card.Padding.Horizontal - 20, 0);
            lblContent.AutoSize = true;
            lblContent.AutoEllipsis = false;

            // EDIT BUTTON
            Button btnEdit = new Button();
            btnEdit.Text = "Edit";
            btnEdit.BackColor = Color.FromArgb(45, 183, 115);
            btnEdit.ForeColor = Color.White;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Font = new Font("Segoe UI", 7, FontStyle.Bold);
            btnEdit.Size = new Size(60, 25);
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.Location = new Point(card.Width - btnEdit.Width - 15, 15); // shifted to original delete button spot
            btnEdit.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnEdit.Width, btnEdit.Height, 5, 5));


            btnEdit.Click += (s, e) =>
            {
                // Ipadala ang BUONG MoodEntry object sa Mood form
                Mood moodForm = new Mood(entry);
                moodForm.Show();
                this.Hide();
            };

            // Add controls to the card
            card.Controls.Add(lblMood);
            card.Controls.Add(lblDate);
            card.Controls.Add(lblContent);
            card.Controls.Add(btnEdit);

            // Set manual positions
            int topMargin = 20;
            lblMood.Location = new Point(13, topMargin);
            lblDate.Location = new Point(13, lblMood.Bottom + 20);
            lblContent.Location = new Point(13, lblDate.Bottom + 20);

            // Adjust card height
            int totalHeight = lblContent.Bottom + 30;
            card.Height = totalHeight;

            // Rounded corners
            card.Paint += (s, e) =>
            {
                int radius = 25;
                var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(card.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(card.Width - radius, card.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, card.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                card.Region = new Region(path);
            };

            JournalDataPanel.Controls.Add(card);
            PanelCheckerIfEmpty();
        }




        private Color GetRandomNoteColor()
        {
            Color[] colors = { Color.FromArgb(240, 253, 244), Color.FromArgb(254, 247, 236), Color.FromArgb(242, 232, 255), Color.FromArgb(254, 242, 242) };
            return colors[new Random().Next(colors.Length)];
        }

        


        // Dummy method to simulate getting entries from a database
        private List<MoodEntry> GetEntriesFromDatabase()
        {
            List<MoodEntry> entries = new List<MoodEntry>();

            using (SQLiteConnection conn = new SQLiteConnection(DbConnectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Mood ORDER BY Id DESC";

                using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // DAGDAGAN NG ID
                        int id = Convert.ToInt32(reader["Id"]); // <- Importanteng fix!
                        string mood = reader["emotion"].ToString();

                        // Date handling
                        string date;
                        if (reader["date"] != DBNull.Value && DateTime.TryParse(reader["date"].ToString(), out DateTime parsedDate))
                        {
                            date = parsedDate.ToString("MMMM dd, yyyy");
                        }
                        else
                        {
                            date = DateTime.Now.ToString("MMMM dd, yyyy");
                        }

                        string content = reader["notes"].ToString(); // Palitan ng content ang variable name

                        entries.Add(new MoodEntry(id, mood, date, content)); // Dapat 4 parameters
                    }
                }
            }

            return entries;
        }


        #endregion


        #region others
        private void Dashboard_Click(object sender, EventArgs e)
        {
            new Home().Show();
            this.Hide();
        }

        private void Journal_Click(object sender, EventArgs e)
        {
            new Journal().Show();
            this.Hide();
        }

        private void BackToMood_Click(object sender, EventArgs e)
        {
            new Mood(empty).Show();
            this.Hide();
        }

        private void AskJosh_Click(object sender, EventArgs e)
        {
            new AskJosh().Show();
            this.Hide();
        }

        private void Exercise_Click(object sender, EventArgs e)
        {
            new Exercise().Show();
            this.Hide();
        }
        #endregion

    }
}
