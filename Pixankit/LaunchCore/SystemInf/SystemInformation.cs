using System.Runtime.InteropServices;

namespace PixanKit.LaunchCore.SystemInf;

/// <summary>
/// Provides Information Like Operating System And Computer Architecture
/// </summary>
public static class SysInfo
{
    static SysInfo()
    {
        SetOs();
        SetArch();
    }

    private static void SetOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) OsName = "windows";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) OsName = "linux";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) OsName = "osx";
        else OsName = "others";
    }

    private static void SetArch()
    {
        CpuArch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x86_64",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => "other"
        };
    }

    /// <summary>
    /// Represents the name of the operating system<br/>
    /// windows: Windows7 -> Latest Windows<br/>
    /// linux: Ubuntu, Debian, Arch, ChromeOS, Fedora, RedHat...<br/>
    /// osx: OSX, MacOS 11 -> Latest MacOS<br/>
    /// unix: others<br/>
    /// </summary>
    public static string OsName { get; private set; } = "windows";

    /// <summary>
    /// Represents the architecture of the cpu
    /// <br/>
    /// x86: x86 i386<br/>
    /// x86_64: x86_64, AMD64<br/>
    /// arm64: Arm64 AArch64 Arm_v8
    /// arm: Arm A32
    /// </summary>
    public static string CpuArch { get; private set; } = "x86_64";

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string Shell()
    {
        if (OsName == "windows") return "cmd";
        return Environment.GetEnvironmentVariable("SHELL") ?? "/bin/bash";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <returns></returns>
    public static string GetVarCmd(string key, string val)
    {
        return OsName == "windows" ? 
            $"set {key}={val}" : 
            $"export {key}={val}";
    }
        
    /// <summary>
    /// Gets the available memory size in megabytes.
    /// </summary>
    /// <returns>
    /// A long value representing the available memory size in megabytes.
    /// </returns>
    public static long GetAvailableMemSize()
    {
        return Environment.WorkingSet / 1024 / 1024;
    }
}