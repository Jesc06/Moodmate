using Guna.Charts.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Escarez_Finals_Mental_Health_Tracker
{
    public partial class MoodmateIntro : Form
    {
        public MoodmateIntro()
        {
            InitializeComponent();
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        }


        

        private async void showAnotherForm()
        {
            title.Text = "";
            string content = "Moodmate";
            foreach(char a in content)
            {
                title.Text += a;
                await Task.Delay(80);
            }
            await Task.Delay(2000);
            new Form1().Show();
            this.Hide();
        }


        private void MoodmateIntro_Load(object sender, EventArgs e)
        {
            showAnotherForm();
        }


    }
}
