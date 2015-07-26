using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace pcnafsplit
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("To split a single .pc.naf file");
                Console.WriteLine("  pcnafsplit.exe {filename.xbd) {output directory}");
                Console.WriteLine("To split all .pc.naf files in the current directory");
                Console.WriteLine("  pcnafsplit.exe * {output directory}");
                return -1;
            }
            string inputFile = args[0];
            string outputDir = args[1];

            //inputFile = @"C:\temp\splitxbd\MISC03_ASYLUM_CUTSCENE.xbd_LNGL";
            //inputFile = @"09_REEF.xbd_LNGL";
            //inputFile = @"06_REFINERY_PT1.xbd_LNGL";
            //inputFile = @"02_STREETS_ONE_PT1.xbd_LNGL";
            //inputFile = @"misc03_asylum_cutscene\NifExport.pc.naf";
            //inputFile = "*";

            if (inputFile != "*"
                && !File.Exists(inputFile))
            {
                Console.WriteLine("File {0} was not found!", inputFile);
                return -1;
            }

            if (!Directory.Exists(outputDir))
            {
                Console.WriteLine("Directory {0} was not found!", outputDir);
                return -1;
            }

            if (inputFile != "*")
            {
                Console.WriteLine("Processing file {0}", inputFile);
                if (!ProcessFile(inputFile, outputDir))
                {
                    Console.WriteLine("Split FAILED!!!");
                    //Console.ReadLine();
                    return -1;
                }
            }
            else
            {
                string[] files = Directory.GetFiles(".", "*.pc.naf", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    Console.WriteLine(new string('-', 32));
                    Console.WriteLine("Processing file {0}", file);
                    if (!ProcessFile(file, outputDir))
                    {
                        Console.WriteLine("Split FAILED!!!");
                        //Console.ReadLine();
                        return -1;
                    }
                }
            }

            Console.WriteLine("PC.NAF Split OK :)");
            Console.ReadLine();

            return 0;
        }

        static bool ProcessFile(string inputFile, string outputDirectory)
        {

            try
            {
                PCNAF pcNaf = new PCNAF();
                pcNaf.PCNAFEntries = new List<PCNAFEntry>();

                int matchCount = 0;
                long binStart = 0;

                using (BinaryReader b = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    short entries = 0;
                    while (b.BaseStream.Position < b.BaseStream.Length)
                    {
                        //PCNAFEntry entry = new PCNAFEntry();

                        byte checkByte = b.ReadByte();

                        if (matchCount < 2
                            && checkByte == 68)
                        {
                            matchCount++;
                        }
                        else if (matchCount == 2
                            && checkByte == 83)
                        {
                            matchCount++;
                        }
                        else if (matchCount == 3
                            && checkByte == 32)
                        {
                            matchCount++;
                        }
                        else
                        {
                            matchCount = 0;
                        }

                        if (matchCount != 4)
                        {
                            continue;
                        }

                        Console.WriteLine("Found texture at {0}", b.BaseStream.Position - 4);

                        //jump back to texture length
                        b.BaseStream.Position -= 8;

                        //output the previous chunk as bin
                        PCNAFEntry binEntry = new PCNAFEntry();

                        binEntry.EntryNumber = entries++;
                        binEntry.EntryType = "bin";

                        binEntry.Length = (int)(b.BaseStream.Position - binStart);
                        b.BaseStream.Position = binStart;
                        binEntry.Content = b.ReadBytes(binEntry.Length);
                        Console.WriteLine("Saving bin at {0} length {1}", binStart, binEntry.Length);

                        pcNaf.PCNAFEntries.Add(binEntry);

                        PCNAFEntry entry = new PCNAFEntry();

                        entry.EntryNumber = entries++;
                        entry.EntryType = "dds";

                        entry.Length = b.ReadInt32();
                        entry.Content = b.ReadBytes(entry.Length);

                        pcNaf.PCNAFEntries.Add(entry);

                        binStart = b.BaseStream.Position;

                        matchCount = 0;
                       
                    }
                }

                string outputDir = Path.Combine(outputDirectory, Path.GetDirectoryName(inputFile));
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                foreach (PCNAFEntry entry in pcNaf.PCNAFEntries)
                {
                    string extension = "";

                    if (entry.EntryType == "dds")
                    {
                        extension = "dds";
                    }
                    else
                    {
                        extension = "bin";
                    }

                    string writeFile = Path.Combine(outputDir, entry.EntryType + "_" + entry.EntryNumber + "." + extension);
                    entry.FileName = Path.GetFileName(writeFile);

                    //if (File.Exists(writeFile))
                    //{
                    //    Console.WriteLine("File to write to {0} already exists!!!", writeFile);
                    //    return false;
                    //}

                    Console.Write("  Writing content data to {0}", writeFile);
                    File.WriteAllBytes(writeFile, entry.Content);
                    Console.WriteLine("    OK");
                }

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(PCNAF));

                using (FileStream fileStream = new FileStream(Path.Combine(outputDir, Path.GetFileName(inputFile)) + ".xml", FileMode.Create))
                {
                    xmlSerializer.Serialize(fileStream, pcNaf);
                    fileStream.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing file: {0}", ex.Message);
                return false;
            }

            return true;
        }
    }
}
