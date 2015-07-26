using pcnafsplit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace pcnafjoin
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("To join a single .pc.naf file");
                Console.WriteLine("  pcnafjoin.exe {input file} {output directory");
                Console.WriteLine("To split all .pc.naf files in the current directory");
                Console.WriteLine("  pcnafjoin.exe * {output directory}");
                return -1;
            }

            string inputFile = args[0];
            string outputDir = args[1];

            //inputFile = @"01_HOUSE.xbd_LNGL.xml";

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
                    Console.WriteLine("Join FAILED!!!");
                    return -1;
                }
            }
            else
            {
                string[] files = Directory.GetFiles(".", "*.pc.naf.xml", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    Console.WriteLine("Processing file {0}", file);
                    if (!ProcessFile(file, outputDir))
                    {
                        Console.WriteLine("Join FAILED!!!");
                        return -1;
                    }
                }
            }

            Console.WriteLine("Join OK :)");
            //Console.ReadLine();

            return 0;
        }

        private static bool ProcessFile(string inputFile, string outputDir)
        {
            PCNAF pcNaf = null;

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreWhitespace = false;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PCNAF));

            //using (FileStream fileStream = new FileStream(inputFile, FileMode.Open))
            using (XmlReader xmlReader = XmlReader.Create(inputFile, readerSettings))
            {
                pcNaf = xmlSerializer.Deserialize(xmlReader) as PCNAF;
                xmlReader.Close();
            }

            string outDir = Path.Combine(outputDir, Path.GetDirectoryName(inputFile));
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            string outputFile = Path.Combine(outDir, Path.GetFileName(inputFile).Replace(".xml", ""));

            using (BinaryWriter b = new BinaryWriter(File.Open(outputFile, FileMode.Create, FileAccess.Write, FileShare.None)))
            {

                foreach (PCNAFEntry entry in pcNaf.PCNAFEntries)
                {
                    byte[] content = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(inputFile), entry.FileName));

                    if (entry.EntryType == "bin")
                    {
                        //write chunk
                        b.Write(content);
                        //write number of textures
                        b.Write((int)(pcNaf.PCNAFEntries.Count - 1));
                    }
                    else
                    {
                        //texture id
                        b.Write(entry.EntryNumber);

                        b.Write(content.Length);
                        //write chunk
                        b.Write(content);

                        //write * terminator
                        b.Write((byte)42);
                    }
                }
            }

            return true;
        }
    }
}
