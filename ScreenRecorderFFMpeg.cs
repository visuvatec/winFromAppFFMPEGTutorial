using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using Accord.Video.FFMPEG;

namespace ScreenMakerWindowsFormsApp
{
    class ScreenRecorderFFMpeg
    {

        // video variables
        private Rectangle bounds;
        private string outputPath = "";
        private string tempPath = "";
        private double fileCount = 1;
        private List<string> inputImageSequence = new List<string>();

        //file variables
        private string audioName = "mic.wav";
        private string videoName = "video.mp4";
        private string finalName = "finalVideo.mp4";

        //Time variable
        Stopwatch watch = new Stopwatch();

        //Audio variable
        public static class NativeMethods
        {
            [DllImport("winmm.dll", EntryPoint = "mciSendStringA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
            public static extern int record(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);
        }


        public ScreenRecorderFFMpeg(Rectangle b, string outPath)
        {
            CreateTempFolder("tempScreenshots");

            bounds = b;
            outputPath = outPath;
        }

        private void CreateTempFolder(string name)
        {

            string pathName = $"C://{name}";

            if (Directory.Exists("D://"))
            {
                pathName = $"D://{name}";
            }
            Directory.CreateDirectory(pathName);
            tempPath = pathName;



        }

        private void DeletePath(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach(string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach(string dir in dirs)
            {
                DeletePath(dir);
            }

            Directory.Delete(targetDir, false);
        }

        private void DeleteFileExcept(string targetFile, string excepFile)
        {
            string[] files = Directory.GetFiles(targetFile);

            foreach(string file in files)
            {
                if(file != excepFile)
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);

                }
            }

        }


        public void cleanup()
        {
            if(Directory.Exists(tempPath))
            {
                DeletePath(tempPath);
            }
        }

        public string getElapsed()
        {
            return string.Format("{0:00}:{1:00}:{2:00}",
                    watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);
        }

        public void RecordVideo()
        {
            watch.Start();

            using(Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using(Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top),
                        Point.Empty, bounds.Size);
                }
                string name = tempPath + "//screenshot-" + fileCount + ".png";
                bitmap.Save(name, ImageFormat.Png);
                inputImageSequence.Add(name);
                fileCount++;


                bitmap.Dispose();
            }
        }


        public void RecordAudio()
        {
            NativeMethods.record("open new Type waveaudio Alias recsound", "", 0, 0);
            NativeMethods.record("record recsound", "", 0, 0);
        }

        private void SaveVideo(int width, int height, int frameRate)
        {
            using(VideoFileWriter vfWriter = new VideoFileWriter())
            {
                vfWriter.Open(outputPath + "//" + videoName, width, height,
                    frameRate, VideoCodec.MPEG4);

                foreach(string imageLoc in inputImageSequence)
                {
                    Bitmap imageFrame = System.Drawing.Image.FromFile(imageLoc) as Bitmap;
                    vfWriter.WriteVideoFrame(imageFrame);
                    imageFrame.Dispose();
                
                }

                vfWriter.Close();
               
            }

        }


        private void SaveAudio()
        {
            string aduioPath = "save recsound " + outputPath + "//" + audioName;
            NativeMethods.record(aduioPath, "", 0, 0);
            NativeMethods.record("close recsound", "", 0, 0);
        }


        private void CombineVideoAndAudio(string video, string audio)
        {
            string command = $"/c ffmpeg -i \"{video}\" -i \"{audio}\" -shortest {finalName}";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                FileName = "cmd.exe",
                WorkingDirectory = outputPath,
                Arguments = command
            };

            using (Process execProcess = Process.Start(startInfo))
            {
                execProcess.WaitForExit();
            }

        }


        public void stop()
        {
            watch.Stop();

            int width = bounds.Width;
            int height = bounds.Height;
            int frameRate = 10;

            SaveAudio();

            SaveVideo(width, height, frameRate);

            CombineVideoAndAudio(videoName, audioName);

            DeletePath(tempPath);

            DeleteFileExcept(outputPath, outputPath + "\\" + finalName);
        }



    }

}

