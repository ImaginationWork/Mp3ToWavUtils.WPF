using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;


namespace Mp3ToWavUtils.WPF
{
    public partial class MainWindow : Window
    {
        private static StringBuilder output = new StringBuilder();

        private static string appDataDir;
        private static string appDataPath_setting;
        private static string appDataPath_ffmpeg;
        private static string appDataPath_tempMp3;
        private static string appDataPath_tempWav;

        private System.Media.SoundPlayer m_player;

        private IniFile m_ini;

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                var mainExeDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                appDataPath_setting = System.IO.Path.Combine(mainExeDir, "settings.ini");
                appDataPath_ffmpeg = System.IO.Path.Combine(mainExeDir, "ffmpeg.exe");

                appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                if (!System.IO.File.Exists(appDataPath_ffmpeg))
                {
                    throw new Exception("ffmpeg.exe could not found.");
                }

                appDataPath_tempMp3 = System.IO.Path.Combine(appDataDir, "temp.mp3");
                appDataPath_tempWav = System.IO.Path.Combine(appDataDir, "temp.wav");


                m_ini = new IniFile(appDataPath_setting);
                var wavPath = m_ini.ReadKey("DefaultWavDir");
                if (wavPath != "")
                {
                    defaultWavDirectory.Text = wavPath;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                Environment.Exit(0);
            }
        }

        public static int StartFFmpeg(string exeutable, string arguments)
        {
            var ret = -1;

            try
            {
                Process process = new Process();
                process.StartInfo.FileName = exeutable;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                if (!process.Start())
                {
                    throw new Exception("Could not start " + exeutable);
                }
                process.WaitForExit();

                // ffmpeg use StandardError
                System.IO.StreamReader reader = process.StandardError;
                string line;
                output.Clear();
                while ((line = reader.ReadLine()) != null)
                {
                    output.AppendLine(line);
                }

                ret = process.ExitCode;

                process.Close();

                return ret;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return ret;
        }

        private void RefreshWavPath()
        {
            try
            {
                pathOfWav.Text = System.IO.Path.Combine(defaultWavDirectory.Text, System.IO.Path.GetFileNameWithoutExtension(pathOfMp3.Text) + ".wav");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonToTransform_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!System.IO.File.Exists(pathOfMp3.Text))
                {
                    ShowWarning(pathOfMp3.Text + " not exist!");
                    return;
                }

                if (!System.IO.File.Exists(pathOfWav.Text))
                {
                    if (!System.IO.Directory.Exists(defaultWavDirectory.Text))
                    {
                        ShowWarning("Please Specific the Ouput Dir or Wav Path!");
                    }
                    else
                    {
                        RefreshWavPath();
                    }
                }

                System.IO.File.Copy(pathOfMp3.Text, appDataPath_tempMp3, true);

                var ret = StartFFmpeg(appDataPath_ffmpeg, " -hide_banner  -i " + appDataPath_tempMp3 + "  -ac 1 " + appDataPath_tempWav + " -y");


                System.IO.File.Delete(pathOfWav.Text);

                System.IO.File.Move(appDataPath_tempWav, pathOfWav.Text);

                if (0 != ret)
                {
                    ShowWarning(appDataPath_ffmpeg + " ffmpeg error");
                    MessageBox.Show(output.ToString(), "Error: " + pathOfMp3.Text);
                }
                else
                {
                    CopyStringToClipboard(pathOfWav.Text);
                    ShowInfo(pathOfWav.Text + " generated, and the path of wav is copy to the clipboard");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ShowWarning(string message)
        {
            statusText.Text = message;
            statusText.Foreground = new SolidColorBrush(Colors.Red);
        }

        private void ShowInfo(string message)
        {
            statusText.Text = message;
            statusText.Foreground = new SolidColorBrush(Colors.Black);
        }


        private void FileDroped(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                pathOfMp3.Text = files[0];


                // clear output wav path
                pathOfWav.Text = "";

                if (checkBox_refreshWavPath.IsChecked.HasValue && checkBox_refreshWavPath.IsChecked.Value == true)
                {
                    RefreshWavPath();
                }
            }
        }

        private void CopyStringToClipboard(string str)
        {
            Clipboard.SetDataObject(str);
        }

        private void buttonToCopyWavPath_Click(object sender, RoutedEventArgs e)
        {
            if (pathOfWav.Text != "")
            {
                CopyStringToClipboard(pathOfWav.Text);
                ShowInfo("Wav Path Copy to Clipboard!");
            }
        }

        private void playerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (m_player == null)
                {
                    m_player = new System.Media.SoundPlayer(pathOfWav.Text);
                    m_player.Play();
                    (sender as Button).Content = "||";
                }
                else
                {
                    m_player.Stop();
                    (sender as Button).Content = ">";
                    m_player = null;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void buttonToOpenFolderOfWav_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select," + pathOfWav.Text);
            }
            catch (Exception ex)
            {
                ShowWarning(ex.ToString());
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (System.IO.Directory.Exists(defaultWavDirectory.Text))
            {
                m_ini.WriteKey("DefaultWavDir", defaultWavDirectory.Text);
            }
        }

    }
}
