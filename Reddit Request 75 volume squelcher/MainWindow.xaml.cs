using NAudio.CoreAudioApi;
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

namespace Reddit_Request_75_volume_squelcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var deviceEnumerator = new MMDeviceEnumerator();
            foreach (var device in deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                cbDevices.Items.Add(device);
                if (device.ID == Properties.Settings.Default.DeviceId)
                    cbDevices.SelectedIndex = cbDevices.Items.Count-1;
            }
            cbDevices.SelectionChanged += CbDevices_SelectionChanged;

            tbHotkey.Text = Properties.Settings.Default.Hotkey;
            hotKeyList = HotkeyStringToList(Properties.Settings.Default.Hotkey);
            slideVolLow.Value = Properties.Settings.Default.VolLow;
            slideVolHigh.Value = Properties.Settings.Default.VolHigh;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                await Task.Delay(5);
                if (newHotkey && hotKeyList.Count>0&& hotKeyList.All(k => Keyboard.IsKeyDown(k)))
                {
                    this.Title = "hotkey down";
                    try
                    {
                        MMDevice device = (MMDevice)cbDevices.SelectedItem;
                        device.AudioEndpointVolume.MasterVolumeLevelScalar = (float)Properties.Settings.Default.VolLow;
                    }
                    catch{}
                }
                else if (newHotkey)
                {
                    this.Title = "hotkey up";
                    try
                    {
                        MMDevice device = (MMDevice)cbDevices.SelectedItem;
                        device.AudioEndpointVolume.MasterVolumeLevelScalar = (float)Properties.Settings.Default.VolHigh;
                    }
                    catch{}
                }
            }
        }

        List<Key> hotKeyList = new List<Key>();
        bool newHotkey = true;
        private void tbHotkey_KeyUp(object sender, KeyEventArgs e)
        {
            if (hotKeyList.All(k => Keyboard.IsKeyUp(k)))
            {
                newHotkey = true;
                slideVolLow.Focus();
                Properties.Settings.Default.Hotkey = HotkeyString(hotKeyList);
                Properties.Settings.Default.Save();
            }
        }

        private void tbHotkey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (newHotkey)
            {
                newHotkey = false;
                hotKeyList.Clear();
            }
            if (!hotKeyList.Contains(e.Key))
                hotKeyList.Add(e.Key);

            tbHotkey.Text = HotkeyString(hotKeyList);
            e.Handled = true;
        }

        string HotkeyString(List<Key> list)
        {
            string hkStr = "";
            foreach (var k in hotKeyList)
                hkStr += string.Format("{0}+", k);
            return hkStr.Trim('+'); ;
        }

        List<Key> HotkeyStringToList(string hkStr)
        {
            List<Key> hkList = new List<Key>();
            
            foreach (var kStr in hkStr.Split('+'))
                foreach (Key k in (Key[])Enum.GetValues(typeof(Key)))
                    if (k.ToString() == kStr)
                        hkList.Add(k);
            return hkList;
        }

        #region Settings Save
        private void slideVolLow_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Properties.Settings.Default.VolLow = e.NewValue;
            Properties.Settings.Default.Save();
        }

        private void slideVolHigh_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Properties.Settings.Default.VolHigh = e.NewValue;
            Properties.Settings.Default.Save();
        }

        private void CbDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MMDevice device = (MMDevice)cbDevices.SelectedItem;
            Properties.Settings.Default.DeviceId = device.ID;
            Properties.Settings.Default.Save();
        }
        #endregion
    }
}
