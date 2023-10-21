using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FP_MyBackup
{
    /// <summary>
    /// Логика взаимодействия для About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            labelName_.Content += "   (v" + new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("yyyy.MM.dd") + ")";

            richtextbox.Selection.Text = "FP MyBackup - це проста програма для синхронізації резервних копій. " +
                "\r\nВ моєму розумінні резервна копія - це буквально копія даних. Не архів, а саме копія, яка містить аналогічні дані. " +
                "Резервна копія не передбачає роботи з даними в ній, тому якщо в резервній копії відбулись якісь зміни - вони будуть видалені. " +
                "Збережуться тільки зміни внесені в основній копії даних." +
                "\r\nВи завжди зможете помістити файл з цією програмою у себе на диску на якому зберігається резервна копія ваших даних та оновити їх швидко, в кілька кліків. " +
                "Адреса папок зберігається в файл поряд з файлом програми і коли ви наступний раз відкриєте програму, " +
                "не знадобиться повторно обирати адреси, якщо вони не змінювались." +
                "\r\nЯ вирішив написати цю програму тому, що так і не знайшов простої та безкоштовної програми для простої задачі - синхронізації файлів між двома дисками. " +
                "Аналоги, які мені вдавалось знайти, або мали рекламу, або працювали некоректно, або потребували покупки ліцензії. " +
                "Моя програма повністю безкоштовна, не має реклами та не робить нічого зайвого." +
                "\r\nЯкщо ви не впевнені чи правильно програма зрозуміє які файли куди потрібно переміщувати, можна натиснути Аналіз, тоді програма покаже відмінності, " +
                "але не буде проводити операції з файлами, щоб нічого не пошкодити." +
                "\r\n" +
                "\r\nПри написанні цієї програми використані іконки з таких джерел: \r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/hard-disk\" title=\"hard disk icons\">Hard disk icons created by Those Icons - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/favorite\" title=\"favorite icons\">Favorite icons created by Aldo Cervantes - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/file-directory\" title=\"file directory icons\">File directory icons created by Arkinasi - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/search\" title=\"search icons\">Search icons created by Chanut - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/play\" title=\"play icons\">Play icons created by Freepik - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/close\" title=\"close icons\">Close icons created by Alfredo Hernandez - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/document\" title=\"document icons\">Document icons created by Freepik - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/folder\" title=\"folder icons\">Folder icons created by stockes_02 - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/sync\" title=\"sync icons\">Sync icons created by Tempo_doloe - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/ui\" title=\"ui icons\">Ui icons created by riajulislam - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/info\" title=\"info icons\">Info icons created by Freepik - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/copy\" title=\"copy icons\">Copy icons created by Pixel perfect - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/edit\" title=\"edit icons\">Edit icons created by Pixel perfect - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/logout\" title=\"logout icons\">Logout icons created by Pixel perfect - Flaticon</a>\r\n" +
                "<a href=\"https://www.flaticon.com/free-icons/stop-button\" title=\"stop button icons\">Stop button icons created by Pixel perfect - Flaticon</a>";
        }

        private void button_exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
