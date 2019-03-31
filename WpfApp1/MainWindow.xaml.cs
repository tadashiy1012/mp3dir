using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.IO;
using VB = Microsoft.VisualBasic;

namespace Mp3dir
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        static string myReplacer(Match m)
        {
            return VB.Strings.StrConv(m.Value, VB.VbStrConv.Wide);
        }

        private void doWork(IProgress<int> progress, string path) {
            var outdir = path + @"\output";
            var r = new Regex("[\\x00-\\x1f<>:\"/\\\\|?*]" +
                "|^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9]|CLOCK\\$)(\\.|$)" +
                "|[\\. ]$");
            Directory.CreateDirectory(outdir);
            var files = System.IO.Directory.GetFiles(path, "*.mp3", System.IO.SearchOption.TopDirectoryOnly);
            int count = 0;
            foreach (var filePath in files)
            {
                count += 1;
                var tag = TagLib.File.Create(filePath);
                var title = r.Replace(tag.Tag.Title, myReplacer);
                var album = r.Replace(tag.Tag.Album, myReplacer);
                var track = tag.Tag.Track;
                var disc = tag.Tag.Disc;

                var name = disc + "-" + track + "_" + title + ".mp3";
                var dir = outdir + @"\" + album;
                var fullPath = dir + @"\" + name;
                Console.WriteLine(fullPath);
                Directory.CreateDirectory(dir);
                var file = File.ReadAllBytes(filePath);
                File.WriteAllBytes(fullPath, file);
                int percentage = count * 100 / files.Length;
                progress.Report(percentage);
            }
        }

        private void showProgress(int percent) {
            progress1.Value = percent;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            button.IsEnabled = false;
            progress1.Value = 0;
            var p = new Progress<int>(showProgress);
            var path = textBox.Text;
            await Task.Run(() => doWork(p, path));
            progress1.Value = 100;
            button.IsEnabled = true;
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            dlg.Description = "フォルダを選択";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox.Text = dlg.SelectedPath;
            }
        }
    }

}
