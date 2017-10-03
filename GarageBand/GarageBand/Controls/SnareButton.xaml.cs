using GarageBand.Model;
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
    public partial class SnareButton : UserControl
    {
        public SnareButton()
        {
            InitializeComponent();
        }

        private string selectedInstrument = "1";

        private void Snare_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragDrop.DoDragDrop(new ListView(), PlayInstrument.CreateDataObject(InstrumentType.Snaar, selectedInstrument), DragDropEffects.Move);
        }

        private void MenuItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            selectedInstrument = (e.Source as MenuItem).Tag.ToString();
            Sample.Text = $"Sample {selectedInstrument}";

        }
    }
}
