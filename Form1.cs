using System.Configuration;
using System.Data.SQLite;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Security.Cryptography;



namespace Escarez_Finals_Mental_Health_Tracker
{
    public partial class Form1 : Form
    {


        private string DbConnectionString = ConfigurationManager.ConnectionStrings["moodmate"].ConnectionString;


        public Form1()
        {
            InitializeComponent();
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ToolTip();
        }



        private void ToolTip()
        {
            ToolTip toolTip = new ToolTip();
            toolTip.SetToolTip(Fogotpass, "Recover your account");
        }



        private void CheckIfHaveAccIfTrueEnableSignUp()
        {
            try
            {
                using (SQLiteConnection con = new SQLiteConnection(DbConnectionString))
                {
                    con.Open();
                    string query = "select * from Login";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                    {
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count > 0)
                        {
                            MessageBox.Show("You have already account","Failed",MessageBoxButtons.OK,MessageBoxIcon.Error);
                        }
                        else
                        {
                            Sign_up_to_MoodMate signUp = new Sign_up_to_MoodMate();
                            signUp.Show();
                            this.Hide();
                        }
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }

        }




        private async Task EmailNotificationAsync(string toEmail, string subject, string body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("");
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential();
                    smtp.EnableSsl = true;

                    await smtp.SendMailAsync(mail); // Asynchronous call
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send email: " + ex.Message);
            }
        }





        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {   
                    builder.Append(b.ToString("x2"));           
                }
                return builder.ToString();
            }
        }




        private async void Sign_In_Click(object sender, EventArgs e)
        {
            try
            {
                using (SQLiteConnection con = new SQLiteConnection(DbConnectionString))
                {
                    con.Open();

                    // Step 1: Get hashed password from DB using username only
                    string query = "SELECT password FROM Login WHERE username = @username";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username.Text);

                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // if account exists
                            {
                                string storedHashedPassword = reader.GetString(0); // column index 0 = password from DB

                                // Step 2: Hash the entered password
                                string enteredPasswordHash = HashPassword(password.Text);

                                // Step 3: Compare hashes
                                if (storedHashedPassword == enteredPasswordHash)
                                {
                                    await EmailNotificationAsync(
                                        "",
                                        "",
                                        "");

                                    new Home().Show();
                                    this.Hide();
                                }
                                else
                                {
                                    MessageBox.Show("Incorrect password", "Incorrect", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Account does not exist", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }





        private void Sign_Up_Click(object sender, EventArgs e)
        {
            CheckIfHaveAccIfTrueEnableSignUp();
        }



        private void Fogotpass_Click(object sender, EventArgs e)
        {
            new RecoverAccount().Show();
            this.Hide();
        }



        private void username_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                password.Focus();
            }
        }



        private void password_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                Sign_In.PerformClick();
            }
        }




    }
}
