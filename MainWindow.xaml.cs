using CSCore;
using CSCore.MediaFoundation;
using CSCore.SoundOut;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Google.Cloud.TextToSpeech.V1;
using CSCore.CoreAudioAPI;

namespace TTS_Companion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        private LowLevelKeyboardListener _listener;
        private bool _currentKeyPress;
        private int _waveOutDeviceID;

        TTSSettings _settings;
        TextToSpeechClient _client;
        VoiceSelectionParams _voiceParms;
        AudioConfig _audioConfig;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGoogleEnvironmentVariable();
            InitializeSettings();
            InitializeDefaultVoice();
            InitializePlaybackDevices();
            InitializeKeyboardListener();
        }

        public void InitializeSettings()
        {
            _waveOutDeviceID = 0;
            _settings = new TTSSettings();
            try
            {
                _settings = TTSSettings.Read();
                _waveOutDeviceID = _settings.AudioDeviceID;
                SetKeybindsEnabledBold();
            }
            catch { }
        }

        public void InitializePlaybackDevices()
        {
            foreach(WaveOutDevice waveOutDevice in WaveOutDevice.EnumerateDevices())
            {
                MenuItem item = new MenuItem()
                {
                    Header = waveOutDevice.Name,
                    ToolTip = waveOutDevice.DeviceId

                };
                item.Click += new RoutedEventHandler(menu_DeviceList_Click);

                menuPlayback.Items.Add(item);
            }
        }

        private void menu_DeviceList_Click(Object sender, RoutedEventArgs e)
        {
            MenuItem thisItem = (MenuItem)sender;
            _waveOutDeviceID = (int)thisItem.ToolTip;
            _settings.AudioDeviceFriendlyName = thisItem.Header.ToString();
            _settings.AudioDeviceID = (int)thisItem.ToolTip;
            _settings.Save();
        }

        private void menuKeybindEnable(Object sender, RoutedEventArgs e)
        {
            if(_settings.KeybindsEnabled)
            {
                _settings.KeybindsEnabled = false;
            } else
            {
                _settings.KeybindsEnabled = true;
            }
            SetKeybindsEnabledBold();
            _settings.Save();
        }

        private void menu_DefaultVoice_Click(Object sender, RoutedEventArgs e)
        {
            InitializeDefaultVoice();
        }

        private void menu_OtherVoice_Click(Object sender, RoutedEventArgs e)
        {
            MenuItem thisItem = (MenuItem)sender;
            InitiaizeAlternativeVoice(thisItem.Header.ToString());
        }

        private void SetKeybindsEnabledBold()
        {
            if(_settings.KeybindsEnabled)
            {
                menuKeybindEnabled.FontWeight = FontWeights.UltraBold;
            } else
            {
                menuKeybindEnabled.FontWeight = FontWeights.Normal;
            }
        }

        private void InitializeGoogleEnvironmentVariable()
        {
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\\Users\\mikeb\\google_TTS_key.json");
        }

        private void InitializeKeyboardListener()
        {
            _currentKeyPress = false;
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += GlobalKeyboard_OnKeyPressed;
            _listener.HookKeyboard();
        }

        private void InitializeDefaultVoice()
        {
            _client = TextToSpeechClient.Create();

            // Build the voice request.
            _voiceParms = new VoiceSelectionParams
            {
                LanguageCode = "en-US",
                SsmlGender = SsmlVoiceGender.Male
            };

            // Specify the type of audio file.
            _audioConfig = new AudioConfig
            {
                AudioEncoding = Google.Cloud.TextToSpeech.V1.AudioEncoding.Mp3,
                Pitch = -5.0
            };
        }

        private void InitiaizeAlternativeVoice(string Tooltip)
        {
            _client = TextToSpeechClient.Create();

            switch (Tooltip)
            {

                case "Asian - F":
                    _voiceParms = new VoiceSelectionParams
                    {
                        LanguageCode = "ja-JP",
                        SsmlGender = SsmlVoiceGender.Female
                    };
                    break;
                case "Asian - M":
                    _voiceParms = new VoiceSelectionParams
                    {
                        LanguageCode = "ja-JP",
                        SsmlGender = SsmlVoiceGender.Male
                    };
                    break;
                case "Aussie - F":
                    _voiceParms = new VoiceSelectionParams
                    {
                        LanguageCode = "en-AU",
                        SsmlGender = SsmlVoiceGender.Female
                    };
                    break;
                case "Aussie - M":
                    _voiceParms = new VoiceSelectionParams
                    {
                        LanguageCode = "en-AU",
                        SsmlGender = SsmlVoiceGender.Male
                    };
                    break;
                case "British - F":
                    _voiceParms = new VoiceSelectionParams
                    {
                        LanguageCode = "en-GB",
                        SsmlGender = SsmlVoiceGender.Female
                    };
                    break;
                case "British - M":
                    _voiceParms = new VoiceSelectionParams
                    {
                        LanguageCode = "en-GB",
                        SsmlGender = SsmlVoiceGender.Male
                    };
                    break;

            }

            // Specify the type of audio file.
            _audioConfig = new AudioConfig
            {
                AudioEncoding = Google.Cloud.TextToSpeech.V1.AudioEncoding.Mp3,
                Pitch = 0
            };

        }

        private void GoogleSay(string message)
        {
            using (var stream = new MemoryStream())
            {
                
                // The input to be synthesized, can be provided as text or SSML.
                var input = new SynthesisInput
                {
                    Text = message
                };

                // Perform the text-to-speech request.
                var response = _client.SynthesizeSpeech(input, _voiceParms, _audioConfig);

                response.AudioContent.WriteTo(stream);
                
                using (var waveOut = new WaveOut { Device = new WaveOutDevice(_waveOutDeviceID) })
                using (var waveSource = new MediaFoundationDecoder(stream))
                {
                    waveOut.Initialize(waveSource);
                    waveOut.Play();
                    waveOut.WaitForStopped();
                }
            }
        }

        private void GlobalKeyboard_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if (!_currentKeyPress && _settings.KeybindsEnabled)
            {
                _currentKeyPress = true;
                if (e.KeyPressed == System.Windows.Input.Key.Up)
                {
                    GoogleSay("Yes");
                }

                if (e.KeyPressed == System.Windows.Input.Key.Down)
                {
                    GoogleSay("No");
                }

                if (e.KeyPressed == System.Windows.Input.Key.Left)
                {
                    GoogleSay("My Barrier");
                }

                if (e.KeyPressed == System.Windows.Input.Key.Right)
                {
                    GoogleSay("Door is hot");
                }

                /*
                if(e.KeyPressed == System.Windows.Input.Key.Oem5) // Backslash
                {
                    Keyboard.Focus(txtSpeech);
                }*/
                _currentKeyPress = false;
            }
        }

        private void SpeakString_OnClick(object sender, RoutedEventArgs e)
        {
            GoogleSay(txtSpeech.Text);
            txtSpeech.Text = string.Empty;
        }

        private void txtSpeech_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                SpeakString_OnClick(null, null);
            }

            if(e.Key == System.Windows.Input.Key.Escape)
            {
                txtSpeech.Text = string.Empty;
            }
        }

        private void Speak_Content_OnClick(object sender, RoutedEventArgs e)
        {
            string message = ((Button)sender).Content.ToString();
            if (message != string.Empty)
            {
                GoogleSay(message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _listener.UnHookKeyboard();
            }
            catch { }
        }
    }
}