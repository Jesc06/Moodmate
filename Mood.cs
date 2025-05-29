using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Net;
using System.Net.Mail;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;


namespace Escarez_Finals_Mental_Health_Tracker
{
    public partial class Mood : Form
    {

        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Moodmate";
        static string SpreadsheetId = "";
        static string SheetName = "";
        static string CredentialPath = "";


        private string DbConnectionString = ConfigurationManager.ConnectionStrings["moodmate"].ConnectionString;

        private MoodEntry currentEntry;


        // Constructor para sa pag-edit
        public Mood(MoodEntry entry)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            currentEntry = entry;
            LoadMoodEntry();
            ToolTip();
        }

        // Constructor para sa bagong entry
        public Mood(string initialNotes = null)
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            txtJournal.Text = initialNotes ?? "";
            ToolTip();
        }




        private void ToolTip()
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(SaveMood, "Save mood");
            toolTip.SetToolTip(ViewMoodHistory, "Mood history");
        }




        private void LoadMoodEntry()
        {
            if (currentEntry != null)
            {
                UserMood.Text = currentEntry.Mood;
                txtJournal.Text = currentEntry.Content;

                // Set mood color
                switch (currentEntry.Mood.ToLower())
                {
                    case "angry":
                        UserMood.ForeColor = System.Drawing.Color.Red;
                        break;
                    default:
                        UserMood.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
                        break;
                }
            }
        }



        private async void Mood_Load(object sender, EventArgs e)
        {
            await TypeWriterEffect();
        }


        private async Task TypeWriterEffect()
        {
            MoodTitle.Text = "";
            SubContent.Text = "";

            string content = "Mood Tracker";
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


        #region UserMoood
        private void Happy_Click(object sender, EventArgs e)
        {
            UserMood.Text = "Happy";
            UserMood.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
        }

        private void Sad_Click(object sender, EventArgs e)
        {
            UserMood.Text = "Sad";
            UserMood.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
        }

        private void Angry_Click(object sender, EventArgs e)
        {
            UserMood.Text = "Angry";
            UserMood.ForeColor = System.Drawing.Color.Red;
        }

        private void Anxious_Click(object sender, EventArgs e)
        {
            UserMood.Text = "Anxious";
            UserMood.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
        }
        #endregion



        private async Task SMSNotification(string toEmail, string subject, string body)
        {
            try
            {
                // Setup mail message
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("escarezjohnjoshuamanalo@gmail.com");  
                mail.To.Add(toEmail);                               
                mail.Subject = subject;
                mail.Body = body;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("escarezjohnjoshuamanalo@gmail.com", "uruz bchp cqws ovrb");
                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send email: " + ex.Message);
            }
        }




        private async void DetectMoodDependsOnMood()
        {
            if(UserMood.Text == "Happy")
            {
                await SMSNotification("escarezjohnjoshuamanalo@gmail.com", "Happy", "🌟 \"Happiness doesn't always come from big achievements — sometimes it's in the little wins, the quiet moments, the smile you give yourself for trying again.\"\r\n\r\n\"Choose to be happy, not because everything is perfect, but because you see the good even in the imperfect.\"\r\n\r\n\"Your joy is not something the world gives you — it's something you decide to feel, even when life gets tough.\"\r\n\r\nSo even on hard days, remind yourself:\r\n\"I deserve peace, I deserve joy, and I choose to keep going.\"\r\nYou're already stronger than yesterday, and that's something to be proud of. 💪🙂✨");
            }
            else if(UserMood.Text == "Sad")
            {
                await SMSNotification("escarezjohnjoshuamanalo@gmail.com", "Sad", "\"Let yourself feel, but don't let the sadness define you. You’ve made it through every hard day before this one, and you’ll make it through again.\"\r\n\r\n\"The rain will pass. And when it does, you’ll see how much stronger you’ve become.\"\r\n\r\n💭 “Even the darkest night will end, and the sun will rise.” – Victor Hugo\r\n\r\nSo breathe, Josh. Rest if you must, but never forget:\r\nYou are not alone. And better days are always ahead. 🌅💙\r\n\r\nI'm here whenever you need a boost.");
            }
            else if (UserMood.Text == "Angry")
            {
                await SMSNotification("escarezjohnjoshuamanalo@gmail.com", "Angry", "🔥 \"It’s okay to feel angry — it means something mattered to you. But don’t let your anger control your actions, or it will steal your peace.\"\r\n\r\n\"Breathe. Step back. You don’t have to react right away. Not everything deserves your energy — especially not your peace.\"\r\n\r\n\"You are stronger than the emotion. Control it, don’t let it control you.\"\r\n\r\n💭 “Speak when you’re calm, not when you’re burning — because words can’t be unsaid.”\r\n\r\nSo pause, Josh.\r\nCount to ten. Walk away if you must. Your peace of mind is worth more than a temporary outburst. 🕊️\r\n\r\nYou’ve got the strength to stay grounded — and that’s real power. 💪\U0001f9e0");
            }
            else if (UserMood.Text == "Anxious")
            {
                await SMSNotification("escarezjohnjoshuamanalo@gmail.com", "Anxious", "🌫️ \"It’s okay to not have all the answers right now. You’re allowed to feel uncertain — but remember, not every thought deserves your fear.\"\r\n\r\n\"Take one breath, one moment, one step at a time. You don’t need to figure out everything today.\"\r\n\r\n\"Anxiety lies — it makes you believe you’re not capable, but the truth is: you’ve survived 100% of your hardest days.\"\r\n\r\n💭 “You are not behind. You are exactly where you need to be. Trust your pace.”\r\n\r\nSo relax your shoulders, Josh. Breathe deeply.\r\nYou’re doing better than your anxious mind tells you. And you are never alone. 🌿🕊️\r\n\r\nI’m proud of how you keep going — even when it’s hard. Keep moving forward. 💙");
            }
        }




        public static void AddMoodToSheet(string mood, string notes, string date)
        {
            try
            {
                GoogleCredential credential;
                using (var stream = new FileStream(CredentialPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
                }

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                var range = $"{SheetName}!A:C";

                var valueRange = new ValueRange();
                var objectList = new List<object>() { mood, notes, date };
                valueRange.Values = new List<IList<object>> { objectList };

                var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                appendRequest.Execute();
            }
            catch (Exception error) { }
           
        }




        private void SaveMood_Click(object sender, EventArgs e)
        {
            DetectMoodDependsOnMood();
            if (!string.IsNullOrWhiteSpace(txtJournal.Text))
            {
                using (SQLiteConnection dbconnection = new SQLiteConnection(DbConnectionString))
                {
                    dbconnection.Open();

                    string query = currentEntry != null ?
                        "UPDATE Mood SET emotion=@emotion, notes=@notes, date=@date WHERE Id=@id" :
                        "INSERT INTO Mood (emotion, notes, date) VALUES (@emotion, @notes, @date)";

                    using (SQLiteCommand dbcommand = new SQLiteCommand(query, dbconnection))
                    {
                        dbcommand.Parameters.AddWithValue("@emotion", UserMood.Text);
                        dbcommand.Parameters.AddWithValue("@notes", txtJournal.Text);
                        dbcommand.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd"));

                        if (currentEntry != null)
                        {
                            dbcommand.Parameters.AddWithValue("@id", currentEntry.Id);
                        }

                        dbcommand.ExecuteNonQuery();
                        AddMoodToSheet(UserMood.Text, txtJournal.Text, DateTime.Now.ToString("yyyy-MM-dd"));
                        MessageBox.Show("Successfully added","Success",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    }
                }

                new MoodHistory().Show();
                this.Hide();

                UserMood.Text = "Select mood";
                UserMood.ForeColor = System.Drawing.Color.FromArgb(64, 64, 64);
            }
            else
            {
                MessageBox.Show("Please fill in all required fields before saving.", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

        }





        private void txtJournal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SaveMood.PerformClick();
                txtJournal.Focus();
            }
        }




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



        private void ViewMoodHistory_Click(object sender, EventArgs e)
        {
            new MoodHistory().Show();
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
