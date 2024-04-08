using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Xml.Linq;

namespace StudentTesting.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageMain.xaml
    /// </summary>
    public partial class PageMain : Page
    {
        private readonly ClassBaserowApiClient _baserowApiClient;
        private int idGlobal;
        public PageMain(int id)
        {
            InitializeComponent();
            _baserowApiClient = new ClassBaserowApiClient();
            idGlobal = id;

            // Лучше использовать событие Loaded или подобное
            this.Loaded += async (sender, args) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var testStudents = await _baserowApiClient.GetTestStudentByIdAsync(ConfigurationManager.AppSettings["TestStudent"], idGlobal);
                if (testStudents != null && testStudents.Any())
                {
                    var allTests = testStudents.SelectMany(ts => ts.Test).ToList();
                    UpdateListView(allTests);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }

        private void UpdateListView(IEnumerable<StructJson> tests)
        {
            lvSubject.ItemsSource = tests;
        }
        private void btGo_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageStart());
        }
    }
}
