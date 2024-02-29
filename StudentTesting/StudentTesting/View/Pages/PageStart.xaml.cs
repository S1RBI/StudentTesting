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
using System.Windows.Controls.Primitives;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Configuration;

namespace StudentTesting.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageStart.xaml
    /// </summary>
    public partial class PageStart : Page
    {
        private readonly ClassBaserowApiClient _baserowApiClient;
        private readonly ClassNet _emailSender;
        internal PageStart()
        {
            InitializeComponent();
            _baserowApiClient = new ClassBaserowApiClient();
            _emailSender = new ClassNet();
        }

        private async void btGo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentRecord = await _baserowApiClient.GetRecordByEmailAsync(ConfigurationManager.AppSettings["Student"], tbName.Text);

                if (currentRecord == null)
                {
                    MessageBox.Show("Не верно введен email.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                NavigationService.Navigate(new PageMain(true));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
}
