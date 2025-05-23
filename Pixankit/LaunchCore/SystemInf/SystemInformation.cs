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
        #if !NATIVE_AOT
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) OsName = "windows";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) OsName = "linux";
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) OsName = "osx";
        else OsName = "others";
        #else
        #if WINDOWS
        OsName = "windows";
        #elif LINUX
        OsName = "linux";
        #elif OSX
        OsName = "osx";
        #else
        OsName = "others";
        #endif
        #endif
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
    /// linux: Ubuntu, Debian, Arch, Fedora, RedHat...<br/>
    /// osx: OSX, macOS 11 -> Latest macOS <br/>
    /// unix, BSD: others<br/>
    /// Mojang does not officially support the Minecraft client
    /// running on operating systems other than Linux, macOS, and Windows.
    /// Running on other operating systems may cause serious
    /// compatibility issues
    /// </summary>
    public static string OsName { get; private set; } = "windows";

    /// <summary>
    /// Represents the architecture of the cpu
    /// <br/>
    /// x86: x86, i386<br/>
    /// x86_64: x86_64, AMD64<br/>
    /// arm64: Arm64, AArch64, Arm_v8<br/>
    /// arm: Arm, A32 <br/>
    /// Mojang does not officially
    /// support Minecraft client running on CPU architectures
    /// other than x86, amd64 and arm64
    /// <see href="https://www.minecraft.net/en-us/store/minecraft-deluxe-collection-pc?tabs=%7B%22details%22%3A1%7D#accordionv1-b6c8df09da-item-7739893325"/>
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