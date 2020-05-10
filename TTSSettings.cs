using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TTS_Companion
{
    public class TTSSettings
    {
        private const string FILENAME = "tts_settings.xml";
        public int AudioDeviceID { get; set; }
        public string AudioDeviceFriendlyName { get; set; }
        public bool KeybindsEnabled { get; set; }
        public void Save()
        {
            using (StreamWriter sw = new StreamWriter(FILENAME))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(TTSSettings));
                xmls.Serialize(sw, this);
            }
        }
        public static TTSSettings Read()
        {
            using (StreamReader sw = new StreamReader(FILENAME))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(TTSSettings));
                return xmls.Deserialize(sw) as TTSSettings;
            }
        }
    }
}
