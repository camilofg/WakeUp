using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Phoneword.Data
{
    public class ObjectData
    {
        public class Ubication{
            public string Name { get; set; }
            public string Remember { get; set; }
            public DateTime HoraInicial { get; set; }
            public DateTime HoraFinal { get; set; }
            public string Long { get; set; }
            public string Lat { get; set; }
        }
    }
}