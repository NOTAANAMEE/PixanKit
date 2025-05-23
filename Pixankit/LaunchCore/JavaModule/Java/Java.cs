using Newtonsoft.Json.Linq;
using PixanKit.LaunchCore.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PixanKit.LaunchCore.JavaModule.Java;

/// <summary>
/// The JavaRuntime Class To Choose suitable Java runtime
/// </summary>
public partial class JavaRuntime : IToJson
{
    /// <summary>
    /// From Java 8 to Java 11 or later
    /// </summary>
    public ushort Version => _version;

    /// <summary>
    /// The directory of Java
    /// </summary>
    public string JavaFolder => _javaFolder;

    /// <summary>
    /// The binary folder of Java
    /// </summary>
    public string BinaryFolder => JavaFolder + "/bin";

    /// <summary>
    /// The path of java.exe
    /// </summary>
    public string JavaExe => BinaryFolder + "/java.exe";

    /// <summary>
    /// The path of javaw.exe
    /// </summary>
    public string JavawExe => BinaryFolder + "/javaw.exe";

    private string _javaFolder = "";

    private ushort _version;

    /// <summary>
    /// Init a JavaRuntime instance. It will automatically run Java and get the version
    /// </summary>
    /// <param name="javafolder">C:\Program Files\Java\JDK-21\</param>
    public JavaRuntime(string javafolder)
    {
        _javaFolder = javafolder;
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
        LoadFromJson(jData);
    }

    private void GetVersion()
    {
        if (!File.Exists(JavaExe)) throw new FileNotFoundException($"File Not Found. Target File:{JavaExe}");
        ProcessStartInfo processStartInfo = new()
        {
            FileName = JavaExe,
            Arguments = "-version",
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        var p = Process.Start(processStartInfo);
        if (p == null) return;
        var versionInf = "";
        while (!p.HasExited)
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
         *Java HotSpot(TM) 64-Bit Server VM (build 25.421-b09, mixed mode) => That's Java 8
         */

        _version = VersionParse(versionInf);
    }

    private static ushort VersionParse(string versionInf)
    {
        var pattern = "\"([^\"]*)\"";
        var match = Regex.Match(versionInf, pattern);
        var result = match.Groups[1].Value;
        var version = result.Split(".");
        if (version[0] == "1") return ushort.Parse(version[1]);
        else return ushort.Parse(version[0]);
    }
}