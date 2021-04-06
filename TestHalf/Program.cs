using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestHalf
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] halfBytes1 = { 0x62, 0x23 };
            byte[] halfBytes2 = { 0x86, 0xE9 };
            byte[] halfBytes3= { 0x48, 0xD6 };

            float testFloat1 = bytesToFloat(halfBytes1[0], halfBytes1[1]);
            float testFloat2 = bytesToFloat(halfBytes2[0], halfBytes2[1]);
            float testFloat3 = bytesToFloat(halfBytes3[0], halfBytes3[1]);

            Console.WriteLine($"testFloat1 value is [{testFloat1:F6}]");
            Console.WriteLine($"testFloat2 value is [{testFloat2:F6}]");
            Console.WriteLine($"testFloat3 value is [{testFloat3:F6}]");

            float testFloat4 =  35.3828f;
            float testFloat5 = -23.4766f;
            float testFloat6 = -41.7188f;

            byte[] testBytes1 = floatToBytes(testFloat4);
            byte[] testBytes2 = floatToBytes(testFloat5);
            byte[] testBytes3 = floatToBytes(testFloat6);

            Console.WriteLine($"testBytes1 value is [{testBytes1[0]:X}, {testBytes1[1]:X}]");
            Console.WriteLine($"testBytes2 value is [{testBytes2[0]:X}, {testBytes2[1]:X}]");
            Console.WriteLine($"testBytes3 value is [{testBytes3[0]:X}, {testBytes3[1]:X}]");


            Console.ReadLine();
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
    }
}
