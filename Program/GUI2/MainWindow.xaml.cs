using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Net.Cache;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace GUI2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private SoundProcessing sp;
        //private LoadVst lv = new LoadVst();
        private CompareResults cr;
        public string pythonpath = "";
        private PythonScripts ps;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            PythonPath pp = new PythonPath();
            pp.firstLoad();
            this.pythonpath = pp.path;
            ps = new PythonScripts(this, pythonpath);
            vstLaunchBtn.Visibility = Visibility.Hidden;
            analyseTab.IsEnabled = false;
            resultTab.IsEnabled = false;
            audioSlider.IsEnabled = false;
            var bitmap = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\waveform.png"));
            waveformImg.Source = bitmap;
            cr = new CompareResults(this);
            sp = new SoundProcessing(ps);
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            sp.stopPlayback();

            loadFileProgress.Value = 0;
            loadFileProgress.Visibility = Visibility.Visible;

            string path = "No file selected";
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Audio Files|*.wav;*.aac;*.wma;*.mp3;*.WAV;*.AAC;*.WMA;*.MP3;";
            file.InitialDirectory = @AppDomain.CurrentDomain.BaseDirectory + "audio_samples\\";

            if (file.ShowDialog() == true)
            {
                path = file.FileName;
                var fileinfo = new FileInfo(path);

                filepathtxt.Content = path;
                filenametxt.Text = fileinfo.Name;
                filenametxt2.Text = fileinfo.Name;
                filenametxt3.Text = fileinfo.Name;
                fileFormatTxt.Content = fileinfo.Extension;
                fileSizeTxt.Content = Math.Round((fileinfo.Length / 1024f) / 1024f, 2) + " Mb";
                loadFileProgress.Value = 33;

                sp.loadAudio(this, path);

                toggleBtn.PreviewMouseLeftButtonDown += OnToggleButtonPressed;
                toggleBtn.PreviewMouseLeftButtonUp += OnToggleButtonReleased;

                resultxt.Text = "Classifier has not run yet.";
            }
        }


        /*private void Button2_Click(object sender, RoutedEventArgs e)
        {
            string path = (string) filepathtxt.Content;
            if (path != "No file selected")
            {
                ps = new PythonScripts(this);
            }
        }*/



        private void OnToggleButtonPressed(object sender, RoutedEventArgs e)
        {
            graphPreview.Source = null;
            BitmapImage displayImage = new BitmapImage();
            displayImage.BeginInit();
            displayImage.CacheOption = BitmapCacheOption.None;
            displayImage.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            displayImage.CacheOption = BitmapCacheOption.OnLoad;
            displayImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            displayImage.UriSource = new Uri(@AppDomain.CurrentDomain.BaseDirectory + "\\temp-classify.png");
            displayImage.EndInit();
            graphPreview.Source = displayImage;
        }

        private void OnToggleButtonReleased(object sender, RoutedEventArgs e)
        {
            graphPreview.Source = null;
            BitmapImage displayImage = new BitmapImage();
            displayImage.BeginInit();
            displayImage.CacheOption = BitmapCacheOption.None;
            displayImage.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
            displayImage.CacheOption = BitmapCacheOption.OnLoad;
            displayImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            displayImage.UriSource = new Uri(@AppDomain.CurrentDomain.BaseDirectory + "\\temp-display.png");
            displayImage.EndInit();
            graphPreview.Source = displayImage;
        }

        private void vstLaunchBtn_Click(object sender, RoutedEventArgs e)
        {
            /*var vstWindow = new Window();

            if (lv.PluginContext.PluginCommandStub.Commands.EditorGetRect(out Rectangle wndRect))
            {
                vstWindow.Width = wndRect.Width + 15;
                vstWindow.Height = wndRect.Height + 40;   
            }

            vstWindow.Title = lv.PluginContext.PluginCommandStub.Commands.GetEffectName();
            
            var icon = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "icon.png"));
            vstWindow.Icon = icon;
            vstWindow.Show();

            var vstWindowHandle = new WindowInteropHelper(vstWindow).Handle;
            lv.PluginContext.PluginCommandStub.Commands.EditorOpen(vstWindowHandle);*/

        }

    }
}
