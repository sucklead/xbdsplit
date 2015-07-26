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
            //Console.ReadLine();

            return 0;
        }

        static bool ProcessFile(string inputFile, string outputDirectory)
        {

            try
            {
                PCNAF pcNaf = new PCNAF();
                pcNaf.PCNAFEntries = new List<PCNAFEntry>();

                int matchCount = 0;
                //long binStart = 0;

                using (BinaryReader b = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    while (b.BaseStream.Position < b.BaseStream.Length)
                    {
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
                        break;
                    }
                    //jump back to number of textures
                    b.BaseStream.Position -= 14;

                    //output the previous chunk as bin
                    PCNAFEntry binEntry = new PCNAFEntry();
                    binEntry.EntryType = "bin";

                    binEntry.EntryNumber = 0;
                    binEntry.Length = (int)(b.BaseStream.Position);
                    b.BaseStream.Position = 0;
                    binEntry.Content = b.ReadBytes(binEntry.Length);

                    pcNaf.PCNAFEntries.Add(binEntry);

                    int numberOfTextures = b.ReadInt32();
                    
                    for (int i = 0; i < numberOfTextures; i++)
                    {
                        //output the previous chunk as bin
                        PCNAFEntry entry = new PCNAFEntry();
                        entry.EntryType = "dds";

                        entry.EntryNumber = b.ReadInt16();
                        entry.Length = b.ReadInt32();
                        entry.Content = b.ReadBytes(entry.Length);

                        pcNaf.PCNAFEntries.Add(entry);

                        //skip the *
                        b.ReadByte();
                    }
                }

                string outputDir = Path.Combine(outputDirectory, Path.GetDirectoryName(inputFile));
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }



                foreach (PCNAFEntry entry in pcNaf.PCNAFEntries)
                {
                    string writeFile = "";
                    if (entry.EntryType == "dds")
                    {
                        writeFile = Path.Combine(outputDir, "Texture_" + entry.EntryNumber.ToString("000000") + ".dds");
                    }
                    else
                    {
                        writeFile = Path.Combine(outputDir, "Unknown.bin");
                    }

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
