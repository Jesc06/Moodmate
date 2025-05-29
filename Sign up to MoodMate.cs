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
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace Escarez_Finals_Mental_Health_Tracker
{
    public partial class Sign_up_to_MoodMate : Form
    {

        private string DbConnectionString = ConfigurationManager.ConnectionStrings["moodmate"].ConnectionString;

        public Sign_up_to_MoodMate()
        {
            InitializeComponent();
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }


        private void Sign_In_Click(object sender, EventArgs e)
        {
            Form1 Sign_in = new Form1();
            Sign_in.Show();
            this.Hide();
        }


        private void IfUsernameNot6Characters()
        {
            if (username.Text.Length <= 6)
            {
                usernameLabel.ForeColor = Color.Red;
            }
        }


        private void IfPasswordOver16Characters()
        {
            if (password.Text.Length > 16)
            {
                passwordLabel.ForeColor = Color.Red;
            }
        }



        private void PassWordMatchingToConfirmPassword()
        {
            if (password.Text != confirmpassword.Text)
            {
                Confirmpass.ForeColor = Color.Red;
            }
        }


        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
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
                    builder.Append(b.ToString("x2")); // convert to hex
                }
                return builder.ToString();
            }
        }



        private void SuccessRegisterIfAllValid()
        {
            if (username.Text.Length >= 6 && password.Text.Length <= 16)
            {
                try
                {
                    using (SQLiteConnection connection = new SQLiteConnection(DbConnectionString))
                    {
                        connection.Open();
                        string insert = "insert into Login (username,password,email) values (@username,@password,@email)";
                        using (SQLiteCommand cmd = new SQLiteCommand(insert, connection))
                        {

                            string hashedPassword = HashPassword(password.Text);

                            cmd.Parameters.AddWithValue("@username", username.Text);
                            cmd.Parameters.AddWithValue("@password", hashedPassword);
                            cmd.Parameters.AddWithValue("@email", email.Text);

                            cmd.ExecuteNonQuery();
                            username.Clear();
                            email.Clear();
                            password.Clear();
                            confirmpassword.Clear();
                            MessageBox.Show("Successfully sign up", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
            }
            else
            {
                IfUsernameNot6Characters();
                IfPasswordOver16Characters();
                PassWordMatchingToConfirmPassword();
            }
        }




        private void Signup_Click(object sender, EventArgs e)
        {
            if (!IsValidEmail(email.Text))
            {
                emailLabel.ForeColor = Color.Red;
            }
            else
            {
                SuccessRegisterIfAllValid();
            }
        }



        private void username_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                email.Focus();
            }
        }

        private void email_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                password.Focus();
            }
        }

        private void password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                confirmpassword.Focus();
            }
        }

        private void confirmpassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Signup.PerformClick();
            }
        }




    }
}
