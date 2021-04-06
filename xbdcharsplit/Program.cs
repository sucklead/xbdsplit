﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace xbdcharsplit
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("To split a single .xbd_CHAR file");
                Console.WriteLine("  xbdcharsplit.exe {filename.xbd_CHAR) {output directory}");
                Console.WriteLine("To split all .xbd_CHAR files in the current directory");
                Console.WriteLine("  xbdcharsplit.exe * {output directory}");
                return -1;
            }

            string inputFile = args[0];
            string outputDir = args[1];

            bool onlyModels = false;
            if (args.Length > 2)
            {
                onlyModels = (args[2] == "--only-models");
            }

            //inputFile = @"S:\Games\Call Of Cthulhu DCoTE\Resources\Xbox\01_HOUSE.xbd";

            if (inputFile != "*"
                && !File.Exists(inputFile))
            {
                Console.WriteLine("File {0} was not found!", inputFile);
                return -1;
            }

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
                if (!Directory.Exists(outputDir))
                {
                    //Console.WriteLine("Directory {0} was not found!", outputDir);
                    return -1;
                }
            }

            var fileProcessor = new FileProcessor();
            var fileOutput = new FileOutput();

            if (inputFile != "*")
            {
                Characters characters;
                Console.WriteLine("Processing file {0}", inputFile);
                if (!fileProcessor.ProcessFile(inputFile, out characters))
                {
                    Console.WriteLine("Split FAILED!!!");
                    return -1;
                }
                else
                {
                    fileOutput.OutputResults(inputFile, outputDir, characters, onlyModels);
                }
            }
            else
            {
                string[] files = Directory.GetFiles(".", "*.xbd_CHAR");
                foreach (string file in files)
                {
                    Characters characters;
                    Console.WriteLine("Processing file {0}", file);
                    if (!fileProcessor.ProcessFile(file, out characters))
                    {
                        Console.WriteLine("Split FAILED!!!");
                        return -1;
                    }
                    else
                    {
                        fileOutput.OutputResults(file, outputDir, characters, onlyModels);
                    }
                }
            }

            Console.WriteLine("Split OK :)");
            //Console.ReadLine();

            return 0;
        }

    }
}