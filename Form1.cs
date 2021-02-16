using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenMakerWindowsFormsApp
{
    public partial class Form1 : Form
    {
        bool folderSelected = false;
        string outputPath = "";
        string finalVideoName = "FinalVideo.mp4";

        ScreenRecorderFFMpeg screenRecorder = new ScreenRecorderFFMpeg(new Rectangle(), "");


        public Form1()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {

        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            tmrRecord.Stop();
            screenRecorder.stop();
            Application.Restart();

        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            if (folderSelected)
            {
                tmrRecord.Start();
            }else
            {
                MessageBox.Show("Select a Folder to start record", "Error");
            }
            
        }

        private void folderbutton_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = "Select an output folder";

            if(folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                outputPath = folderBrowser.SelectedPath;
                folderSelected = true;


                Rectangle bounds = Screen.FromControl(this).Bounds;
                screenRecorder = new ScreenRecorderFFMpeg(bounds, outputPath);

            } else
            {
                MessageBox.Show("Please select a folder", "Error");

            }


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            screenRecorder.RecordAudio();
            screenRecorder.RecordVideo();
            lblTime.Text = screenRecorder.getElapsed();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
