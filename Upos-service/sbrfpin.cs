using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
namespace Upos_service
{
    public class SBRFpin : IDisposable
    {
        private dynamic _pinpad;
        public SBRFpin()
       {
           CreatePin();
       }
        public void CreatePin()
        {
            var t1 = Task.Factory.StartNew(() =>
            { 
                try
                {
                    var type = Type.GetTypeFromProgID("SBRFSRV.Server");
                    _pinpad = Activator.CreateInstance(type);
                }
                catch (Exception)
                {
                    MessageBox.Show("Библиоотека не зарегестрирована");
                    //_pinpad = null;
                }
            });
            t1.Wait();
        }
        public bool Sbrfready()
        {
            if (_pinpad == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public async Task<string> AsyncTid()
        {
            _pinpad.Clear();
            int result = await Task.Factory.StartNew(() => _pinpad.NFun(15));
            if (result == 0)
                return  _pinpad.GParamString("TermNum");
            else
            {
                return result.ToString();
            }
        }
      
        public async Task<int> AsyncPinReady()
        {
           
           int result=await Task.Factory.StartNew(() => _pinpad.NFun(13));
            return result;
        }
        public async Task<string> PayAsync(int amount)
        {
            _pinpad.Clear();
           _pinpad.SParam("Amount", amount);
            int result = await Task.Factory.StartNew(() => _pinpad.NFun(4000));
            if (result == 0)
               return  _pinpad.GParamString("Cheque");
            else
            {
                return result.ToString();
            }
        }
        public async Task<string> CanselPayAsync()
        {
            _pinpad.Clear();
            int result = await Task.Factory.StartNew(() => _pinpad.NFun(4003));
            if (result == 0)
                return  _pinpad.GParamString("Cheque");
            else
            {
                return result.ToString();
            }
        }
        public async Task<string> FinaldayAsync()
        {
            _pinpad.Clear();
            int result = await Task.Factory.StartNew(() => _pinpad.NFun(6000));
            if (result == 0)
                return  _pinpad.GParamString("Cheque");
            else
            {
                return result.ToString();
            }
        }
        public async Task<string> ForwPayAsync(int amount)
        {
            _pinpad.Clear();
            _pinpad.SParam("Amount", amount);
            int result = await Task.Factory.StartNew(() => _pinpad.NFun(4002));
            if (result == 0)
                return await _pinpad.GParamString("Cheque");
            else
            {
                return  result.ToString();
            }
        }
        public async Task<string> Sb_pilotAsync(string pathcom)
        {
            _pinpad.Clear();
            _pinpad.SParam("CmdLine", pathcom);
            var result = await Task.Factory.StartNew(() => _pinpad.NFun(17));
            if (result == 0)
                return _pinpad.GParamString("Cheque");
            else
            {
                return  result.ToString();
            }
        }
        public void Dispose()
        {
            if (_pinpad != null)
            {
                Marshal.ReleaseComObject(_pinpad);
            }
        }
    }
}
