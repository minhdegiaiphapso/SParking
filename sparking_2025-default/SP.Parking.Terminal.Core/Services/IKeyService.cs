using Newtonsoft.Json;
using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Utilities;
using SP.Parking.Terminal.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SP.Parking.Terminal.Core.Services
{
    //public class KeyActionDetail
    //{
    //    public string Key { get; set; }
    //}

    //public class UpdateVehicleTypeKeyAction : KeyActionDetail
    //{
    //    public VehicleType VehicleType { get; set; }
    //}
    
    //public class KeyActionDictionary
    //{
    //    public List<KeyActionItem> Data { get; set; }
    //    public void Add(KeyAction action, KeyActionItem actionItem, bool isUnique = true)
    //    {
    //        if (ContainAction(action))
    //        {
    //            var item = GetItem(action);
    //            if(isUnique)

    //        }
    //    }

    //    public bool ContainAction(KeyAction action)
    //    {
    //        if (Data.Where(item => item.Action == action).Count() > 0)
    //            return true;
    //        else
    //            return false;
    //    }

    //    public KeyActionItem GetItem (KeyAction action)
    //    {
    //        return Data.Where(item => item.Action == action).FirstOrDefault();
    //    }
    //}

    //public class KeyMapDictionary : Dictionary<KeyAction, List<KeyActionDetail>>
    //{
    //    public KeyAction GetAction(string key, out object moreDetail)
    //    {
    //        moreDetail = null;
    //        foreach (var item in this)
    //        {
    //            var details = item.Value;
    //            var detail = details.Where(i => i.Key == key).FirstOrDefault();
    //            if (detail != null)
    //            {
    //                if(detail is UpdateVehicleTypeKeyAction)
    //                {
    //                    moreDetail = (detail as UpdateVehicleTypeKeyAction).VehicleType;
    //                }

    //                return item.Key;
    //            }
    //        }

    //        return KeyAction.DoNothing;
    //    }
    //}

    public class KeyMap
    {
        [JsonProperty("key_map")]
        public Dictionary<KeyAction, string> KeysMap { get; private set; }

        //public KeyMapDictionary Dataaa { get; set; }

        [JsonProperty("lane_position")]
        public SectionPosition LanePosition { get; private set; }

        public KeyMap()
        {
            KeysMap = new Dictionary<KeyAction, string>();
            //Dataaa = new KeyMapDictionary();
        }

        public KeyMap(SectionPosition pos)
            : this()
        {
            LanePosition = pos;

            // init if there is no configuration file
            switch (pos)
            {
                case SectionPosition.Lane1:
                    {
                        //KeysMap.Add(KeyAction.Configuration, Key.F12.ToString());
                        //KeysMap.Add(KeyAction.DoSearch, Key.Return.ToString());
                        //KeysMap.Add(KeyAction.Back, Key.Oem5.ToString());
                        //KeysMap.Add(KeyAction.ExceptionalCheckout, Key.Delete.ToString());
                        KeysMap.Add(KeyAction.Delete, Key.Back.ToString());

                        KeysMap.Add(KeyAction.ChangeLaneDirection, Key.F1.ToString());
                        KeysMap.Add(KeyAction.ChangeLane, Key.F2.ToString());
                        //KeysMap.Add(KeyAction.Search, Key.Z.ToString());
                        KeysMap.Add(KeyAction.CheckOut, Key.LeftCtrl.ToString());
                        KeysMap.Add(KeyAction.CancelCheckOut, "LeftSystem");
                        //KeysMap.Add(KeyAction.Logout, Key.Q.ToString());
                        KeysMap.Add(KeyAction.ShowVehicleType, Key.F4.ToString());

                        KeysMap.Add(KeyAction.ConfirmCheckInKey, Key.Z.ToString());
                        KeysMap.Add(KeyAction.AddNewNumber, Key.X.ToString());
                        KeysMap.Add(KeyAction.CancelCheckInKey, Key.C.ToString());
                        KeysMap.Add(KeyAction.ForcedBarier, Key.F5.ToString());
                        KeysMap.Add(KeyAction.CashDrawer, Key.F6.ToString());
                        KeysMap.Add(KeyAction.PrintBill, Key.Q.ToString());
                        break;
                    }
                case SectionPosition.Lane2:
                    {
                        //KeysMap.Add(KeyAction.Configuration, Key.F12.ToString());
                        //KeysMap.Add(KeyAction.DoSearch, Key.Return.ToString());
                        //KeysMap.Add(KeyAction.Back, Key.PageUp.ToString());
                       // KeysMap.Add(KeyAction.ExceptionalCheckout, Key.Delete.ToString());
                        KeysMap.Add(KeyAction.Delete, Key.Subtract.ToString());

                        KeysMap.Add(KeyAction.ChangeLaneDirection, Key.F12.ToString());
                        KeysMap.Add(KeyAction.ChangeLane, Key.F11.ToString());
                        //KeysMap.Add(KeyAction.Search, Key.Divide.ToString());
                        KeysMap.Add(KeyAction.CheckOut, Key.RightCtrl.ToString());
                        KeysMap.Add(KeyAction.CancelCheckOut, "RightSystem");
                        //KeysMap.Add(KeyAction.Logout, Key.End.ToString());
                        KeysMap.Add(KeyAction.ShowVehicleType, Key.F9.ToString());

                        KeysMap.Add(KeyAction.ConfirmCheckInKey, Key.M.ToString());
                        KeysMap.Add(KeyAction.AddNewNumber, Key.N.ToString());
                        KeysMap.Add(KeyAction.CancelCheckInKey, Key.B.ToString());
                        KeysMap.Add(KeyAction.ForcedBarier, Key.F8.ToString());
                        KeysMap.Add(KeyAction.CashDrawer, Key.F6.ToString());
                        KeysMap.Add(KeyAction.PrintBill, Key.P.ToString());
                        break;
                    }

                case SectionPosition.Lane3:
                    {
                        //KeysMap.Add(KeyAction.Configuration, Key.F12.ToString());
                        //KeysMap.Add(KeyAction.DoSearch, Key.Return.ToString());
                        //KeysMap.Add(KeyAction.Back, Key.Oem5.ToString());
                        //KeysMap.Add(KeyAction.ExceptionalCheckout, Key.Delete.ToString());
                        KeysMap.Add(KeyAction.Delete, Key.Back.ToString());

                        KeysMap.Add(KeyAction.ChangeLaneDirection, Key.F1.ToString());
                        KeysMap.Add(KeyAction.ChangeLane, Key.F2.ToString());
                        //KeysMap.Add(KeyAction.Search, Key.Z.ToString());
                        KeysMap.Add(KeyAction.CheckOut, Key.LeftCtrl.ToString());
                        KeysMap.Add(KeyAction.CancelCheckOut, "LeftSystem");
                        //KeysMap.Add(KeyAction.Logout, Key.Q.ToString());
                        KeysMap.Add(KeyAction.ShowVehicleType, Key.F4.ToString());

                        KeysMap.Add(KeyAction.ConfirmCheckInKey, Key.Z.ToString());
                        KeysMap.Add(KeyAction.AddNewNumber, Key.X.ToString());
                        KeysMap.Add(KeyAction.CancelCheckInKey, Key.C.ToString());
                        KeysMap.Add(KeyAction.ForcedBarier, Key.F5.ToString());
                        KeysMap.Add(KeyAction.CashDrawer, Key.F6.ToString());
                        KeysMap.Add(KeyAction.PrintBill, Key.Q.ToString());
                        break;
                    }
                case SectionPosition.Lane4:
                    {
                        //KeysMap.Add(KeyAction.Configuration, Key.F12.ToString());
                        //KeysMap.Add(KeyAction.DoSearch, Key.Return.ToString());
                        //KeysMap.Add(KeyAction.Back, Key.PageUp.ToString());
                        // KeysMap.Add(KeyAction.ExceptionalCheckout, Key.Delete.ToString());
                        KeysMap.Add(KeyAction.Delete, Key.Subtract.ToString());

                        KeysMap.Add(KeyAction.ChangeLaneDirection, Key.F12.ToString());
                        KeysMap.Add(KeyAction.ChangeLane, Key.F11.ToString());
                        //KeysMap.Add(KeyAction.Search, Key.Divide.ToString());
                        KeysMap.Add(KeyAction.CheckOut, Key.RightCtrl.ToString());
                        KeysMap.Add(KeyAction.CancelCheckOut, "RightSystem");
                        //KeysMap.Add(KeyAction.Logout, Key.End.ToString());
                        KeysMap.Add(KeyAction.ShowVehicleType, Key.F9.ToString());
                        KeysMap.Add(KeyAction.PrintBill, Key.P.ToString());
                        KeysMap.Add(KeyAction.ConfirmCheckInKey, Key.M.ToString());
                        KeysMap.Add(KeyAction.AddNewNumber, Key.N.ToString());
                        KeysMap.Add(KeyAction.CancelCheckInKey, Key.B.ToString());
                        KeysMap.Add(KeyAction.ForcedBarier, Key.F8.ToString());
                        KeysMap.Add(KeyAction.CashDrawer, Key.F6.ToString());

                        break;
                    }
            }
        }

        public void AddItem(KeyAction keyAction, string keyboard, bool isUnique = false)
        {
            if (KeysMap.ContainsValue(keyboard))
            {
                var k = KeysMap.FirstOrDefault(x => x.Value == keyboard).Key;
                KeysMap[keyAction] = string.Empty;
            }

            if (KeysMap.ContainsKey(keyAction))
                KeysMap[keyAction] = keyboard;
            else
                KeysMap.Add(keyAction, keyboard);
        }

        //public void AddItem(KeyAction action, KeyActionDetail detail, bool isUnique = true)
        //{
        //    if (Dataaa.ContainsKey(action))
        //    {
        //        if (isUnique)
        //            Dataaa[action] = new List<KeyActionDetail> { detail };
        //        else
        //            Dataaa[action].Add(detail);
        //    }
        //    else
        //    {
        //        Dataaa.Add(action, new List<KeyActionDetail> { detail });
        //    }
        //}

        public string GetKey(KeyAction action)
        {
            if (KeysMap.ContainsKey(action))
                return KeysMap[action];
            else
                return string.Empty;
        }

        public static string ConvertToNumericKey(Key key)
        {
            if (key >= Key.D0 && key <= Key.D9)
            {
                int number = (int)key - (int)Key.D0;
                return number.ToString();
            }
            else if (key >= Key.NumPad0 && key <= Key.NumPad9)
            {
                int number = (int)key - (int)Key.NumPad0;
                return number.ToString();
            }
            else return string.Empty;
        }

        public KeyAction GetAction(KeyEventArgs keyArgs, out string output, Type type = null)
        {
            Key key = keyArgs.Key;
            output = string.Empty;

            if (key >= Key.D0 && key <= Key.D9 && LanePosition == SectionPosition.Lane1)
            {
                int number = (int)key - (int)Key.D0;
                output = number.ToString();
                return KeyAction.Number;
            }
            else if (key >= Key.NumPad0 && key <= Key.NumPad9 && LanePosition == SectionPosition.Lane2)
            {
                int number = (int)key - (int)Key.NumPad0;
                output = number.ToString();
                return KeyAction.Number;
            }
            
            else
            {
                if (key == Key.Return)
                {
                    if (type == typeof(SearchViewModel))
                    {
                        return KeyAction.DoSearch;
                    }
                }
                if (LanePosition == SectionPosition.Lane1)
                {
                    if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.Q)
                        return KeyAction.TypeBike;
                    else if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.W)
                        return KeyAction.TypeCar;
                    //else if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.E)
                    //    return KeyAction.TypeCarConten;
                    //else if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.R)
                    //    return KeyAction.TypeDeliverryMobi;
                }
                else if (LanePosition == SectionPosition.Lane2)
                {
                    if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.P)
                        return KeyAction.TypeBike;
                    else if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.O)
                        return KeyAction.TypeCar;
                    //else if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.I)
                    //    return KeyAction.TypeCarConten;
                    //else if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.U)
                    //    return KeyAction.TypeDeliverryMobi;
                }

               
                string strKey = KeyUtil.ConvertToString(keyArgs);
                return this.KeysMap.FirstOrDefault(x => x.Value == strKey).Key;
            }
        }

        //public KeyAction GetAction1(KeyEventArgs keyArgs, out object output, Type type = null)
        //{
        //    Key key = keyArgs.Key;
        //    output = string.Empty;

        //    if (key >= Key.D0 && key <= Key.D9 && LanePosition == SectionPosition.Lane1)
        //    {
        //        int number = (int)key - (int)Key.D0;
        //        output = number.ToString();
        //        return KeyAction.Number;
        //    }
        //    else if (key >= Key.NumPad0 && key <= Key.NumPad9 && LanePosition == SectionPosition.Lane2)
        //    {
        //        int number = (int)key - (int)Key.NumPad0;
        //        output = number.ToString();
        //        return KeyAction.Number;
        //    }

        //    else
        //    {
        //        if (key == Key.Return)
        //        {
        //            if (type == typeof(SearchViewModel))
        //            {
        //                return KeyAction.DoSearch;
        //            }
        //        }
        //        string strKey = KeyUtil.ConvertToString(keyArgs);
        //        object obj;
        //        return Dataaa.GetAction(strKey, out obj);
        //    }
        //}
    }

    public enum KeyAction
    {
        DoNothing = 0,
        CheckOut,
        CancelCheckOut,
        UpdateCheckOut,
        Number,
        Delete,
        Search,
        ChangeLaneDirection,
        Back,
        Logout,
        ExceptionalCheckout,
        Configuration,
        DoSearch,
        ShowVehicleType,
        ChangeLane,
        ActivateProlificCardReader,
        ActivateSoyalCardReader,
        PrintBill,
        AddNewNumber,
        ConfirmCheckInKey,
        CancelCheckInKey,
        ForcedBarier,
        CashDrawer,
        Cashier,
        /// <summary>
        /// Xe Oto
        /// </summary>
        TypeCar = 2000101,

        /// <summary>
        /// Xe may
        /// </summary>
        TypeBike = 1000001,

        /// <summary>
        ///// Xe tai cho hang
        ///// </summary>
        //TypeCarConten = 5000401,
        ///// <summary>
        ///// Xe may giao hang
        ///// </summary>
        //TypeDeliverryMobi = 4000301
    }

    //public interface IKeyService
    //{
    //    bool IsKeyExtended(KeyEventArgs args);
    //    Key ConvertKeyFrom(object obj);
    //    //string ConvertToString(KeyEventArgs args);
    //    KeyResponse HandleLaneKey<T>(ISection section, KeyEventArgs args);
    //}

    public class KeyResponse
    {
        public KeyAction KeyAdvice { get; set; }
        public string Output { get; set; }
    }

    //public class KeyService : IKeyService
    //{
    //    KeyConverter _keyConverter;

    //    public KeyService()
    //    {
    //        _keyConverter = new KeyConverter();
    //    }

    //    public Key ConvertKeyFrom(object obj)
    //    {
    //        return (Key) _keyConverter.ConvertFrom(obj);
    //    }

    //    public bool IsKeyExtended(KeyEventArgs args)
    //    {
    //        return (bool)typeof(KeyEventArgs).InvokeMember("IsExtendedKey", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance, null, args, null);
    //    }

    //    public KeyResponse HandleLaneKey<T>(ISection section, KeyEventArgs args)
    //    {
    //        Key key = args.Key;

    //        if (key >= Key.D0 && key <= Key.D9)
    //        {
    //            int number = (int)key - (int)Key.D0;

    //            if (section.Id == SectionPosition.Lane1)
    //                return new KeyResponse { KeyAdvice = KeyAction.Number, Output = number.ToString() };
    //        }
    //        else if (key >= Key.NumPad0 && key <= Key.NumPad9)
    //        {
    //            int number = (int)key - (int)Key.NumPad0;

    //            if (section.Id == SectionPosition.Lane2)
    //                return new KeyResponse { KeyAdvice = KeyAction.Number, Output = number.ToString() };
    //        }
    //        else if (key == Key.Back)
    //        {
    //            if (section.Id == SectionPosition.Lane1)
    //                return new KeyResponse { KeyAdvice = KeyAction.Delete };
    //        }
    //        else if (key == Key.Subtract)
    //        {
    //            if (section.Id == SectionPosition.Lane2)
    //                return new KeyResponse { KeyAdvice = KeyAction.Delete };
    //        }
    //        else if (key == Key.RightCtrl)
    //        {
    //            if (section.Id == SectionPosition.Lane2)
    //                return new KeyResponse { KeyAdvice = KeyAction.CancelCheckOut };
    //        }
    //        else if (key == Key.LeftCtrl)
    //        {
    //            if (section.Id == SectionPosition.Lane1)
    //                return new KeyResponse { KeyAdvice = KeyAction.CancelCheckOut };
    //        }
    //        else if (key == Key.Enter)
    //        {
    //            if (typeof(T) == typeof(CheckOut))
    //            {
    //                if (section.Id == SectionPosition.Lane1 && !IsKeyExtended(args))
    //                    return new KeyResponse { KeyAdvice = KeyAction.CheckOut };
    //                if (section.Id == SectionPosition.Lane2 && IsKeyExtended(args))
    //                    return new KeyResponse { KeyAdvice = KeyAction.CheckOut };
    //            }
    //            else if (typeof(T) == typeof(SearchViewModel))
    //            {
    //                if (section.Id == SectionPosition.Lane1 && !IsKeyExtended(args))
    //                    return new KeyResponse { KeyAdvice = KeyAction.Search };
    //                if (section.Id == SectionPosition.Lane2 && IsKeyExtended(args))
    //                    return new KeyResponse { KeyAdvice = KeyAction.Search };
    //            }
    //        }

    //        return new KeyResponse { KeyAdvice = KeyAction.DoNothing };
    //    }
    
    //    //public string ConvertToString(KeyEventArgs args)
    //    //{
    //    //    string s = args.Key.ToString();
    //    //    Key k = (Key)Enum.Parse(typeof(Key), s);

    //    //    switch (args.Key)
    //    //    {
    //    //        case Key.System:
    //    //        case Key.Enter:
    //    //            {
    //    //                return IsKeyExtended(args) ? "Right" + s : "Left" + s;
    //    //            }
    //    //    }

    //    //    return s;
    //    //}
    
    //}
}
