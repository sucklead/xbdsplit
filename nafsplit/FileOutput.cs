using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace nafsplit
{
    public class FileOutput
    {
        private void CreateCharacter(string destinationFile, string vertexFile, string facesFile)
        {
            string[] inputFilePaths = { vertexFile, facesFile };
            //string[] inputFilePaths = Directory.GetFiles(inputDirectoryPath, inputFileNamePattern);
            //Console.WriteLine("Number of files: {0}.", inputFilePaths.Length);
            using (var outputStream = File.Create(destinationFile))
            {
                foreach (var inputFilePath in inputFilePaths)
                {
                    using (var inputStream = File.OpenRead(inputFilePath))
                    {
                        // Buffer size can be passed as the second argument.
                        inputStream.CopyTo(outputStream);
                    }
                }
            }
        }

        public bool OutputResults(string inputFile, string outputDirectory, Characters characters, bool onlyModels)
        {
            try
            {
                //string fileDir = Path.Combine(outputDirectory, Path.GetFileName(inputFile));
                string fileDir = outputDirectory;
                if (!Directory.Exists(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                }

                string outputDir = fileDir; // Path.Combine(fileDir, "Chunks");
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                //write the header file
                //string headerFile = Path.Combine(outputDir, Path.GetFileName(inputFile)) + ".xml";

                foreach (var character in characters)
                {
                    //string writeFile = Path.Combine(outputDir, Path.GetFileName(inputFile) + "_" + (character.CharacterName.Replace(" ", "_")));
                    string writeFile = Path.Combine(outputDir, (character.CharacterName.Replace(" ", "_")));
                    character.FileName = writeFile + ".obj";

                    //write the data out
                    foreach (Vertices vertices in character.VerticesList)
                    {
                        Console.WriteLine($"  Writing vertices from offset {vertices.Offset}");
                        vertices.Filename = $"{writeFile}_{vertices.Offset}_{vertices.Offset:X}_[{vertices.Items.Count}]_v.obj";
                        using (StreamWriter streamWriter = File.CreateText(vertices.Filename))
                        {
                            streamWriter.WriteLine($"# Vertices for offset {vertices.Offset} [{vertices.Offset:X}], vertex count = {vertices.Items.Count}");

                            foreach (Vertex vertex in vertices.Items)
                            {
                                // meshlab format
                                //streamWriter.WriteLine($"f {face.Index1}//{face.Index1} {face.Index2}//{face.Index2} {face.Index3}//{face.Index3}");
                                // hex2obj format
                                streamWriter.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");
                            }

                        }
                        Console.WriteLine("    OK");
                    }

                    //write the data out
                    foreach (Faces faces in character.FacesList)
                    {
                        Console.WriteLine($"  Writing faces from offset {faces.Offset} [{faces.Offset:X}]");
                        faces.Filename = $"{writeFile}_{faces.Offset}_{faces.Offset:X}_[{faces.HighestVertex}]_f.obj";
                        using (StreamWriter streamWriter = File.CreateText(faces.Filename))
                        {
                            streamWriter.WriteLine($"# Faces for offset {faces.Offset}");
                            streamWriter.WriteLine($"# Maximum Vertex is {faces.HighestVertex}");
                            streamWriter.WriteLine($"# Maximum Value is {faces.HighestValue}");
                            streamWriter.WriteLine($"g submesh_{faces.Submesh}");

                            foreach (Face face in faces.Items)
                            {
                                // meshlab format
                                //streamWriter.WriteLine($"f {face.Index1}//{face.Index1} {face.Index2}//{face.Index2} {face.Index3}//{face.Index3}");
                                // hex2obj format
                                streamWriter.WriteLine($"f {face.Index1} {face.Index2} {face.Index3}");
                            }
                            streamWriter.WriteLine($"# {faces.Items.Count} faces, 0 coords texture");

                        }
                        Console.WriteLine("    OK");
                    }
                    
                    // any matches?
                    foreach (Vertices vertices in character.VerticesList)
                    {
                        foreach (Faces faces in character.FacesList)
                        {
                            if (vertices.Items.Count == faces.HighestVertex)
                            {
                                CreateCharacter(character.FileName, vertices.Filename, faces.Filename);
                            }

                            if (onlyModels)
                            {
                                File.Delete(faces.Filename);
                            }
                        }

                        if (onlyModels)
                        {
                            File.Delete(vertices.Filename);
                        }
                    }


                    //XmlSerializer xmlSerializer = new XmlSerializer(typeof(CHAR));
                    //using (FileStream fileStream = new FileStream(headerFile, FileMode.OpenOrCreate))
                    //{
                    //    xmlSerializer.Serialize(fileStream, thiChar);
                    //    fileStream.Close();
                    //}
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
