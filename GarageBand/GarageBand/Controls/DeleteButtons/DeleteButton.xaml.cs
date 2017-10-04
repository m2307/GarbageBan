using GarageBand.Model;
using GarageBand.ViewModel;
using SoundLib;
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

namespace GarageBand.Controls
{
    /// <summary>
    /// Interaction logic for SnareButton.xaml
    /// </summary>
    public partial class DeleteButton : UserControl
    {
        public DeleteButton(PlayInstrument playInstrument, Grid parentGrid, MainViewModel context, int rowPosition)
        {
            InitializeComponent();
            Uri uriSource = null; 

            switch (playInstrument.Type)
            {
                case InstrumentType.Snaar:
                    uriSource = new Uri(@"../../Assets/snare.png", UriKind.Relative);
                    break;
                case InstrumentType.Hihat:
                    uriSource = new Uri(@"../../Assets/hihat.png", UriKind.Relative);
                    break;
                case InstrumentType.Basskick:
                    uriSource = new Uri(@"../../Assets/kick.png", UriKind.Relative);
                    break;
                default:
                    break;
            }

            DeleteIcon.Source = new BitmapImage(uriSource);

            this.mainButton.Command = context.DeleteCommand;
            this.mainButton.CommandParameter = playInstrument;
            Sample.Text = "Sample " + playInstrument.SoundType;
            Grid.SetColumn(this, playInstrument.Position - 1);
            this.mainButton.Click += Button_Click;
            Grid.SetRow(this, rowPosition);

            parentGrid.Children.Add(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Grid).Children.Remove(this); 
        }
    }
}
