using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace xbdsplit
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("To split a single .xbd file");
                Console.WriteLine("  xbdsplit.exe {filename.xbd) {output directory}");
                Console.WriteLine("To split all .xbd files in the current directory");
                Console.WriteLine("  xbdsplit.exe * {output directory}");
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
                    Console.WriteLine("Split FAILED!!!");
                    return -1;
                }
            }
            else
            {
                string[] files = Directory.GetFiles(".", "*.xbd");
                foreach(string file in files)
                {
                    Console.WriteLine("Processing file {0}", file);
                    if (!ProcessFile(file, outputDir))
                    {
                        Console.WriteLine("Split FAILED!!!");
                        return -1;
                    }
                }
            }

            Console.WriteLine("Split OK :)");
            Console.ReadLine();

            return 0;
        }

        static bool ProcessFile(string inputFile, string outputDirectory)
        {

            try
            {
                XBD xbd = new XBD();
                xbd.Chunks = new List<XBDChunk>();

                using (BinaryReader b = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    if (b.BaseStream.Length < 12)
                    {
                        Console.WriteLine("Skipping not long enough so not an .xbd file!");
                        return true;
                    }

                    Console.WriteLine("Checking for magic at [{0}]", b.BaseStream.Position);
                    string magic = new string(b.ReadChars(4).Reverse().ToArray());
                    //check magic
                    if (magic != "CHNK")
                    {
                        Console.WriteLine("  Skipping not an .xbd file!");
                        return true;
                    }
                    Console.WriteLine("  Found magic of {0}", magic);

                    Console.WriteLine("Reading Int32 at [{0}]", b.BaseStream.Position);
                    xbd.Version = b.ReadInt32();
                    Console.WriteLine("  Version is {0}", xbd.Version);

                    Console.WriteLine("Reading Int32 at [{0}]", b.BaseStream.Position);
                    xbd.NumberOfChunks = b.ReadInt32();
                    Console.WriteLine("  Number of chunks is {0}", xbd.NumberOfChunks);

                    long chunkStart; //= b.BaseStream.Position;
                    long chunkDataStart;
                    for (int i = 1; i <= xbd.NumberOfChunks; i++)
                    {
                        XBDChunk chunk = new XBDChunk();
                        //save chunk start position
                        chunkStart = b.BaseStream.Position;
                        Console.WriteLine(new string('-', 30));
                        Console.WriteLine("Processing chunk number {0}", i);
                        Console.WriteLine("  Reading Chunk Type at [{0}]", chunkStart);
                        chunk.ChunkType = new string(b.ReadChars(4).Reverse().ToArray());
                        Console.WriteLine("  Chunk type is {0}", chunk.ChunkType);

                        Console.WriteLine("  Reading Int32 at [{0}]", b.BaseStream.Position);
                        int chunkLength = b.ReadInt32();
                        Console.WriteLine("  Chunk length is {0}", chunkLength);

                        chunkDataStart = b.BaseStream.Position;
                        chunk.Content = b.ReadBytes(chunkLength);

                        xbd.Chunks.Add(chunk);

                        if (i < xbd.NumberOfChunks)
                        {
                            Console.WriteLine("  Moving to next chunk");
                            b.BaseStream.Seek(chunkDataStart + chunkLength, SeekOrigin.Begin);
                        }
                    }
                }

                //write the header file
                string headerFile = Path.Combine(outputDirectory, Path.GetFileName(inputFile)) + ".xml";
                //write the data out
                foreach (XBDChunk chunk in xbd.Chunks)
                {
                    //xmlTextWriter.WriteStartElement("Chunk");
                    string writeFile = Path.Combine(outputDirectory, Path.GetFileName(inputFile) + "_" + chunk.ChunkType);
                    chunk.FileName = Path.GetFileName(writeFile);

                    //if (File.Exists(writeFile))
                    //{
                    //    Console.WriteLine("File to write to {0} already exists!!!", writeFile);
                    //    return false;
                    //}

                    Console.WriteLine("  Writing chunk data to {0}", writeFile);
                    File.WriteAllBytes(writeFile, chunk.Content);
                    Console.WriteLine("    OK");

                }

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(XBD));
                using (FileStream fileStream = new FileStream(headerFile, FileMode.OpenOrCreate))
                {
                    xmlSerializer.Serialize(fileStream, xbd);
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
