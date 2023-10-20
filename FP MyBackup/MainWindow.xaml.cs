using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace FP_MyBackup
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_select_source_folder_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker dlg = new FolderPicker();
            if (dlg.ShowDialog() == true)
            {
                textbox_source_path.Text = dlg.ResultPath;
            }
        }

        private void button_select_destination_folder_Click(object sender, RoutedEventArgs e)
        {

            FolderPicker dlg = new FolderPicker();
            if (dlg.ShowDialog() == true)
            {
                textbox_destination_path.Text = dlg.ResultPath;
            }
        }
    }
}
