using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Cache;
using System.Timers;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using NAudio.Wave;
using NAudio.WaveFormRenderer;

namespace GUI2
{
    class SoundProcessing
    {
        private MainWindow mw;
        public WaveOutEvent outputDevice;
        public AudioFileReader audioFile;
        private string audioFilePath;
        private Timer timer;
        private Thumb thumb;
        private PythonScripts ps;

        public SoundProcessing(PythonScripts ps)
        {
            this.ps = ps;
        }

        public SoundProcessing()
        {

        }

        private void createAudioPlayer()
        {
            if (timer != null)
            {
                timer.Stop();
            }
            if (outputDevice != null & audioFile != null)
            {
                outputDevice.Dispose();
                outputDevice = null;
                audioFile.Dispose();
                audioFile = null;
            }
            
            this.timer = audioTimer();

            mw.playBtn.Click += OnButtonPlayClick;
            mw.stopBtn.Click += OnButtonStopClick;
            mw.Closing += OnButtonStopClick;
            mw.choosebtn.Click += OnButtonStopClick;

            thumb = (mw.audioSlider.Template.FindName("PART_Track", mw.audioSlider) as Track).Thumb;
            thumb.MouseEnter += OnSliderEnter;
            //mw.waveformImg.MouseEnter += OnSliderEnter;

            mw.playBtn.IsEnabled = true;
            mw.stopBtn.IsEnabled = true;
            mw.audioSlider.IsEnabled = true;

            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += OnPlaybackStopped;
        }

        public void loadAudio(MainWindow mw, string audioFilePath)
        {
            this.mw = mw;
            this.audioFilePath = audioFilePath;
            
            if (outputDevice == null)
            {
                createAudioPlayer();
            }

            audioFile = new AudioFileReader(audioFilePath);
            outputDevice.Init(audioFile);
            updateDisplay();
            mw.audioSlider.Value = 0;

            generateWaveForm(mw);

            ps.runMelSpectrogramAsync(audioFilePath, mw);
            //new PythonScripts(mw, mw.pythonpath).runMelSpectrogramAsync(audioFilePath, mw);
        }

        public void loadVstAudio(MainWindow mw, string audioFilePath)
        {
            this.mw = mw;
            this.audioFilePath = audioFilePath;
            outputDevice = new WaveOutEvent();
            audioFile = new AudioFileReader(audioFilePath);
            outputDevice.Init(audioFile);
            outputDevice.PlaybackStopped += OnPlaybackStoppedLite;
        }

        public void play()
        {
            outputDevice.Play();
        }

        private Boolean isPlaying = false;

        private void OnButtonPlayClick(object sender, EventArgs args)
        {
            if (!isPlaying)
            {
                outputDevice.Play();
                timer.Start();
                isPlaying = !isPlaying;
                mw.playBtn.Content = "Pause";
            } else
            {
                outputDevice.Stop();
                timer.Stop();
                isPlaying = !isPlaying;
                mw.playBtn.Content = "Play";
            }
        }

        private void OnButtonStopClick(object sender, EventArgs args)
        {
            outputDevice.Stop();
            timer.Stop();
            if (audioFile != null)
            {
                audioFile.CurrentTime = new TimeSpan(0, 0, 0, 0, 0);
                updateDisplay();
                mw.playBtn.Content = "Play";
                isPlaying = false;
            }
        }

        public void stopPlayback()
        {
            if (audioFile != null & outputDevice != null)
            {
                outputDevice.Stop();
                timer.Stop();
                audioFile.CurrentTime = new TimeSpan(0, 0, 0, 0, 0);
                updateDisplay();
                mw.playBtn.Content = "Play";
                isPlaying = false;
            }
        }


