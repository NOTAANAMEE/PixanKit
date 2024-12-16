using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace GPUSetter
{
    public static class GPUSettings
    {
        public static string[] GetGPUS()
        {
            List<string> GPUs = new List<string>();
            ManagementObjectSearcher searcher = new("select * from Win32_VideoController");
            foreach (ManagementObject obj in searcher.Get())
            {
                GPUs.Add(obj["name"].ToString());
            }
            return GPUs.ToArray();
        }
    }
}
