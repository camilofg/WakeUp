using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.Geolocator;
using System.Threading.Tasks;
using PCLStorage;
using static Phoneword.Data.ObjectData;
using System.IO;
using Newtonsoft.Json;
using Android.Content.PM;
using WakeUp.Data;

namespace WakeUp
{
    [Activity(Label = "WakeUp", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        private bool _initialH;
        const int TIME_DIALOG_ID = 0;

        private int hour;
        private int minute;

        private Context context;
        Button BtnConfig;
        Button BtnAlarms;
        ViewFlipper VF;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            context = this;
            Button getPositionBtn = FindViewById<Button>(Resource.Id.GetPositionBtn);
            TextView txtName = FindViewById<TextView>(Resource.Id.TxtName);
            TextView timeInitial = FindViewById<TextView>(Resource.Id.InitialHour);
            TextView timeFinish = FindViewById<TextView>(Resource.Id.FinishHour);

            BtnConfig = FindViewById<Button>(Resource.Id.BtnConfig);
            BtnAlarms = FindViewById<Button>(Resource.Id.BtnAlarms);
            VF = FindViewById<ViewFlipper>(Resource.Id.ViewFlipper01);
            //Get the current time
            hour = DateTime.Now.Hour;
            minute = DateTime.Now.Minute;

            UpdateDisplay(true);
            getPositionBtn.Click += GetPositionBtn_Click;

            VF.DisplayedChild = 0;

            BtnConfig.Click += BtnConfig_Click;
            BtnAlarms.Click += BtnAlarms_Click;

            // Add a click listener to the button
            timeInitial.Click += (o, e) => ShowDialog(TIME_DIALOG_ID);
            timeFinish.Click += (o, e) => ShowDialog(1);
            Button btnClean = FindViewById<Button>(Resource.Id.BtnClean);
            btnClean.Click += BtnClean_Click;

            Button BtnAlarm = FindViewById<Button>(Resource.Id.BtnAlarm);
            BtnAlarm.Click += delegate
            {
                var alarmIntent = new Intent(this, typeof(AlarmReceiver));
                alarmIntent.PutExtra("title", "Hello");
                alarmIntent.PutExtra("message", "World!");

                var pending = PendingIntent.GetBroadcast(this, 0, alarmIntent, PendingIntentFlags.UpdateCurrent);

                var alarmManager = GetSystemService(AlarmService).JavaCast<AlarmManager>();
                alarmManager.Set(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime() + 5 * 1000, pending);
                //alarmManager.Cancel(pending);
            };

            Button BtnRead = FindViewById<Button>(Resource.Id.BtnRead);
            BtnRead.Click += BtnRead_Click;
        }

        private void BtnRead_Click(object sender, EventArgs e)
        {
            string salida = "";
            var filename = GetFilePath();
            salida = ReadFile(filename, salida);
            FindViewById<TextView>(Resource.Id.TxtArchivo).Text = salida;
        }

        private void BtnClean_Click(object sender, EventArgs e)
        {
            string filePath = GetFilePath();
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.WriteAllText(filePath, "");
            }
        }


        private void BtnConfig_Click(object sender, EventArgs e)
        {
            VF.DisplayedChild = 0;
        }

        private void BtnAlarms_Click(object sender, EventArgs e)
        {
            VF.DisplayedChild = 1;
        }

        private static string GetFilePath()
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            return Path.Combine(path, "Positions.json");
        }

        private void TimePickerCallback(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            hour = e.HourOfDay;
            minute = e.Minute;
            UpdateDisplay(false);
        }

        protected override Dialog OnCreateDialog(int id)
        {
            // if (id == TIME_DIALOG_ID)
            if (id == 0)
                _initialH = true;

            else
                _initialH = false;

            return new TimePickerDialog(this, TimePickerCallback, hour, minute, false);
        }

        private void UpdateDisplay(bool init)
        {
            string time = string.Format("{0}:{1}", hour, minute.ToString().PadLeft(2, '0'));
            if (init)
                FindViewById<TextView>(Resource.Id.InitialHour).Text = FindViewById<TextView>(Resource.Id.FinishHour).Text = time;

            if (_initialH)
                FindViewById<TextView>(Resource.Id.InitialHour).Text = time;
            else
                FindViewById<TextView>(Resource.Id.FinishHour).Text = time;
        }

        private async void GetPositionBtn_Click(object sender, EventArgs e)
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 50;

            var position = await locator.GetPositionAsync(10000);
            Console.WriteLine("Position Status: {0}", position.Timestamp);

            var pos = new Ubication()
            {
                Name = FindViewById<EditText>(Resource.Id.TxtName).Text,
                Remember = FindViewById<EditText>(Resource.Id.TxtRemember).Text,
                HoraInicial = Convert.ToDateTime(FindViewById<TextView>(Resource.Id.InitialHour).Text),
                HoraFinal = Convert.ToDateTime(FindViewById<TextView>(Resource.Id.FinishHour).Text),
                Long = position.Longitude.ToString(),
                Lat = position.Latitude.ToString()
            };
            var json = JsonConvert.SerializeObject(pos, Newtonsoft.Json.Formatting.Indented);

            var test = WriteFile(json);
            FindViewById<TextView>(Resource.Id.TxtArchivo).Text = test;
        }

        private static string WriteFile(string json)
        {
            
            string salida = "";
            var filename = GetFilePath();
            salida = ReadFile(filename, salida);
            using (var streamWriter = new StreamWriter(filename, true))
            {
                streamWriter.WriteLine(json);
            }

            salida += ReadFile(filename, salida);
            //using (var streamReader = new StreamReader(filename))
            //{
            //    string content = streamReader.ReadToEnd();
            //    System.Diagnostics.Debug.WriteLine(content);
            //    salida += content;
            //}
            return salida;
        }

        private static string ReadFile(string filename, string salida)
        {
            if (File.Exists(filename))
            {
                using (var streamReader = new StreamReader(filename))
                {
                    string content = streamReader.ReadToEnd();
                    System.Diagnostics.Debug.WriteLine(content);
                    salida = content;
                }
            }

            return salida;
        }

        public void SetAlarm(Context context, int alertTime)
        {
            long now = SystemClock.CurrentThreadTimeMillis();
            AlarmManager am = (AlarmManager)context.GetSystemService(Context.AlarmService);
            Intent intent = new Intent(context, this.Class);
            PendingIntent pi = PendingIntent.GetBroadcast(context, 0, intent, 0);
            am.Set(AlarmType.RtcWakeup, now + ((long)(alertTime * 10000)), pi);
        }

    }
}

