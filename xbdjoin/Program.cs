using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using xbdsplit;

namespace xbdjoin
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

            //inputFile = @"S:\Games\Call Of Cthulhu DCoTE\Resources\Xbox\01_HOUSE.xbd";

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
                string[] files = Directory.GetFiles(".", "*.xbd.xml", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    //don't process .pc.xbd.xml files
                    if (file.EndsWith(".pc.xbd.xml"))
                    {
                        continue;
                    }

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
            XBD xbd = null;
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(XBD));
            using (FileStream fileStream = new FileStream(inputFile, FileMode.Open))
            {
                xbd = xmlSerializer.Deserialize(fileStream) as XBD;
                fileStream.Close();
            }

            string outputFile = Path.Combine(outputDir, Path.GetFileName(inputFile).Replace(".xml",""));

            using (BinaryWriter b = new BinaryWriter(File.Open(outputFile, FileMode.Create, FileAccess.Write, FileShare.Read)))
            {
                b.Write("KNHC".ToCharArray());
                b.Write(xbd.Version);
                b.Write(xbd.NumberOfChunks);
                foreach(XBDChunk chunk in xbd.Chunks)
                {
                    char[] type = chunk.ChunkType.ToCharArray();
                    Array.Reverse(type);
                    b.Write(type);

                    byte[] content = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(inputFile), chunk.FileName));
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
