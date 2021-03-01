using System;
using System.IO;
using Ionic.Zlib;
public static class Util
{
    public static int ByteArray2Int32(byte[] data)
    {
        return Convert.ToInt32((BitConverter.ToString(data)).Replace("-", ""), 16);
    }
    public static long ByteArray2Int64(byte[] data)
    {
        return Convert.ToInt64((BitConverter.ToString(data)).Replace("-", ""), 16);
    }
    public static long HexString2Long(string data)
    {
        return Convert.ToInt64(data.Replace("-", ""), 16);
    }
    public static uint calcCRC(byte[] bytes)
    {
        uint[] CRCTable = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            var x = i;
            for (var j = 0; j < 8; j++)
            {
                x = (uint)((x & 1) == 0 ? x >> 1 : -306674912 ^ x >> 1);
            }
            CRCTable[i] = x;
        }
        uint num = uint.MaxValue;
        for (var i = 0; i < bytes.Length; i++)
        {
            num = CRCTable[(num ^ bytes[i]) & 255] ^ num >> 8;
        }
        return (uint)(num ^ -1);
    }
    public static byte[] Deflate(byte[] data)
    {
        int outputSize = 1024;
        byte[] output = new Byte[outputSize];
        int lengthToCompress = data.Length;

        //ZLIB stream   :true
        //DEFLATE stream:false
        bool wantRfc1950Header = true;

        using (MemoryStream ms = new MemoryStream())
        {
            ZlibCodec compressor = new ZlibCodec();
            compressor.InitializeDeflate(Ionic.Zlib.CompressionLevel.BestCompression, wantRfc1950Header);

            compressor.InputBuffer = data;
            compressor.AvailableBytesIn = lengthToCompress;
            compressor.NextIn = 0;
            compressor.OutputBuffer = output;

            foreach (var f in new FlushType[] { FlushType.None, FlushType.Finish })
            {
                int bytesToWrite = 0;
                do
                {
                    compressor.AvailableBytesOut = outputSize;
                    compressor.NextOut = 0;
                    compressor.Deflate(f);

                    bytesToWrite = outputSize - compressor.AvailableBytesOut;
                    if (bytesToWrite > 0)
                        ms.Write(output, 0, bytesToWrite);
                }
                while ((f == FlushType.None && (compressor.AvailableBytesIn != 0 || compressor.AvailableBytesOut == 0)) ||
                    (f == FlushType.Finish && bytesToWrite != 0));
            }

            compressor.EndDeflate();

            ms.Flush();
            return ms.ToArray();
        }
    }
    public static byte[] CopyFromArray(byte[] source, int index1, int length, int index2)
    {
        byte[] target = new byte[length];
        Array.Copy(source, index1, target, index2, length);
        return target;
    }
    public static string[] CopyFromArray(string[] source, int index1, int length, int index2)
    {
        string[] target = new string[length];
        Array.Copy(source, index1, target, index2, length);
        return target;
    }
    public static byte[] CopyFromArray(byte[] source, long index1, int length, long index2)
    {
        byte[] target = new byte[length];
        Array.Copy(source, index1, target, index2, length);
        return target;
    }
    public static byte[] InitialPart(byte[] source, int length)
    {
        byte[] target = new byte[length];
        Array.Copy(source, 0, target, 0, length);
        return target;
    }
    public static bool Compare(byte[] array1,byte[] array2)
    {
        return System.Collections.StructuralComparisons.StructuralEqualityComparer.Equals(array1, array2);
    }
    public static bool CheckMagicNumber(byte[] source,byte[] target)
    {
        return Compare(InitialPart(source, target.Length), target);
    }
    public static bool CheckMagicNumber2(byte[] source, byte[] target)
    {
        byte[] mn2 = new byte[4];
        Array.Copy(source, 8, mn2, 0, 4);
        return Compare(mn2, target);
    }
    public static Int32 ByteArray2Int32(byte[] source, int startIndex, int length)
    {
        byte[] tmp = new byte[length];
        Array.Copy(source, startIndex, tmp, 0, length);
        return Util.ByteArray2Int32(tmp);
    }
}