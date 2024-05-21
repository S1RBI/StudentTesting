using Microsoft.Win32;
using StudentTesting.View.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace StudentTesting
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public event EventHandler<CancelEventArgs> WindowClosing;
        public MainWindow()
        {
            InitializeComponent();
            mainFrame.Content = new PageMain(1, "Михейкин Юрий Андреевич");
            //mainFrame.Content = new PageStart();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowClosing?.Invoke(this, e);
        }

    }
}
