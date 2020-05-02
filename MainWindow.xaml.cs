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

        TextToSpeechClient _client;
        VoiceSelectionParams _voiceParms;
        AudioConfig _audioConfig;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGoogleEnvironmentVariable();
            InitializeVoice();
            InitializeKeyboardListener();
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

        private void InitializeVoice()
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
                Pitch = 0.0
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
                
                using (var waveOut = new WaveOut { Device = new WaveOutDevice(2) })
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
            if (!_currentKeyPress)
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
                    GoogleSay("I am Being Ganked");
                }

                if (e.KeyPressed == System.Windows.Input.Key.Right)
                {
                    GoogleSay("Last district");
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
        }

        private void txtSpeech_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                SpeakString_OnClick(null, null);
                ((TextBox)sender).Text = string.Empty;
            }
        }

        private void Speak_Content_OnClick(object sender, RoutedEventArgs e)
        {
            GoogleSay(((Button)sender).Content.ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _listener.UnHookKeyboard();
        }
    }
}