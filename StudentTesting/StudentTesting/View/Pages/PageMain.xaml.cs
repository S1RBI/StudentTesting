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
        public PageMain(int id, string fullName)
        {
            InitializeComponent();
            txtBlock.Text = fullName;
            _baserowApiClient = new ClassBaserowApiClient();
            _idGlobal = id;
            Loaded += PageMain_Loaded;
        }

        private async void PageMain_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var testStudents = await _baserowApiClient.GetTestStudentByIdAsync(_idGlobal);

                if (testStudents?.Any() == true)
                {
                    var allSubject = (await _baserowApiClient.GetTestSubjectsByIdAsync(testStudents.SelectMany(ts => ts.Test).ToList()))
                            .Distinct(new StructJsonComparer())
                            .ToList();

                    UpdateListView(allSubject);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.ToString()}");
            }
        }
        private void UpdateListView(List<StructJson> tests)
        {
            // Устанавливаем коллекцию тестов как источник данных для ListView 'lvSubject',
            // что позволяет отобразить эти тесты в пользовательском интерфейсе.
            lvSubject.ItemsSource = tests;
        }

        private async void lvSubject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Сбрасываем видимость всех вложенных ListView
            foreach (var item in lvSubject.Items)
            {
                var container = lvSubject.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (container != null)
                {
                    // Находим вложенный ListView в контейнере
                    var lvSubItems = FindVisualChild<ListView>(container);
                    if (lvSubItems != null)
                    {
                        // Скрываем вложенный ListView
                        lvSubItems.Visibility = Visibility.Collapsed;
                    }
                }
            }

            // Показываем вложенный ListView для выбранного элемента
            if (lvSubject.SelectedItem is StructJson selectedItem)
            {
                var testsForStudentAndSubject = await _baserowApiClient.GetTestsByStudentIdAndSubjectIdAsync(_idGlobal, selectedItem.Id);

                // Получаем контейнер StackPanel для выбранного элемента
                var container = lvSubject.ItemContainerGenerator.ContainerFromItem(selectedItem) as ListViewItem;
                if (container != null)
                {
                    // Находим вложенный ListView в контейнере
                    var lvSubItems = FindVisualChild<ListView>(container);
                    if (lvSubItems != null)
                    {
                        // Устанавливаем список тестов в качестве источника данных для вложенного ListView
                        lvSubItems.ItemsSource = testsForStudentAndSubject;
                        lvSubItems.Visibility = Visibility.Visible;
                    }
                }
            }
        }
        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private async void SubItemsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var selectedItem = e.AddedItems[0] as TestStudent;
                var selectedItemTest = await _baserowApiClient.GetTestByIDAsync(selectedItem.Test[0].Id.ToString());
                MessageBoxResult result = MessageBox.Show($"Время на выполнение: {selectedItemTest.Period} мин.\nКоличество вопросов: {selectedItemTest.Quantity}", $"Вы уверены что хотите начать {selectedItemTest.NameTest}", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var deserializedResponse = await _baserowApiClient.LoadQuestionsFromFile(selectedItemTest.Files[0].Url);
                }
                else if (result == MessageBoxResult.No)
                { }
                else
                { }
            }
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

        private void MenuItem_ClickGoBack(object sender, RoutedEventArgs e)
        {
            // Навигируем к новой странице PageStart с помощью NavigationService,
            // переводя пользователя на другую страницу в приложении.
            NavigationService.Navigate(new PageStart());
        }

        private void MenuItem_ClickExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
