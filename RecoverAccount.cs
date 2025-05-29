using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Configuration;
using System.Data.SQLite;
using System.Security.Cryptography;

namespace Escarez_Finals_Mental_Health_Tracker
{
    public partial class RecoverAccount : Form
    {


        private string DbConnectionString = ConfigurationManager.ConnectionStrings["moodmate"].ConnectionString;
        public RecoverAccount()
        {
            InitializeComponent();
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
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





        private async void Recover_Click(object sender, EventArgs e)
        {

            string emailInput = Email.Text.Trim();
            string Username = username.Text.Trim();
            string newPassword = password.Text.Trim();

            string hashpassword = HashPassword(newPassword);

            if (string.IsNullOrWhiteSpace(emailInput) || string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Please fill in both Email and New Password.", "Input all forms", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }


            using (SQLiteConnection conn = new SQLiteConnection(DbConnectionString))
            {
                conn.Open();

                // Check if email exists
                string checkQuery = "SELECT COUNT(*) FROM Login WHERE email = @Email";
                using (SQLiteCommand checkCmd = new SQLiteCommand(checkQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Email", emailInput);
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        // Email found, update password
                        string updateQuery = "UPDATE Login SET password = @Password  WHERE email = @Email";
                        using (SQLiteCommand updateCmd = new SQLiteCommand(updateQuery, conn))
                        {
                         
                            updateCmd.Parameters.AddWithValue("@Password", hashpassword);
                            updateCmd.Parameters.AddWithValue("@Email", emailInput);

                            int rowsAffected = updateCmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Password recover successfully!", "success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Email.Clear();
                                password.Clear();
                                username.Clear();
                            }
                            else
                            {
                                MessageBox.Show("Failed to recover password.", "Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Email not register.", "Not register", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

            }
        }



        private void Signin_Click(object sender, EventArgs e)
        {
            new Form1().Show();
            this.Hide();
        }

        private void Email_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                username.Focus();
            }
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
                Recover.PerformClick();
            }
        }



    }
}
