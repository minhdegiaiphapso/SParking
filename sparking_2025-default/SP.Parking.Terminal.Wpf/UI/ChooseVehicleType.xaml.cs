using SP.Parking.Terminal.Core.Models;
using SP.Parking.Terminal.Core.Services;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SP.Parking.Terminal.Wpf.UI
{
    public class ChooseVehicleTypeArg : EventArgs
    {
        public VehicleType VehicleType { get; set; }
    }

    /// <summary>
    /// Interaction logic for ChooseVehicleType.xaml
    /// </summary>
    public partial class ChooseVehicleTypeWindow : CentralLaneWindow
    {
        public event EventHandler ChoseVehicleType;

        List<VehicleType> _vehicleType;
        public List<VehicleType> VehicleType
        {
            get { return _vehicleType; }
            set
            {
                _vehicleType = value;
                if (this._vehicleType != null)
                {
                    this._vehicleType = this._vehicleType.FindAll(
                        delegate(VehicleType Info)
                        {
                            return !Info.Id.Equals(100000000);
                        }
                        ).OrderBy(v=>v.Id).ToList();
                }
            }
        }

        public ChooseVehicleTypeWindow(List<VehicleType> vehicleType)
            : base()
        {
            InitializeComponent();
            VehicleType = vehicleType;
            this.KeyUp += ChooseVehicleTypeWindow_KeyUp;

            Layout();
        }

        private void Layout()
        {
            int rows = VehicleType.Count / 3 + 1;
            for (int i = 0; i < rows; i++)
            {
                var rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.RowDefinitions.Add(rowDef);
            }

            for (int i = 0; i < VehicleType.Count; i++)
            {
                int col = i % 3;
                int row = i / 3;
                VehicleTypeUI uc = new VehicleTypeUI() { Number = i + 1, TypeName = VehicleType[i].Name };
                MainGrid.Children.Add(uc);
                uc.SetValue(Grid.ColumnProperty, col);
                uc.SetValue(Grid.RowProperty, row);
            }

            this.Height = rows * 150 + 20;
        }

        void ChooseVehicleTypeWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                return;
            }

            int count = VehicleType.Count;
            int key = -1;
            var numStr = KeyMap.ConvertToNumericKey(e.Key);
            bool parseSuccess = int.TryParse(numStr, out key);
            key -= 1;
            if (parseSuccess && key < count)
            {                
                ChoseVehicleType(this, new ChooseVehicleTypeArg { VehicleType = VehicleType[key] });
                Close();
            }
        }
    }
}