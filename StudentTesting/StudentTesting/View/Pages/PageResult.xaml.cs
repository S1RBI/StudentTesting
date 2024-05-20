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

namespace StudentTesting.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageResult.xaml
    /// </summary>
    public partial class PageResult : Page
    {

        private int _idStudent;
        private string _fullName;
        public PageResult(double bal, string fullName,  string nameTest, int idStudent)
        {
            _idStudent= idStudent;
            _fullName = fullName;
            InitializeComponent();
            txtBlock.Text = fullName;
            txtBlockTest.Text = $"Тест «{nameTest}» завершен!";
            txtBlockResult.Text = $"Ваш результат: {bal}/54";
        }

        private void MenuItem_ClickGoStart(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageStart());
        }

        private void MenuItem_ClickGoExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void MenuItem_ClickGoMain(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageMain(_idStudent, _fullName));
        }
    }
}
