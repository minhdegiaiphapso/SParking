using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Green.Devices.Dal.CardControler
{
    public class CashDrawer
    {
        SerialPort _comPort = new SerialPort();
        public CashDrawer(string PortName)
        {
            _comPort.PortName = PortName;
            _comPort.BaudRate = 9600;
            _comPort.DataBits = 8;
            _comPort.StopBits = StopBits.One;
            _comPort.Handshake = Handshake.None;
            _comPort.Parity = Parity.None;
        }
        public bool Open()
        {
            try
            {
                _comPort.Open();
                _comPort.Write("1");
                _comPort.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool Close()
        {
            try
            {
                _comPort.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
    public class ComAlarm
    {
        SerialPort _comPort = new SerialPort();
        public ComAlarm(string PortName)
        {
            _comPort.PortName = PortName;
            _comPort.BaudRate = 9600;
            _comPort.DataBits = 8;
            _comPort.StopBits = StopBits.One;
            _comPort.Handshake = Handshake.None;
            _comPort.Parity = Parity.None;
        }
        public bool Open(string actionKey)
        {
            try
            {
                _comPort.Open();
                _comPort.Write(actionKey);
                _comPort.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool Close()
        {
            try
            {
                _comPort.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
