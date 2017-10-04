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
    /// ///
    public class MainViewModel : ViewModelBase
    {
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand SelectedItemChangedCommand { get; set; }

        private SoundPlayer soundPlayer = new SoundPlayer();
        private ObservableCollection<Player> _songList = new ObservableCollection<Player>();
        public ObservableCollection<Player> SongList
        {
            get
            {
                return _songList;
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
            ClearCommand = new RelayCommand(ClearPlayerBeats);
            mainTimer = new System.Timers.Timer(60000 / Player.BPM);
            mainTimer.Start();
            mainTimer.Elapsed += new ElapsedEventHandler(PlayerPlaySound);
            LoadListSongs();
        }

        /// <summary>
        /// Clears the current song of all sounds
        /// </summary>
        public void ClearPlayerBeats()
        {
            this.Player.PlayInstruments.Clear();
        }

        /// <summary>
        /// Loads a player (song) into the current display
        /// </summary>
        /// <param name="obj"></param>
        public void Load(object obj)
        {
            Debug.WriteLine($"Loading player {(obj as Player).Name}");
            Player = (Player)obj;
            SliderValue = Player.BPM;
            RaisePropertyChanged("Player");
            RaisePropertyChanged("SliderValue");

        }

        /// <summary>
        /// Removes a sound from the current song
        /// </summary>
        /// <param name="obj"></param>
        private void Delete(object obj)
        {
            PlayInstrument removePlayInstrument = (PlayInstrument)obj;
            Debug.WriteLine($"DELETE {removePlayInstrument.Type} {removePlayInstrument.SoundType} - {removePlayInstrument.Position}");
            Player.PlayInstruments.Remove(removePlayInstrument);
        }

        /// <summary>
        /// Checks for sounds to play at the current position (beat) in the song.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Saves the current song to xml
        /// </summary>
        public void Save()
        {
            XDocument doc = XDocument.Load(@"../../Resources/songs.xml");

            RemoveSongXML(doc, Player.Name);

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
            SaveXML(doc);
        }

        /// <summary>
        /// Saves the song XML
        /// </summary>
        /// <param name="doc"></param>
        private void SaveXML(XDocument doc)
        {
            doc.Save(@"../../Resources/songs.xml");
            LoadListSongs();
            Debug.WriteLine("Saved");
        }

        /// <summary>
        /// Removes a song from the xml
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="removePlayerName"></param>
        private void RemoveSongXML(XDocument doc, string removePlayerName)
        {
            //Remove existing
            var playerElements = doc.Descendants().Where(x => x.Attribute("name") != null).Select(x => x.Attribute("name"));
            if (playerElements.Count() != 0 && playerElements.First() != null)
            {
                var playerElement = playerElements.FirstOrDefault(x => x.Value == removePlayerName);
                if (playerElement != null)
                {
                    playerElement.Parent.Remove();
                }
            }
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

        /// <summary>
        /// Adds a new sound to the current song
        /// </summary>
        /// <param name="playInstrument"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Reloads the list of songs
        /// </summary>
        public void LoadListSongs()
        {
            _songList.Clear();

            XDocument doc = XDocument.Load(@"../../Resources/songs.xml");
            var elements = doc.Root.Elements("player");

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

            if (_songList.FirstOrDefault(x => x.Name == Player.Name) != null)
            {
                Player = _songList.First(x => x.Name == Player.Name);
            }
        }

        /// <summary>
        /// Removes the selected song
        /// </summary>
        /// <param name="selectedItem"></param>
        internal void Remove(object selectedItem)
        {
            //if showing the deleted item - show default
            if ((selectedItem as Player) == Player)
            {
                Player = new Player()
                {
                    Name = "Default",
                    BPM = 120,
                    MaxPosition = 8
                };
            }

            XDocument doc = XDocument.Load(@"../../Resources/songs.xml");

            RemoveSongXML(doc, (selectedItem as Player).Name);

            SaveXML(doc);
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}