using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Control = System.Windows.Controls.Control;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;


namespace Upos_service
{

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        
        pinpadini spinpadini = new pinpadini();
        public Cheque saveCheque = new Cheque();
        SBRFpin pin ;
        string[] Vidorig = { "VID_079B&PID_0028", "VID_1234&PID_0101", "VID_193A&PID_1000", "VID_11CA&PID_0219", "VID_11CA&PID_0220", "VID_9908& PID_9030" }; //ingenico+ pax verifone verifone
        private string boxpath;
        public MainWindow()
        {
            InitializeComponent();
            boxpath = box_target.Text;
           
            //проверка на запуск от администратора
            if (IsAdmin())
            {
                adm_but.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                grid_adm.IsEnabled = false;
                System.Drawing.Icon img = System.Drawing.SystemIcons.Shield;
                Bitmap bitmap = img.ToBitmap();
                IntPtr hBitmap = bitmap.GetHbitmap();
                ImageSource wpfBitmap =
                    System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap, IntPtr.Zero, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                adm_img.Source = wpfBitmap;
                adm_img.Height = 38;

            }

            // передача параметров для перезапуска
            string[] args = Environment.GetCommandLineArgs();
            if (args.Count() > 1)
            {
                this.Left = Int32.Parse(args[2]);
                this.Top = Int32.Parse(args[3]);
                this.Width = Int32.Parse(args[4]);
                this.Height = Int32.Parse(args[5]);
            }

        }
        public static bool IsAdmin()
        {
            System.Security.Principal.WindowsIdentity id = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal p = new System.Security.Principal.WindowsPrincipal(id);

            return p.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        //Метод копирования директории и файлов
        public async Task CopyDirectory(DirectoryInfo source, DirectoryInfo target, Label status)
        {
            //если дериктория  существует
            if (target.Exists)
            {
                Random rnd = new Random();
                Directory.Move(target.FullName,
                    target.FullName.Remove(target.FullName.Length - 1) + "_old" + rnd.Next().ToString());
            }

            target.Create(); //создать директорию

            // копирование всех файлов
            FileInfo[] files = source.GetFiles();
            await Task.Run(() =>
            {
                foreach (FileInfo file in files)
                {
                    file.CopyTo(Path.Combine(target.FullName, file.Name));
                    Dispatcher.BeginInvoke(new ThreadStart(delegate { status.Content = "Скопировано: " + file.Name;}));
                  
                }

                DirectoryInfo[] dirs = source.GetDirectories();
                foreach (DirectoryInfo dir in dirs)
                {
                    string destinationDir = Path.Combine(target.FullName, dir.Name);
                    CopyDirectory(dir, new DirectoryInfo(destinationDir), status);
                }

                Dispatcher.BeginInvoke(new ThreadStart(delegate { status.Content = "Готово"; })); 
            });
        }
        
        private void Copy_but_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryInfo sourse = new DirectoryInfo(@"upos\"+ version_upos.Text);
                DirectoryInfo target = new DirectoryInfo(boxpath);
                CopyDirectory(sourse, target,status_lab);
            }
            catch (System.NotSupportedException exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void But_dll_Click(object sender, RoutedEventArgs e)
        {
            Newprocces("regsvr32", boxpath+ @"sbrf.dll "+boxpath +"sbrfcom.dll");
        }
        //запуск нового процесса
        public static async Task Newprocces(string procpath, string aurg)
        {
            Process reg = new Process();
            reg.StartInfo = new ProcessStartInfo(procpath, aurg);
            reg.StartInfo.RedirectStandardOutput = true;
            reg.StartInfo.UseShellExecute = false;
            //Запускаем процесс 
            try
            {
                reg.Start();
                string req = await reg.StandardOutput.ReadToEndAsync();
                if (req.Length != 0)
                    MessageBox.Show(req);
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        //Запуск службы
        public static void StartService(string serviceName)
        {
            ServiceController service = new ServiceController(serviceName);
            // Проверяем не запущена ли служба
            try
            {
                if (service.Status != ServiceControllerStatus.Running)
                {
                    // Запускаем службу
                    service.Start();
                    // В течении минуты ждём статус от службы
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(1));
                    MessageBox.Show("Служба была успешно запущена!");
                }
                else
                {
                    MessageBox.Show("Служба уже запущена!");
                }
            }
            catch (System.InvalidOperationException e)
            {
                MessageBox.Show(e.Message);

            }

        }
        
        private void But_agent_Click(object sender, RoutedEventArgs e)
        {
          
            Newprocces("cmd.exe", $"/C {boxpath}agent.exe /reg").ContinueWith(x => StartService("Upos2Agent"));
          
        }

        
      //  comPort.vid == "VID_079B&PID_0028"|| comPort.vid == "VID_1234&PID_0101" || comPort.vid == "VID_193A&PID_1000" || comPort.vid == "VID_11ca&PID_0219"

        private void But_port_Click(object sender, RoutedEventArgs e)
        {
            list_serial.Items.Clear();
            foreach (COMPortInfo comPort in COMPortInfo.GetCOMPortsInfo())

            {// ingenico /pax/ /verifone/ verifone
                if (Vidorig.Contains(comPort.vid))
                {
                    ListBoxItem li = new ListBoxItem();
                    li.Background = Brushes.Green;
                    li.Content = comPort.Name + "/" + comPort.Description;
                    list_serial.Items.Add(li);
                }
                else
                {
                    list_serial.Items.Add(comPort.Name + "/" + comPort.Description);
                }
               
            }
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string sel;


            if ( list_serial.SelectedItem != null)
            {
                sel = list_serial.SelectedItem.ToString();
                Match match = Regex.Match(sel ,"COM([0-9]+)");
           
                 sel = match.Groups[1].Value;
                
            }
            else
            {
                sel = "9";
            }
            comp_lab.Content = "ComPort="+sel;
            comp_tex.Text = sel;

        }

         
     int p = 3;

        


        private async void But_call_Click(object sender, RoutedEventArgs e)
        {
            
            test.Content = "ждем ответ";
            but_call.IsEnabled = false;
            list_serial.IsEnabled = false;
            p = await pin.AsyncPinReady();
            if (p == 0)
            { 
                test.Content = "Подключен:"+p.ToString();
                test.Background = System.Windows.Media.Brushes.Green;
            }
            else
            {
             test.Content = "Не подключен:" + p.ToString();
             test.Background= System.Windows.Media.Brushes.Red;
            }
            but_call.IsEnabled = true;
            list_serial.IsEnabled = true;

        }


        #region Pinpad.ini

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            spinpadini.PinpadLog = "PinpadLog=1";
            pinl_text.Content = spinpadini.PinpadLog;
        }

        private void Pinl_chek_Unchecked(object sender, RoutedEventArgs e)
        {
            spinpadini.PinpadLog = "PinpadLog=0";
            pinl_text.Content = spinpadini.PinpadLog;
        }

        private void Show_chk_Checked(object sender, RoutedEventArgs e)
        {
            spinpadini.ShowScreens = "ShowScreens=1";
            show_text.Content = spinpadini.ShowScreens;
        }

        private void Show_chk_Unchecked(object sender, RoutedEventArgs e)
        {
            spinpadini.ShowScreens = "ShowScreens=0";
            show_text.Content = spinpadini.ShowScreens;
        }
        private void Ptinend_tex_TextChanged(object sender, TextChangedEventArgs e)
        {
            spinpadini.printerend = "printerend=" + ptinend_tex.Text;
            printend_lab.Content = spinpadini.printerend;
        }

        private void Comp_tex_TextChanged(object sender, TextChangedEventArgs e)
        {
            spinpadini.ComPort = "ComPort=" + comp_tex.Text;
            comp_lab.Content = spinpadini.ComPort;
        }

        private void Prfil_text_TextChanged(object sender, TextChangedEventArgs e)
        {   
            spinpadini.printerfile = "printerfile=" + prfil_text.Text;
            prifile_lab.Content = spinpadini.printerfile;
            
        }
        
        private void Clearpin_but_click(object sender, RoutedEventArgs e)
        {
            comp_tex.Text = "9";
            pinl_chek.IsChecked = false;
            show_chk.IsChecked = true;
            ptinend_tex.Text = "01";
            prfil_text.Text = "p";
        }
        //сохранить в пинпад.ини
        private void Savepin_but_click(object sender, RoutedEventArgs e)
        {
            string str = string.Empty;
            using (System.IO.StreamReader reader = System.IO.File.OpenText($"{boxpath}/pinpad.ini"))
            {
                str = reader.ReadToEnd();
            }
            //pinpadlog
            {
                if (Regex.IsMatch(str, "PinpadLog=[0-1]+"))
                    str = Regex.Replace(str, "PinpadLog=[0-1]+", spinpadini.PinpadLog);
                else
                {
                    str = $"{spinpadini.PinpadLog}\r\n" + str;
                }
            }
            //Printerfile
            {
                if (Regex.IsMatch(str, "printerfile=.+"))
                    str = Regex.Replace(str, "printerfile.+", spinpadini.printerfile);
                else
                {
                    str = $"{spinpadini.printerfile}\r\n" + str;
                }
            }
            //printerend
            {
                if (Regex.IsMatch(str, "printerend=.+"))
                    str = Regex.Replace(str, "printerend=.+", spinpadini.printerend);
                else
                {
                    str = $"{spinpadini.printerend}\r\n" + str;
                }
            }
            //ShowScreens
            {
                if (Regex.IsMatch(str, "ShowScreens=[0-1]+"))
                    str = Regex.Replace(str, "ShowScreens=[0-1]+", spinpadini.ShowScreens);
                else
                {
                    str = $"{spinpadini.ShowScreens}\r\n" + str;
                }
            }

            //ComPort
            {
                if (Regex.IsMatch(str, "ComPort=[0-9]+"))
                    str = Regex.Replace(str, "ComPort=[0-9]+", spinpadini.ComPort);
                else
                {
                    str = $"{spinpadini.ComPort}\r\n" + str;
                }
            }


            using (System.IO.StreamWriter file = new System.IO.StreamWriter($"{boxpath}/pinpad.ini"))
            {
                file.Write(str);
            }

            status_lab.Content = "Pinpad.ini сохранен";
        }

        #endregion

        #region install drivers

        //установить драйвер верифон
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryInfo sourse = new DirectoryInfo(@"drivers\VeriFone\");
                DirectoryInfo target = new DirectoryInfo(boxpath+ @"\VeriFone");

              CopyDirectory(sourse, target, status_lab).ContinueWith(x=>Newprocces(boxpath + @"\VeriFone\setup.bat", "")) ;
                
            }
            catch (NotSupportedException exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
        //установить драйвер инженико
        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryInfo sourse = new DirectoryInfo(@"drivers\Ingenico\");
                DirectoryInfo target = new DirectoryInfo(boxpath + @"\Ingenico");
                CopyDirectory(sourse, target, status_lab).ContinueWith(x=> Newprocces(boxpath + @"\Ingenico\setup.exe", ""));
            }
            catch (NotSupportedException exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
        //установить драйвер пакс
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                DirectoryInfo sourse = new DirectoryInfo(@"drivers\Pax\");
                DirectoryInfo target = new DirectoryInfo(boxpath + @"\Pax");
                CopyDirectory(sourse, target, status_lab).ContinueWith(x=> Newprocces(boxpath + @"\Pax\setup.exe", ""));
            }
            catch (NotSupportedException exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        #endregion

        
        //выполнить удаленную загрузку
        private void Uz_but_Click(object sender, RoutedEventArgs e)
        {
            grid_term.IsEnabled = false;
            Newprocces($"{boxpath}/loadparm.exe", $"21 {ka_tex.Text} {tid_tex.Text}").ContinueWith(x => Dispatcher.BeginInvoke(new ThreadStart(
                delegate { grid_term.IsEnabled = true; }))); 
            
        }

       
        //выполнить отмену
        private async void Canselpay_but_Click(object sender, RoutedEventArgs e)
        {
            grid_term.IsEnabled = false;
            saveCheque.Return_ = await pin.CanselPayAsync();
            Status(saveCheque.Return_,refund_stat);
            grid_term.IsEnabled = true;
        }
        //сверка итогов
        private async void  Final_but_click(object sender, RoutedEventArgs e)
        {

           grid_term.IsEnabled = false;
            saveCheque.Final_ = await pin.FinaldayAsync();
            Status(saveCheque.Final_,final_stat);
            grid_term.IsEnabled = true;
            
        }

        private void Status(string cheque, Label status)
        {
            if (cheque.Length > 10)
            {
                status.Content = "Успешно";
            }
            else
            {
                status.Content = "Произошла ошибка: " + cheque;
            }
        }
        //оплата
        private async void Pay_but_Click(object sender, RoutedEventArgs e)
        {
           grid_term.IsEnabled = false;
            saveCheque.Summ_ = await pin.PayAsync(Convert.ToInt32(sum_tex.Text));
            Status(saveCheque.Summ_,pay_stat);
            grid_term.IsEnabled = true;
          
        }
        //возврат на сумму 
        private async void Forward_but_click(object sender, RoutedEventArgs e)
        {
            grid_term.IsEnabled = false;
            saveCheque.Return_ = await pin.ForwPayAsync(Convert.ToInt32(sum_tex.Text));
            Status(saveCheque.Return_,rrefund_stat);
            grid_term.IsEnabled = true;
        }
        //получить чек помощь
        private void Help_but_click(object sender, RoutedEventArgs e)
        {
            grid_term.IsEnabled = false;
            Newprocces($"{boxpath}\\loadparm.exe", $"36");
            string str;
            FileInfo tagInfo = new FileInfo( boxpath+"\\p");
            if (tagInfo.Exists)
            {
                str = File.ReadAllText($"{boxpath}\\p",Encoding.GetEncoding(866));
            }
            else
            {
                str = "Ошибка";
            }
            saveCheque.Help_ = str;
            Status(saveCheque.Help_,help_stat);
           
            grid_term.IsEnabled = true;

        }
        //выполнить проверку связи
        private async void Ping_but_click(object sender, RoutedEventArgs e)
        {
            grid_term.IsEnabled = false;
            saveCheque.Ping_ =await pin.Sb_pilotAsync(@"C:/sc552/sbrf.dll 47 2");
            Status(saveCheque.Ping_,ping_stat);
            grid_term.IsEnabled = true;
        }
        //получить тид терминала
        private async void Get_tid_but_Click(object sender, RoutedEventArgs e)
        {
            grid_term.IsEnabled = false;
            tid_tex.Text = await pin.AsyncTid();
            grid_term.IsEnabled = true;
        }

       
        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            pin = new SBRFpin();
            
            if (pin.Sbrfready() ==true )
            {
                grid_term.IsEnabled = true;
            }
            else
            {
               grid_term.IsEnabled = false;
            }

            //получаем список версий
            
            DirectoryInfo verspath = new DirectoryInfo(@"upos\");
            if (verspath.Exists)
            {

            
            DirectoryInfo[] vers = verspath.GetDirectories();

            for (int i = 0; i < vers.Length; i++)
            {
                version_upos.Items.Add(vers[i]);
            }

            version_upos.SelectedIndex = 0;
            foreach (COMPortInfo comPort in COMPortInfo.GetCOMPortsInfo())

            {
                if (Vidorig.Contains(comPort.vid))
                {
                    ListBoxItem li = new ListBoxItem();
                    li.Background = Brushes.Green;
                    li.Content = comPort.Name + "/" + comPort.Description;
                    list_serial.Items.Add(li);
                }
                else
                {
                    list_serial.Items.Add(comPort.Name + "/" + comPort.Description);
                }
            }

            }
        }

        private void Ready_sbrf_click(object sender, RoutedEventArgs e)
        {
            pin = new SBRFpin();

            if (pin.Sbrfready() == true)
            {
                grid_term.IsEnabled = true;
            }
            else
            {
                grid_term.IsEnabled = false;
            }
        }

        private void Adm_but_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = System.Windows.Forms.Application.ExecutablePath;
            startInfo.Arguments = "restart " + this.Left + " "
                                  + this.Top + " " + this.Width + " " + this.Height;

            startInfo.Verb = "runas";
            try
            {
                Process p = Process.Start(startInfo);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Open_form_cheque(object sender, RoutedEventArgs e)
        {
            Window1 win2 = new Window1(saveCheque);
            win2.Show();
        }

        private void Select_dir_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                boxpath = fbd.SelectedPath+"\\";
            }
        }
        //обновление
        

        private void About_menu_click(object sender, RoutedEventArgs e)
        {
            About form = new About();
            form.Show();
        }

        private void Version_menu_click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                HttpClient http = new HttpClient();
                Version latestVersion = new Version(http.GetStringAsync(@"https://raw.githubusercontent.com/Dajemor/Upos-Service/master/version.txt").GetAwaiter().GetResult());
                if (latestVersion > currentVersion)
                {

                    var result = MessageBox.Show("Найдена новая версия, Скачать?","Обновление", MessageBoxButton.YesNo,MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start($"https://github.com/Dajemor/Upos-Service/releases/download/"+ latestVersion+"/Upos_service.rar");
                    }
                }
                else
                {
                    MessageBox.Show("Обновления не найдены");
                }
            }
            catch (System.Net.Http.HttpRequestException exception)
            {
                MessageBox.Show("Интернет соединение отсутствует" + exception);
                
            }
            
        }
        //привязка к клавишам
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            if (e.Key == Key.LeftCtrl)
            {
                overlay.Visibility = Visibility.Visible;
            }

            if (e.Key == Key.Z && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Copy_but_Click(sender, e);
            }
            if (e.Key == Key.X && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                But_dll_Click(sender, e);
                But_agent_Click(sender, e);
            }

            if (e.Key == Key.C && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Button_Click_1(sender, e);
            }
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Savepin_but_click(sender, e);
            }

            if (e.Key == Key.D1 && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Pay_but_Click(sender, e);
            }
            if (e.Key == Key.D2 && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Canselpay_but_Click(sender, e);
            }
            if (e.Key == Key.D3 && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Ping_but_click(sender, e);
            }
            if (e.Key == Key.D4 && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Final_but_click(sender, e);
            }
            if (e.Key == Key.D5 && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Help_but_click(sender, e);
            }
            if (e.Key == Key.Q && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
              Butt_all(sender,e);
            }
            if (e.Key == Key.W && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Open_form_cheque(sender, e);
            }
            if (e.Key == Key.E && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Loadparm_Click(sender, e);
            }
            if (e.Key == Key.P && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                Delmac_but_Click(sender, e);
            }


        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                overlay.Visibility = Visibility.Collapsed;
            }
        }

        private void Loadparm_Click(object sender, RoutedEventArgs e)
        {
            grid_term.IsEnabled = false;
            Newprocces($"{boxpath}/loadparm.exe", $"11").ContinueWith(x => Dispatcher.BeginInvoke(new ThreadStart(
                delegate { grid_term.IsEnabled = true;})));

           // grid_term.IsEnabled = true;
            // await pin.Sb_pilotAsync(@"C:/sc552/sbrf.dll 11");
        }

        private void Delmac_but_Click(object sender, RoutedEventArgs e)
        {
           Newprocces($"{boxpath}/loadparm.exe", $"22");
            // await pin.Sb_pilotAsync(@"C:/sc552/sbrf.dll 22");
        }

        private async void Butt_all(object sender, RoutedEventArgs e)
        {
            grid_term.IsEnabled = false;


            //Final_but_click(sender, e);
            saveCheque.Final_ = await pin.FinaldayAsync();
            Status(saveCheque.Final_, final_stat);
            //Pay_but_Click(sender, e);
            saveCheque.Summ_ = await pin.PayAsync(Convert.ToInt32(sum_tex.Text));
            Status(saveCheque.Summ_, pay_stat);
            //Canselpay_but_Click(sender, e);
            saveCheque.Return_ = await pin.CanselPayAsync();
            Status(saveCheque.Return_, refund_stat);
            //Final_but_click(sender, e);
            saveCheque.Final_ = await pin.FinaldayAsync();
            Status(saveCheque.Final_, final_stat);
            //Ping_but_click(sender, e);
            saveCheque.Ping_ = await pin.Sb_pilotAsync(@"C:/sc552/sbrf.dll 47 2");
            Status(saveCheque.Ping_, ping_stat);
            //Help_but_click(sender, e);
             await Newprocces($"{boxpath}\\loadparm.exe", $"36");
            
            string str;
            FileInfo tagInfo = new FileInfo(boxpath + "\\p");
            if (tagInfo.Exists)
            {
                str = File.ReadAllText($"{boxpath}\\p", Encoding.GetEncoding(866));
            }
            else
            {
                str = "Ошибка";
            }
            saveCheque.Help_ = str;
            Status(saveCheque.Help_, help_stat);
            grid_term.IsEnabled = true;
        }

        private void box_target_TextChanged(object sender, TextChangedEventArgs e)
        {
            boxpath = box_target.Text;
        }
    }
    }


