using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GarageBand.Model;
using SoundLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using System.Xml.Linq;

namespace GarageBand.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SelectedItemChangedCommand { get; set; }
        private SoundPlayer soundPlayer = new SoundPlayer();
        private ObservableCollection<Player> _songList = new ObservableCollection<Player>();
        public ObservableCollection<Player> SongList
        {
            get
            {
                return _songList.OrderBy(x=>x.Name);
            }
            set
            {
                if (_songList != value)
                {
                    _songList = value;
                    RaisePropertyChanged("SongList");
                }
            }
        }
        System.Timers.Timer mainTimer;
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            SaveCommand = new RelayCommand(Save);
            DeleteCommand = new RelayCommand<object>(Delete);
            SelectedItemChangedCommand = new RelayCommand<object>(Load);
            mainTimer = new System.Timers.Timer(60000 / Player.BPM);
            mainTimer.Start();
            mainTimer.Elapsed += new ElapsedEventHandler(PlayerPlaySound);
            LoadListSongs();
        }

        public void Load(object obj)
        {
            Debug.WriteLine($"Loading player {(obj as Player).Name}");
            Player = (Player)obj;
            SliderValue = Player.BPM;
            RaisePropertyChanged("Player");
            RaisePropertyChanged("SliderValue");

        }

        private void Delete(object obj)
        {
            PlayInstrument removePlayInstrument = (PlayInstrument)obj;
            Debug.WriteLine($"DELETE {removePlayInstrument.Type} {removePlayInstrument.SoundType} - {removePlayInstrument.Position}");
            Player.PlayInstruments.Remove(removePlayInstrument);
        }

        public void PlayerPlaySound(object source, ElapsedEventArgs e)
        {
            Player.IncrementPosition();
            Debug.WriteLine("---------------");
            Debug.WriteLine($"PLAYSOUND {Player.Position}");

            var toPlay = Player.PlayInstruments.Where(x => x.Position == Player.Position);

            foreach (var item in toPlay)
            {
                Debug.WriteLine(item.Type.ToString() + " " + item.SoundType);
                soundPlayer.PlaySound(item.Type, item.SoundType);
            }
        }

        public void Save()
        {
            XDocument doc = XDocument.Load(@"../../Resources/songs.xml");

            //Remove existing
            var playerElements = doc.Descendants().Where(x => x.Attribute("name") != null).Select(x => x.Attribute("name"));
            if (playerElements.Count() != 0 && playerElements.First() != null)
            {
                var playerElement = playerElements.FirstOrDefault(x => x.Value == Player.Name);
                if (playerElement != null)
                {
                    playerElement.Parent.Remove();
                }
            }

            XElement pElement = new XElement("player",
                new XAttribute("name", Player.Name),
                new XAttribute("bpm", Player.BPM),
                new XAttribute("positions", Player.MaxPosition));

            foreach (var sound in Player.PlayInstruments)
            {
                XElement sElement = new XElement("sound",
                    new XAttribute("position", sound.Position),
                    new XAttribute("type", sound.Type),
                    new XAttribute("soundtype", sound.SoundType));
                pElement.Add(sElement);
            }

            doc.Root.Add(pElement);
            doc.Save(@"../../Resources/songs.xml");

            LoadListSongs();

            Debug.WriteLine("Saved");
        }

        private int _sliderValue = 120;

        public int SliderValue
        {
            get { return _sliderValue; }
            set
            {
                RaisePropertyChanged("Slider");
                _sliderValue = value;
                Player.BPM = _sliderValue;
                mainTimer.Interval = 60000 / Player.BPM;
                BPMText = "BPM: " + value;
            }
        }

        private string _bpmText = "BPM: 120";

        public string BPMText
        {
            get { return _bpmText; }
            set
            {
                _bpmText = value;
                RaisePropertyChanged("BPMText");
            }
        }

        private Player _player = new Player()
        {
            Name = "Default",
            BPM = 120,
            MaxPosition = 8
        };

        public Player Player
        {
            get { return _player; }
            set
            {
                RaisePropertyChanged("Player");

                mainTimer.Interval = 60000 / Player.BPM;
                _player = value;
            }
        }

        public bool AddSound(PlayInstrument playInstrument)
        {
            Debug.WriteLine($"NEW SOUND: {playInstrument.Type.ToString()} ; POSITION: {playInstrument.Position}");

            //Check same position/type
            if (Player.PlayInstruments.Any(x => x.Position == playInstrument.Position && x.Type == playInstrument.Type))
            {
                return false;
            }

            Player.PlayInstruments.Add(playInstrument);
            RaisePropertyChanged("Player");

            return true;
        }

        public void LoadListSongs()
        {
            _songList.Clear();

            XDocument doc = XDocument.Load(@"../../Resources/songs.xml");
            var elements = doc.Root.Elements("player").Where(x => x.Attribute("name").Value != Player.Name);

            _songList.Add(Player);

            foreach (var element in elements)
            {
                var p = new Player()
                {
                    Name = element.Attribute("name").Value,
                    BPM = int.Parse(element.Attribute("bpm").Value),
                    MaxPosition = int.Parse(element.Attribute("positions").Value)
                };

                foreach (var soundElement in element.Elements("sound"))
                {
                    p.PlayInstruments.Add(new PlayInstrument()
                    {
                        Position = int.Parse(soundElement.Attribute("position").Value),
                        SoundType = soundElement.Attribute("soundtype").Value,
                        Type = (InstrumentType)Enum.Parse(typeof(InstrumentType), soundElement.Attribute("type").Value)
                    });
                }
                _songList.Add(p);
            }

            _songList.OrderBy(x => x.Name);
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}