using System.Windows;
using GarageBand.ViewModel;
using SoundLib;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;
using System;
using GarageBand.Controls;
using System.Windows.Data;
using System.Collections.Generic;
using GarageBand.Model;
using System.ComponentModel;

namespace GarageBand
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<TextBlock> beatColumns = new List<TextBlock>();

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
            (DataContext as MainViewModel).changeTextColor += OnColorChange;

            beatColumns.Clear();
            Beatgrid_Generate();

            SongListBox.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            //Create delete buttons
            foreach (PlayInstrument beat in (DataContext as MainViewModel).Player.PlayInstruments)
            {
                CreateDeleteButton(beat);
            }
        }

        private void OnColorChange(int position, Color color)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                beatColumns[position].Background = new SolidColorBrush(color);
            }));
        }

        private void Beatgrid_Generate()
        {
            beatGrid.Children.Clear();
            beatGrid = new Grid();
            LayoutRoot.Children.Add(beatGrid);
            beatGrid.Drop += LayoutRoot_Drop;
            beatGrid.DragEnter += LayoutRoot_DragEnter;

            Grid.SetRowSpan(beatGrid, 2);
            Grid.SetColumn(beatGrid, 1);

            RowDefinition r2 = new RowDefinition();
            r2.MinHeight = 40;
            beatGrid.RowDefinitions.Add(r2);


            for (int j = 0; j < Enum.GetNames(typeof(InstrumentType)).Length; j++)
            {
                RowDefinition r = new RowDefinition();
                r.Height = new GridLength(60, GridUnitType.Star);
                beatGrid.RowDefinitions.Add(r);
            }

            const int COLUMNWIDTH = 50;
            for (int i = 0; i < (DataContext as MainViewModel).Player.MaxPosition; i++)
            {
                ColumnDefinition c = new ColumnDefinition();
                c.Width = new GridLength(50, GridUnitType.Star);
                c.MinWidth = COLUMNWIDTH;
                beatGrid.ColumnDefinitions.Add(c);

                //Labels Beat
                TextBlock t = new TextBlock();
                t.MinWidth = 30;
                t.Text = (i + 1).ToString();
                t.VerticalAlignment = VerticalAlignment.Center;
                t.HorizontalAlignment = HorizontalAlignment.Center;
                t.FontSize = 16;
                t.VerticalAlignment = VerticalAlignment.Top;
                t.TextAlignment = TextAlignment.Center;
                t.Padding = new Thickness(5, 3, 5, 3);
                Grid.SetRow(t, 0);
                Grid.SetColumn(t, i);
                beatGrid.Children.Add(t);

                beatColumns.Add(t);

                for (int j = 0; j < Enum.GetNames(typeof(InstrumentType)).Length; j++)
                {
                    Border b = new Border();

                    TextBlock t2 = new TextBlock();

                    Style style = this.FindResource("BeatGridStyle1") as Style;
                    t2.Style = style;

                    b.Child = t2;
                    t2.Text = "       ";

                    Grid.SetRow(b, j + 1);
                    Grid.SetColumn(b, i);
                    beatGrid.Children.Add(b);
                }
            }
            beatGrid.MinWidth = (DataContext as MainViewModel).Player.MaxPosition * COLUMNWIDTH;
        }

        private void LayoutRoot_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PlayInstrument)) && e.Source is TextBlock)
            {
                PlayInstrument playInstrument = (PlayInstrument)e.Data.GetData(typeof(PlayInstrument));
                playInstrument.Position = Grid.GetColumn(((e.Source as TextBlock).Parent as UIElement)) + 1;

                if ((DataContext as MainViewModel).AddSound(playInstrument))
                {
                    CreateDeleteButton(playInstrument);
                }

            }
        }

        private void CreateDeleteButton(PlayInstrument playInstrument)
        {
            switch (playInstrument.Type)
            {
                case InstrumentType.Snaar:
                    new DeleteButton(playInstrument, beatGrid, DataContext as MainViewModel, 3);
                    break;
                case InstrumentType.Hihat:
                    new DeleteButton(playInstrument, beatGrid, DataContext as MainViewModel, 1);
                    break;
                case InstrumentType.Basskick:
                    new DeleteButton(playInstrument, beatGrid, DataContext as MainViewModel, 2);
                    break;
                default:
                    break;
            }
        }

        private void LayoutRoot_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(InstrumentType)) ||
                sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void SongListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SongListBox.SelectedItem != null)
            {
                (DataContext as MainViewModel).Load(SongListBox.SelectedItem);

                Beatgrid_Generate();

                //Create delete buttons
                foreach (PlayInstrument beat in (DataContext as MainViewModel).Player.PlayInstruments)
                {
                    CreateDeleteButton(beat);
                }
            }
        }

        private void Button_Click_Clear(object sender, RoutedEventArgs e)
        {
            (DataContext as MainViewModel).ClearPlayerBeats();
            Beatgrid_Generate();
        }

        private void Button_Click_RemoveSong(object sender, RoutedEventArgs e)
        {
            (DataContext as MainViewModel).Remove(SongListBox.SelectedItem);
            Beatgrid_Generate();
        }
    }
}