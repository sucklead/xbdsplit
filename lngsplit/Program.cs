using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace lngsplit
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("To split a single .xbd_LNGx file");
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
            inputFile = "*";

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
                string[] files = Directory.GetFiles(".", "*.xbd_LNG?", SearchOption.AllDirectories);
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

            Console.WriteLine("LNG Split OK :)");
            //Console.ReadLine();

            return 0;
        }

        static bool ProcessFile(string inputFile, string outputDirectory)
        {

            try
            {
                LNGL lngl = new LNGL();
                lngl.Languages = new List<Language>();

                using (BinaryReader b = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    if (b.BaseStream.Length < 4)
                    {
                        Console.WriteLine("Skipping not long enough so not an .xbd_LNGx file!");
                        return true;
                    }

                    Console.WriteLine("Checking for magic at [{0}]", b.BaseStream.Position);
                    //Console.WriteLine("Reading Int32 at [{0}]", b.BaseStream.Position);
                    lngl.Version = b.ReadInt32();
                    if (lngl.Version != 2)
                    {
                        Console.WriteLine("  Skipping not an .xbd_LNGx file!");
                        return true;
                    }
                    Console.WriteLine("  Found magic of {0}", lngl.Version);

                    //Console.WriteLine("Reading Int32 at [{0}]", b.BaseStream.Position);
                    lngl.NumberOfLanguages = b.ReadInt16();
                    Console.WriteLine("  Number of languages is {0}", lngl.NumberOfLanguages);

                    if (lngl.NumberOfLanguages != 3)
                    {
                        Console.WriteLine("  Skipping not a correct .xbd_LNGx file!");
                        return true;
                    }


                    for (int i = 1; i <= lngl.NumberOfLanguages; i++)
                    {

                        Language language = new Language();

                        //Console.WriteLine("Reading Int32 at [{0}]", b.BaseStream.Position);
                        language.NameLength = b.ReadInt32();
                        Console.WriteLine("  Language name length is {0}", language.NameLength);

                        //Console.WriteLine("Reading String at [{0}] with length [{1}]", b.BaseStream.Position, languageNamelength);
                        language.Name = new string(b.ReadChars(language.NameLength));
                        Console.WriteLine("  Language name is {0}", language.Name);

                        //Console.WriteLine("Reading Int32 at [{0}]", b.BaseStream.Position);
                        language.NumberOfEntries = b.ReadInt16();
                        Console.WriteLine("  Number of entries is {0}", language.NumberOfEntries);

                        //Console.WriteLine("Reading unknown Int16 at [{0}]", b.BaseStream.Position);
                        //language.Unknown = b.ReadInt16();

                        //calculate start of strings
                        int startOfStrings = (int)(b.BaseStream.Position + (8 * language.NumberOfEntries) + 4);
                        Console.WriteLine("  Start of strings is {0}", startOfStrings);
                        //long nextString = startOfStrings;
                        //Int32 lastOffset = 0;

                        language.Entries = new List<Entry>();
                        lngl.Languages.Add(language);

                        Console.WriteLine("Reading First String at [{0}]", b.BaseStream.Position);
                        int firstString = startOfStrings + b.ReadInt32();
                        Console.WriteLine("  First String at {0}", firstString);

                        int nextEntry = (int)(b.BaseStream.Position);
                        int nextString = firstString;
                        for (int e = 1; e <= language.NumberOfEntries; e++)
                        {
                            Entry entry = new Entry();

                            //restore next entry position
                            b.BaseStream.Position = nextEntry;

                            //Console.WriteLine("Reading unknown Int16 at [{0}]", b.BaseStream.Position);
                            entry.Unknown = b.ReadInt16();
                            Console.WriteLine("Reading TextId at [{0}]", b.BaseStream.Position);
                            entry.TextId = b.ReadInt16();
                            //Console.WriteLine("  Entry {0} text id is {1}", e, entry.TextId);

                            Console.WriteLine("Reading NextEntry at [{0}]", b.BaseStream.Position);
                            int entryOffset = b.ReadInt32();
                            Console.WriteLine("  Entry {0} text id {1} entry offset {2}", e, entry.TextId, entryOffset);

                            //save next entry point
                            nextEntry = (int)(b.BaseStream.Position);

                            //start at next string
                            b.BaseStream.Position = nextString;

                            //read length bytes into the string skip -1 00
                            //string text = new string(b.ReadChars(stringOffset - lastOffset - 1));

                            List<byte> byteList = new List<byte>();
                            byte readB = b.ReadByte();
                            while (readB != 0)
                            {
                                byteList.Add(readB);
                                readB = b.ReadByte();
                            }
                            //byte[] tvByteArray = new byte[nextString - 1];
                            //tvByteArray = b.ReadBytes(nextString - 1);
                            //read the 00 byte
                            //b.ReadByte();

                            byte[] tvByteArray = byteList.ToArray();

                            nextString = startOfStrings + entryOffset;

                            //System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                            entry.Text = System.Text.Encoding.Default.GetString(tvByteArray);
                            //entry.Text = enc.GetString(tvByteArray);
                            if (entry.Text.StartsWith("Komm"))
                            {
                                Console.WriteLine("HERE");
                            }


                            //remove all the \0 and trim the string  
                            //return tvTemp.Replace("\0", "").Trim();   

                            Console.WriteLine("Text {0} ({1}) is [{2}]", e, entry.TextId, entry.Text);

                            language.Entries.Add(entry);
                        }

                        Console.WriteLine("Reading NumberOfIdValuePairs at [{0}]", b.BaseStream.Position);
                        language.NumberOfIdValuePairs = b.ReadInt16();

                        language.IdValuePairs = new List<IdValuePair>();
                        if (language.NumberOfIdValuePairs > 0)
                        {
                            for (int t = 1; t <= language.NumberOfIdValuePairs; t++)
                            {
                                IdValuePair idValuePair = new IdValuePair();

                                Console.WriteLine("Reading Id at [{0}]", b.BaseStream.Position);
                                idValuePair.Id = b.ReadInt32();
                                Console.WriteLine("Reading Value at [{0}]", b.BaseStream.Position);
                                idValuePair.Value = b.ReadInt32();
                                Console.WriteLine("  Thing Id {0} Value{1}", idValuePair.Id, idValuePair.Value);

                                language.IdValuePairs.Add(idValuePair);
                            }
                        }
                    }
                }

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(LNGL));
                
                using (FileStream fileStream = new FileStream(inputFile + ".xml", FileMode.Create))
                {
                    xmlSerializer.Serialize(fileStream, lngl);
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
