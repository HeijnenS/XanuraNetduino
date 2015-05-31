using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using SecretLabs.NETMF.IO;
using Microsoft.SPOT;
using Microsoft.SPOT.IO;
using Domotica;

namespace mySecondtry
{
    static class Logging
    {
        /*
         *this logging class is resposible for writing to log files on storage
         *it should  
         *  check file exists
         *  create new if it doesn't
         *  check filesize, cannot be bigger then 10 MB
         *  check free space on disk, cannot be lower then 100 mb, delete old files if necessary
         *  write line to a text file
         */
        public static string Path = "";
        public static string fileType = "";
        private static bool debug = true;



        public static string LogPathAll()
        {
            DateTime datet = DateTime.Now;
            DirectoryInfo rootDirectory = new DirectoryInfo(@"\SD\"); //root dir of Netduino
            return rootDirectory + "\\Log " + "All" + " " + datet.ToString("MM_dd") + ".log";
        }

        public static void LogMessageToFile(String message, String caller)
        {
            DateTime datet = DateTime.Now;
            try
            {
                if (SD.VolumeExist())
                {
                    DirectoryInfo rootDirectory = new DirectoryInfo(@"\SD\"); //root dir of Netduino
                    String filePath = rootDirectory + "\\Log " + caller.ToUpper() + " " + datet.ToString("MM_dd") + ".log";
                    VolumeInfo vol = new VolumeInfo("SD");

                    if (vol.TotalFreeSpace < vol.TotalSize * .1)
                    {                        
                        DeleteOlderFiles();
                    }

                    if (!File.Exists(filePath))
                    {
                        CreateFile(filePath);
                    }
                    else
                    {
                        if (FileSize(filePath) > 10 * System.Math.Pow(10, 6))//check if file over 10 mb (10*10^6 bytes) then create version 2
                        {
                            FileInfo F = new FileInfo(filePath);
                            
                            File.Move(filePath, rootDirectory + "\\Log " + caller.ToUpper() + " " + datet.ToString("MM_dd_hh") + ".log");
                            // filePath = "Log " + fileType + " " + datet.ToString("MM_dd") + ".log";
                            // create a new consistant file name
                        }
                    }
                }
            }
            catch
            {
                return;
            }

            try
            {
                DirectoryInfo rootDirectory = new DirectoryInfo(@"\SD\"); //root dir of Netduino
                String filePath = rootDirectory + "\\Log " + caller + " " + datet.ToString("MM_dd") + ".log";
                using (var filestream = new FileStream(filePath, FileMode.Append))
                {
                    StreamWriter streamWriter = new StreamWriter(filestream);
                    streamWriter.WriteLine(datet.ToString("MM/dd hh:mm:ss") + "> " + message);
                    streamWriter.Close();
                    if (debug)
                    {
                        Debug.Print("Logging debug => " + message);                        
                    }
                }
            }
            catch
            {
                //throw new Exception("Error in Writing line to file" + e.Message);
                return;
            }
        }

        public static string ReadActiveLogFile()
        {
            DateTime datet = DateTime.Now;
            try
            {
                DirectoryInfo rootDirectory = new DirectoryInfo(@"\SD\"); //root dir of Netduino
                String filePath = rootDirectory + "\\Log " + "ALL" + " " + datet.ToString("MM_dd") + ".log";
                char[] buffer = new char[1000];
                using (var filestream = new FileStream(filePath, FileMode.Open))
                {
                    StreamReader streamReader = new StreamReader(filestream);
                    //int i = streamReader.ReadBlock(buffer, 0, buffer.Length);
                    string text = streamReader.ReadToEnd();
                    //.string.string text = new string(buffer);
                    return text;
                }
            }
            catch
            {
                //throw new Exception("Error in Writing line to file" + e.Message);
                return "";
            }
        }

        private static void CreateFile(string filePath)
        {
            try
            {
                if (SD.VolumeExist())
                {
                    using (var filestream = new FileStream(filePath, FileMode.Create))
                    {
                        StreamWriter streamWriter = new StreamWriter(filestream);
                        streamWriter.WriteLine("New file created");
                        streamWriter.Close();
                    }
                }
            }
            catch
            {
                //throw new Exception("Error in creating file" + e.Message);
                return;
            }
        }

        private static long FileSize(string filePath)
        {
            try
            {
                if (SD.VolumeExist())
                {
                    FileInfo info = new FileInfo(filePath);
                    return info.Length;
                }
                return 0;
            }
            catch
            {
                //throw new Exception("Requesting filesize failed" + e.Message);
                return 0;
            }
        }

        private static void  DeleteOlderFiles()
        {
            DirectoryInfo rootDirectory = new DirectoryInfo(@"\SD\");
            foreach (FileInfo F in rootDirectory.GetFiles())
            {
                if(F.CreationTime < (DateTime.Now.AddDays(-14)))
                {
                    Logging.LogMessageToFile("Deleting file : " + F.FullName, "ALL");
                    F.Delete();
                }
           
            }

        }

    }
}