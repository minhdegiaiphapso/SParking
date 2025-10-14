using Green.Apms.Terminal.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;

namespace Green.Apms.Terminal.Wpf
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new App();
            app.InitializeComponent();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            app.Run();
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            switch (args.Name)
            {
                case "Bosch.VideoSDK.AxCameoLib":
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Bosch.VideoSDK.AxCameoLib"))
                    {
                        byte[] assemblyData = new byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                case "Bosch.VideoSDK.CameoLib":
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Bosch.VideoSDK.CameoLib"))
                    {
                        byte[] assemblyData = new byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                default:
                    return args.RequestingAssembly;
            }
        }
    }
}
