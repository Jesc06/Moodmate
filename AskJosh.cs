using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Configuration;
using System.Data.SqlClient;
using Guna.UI2.WinForms;
using Guna.Charts.WinForms;

namespace Escarez_Finals_Mental_Health_Tracker
{
    public partial class AskJosh : Form
    {
        private string DbConnectionString = ConfigurationManager.ConnectionStrings["moodmate"].ConnectionString;

        public string empty = null;
        public AskJosh()
        {
            InitializeComponent();
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;

            LoadChatHistory();
            TypeWriterEffect2();
            PanelCheckerIfEmpty();
            ToolTip();
        }


        private void ToolTip()
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(Send, "Send message");
            toolTip.SetToolTip(DeleteChat, "Delete");
        }


        private void PanelCheckerIfEmpty()
        {
            if (ChatLandingPage.Controls.Count > 0)
            {
                JournalTitle.Visible = false;
                subtitle.Visible = false;
                logoai.Visible = false;
            }
            else
            {
                JournalTitle.Visible = true;
                subtitle.Visible = true;
                logoai.Visible = true;
            }
        }


        private async Task TypeWriterEffect2()
        {
            title.Text = "";
            sub.Text = "";

            string content = "AskJosh chatbot";
            string Subcontent = "Your smart AI assistant!";

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



        #region  BubbleChatUI
        private void AddMessage(string message, bool isSender)
        {
            // Container panel for alignment
            Panel containerPanel = new Panel();
            containerPanel.Width = ChatLandingPage.Width - 30;
            containerPanel.Padding = new Padding(5);
            containerPanel.Margin = new Padding(5);
            containerPanel.BackColor = Color.Transparent;

            // Calculate maximum label width (60% of container width) 🟡 Modified from 70% to 60%
            int maxLabelWidth = (int)(containerPanel.Width * 0.4);

            // Message label
            Label msgLabel = new Label();
            msgLabel.AutoSize = true;
            msgLabel.MaximumSize = new Size(maxLabelWidth, 0);
            msgLabel.Text = message;
            msgLabel.Padding = new Padding(10);
            msgLabel.BackColor = isSender ? Color.FromArgb(9, 130, 255) : Color.FromArgb(45, 183, 115);
            msgLabel.Font = new Font("Segoe UI", 13);
            msgLabel.ForeColor = Color.White;

            // Add label to container first to calculate dimensions
            containerPanel.Controls.Add(msgLabel);

            // Profile picture (if receiver)
            PictureBox profilePicture = null;
            if (!isSender)
            {
                profilePicture = new PictureBox();
                profilePicture.Width = 40;
                profilePicture.Height = 40;
                profilePicture.Image = Image.FromFile(@".\Chatbot logo.png");
                profilePicture.SizeMode = PictureBoxSizeMode.StretchImage;
                containerPanel.Controls.Add(profilePicture);
            }

            // Set label location after adding to container
            if (isSender)
            {
                msgLabel.Location = new Point(containerPanel.Width - msgLabel.Width - 10, 0);
            }
            else
            {
                msgLabel.Location = new Point(65, 0);
            }

            // Calculate container height
            int labelHeight = msgLabel.Height;
            int profileHeight = profilePicture?.Height ?? 0;
            containerPanel.Height = Math.Max(labelHeight, profileHeight) + 10;

            // Position profile picture at TOP LEFT
            if (profilePicture != null)
            {
                profilePicture.Location = new Point(10, 0);
            }

            // Add to chat container
            ChatLandingPage.Controls.Add(containerPanel);
            ChatLandingPage.ScrollControlIntoView(containerPanel);
        }
        #endregion EndOfBubbleChatUi

        //for chat history reponse of AskJosh
        private List<Dictionary<string, string>> LoadChatHistoryFromDatabase()
        {
            List<Dictionary<string, string>> history = new List<Dictionary<string, string>>();

            using (SQLiteConnection dbConnection = new SQLiteConnection(DbConnectionString))
            {
                dbConnection.Open();
                string query = "SELECT is_sender, message FROM chat_history ORDER BY timestamp ASC";
                using (SQLiteCommand dbCommand = new SQLiteCommand(query, dbConnection))
                using (SQLiteDataReader reader = dbCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bool isSender = reader.GetBoolean("is_sender");
                        string message = reader.GetString("message");

                        history.Add(new Dictionary<string, string>
                        {
                            { "role", isSender ? "user" : "assistant" },
                            { "content", message }
                        });
                    }
                }
            }

            return history;
        }




