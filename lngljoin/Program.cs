using lngsplit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace lngljoin
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("To join a single .xbd file");
                Console.WriteLine("  xbdjoin.exe {input file} {output directory");
                Console.WriteLine("To split all .xbd files in the current directory");
                Console.WriteLine("  xbdjoin.exe * {output directory}");
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
                string[] files = Directory.GetFiles(".", "*_LNGL.xml", SearchOption.AllDirectories);
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
            LNGL lngl = null;

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.IgnoreWhitespace = false;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(LNGL));

            //using (FileStream fileStream = new FileStream(inputFile, FileMode.Open))
            using (XmlReader xmlReader = XmlReader.Create(inputFile, readerSettings))
            {
                lngl = xmlSerializer.Deserialize(xmlReader) as LNGL;
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
                //b.Write("LGNL".ToCharArray());
                b.Write(lngl.Version);
                b.Write(lngl.NumberOfLanguages);
                foreach (Language language in lngl.Languages)
                {
                    //char[] type = chunk.ChunkType.ToCharArray();
                    //Array.Reverse(type);
                    b.Write(language.NameLength);
                    b.Write(language.Name.ToCharArray());
                    b.Write(language.NumberOfEntries);

                    //write first offset
                    b.Write((Int32)0);

                    int nextString = 0; //(int)(b.BaseStream.Position + (language.NumberOfEntries * 8));
                    foreach(Entry entry in language.Entries)
                    {
                        //fix line endings
                        entry.Text = entry.Text.Replace("\n", "\r\n");

                        b.Write(entry.Unknown);
                        b.Write(entry.TextId);
                        nextString += entry.Text.Length + 1;
                        b.Write(nextString);
                    }
                    foreach (Entry entry in language.Entries)
                    {
                        b.Write(System.Text.Encoding.Default.GetBytes(entry.Text));
                        b.Write((byte)0);
                    }

                    b.Write(language.NumberOfIdValuePairs);
                    foreach (IdValuePair idValuePair in language.IdValuePairs)
                    {
                        b.Write(idValuePair.Id);
                        b.Write(idValuePair.Value);
                    }

                    //write contents
                    //b.Write(File.ReadAllBytes(chunk.FileName));
                }
            }

            return true;
        }
    }
}
