using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonitorService
{
    public static class HarddiskStatus
    {
        public static double TotalDiskSpace()
        {
            try
            {
                ObjectQuery TotalAmountDiskSpace = new ObjectQuery("SELECT * FROM Win32_DiskDrive");
                ManagementObjectSearcher searcherTotalAmountDisk = new ManagementObjectSearcher(TotalAmountDiskSpace);
                ManagementObjectCollection resultTotalAmountDiskSpace = searcherTotalAmountDisk.Get();
                double TotalDiskSpaces = 0;
                double TotalDiskSpaceGB = 0;
                foreach (var freeSpace in resultTotalAmountDiskSpace)
                {
                    TotalDiskSpaces = Convert.ToDouble(freeSpace["Size"]); //Get the property Size
                    TotalDiskSpaceGB = Math.Round(TotalDiskSpaces / 1000000000,0);
                    return TotalDiskSpaceGB;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Program.log.Error("Network read error: " + e);
            }
            return 0;
        }

        public static double UsedDiskSpace()
        {
            try
            {
                ObjectQuery FreeSpaceOnDisk = new ObjectQuery("SELECT * FROM Win32_LogicalDisk");
                ManagementObjectSearcher searcherFreeDiskSpace = new ManagementObjectSearcher(FreeSpaceOnDisk);
                ManagementObjectCollection resultFreeDiskSpace = searcherFreeDiskSpace.Get();
                double TotalFreeDiskSpace = 0;
                double UsedGBDiskSpace = 0;
                foreach (var freeSpace in resultFreeDiskSpace)
                {
                    TotalFreeDiskSpace = Convert.ToDouble(freeSpace["FreeSpace"]); //Get the property FreeSpace
                    UsedGBDiskSpace = Math.Round(TotalFreeDiskSpace/1000000000,0);
                    break;
                }
                return UsedGBDiskSpace;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Program.log.Error("Network read error: " + e);
            }
            return 0;
        }
    }
}


