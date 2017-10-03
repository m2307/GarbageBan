using GalaSoft.MvvmLight;
using SoundLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GarageBand.Model
{
    public class PlayInstrument : ObservableObject
    {
        public int Position { get; set; }
        public InstrumentType Type { get; set; }

        public string SoundType { get; set; }

        public static DataObject CreateDataObject(InstrumentType type, string soundType)
        {
            PlayInstrument play = new PlayInstrument() { SoundType = soundType, Type = type };
            return new DataObject(play);
        }
    }
}
