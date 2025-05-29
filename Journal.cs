using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Configuration;


namespace Escarez_Finals_Mental_Health_Tracker
{
    public partial class Journal : Form
    {

        private string DbConnectionString = ConfigurationManager.ConnectionStrings["moodmate"].ConnectionString;
        private int? editingEntryId = null; 
        public string empty = null;

        public Journal()
        {
            InitializeComponent();
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            PanelCheckerIfEmpty();
            LoadJournalEntries();
            TypeWriterEffect();
            TypeWriterEffect2();

            ToolTip();

        }



        private void ToolTip()
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(BrowseFiles, "Insert Journal");
            toolTip.SetToolTip(Save_Journal, "Add Journal");
        }


        private async Task TypeWriterEffect2()
        {
            title.Text = "";
            sub.Text = "";

            string content = "Moodmate Journal";
            string Subcontent = "Share your daily moments";

            foreach (char type in content)
            {
                title.Text += type;
                await Task.Delay(100);
            }
            await Task.Delay(300);
            foreach (char type in Subcontent)
            {
                sub.Text += type;
                await Task.Delay(80);
            }
        }



        private async Task TypeWriterEffect()
        {
            string content = "Type your journal entry here.";

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




        #region  ALl Design logic

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
            List<JournalEntry> entries = GetEntriesFromDatabase();

            foreach (var entry in entries)
            {
                AddJournalCard(entry);
            }

            PanelCheckerIfEmpty();
        }



        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse
          );


        private void AddJournalCard(JournalEntry entry)
        {
            Panel card = new Panel();
            card.Width = 400;
            card.BackColor = GetRandomNoteColor();
            card.Margin = new Padding(9);
            card.Padding = new Padding(9);
            card.AutoSize = false;
            card.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // Date Label
            Label lblDate = new Label();
            lblDate.Text = entry.Date;
            lblDate.Font = new Font("Segoe UI", 10, FontStyle.Italic);
            lblDate.ForeColor = Color.Black;
            lblDate.MaximumSize = new Size(card.Width - card.Padding.Horizontal, 0);
            lblDate.AutoSize = true;
            lblDate.Padding = new Padding(15, 15, 0, 0);

            // Content Label
            Label lblContent = new Label();
            lblContent.Text = entry.Content;
            lblContent.Font = new Font("Segoe UI", 13);
            lblContent.ForeColor = Color.Black;
            lblContent.MaximumSize = new Size(card.Width - card.Padding.Horizontal, 0);
            lblContent.AutoSize = true;
            lblContent.Padding = new Padding(15, 15, 0, 0);

            // DELETE BUTTON - MODERN STYLE
            Button btnDelete = new Button();
            btnDelete.Text = "Delete"; // Optional emoji icon
            btnDelete.BackColor = Color.FromArgb(255, 82, 82); // Bootstrap red-ish
            btnDelete.ForeColor = Color.White;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.Font = new Font("Segoe UI", 7, FontStyle.Bold);
            btnDelete.Size = new Size(60, 25);
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Location = new Point(card.Width - btnDelete.Width - 15, 10);
            btnDelete.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnDelete.Width, btnDelete.Height, 5, 5));


            btnDelete.Click += (s, e) =>
            {
                var confirm = MessageBox.Show("Are you sure you want to delete this entry?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    using (var conn = new SQLiteConnection(DbConnectionString))
                    {
                        conn.Open();

                        string deleteQuery = "DELETE FROM Journal WHERE Id = @Id";
                        using (var cmd = new SQLiteCommand(deleteQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@Id", entry.Id);
                            cmd.ExecuteNonQuery();
                        }

                        conn.Close();
                    }

                    JournalDataPanel.Controls.Remove(card);
                    PanelCheckerIfEmpty();
                }
            };


            Button btnEdit = new Button();
            btnEdit.Text = "Edit"; // Optional emoji icon
            btnEdit.BackColor = Color.FromArgb(113, 199, 236); // Bootstrap blue
            btnEdit.ForeColor = Color.White;
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderSize = 0;
            // Palit ng mas maliit na font size at same font style
            btnEdit.Font = new Font("Segoe UI", 7, FontStyle.Bold);
            // Paliit ng size, mas compact siya
            btnEdit.Size = new Size(60, 25);
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.Location = new Point(btnDelete.Left - btnEdit.Width - 10, 10);
            btnEdit.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnEdit.Width, btnEdit.Height, 5, 5));

            btnEdit.Click += (s, e) =>
            {
                txtJournal.Text = entry.Content;
                editingEntryId = entry.Id;
                txtJournal.Focus();
            };


            // Add controls to the card
            card.Controls.Add(lblDate);
            card.Controls.Add(lblContent);
            card.Controls.Add(btnDelete);
            card.Controls.Add(btnEdit);

            // Set position of content label under date
            lblContent.Location = new Point(0, lblDate.Bottom + 10);

            // Adjust card height to fit all controls
            int totalHeight = lblDate.Height + lblContent.Height + 40;
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
        private List<JournalEntry> GetEntriesFromDatabase()
        {
            List<JournalEntry> entries = new List<JournalEntry>();

            using (var conn = new SQLiteConnection(DbConnectionString))
            {
                conn.Open();

                string selectQuery = "SELECT Id, Date, content FROM Journal ORDER BY Id DESC";
                using (var cmd = new SQLiteCommand(selectQuery, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["Id"]);
                        string date = reader["Date"].ToString();
                        string content = reader["content"].ToString();
                        entries.Add(new JournalEntry(id, date, content));
                    }
                }

                conn.Close();
            }

            return entries;
        }




        private void Save_Journal_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtJournal.Text))
            {
                using (var conn = new SQLiteConnection(DbConnectionString))
                {
                    conn.Open();

                    if (editingEntryId.HasValue)
                    {
                        // UPDATE logic
                        string updateQuery = "UPDATE Journal SET content = @Content WHERE Id = @Id";
                        using (var cmd = new SQLiteCommand(updateQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@Content", txtJournal.Text.Trim());
                            cmd.Parameters.AddWithValue("@Id", editingEntryId.Value);
                            cmd.ExecuteNonQuery();
                        }

                        editingEntryId = null;
                    }
                    else
                    {
                        // INSERT logic
                        string insertQuery = "INSERT INTO Journal (Date, content) VALUES (@Date, @Content)";
                        using (var cmd = new SQLiteCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@Date", DateTime.Now.ToString("MMMM dd, yyyy"));
                            cmd.Parameters.AddWithValue("@Content", txtJournal.Text.Trim());
                            cmd.ExecuteNonQuery();
                        }
                    }

                    conn.Close();
                }

                JournalDataPanel.Controls.Clear();
                LoadJournalEntries();
                txtJournal.Clear();
            }
        }



        private void txtJournal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Save_Journal.PerformClick();
                txtJournal.Focus();
            }
        }



        private void BrowseFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;
                string fileContent = File.ReadAllText(filePath);
                txtJournal.Text = fileContent;
            }
        }



        private void Dashboard_Click(object sender, EventArgs e)
        {
            new Home().Show();
            this.Hide();
        }


        private void Mood_Click(object sender, EventArgs e)
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
