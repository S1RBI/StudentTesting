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
using System.Text.RegularExpressions;

namespace StudentTesting.View.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageStart.xaml
    /// </summary>
    public partial class PageStart : Page
    {
        private readonly ClassBaserowApiClient _baserowApiClient;
        private readonly ClassNet _emailSender;
        private string _code;
        private int _id;
        internal PageStart()
        {
            
            InitializeComponent();
            stPanelCod.Visibility = Visibility.Collapsed;
            stLinkBack.Visibility = Visibility.Collapsed;

            _baserowApiClient = new ClassBaserowApiClient();
            _emailSender = new ClassNet();
        }

        private async void btGo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (stPanelMail.Visibility == Visibility.Collapsed)
                {
                    if (_code == (tbCod1.Text + tbCod2.Text + tbCod3.Text + tbCod4.Text))
                    {
                        NavigationService.Navigate(new PageMain(_id));
                    }
                    else
                    {
                        clearTextBox();
                        lableError.Content = "Введен не верный код";
                    }

                    return;
                }

                var currentRecord = await _baserowApiClient.GetStudentByEmailAsync(tbName.Text);
                _id = currentRecord.Id;

                if (currentRecord == null)
                {
                    lableError.Content = "Введен не верный пароль";
                    return;
                }
                else
                {
                    lableError.Content = "";
                    stPanelMail.Visibility = Visibility.Collapsed;
                    stPanelCod.Visibility = Visibility.Visible;
                    stLinkBack.Visibility = Visibility.Visible;
                    btGo.Content = "Войти";
                    Random random = new Random();
                    _code = Convert.ToString(random.Next(1000, 10000));
                    _emailSender.SendEmail(tbName.Text, "Одноразовый код", "Код: " + _code);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void tbCod_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Получить текст из буфера обмена
                string clipboard = Clipboard.GetText();

                if (Regex.IsMatch(clipboard, "^\\d+$"))
                {
                    clearTextBox();
                    tbCod1.Text = clipboard[0].ToString();
                    tbCod2.Text = clipboard[1].ToString();
                    tbCod3.Text = clipboard[2].ToString();
                    tbCod4.Text = clipboard[3].ToString();
                }
            }
        }

        private void clearTextBox()
        {
            tbCod1.Text = string.Empty;
            tbCod2.Text = string.Empty;
            tbCod3.Text = string.Empty;
            tbCod4.Text = string.Empty;
        }
        private void tbLinkBack_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            stLinkBack.Visibility = Visibility.Collapsed;
            stPanelCod.Visibility = Visibility.Collapsed;
            stPanelMail.Visibility = Visibility.Visible;
            lableError.Content = "";
            btGo.Content = "Отправить код";
            clearTextBox();
        }
    }   
}
