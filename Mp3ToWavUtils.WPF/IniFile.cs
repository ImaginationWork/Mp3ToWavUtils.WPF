using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;

namespace Mp3ToWavUtils.WPF
{
    class IniFile
    {
        string iniFilePath;
        string exeFileName = Assembly.GetExecutingAssembly().GetName().Name;

        [DllImport("kernel32")]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string IniPath = null)
        {
            iniFilePath = new FileInfo(IniPath ?? exeFileName + ".ini").FullName.ToString();
        }

        public string ReadKey(string Key, string Section = null)
        {
            var capLenth = 255;
            var RetVal = new StringBuilder(capLenth);
            GetPrivateProfileString(Section ?? exeFileName, Key, "", RetVal, capLenth, iniFilePath);
            return RetVal.ToString();
        }

        public void WriteKey(string Key, string Value, string Section = null)
        {
            WritePrivateProfileString(Section ?? exeFileName, Key, Value, iniFilePath);
        }

        public void DeleteKey(string Key, string Section = null)
        {
            WriteKey(Key, null, Section ?? exeFileName);
        }

        public void DeleteSection(string Section = null)
        {
            WriteKey(null, null, Section ?? exeFileName);
        }

        public bool IsKeyExists(string Key, string Section = null)
        {
            return ReadKey(Key, Section).Length > 0;
        }
    }
}
