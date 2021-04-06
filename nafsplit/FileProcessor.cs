using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace nafsplit
{
    public class FileProcessor
    {

        public bool ProcessFile(string inputFile, out Models models)
        {
            models = new Models();

            try
            {

                using (BinaryReader b = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    long fileLength = b.BaseStream.Length;

                    int numberOfModels;
                    if (false)
                    {
                        Console.WriteLine($"Reading Int32 at [{b.BaseStream.Position}]");
                        var version = b.ReadInt32();
                        Console.WriteLine($"  Version is {version}");

                        Console.WriteLine($"Reading Int32 at [{b.BaseStream.Position}]");
                        numberOfModels = b.ReadInt32();
                        Console.WriteLine($"  Number of models is {numberOfModels}");
                        numberOfModels = 1;
                    }
                    else
                    {
                        numberOfModels = 1;
                    }


                    long chunkStart; //= b.BaseStream.Position;
                    long chunkDataStart;
                    for (int i = 1; i <= numberOfModels; i++)
                    {
                        Model model = new Model();
                        models.Add(model);

                        //save chunk start position
                        chunkStart = b.BaseStream.Position;

                        if (false)
                        {
                            try
                            {
                                int charNameLength = b.ReadInt32();
                                Console.WriteLine($"Model name length is {charNameLength}");

                                model.ModelName = new string(b.ReadChars(charNameLength).ToArray());
                                Console.WriteLine($"  Model name is {model.ModelName}");
                            }
                            catch (Exception ex)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            model.ModelName = "Root";
                        }

                        // process the byte stream
                        while (b.BaseStream.Position < fileLength)
                        {
                            if (IsNewModel(b))
                            {
                                b.BaseStream.Seek(-4, SeekOrigin.Current);
                                numberOfModels += 1;
                                break;
                            }
                            else 
                            if (IsFaces(b))
                            {
                                ProcessFaces(b, model.FacesList);
                            }
                            else if (IsVertices(b))
                            {
                                ProcessVertices(b, model.VerticesList);
                            }
                            else
                            {
                                try
                                {
                                    byte readByte = b.ReadByte();
                                }
                                catch (Exception ex)
                                {
                                    return false;
                                }
                            }
                        }

                        //break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing file: {0}", ex.Message);
                return false;
            }
            return true;
        }

        private bool IsNewModel(BinaryReader b)
        {
            string[] characters = {"FBI Agent Variant",
                                   "Cultist Variant",
                                   "Shot Cultist",
                                   "Policeman Variant",
                                   "Elliot Ropes",
                                   "Charles Gilman",
                                   "Hybrid 1",
                                   "Innsmouth Male Misc Variant",
                                   "Joe Sargent",
                                   "Lucas Mackey",
                                   "Nathan Birch",
                                   "Rebecca Lawrence",
                                   "Zadok Allen",
                                   "Female Variant Hybrid",
                                   "Innsmouth Female Misc",
                                   "Pensioner Female",
                                   "Pensioner Male Variant",
                                   "Ramona Waites",
                                   "Ruth Billingham",
                                   "Andrew Martin",
                                   "Edgar Hoover",
                                   "Deep One",
                                   "Hybrid 2",
                                   "Inmate",
                                   "Hybrid 3",
                                   "Jacob Marsh",
                                   "Marine Variant",
                                   "Sailor Variant",
                                   "Tainted Man",
                                   "Robert Marsh",
                                   "Sebastian Marsh"
            };
            foreach(var character in characters)
            {
                if (IsPattern(b, Encoding.ASCII.GetBytes(character)))
                {
                    return true;
                }
            }
            return false;
        }

        private void ProcessVertices(BinaryReader b, List<Vertices> verticesList)
        {

            Console.WriteLine($"  Vertices at {b.BaseStream.Position} [{b.BaseStream.Position:X}]");

            Vertices vertices = new Vertices();
            verticesList.Add(vertices);
            vertices.Offset = b.BaseStream.Position;

            // seek backwards 2 to get length
            b.BaseStream.Seek(-2, SeekOrigin.Current);


            Console.WriteLine($"    Reading Int16 at [{b.BaseStream.Position}]");
            int verticesLength = b.ReadInt16();
            Console.WriteLine($"      VerticesLength is {verticesLength}");

            if (verticesLength == 0)
            {
                b.ReadByte();
                return;
            }

            for (var thisVertex = 0; thisVertex < verticesLength; thisVertex++)
            {
                // not enough bytes
                if (b.BaseStream.Position + 8 >= b.BaseStream.Length)
                {
                    Console.WriteLine("!!! NOT ENOUGH BYTES !!!");
                    break;
                }

                Vertex vertex = new Vertex()
                {
                    Position = b.ReadUInt16(),
                    X = bytesToFloat(b.ReadByte(), b.ReadByte()),
                    Y = bytesToFloat(b.ReadByte(), b.ReadByte()),
                    Z = bytesToFloat(b.ReadByte(), b.ReadByte())
                };

                vertices.Items.Add(vertex);
            }

        }

        private void ProcessFaces(BinaryReader b, List<Faces> facesList)
        {
            // seek back 16 to get to length
            b.BaseStream.Seek(-16, SeekOrigin.Current);

            Faces faces = new Faces();
            facesList.Add(faces);

            Console.WriteLine($"    Reading Int32 at [{b.BaseStream.Position}]");
            int facesLength = b.ReadInt32();
            Console.WriteLine($"      FacesLength is {facesLength}");

            // seek forward 12 to get to faces
            b.BaseStream.Seek(12, SeekOrigin.Current);


            //Console.WriteLine($"    Reading UInt16 at [{b.BaseStream.Position}]");
            //ushort submesh = b.ReadUInt16();
            //faces.Submesh = submesh;

            //Console.WriteLine($"      Submesh is {faces.Submesh}");

            //Console.WriteLine($"    Reading {facesLength * 6} bytes at {b.BaseStream.Position}...");
            //byte[] facesBytes = b.ReadBytes(facesLength * 6);

            Console.WriteLine($"  Faces at {b.BaseStream.Position} [{b.BaseStream.Position:X}]");
            faces.Offset = b.BaseStream.Position;

            for (var thisFace = 0; thisFace < facesLength; thisFace++)
            {
                // not enough bytes
                if (b.BaseStream.Position + 6 >= b.BaseStream.Length)
                {
                    break;
                }

                Face face = new Face()
                {
                    //FaceIndex = faceIndex,
                    Index1 = b.ReadUInt16(),
                    Index2 = b.ReadUInt16(),
                    Index3 = b.ReadUInt16()
                };

                // reject if we hit 0xFFFF
                if (face.Index1 == 0xFFFF
                    || face.Index2 == 0xFFFF
                    || face.Index3 == 0xFFFF)
                {
                    break;
                }

                face.Index1 += 1;
                face.Index2 += 1;
                face.Index3 += 1;

                faces.Items.Add(face);
            }
            Console.WriteLine($"      Faces added to list");

            // set highest values for header
            foreach (var face in faces.Items)
            {
                if (face.Index1 > faces.HighestValue)
                {
                    faces.HighestValue = face.Index1;
                }
                if (face.Index2 > faces.HighestValue)
                {
                    faces.HighestValue = face.Index2;
                }
                if (face.Index3 > faces.HighestValue)
                {
                    faces.HighestValue = face.Index3;
                }

                if (face.Index1 > faces.HighestVertex
                    && face.Index1 != 0xFFFF)
                {
                    faces.HighestVertex = face.Index1;
                }
                if (face.Index2 > faces.HighestVertex
                    && face.Index2 != 0xFFFF)
                {
                    faces.HighestVertex = face.Index2;
                }
                if (face.Index3 > faces.HighestVertex
                    && face.Index3 != 0xFFFF)
                {
                    faces.HighestVertex = face.Index3;
                }
            }
        }

        public static float bytesToFloat(byte lowByte, byte highByte)
        {
            float result = (sbyte)highByte + ((float)lowByte / 256);

            return result;
        }

        public static byte[] floatToBytes(float value)
        {
            byte[] result = { 0x00, 0x00 };

            result[1] = (byte)value;
            result[0] = (byte)((value - result[1]) * 256);

            return result;
        }

        public bool IsFaces(BinaryReader b)
        {
            byte[] facesBytes = { 0x00, 0x00, 0x01, 0x00, 0x02, 0x00 };

            return IsPattern(b, facesBytes);

        }

        public bool IsVertices(BinaryReader b)
        {
            // search for 4 numbered vertices
            int findVerticesCount = 4;

            var savePosition = b.BaseStream.Position;

            // are there enough bytes?
            if (b.BaseStream.Position + (findVerticesCount * 8) > b.BaseStream.Length)
            {
                return false;
            }

            // find first 0x00 0x00
            if (b.ReadInt16() != 0x00)
            {
                b.BaseStream.Position = savePosition;
                return false;
            }

            Int16 readInt16;
            for (int rb = 1; rb < findVerticesCount; rb++)
            {
                b.BaseStream.Position += 6;
                readInt16 = b.ReadInt16();
                if (readInt16 != rb)
                {
                    b.BaseStream.Position = savePosition;
                    return false;
                }
            }

            b.BaseStream.Position = savePosition;
            return true;
        }

        public bool IsPattern(BinaryReader b, byte[] patternBytes)
        {
            var savePosition = b.BaseStream.Position;

            // are there enough bytes?
            if (b.BaseStream.Position + patternBytes.Length >= b.BaseStream.Length)
            {
                return false;
            }

            if (b.ReadByte() != patternBytes[0])
            {
                b.BaseStream.Position = savePosition;
                return false;
            }

            Byte readByte;
            for (int rb = 1; rb < patternBytes.Length; rb++)
            {
                try
                {
                    readByte = b.ReadByte();
                    if (readByte != patternBytes[rb])
                    {
                        b.BaseStream.Position = savePosition;
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            b.BaseStream.Position = savePosition;
            return true;
        }
    }
}
