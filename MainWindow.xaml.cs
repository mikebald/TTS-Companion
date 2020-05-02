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

namespace TTS_Companion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();
        private LowLevelKeyboardListener _listener;

        public MainWindow()
        {
            InitializeComponent();
            InitializeVoice();
            InitializeKeyboardListener();
        }

        private void InitializeKeyboardListener()
        {
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += GlobalKeyboard_OnKeyPressed;

            _listener.HookKeyboard();
        }

        private void InitializeVoice()
        {
            _speechSynthesizer.SelectVoiceByHints(VoiceGender.Male);
            _speechSynthesizer.Rate = 1;
            _speechSynthesizer.Volume = 60;
        }

        private void GlobalKeyboard_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            if(e.KeyPressed == System.Windows.Input.Key.Up)
            {
                Speak("Yes");
            }

            if (e.KeyPressed == System.Windows.Input.Key.Down)
            {
                Speak("No");
            }

            if (e.KeyPressed == System.Windows.Input.Key.Left)
            {
                Speak("Being Ganked");
            }

            if (e.KeyPressed == System.Windows.Input.Key.Right)
            {
                Speak("Last district");
            }

            /*
            if(e.KeyPressed == System.Windows.Input.Key.Oem5) // Backslash
            {
                Keyboard.Focus(txtSpeech);
            }*/
        }

        private void SpeakString_OnClick(object sender, RoutedEventArgs e)
        {
            Speak(txtSpeech.Text);
        }

        private void txtSpeech_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                SpeakString_OnClick(null, null);
                ((TextBox)sender).Text = string.Empty;
            }
        }

        private void Speak(string message)
        {
            using (var stream = new MemoryStream())
            {
                _speechSynthesizer.SetOutputToWaveStream(stream);
                _speechSynthesizer.Speak(message);

                using (var waveOut = new WaveOut { Device = new WaveOutDevice(2) })
                using (var waveSource = new MediaFoundationDecoder(stream))
                {
                    waveOut.Initialize(waveSource);
                    waveOut.Play();
                    waveOut.WaitForStopped();
                }
            }
        }

        private void Speak_Content_OnClick(object sender, RoutedEventArgs e)
        {
            Speak(((Button)sender).Content.ToString());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _listener.UnHookKeyboard();
        }
    }
}