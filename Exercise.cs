using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using WMPLib;
using System.Diagnostics;
using System.Speech.Synthesis;





namespace Escarez_Finals_Mental_Health_Tracker
{
    public partial class Exercise : Form
    {

        private bool hasSpoken;

        private SpeechSynthesizer synthesizer;
        

        SoundPlayer player;
        WindowsMediaPlayer axWindowsMediaPlayer1;


        private enum BreathingPhase { Inhale, Hold, Exhale }
        private BreathingPhase currentPhase;
        private int phaseTime;
        private int maxPhaseTime = 100;

        private int circleSize = 120;
        private int maxCircleSize = 350;
        private int minCircleSize = 120;

        private int sessionDurationMs = 100000; // 2 min
        private int sessionElapsedMs = 0;

        private PointF[] auraDotsPositions;
        private float[] auraDotsAngles;
        private int auraDotsCount = 20;
        private Random rand = new Random();



        public Exercise()
        {
            InitializeComponent();


            synthesizer = new SpeechSynthesizer(); // <-- Move this here
            synthesizer.SelectVoiceByHints(VoiceGender.Female);
            synthesizer.Rate = -1; // default speaking rate



            player = new SoundPlayer(@".\Music.wav");
            TypeWriterEffect();

            this.WindowState = FormWindowState.Maximized;

            // Enable double buffering on panelAnimation
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, panelAnimation, new object[] { true });

            auraDotsPositions = new PointF[auraDotsCount];
            auraDotsAngles = new float[auraDotsCount];

            for (int i = 0; i < auraDotsCount; i++)
                auraDotsAngles[i] = (float)(rand.NextDouble() * 2 * Math.PI);

            panelAnimation.Paint += panelAnimation_Paint;
            panelAnimation.Click += panelAnimation_Click;

            // Use ExerciseTimer declared in Designer
            ExerciseTimer.Interval = 30;
            ExerciseTimer.Tick += ExerciseTimer_Tick;

            this.Load += Exercise_Load;
        }





