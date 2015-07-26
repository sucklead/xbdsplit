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
            inputFile = @"misc03_asylum_cutscene\NifExport.pc.naf";
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
                string[] files = Directory.GetFiles(".", "*.pc.xbd");
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

                using (BinaryReader b = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
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

                        if (matchCount != 4)
                        {
                            continue;
                        }

                        Console.WriteLine("Found texture at {0}", b.BaseStream.Position - 4);

                        //jump back to texture length
                        b.BaseStream.Position -= 8;

                        PCNAFEntry entry = new PCNAFEntry();

                        //entry.EntryNumber = 

                        pcNaf.PCNAFEntries.Add(entry);

                        matchCount = 0;

                        //entry.EntryNumber = b.ReadInt16();
                        //Console.WriteLine("Entry Number is {0}", entry.EntryNumber);

                        //entry.TypeNumber = b.ReadInt32();

                        //if (entry.TypeNumber == 894720068)
                        //{
                        //    entry.EntryType = "DXT5";
                        //}
                        //else
                        //{
                        //    entry.EntryType = string.Format("Unknown{0}", entry.TypeNumber);
                        //}

                        ////entry.EntryType = new string(b.ReadChars(4).ToArray());
                        //Console.WriteLine(" Entry Type is {0}", entry.EntryType);

                        //if (entry.EntryType == "DXT5")
                        //{
                        //    entry.UnknownShort1 = b.ReadInt16();
                        //    entry.UnknownShort2 = b.ReadInt16();
                        //    //Console.WriteLine(" UnknownShort1 {0} UnknownShort2 {1}", entry.UnknownShort1, entry.UnknownShort2);
                        //}

                        //entry.Length = b.ReadInt32();
                        //Console.WriteLine(" Content Length {0}", entry.Length);

                        //entry.Content = b.ReadBytes(entry.Length);
                        //Console.WriteLine("(Position {0} of {1})", b.BaseStream.Position, b.BaseStream.Length);

                        //pcNaf.PCNAFEntries.Add(entry);
                    }
                }

                string outputDir = Path.Combine(outputDirectory, Path.GetFileName(inputFile));
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                foreach (PCNAFEntry entry in pcNaf.PCNAFEntries)
                {
                    string extension = "";

                    if (entry.EntryType == "DXT5")
                    {
                        extension = "dds";
                    }
                    else
                    {
                        extension = "obj";
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
