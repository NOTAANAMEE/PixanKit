using PixanKit.LaunchCore.SystemInf;
using PixanKit.LaunchCore.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PixanKit.LaunchCore.JavaModule.Java
{
    /// <summary>
    /// The JavaRuntime Class To Choose suitable Java runtime
    /// </summary>
    public class JavaRuntime:IToJSON
    {
        /// <summary>
        /// From Java 8 to Java 11 or later
        /// </summary>
        public ushort Version
        {
            get => _version;
        }

        /// <summary>
        /// The directory of Java
        /// </summary>
        public string JavaFolder
        {
            get => _javaFolder;
        }

        /// <summary>
        /// The binary folder of Java
        /// </summary>
        public string BinaryFolder
        {
            get => JavaFolder + "/bin";
        }

        /// <summary>
        /// The path of java.exe
        /// </summary>
        public string JavaEXE 
        { 
            get => BinaryFolder + "/java.exe"; 
        }

        /// <summary>
        /// The path of javaw.exe
        /// </summary>
        public string JavawEXE 
        { 
            get => BinaryFolder + "/javaw.exe"; 
        }

        private string _javaFolder = "";

        private ushort _version;

        /// <summary>
        /// Init a JavaRuntime instance. It will automatically run Java and get the version
        /// </summary>
        /// <param name="javafolder">C:\Program Files\Java\JDK-21\</param>
        public JavaRuntime(string javafolder) 
        {
            _javaFolder = Localize.DeLocalize(javafolder);
            GetVersion();
        }

        /// <summary>
        /// Init a JavaRuntime instance with Directory And Version
        /// </summary>
        /// <param name="javafolder">Directory Of /bin/java.exe</param>
        /// <param name="version">Java Version (8-23)</param>
        public JavaRuntime(string javafolder, ushort version)
        {
            _javaFolder = javafolder;
            _version = version;
        }

        /// <summary>
        /// Init A JavaRuntime Instance With JSON Data
        /// </summary>
        /// <param name="jData"></param>
        public JavaRuntime(JObject jData)
        {
            _javaFolder = jData["path"].ToString();
            _version = (ushort)jData["version"];
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public JObject ToJSON()
        {
            return new JObject()
            {
                { "path", _javaFolder },
                { "version", _version }, 
            };
        }

        private void GetVersion()
        {
            if (!File.Exists(JavaEXE)) throw new FileNotFoundException($"File Not Found. Target File:{JavaEXE}");
            ProcessStartInfo processStartInfo = new()
            {
                FileName = Localize.PathLocalize(JavaEXE),
                Arguments = "-version",
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            Process? p = Process.Start(processStartInfo);
            if (p == null) return;
            string versionInf = "";
            while (!p.HasExited && p.StandardError != null)
            {
                versionInf += p.StandardError.ReadLine();
            }
            p.WaitForExit();
            /*Output Will Be Like:
             *java 17.0.11 2024-04-16 LTS
             *Java(TM) SE Runtime Environment (build 17.0.11+7-LTS-207)
             *Java HotSpot(TM) 64-Bit Server VM (build 17.0.11+7-LTS-207, mixed mode, sharing)
             *java version "1.8.0_421"
             *Java(TM) SE Runtime Environment (build 1.8.0_421-b09)
             (Java HotSpot(TM) 64-Bit Server VM (build 25.421-b09, mixed mode) => Fuck You Java 8
             */

            _version = VersionParse(versionInf);
        }

        private static ushort VersionParse(string versionInf)
        {
            string pattern = "\"([^\"]*)\"";
            Match match = Regex.Match(versionInf, pattern);
            string result = match.Groups[1].Value;
            string[] version = result.Split(".");
            if (version[0] == "1") return ushort.Parse(version[1]);
            else return ushort.Parse(version[0]);
        }
    }
}