        private async Task TypeWriterEffect()
        {
            WelcomeHome.Text = "";
            SubContent.Text = "";

            string content = "Breathing Exercise";
            string Subcontent = "Take a Breath, You Deserve It.";

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




        private void Exercise_Load(object? sender, EventArgs e)
        {
            hasSpoken = false;

            player.PlayLooping();

            currentPhase = BreathingPhase.Hold;  // default phase na static
            circleSize = minCircleSize;

            lblPhase.Text = "Waiting"; // nagpapahiwatig na hindi pa nagsisimula breathing
            lblPhase.ForeColor = Color.Gray;

            ExerciseTimer.Start();  // start na agad para umikot ang aura dots
        }



        private void panelAnimation_Click(object? sender, EventArgs e)
        {
            StartExercise();
        }



        private void StartExercise()
        {
            currentPhase = BreathingPhase.Inhale;
            phaseTime = 0;
            sessionElapsedMs = 0;
            lblPhase.Text = "Inhale";
            lblPhase.ForeColor = Color.FromArgb(224, 224, 224);

            ExerciseTimer.Stop();
            ExerciseTimer.Start();
        }



        private void ExerciseTimer_Tick(object? sender, EventArgs e)
        {
            for (int i = 0; i < auraDotsCount; i++)
            {
                auraDotsAngles[i] += 0.02f;
                if (auraDotsAngles[i] > 2 * Math.PI)
                    auraDotsAngles[i] -= (float)(2 * Math.PI);
            }

            if (lblPhase.Text == "Waiting")
            {
                // Static circle size, just rotate aura dots and repaint
                circleSize = minCircleSize;
            }
            else if (ExerciseTimer.Enabled && lblPhase.Text != "Task Completed")
            {
                phaseTime++;
                sessionElapsedMs += ExerciseTimer.Interval;

                Color phaseColor = Color.LightBlue;

                switch (currentPhase)
                {
                    case BreathingPhase.Inhale:
                        lblPhase.Text = "Inhale";
                        if (!hasSpoken)
                        {
                            synthesizer.SpeakAsync("Inhale");
                            hasSpoken = true;
                        }
                        circleSize = minCircleSize + (int)((maxCircleSize - minCircleSize) * (phaseTime / (float)maxPhaseTime));
                        phaseColor = Color.FromArgb(64, 64, 64);
                        break;

                    case BreathingPhase.Hold:
                        lblPhase.Text = "Hold";
                        if (!hasSpoken)
                        {
                            synthesizer.SpeakAsync("Hold");
                            hasSpoken = true;
                        }
                        circleSize = maxCircleSize;
                        phaseColor = Color.FromArgb(64, 64, 64);
                        break;

                    case BreathingPhase.Exhale:
                        lblPhase.Text = "Exhale";
                        if (!hasSpoken)
                        {
                            synthesizer.SpeakAsync("Exhale");
                            hasSpoken = true;
                        }
                        circleSize = maxCircleSize - (int)((maxCircleSize - minCircleSize) * (phaseTime / (float)maxPhaseTime));
                        phaseColor = Color.FromArgb(64, 64, 64);
                        break;
                }


                lblPhase.ForeColor = phaseColor;

                if (phaseTime >= maxPhaseTime)
                {
                    phaseTime = 0;
                    hasSpoken = false; // <-- Reset for next phase

                    currentPhase = currentPhase switch
                    {
                        BreathingPhase.Inhale => BreathingPhase.Hold,
                        BreathingPhase.Hold => BreathingPhase.Exhale,
                        _ => BreathingPhase.Inhale,
                    };
                }


                if (sessionElapsedMs >= sessionDurationMs)
                {
                   
                    lblPhase.Text = "Task Completed";
                    synthesizer.SpeakAsync("Task Completed");
                    lblPhase.ForeColor = Color.Gray;
                     
                }
            }

            panelAnimation.Invalidate();
        }




        private void panelAnimation_Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int centerX = panelAnimation.Width / 2;
            int centerY = panelAnimation.Height / 2;
            int x = centerX - circleSize / 2;
            int y = centerY - circleSize / 2;

            Color phaseColor = currentPhase switch
            {
                BreathingPhase.Inhale => Color.FromArgb(100, 200, 255),
                BreathingPhase.Hold => Color.FromArgb(130, 160, 255),
                BreathingPhase.Exhale => Color.FromArgb(255, 150, 150),
                _ => Color.LightBlue
            };

            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddEllipse(x, y, circleSize, circleSize);
                using (var brush = new System.Drawing.Drawing2D.PathGradientBrush(path))
                {
                    brush.CenterColor = Color.White;
                    brush.SurroundColors = new[] { phaseColor };
                    g.FillEllipse(brush, x, y, circleSize, circleSize);
                }
            }

            for (int i = 1; i <= 6; i++)
            {
                int glowSize = circleSize + (i * 15);
                int glowX = centerX - glowSize / 2;
                int glowY = centerY - glowSize / 2;
                int alpha = Math.Max(0, 40 - (i * 5));
                using (SolidBrush glowBrush = new SolidBrush(Color.FromArgb(alpha, phaseColor)))
                {
                    g.FillEllipse(glowBrush, glowX, glowY, glowSize, glowSize);
                }
            }

            int auraRadius = circleSize / 2 + 30;

            for (int i = 0; i < auraDotsCount; i++)
            {
                float angle = auraDotsAngles[i];
                float dotX = centerX + auraRadius * (float)Math.Cos(angle);
                float dotY = centerY + auraRadius * (float)Math.Sin(angle);

                int dotSize = 5;
                using (SolidBrush dotBrush = new SolidBrush(Color.FromArgb(120, phaseColor)))
                {
                    g.FillEllipse(dotBrush, dotX - dotSize / 2, dotY - dotSize / 2, dotSize, dotSize);
                }
            }
        }



        private void Dashboard_Click(object sender, EventArgs e)
        {
            synthesizer.Pause();
            new Home().Show();
            player.Stop(); 
            this.Hide();
        }


        private void Mood_Click(object sender, EventArgs e)
        {
            synthesizer.Pause();
            new Mood().Show();
            player.Stop();
            this.Hide();
        }


        private void Journal_Click(object sender, EventArgs e)
        {
            synthesizer.Pause();
            new Journal().Show();
            player.Stop();
            this.Hide();
        }


        private void Ask_Josh_Click(object sender, EventArgs e)
        {
            synthesizer.Pause();
            new AskJosh().Show();
            this.Hide();
            player.Stop();
        }




    }
}
