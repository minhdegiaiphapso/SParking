using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SP.Parking.Terminal.Core.Utilities
{
    public static class KeyUtil
    {
        public static string ConvertToString(KeyEventArgs args)
        {
            string s = args.Key.ToString();
            //Key k = (Key)Enum.Parse(typeof(Key), s);

            switch (args.Key)
            {
                case Key.System:
                case Key.Enter:
                    {
                        return IsKeyExtended(args) ? "Right" + s : "Left" + s;
                    }
            }

            return s;
        }

        public static bool IsKeyExtended(KeyEventArgs args)
        {
            return (bool)typeof(KeyEventArgs).InvokeMember("IsExtendedKey", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance, null, args, null);
        }
    }
}
