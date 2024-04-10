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
        // Экземпляр клиента API Baserow, который будет использоваться для взаимодействия с API.
        private readonly ClassBaserowApiClient _baserowApiClient;

        // Глобальный идентификатор, который будет использоваться в пределах класса.
        private int _idGlobal;
        public PageMain(int id)
        {
            InitializeComponent();
            // Создаем новый экземпляр ClassBaserowApiClient для взаимодействия с API Baserow.
            _baserowApiClient = new ClassBaserowApiClient();

            // Сохраняем глобальный идентификатор для дальнейшего использования в классе.
            _idGlobal = id;

            // Подписываемся на событие Loaded для текущего класса.
            // Как только событие произойдет, будет асинхронно загружены данные.
            this.Loaded += async (sender, args) => await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Получаем тесты студентов по указанному идентификатору, используя Baserow API.
                var testStudents = await _baserowApiClient.GetTestStudentByIdAsync(ConfigurationManager.AppSettings["TestStudent"], _idGlobal);

                // Проверяем, что коллекция testStudents не равна null и содержит элементы.
                if (testStudents != null && testStudents.Any())
                {
                    // Если есть тестовые студенты, извлекаем все тесты и объединяем их в один список.
                    var allTests = testStudents.SelectMany(ts => ts.Test).ToList();

                    // Обновляем представление списка с использованием полученных тестов.
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
            // Устанавливаем коллекцию тестов как источник данных для ListView 'lvSubject',
            // что позволяет отобразить эти тесты в пользовательском интерфейсе.
            lvSubject.ItemsSource = tests;
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            // Навигируем к новой странице PageStart с помощью NavigationService,
            // переводя пользователя на другую страницу в приложении.
            NavigationService.Navigate(new PageStart());
        }
    }
}
