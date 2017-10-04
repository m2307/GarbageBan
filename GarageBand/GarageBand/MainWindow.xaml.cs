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
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();

            Beatgrid_Generate();

            SongListBox.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            //Create delete buttons
            foreach (PlayInstrument beat in (DataContext as MainViewModel).Player.PlayInstruments)
            {
                CreateDeleteButton(beat);
            }
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
            beatGrid.Margin = new Thickness(20);
            beatGrid.MinHeight = 300;

            const int COLUMNWIDTH = 50;
            for (int i = 0; i < (DataContext as MainViewModel).Player.MaxPosition; i++)
            {
                //Columndefinitions
                ColumnDefinition c = new ColumnDefinition();
                c.Width = new GridLength(50, GridUnitType.Star);
                c.MinWidth = COLUMNWIDTH;
                beatGrid.ColumnDefinitions.Add(c);

                RowDefinition r2 = new RowDefinition();
                r2.MinHeight = 40;
                beatGrid.RowDefinitions.Add(r2);

                //Labels Beat
                TextBlock t = new TextBlock();
                t.Width = 30;
                t.Text = (i + 1).ToString();
                t.VerticalAlignment = VerticalAlignment.Center;
                t.HorizontalAlignment = HorizontalAlignment.Center;
                t.FontSize = 16;
                t.VerticalAlignment = VerticalAlignment.Top;
                Grid.SetRow(t, 0);
                Grid.SetColumn(t, i);
                beatGrid.Children.Add(t);

                for (int j = 0; j < Enum.GetNames(typeof(InstrumentType)).Length; j++)
                {
                    RowDefinition r = new RowDefinition();
                    r.Height = new GridLength(60, GridUnitType.Star);
                    r.MinHeight = 60;
                    beatGrid.RowDefinitions.Add(r);

                    Border b = new Border();

                    TextBlock t2 = new TextBlock();
                    if (j % 2 == 0)
                    {
                        t2.Background = new SolidColorBrush(Colors.Gray);
                    }
                    else
                    {
                        t2.Background = new SolidColorBrush(Colors.DarkGray);
                    }

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
            //Add delete button
            Button b = new Button();
            b.Command = (DataContext as MainViewModel).DeleteCommand;
            b.CommandParameter = playInstrument;
            b.Content = "Delete\nSample " + playInstrument.SoundType;
            b.VerticalContentAlignment = VerticalAlignment.Center;
            b.HorizontalContentAlignment = HorizontalAlignment.Center;
            Grid.SetColumn(b, playInstrument.Position - 1);
            b.Click += Button_Click;

            switch (playInstrument.Type)
            {
                case InstrumentType.Snaar:
                    Grid.SetRow(b, 3); //fix
                    break;
                case InstrumentType.Hihat:
                    Grid.SetRow(b, 1);
                    break;
                case InstrumentType.Basskick:
                    Grid.SetRow(b, 2);
                    break;
                default:
                    break;
            }
            beatGrid.Children.Add(b);
        }

        private void LayoutRoot_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(InstrumentType)) ||
                sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            beatGrid.Children.Remove(e.Source as Button);
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Beatgrid_Generate();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            (DataContext as MainViewModel).Remove(SongListBox.SelectedItem);
            Beatgrid_Generate();
        }
    }
}