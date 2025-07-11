﻿namespace PixanKit.LaunchCore.SystemInf;

internal static class Localize
{
    public static string CpLocalize(string classpathArg)
    {
        return SysInfo.OsName switch
        {
            "windows" => classpathArg.Replace("${classpath_separator}", ";"),
            _ => classpathArg.Replace("${classpath_separator}", ":")
        };
    }

    public static string LocalParser = "${classpath_separator}";
}