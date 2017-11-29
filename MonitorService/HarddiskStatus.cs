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
                    //The size of the disk is calculated as followed: TotalCylinder * TracksPerCylinder * SectorsPerTrack * BytesInEachSector
                    //So for a 512GB disk it is: 62260 * 255 * 63 * 512 = 512GB of disk storage total
                    TotalDiskSpaces = Convert.ToDouble(freeSpace["Size"]); //Get the property Size
                    TotalDiskSpaceGB = Math.Round(TotalDiskSpaces / 1000000000,0);
                    return TotalDiskSpaceGB;
                    //The return value will not match what windows task manager shows, due to reserved hidden system files.
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
                //Here we get the amount of free disk space of the total amount on the disk.
                //If you have a total disk space of 512GB, we get the used space of the 512GB here
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


