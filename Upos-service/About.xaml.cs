using System.Diagnostics;
using System.Windows;
namespace Upos_service
{
    /// <summary>
    /// Логика взаимодействия для About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Dajemor/Upos-Service");
        }
    }
}
