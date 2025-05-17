using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixanKit.LaunchCore.GameModule.Game;

public struct GameSettings()
{
    public readonly bool IsOverall = false;

    public SettingValue JavaSetting;

    public SettingValue RunningFolderSetting;

    public SettingValue JVMArgSetting;

    public SettingValue PreArgSetting;

    public SettingValue PostArgSetting;

    public string CustomJavaSetting;    

    public string CustomRunningFolderSetting;

    public string Description;

    public string CustomJVMArgSetting;
    
    public string CustomPreArgSetting;

    public string CustomPostArgSetting;


}

public enum SettingValue
{
    Overall,
    Custom,
    
    Self,

    Latest,
    Closest,
    Specified
}
