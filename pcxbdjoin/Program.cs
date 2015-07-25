using pcxbdsplit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace pcxbdjoin
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("To join a single .pc.xbd file");
                Console.WriteLine("  pcxbdjoin.exe {input file} {output directory");
                Console.WriteLine("To split all .pc.xbd files in the current directory");
                Console.WriteLine("  pcxbdjoin.exe * {output directory}");
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
                string[] files = Directory.GetFiles(".", "*.pc.xbd.xml", SearchOption.AllDirectories);
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
            PCXBD pcXbd = null;

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreWhitespace = false;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(PCXBD));

            //using (FileStream fileStream = new FileStream(inputFile, FileMode.Open))
            using (XmlReader xmlReader = XmlReader.Create(inputFile, readerSettings))
            {
                pcXbd = xmlSerializer.Deserialize(xmlReader) as PCXBD;
                xmlReader.Close();
            }

            string outDir = Path.Combine(outputDir); //, Path.GetDirectoryName(inputFile));
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            string outputFile = Path.Combine(outDir, Path.GetFileName(inputFile).Replace(".xml", ""));

            using (BinaryWriter b = new BinaryWriter(File.Open(outputFile, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                foreach (PCXBDEntry entry in pcXbd.PCXBDEntries)
                {
                    //Array.Reverse(type);
                    b.Write(entry.EntryNumber);
                    b.Write(entry.TypeNumber);

                    if (entry.TypeNumber == 894720068)
                    {
                        b.Write(entry.UnknownShort1);
                        b.Write(entry.UnknownShort2);
                    }

                    byte[] content = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(inputFile), entry.FileName));
                    //write chunk length
                    b.Write(content.Length);

                    //write chunk
                    b.Write(content);
                }
            }

            return true;
        }
    }

}
