using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Diagnostics;
using System.Resources;


namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        string g_FilePath;        
        public static string[] g_pstrDstDisk = { "R", "E", "B" };
        public static string[] g_pstrSelPrinterLan = { "TSPL", "ZPL", "DPL" };
        private readonly ResourceManager resourceManager;

        private void SwitchToEnglish()
        {
            // 切換到英文樣式
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/WpfApp1;component//language/en-us.xaml", UriKind.RelativeOrAbsolute) });
        }
        private void SwitchToChinese()
        {
            // 切換到英文樣式
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("/WpfApp1;component//language/zh-cn.xaml", UriKind.RelativeOrAbsolute) });
        }
        public MainWindow()
        {
            InitializeComponent();
            SwitchToEnglish();
            /* 選項添加 */
            for (int i = 0; i < g_pstrDstDisk.Length; i++)
                diskComboBox.Items.Add(g_pstrDstDisk[i]); // dpi選項添加

            for (int i = 0; i < g_pstrSelPrinterLan.Length; i++)
                LangComboBox.Items.Add(g_pstrSelPrinterLan[i]); // dpi選項添加
        }

        private void loadFileButton_Click(object sender, RoutedEventArgs e)
        {
            //StreamReader sr = new StreamReader;
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document"; // Default file name
            dialog.DefaultExt = ".ttf"; // Default file extension
            dialog.Filter = "字體文件 (.ttf)|*.ttf"; // Filter files by extension
            g_FilePath = "";

            // Show open file dialog box
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                g_FilePath = dialog.FileName;
                labelSelectSrc.Content = g_FilePath;
                labelSelectSrc.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                //labelSelectSrc.Content = "請選擇一個TTF檔案";
                labelSelectSrc.Content = TryFindResource("msgSelectTTF") as string;
                labelSelectSrc.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void dstFileButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            dstPathText.Text = path.SelectedPath;
        }

        private void exportFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (g_FilePath ==null || g_FilePath.Length == 0)
            {
                labelStatus.Content = TryFindResource("msgSrcPathIsNull") as string;
                labelStatus.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }

            if (g_FilePath.Length > 0 && dstPathText.Text.Length > 0 && wFileName.Text.Length > 0)
            {
                byte[] byteGet;
                byte[] byteWrite;
                string fileName;
                int fileSize;

                if (File.Exists(g_FilePath))
                {
                    fileName = System.IO.Path.GetFileName(g_FilePath);
                    byteGet = File.ReadAllBytes(g_FilePath);
                    fileSize = byteGet.Length;
                }
                else
                {
                    labelStatus.Content = TryFindResource("msgCantFindFile") as string;
                    labelStatus.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }

                if (diskComboBox.SelectedIndex == -1)
                {
                    labelStatus.Content = TryFindResource("msgDiskIsNull") as string;
                    labelStatus.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                if (LangComboBox.SelectedIndex == -1)
                {
                    labelStatus.Content = TryFindResource("msgPrinterLangaugeIsNull") as string;
                    labelStatus.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }

                string stringLangHead;
                string stringD2Dst;

                if (LangComboBox.Text == g_pstrSelPrinterLan[0])
                {
                    /* TSPL 指令下載 */
                    if (diskComboBox.Text == g_pstrDstDisk[0])
                        stringLangHead = "DOWNLOAD" + "\"" + wFileName.Text + ".TTF\"" + "," + byteGet.Length + ",";
                    else if (diskComboBox.Text == g_pstrDstDisk[1])
                        stringLangHead = "DOWNLOAD F," + "\"" + wFileName.Text + ".TTF\"" + "," + byteGet.Length + ",";
                    else
                        return;
                }
                else if (LangComboBox.Text == g_pstrSelPrinterLan[1])
                {
                    /* ZPL 指令下載*/
                    if (diskComboBox.Text == g_pstrDstDisk[0])
                        stringD2Dst = "R";
                    else if (diskComboBox.Text == g_pstrDstDisk[1])
                        stringD2Dst = "E";
                    else if (diskComboBox.Text == g_pstrDstDisk[2])
                        stringD2Dst = "B";
                    else
                        return;

                    stringLangHead = "~DY" + stringD2Dst + ":" + wFileName.Text + ",b,T," + byteGet.Length + ",";
                }
                else if (LangComboBox.Text == g_pstrSelPrinterLan[2])
                {
                    string stringFileSize8B = fileSize.ToString("X").PadLeft(8, '0');
                    /* DPL 指令下載*/
                    if (diskComboBox.Text == g_pstrDstDisk[0])
                        stringD2Dst = "D";
                    else if (diskComboBox.Text == g_pstrDstDisk[1])
                        stringD2Dst = "F";
                    else if (diskComboBox.Text == g_pstrDstDisk[2])
                        stringD2Dst = "Y";
                    else
                        return;

                    stringLangHead = "\x2i" + stringD2Dst + "u" + wFileName.Text + "TTFont" + "\xD" + stringFileSize8B;
                }
                else
                {
                    return;
                }

                byte[] CommandDecode = Encoding.ASCII.GetBytes(stringLangHead);
                byteWrite = CommandDecode.Concat(byteGet).ToArray();
                string target = dstPathText.Text + "\\" + wFileName.Text + ".prn";
                File.WriteAllBytes(target, byteWrite);
                labelStatus.Content = TryFindResource("msgExportComplete") as string;
                labelStatus.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                if (g_FilePath.Length == 0)
                {
                    labelStatus.Content = TryFindResource("msgSrcPathIsNull") as string;
                    labelStatus.Foreground = new SolidColorBrush(Colors.Red);
                }
                else if (dstPathText.Text.Length == 0)
                {
                    labelStatus.Content = TryFindResource("msgDstPathIsNull") as string;
                    labelStatus.Foreground = new SolidColorBrush(Colors.Red);
                }
                else if (wFileName.Text.Length == 0)
                {
                    labelStatus.Content = TryFindResource("msgFontNameIsNull") as string;
                    labelStatus.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
        }
        static bool bIsChinese = true;
        private void btnSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (bIsChinese)
            {
                SwitchToEnglish();
                bIsChinese = false;
            }
            else
            {
                SwitchToChinese();
                bIsChinese = true;
            }
        }
    }
}
