using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using Path = System.IO.Path;

namespace FarManagerWithAsyncAwait
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string CurrentTime
        {
            get { return (string)GetValue(CurrentTimeProperty); }
            set { SetValue(CurrentTimeProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CurrentTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentTimeProperty =
            DependencyProperty.Register("CurrentTime", typeof(string), typeof(MainWindow));

        public string CurrentDate
        {
            get { return (string)GetValue(CurrentDateProperty); }
            set { SetValue(CurrentDateProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CurrentDate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentDateProperty =
            DependencyProperty.Register("CurrentDate", typeof(string), typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            DynamicCurrentTime();
            DynamicCurrentDate();

            cmbFiles1.SelectedIndex = 0;
            cmbFiles2.SelectedIndex = 0;
        }

        private async void DynamicCurrentTime()
        {
            CurrentTime = DateTime.Now.ToString("T");
            await Task.Delay(1000);
            DynamicCurrentTime();
        }

        private async void DynamicCurrentDate()
        {
            CurrentDate = DateTime.Now.ToString("d");
            await Task.Delay(1000);
            DynamicCurrentDate();
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (lbFiles1.SelectedIndex != -1 || lbFiles2.SelectedIndex != -1)
            {

                if (lbFiles1.SelectedIndex != -1)
                {
                    OpenFileOrFolderAsync(lbFiles1, tbFileDirectory1, filesCount1, usedBytes1);
                }
                else if (lbFiles2.SelectedIndex != -1)
                {
                    OpenFileOrFolderAsync(lbFiles2, tbFileDirectory2, filesCount2, usedBytes2);
                }
            }
            else
            {
                MessageBox.Show("No files or folders selected!");
            }

        }
        private async void OpenFileOrFolderAsync(ListBox lbFiles, TextBlock tbFileDirectory, TextBlock filesCount, TextBlock usedBytes)
        {
            try
            {
                if (lbFiles.SelectedItem.GetType() == Type.GetType("System.IO.DirectoryInfo"))
                {
                    if (!tbFileDirectory.Text.EndsWith(@"\"))
                        tbFileDirectory.Text += @"\";

                    string newDirectory = tbFileDirectory.Text + lbFiles.SelectedItem.ToString();
                    tbFileDirectory.Text = newDirectory;

                    await FolderAccessAsync(newDirectory, lbFiles, filesCount, usedBytes);

                }
                else
                {
                    string newFile = tbFileDirectory.Text + @"\" + lbFiles.SelectedItem.ToString();
                    if (newFile.EndsWith(".txt"))
                    {
                        Process.Start(newFile);
                    }
                    else
                    {
                        MessageBox.Show("You cannot open this file!");
                    }
                }
            }
            catch (Exception ex)
            {
                string[] oldDirectory = tbFileDirectory.Text.Split('\\');
                string lastDirectory = oldDirectory.Last();
                var oldDirectoryCommon = String.Join(@"\", oldDirectory.ToArray());
                string newDirectory = oldDirectoryCommon.ToString().Replace(lastDirectory, null);

                if (newDirectory.Count(c => c == '\\') > 1)
                    tbFileDirectory.Text = newDirectory.Remove(newDirectory.Length - 1, 1);
                else
                    tbFileDirectory.Text = newDirectory;
                MessageBox.Show("Faylin acilmaginda problem yarandi!");
                MessageBox.Show(ex.Message);
            }
        }

        private void CmbFiles1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DirectoryInfo folders;
            DriveInfo di;
            if (cmbFiles1.SelectedIndex == 0)
            {
                folders = new DirectoryInfo(@"C:\");
                di = new DriveInfo(@"C:\");
                tbFileDirectory1.Text = @"C:\";
            }
            else
            {
                folders = new DirectoryInfo(@"D:\");
                di = new DriveInfo(@"D:\");
                tbFileDirectory1.Text = @"D:\";
            }

            ArrayList allFilesFolders = new ArrayList();
            allFilesFolders.AddRange(folders.GetDirectories());
            allFilesFolders.AddRange(folders.GetFiles());

            lbFiles1.ItemsSource = allFilesFolders;
            filesCount1.Text = allFilesFolders.Count.ToString();
            usedBytes1.Text = (di.TotalSize - di.TotalFreeSpace).ToString();
        }

        private void CmbFiles2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DirectoryInfo folders;
            DriveInfo di;
            if (cmbFiles2.SelectedIndex == 0)
            {
                folders = new DirectoryInfo(@"C:\");
                di = new DriveInfo(@"C:\");
                tbFileDirectory2.Text = @"C:\";
            }
            else
            {
                folders = new DirectoryInfo(@"D:\");
                di = new DriveInfo(@"D:\");
                tbFileDirectory2.Text = @"D:\";
            }

            ArrayList allFilesFolders = new ArrayList();
            allFilesFolders.AddRange(folders.GetDirectories());
            allFilesFolders.AddRange(folders.GetFiles());

            lbFiles2.ItemsSource = allFilesFolders;
            filesCount2.Text = allFilesFolders.Count.ToString();
            usedBytes2.Text = (di.TotalSize - di.TotalFreeSpace).ToString();
        }

        private void TbFileFirectory_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var tbFileFactory = sender as TextBlock;
            double desiredHeight = 30;
            if (tbFileFactory.ActualHeight > desiredHeight)
            {
                double fontsizeMultiplier = Math.Sqrt(desiredHeight / tbFileFactory.ActualHeight);
                tbFileFactory.FontSize = Math.Floor(tbFileFactory.FontSize * fontsizeMultiplier);
            }
            else
            {
                tbFileFactory.FontSize = 18;
            }
            tbFileFactory.Height = desiredHeight;
        }

        private async Task FolderAccessAsync(string name, ListBox lbFiles, TextBlock filesCount, TextBlock usedBytes)
        {
            DirectoryInfo folders = new DirectoryInfo(name);
            DriveInfo di = new DriveInfo(name);

            ArrayList allFilesFolders = new ArrayList();
            allFilesFolders.AddRange(folders.GetDirectories());
            allFilesFolders.AddRange(folders.GetFiles());

            lbFiles.ItemsSource = allFilesFolders;

            long sizeFolders = 0;
            try
            {
                sizeFolders = await Task.Run(() => folders.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length));
            }
            catch (UnauthorizedAccessException) { }

            filesCount.Text = allFilesFolders.Count.ToString();
            usedBytes.Text = sizeFolders.ToString();
        }

        private void LbFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            var lbFiles = sender as ListBox;

            if (lbFiles.SelectedIndex != -1)
            {
                if (lbFiles.Name == "lbFiles1")
                    OpenFileOrFolderAsync(lbFiles1, tbFileDirectory1, filesCount1, usedBytes1);
                else
                    OpenFileOrFolderAsync(lbFiles2, tbFileDirectory2, filesCount2, usedBytes2);
            }
        }

        private void BtnBackFiles_Click(object sender, RoutedEventArgs e)
        {
            var btnBackFiles = sender as Button;
            if (btnBackFiles.Name == "btnBackFiles1")
                BackFilesMenu(tbFileDirectory1, lbFiles1, filesCount1, usedBytes1);
            else
                BackFilesMenu(tbFileDirectory2, lbFiles2, filesCount2, usedBytes2);

        }

        private void BackFilesMenu(TextBlock tbFileFirectory, ListBox lbFiles, TextBlock filesCount, TextBlock usedBytes)
        {
            if (tbFileFirectory.Text.Last() != '\\')
            {
                string[] oldDirectory = tbFileFirectory.Text.Split('\\');
                string lastDirectory = oldDirectory.Last();
                var oldDirectoryCommon = String.Join(@"\", oldDirectory.ToArray());
                string newDirectory = oldDirectoryCommon.ToString().Replace(lastDirectory, null);

                if (newDirectory.Count(c => c == '\\') > 1)
                    tbFileFirectory.Text = newDirectory.Remove(newDirectory.Length - 1, 1);
                else
                    tbFileFirectory.Text = newDirectory;
                _ = FolderAccessAsync(tbFileFirectory.Text, lbFiles, filesCount, usedBytes);
            }
            else
                SystemSounds.Hand.Play();
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbFiles1.SelectedIndex != -1 || lbFiles2.SelectedIndex != -1)
            {
                if (lbFiles1.SelectedIndex != -1)
                {
                    if (lbFiles1.SelectedItem.GetType() == Type.GetType("System.IO.DirectoryInfo"))
                    {
                        var dir = lbFiles1.SelectedItem as DirectoryInfo;
                        await Task.Delay(1000);
                        dir.Delete(true);
                    }
                    else
                    {
                        var file = lbFiles1.SelectedItem as FileInfo;
                        file.Delete();
                    }
                    await FolderAccessAsync(tbFileDirectory1.Text, lbFiles1, filesCount1, usedBytes1);
                    MessageBox.Show("Success deleted!");

                }
                else if (lbFiles2.SelectedIndex != -1)
                {
                    if (lbFiles2.SelectedItem.GetType() == Type.GetType("System.IO.DirectoryInfo"))
                    {
                        var dir = lbFiles2.SelectedItem as DirectoryInfo;
                        dir.Delete(true);
                    }
                    else
                    {
                        var file = lbFiles2.SelectedItem as FileInfo;
                        file.Delete();
                    }
                    await FolderAccessAsync(tbFileDirectory2.Text, lbFiles2, filesCount2, usedBytes2);
                    MessageBox.Show("Success deleted!");
                }
            }
            else
            {
                MessageBox.Show("No files or folders selected!");
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lbFiles1.SelectedIndex != -1 || lbFiles2.SelectedIndex != -1)
            {
                if (lbFiles1.SelectedIndex != -1)
                {
                    string txtFile = tbFileDirectory1.Text + @"\" + lbFiles1.SelectedItem.ToString();
                    if (txtFile.EndsWith(".txt"))
                    {
                        var process = Process.Start(txtFile);
                        process.WaitForExit();
                        _ = FolderAccessAsync(tbFileDirectory1.Text, lbFiles1, filesCount1, usedBytes1);
                        MessageBox.Show("Edited succesfully!");
                    }
                    else
                    {
                        MessageBox.Show("You cannot edit this file!");
                    }
                }
                else if (lbFiles2.SelectedIndex != -1)
                {
                    string txtFile = tbFileDirectory2.Text + @"\" + lbFiles2.SelectedItem.ToString();
                    if (txtFile.EndsWith(".txt"))
                    {
                        var process = Process.Start(txtFile);
                        process.WaitForExit();
                        _ = FolderAccessAsync(tbFileDirectory2.Text, lbFiles2, filesCount2, usedBytes2);
                        MessageBox.Show("Edited succesfully!");
                    }
                    else
                    {
                        MessageBox.Show("You cannot edit this file!");
                    }
                }
            }
            else
            {
                MessageBox.Show("No .txt files selected!");
            }
        }

        private void LbFiles1_SelectionChanged(object sender, SelectionChangedEventArgs e) => lbFiles2.UnselectAll();

        private void LbFiles2_SelectionChanged(object sender, SelectionChangedEventArgs e) => lbFiles1.UnselectAll();

        private void BtnProporties_Click(object sender, RoutedEventArgs e)
        {
            if (lbFiles1.SelectedIndex != -1 || lbFiles2.SelectedIndex != -1)
            {
                if (lbFiles1.SelectedIndex != -1)
                {
                    ViewProportiesAsync(lbFiles1);
                }
                else if (lbFiles2.SelectedIndex != -1)
                {
                    ViewProportiesAsync(lbFiles2);
                }
            }
            else
            {
                MessageBox.Show("No files or folders selected!");
            }
        }

        private async void ViewProportiesAsync(ListBox lbFiles)
        {
            if (lbFiles.SelectedItem.GetType() == Type.GetType("System.IO.DirectoryInfo"))
            {
                var dir = lbFiles.SelectedItem as DirectoryInfo;
                decimal sizeFolders = 0;
                string sizeFoldersFormat = "";
                long countFiles = 0;
                long countFolders = 0;
                try
                {
                    sizeFolders = await Task.Run(() => dir.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length));
                    countFiles = await Task.Run(() => dir.EnumerateFiles("*", SearchOption.AllDirectories).Count());
                    countFolders = await Task.Run(() => dir.EnumerateDirectories("*", SearchOption.AllDirectories).Count());
                }
                catch (UnauthorizedAccessException) { }

                if (sizeFolders >= 1073741824) // GB
                {
                    sizeFolders /= 1073741824;
                    sizeFoldersFormat = sizeFolders.ToString("0.00") + " GB";
                }
                else if (sizeFolders < 1073741824 && sizeFolders >= 1048576) // MB
                {
                    sizeFolders /= 1048576;
                    sizeFoldersFormat = sizeFolders.ToString("0.00") + " MB";
                }
                else if (sizeFolders < 1048576 && sizeFolders >= 1024) // KB
                {
                    sizeFolders /= 1024;
                    sizeFoldersFormat = sizeFolders.ToString("0.00") + " KB";
                }
                else if (sizeFolders < 1024) // bytes
                {
                    sizeFoldersFormat = sizeFolders.ToString("0.00") + " bytes";
                }

                string proporties = $@"
Folder name:   {dir.Name}
Type:               File folder
Location:         {dir.Root.FullName}
Size:                {sizeFoldersFormat} ({(double)sizeFolders} bytes) 
Contains:        {countFiles} Files, {countFolders} Folders
Created:          {dir.CreationTime.ToString("dddd, dd MMMM yyyy HH:mm:ss")}";
                MessageBox.Show(proporties, dir.Name + " Proporties", MessageBoxButton.OK);
            }
            else
            {
                var file = lbFiles.SelectedItem as FileInfo;
                decimal sizeFile = file.Length;
                string sizeFileFormat = "";

                if (sizeFile >= 1073741824) // GB
                {
                    sizeFile /= 1073741824;
                    sizeFileFormat = sizeFile.ToString("0.00") + " GB";
                }
                else if (sizeFile < 1073741824 && sizeFile >= 1048576) // MB
                {
                    sizeFile /= 1048576;
                    sizeFileFormat = sizeFile.ToString("0.00") + " MB";
                }
                else if (sizeFile < 1048576 && sizeFile >= 1024) // KB
                {
                    sizeFile /= 1024;
                    sizeFileFormat = sizeFile.ToString("0.00") + " KB";
                }
                else if (sizeFile < 1024) // bytes
                {
                    sizeFileFormat = sizeFile.ToString("0.00") + " bytes";
                }

                string proporties = $@"
File name:       {file.Name}
Type of file:      {file.Extension}
Location:         {System.IO.Path.GetDirectoryName(file.FullName)}
Size:                {sizeFileFormat} ({(double)sizeFile} bytes) 
Created:          {file.CreationTime.ToString("dddd, dd MMMM yyyy HH:mm:ss")}
Modified:         {file.LastWriteTime.ToString("dddd, dd MMMM yyyy HH:mm:ss")}
Accessed:         {file.LastAccessTime.ToString("dddd, dd MMMM yyyy HH:mm:ss")}";
                MessageBox.Show(proporties, file.Name + " Proporties", MessageBoxButton.OK);
            }
        }
        public async Task CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            else
                Directory.CreateDirectory(destFolder + " - Copy");

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);

                CopyFolder(folder, dest);
            }
        }
        private async void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (lbFiles1.SelectedIndex != -1 || lbFiles2.SelectedIndex != -1)
            {
                if (lbFiles1.SelectedIndex != -1)
                {
                    if (lbFiles1.SelectedItem.GetType() == Type.GetType("System.IO.DirectoryInfo"))
                    {
                        string sourceFolder = tbFileDirectory1.Text + @"\" + lbFiles1.SelectedItem.ToString();
                        string destFolder = tbFileDirectory2.Text + @"\" + lbFiles1.SelectedItem.ToString();
                        await CopyFolder(sourceFolder, destFolder);
                    }
                    else
                    {
                        await CopyFileAsync(tbFileDirectory1.Text + @"\" + lbFiles1.SelectedItem.ToString(), tbFileDirectory2.Text + @"\" + lbFiles1.SelectedItem.ToString(), tbFileDirectory1.Text);
                        await Task.Delay(2000);
                    }
                    await FolderAccessAsync(tbFileDirectory2.Text, lbFiles2, filesCount2, usedBytes2);
                    MessageBox.Show("Copied successfully!");
                }
                else if (lbFiles2.SelectedIndex != -1)
                {
                    if (lbFiles2.SelectedItem.GetType() == Type.GetType("System.IO.DirectoryInfo"))
                    {
                        string sourceFolder = tbFileDirectory2.Text + @"\" + lbFiles2.SelectedItem.ToString();
                        string destFolder = tbFileDirectory1.Text + @"\" + lbFiles2.SelectedItem.ToString();
                        await CopyFolder(sourceFolder, destFolder);
                    }
                    else
                    {
                        await CopyFileAsync(tbFileDirectory2.Text + @"\" + lbFiles2.SelectedItem.ToString(), tbFileDirectory1.Text + @"\" + lbFiles2.SelectedItem.ToString(), tbFileDirectory2.Text);
                        await Task.Delay(2000);
                    }
                    await FolderAccessAsync(tbFileDirectory1.Text, lbFiles1, filesCount1, usedBytes1);
                    MessageBox.Show("Copied successfully!");
                }
            }
            else
            {
                MessageBox.Show("No files or folders selected!");
            }
        }

        public async Task CopyFileAsync(string sourceFileFullname, string destinationFile, string sourceFile)
        {
            var destFile = destinationFile.Split('\\').Last();
            DirectoryInfo directoryInfo = new DirectoryInfo(sourceFile);
            if (directoryInfo.EnumerateFiles().ToList().Exists(f => f.Name == destFile))
                File.Delete(destinationFile);

            using (var sourceStream = new FileStream(sourceFileFullname, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var destinationStream = new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                await sourceStream.CopyToAsync(destinationStream);


        }

        private async void BtnMove_Click(object sender, RoutedEventArgs e)
        {
            if (lbFiles1.SelectedIndex != -1 || lbFiles2.SelectedIndex != -1)
            {
                if (lbFiles1.SelectedIndex != -1)
                {
                    DirectoryInfo dir;
                    string sourceFolder = tbFileDirectory1.Text + @"\" + lbFiles1.SelectedItem.ToString();
                    string destFolder = tbFileDirectory2.Text + @"\" + lbFiles1.SelectedItem.ToString();
                    if (lbFiles1.SelectedItem.GetType() == Type.GetType("System.IO.DirectoryInfo"))
                    {
                        dir = new DirectoryInfo(sourceFolder);
                        await Task.Delay(2000);
                        dir.MoveTo(destFolder);
                    }
                    else
                    {
                        FileInfo fileInfo = new FileInfo(sourceFolder);
                        await Task.Delay(2000);
                        fileInfo.MoveTo(destFolder);
                    }
                    await FolderAccessAsync(tbFileDirectory2.Text, lbFiles2, filesCount2, usedBytes2);
                    await FolderAccessAsync(tbFileDirectory1.Text, lbFiles1, filesCount1, usedBytes1);
                    MessageBox.Show("Movied successfully!");
                }
                else if (lbFiles2.SelectedIndex != -1)
                {
                    DirectoryInfo dir;
                    string sourceFolder = tbFileDirectory2.Text + @"\" + lbFiles2.SelectedItem.ToString();
                    string destFolder = tbFileDirectory1.Text + @"\" + lbFiles2.SelectedItem.ToString();
                    if (lbFiles2.SelectedItem.GetType() == Type.GetType("System.IO.DirectoryInfo"))
                    {
                        dir = new DirectoryInfo(sourceFolder);
                        await Task.Delay(2000);
                        dir.MoveTo(destFolder);
                    }
                    else
                    {
                        FileInfo fileInfo = new FileInfo(sourceFolder);
                        await Task.Delay(2000);
                        fileInfo.MoveTo(destFolder);
                    }
                    await FolderAccessAsync(tbFileDirectory1.Text, lbFiles1, filesCount1, usedBytes1);
                    await FolderAccessAsync(tbFileDirectory2.Text, lbFiles2, filesCount2, usedBytes2);
                    MessageBox.Show("Moved successfully!");
                }
            }
            else
            {
                MessageBox.Show("No files or folders selected!");
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);
    }
}
