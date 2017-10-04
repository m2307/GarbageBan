using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarageBand.Model
{
    public class Player : ObservableObject
    {
        public Player()
        {
            Position = 0;
            PlayInstruments = new ObservableCollection<PlayInstrument>();
        }

        public string Name { get; set; }

        public int BPM { get; set; }
        public ObservableCollection<PlayInstrument> PlayInstruments { get; set; }

        public byte Position { get; set; }

        public int MaxPosition { get; set; }

        public void IncrementPosition()
        {
            Position++;

            if (Position >= MaxPosition)
            {
                Position = 0;
            }

            RaisePropertyChanged("Position");
        }

        /// <summary>
        /// Temporary bugfix -->  use displaymember Name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
