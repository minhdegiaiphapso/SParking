using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Green.Devices.Dal.ProxiesReader
{
    public class MasterRDimport
    {
        [DllImport("MasterRD.dll", EntryPoint = "rf_init_com")]
        internal static extern int rf_init_com(ushort port, ushort baud);
        /// <summary>

        /// </summary>
        /// <returns></returns>
        [DllImport("MasterRD.dll", EntryPoint = "rf_ClosePort")]
        internal static extern int rf_ClosePort();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [DllImport("MasterRD.dll", EntryPoint = "Read_Em4001")]
        internal static extern int Read_Em4001(byte[] data);
        [DllImport("MasterRD.dll", EntryPoint = "rf_light")]
        internal static extern int rf_light(byte icdev, byte color);

        [DllImport("MasterRD.dll", EntryPoint = "rf_beep")]
        internal static extern int rf_beep(byte icdev, int msec);
        //Write Standard T55x7

        [DllImport("MasterRD.dll", EntryPoint = "Standard_Write")]
        internal static extern int Standard_Write(byte opcode, char Lock, char Data_Write, char Block);
    }
}
