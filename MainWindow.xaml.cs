using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace MinecraftResourcepacksMaker
{
    public class PackMcmeta
    {
        [JsonPropertyName("pack")] // 显式指定 JSON 字段名，避免命名规范冲突
        public PackageInfo Pack { get; set; }
        // 将 RelayCommand 构造函数调用中的 null 显式转换为 Func<object, bool>
    }
    public class PackageInfo
    {
        /// <summary>
        /// 资源包描述
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("pack_format")]
        public int pack_format { get; set; }

        /// <summary>
        /// 最小格式版本
        /// </summary>
        [JsonPropertyName("min_format")]
        public int MinFormat { get; set; }

        /// <summary>
        /// 最大格式版本
        /// </summary>
        [JsonPropertyName("max_format")]
        public int MaxFormat { get; set; }
    }
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public string ProjectLocation { get; set; }
        public string ProjectDesciprion { get; set; }
        public string ProjectVersion { get; set; }
        public string ProjectCover { get; set; }
        public string originalData = "\\data\\{version}\\assets\\minecraft\\textures";
        public string selectedFile = "";
        public int selectedType = 0;//0:Block 1:Item 2:Entity 3:Gui 4:Map 5:Mob_effect 6:Painting 7:Particle
        public MainWindow()
        {
            InitializeComponent();
        }

        public string SelectFile(string title, string filter)
        {
            string selectedFilePath = "null";
            Microsoft.Win32.OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = title;
            openFileDialog.Filter = filter;
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
        public static BitmapImage GetImage(string imagePath)
        {
            BitmapImage bitmap = new BitmapImage();
            if (File.Exists(imagePath))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                using (Stream ms = new MemoryStream(File.ReadAllBytes(imagePath)))
                {
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();  // 在这里释放资源  
                }
            }
            return bitmap;
        }

        private void newProject_Click(object sender, RoutedEventArgs e)
        {
            MakeNewProject form2 = new MakeNewProject();
            form2.ShowDialog();
        }
        private void openProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                progressBar.Value = 0;
                string projectFileLocation = SelectFile("选择材质包", "资源包识别文件 (pack.mcmeta)|pack.mcmeta");
                if (projectFileLocation == null)
                {
                    return;
                }
                PackMcmeta packMcmeta = new PackMcmeta();
                packMcmeta = JsonSerializer.Deserialize<PackMcmeta>(File.ReadAllText(projectFileLocation));
                ProjectLocation = projectFileLocation.Remove(projectFileLocation.Length - 12);
                ProjectDesciprion = packMcmeta.Pack.Description;
                ProjectVersion = packMcmeta.Pack.pack_format.ToString();
                this.Title = ProjectDesciprion + "@" + ProjectVersion;
                originalData = "\\data\\" + ProjectVersion.ToString() + "\\assets\\minecraft\\textures";
                progressBar.Value = 20;
                InitializeTextureList();
                progressBar.Value = 50;
                InitializeProjectInfo();
                progressBar.Value = 100;
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开失败：" + ex.Message);
                progressBar.Value = 100;
            }
        }
        public void NewProject()
        {
            progressBar.Value = 0;
            this.Title = ProjectDesciprion + "@" + ProjectVersion;
            var PackMcmeta = new PackMcmeta
            {
                Pack = new PackageInfo
                {
                    Description = ProjectDesciprion,
                    pack_format = int.Parse(ProjectVersion),
                    MaxFormat = int.Parse(ProjectVersion),
                    MinFormat = int.Parse(ProjectVersion)
                }
            };
            string jsonString = JsonSerializer.Serialize(PackMcmeta, new JsonSerializerOptions
            {
                WriteIndented = true // 格式化输出
            });
            File.CreateText(ProjectLocation + "\\pack.mcmeta").Close();
            File.WriteAllText(ProjectLocation + "\\pack.mcmeta", jsonString);
            originalData = "\\data\\" + ProjectVersion.ToString() + "\\assets\\minecraft\\textures";
            if (File.Exists(ProjectCover))
            {
                File.Copy(ProjectCover, ProjectLocation + "\\pack.png", true);

            }
            progressBar.Value = 20;
            InitializeFolderStructure();
            progressBar.Value = 50;
            InitializeTextureList();
            progressBar.Value = 90;
            InitializeProjectInfo();
            progressBar.Value = 100;
        }
        public void InitializeFolderStructure()
        {
            Directory.CreateDirectory(ProjectLocation + "\\assets\\minecraft\\textures\\block\\");
            Directory.CreateDirectory(ProjectLocation + "\\assets\\minecraft\\textures\\item\\");
            Directory.CreateDirectory(ProjectLocation + "\\assets\\minecraft\\textures\\entity\\");
            Directory.CreateDirectory(ProjectLocation + "\\assets\\minecraft\\textures\\gui\\");
            Directory.CreateDirectory(ProjectLocation + "\\assets\\minecraft\\textures\\map\\");
            Directory.CreateDirectory(ProjectLocation + "\\assets\\minecraft\\textures\\mob_effect\\");
            Directory.CreateDirectory(ProjectLocation + "\\assets\\minecraft\\textures\\painting\\");
            Directory.CreateDirectory(ProjectLocation + "\\assets\\minecraft\\textures\\particle\\");

        }

        public void InitializeProjectInfo()
        {
            editMcmeta.IsEnabled = true;
            export.IsEnabled = true;
            projectName.Text = ProjectDesciprion;
        }

        public void InitializeTextureList()
        {
            BlockTextureList();
            EntityTextureList();
            UpdateBlockSelection();
            UpdateEntitySelection();
        }


        public void BlockTextureList()
        {
            blockTextureSelector.ItemsSource = new List<string>();
            // 1. 初始化数据源
            ObservableCollection<BlockListItem> _listItems = new ObservableCollection<BlockListItem>();

            // 2. 将数据源绑定到ListView的ItemsSource属性
            // 绑定后，ListView会自动显示集合中的所有项
            blockTextureSelector.ItemsSource = _listItems;

            string blockTextureFolder = AppDomain.CurrentDomain.BaseDirectory + originalData + "\\block";

            DirectoryInfo directory = new DirectoryInfo(blockTextureFolder);
            FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);

            foreach (FileInfo file in files)
            {
                if(Path.GetExtension(file.FullName) != ".png")
                {
                    continue;
                }
                var newItem = new BlockListItem
                {
                    // 设置显示文本：拼接编号（集合当前数量+1）
                    DisplayText = $"{file.Name}",
                    // 绑定按钮点击命令：关联到OnItemButtonClick方法
                    // RelayCommand封装了按钮点击的具体逻辑
                    // 将此处的 RelayCommand 构造函数调用显式传递 null 作为第二个参数，消除二义性
                    ButtonClickCommand = new RelayCommand(OnBlockButtonClick, (Func<object, bool>)null)
                };
                _listItems.Insert(0, newItem);
            }
        }
        private void OnBlockButtonClick(object parameter)
        {
            // 将参数转换为字符串（防止类型不匹配）
            string itemText = parameter as string;
            string blockTextureFolder = AppDomain.CurrentDomain.BaseDirectory + originalData + "\\block";
            if (File.Exists(ProjectLocation + "\\assets\\minecraft\\textures\\block\\" + itemText))
            {
                MessageBox.Show("已经存在" + itemText + "\n无法再次添加");
                return;
            }
            File.Copy(blockTextureFolder + "\\" + itemText, ProjectLocation + "\\assets\\minecraft\\textures\\block\\" + itemText);
            UpdateBlockSelection();
        }
        public void EntityTextureList()
        {
            entityTextureSelector.ItemsSource = new List<string>();
            ObservableCollection<EntityListItem> _listItems = new ObservableCollection<EntityListItem>();
            entityTextureSelector.ItemsSource = _listItems;
            string entityTextureFolder = AppDomain.CurrentDomain.BaseDirectory + originalData + "\\entity";
            DirectoryInfo directory = new DirectoryInfo(entityTextureFolder);
            FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                if (Path.GetExtension(file.FullName) != ".png")
                {
                    continue;
                }
                var newItem = new EntityListItem
                {
                    DisplayText = $"{file.Name}",
                    ButtonClickCommand = new RelayCommand(OnEntityButtonClick, (Func<object, bool>)null),
                    RawPath = file.FullName
                };
                _listItems.Insert(0, newItem);
            }
        }
        public void OnEntityButtonClick(object parameter)
        {
            string itemText = parameter as string;
            string d = ProjectLocation + "\\assets\\minecraft\\textures\\entity\\" + itemText.Remove(0, (AppDomain.CurrentDomain.BaseDirectory + originalData + "\\entity").Length);
            try
            {
                if (!Directory.Exists(d))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(d));
                }
                if (File.Exists(d))
                {
                    MessageBox.Show("已经存在" + itemText + "\n无法再次添加");
                    return;
                }
                File.Copy(itemText, d);
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加失败：" + ex.Message);
            }
            UpdateEntitySelection();
        }

        private void UpdateBlockSelection()
        {
            string blockTextureFolder = ProjectLocation + "\\assets\\minecraft\\textures\\block\\";
            blockTextureSelection.ItemsSource = new List<string>();
            // 1. 初始化数据源
            ObservableCollection<SelectionListItem> _listItems = new ObservableCollection<SelectionListItem>();

            // 2. 将数据源绑定到ListView的ItemsSource属性
            // 绑定后，ListView会自动显示集合中的所有项
            blockTextureSelection.ItemsSource = _listItems;
            DirectoryInfo directory = new DirectoryInfo(blockTextureFolder);
            FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);

            foreach (FileInfo file in files)
            {
                var newItem = new SelectionListItem
                {
                    // 设置显示文本：拼接编号（集合当前数量+1）
                    DisplayText = $"{file.Name}",
                    RawPath = file.FullName
                    // 绑定按钮点击命令：关联到OnItemButtonClick方法
                    // RelayCommand封装了按钮点击的具体逻辑
                };
                _listItems.Insert(0, newItem);
            }
        }
        private void UpdateEntitySelection()
        {
            string entityTextureFolder = ProjectLocation + "\\assets\\minecraft\\textures\\entity\\";
            entityTextureSelection.ItemsSource = new List<string>();
            // 1. 初始化数据源
            ObservableCollection<SelectionListItem> _listItems = new ObservableCollection<SelectionListItem>();
            // 2. 将数据源绑定到ListView的ItemsSource属性
            // 绑定后，ListView会自动显示集合中的所有项
            entityTextureSelection.ItemsSource = _listItems;
            DirectoryInfo directory = new DirectoryInfo(entityTextureFolder);
            FileInfo[] files = directory.GetFiles("*", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                var newItem = new SelectionListItem
                {
                    DisplayText = $"{file.Name}",
                    RawPath = file.FullName
                };
                _listItems.Insert(0, newItem);
            }
        }

        private void BlockSelected(object sender, RoutedEventArgs e)
        {
            if (blockTextureSelection.SelectedItem is SelectionListItem item)
            {
                selectedFile = item.RawPath;
                selectedType = 0;
                SelectedItemPathShower.Text = selectedFile;
                File.Copy(selectedFile, AppDomain.CurrentDomain.BaseDirectory + "temp.png", true);
                preView.Source = GetImage(AppDomain.CurrentDomain.BaseDirectory + "temp.png");
            }
        }
        private void EntitySelected(object sender, RoutedEventArgs e)
        {
            if (entityTextureSelection.SelectedItem is SelectionListItem item)
            {
                selectedFile = item.RawPath;
                selectedType = 2;
                SelectedItemPathShower.Text = selectedFile;
                File.Copy(selectedFile, AppDomain.CurrentDomain.BaseDirectory + "temp.png", true);
                preView.Source = GetImage(AppDomain.CurrentDomain.BaseDirectory + "temp.png");
            }
        }

        private void DeleteSelected(object sender, RoutedEventArgs e)
        {
            switch (selectedType)
            {
                case 0:
                    BlockDelete(sender, e);
                    break;
                case 1:
                    break;
                case 2:
                    EntityDelete(sender, e);
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
            }
        }
        private void BlockDelete(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = selectedFile;
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    MessageBox.Show("已删除文件：" + filePath);
                    UpdateBlockSelection();
                }
                else
                {
                    MessageBox.Show("文件不存在：" + filePath);
                }
                selectedFile = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除失败：" + ex.Message);
            }
        }
        private void EntityDelete(object sender, RoutedEventArgs e)
        {
            try
            {
                string filePath = selectedFile;
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    MessageBox.Show("已删除文件：" + filePath);
                    UpdateEntitySelection();
                }
                else
                {
                    MessageBox.Show("文件不存在：" + filePath);
                }
                selectedFile = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除失败：" + ex.Message);
            }
        }

        private void EditSelected(object sender, RoutedEventArgs e)
        {
            switch (selectedType)
            {
                case 0:
                    BlockEdit(sender, e);
                    break;
                case 1:
                    break;
                case 2:
                    EntityEdit(sender, e);
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
            }
        }
        private void BlockEdit(object sender, RoutedEventArgs e)
        {
            string filePath = selectedFile;
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start("mspaint", filePath);
            }
            else
            {
                MessageBox.Show("文件不存在：" + filePath);
            }
        }
        private void EntityEdit(object sender, RoutedEventArgs e)
        {
            string filePath = selectedFile;
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start("mspaint", filePath);
            }
            else
            {
                MessageBox.Show("文件不存在：" + filePath);
            }
        }

        private void ResetSelected(object sender, RoutedEventArgs e)
        {
            switch (selectedType)
            {
                case 0:
                    BlockReset();
                    break;
                case 1:
                    break;
                case 2:
                    EntityReset();
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
            }
        }
        private void BlockReset()
        {
            string opath = AppDomain.CurrentDomain.BaseDirectory + originalData + "\\block\\" + System.IO.Path.GetFileName(selectedFile);
            //MessageBox.Show(opath);
            try
            {
                File.Delete(selectedFile);
                File.Copy(opath, selectedFile);
                MessageBox.Show("已经重置该材质");
            }
            catch (Exception ex)
            {
                MessageBox.Show("重置失败：" + ex.Message);
            }
        }
        private void EntityReset()
        {
            string opath = AppDomain.CurrentDomain.BaseDirectory + originalData + "\\entity\\" + System.IO.Path.GetFullPath(selectedFile).Remove(0, (ProjectLocation + "\\assets\\minecraft\\textures\\entity\\").Length);
            //MessageBox.Show(opath);
            try
            {
                File.Delete(selectedFile);
                File.Copy(opath, selectedFile);
                MessageBox.Show("已经重置该材质");
            }
            catch (Exception ex)
            {
                MessageBox.Show("重置失败：" + ex.Message);
            }
        }

        private void editMcmeta_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("懒得写界面了，直接打开文件编辑吧 :D");
            System.Diagnostics.Process.Start("notepad.exe", ProjectLocation + "\\pack.mcmeta");
        }
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = ProjectLocation;
            string zipPath = SelectFolder("选择导出位置") + "\\" + ProjectDesciprion + "@" + ProjectVersion + ".zip";
            ZipFile.CreateFromDirectory(folderPath, zipPath, CompressionLevel.Optimal, false);
            MessageBox.Show("导出成功！\n文件路径：" + zipPath);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About form2 = new About();
            form2.ShowDialog();
        }
    }
}
