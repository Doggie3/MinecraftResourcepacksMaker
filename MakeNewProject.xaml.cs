using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MinecraftResourcepacksMaker
{
    /// <summary>
    /// MakeNewProject.xaml 的交互逻辑
    /// </summary>
    public partial class MakeNewProject : Window
    {
        public string ProjectLocation { get; set; }
        public string ProjectDesciprion { get; set; }
        public string ProjectVersion { get; set; }
        public string ProjectCover { get; set; }

        public MakeNewProject()
        {
            InitializeComponent();
            InitializeVersion();
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            ProjectLocation = SelectFolder("选择材质包位置");
            projectlocation.Text = ProjectLocation;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (ProjectLocation == null)
            {
                MessageBox.Show("没有选择项目位置");
            }
            if (version.SelectedIndex == -1)
            {
                MessageBox.Show("没有选择项目版本");
            }
            if (version.SelectedIndex == -1 || ProjectLocation == null)
            {
                return;
            }
            ProjectDesciprion = description.Text;

            string v="-1";

            List<string> ver = new List<string>();
            StreamReader reader = new StreamReader("versionIndex.txt");
            string line;
            v = ((ComboBoxItem)version.SelectedItem).Tag.ToString();


            ProjectVersion = v;

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.ProjectLocation = ProjectLocation;
            mainWindow.ProjectDesciprion = ProjectDesciprion;
            mainWindow.ProjectVersion = ProjectVersion;
            mainWindow.ProjectCover = ProjectCover;
            mainWindow.NewProject();
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CoverSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
                ProjectCover = SelectFile("选择封面图片", "Image files (*.png)|*.png");
                if (ProjectCover != null)
                {
                    CoverPreview.Source = MainWindow.GetImage(ProjectCover);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void InitializeVersion()
        {
            List<string> ver = new List<string>();
            StreamReader reader = new StreamReader("versionIndex.txt");
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                ver.Add(line);
            }
            foreach(string v in ver)
            {
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\data\\" + v))
                {
                    foreach(var item in version.Items)
                    {
                        if(item is ComboBoxItem comboBoxItem)
                        {
                            if (comboBoxItem.Content.ToString().StartsWith(v))
                            {
                                comboBoxItem.IsEnabled = true;
                                comboBoxItem.Tag = v;
                            }
                        }
                    }
                }
            }
        }

        public string SelectFile(string title, string filter)
        {
            string selectedFilePath = "null";
            Microsoft.Win32.OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter
            };
            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
            }
            return selectedFilePath;
        }
        public string SelectFolder(string Description)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = Description;
            CommonFileDialogResult result = dialog.ShowDialog();

            if (result == CommonFileDialogResult.Ok)
            {
                var folderPath = dialog.FileName;
                Console.WriteLine("选择的文件夹路径：" + folderPath);
                return folderPath;
            }
            return "null";
        }
    }
}
