using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GUI2
{
    class CompareResults
    {

        MainWindow mw;
        private ComboBox guitarPartComboBox;
        private ComboBox ampChannelComboBox;
        private ComboBox ampSettingsComboBox;
        private Button playButton;
        private string pathToAudioFile;
        private string path1;
        private string path2;
        private string path3;
        private bool isPlaying = false;
        SoundProcessing sp;


        public CompareResults(MainWindow mw)
        {
            this.mw = mw;
            this.guitarPartComboBox = mw.guitarPartComboBox;
            this.guitarPartComboBox.SelectionChanged += OnGuitarPartChanged;
            this.ampChannelComboBox = mw.ampChannelComboBox;
            this.ampChannelComboBox.SelectionChanged += OnAmpChannelChanged;
            this.ampSettingsComboBox = mw.ampSettingsComboBox;
            this.ampSettingsComboBox.SelectionChanged += OnAmpSettingsChanged;
            this.playButton = mw.playSampleBtn;
            this.playButton.Click += OnPlayButtonClicked;

            UpdateAmpImage();
        }

        private void OnGuitarPartChanged(object sender, SelectionChangedEventArgs args)
        {
            switch (guitarPartComboBox.SelectedIndex)
            {
                case 0:
                    path1 = "melody";
                    break;
                case 1:
                    path1 = "chords";
                    break;
                default:
                    break;
            }
        }

        private void OnAmpChannelChanged(object sender, SelectionChangedEventArgs args)
        {
            switch (ampChannelComboBox.SelectedIndex)
            {
                case 0:
                    path2 = "clean";
                    break;
                case 1:
                    path2 = "rhythm";
                    break;
                case 2:
                    path2 = "lead";
                    break;
                default:
                    break;
            }
            UpdateAmpImage();
        }

        private void OnAmpSettingsChanged(object sender, SelectionChangedEventArgs args)
        {
            switch (ampSettingsComboBox.SelectedIndex)
            {
                case 0:
                    path3 = "1000";
                    break;
                case 1:
                    path3 = "0100";
                    break;
                case 2:
                    path3 = "0010";
                    break;
                case 3:
                    path3 = "555";
                    break;
                default:
                    break;
            }
            UpdateAmpImage();
        }

        private void OnPlayButtonClicked(object sender, EventArgs args)
        {
            if (!isPlaying)
            {
                if (path1 != null & path2 != null & path3 != null)
                {
                    pathToAudioFile = AppDomain.CurrentDomain.BaseDirectory + @"audio_test_files\" + path1 + "-" + path2 + path3 + ".wav";
                    sp = new SoundProcessing();
                    sp.loadVstAudio(this.mw, pathToAudioFile);
                    sp.play();
                    isPlaying = true;
                    playButton.Content = "Stop Playback";
                }
            } else
            {
                sp.stopPlaybackLite();
                isPlaying = false;
            }
        }

        private void UpdateAmpImage()
        {

            var imgsource = "amplifier.jpg";

            if (path2 != null & path3 != null)
            {
                imgsource = path2 + path3 + ".png";
            }

            mw.ampImg.Source = null;

            BitmapImage displayImage = new BitmapImage();
            displayImage.BeginInit();
            displayImage.CacheOption = BitmapCacheOption.None;
            displayImage.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            displayImage.CacheOption = BitmapCacheOption.OnLoad;
            displayImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            displayImage.UriSource = new Uri(@AppDomain.CurrentDomain.BaseDirectory + @"audio_test_files\" + imgsource);
            displayImage.EndInit();
            mw.ampImg.Source = displayImage;
        }

    }
}
