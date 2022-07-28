using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fileindex
{
    class Program
    {
        public static string Directorypattern = string.Empty;
        static void Main(string[] args)

        {
            Console.WriteLine("Input search pattern:");
            string ACCDir = Console.ReadLine();
            string pattern = "*";
            List<string> allDirectories = new List<string> { ACCDir };
            Stack<string> directories = new Stack<string>(allDirectories);
            List<string[]> allFiles = new List<string[]>();
            Directorypattern = ACCDir;
            while (directories.Count > 0)
            {
                try
                {
                    long Filelength = 0;
                    long Incfilelength = 0;
                    int count = 0;
                    int TotalFilecount = 0;

                    string Accfilesize = "";
                    foreach (string dir in Directory.GetDirectories(directories.Pop()))
                    {
                        directories.Push(dir);
                        allDirectories.Add(dir);
                        try
                        {
                            allFiles.Add(Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly));
                            var CurrentFile = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);
                            Parallel.ForEach(CurrentFile, s =>
                            {
                                string fileName = s;
                                FileInfo fi = new FileInfo(fileName);
                                Filelength += fi.Length;
                                count++;
                            });

                            TotalFilecount += count;
                            Incfilelength += Filelength;
                            Accfilesize = FileSizeFormatter.FormatSize(Incfilelength);
                        }
                        catch (UnauthorizedAccessException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    if (directories.Count > 0 && count > 0)
                    {
                        Console.WriteLine(TotalFilecount + " Files in " + directories.Count + " Directory, " + Accfilesize + " Total Size");
                        QueryDuplicates(ACCDir);
                    }


                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
        static void QueryDuplicates(string ACCDir)
        {
            string startFolder = ACCDir;

            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(startFolder);

            // To get all folders under the specified path.  
            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            int charsToSkip = startFolder.Length;

            // var can be used for convenience with groups.  
            var queryDupNames =
                from file in fileList
                group file.FullName.Substring(charsToSkip) by file.Name into fileGroup
                where fileGroup.Count() > 1
                select fileGroup;

            // output one page at a time.  
            PageOutput<string, string>(queryDupNames);
        }

        private static void PageOutput<K, V>(IEnumerable<System.Linq.IGrouping<K, V>> groupByExtList)
        {
            bool goAgain = true;

            int numLines = Console.WindowHeight - 3;
            int inccount = 0;
            long Duplicatefilesize = 0;

            foreach (var filegroup in groupByExtList)
            {
                if (inccount == 0)
                {
                    Console.WriteLine("Identical files found");
                }
                inccount++;
                // To start a new extension at the top of a page.  
                int currentLine = 0;

                    do
                    {
                        Console.WriteLine("Filename = {0}", filegroup.Key.ToString() == String.Empty ? "[none]" : filegroup.Key.ToString());

                        var resultPage = filegroup.Skip(currentLine).Take(numLines);
                    long Filelength = 0;
                    string Accfilesize = "";
                    //Execute the resultPage query  
                    foreach (var fileName in resultPage)
                        {
                        FileInfo fi = new FileInfo(Directorypattern + fileName);
                        Filelength = fi.Length;
                        Accfilesize = FileSizeFormatter.FormatSize(Filelength);
                        Console.WriteLine("\t{0}",Accfilesize +"-"+ fileName);
                        }
                    Duplicatefilesize += Filelength;


                    // Increment the line counter.  
                    currentLine += numLines;
                    } while (currentLine < filegroup.Count());

                    if (goAgain == false)
                        break;
                
            }
          
            if (inccount>0)
            {
                string Accduplicatefilesize = FileSizeFormatter.FormatSize(Duplicatefilesize);
                Console.WriteLine("Total Identical files " + inccount + "(Unique) Total size-" + Accduplicatefilesize);
            }
            else
            {
                Console.WriteLine("No Identical files found");
            }
        }


        public static class FileSizeFormatter
        {
            // Load all suffixes in an array  
            static readonly string[] suffixes =
            { "Bytes", "KB", "MB", "GB", "TB", "PB" };
            public static string FormatSize(Int64 bytes)
            {
                int counter = 0;
                decimal number = (decimal)bytes;
                while (Math.Round(number / 1024) >= 1)
                {
                    number = number / 1024;
                    counter++;
                }
                return string.Format("{0:n1}{1}", number, suffixes[counter]);
            }
        }
    }
}
