using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.Charts.WinForms;
using System.Data.SQLite;
using System.Configuration;
using OfficeOpenXml;
using System.IO;
using System.Globalization;
using OfficeOpenXml.Style;
using System.Diagnostics;



namespace Escarez_Finals_Mental_Health_Tracker
{
    public partial class Home : Form
    {

        private string DbConnectionString = ConfigurationManager.ConnectionStrings["moodmate"].ConnectionString;
        public string empty = null;
        public Home()
        {
            InitializeComponent();
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;


            Bar_Chart();
            Pie_Chart();
            DisplayEmotionCounts();

            Tooltip();

        }



        private void Tooltip()
        {
            ToolTip tooltip = new ToolTip();
            tooltip.SetToolTip(GenerateReport, "Generate Report");
        }


        private async void Home_Load(object sender, EventArgs e)
        {
            await TypeWriterEffect();
        }



        private async Task TypeWriterEffect()
        {
            WelcomeHome.Text = "";
            SubContent.Text = "";

            string content = "Welcome to Moodmate";
            string Subcontent = "A Daily Check-In With Yourself";

            foreach (char type in content)
            {
                WelcomeHome.Text += type;
                await Task.Delay(100);
            }
            await Task.Delay(300);
            foreach (char type in Subcontent)
            {
                SubContent.Text += type;
                await Task.Delay(80);
            }

        }



        #region Generate Report


        private void ExportToExcel()
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Emotion Report");

                    // Get date from database
                    var moodEntries = GetMoodEntries();

                    // get the emotion counts
                    var emotionCounts = moodEntries
                        .Where(e => !string.IsNullOrWhiteSpace(e.Emotion))
                        .GroupBy(e => e.Emotion.ToLower())
                        .ToDictionary(g => g.Key, g => g.Count());

                    // target emotion
                    string[] targetEmotions = { "happy", "angry", "sad", "anxious" };


                    // MAIN TABLE FORMATTING

                    worksheet.Column(1).Width = 15;  // Column A: Emotion
                    worksheet.Column(2).Width = 10;  // Column B: Count
                    worksheet.Column(3).Width = 5;   // Column C: Spacer
                    worksheet.Column(4).Width = 5;   // Column D: Spacer

                    // Table headers
                    worksheet.Cells["A1:B1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells["A1:B1"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    worksheet.Cells["A1"].Value = "Emotion";
                    worksheet.Cells["B1"].Value = "Count";

                    // Populate data
                    int row = 2;
                    foreach (var emotion in targetEmotions)
                    {
                        int count = emotionCounts.ContainsKey(emotion) ? emotionCounts[emotion] : 0;
                        worksheet.Cells[$"A{row}"].Value = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(emotion);
                        worksheet.Cells[$"B{row}"].Value = count;
                        row++;
                    }

                    // Add borders
                    var lastDataRow = targetEmotions.Length + 1;
                    worksheet.Cells[$"A1:B{lastDataRow}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A1:B{lastDataRow}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A1:B{lastDataRow}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[$"A1:B{lastDataRow}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;


                    // TREND SUMMARY
                    int maxCount = targetEmotions
                        .Select(e => emotionCounts.ContainsKey(e) ? emotionCounts[e] : 0)
                        .Max();

                    var topEmotions = targetEmotions
                        .Where(e => emotionCounts.ContainsKey(e) && emotionCounts[e] == maxCount && maxCount > 0)
                        .ToList();

                    if (topEmotions.Any())
                    {
                        int summaryRow = lastDataRow + 2;
                        var trendCell = worksheet.Cells[$"A{summaryRow}:D{summaryRow}"];
                        trendCell.Merge = true;

                        // emotional trends sum
                        trendCell.Value = $"Emotional trends are: {string.Join(", ", topEmotions.Select(e => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(e)))} ({maxCount})";

                        // Style trend text
                        trendCell.Style.Font.Bold = true;
                        trendCell.Style.Font.Color.SetColor(Color.DarkBlue);
                        trendCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    }


                    worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
                    worksheet.PrinterSettings.Orientation = eOrientation.Portrait;
                    worksheet.PrinterSettings.FitToPage = true;


                    SaveFileDialog saveDialog = new SaveFileDialog
                    {
                        Filter = "Excel Files|*.xlsx",
                        FileName = "EmotionReport.xlsx"
                    };

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        package.SaveAs(new FileInfo(saveDialog.FileName));
                        MessageBox.Show("Na-save nang maayos!", "Tagumpay", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"May problema sa pag-export: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private List<MoodEntrys> GetMoodEntries()
        {
            var entries = new List<MoodEntrys>();

            using (var conn = new SQLiteConnection(DbConnectionString))
            {
                conn.Open();
                string query = "SELECT emotion FROM Mood;"; // Simplified query

                using (var cmd = new SQLiteCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var entry = new MoodEntrys();
                        entry.Emotion = reader.IsDBNull(reader.GetOrdinal("emotion")) ?
                            string.Empty :
                            reader.GetString(reader.GetOrdinal("emotion"));
                        entries.Add(entry);
                    }
                }
            }
            return entries;
        }



        private void GenerateReport_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }


        #endregion



        private void DisplayEmotionCounts()
        {
            var emotionCounts = GetEmotionCounts();

            // Reset muna labels para di madoble o magkalituhan
            happy.Text = "0";
            sad.Text = "0";
            Angry.Text = "0";
            Anxious.Text = "0";


            foreach (var entry in emotionCounts)
            {
                switch (entry.Key.ToLower())
                {
                    case "happy":
                        happy.Text = entry.Value.ToString();
                        break;
                    case "sad":
                        sad.Text = entry.Value.ToString();
                        break;
                    case "angry":
                        Angry.Text = entry.Value.ToString();
                        break;
                    case "anxious":
                        Anxious.Text = entry.Value.ToString();
                        break;
                }
            }
        }





        private Dictionary<string, int> GetEmotionCounts()
        {
            var counts = new Dictionary<string, int>();

            // Adjust the connection string according to your database location
            string connectionString = DbConnectionString;

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT emotion, COUNT(*) as count FROM Mood GROUP BY emotion;";

                using (var cmd = new SQLiteCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string emotion = reader["emotion"].ToString();
                        int count = Convert.ToInt32(reader["count"]);
                        counts[emotion] = count;
                    }
                }
            }

            return counts;
        }




        private void Bar_Chart()
        {
            gunaChart1.Datasets.Clear();
            var barData = new GunaBarDataset { Label = "Mood Level" };

            var emotionCounts = GetEmotionCounts();
            foreach (var entry in emotionCounts)
            {
                barData.DataPoints.Add(new LPoint(entry.Key, entry.Value));
            }

            gunaChart1.Datasets.Add(barData);
            gunaChart1.Update();
        }




        private void Pie_Chart()
        {
            gunaChart2.Datasets.Clear();
            var pieData = new GunaPieDataset { Label = "Mood Distribution" };

            var emotionCounts = GetEmotionCounts();
            foreach (var entry in emotionCounts)
            {
                pieData.DataPoints.Add(new LPoint(entry.Key, entry.Value));
            }

            gunaChart2.Datasets.Add(pieData);
            gunaChart2.Update();
        }




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



    }
}
