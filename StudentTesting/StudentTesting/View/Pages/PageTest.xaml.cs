using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace StudentTesting.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageTest.xaml
    /// </summary>
    public partial class PageTest : Page
    {
        private int _index = -1;
        private int _idStudent;
        private int _idTest;

        private string _fullName;
        private string _nameTest;

        private List<Question> _questions;
        private Timer _timer;
        private TimeSpan _timeRemaining;

        private bool _isPulsing = false;

        ListBoxItem _selectedItem;

        private readonly ClassBaserowApiClient _baserowApiClient;

        internal PageTest(int idStudent, string fullName, int idTest, int count, int time, string nameTest, List<Question> questions)
        {
            _nameTest = nameTest;
            _idStudent = idStudent;
            _idTest = idTest;
            _fullName = fullName; 
            _questions = questions;
            _timeRemaining = TimeSpan.FromMinutes(time);
            List<string> items = new List<string>();
            for (int i = 1; i <= count; i++) items.Add(i.ToString());
            _baserowApiClient = new ClassBaserowApiClient();

            InitializeComponent();

            Task.Run(() => StartTimer());
            txtBlockStudentFullName.Text = fullName;
            txtBlockTestName.Text = nameTest;
            lvButton.ItemsSource = items;

            lvButton.Loaded += LvButton_Loaded;
        }
        private void LvButton_Loaded(object sender, RoutedEventArgs e)
        {
            // Программное выделение первого элемента после загрузки ListView
            lvButton.SelectedItem = lvButton.Items[0];
        }


        private void StartTimer()
        {
            _timer = new Timer(1000); // Устанавливаем интервал в 1 секунду
            _timer.Elapsed += Timer_Tick;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            if (_timeRemaining.TotalSeconds > 0)
            {
                _timeRemaining = _timeRemaining.Add(TimeSpan.FromSeconds(-1));
                UpdateTimerText();

                if (_timeRemaining.TotalSeconds <= 60)
                {
                    UpdateTimerAppearance();
                }
            }
            else
            {
                _timer.Stop();
                NavigateToResultsPage();
            }
        }
        private void UpdateTimerText()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    txtBlockTime.Text = $"{_timeRemaining.Minutes:D2}:{_timeRemaining.Seconds:D2}";
                });
            }
            catch { }
            
        }
        private void UpdateTimerAppearance()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!_isPulsing)
                {
                    txtBlockTime.Foreground = Brushes.Red;
                    txtBlockTime.FontWeight = FontWeights.Bold;
                    StartPulsingEffect();
                }
            });
        }

        private void StartPulsingEffect()
        {
            _isPulsing = true;
            var animationTimer = new Timer(500);
            animationTimer.Elapsed += (s, e) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    txtBlockTime.Opacity = txtBlockTime.Opacity == 1.0 ? 0.5 : 1.0;
                });
            };
            animationTimer.AutoReset = true;
            animationTimer.Start();
        }

        private void lvButton_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                int selectedIndex = lvButton.Items.IndexOf(e.AddedItems[0]);
                SavePreviousAnswer();
                UpdateUI(selectedIndex);
            }
        }

        private void SavePreviousAnswer()
        {
            if (_index != -1)
            {
                List<string> selectedAnswers = GetSelectedAnswers();
                _questions[_index].Answer = selectedAnswers;
                if(selectedAnswers.Count != 0) MarkPreviousButtonAsAnswered(_selectedItem);
            }
        }

        private List<string> GetSelectedAnswers()
        {
            List<string> selectedAnswers = new List<string>();

            switch (_questions[_index].Type)
            {
                case "SingleChoice":
                    selectedAnswers = GetSelectedSingleChoiceItems();
                    break;
                case "MultipleChoice":
                    selectedAnswers = GetSelectedMultipleChoiceItems();
                    break;
                case "TextAnswer":
                    selectedAnswers = GetTextAnswer();
                    break;
            }

            return selectedAnswers;
        }

        private List<string> GetSelectedSingleChoiceItems()
        {
            List<string> selectedChoices = new List<string>();

            foreach (var item in lvChoices.Items)
            {
                ListViewItem listViewItem = (ListViewItem)lvChoices.ItemContainerGenerator.ContainerFromItem(item);
                RadioButton radioButton = FindVisualChild<RadioButton>(listViewItem);

                if (radioButton != null && radioButton.IsChecked == true)
                {
                    selectedChoices.Add(item.ToString());
                }
            }

            return selectedChoices;
        }

        private List<string> GetSelectedMultipleChoiceItems()
        {
            List<string> selectedChoices = new List<string>();

            foreach (var item in lvMultipleChoice.Items)
            {
                ListViewItem listViewItem = (ListViewItem)lvMultipleChoice.ItemContainerGenerator.ContainerFromItem(item);
                CheckBox checkBox = FindVisualChild<CheckBox>(listViewItem);

                if (checkBox != null && checkBox.IsChecked == true)
                {
                    selectedChoices.Add(item.ToString());
                }
            }

            return selectedChoices;
        }

        private List<string> GetTextAnswer()
        {
            List<string> textAnswer = new List<string>();

            if (!string.IsNullOrWhiteSpace(txtBoxChoice.Text))
            {
                textAnswer.Add(txtBoxChoice.Text);
            }

            return textAnswer;
        }

        private void UpdateUI(int selectedIndex)
        {
            tbInformation.Text = _questions[selectedIndex].QuestionText;

            switch (_questions[selectedIndex].Type)
            {
                case "SingleChoice":
                    spMultipleChoice.Visibility = Visibility.Collapsed;
                    spTextAnswer.Visibility = Visibility.Collapsed;
                    lvChoices.ItemsSource = null;
                    lvChoices.ItemsSource = _questions[selectedIndex].Choices;
                    lvChoices.UpdateLayout();
                    foreach (var item in lvChoices.Items)
                    {
                        string choice = item.ToString();

                        ListViewItem listViewItem = lvChoices.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;

                        if (listViewItem != null)
                        {
                            listViewItem.IsSelected = _questions[selectedIndex].Answer.Contains(choice);
                        }
                    }
                    spSingleChoice.Visibility = Visibility.Visible;
                    break;
                case "MultipleChoice":
                    spSingleChoice.Visibility = Visibility.Collapsed;
                    spTextAnswer.Visibility = Visibility.Collapsed;
                    lvMultipleChoice.ItemsSource = null;
                    lvMultipleChoice.ItemsSource = _questions[selectedIndex].Choices;
                    lvMultipleChoice.UpdateLayout();
                    foreach (var item in lvMultipleChoice.Items)
                    {
                        string choice = item.ToString();

                        ListViewItem listViewItem = lvMultipleChoice.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;

                        if (listViewItem != null)
                        {
                            listViewItem.IsSelected = _questions[selectedIndex].Answer.Contains(choice);
                        }
                    }
                    spMultipleChoice.Visibility = Visibility.Visible;
                    break;
                case "TextAnswer":
                    txtBoxChoice.Clear();
                    if(_questions[selectedIndex].Answer.Count != 0) txtBoxChoice.Text = _questions[selectedIndex].Answer[0];
                    spSingleChoice.Visibility = Visibility.Collapsed;
                    spMultipleChoice.Visibility = Visibility.Collapsed;
                    spTextAnswer.Visibility = Visibility.Visible;
                    break;
            }

            _index = selectedIndex;
            _selectedItem = (ListBoxItem)lvButton.ItemContainerGenerator.ContainerFromIndex(selectedIndex);
        }

        private void MarkPreviousButtonAsAnswered(ListBoxItem previousItem)
        {
            Button selectedButton = FindVisualChild<Button>(previousItem);
            selectedButton.Style = (Style)FindResource("MaterialDesignFloatingActionMiniAccentButton");
        }

        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private double GetResultTest()
        {
            double totalScore = 0;

            foreach (var question in _questions)
            {
                double score = 0;
                int correctCount = question.Answer.Count(a => question.CorrectAnswers.Any(c => c.Equals(a, StringComparison.OrdinalIgnoreCase)));
                int incorrectCount = question.Answer.Count - correctCount;

                if (question.CorrectAnswers.Count > 1)
                {
                    // Для вопросов с несколькими вариантами ответа
                    double correctPercentage = question.Points;
                    if (question.Type == "MultipleChoice")
                    {
                        correctPercentage /= question.CorrectAnswers.Count;
                    }
                    double rightPoints = correctPercentage * correctCount;
                    double notRightPoints = correctPercentage * incorrectCount;
                    score = rightPoints - notRightPoints;
                }
                else
                {
                    // Для вопросов с одним вариантом ответа
                    score = question.Points * correctCount;
                }

                totalScore += score;
            }
            totalScore = Math.Round(totalScore, 2);
            return totalScore;
        }

        private async void btEnd_Click(object sender, RoutedEventArgs e)
        {
            lvButton.SelectedItem = lvButton.Items[0];
            int nullValue = 0;
            string message;
            foreach (var question in _questions)
            {
                if (question.Answer.Count == 0) nullValue++;
            }
            if(nullValue > 0)
            {
                message = $"Остались вопросы без ответа (количество: {nullValue})";
                
            }
            else
            {
                message = $"Завершение тестирования";
            }
            var result = MessageBox.Show(message, $"Вы уверены, что хотите закончить тест?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                double resultTest = GetResultTest();
                await _baserowApiClient.UpdateStudentByIDAsync(_idStudent.ToString(), _idTest.ToString(), resultTest);
                NavigationService.Navigate(new PageResult(resultTest, _fullName, _nameTest, _idStudent));
            }
        }

        private async void NavigateToResultsPage()
        {
            // Переход на другую страницу в UI-потоке
            Application.Current.Dispatcher.Invoke(() =>
            {
                lvButton.SelectedItem = lvButton.Items[0];
            });
            double resultTest = GetResultTest();
            // Асинхронный вызов вне лямбда-выражения
            await _baserowApiClient.UpdateStudentByIDAsync(_idStudent.ToString(), _idTest.ToString(), resultTest);

            // Переход на другую страницу в UI-потоке
            Application.Current.Dispatcher.Invoke(() =>
            {
                NavigationService.Navigate(new PageResult(resultTest, _fullName, _nameTest, _idStudent));
            });
        }
    }   
}
