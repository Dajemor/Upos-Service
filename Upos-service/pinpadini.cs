namespace Upos_service
{
    class pinpadini
    {
        

        public string ComPort{ get; set; }
        public string PinpadLog { get; set; }
       public string ShowScreens { get; set; }
        public string printerend { get; set; }
        public string printerfile { get; set; }

        public pinpadini()
        {
            this.ComPort = "9";
            this.PinpadLog = "0";
            this.ShowScreens = "1";
            this.printerend = "01";
            this.printerend = "01";
            this.printerfile = "p";
        }





    }
}
