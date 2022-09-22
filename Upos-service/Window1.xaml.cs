using System.Windows;
using System.Windows.Input;
namespace Upos_service
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        
        public Window1(Cheque form1Cheque)
        {
            InitializeComponent();
            summ_ch.Text = form1Cheque.Summ_;
            return_ch.Text = form1Cheque.Return_;
            final_ch.Text = form1Cheque.Final_;
            ping_ch.Text = form1Cheque.Ping_;
            help_ch.Text = form1Cheque.Help_;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