        private async void Send_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(SendMessage.Text))
                {
                    string userMessage = SendMessage.Text;

                    AddMessage(userMessage, true);
                    SendMessage.Clear();

                    // Add AI response to history
                    var chatHistory = LoadChatHistoryFromDatabase();

                    // Get AI response
                    ChatBot_AskJosh deep = new ChatBot_AskJosh();
                    string reply = await deep.GetDeepSeekResponse(chatHistory);

                    AddMessage(reply, false);
                    SaveMessageToDatabase(userMessage, true);
                    SaveMessageToDatabase(reply, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.StartsWith("Error") ? ex.Message : "No internet connection");
            }
            PanelCheckerIfEmpty();
        }




        private void SaveMessageToDatabase(string message, bool isSender)
        {
            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(DbConnectionString))
                {
                    dbConnection.Open();
                    string query = "INSERT INTO chat_history (message, is_sender) VALUES (@message, @is_sender)";
                    using (SQLiteCommand dbCommand = new SQLiteCommand(query, dbConnection))
                    {
                        dbCommand.Parameters.AddWithValue("@message", message);
                        dbCommand.Parameters.AddWithValue("@is_sender", isSender);
                        dbCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception error) { MessageBox.Show(error.Message); }
        }




        private void LoadChatHistory()
        {
            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(DbConnectionString))
                {
                    dbConnection.Open();
                    string query = "SELECT message, is_sender FROM chat_history ORDER BY timestamp ASC";
                    using (SQLiteCommand dbCommand = new SQLiteCommand(query, dbConnection))
                    using (SQLiteDataReader reader = dbCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string message = reader.GetString("message");
                            bool isSender = reader.GetBoolean("is_sender");
                            AddMessage(message, isSender);
                        }
                    }
                }
            }
            catch (Exception error) { MessageBox.Show(error.Message); }
        }




        private void DeleteChat_Click(object sender, EventArgs e)
        {
            try
            {
                var validation = MessageBox.Show(@"Are you sure you want to delete all messages?",
                                                 "Confirm Delete",
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Question);

                if (validation == DialogResult.Yes)
                {
                    using (SQLiteConnection dbconnection = new SQLiteConnection(DbConnectionString))
                    {
                        dbconnection.Open();
                        string query = "delete from chat_history";
                        using (SQLiteCommand dbcommand = new SQLiteCommand(query, dbconnection))
                        {
                            dbcommand.ExecuteNonQuery();
                            ChatLandingPage.Controls.Clear();
                        }
                    }
                }

            }
            catch (Exception error) { MessageBox.Show(error.Message); }
            PanelCheckerIfEmpty();
        }



        #region others
        private void Journal_Click(object sender, EventArgs e)
        {
            new Journal().Show();
            this.Hide();
        }
        private void Mood_Click(object sender, EventArgs e)
        {
            new Mood(empty).Show();
            this.Hide();
        }
        private void Dashboard_Click(object sender, EventArgs e)
        {
            new Home().Show();
            this.Hide();
        }

        private void Exercise_Click(object sender, EventArgs e)
        {
            new Exercise().Show();
            this.Hide();
        }

        private void SendMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Send.PerformClick();
                SendMessage.Focus();
                PanelCheckerIfEmpty();
            }
        }

        #endregion






    }
}
