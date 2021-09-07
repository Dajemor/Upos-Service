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
using System.Windows.Shapes;

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
