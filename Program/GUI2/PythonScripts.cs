using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Net.Cache;

namespace GUI2
{
    class PythonScripts
    {

        private string pythonpath = "";
        private string modelName = "";
        private MainWindow mw;
        private bool isRunning = false;

        public PythonScripts(MainWindow mw, string pythonpath)
        {
            this.mw = mw;
            mw.modelSelect.SelectionChanged += onModelChanged;
            mw.modelSelect.SelectedIndex = 0;
            mw.classifybtn.Click += onClassifyBtnClicked;
            this.pythonpath = pythonpath;
        }

        private void onModelChanged(object sender, EventArgs args)
        {
            switch (mw.modelSelect.SelectedIndex)
            {
                case 0:
                    this.modelName = "single-pred-binary.py";
                    mw.accuracyTxt.Content = "98%";
                    break;
                case 1:
                    this.modelName = "single-pred-multi.py";
                    mw.accuracyTxt.Content = "81%";
                    break;
                default:
                    break;
            }
        }

        private void onClassifyBtnClicked(object sender, EventArgs args)
        {
            if (!isRunning)
            {
                runClassifierAlgoAsync(this.modelName);
                isRunning = true;
            }
        }


        // The below methods runs the python scripts in a background process, leaving the UI responsive
        public void runClassifierAlgoAsync(string modelName)
        {
            BackgroundWorker ticker = runTicker(mw);
            ticker.RunWorkerAsync();

            //this.mw.resultxt.Text = "Running...";

            BackgroundWorker bw = new BackgroundWorker();

            string result = "";

            // what to do in the background thread
            bw.DoWork += new DoWorkEventHandler(
            delegate (object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;

                result = "";
                ProcessStartInfo start = new ProcessStartInfo();
                //start.FileName = @"D:\Misc\Napier\Honours-Project\zgui\kerasenv\Scripts\python.exe";
                Debug.WriteLine(mw.pythonpath + "\n" + @AppDomain.CurrentDomain.BaseDirectory + modelName + " \"" + @AppDomain.CurrentDomain.BaseDirectory + "temp-classify.png" + "\" ");
                start.FileName = mw.pythonpath;
                start.Arguments = @AppDomain.CurrentDomain.BaseDirectory + modelName + " \"" + @AppDomain.CurrentDomain.BaseDirectory + "temp-classify.png" + "\" ";
                start.WindowStyle = ProcessWindowStyle.Hidden;
                start.CreateNoWindow = true;
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;

                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                    }
                }
                args.Result = result;
            });

            // what to do when worker completes its task (notify the user)
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate (object o, RunWorkerCompletedEventArgs args)
            {
                ticker.CancelAsync();
                Debug.WriteLine("Finished! Result: " + args.Result);
                mw.resultxt.Text = args.Result.ToString();
                mw.classifybtn.IsEnabled = true;
                mw.resultTab.IsEnabled = true;
                isRunning = false;
            });

            bw.RunWorkerAsync();
        }





        // Another background worker that simply updates the UI while the python process is running
        // It may be overkill to have it working on another thread, a traditional ticker could have done the job.
        public BackgroundWorker runTicker(MainWindow mw)
        {
            int timecounter = 0;
            BackgroundWorker bw = new BackgroundWorker();
            // this allows our worker to report progress during work
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            // what to do in the background thread
            bw.DoWork += new DoWorkEventHandler(
            delegate (object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;

                // do some simple processing for 10 seconds
                int step = 0;
                for (int i = 1; i <= 50; i++)
                {
                    if (bw.CancellationPending)
                    {
                        args.Cancel = true;
                        return;
                    }   
                    string message = step == 0 ? "Running" : step == 1 ? "Running." : step == 2 ? "Running.." : "Running...";
                    step = step == 3 ? 0 : step+1;
                    b.ReportProgress(i, message);
                    timecounter += 500;
                    Thread.Sleep(1000);
                }

            });

            // what to do when progress changed (update the progress bar for example)
            bw.ProgressChanged += new ProgressChangedEventHandler(
            delegate (object o, ProgressChangedEventArgs args)
            {
                mw.resultxt.Text = args.UserState.ToString();
            });

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate (object o, RunWorkerCompletedEventArgs args)
            {
                if (args.Cancelled)
                {
                    Debug.WriteLine("TICKER: Ticker cancelled");
                    mw.compileTimeTxt.Content = timecounter / 1000 + "s";
                }
            });

            return bw;
        }

        // The below generates a mel-spectrogram both for the UI (temp-display.png) and for the AI classifier (temp-classify.png)
        public void runMelSpectrogramAsync(string file, MainWindow mw)
        {
            mw.analyseTab.IsEnabled = false;

            BackgroundWorker bw = new BackgroundWorker();

            string result = "";

            // what to do in the background thread
            bw.DoWork += new DoWorkEventHandler(
            delegate (object o, DoWorkEventArgs args)
            {
                BackgroundWorker b = o as BackgroundWorker;

                result = "";
                ProcessStartInfo start = new ProcessStartInfo();
                //start.FileName = @"D:\Misc\Napier\Honours-Project\zgui\kerasenv\Scripts\python.exe";
                start.FileName = mw.pythonpath;
                start.Arguments = @AppDomain.CurrentDomain.BaseDirectory + "single-spectrogram.py" + " \"" + file + "\" \"" + @AppDomain.CurrentDomain.BaseDirectory + "\"";
                Debug.WriteLine(start.Arguments.ToString());
                start.WindowStyle = ProcessWindowStyle.Hidden;
                start.CreateNoWindow = true;
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;

                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                    }
                }
                args.Result = result;
            });

            // what to do when worker completes its task (notify the user)
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
            delegate (object o, RunWorkerCompletedEventArgs args)
            {
                Debug.WriteLine("Finished! Result: " + args.Result);
                if (args.Result.ToString().Contains("SUCCESS"))
                {
                    mw.graphPreview.Source = null;

                    BitmapImage displayImage = new BitmapImage();
                    displayImage.BeginInit();
                    displayImage.CacheOption = BitmapCacheOption.None;
                    displayImage.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                    displayImage.CacheOption = BitmapCacheOption.OnLoad;
                    displayImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    displayImage.UriSource = new Uri(@AppDomain.CurrentDomain.BaseDirectory + "\\temp-display.png");
                    displayImage.EndInit();
                    mw.graphPreview.Source = displayImage;

                    mw.analyseTab.IsEnabled = true;
                    mw.loadFileProgress.Value = 100;
                    mw. loadFileProgress.Visibility = Visibility.Hidden;

                    var fileinfo = new FileInfo(@AppDomain.CurrentDomain.BaseDirectory + "\\temp-classify.png");
                    mw.spectrogramSizeTxt.Content = Math.Round((fileinfo.Length / 1024f), 2) + " Kb";

                } else
                {
                    mw.loadFileProgress.Value = 100;
                    mw.loadFileProgress.Visibility = Visibility.Hidden;
                    MessageBox.Show("Something went wrong when processing audio file.\nPlease try again.", "Error");
                }
            });

            bw.RunWorkerAsync();
        }

    }
}
