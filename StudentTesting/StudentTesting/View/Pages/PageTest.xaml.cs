using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace StudentTesting.View.Pages
{
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
        private ListBoxItem _selectedItem;
        private readonly ClassBaserowApiClient _baserowApiClient;

        internal PageTest(int idStudent, string fullName, int idTest, int count, int time, string nameTest, List<Question> questions)
        {
            _nameTest = nameTest;
            _idStudent = idStudent;
            _idTest = idTest;
            _fullName = fullName;
            _questions = questions;
            _timeRemaining = TimeSpan.FromMinutes(time);
            _baserowApiClient = new ClassBaserowApiClient();

            InitializeComponent();
            Loaded += PageTest_Loaded;
            this.Unloaded += OnPageUnloaded;

            // Подписываемся на системные события
            SystemEvents.SessionEnding += OnSessionEnding;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;

            if (App.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.WindowClosing += MainWindow_WindowClosing;
            }

            InitializeTestUI(count);
            StartTimer();
        }

        private void InitializeTestUI(int count)
        {
            var items = Enumerable.Range(1, count).Select(i => i.ToString()).ToList();
            txtBlockStudentFullName.Text = _fullName;
            txtBlockTestName.Text = _nameTest;
            lvButton.ItemsSource = items;
            lvButton.Loaded += LvButton_Loaded;
        }

        private void LvButton_Loaded(object sender, RoutedEventArgs e)
        {
            lvButton.SelectedItem = lvButton.Items[0];
        }

        private async void PageTest_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await _baserowApiClient.UpdateStudentByIDAsync(_idStudent.ToString(), _idTest.ToString(), 0f);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex}");
            }
        }

        private async void OnSessionEnding(object sender, SessionEndingEventArgs e)
        {
            await SaveAndExitAsync(GetResultTest());
        }

        private async void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Suspend)
            {
                await SaveAndExitAsync(GetResultTest());
            }
        }

        private void OnPageUnloaded(object sender, RoutedEventArgs e)
        {
            SystemEvents.SessionEnding -= OnSessionEnding;
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        }

        private void MainWindow_WindowClosing(object sender, CancelEventArgs e)
        {
            //lvButton.SelectedItem = lvButton.Items[0];
            //var nullValue = _questions.Count(q => q.Answer.Count == 0);
            //var message = nullValue > 0
            //    ? $"Ваш текущий результат будет сохранен\nОстались вопросы без ответа (количество: {nullValue})"
            //    : "Ваш текущий результат будет сохранен\nЗавершение тестирования";

            //var result = MessageBox.Show(message, "Вы уверены, что хотите закрыть приложение?", MessageBoxButton.YesNo);

            //if (result == MessageBoxResult.Yes)
            //{
            //    try
            //    {
            //        await SaveAndExitAsync(GetResultTest());
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }
            //}
            //else
            //{
            //    e.Cancel = true;
            //}
            // e.Cancel = true;
        }

        private async Task SaveAndExitAsync(double resultTest)
        {
            await _baserowApiClient.UpdateStudentByIDAsync(_idStudent.ToString(), _idTest.ToString(), resultTest);
        }

        private void StartTimer()
        {
            _timer = new Timer(1000);
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
                Application.Current.Dispatcher.Invoke(() => NavigateToResultsPage());
            }
        }

        private void UpdateTimerText()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                txtBlockTime.Text = $"{_timeRemaining.Minutes:D2}:{_timeRemaining.Seconds:D2}";
            });
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
                if (selectedAnswers.Count != 0)
                {
                    MarkPreviousButtonAsAnswered(_selectedItem);
                }
            }
        }

        private List<string> GetSelectedAnswers()
        {
            return _questions[_index].Type switch
            {
                "SingleChoice" => GetSelectedSingleChoiceItems(),
                "MultipleChoice" => GetSelectedMultipleChoiceItems(),
                "TextAnswer" => GetTextAnswer(),
                _ => new List<string>()
            };
        }

        private List<string> GetSelectedSingleChoiceItems()
        {
            return lvChoices.Items.Cast<object>()
                .Where(item => FindVisualChild<RadioButton>(lvChoices.ItemContainerGenerator.ContainerFromItem(item))?.IsChecked == true)
                .Select(item => item.ToString())
                .ToList();
        }

        private List<string> GetSelectedMultipleChoiceItems()
        {
            return lvMultipleChoice.Items.Cast<object>()
                .Where(item => FindVisualChild<CheckBox>(lvMultipleChoice.ItemContainerGenerator.ContainerFromItem(item))?.IsChecked == true)
                .Select(item => item.ToString())
                .ToList();
        }

        private List<string> GetTextAnswer()
        {
            return !string.IsNullOrWhiteSpace(txtBoxChoice.Text) ? new List<string> { txtBoxChoice.Text } : new List<string>();
        }

        private void UpdateUI(int selectedIndex)
        {
            tbInformation.Text = _questions[selectedIndex].QuestionText;

            switch (_questions[selectedIndex].Type)
            {
                case "SingleChoice":
                    UpdateVisibility(spSingleChoice);
                    lvChoices.ItemsSource = _questions[selectedIndex].Choices;
                    lvChoices.UpdateLayout();
                    SelectPreviousAnswers(lvChoices, _questions[selectedIndex].Answer);
                    break;
                case "MultipleChoice":
                    UpdateVisibility(spMultipleChoice);
                    lvMultipleChoice.ItemsSource = _questions[selectedIndex].Choices;
                    lvMultipleChoice.UpdateLayout();
                    SelectPreviousAnswers(lvMultipleChoice, _questions[selectedIndex].Answer);
                    break;
                case "TextAnswer":
                    UpdateVisibility(spTextAnswer);
                    txtBoxChoice.Text = _questions[selectedIndex].Answer.FirstOrDefault() ?? string.Empty;
                    break;
            }

            _index = selectedIndex;
            _selectedItem = (ListBoxItem)lvButton.ItemContainerGenerator.ContainerFromIndex(selectedIndex);
        }

        private void UpdateVisibility(StackPanel visiblePanel)
        {
            spSingleChoice.Visibility = spSingleChoice == visiblePanel ? Visibility.Visible : Visibility.Collapsed;
            spMultipleChoice.Visibility = spMultipleChoice == visiblePanel ? Visibility.Visible : Visibility.Collapsed;
            spTextAnswer.Visibility = spTextAnswer == visiblePanel ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SelectPreviousAnswers(ListView listView, List<string> previousAnswers)
        {
            foreach (var item in listView.Items)
            {
                ListViewItem listViewItem = listView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                listViewItem.IsSelected = previousAnswers.Contains(item.ToString());
            }
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
                if (child is T tChild) return tChild;
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null) return childOfChild;
            }
            return null;
        }

        private double GetResultTest()
        {
            lvButton.SelectedItem = lvButton.Items[0];
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
            var nullValue = _questions.Count(q => q.Answer.Count == 0);
            var message = nullValue > 0
                ? $"Остались вопросы без ответа (количество: {nullValue})"
                : "Завершение тестирования";

            var result = MessageBox.Show(message, "Вы уверены, что хотите закончить тест?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                await NavigateToResultsPage();
            }
        }

        private async Task NavigateToResultsPage()
        {
            double resultTest = GetResultTest();
            await SaveAndExitAsync(resultTest);
            Application.Current.Dispatcher.Invoke(() =>
            {
                _timer.Stop();
                NavigationService.Navigate(new PageResult(resultTest, _fullName, _nameTest, _idStudent));
            });
        }
    }
}
