using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace pcxbdsplit
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("To split a single .pc.xbd_file");
                Console.WriteLine("  lngsplit.exe {filename.xbd) {output directory}");
                Console.WriteLine("To split all .xbd_LNGx files in the current directory");
                Console.WriteLine("  lngsplit.exe * {output directory}");
                return -1;
            }
            string inputFile = args[0];
            string outputDir = args[1];

            //inputFile = @"C:\temp\splitxbd\MISC03_ASYLUM_CUTSCENE.xbd_LNGL";
            //inputFile = @"09_REEF.xbd_LNGL";
            //inputFile = @"06_REFINERY_PT1.xbd_LNGL";
            //inputFile = @"02_STREETS_ONE_PT1.xbd_LNGL";
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

            Console.WriteLine("PC.XBD Split OK :)");
            Console.ReadLine();

            return 0;
        }

        static bool ProcessFile(string inputFile, string outputDirectory)
        {

            try
            {
                PCXBD pcXbd = new PCXBD();
                pcXbd.PCXBDEntries = new List<PCXBDEntry>();

                using (BinaryReader b = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    while (b.BaseStream.Position < b.BaseStream.Length)
                    {
                        PCXBDEntry entry = new PCXBDEntry();
                        entry.EntryNumber = b.ReadInt16();
                        Console.WriteLine("Entry Number is {0}", entry.EntryNumber);

                        entry.TypeNumber = b.ReadInt32();

                        if (entry.TypeNumber == 894720068)
                        {
                            entry.EntryType = "DXT5";
                        }
                        else
                        {
                            entry.EntryType = string.Format("Unknown{0}", entry.TypeNumber);
                        }

                        //entry.EntryType = new string(b.ReadChars(4).ToArray());
                        Console.WriteLine(" Entry Type is {0}", entry.EntryType);

                        if (entry.EntryType == "DXT5")
                        {
                            entry.UnknownShort1 = b.ReadInt16();
                            entry.UnknownShort2 = b.ReadInt16();
                            //Console.WriteLine(" UnknownShort1 {0} UnknownShort2 {1}", entry.UnknownShort1, entry.UnknownShort2);
                        }

                        entry.Length = b.ReadInt32();
                        Console.WriteLine(" Content Length {0}", entry.Length);

                        entry.Content = b.ReadBytes(entry.Length);
                        Console.WriteLine("(Position {0} of {1})", b.BaseStream.Position, b.BaseStream.Length);

                        pcXbd.PCXBDEntries.Add(entry);
                    }
                }

                foreach (PCXBDEntry entry in pcXbd.PCXBDEntries)
                {
                    string outputDir = Path.Combine(outputDirectory, Path.GetFileName(inputFile));
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }

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

                    Console.WriteLine("  Writing content data to {0}", writeFile);
                    File.WriteAllBytes(writeFile, entry.Content);
                    Console.WriteLine("    OK");

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