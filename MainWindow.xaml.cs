using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.IO;
using System.Text.Json.Nodes;
using System.Diagnostics;


namespace SkinInjector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Boolean isPersonaSelected = false;
        private Boolean isInjectSkinPackSelected = false;

        public MainWindow()
        {
            InitializeComponent();

            PackListView.ItemsSource = GetAllSkinPackData();

        }

        private void Choose_PersonaFolder(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new()
            {
                Description = "personaフォルダを選択してください"
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.personaPath.Text = dialog.SelectedPath;
                isPersonaSelected = true;
                if (isPersonaSelected && isInjectSkinPackSelected)
                {
                    inject.IsEnabled = true;
                }
            }
        }

        private List<PackInfo> GetAllSkinPackData()
        {
            string targetPath = Environment.ExpandEnvironmentVariables(this.minecraftPath.Text.ToString());

            var packList = new List<PackInfo>();

            var subFolders = Directory.GetDirectories(targetPath);

            foreach (var subFolder in subFolders)
            {
                string manifestPath = System.IO.Path.Combine(subFolder, "manifest.json");

                if (File.Exists(manifestPath))
                {
                    try
                    {
                        string content = File.ReadAllText(manifestPath);
                        var json = JsonObject.Parse(content);
                        string? packName = json["header"]?["name"]?.ToString();

                        packList.Add(new PackInfo
                        {
                            FolderPath = subFolder,
                            PackName = packName
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"エラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                        Debug.WriteLine(ex.ToString());
                    }
                }
            }

            return packList;
        }

        private void InjectClick(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(personaPath.Text))
            {

                List<PackInfo> PackList = GetAllSkinPackData();

                string currentDirectory = Directory.GetCurrentDirectory();
                string folderName = "persona";

                string copyPath = System.IO.Path.Combine(currentDirectory, folderName);
                
                DirectoryInfo directory = new(copyPath);
                

                string sourcePath = personaPath.Text;
                string? targetPath = PackList[PackListView.SelectedIndex].FolderPath;
                

                if (Directory.Exists(copyPath))
                {
                    directory.Delete(true);
                    
                }

                Directory.CreateDirectory(copyPath);

                foreach (var filePath in Directory.GetFiles(sourcePath))
                {
                    string fileName = System.IO.Path.GetFileName(filePath);
                    if (fileName != "manifest.json")
                    {
                        string destFilePath = System.IO.Path.Combine(copyPath, fileName);
                        File.Copy(filePath, destFilePath);
                    }
                }

                if (targetPath != null)
                {
                    foreach (var filePath in Directory.GetFiles(targetPath))
                    {
                        string fileName = System.IO.Path.GetFileName(filePath);
                        if (fileName != "manifest.json")
                        {
                            File.Delete(filePath);
                        }
                    }

                    foreach (var subDir in Directory.GetDirectories(targetPath))
                    {
                        Directory.Delete(subDir, true);
                    }

                    foreach (var filePath in Directory.GetFiles(copyPath))
                    {
                        string fileName = System.IO.Path.GetFileName(filePath);
                        string destFilePath = System.IO.Path.Combine(targetPath, fileName);
                        File.Copy(filePath, destFilePath);
                    }

                    Process encript = new();
                    string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                    encript.StartInfo.FileName = System.IO.Path.Combine(currentDir, "MCEnc", "McEncryptor.exe");

                    encript.StartInfo.UseShellExecute = false;
                    encript.StartInfo.RedirectStandardInput = true;
                    encript.StartInfo.CreateNoWindow = true;

                    encript.Start();
                    encript.StandardInput.WriteLine(targetPath);
                    encript.StandardInput.Close();

                    encript.WaitForExit();

                    System.Windows.MessageBox.Show($"処理に成功しました。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                
            } else
            {
                System.Windows.MessageBox.Show($"パスが無効です。確認してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PackListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isInjectSkinPackSelected = true;
            if (isPersonaSelected && isInjectSkinPackSelected)
            {
                inject.IsEnabled = true;
            }
        }
    }

    public class PackInfo
    {
        public string? FolderPath { get; set; }
        public string? PackName { get; set; }
    }

}