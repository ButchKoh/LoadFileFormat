using System.Collections;

public class PNG
{
    public static bool isPng;

    public class Chunk
    {
        private int _length;
        public int length { get { return _length; } set { _length = value; } }
        private string _chunkTypeStr;
        public string chunkTypeStr { get { return _chunkTypeStr; } set { _chunkTypeStr = value; } }
        private long _CRC;
        public long CRC { get { return _CRC; } set { _CRC = value; } }
        public static byte[] GetLengthFromChunk(byte[] IHDR)
        {
            return Util.CopyFromArray(IHDR, 0, 4, 0);
        }
        public static byte[] GetChunkTypeFromChunk(byte[] IHDR)
        {
            return Util.CopyFromArray(IHDR, 4, 4, 0);
        }
        #region Chunk Type判別用
        public static readonly byte[] IHDR = { 0x49, 0x48, 0x44, 0x52 };
        public static readonly byte[] PLTE = { 0x50, 0x4C, 0x54, 0x45 };
        public static readonly byte[] IDAT = { 0x49, 0x44, 0x41, 0x54 };
        public static readonly byte[] IEND = { 0x49, 0x45, 0x4E, 0x44 };
        public static readonly byte[] tRNS = { 0x74, 0x52, 0x4E, 0x53 };
        public static readonly byte[] gAMA = { 0x67, 0x41, 0x4D, 0x41 };
        public static readonly byte[] cHRM = { 0x63, 0x48, 0x52, 0x4D };
        public static readonly byte[] sRGB = { 0x73, 0x52, 0x47, 0x42 };
        public static readonly byte[] iCCP = { 0x69, 0x43, 0x43, 0x50 };
        public static readonly byte[] tEXt = { 0x74, 0x45, 0x58, 0x74 };
        public static readonly byte[] zTXt = { 0x7A, 0x54, 0x58, 0x74 };
        public static readonly byte[] iTXt = { 0x69, 0x54, 0x58, 0x74 };
        public static readonly byte[] bKGD = { 0x62, 0x4b, 0x47, 0x44 };
        public static readonly byte[] pHYs = { 0x70, 0x48, 0x59, 0x73 };
        public static readonly byte[] sBIT = { 0x73, 0x42, 0x49, 0x54 };
        public static readonly byte[] sPLT = { 0x73, 0x50, 0x4C, 0x54 };
        public static readonly byte[] hIST = { 0x68, 0x49, 0x53, 0x54 };
        public static readonly byte[] tIME = { 0x74, 0x49, 0x4D, 0x45 };
        #endregion
    }
    public class IHDR : Chunk
    {
        #region IHDR各項目のget,set等
        public new const int length = 25;
        private int _width;
        public int width { get { return _width; } set { _width = value; } }
        private int _height;
        public int height { get { return _height; } set { _height = value; } }
        private int _bitDepth;
        public int bitDepth { get { return _bitDepth; } set { _bitDepth = value; } }
        private int _colorTypeInt;
        public int colorTypeInt { get { return _colorTypeInt; } set { _colorTypeInt = value; } }
        public string colorTypeStr
        {
            get
            {
                if (_colorTypeInt == 0) return "Gray Scale";
                else if (_colorTypeInt == 2) return "True Color";
                else if (_colorTypeInt == 3) return "Index Color(require PLTE)";
                else if (_colorTypeInt == 4) return "Gray Scale + Alpha";
                else if (_colorTypeInt == 6) return "True Color + Alpha";
                else return "unknown";
            }
        }
        private int _compMethod;
        public int compMethodInt { get { return _compMethod; } set { _compMethod = value; } }
        public string compMethodStr
        {
            get
            {
                if (_compMethod == 0) return "Deflete";
                else return "unknown";
            }
        }
        private int _filterMethodInt;
        public int filterMethodInt { get { return _filterMethodInt; } set { _filterMethodInt = value; } }
        public string filterMethodStr
        {
            get
            {
                if (_filterMethodInt == 0) return "Filter before comp";
                else return "unknown";
            }
        }
        private int _interlaceMethodInt;
        public int interlaceMethodInt { get { return _interlaceMethodInt; } set { _interlaceMethodInt = value; } }
        public string interlaceMethodStr
        {
            get
            {
                if (_interlaceMethodInt == 0) return "invalid";
                else return "valid";
            }
        }
        public byte[] CRCByte = new byte[4];
        public string CRC_hex;
        public long calcCRC;
        #endregion
        public static byte[] GetWidthFromIHDR(byte[] IHDR)
        {
            return Util.CopyFromArray(IHDR, 8, 4, 0);
        }
        public static byte[] GetHeightFromIHDR(byte[] IHDR)
        {
            return Util.CopyFromArray(IHDR, 12, 4, 0);
        }
        public static byte[] GetCRCFromIHDR(byte[] IHDR)
        {
            return Util.CopyFromArray(IHDR, 21, 4, 0);
        }
        public static byte[] GetDataForCRCcalc(byte[] IHDR)
        {
            return Util.CopyFromArray(IHDR, 4, PNG.IHDR.length - 8, 0);
        }

    }
    public static byte[] GetPngHeaderFromWholeData(byte[] data)
    {
        return Util.CopyFromArray(data, 0, 8, 0);
    }
    public static byte[] GetIHDRFromWholeData(byte[] data)
    {
        return Util.CopyFromArray(data, 8, 25, 0);
    }
}