        public void stopPlaybackLite()
        {
            if (audioFile != null & outputDevice != null)
            {
                outputDevice.Stop();
                audioFile.CurrentTime = new TimeSpan(0, 0, 0, 0, 0);
                isPlaying = false;
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            timer.Stop();
            if(audioFile != null)
            {
                TimeSpan ctime = audioFile.CurrentTime;
                TimeSpan ttime = audioFile.TotalTime;

                string currentTime = sanitizeDigits(ctime.Minutes) + ":" + sanitizeDigits(ctime.Seconds);
                string totalTime = sanitizeDigits(ttime.Minutes) + ":" + sanitizeDigits(ttime.Seconds);

                if (currentTime.Equals(totalTime))
                {
                    audioFile.CurrentTime = new TimeSpan(0, 0, 0, 0, 0);
                    updateDisplay();
                    mw.playBtn.Content = "Play";
                    isPlaying = false;
                    mw.audioSlider.Value = 0;
                }
            }
        }

        private void OnPlaybackStoppedLite(object sender, StoppedEventArgs args)
        {
            if (audioFile != null & outputDevice != null)
            {
                outputDevice.Stop();
                audioFile.CurrentTime = new TimeSpan(0, 0, 0, 0, 0);
                isPlaying = false;
                mw.playSampleBtn.Content = "Play Selected Sample";
            }
        }

        private void OnSliderEnter(object sender, EventArgs args)
        {
            Debug.WriteLine("MOUSE ENTERED SLIDER");
        }

        private void OnSliderMoved(object sender, RoutedEventArgs args)
        {
            //if (mw.audioSlider.IsMouseOver)
            //{
                double pos = mw.audioSlider.Value * audioFile.TotalTime.TotalMilliseconds / 100;
                audioFile.CurrentTime = new TimeSpan(0, 0, 0, 0, (int)pos);
                updateDisplay();
            //}
        }

        private void generateWaveForm(MainWindow mw)
        {
            var blockSize = 200;
            var sampleInterval = 200;
            var scaleFactor = 4;
            var maxPeakProvider = new MaxPeakProvider();
            var rmsPeakProvider = new RmsPeakProvider(blockSize); // e.g. 200
            var samplingPeakProvider = new SamplingPeakProvider(sampleInterval); // e.g. 200
            var averagePeakProvider = new AveragePeakProvider(scaleFactor); // e.g. 4

            //var myRendererSettings = new StandardWaveFormRendererSettings();
            var myRendererSettings = new SoundCloudBlockWaveFormSettings(
                Color.FromArgb(0, 150, 255),
                Color.FromArgb(0, 50, 255),
                Color.FromArgb(140, 198, 255),
                Color.FromArgb(200, 230, 255));
            myRendererSettings.Width = (int) mw.waveformImg.Width;
            myRendererSettings.TopHeight = 64;
            myRendererSettings.BottomHeight = 64;

            var renderer = new WaveFormRenderer();
            var audioFilePath = this.audioFilePath;
            var image = renderer.Render(audioFilePath, averagePeakProvider, myRendererSettings);

            var filename = "temp.png";
            image.Save(filename, ImageFormat.Png);
            var filePath = AppDomain.CurrentDomain.BaseDirectory + "\\" + filename;
            //bitmap = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\" + filename));
            //mw.waveformImg.Source = bitmap;

            mw.waveformImg.Source = null;

            BitmapImage displayImage = new BitmapImage();
            displayImage.BeginInit();
            displayImage.CacheOption = BitmapCacheOption.None;
            displayImage.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            displayImage.CacheOption = BitmapCacheOption.OnLoad;
            displayImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            displayImage.UriSource = new Uri(filePath);
            displayImage.EndInit();
            mw.waveformImg.Source = displayImage;

            try {
                var reader = new WaveFileReader(audioFilePath);
                mw.fileBitrateTxt.Content = (reader.WaveFormat.AverageBytesPerSecond * 8) / 1000 + " Kb/s";
                mw.fileSampleRateTxt.Content = reader.WaveFormat.SampleRate + " Hz";
                mw.fileTotalTimeTxt.Content = audioFile.TotalTime.TotalMilliseconds + " Ms";
                string channeltxt = reader.WaveFormat.Channels == 2 ? " (Stereo)" : " (Mono)";
                mw.fileChannelsTxt.Content = reader.WaveFormat.Channels + channeltxt;
                reader.Dispose();
            } catch (Exception e)
            {
                mw.fileBitrateTxt.Content = "-";
                mw.fileSampleRateTxt.Content = "-";
                mw.fileTotalTimeTxt.Content = audioFile.TotalTime.TotalMilliseconds + " Ms";
                mw.fileChannelsTxt.Content = "-";
                Debug.WriteLine("WARNING: File is not a WAVE file. Only playback is possible and partial information could be retrieved.\n" + e.Message);
            }

            mw.loadFileProgress.Value = 66;

            splitTrackTime(audioFile.TotalTime.TotalMilliseconds);

        }

        // Creates 5 time labels for the audio player based on the track length
        private void splitTrackTime(double milliseconds)
        {
            string[] output = new string[5];
            double step = milliseconds / 5;
            double temp = 0;

            for (int i=0; i<5; ++i)
            {
                temp = temp + step;
                TimeSpan t = TimeSpan.FromMilliseconds(temp);
                output[i] = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
                //output[i] = sanitizeDigits(Math.Round(TimeSpan.FromMilliseconds(temp).TotalMinutes)) + ":" + sanitizeDigits(Math.Round(TimeSpan.FromMilliseconds(temp).TotalSeconds));
            }

            mw.timeTxt1.Content = output[0];
            mw.timeTxt2.Content = output[1];
            mw.timeTxt3.Content = output[2];
            mw.timeTxt4.Content = output[3];
            mw.timeTxt5.Content = output[4];
        }

        private Timer audioTimer()
        {
            Timer timer = new Timer();
            timer.Interval = 15;
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
            return timer;
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            mw.Dispatcher.Invoke(() =>
            {
                if (audioFile != null)
                {
                    updateDisplay();
                }
            });
        }

        private void updateDisplay()
        {
            TimeSpan ctime = audioFile.CurrentTime;
            TimeSpan ttime = audioFile.TotalTime;

            string currentTime = sanitizeDigits(ctime.Minutes) + ":" + sanitizeDigits(ctime.Seconds);
            string totalTime = sanitizeDigits(ttime.Minutes) + ":" + sanitizeDigits(ttime.Seconds);

            mw.timeTxt.Content = currentTime + " / " + totalTime;

            if (!mw.audioSlider.IsMouseOver)
            {
                var sliderPosition = ctime.TotalMilliseconds * 100 / ttime.TotalMilliseconds;
                mw.audioSlider.Value = sliderPosition;
            }
        }

        private string sanitizeDigits(double digit)
        {
            string output = "";
            if (digit < 10)
            {
                output = "0" + digit;
            }
            else if (digit > 99)
            {
                digit = digit > 1000 ? Double.Parse(digit.ToString().Substring(0, 3)) : digit;
                digit = Math.Round(digit / 1000, 3);
                output = (digit*100).ToString();
            }
            else
            {
                output = digit.ToString();
            }
            return output;
        }

        public void dispose()
        {
            if (outputDevice != null)
                outputDevice.Dispose();
            if(audioFile !=null)
                audioFile.Dispose();
        }

    }
}
