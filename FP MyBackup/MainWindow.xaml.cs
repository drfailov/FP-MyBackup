using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace FP_MyBackup
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<Action> actionsList = new ObservableCollection<Action>();
        string originalFolder, copyFolder;
        Thread analyzeThread = null;
        Thread syncThread = null;
        bool cont = false;



        public MainWindow()
        {
            InitializeComponent();
            IniFile MyIni = new IniFile("Settings.ini");
            if (MyIni.KeyExists("source_folder"))
            {
                textbox_source_path.Text = MyIni.Read("source_folder");
            }
            if (MyIni.KeyExists("destination_folder"))
            {
                textbox_destination_path.Text = MyIni.Read("destination_folder");
            }
            actionsGrid.ItemsSource = actionsList;
            Title += "   (v" + new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("yyyy.MM.dd") + ")";
        }

        private void button_select_source_folder_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker dlg = new FolderPicker();
            if (dlg.ShowDialog() == true)
            {
                textbox_source_path.Text = dlg.ResultPath;
                IniFile MyIni = new IniFile("Settings.ini");
                MyIni.Write("source_folder", textbox_source_path.Text);
            }
        }

        private void button_select_destination_folder_Click(object sender, RoutedEventArgs e)
        {

            FolderPicker dlg = new FolderPicker();
            if (dlg.ShowDialog() == true)
            {
                textbox_destination_path.Text = dlg.ResultPath;
                IniFile MyIni = new IniFile("Settings.ini");
                MyIni.Write("destination_folder", textbox_destination_path.Text);
            }
        }
        private bool checkPaths()
        {
            originalFolder = textbox_source_path.Text;
            copyFolder = textbox_destination_path.Text;

            if (textbox_source_path.Text.Length == 0 || textbox_source_path.Text.Equals("Шлях не обрано"))
            {
                actionsList.Add(new Action { Doable = false, ActionIcon = "ic_error.png", Filename = "Шлях до основної папки не обрано. Оберіть шлях до основної папки." });
                return false;
            }
            if (copyFolder.Length == 0 || copyFolder.Equals("Шлях не обрано"))
            {
                actionsList.Add(new Action { Doable = false, ActionIcon = "ic_error.png", Filename = "Шлях до папки копії не обрано. Оберіть шлях до папки копії." });
                return false;
            }
            if (copyFolder.Contains(textbox_source_path.Text) || textbox_source_path.Text.Contains(copyFolder))
            {
                actionsList.Add(new Action { Doable = false, ActionIcon = "ic_error.png", Filename = "Шляхи вкладені, потрібно обрати не вкладені папки." });
                return false;
            }
            if (!Directory.Exists(textbox_source_path.Text))
            {
                actionsList.Add(new Action { Doable = false, ActionIcon = "ic_error.png", Filename = "Шлях до основної папки невірний, такої папки не існує." });
                return false;
            }
            if (!Directory.Exists(copyFolder))
            {
                actionsList.Add(new Action { Doable = false, ActionIcon = "ic_error.png", Filename = "Шлях до папки копії невірний, такої папки не існує." });
                return false;
            }
            return true;
        }

        private void button_analyze_Click(object sender, RoutedEventArgs e)
        {
            actionsList.Clear();
            if (!checkPaths())
                return;
            cont = true;

            addItemToGrid("ic_info.png", "Аналіз запущено...");
            analyzeThread = new Thread(() => runAnalyzeAsync());
            analyzeThread.Start();
        }

        void runAnalyzeAsync()
        {
            try {
                lockButtons();
                DirSearchForRemovingFiles(copyFolder);
                DirSearchForCopyFiles(originalFolder);
                
            }     
            catch(Exception e) {
                addItemToGrid("ic_error.png", "Помилка: " + e.Message );
            }
            finally
            {
                analyzeThread = null;
                unlockButtons();
                addItemToGrid("ic_info.png", "Аналіз завершено.");
                if (!cont)
                    addItemToGrid("ic_info.png", "Перервано користувачем.");
                else if (!hasDoableActions())
                    addItemToGrid("ic_info.png", "Відмінностей не виявлено, синхронізація не потрібна.");
                updateStatusLineCounters();
            }
        }
        private bool hasDoableActions()
        {
            for (int i = 0; i < actionsList.Count; i++)
                if (actionsList[i] != null && actionsList[i].Doable)
                    return true;
            return false;
        }
        void scrollDownGrid()
        {
            if (actionsGrid.Items.Count > 0)
            {
                var border = VisualTreeHelper.GetChild(actionsGrid, 0) as Decorator;
                if (border != null)
                {
                    var scroll = border.Child as ScrollViewer;
                    if (scroll != null) scroll.ScrollToEnd();
                }
            }
        }

        void addItemToGrid(string icon, string message)
        {
            addItemToGrid(
                            /*Selected*/ false,
                            /*Doable*/ false,
                            /*TypeImage*/ "",
                            /*ActionIcon*/ icon,
                            /*Filename*/ message,
                            /*CopyPath*/ "",
                            /*CopySize*/ 0,
                            /*CopyDate*/"",
                            /*MainPath*/ "",
                            /*MainSize*/ 0,
                            /*MainDate*/""
                        );
        }
        void addItemToGrid(string icon, string message, string path)
        {
            addItemToGrid(
                            /*Selected*/ false,
                            /*Doable*/ false,
                            /*TypeImage*/ "",
                            /*ActionIcon*/ icon,
                            /*Filename*/ message,
                            /*CopyPath*/ path,
                            /*CopySize*/ 0,
                            /*CopyDate*/"",
                            /*MainPath*/ "",
                            /*MainSize*/ 0,
                            /*MainDate*/""
                        );
        }
        void addItemToGrid(bool selected, bool doable, 
            string typeImage, string actionImage, string filename, 
            string copyPath, long copySize, String copyDate, 
            string mainPath, long mainSize, string mainDate)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                try
                {
                    actionsList.Add(
                                    new Action
                                    {
                                        Selected = selected,
                                        Doable = doable,
                                        Type = typeImage,
                                        ActionIcon = actionImage,
                                        Filename = filename,
                                        CopyPath = copyPath,
                                        SizeCopy = copySize,
                                        DateCopy = copyDate,
                                        MainPath = mainPath,
                                        DateOriginal = mainDate,
                                        SizeOriginal = mainSize
                                    });
                    scrollDownGrid();
                    updateStatusLineCounters();
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }));
        }

        void refreshGrid()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                CollectionViewSource.GetDefaultView(actionsGrid.ItemsSource).Refresh();
            }));
        }
        void updateStatusLine(String text)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                label_status.Content = text;
            }));
        }
        void updateStatusLineCounters()
        {
            int errors = 0;
            int deleteObjectsTotal = 0;
            int deleteObjectsSelected = 0;
            int folders = 0;
            int files = 0;
            long fileCopySizeTotal = 0;
            long fileCopySizeSelected = 0;
            int total = 0;
            int selected = 0;
            int info = 0;

            for (int i = 0; i < actionsList.Count; i++)
            {
                if (actionsList[i].Doable && actionsList[i].ActionIcon.Equals("ic_delete.png"))
                {
                    deleteObjectsTotal++;
                    if (actionsList[i].Selected)
                        deleteObjectsSelected++;
                }
                if (actionsList[i].Doable && actionsList[i].Type.Equals("ic_folder.png")) folders++;
                if (actionsList[i].Doable && actionsList[i].Type.Equals("ic_file.png"))
                {
                    files++;
                    if (actionsList[i].ActionIcon.Equals("ic_copy.png") || actionsList[i].ActionIcon.Equals("ic_edit.png"))
                    {
                        fileCopySizeTotal += actionsList[i].SizeOriginal;
                        if (actionsList[i].Selected)
                            fileCopySizeSelected += actionsList[i].SizeOriginal;
                    }
                }
                if (actionsList[i].ActionIcon.Equals("ic_error.png")) errors++;
                if (actionsList[i].ActionIcon.Equals("ic_info.png")) info++;
                if (actionsList[i].Doable && actionsList[i].Selected) selected++;
                if (actionsList[i].Doable) total++;
            }
            String prefix = "";
            if (analyzeThread != null)
            {
                prefix = "Триває аналіз...   ";
            }
            else if (syncThread != null)
            {
                prefix = "Триває синхронізація...   ";
            }
            else
                prefix = "Готово.   ";


            updateStatusLine(prefix +
                "Задач: " + selected + "/" + total +
                ",   Видалити: " + deleteObjectsSelected + "/" + deleteObjectsTotal +
                ",   Копіювати: " + ToDataSizeString(fileCopySizeSelected) + "/" + ToDataSizeString(fileCopySizeTotal) +
                ",   Повідомлень: " + info +
                ",   Помилок: " + errors);
        }
        // <summary>
        /// <paramref name="byteCount"/> The original size in bytes ( 8 bits )
        /// <paramref name="notationFormat"/> is supported in the following ways:
        /// [    'B' / 'b' : Binary :   Kilobyte (KB) is 1024 bytes, Megabyte (MB) is 1048576 bytes, etc    ]
        /// [    'I' / 'i' : IEC:       Kibibyte (KiB) is 1024 bytes, Mebibyte (MiB) is 1048576 bytes, etc    ]
        /// [    'D' / 'd' : Decimal :  Kilobyte (KB) is 1000 bytes, Megabyte (MB) is 1000000 bytes, etc    ]
        /// </summary>

        string ToDataSizeString(long byteCount, char notationFormat = 'b')
        {
            char[] supportedFormatChars = { 'b', 'i', 'd' };

            var lowerCaseNotationFormat = char.ToLowerInvariant(notationFormat);

            // Stop shooting holes in my ship!
            if (!supportedFormatChars.Contains(lowerCaseNotationFormat))
            {
                throw new ArgumentException($"notationFormat argument '{notationFormat}' not supported");
            }

            long ebLimit = 1152921504606846976;
            long pbLimit = 1125899906842624;
            long tbLimit = 1099511627776;
            long gbLimit = 1073741824;
            long mbLimit = 1048576;
            long kbLimit = 1024;

            var ebSuffix = "EB";
            var pbSuffix = "PB";
            var tbSuffix = "TB";
            var gbSuffix = "GB";
            var mbSuffix = "MB";
            var kbSuffix = "KB";
            var bSuffix = " B";

            switch (lowerCaseNotationFormat)
            {
                case 'b':
                    // Sweet as
                    break;

                case 'i':
                    // Limits stay the same, suffixes need changed
                    ebSuffix = "EiB";
                    pbSuffix = "PiB";
                    tbSuffix = "TiB";
                    gbSuffix = "GiB";
                    mbSuffix = "MiB";
                    kbSuffix = "KiB";
                    bSuffix = "  B";
                    break;

                case 'd':
                    // Suffixes stay the same, limits need changed
                    ebLimit = 1000000000000000000;
                    pbLimit = 1000000000000000;
                    tbLimit = 1000000000000;
                    gbLimit = 1000000000;
                    mbLimit = 1000000;
                    kbLimit = 1000;
                    break;

                default:
                    // Should have already Excepted, but hey whatever
                    throw new ArgumentException($"notationFormat argument '{notationFormat}' not supported");

            }

            string fileSizeText;

            // Exa/Exbi sized
            if (byteCount >= ebLimit)
            {
                fileSizeText = $"{((double)byteCount / ebLimit):N1} {ebSuffix}";
            }
            // Peta/Pebi sized
            else if (byteCount >= pbLimit)
            {
                fileSizeText = $"{((double)byteCount / pbLimit):N1} {pbSuffix}";
            }
            // Tera/Tebi sized
            else if (byteCount >= tbLimit)
            {
                fileSizeText = $"{((double)byteCount / tbLimit):N1} {tbSuffix}";
            }
            // Giga/Gibi sized
            else if (byteCount >= gbLimit)
            {
                fileSizeText = $"{((double)byteCount / gbLimit):N1} {gbSuffix}";
            }
            // Mega/Mibi sized
            else if (byteCount >= mbLimit)
            {
                fileSizeText = $"{((double)byteCount / mbLimit):N1} {mbSuffix}";
            }
            // Kilo/Kibi sized
            else if (byteCount >= kbLimit)
            {
                fileSizeText = $"{((double)byteCount / kbLimit):N1} {kbSuffix}";
            }
            // Byte sized
            else
            {
                fileSizeText = $"{byteCount} {bSuffix}";
            }

            return fileSizeText;
        }

        void lockButtons()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                button_select_destination_folder.IsEnabled = false;
                button_select_source_folder.IsEnabled = false;
                button_analyze.IsEnabled = false;
                button_run.IsEnabled = false;
                button_stop.IsEnabled = true;
                button_about.IsEnabled = false;
                button_exit.IsEnabled = false;
            }));
        }
        void unlockButtons()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                button_select_destination_folder.IsEnabled = true;
                button_select_source_folder.IsEnabled = true;
                button_analyze.IsEnabled = true;
                button_run.IsEnabled = true;
                button_stop.IsEnabled = false; 
                button_about.IsEnabled = true;
                button_exit.IsEnabled = true;
            }));
        }



        void DirSearchForCopyFiles(string sDir) //argument is ORIGINAL folder
        {
            try
            {
                if (!cont)
                    return;
                //if(!hasDoableActions())
                updateStatusLine("Аналіз папки " + sDir + "...");
                try
                {//check directory
                    string originalAddress = sDir;
                    string copyAddress = sDir.Replace(originalFolder, copyFolder);
                    if (originalAddress.Length > 230) //deal with long path:   add \\?\
                        originalAddress = "\\\\?\\" + originalAddress;
                    if (copyAddress.Length > 230) //deal with long path:   add \\?\
                        copyAddress = "\\\\?\\" + copyAddress;
                    if (!Directory.Exists(copyAddress) && Directory.Exists(originalAddress))
                    {
                        string copyDate = ""; //Directory.GetCreationTime(copyAddress).ToString();
                        string mainDate = Directory.GetCreationTime(originalAddress).ToString();
                        long copySize = 0;// new FileInfo(copyAddress).Length;
                        long originalSize = 0;// new FileInfo(originalAddress).Length;
                        addItemToGrid(
                            /*Selected*/ true,
                            /*Doable*/ true,
                            /*TypeImage*/ "ic_folder.png",
                            /*ActionIcon*/ "ic_copy.png",
                            /*Filename*/ System.IO.Path.GetFileName(copyAddress),
                            /*CopyPath*/ copyAddress,
                            /*CopySize*/ copySize,
                            /*CopyDate*/ copyDate,
                            /*MainPath*/ originalAddress,
                            /*MainSize*/ originalSize,
                            /*MainDate*/ mainDate
                        );
                    }
                }
                catch (Exception e)
                {
                    addItemToGrid("ic_error.png", "Помилка перевірки наявності папки: " + e.Message, sDir);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    DirSearchForCopyFiles(d);
                }
                foreach (string f in Directory.GetFiles(sDir))
                { //check files
                    try
                    {
                        string originalAddress = f;
                        string copyAddress = f.Replace(originalFolder, copyFolder);
                        if (originalAddress.Length > 230) //deal with long path:   add \\?\
                            originalAddress = "\\\\?\\" + originalAddress;
                        if (copyAddress.Length > 230) //deal with long path:   add \\?\
                            copyAddress = "\\\\?\\" + copyAddress;
                        if (!File.Exists(copyAddress) && File.Exists(originalAddress))
                        {
                            string copyDate = ""; //Directory.GetLastWriteTime(copyAddress).ToString();
                            string mainDate = File.GetLastWriteTime(originalAddress).ToString();
                            long copySize = 0; // new FileInfo(copyAddress).Length;
                            long originalSize = new FileInfo(originalAddress).Length;
                            addItemToGrid(
                                /*Selected*/ true,
                                /*Doable*/ true,
                                /*TypeImage*/ "ic_file.png",
                                /*ActionIcon*/ "ic_copy.png",
                                /*Filename*/ System.IO.Path.GetFileName(copyAddress),
                                /*CopyPath*/ copyAddress,
                                /*CopySize*/ copySize,
                                /*CopyDate*/ copyDate,
                                /*MainPath*/ originalAddress,
                                /*MainSize*/ originalSize,
                                /*MainDate*/ mainDate
                            );
                        }
                        if (File.Exists(copyAddress) && File.Exists(originalAddress))
                        {
                            string copyDate = File.GetLastWriteTime(copyAddress).ToString();
                            string mainDate = File.GetLastWriteTime(originalAddress).ToString();
                            long copySize = new FileInfo(copyAddress).Length;
                            long originalSize = new FileInfo(originalAddress).Length;
                            if (!copyDate.Equals(mainDate) || copySize != originalSize)
                            {
                                addItemToGrid(
                                    /*Selected*/ true,
                                    /*Doable*/ true,
                                    /*TypeImage*/ "ic_file.png",
                                    /*ActionIcon*/ "ic_edit.png",
                                    /*Filename*/ System.IO.Path.GetFileName(copyAddress),
                                    /*CopyPath*/ copyAddress,
                                    /*CopySize*/ copySize,
                                    /*CopyDate*/ copyDate,
                                    /*MainPath*/ originalAddress,
                                    /*MainSize*/ originalSize,
                                    /*MainDate*/ mainDate
                                );
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        addItemToGrid("ic_error.png", "Помилка перевірки наявності файлу: " + e.Message, f);
                    }
                }
            }
            catch (System.Exception e)
            {
                addItemToGrid("ic_error.png", "Помилка: " + e.Message);
            }
        }

        void DirSearchForRemovingFiles(string sDir) //argument is COPY folder
        {
            try
            {
                if (!cont)
                    return;
                //if (!hasDoableActions())
                updateStatusLine("Аналіз папки " + sDir + "..."); 
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    DirSearchForRemovingFiles(d);
                }
                foreach (string f in Directory.GetFiles(sDir))
                { //check files
                    try
                    {
                        string copyAddress = f;
                        string originalAddress = f.Replace(copyFolder, originalFolder);
                        if (originalAddress.Length > 230) //deal with long path:   add \\?\
                            originalAddress = "\\\\?\\" + originalAddress;
                        if (copyAddress.Length > 230) //deal with long path:   add \\?\
                            copyAddress = "\\\\?\\" + copyAddress;
                        if (File.Exists(copyAddress) && !File.Exists(originalAddress))
                        {
                            string copyDate = File.GetCreationTime(copyAddress).ToString();
                            string mainDate = ""; // Directory.GetCreationTime(originalAddress).ToString();
                            long copySize = new FileInfo(copyAddress).Length;
                            long originalSize = 0;// new FileInfo(originalAddress).Length;
                            addItemToGrid(
                                /*Selected*/ true,
                                /*Doable*/ true,
                                /*TypeImage*/ "ic_file.png",
                                /*ActionIcon*/ "ic_delete.png",
                                /*Filename*/ System.IO.Path.GetFileName(copyAddress),
                                /*CopyPath*/ copyAddress,
                                /*CopySize*/ copySize,
                                /*CopyDate*/ copyDate,
                                /*MainPath*/ originalAddress,
                                /*MainSize*/ originalSize,
                                /*MainDate*/ mainDate
                            );
                        }
                    }
                    catch(Exception e)
                    {
                        addItemToGrid("ic_error.png", "Помилка перевірки наявності файлу: " + e.Message, f);
                    }
                }
                try{//check directory
                    string copyAddress = sDir;
                    string originalAddress = sDir.Replace(copyFolder, originalFolder);
                    if (originalAddress.Length > 230) //deal with long path:   add \\?\
                        originalAddress = "\\\\?\\" + originalAddress;
                    if (copyAddress.Length > 230) //deal with long path:   add \\?\
                        copyAddress = "\\\\?\\" + copyAddress;
                    if (Directory.Exists(copyAddress) && !Directory.Exists(originalAddress))
                    {
                        addItemToGrid(
                            /*Selected*/ true,
                            /*Doable*/ true,
                            /*TypeImage*/ "ic_folder.png",
                            /*ActionIcon*/ "ic_delete.png",
                            /*Filename*/ System.IO.Path.GetFileName(copyAddress),
                            /*CopyPath*/ copyAddress,
                            /*CopySize*/ 0,
                            /*CopyDate*/"",
                            /*MainPath*/ originalAddress,
                            /*MainSize*/ 0,
                            /*MainDate*/""
                        );
                    }
                }
                catch (Exception e)
                {
                    addItemToGrid("ic_error.png", "Помилка перевірки наявності папки: " + e.Message, sDir);
                }
            }
            catch (System.Exception e)
            {
                addItemToGrid("ic_error.png", "Помилка: " + e.Message);
            }
        }

        private void button_run_Click(object sender, RoutedEventArgs e)
        {
            cont = true;
            if (!checkPaths())
                return;

            if (syncThread == null)
            {
                syncThread = new Thread(() => runRunAsync());
                syncThread.Start();
            }
        }
        void setSelected(int actionIndex, bool newValue)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                actionsList[actionIndex].Selected = newValue;
            }));
        }
        void setAction(int actionIndex, string newValue, string newFilename)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
            {
                actionsList[actionIndex].ActionIcon = newValue;
                actionsList[actionIndex].Filename = newFilename;
            }));
        }

        private void button_stop_Click(object sender, RoutedEventArgs e)
        {
            cont = false;
            button_stop.IsEnabled = false;
        }

        private void button_exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void button_about_Click(object sender, RoutedEventArgs e)
        {
            new About().Show();
        }

        private void runRunAsync()
        {
            try
            {
                if (actionsList.Count == 0)
                {
                    runAnalyzeAsync();
                }
                if (actionsList.Count == 0)
                    return;

                lockButtons();

                //count doable
                int doableCount = 0;
                for (int i = 0; i < actionsList.Count; i++)
                {
                    if (actionsList[i].Doable) { doableCount++; }
                }
                addItemToGrid("ic_info.png", "Всього задач: " + doableCount + " шт");

                for (int i = 0; i < actionsList.Count && cont; i++)
                {
                    if (actionsList[i].Doable && actionsList[i].Selected) {
                        try
                        {
                            if (actionsList[i].Type.Equals("ic_file.png") && actionsList[i].ActionIcon.Equals("ic_delete.png"))
                            {
                                File.SetAttributes(actionsList[i].CopyPath, FileAttributes.Normal);
                                File.Delete(actionsList[i].CopyPath);
                                setSelected(i, false);
                            }
                            if (actionsList[i].Type.Equals("ic_file.png") && actionsList[i].ActionIcon.Equals("ic_copy.png"))
                            {
                                File.Copy(actionsList[i].MainPath, actionsList[i].CopyPath);
                                setSelected(i, false);
                            }
                            if (actionsList[i].Type.Equals("ic_file.png") && actionsList[i].ActionIcon.Equals("ic_edit.png"))
                            {
                                File.SetAttributes(actionsList[i].CopyPath, FileAttributes.Normal);
                                File.Delete(actionsList[i].CopyPath);
                                File.Copy(actionsList[i].MainPath, actionsList[i].CopyPath);
                                setSelected(i, false);
                            }

                            if (actionsList[i].Type.Equals("ic_folder.png") && actionsList[i].ActionIcon.Equals("ic_delete.png"))
                            {
                                Directory.Delete(actionsList[i].CopyPath);
                                setSelected(i, false);
                            }

                            if (actionsList[i].Type.Equals("ic_folder.png") && actionsList[i].ActionIcon.Equals("ic_copy.png"))
                            {
                                Directory.CreateDirectory(actionsList[i].CopyPath);
                                setSelected(i, false);
                            }


                            updateStatusLineCounters();
                        }
                        catch (Exception e) {
                            setAction(i, "ic_error.png", e.Message);
                            updateStatusLineCounters();
                        }
                    }
                }
                refreshGrid();
            }
            catch (Exception e)
            {
                addItemToGrid("ic_error.png", "Помилка: " + e.Message);
            }
            finally
            {
                syncThread = null;
                unlockButtons();
                updateStatusLineCounters();
                if(!cont)
                    updateStatusLine("Перервано користувачем.");
            }
        }
    }


    public class Action
    {
        public bool Doable { get; set; }
        public bool Selected { get; set; }
        public string Type { get; set; } //image
        public string ActionIcon { get; set; } //image
        public string Filename { get; set; } 
        public string CopyPath { get; set; }
        public string MainPath { get; set; }
        public long SizeCopy { get; set; }
        public long SizeOriginal { get; set; }
        public string DateCopy { get; set; }
        public string DateOriginal { get; set; }
    }
}
