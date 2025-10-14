using MahApps.Metro.Controls;
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
using System.Drawing;
using SP.Parking.Terminal.Core.ViewModels;

namespace SP.Parking.Terminal.Wpf.UI
{
    public class CentralLaneWindow : MetroWindow
    {
        private UIElement _container;
        public UIElement Container
        {
            get { return _container; }
            set
            {
                _container = value;
            }
        }

        public CentralLaneWindow()
        {
            this.Topmost = true;

            System.Windows.Application.Current.MainWindow.LayoutUpdated += (s, e) => {
                if (Container == null)
                    return;
                var window = Window.GetWindow(Container);
                if (window != null && window.IsVisible)
                {
                    var pt = Container.PointToScreen(new System.Windows.Point(0, 0));
                    this.Left = pt.X + (Container.RenderSize.Width - this.RenderSize.Width) / 2;
                    this.Top = pt.Y + (Container.RenderSize.Height - this.RenderSize.Height) / 2;
                }
            };
            System.Windows.Application.Current.MainWindow.LocationChanged += (s, e) => {
                if (Container == null)
                    return;
                var window = Window.GetWindow(Container);
                if (window != null && window.IsVisible)
                {
                    var pt = Container.PointToScreen(new System.Windows.Point(0, 0));
                    this.Left = pt.X + (Container.RenderSize.Width - this.RenderSize.Width) / 2;
                    this.Top = pt.Y + (Container.RenderSize.Height - this.RenderSize.Height) / 2;
                }
            };
        }
    }

    /// <summary>
    /// Interaction logic for ConfirmWindow.xaml
    /// </summary>
    public partial class ConfirmWindow : CentralLaneWindow
    {
        
        public ConfirmWindow()
            : base()
        {
            InitializeComponent();

            this.KeyUp += (sender, e) => {
                if (e.Key == Key.Escape)
                    Close();
            };
        }

        public ConfirmWindow(string title, string message, List<string> buttonTexts, params Func<bool>[] actions) : this()
        {
            this.Title = title;
            ConfirmMessage.Content = message.ToUpper();
            ConfirmMessage.FontSize = 14;
            CreateButtons(buttonTexts, actions);
        }

        private void CreateButtons(List<string> buttonTexts, Func<bool>[] actions)
        {
            for (int i = 0; i < buttonTexts.Count; i++)
            {
                Button btn = new Button();
                btn.Content = buttonTexts[i].ToUpper();
                btn.Margin = new Thickness(4, 4, 4, 10);
                btn.Width = 120;
                btn.Height = 35;
                btn.Cursor = Cursors.Hand;
                btn.FontSize = 13;
                
                Func<bool> action = actions[i];
                btn.Click += (sender, e) => {
                    action();
                    Close();
                };

                ButtonPanel.Children.Add(btn);

            }
        }
    }
}
