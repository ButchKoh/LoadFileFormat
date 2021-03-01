using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//参考
//https://en.wikipedia.org/wiki/List_of_file_signatures
public class Signatures 
{
    //ASCII table : https://www.k-cube.co.jp/wakaba/server/ascii_code.html
    public static readonly byte[] SHEBANG   = new byte[2] { 0x23, 0x21 };//#!
    public static readonly byte[] LIBCAP1   = new byte[4] { 0xa1, 0xb2, 0xc3, 0xd4 };
    public static readonly byte[] LIBCAP2   = new byte[4] { 0xd4, 0xc3, 0xb2, 0xa1 };
    public static readonly byte[] PCAPNG    = new byte[4] { 0x0a, 0x0d, 0x0d, 0x0a };
    public static readonly byte[] SQLITE    = new byte[16]{ 0x53, 0x51, 0x4c, 0x69, 0x74, 0x65, 0x20, 0x66, 0x6f, 0x72, 0x6d, 0x61, 0x74, 0x20, 0x33, 0x00 };//SQLite format 3 
    public static readonly byte[] ICON      = new byte[4] { 0x00, 0x00, 0x01, 0x00 };
    public static readonly byte[] t3GPP     = new byte[6] { 0x66, 0x74, 0x79, 0x70, 0x33, 0x67 };//ftyp3g
    public static readonly byte[] LZWcomp   = new byte[2] { 0x1F, 0x9D };
    public static readonly byte[] LZHcomp   = new byte[2] { 0x1F, 0xA0 };
    public static readonly byte[] BZIP2comp = new byte[3] { 0x42, 0x5A, 0x68 };//BZh
    public static readonly byte[] GIF87a    = new byte[6] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };//GIF87a
    public static readonly byte[] GIF89a    = new byte[6] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };//GIF89a
    public static readonly byte[] TIFF_LE   = new byte[4] { 0x49, 0x49, 0x2A, 0x00 };
    public static readonly byte[] TIFF_BE   = new byte[4] { 0x4D, 0x4D, 0x00, 0x2A };
    public static readonly byte[] CANONraw  = new byte[10]{ 0x49, 0x49, 0x2A, 0x00, 0x10, 0x00, 0x00, 0x00, 0x43, 0x52 };//TIFF_LE+末尾2B=CR
    public static readonly byte[] OpenEXR   = new byte[4] { 0x76, 0x2F, 0x31, 0x01 };
    public static readonly byte[] JPEG      = new byte[3] { 0xFF, 0xD8, 0xFF };
    public static readonly byte[] LZIP      = new byte[4] { 0x4C, 0x5A, 0x49, 0x50 };//LZIP
    public static readonly byte[] DOS_MZ    = new byte[2] { 0x4D, 0x5A };//MZ
    public static readonly byte[] ZIPbased  = new byte[4] { 0x50, 0x4B, 0x03, 0x04 };//PK 0x03 0x04
    public static readonly byte[] ZIPea     = new byte[4] { 0x50, 0x4B, 0x05, 0x06 };//PK 0x05 0x06//(empty archive)
    public static readonly byte[] ZIPsa     = new byte[4] { 0x50, 0x4B, 0x07, 0x08 };//PK 0x07 0x08//(spanned archive)
    public static readonly byte[] RAR1_5    = new byte[7] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 };//Rar![SUB][BEL][NULL]
    public static readonly byte[] RAR5_0    = new byte[8] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 };//Rar![SUB][BEL][SOH][NULL]
    public static readonly byte[] PNG       = new byte[8] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };//89 'PNG' 0d 0a 1a 0a
    public static readonly byte[] PDF       = new byte[5] { 0x25, 0x50, 0x44, 0x46, 0x2d };//%PDF-
    public static readonly byte[] Win_ASF   = new byte[16]{ 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9, 0x00, 0xAA, 0x00, 0x62, 0xCE, 0x6C };
    public static readonly byte[] OGG       = new byte[4] { 0x4F, 0x67, 0x67, 0x53 };//OggS
    public static readonly byte[] PHOTOSHOP = new byte[4] { 0x38, 0x42, 0x50, 0x53 };//8BPS

    //RIFF : https://ja.wikipedia.org/wiki/Resource_Interchange_File_Format
    //wave avi webp cda qcp...
    //Signature ->RIFF + (file size->4[B]) + (Type->4[B]/example:WAVE)
    public static readonly byte[] RIFF = new byte[4] { 0x52, 0x49, 0x46, 0x46 };//RIFF
    public static readonly byte[] WAVE = new byte[4] { 0x57, 0x41, 0x56, 0x45 };//WAVE
    public static readonly byte[] AVI  = new byte[4] { 0x41, 0x56, 0x49, 0x20 };//AVI 
    public static readonly byte[] WEBP = new byte[4] { 0x57, 0x45, 0x42, 0x50 };//WEBP
    public static readonly byte[] CDA  = new byte[4] { 0x43, 0x44, 0x44, 0x41 };//CDDA
    public static readonly byte[] QCP  = new byte[4] { 0x51, 0x4C, 0x43, 0x4D };//QLCM
    public static readonly byte[] ANI  = new byte[4] { 0x41, 0x43, 0x4F, 0x4E };//ACON

    public static readonly byte[] MP3_ID3v1_1      = new byte[2] { 0xFF, 0xFB };
    public static readonly byte[] MP3_ID3v1_2      = new byte[2] { 0xFF, 0xF3 };
    public static readonly byte[] MP3_ID3v1_3      = new byte[2] { 0xFF, 0xF2 };
    public static readonly byte[] MP3_ID3v2        = new byte[3] { 0x49, 0x44, 0x33 };//ID3
    public static readonly byte[] BMP              = new byte[2] { 0x42, 0x4D };//BM
    public static readonly byte[] FLAC             = new byte[4] { 0x66, 0x4C, 0x61, 0x43 };//fLaC
    public static readonly byte[] MIDI             = new byte[4] { 0x4D, 0x54, 0x68, 0x64 };//MThd
    public static readonly byte[] CFBC_oldMSOffice = new byte[8] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
    public static readonly byte[] GZIP             = new byte[2] { 0x1F, 0x8B };
    public static readonly byte[] WEBM             = new byte[4] { 0x1A, 0x45, 0xDF, 0xA3 };
    public static readonly byte[] XML              = new byte[6] { 0x3c, 0x3f, 0x78, 0x6d, 0x6c, 0x20 };//<?xml 
    public static readonly byte[] RTF              = new byte[6] { 0x7B, 0x5C, 0x72, 0x74, 0x66, 0x31 };//{\rtf1
    public static readonly byte[] MP4              = new byte[12] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D };
    public static readonly byte[] FLV              = new byte[3] { 0x46, 0x4C, 0x56 };//FLV
    public static readonly byte[] WinRegistry      = new byte[4] { 0x72, 0x65, 0x67, 0x66 };//regf

    public const string SHEBANGdesc     = "shebang\nScript or data to be passed to the program following the shebang (#!)";
    public const string LIBCAP1desc     = "";
    public const string LIBCAP2desc     = "";
    public const string PCAPNGdesc      = "";
    public const string SQLITEdesc      = "sqlitedb/sqlite/db\nSQLite Database";
    public const string ICONdesc        = "ico\nComputer icon encoded";
    public const string t3GPPdesc       = "3gp/3g2\n3rd Generation Partnership Project 3GPP and 3GPP2 multimedia files";
    public const string LZWcompdesc     = "z/tar.z\ncompressed file (often tar zip) using Lempel-Ziv - Welch algorithm ";
    public const string LZHcompdesc     = "z/tar.z\ncompressed file (often tar zip) using LZH algorithm ";
    public const string BZIP2compdesc   = "bz2\nCompressed file using Bzip2 algorithm";
    public const string GIF87adesc      = "gif\nImage file encoded in the Graphics Interchange Format";
    public const string GIF89adesc      = "gif\nImage file encoded in the Graphics Interchange Format";
    public const string TIFF_LEdesc     = "tif/tiff\nTagged Image File Format";
    public const string TIFF_BEdesc     = "tif/tiff\nTagged Image File Format";
    public const string CANONrawdesc    = "cr2\nCanon's RAW format based on TIFF";
    public const string OpenEXRdesc     = "exr\nOpenEXR image";
    public const string JPEGdesc        = "jpg/jpeg\nJPEG raw or in the JFIF or Exif file format";
    public const string LZIPdesc        = "lz\nlzip compressed file";
    public const string DOS_MZdesc      = "exe/dll\nDOS MZ executable file format and its descendants (including NE and PE)";
    public const string ZIPbaseddesc    = "zip/aar/apk/docx/epub/ipa/jar/kmz/maff/odp/ods/odt/pk3/pk4/pptx/usdz/vsdx/xlsx/xpi\nzip file format and formats based on it, such as EPUB, JAR, ODF, OOXML.";
    public const string ZIPeadesc       = "zip/aar/apk/docx/epub/ipa/jar/kmz/maff/odp/ods/odt/pk3/pk4/pptx/usdz/vsdx/xlsx/xpi\nzip file format and formats based on it, such as EPUB, JAR, ODF, OOXML.\n(empty archive)";
    public const string ZIPsadesc       = "zip/aar/apk/docx/epub/ipa/jar/kmz/maff/odp/ods/odt/pk3/pk4/pptx/usdz/vsdx/xlsx/xpi\nzip file format and formats based on it, such as EPUB, JAR, ODF, OOXML.\n(spanned archive)";
    public const string RAR1_5desc      = "rar\nRAR archive version 1.50 onwards";
    public const string RAR5_0desc      = "rar\nRAR archive version 5.0 onwards";
    public const string PNGdesc         = "png\nImage encoded in the Portable Network Graphics format";
    public const string PDFdesc         = "pdf\nPDF document";
    public const string Win_ASFdesc     = "asf/wma/wmv\nAdvanced Systems Format";
    public const string OGGdesc         = "ogg/oga/ogv\nOgg, an open source media container format";
    public const string PHOTOSHOPdesc   = "psd\nPhotoshop Document file, Adobe Photoshop's native file format";
    public const string WAVEdesc        = "wav\nWaveform Audio File Format";
    public const string AVIdesc         = "avi\nAudio Video Interleave video format";
    public const string WEBPdesc        = "webp\nGoogle WebP image file";
    public const string ANIdesc         = "ani\nAnimated cursor";
    public const string CDAdesc         = "cda\nCompact Disc Digital Audio";
    public const string QCPdesc         = "qcp\nQualcomm PureVoice file format";
    public const string MP3_ID3v1_1desc = "mp3\nMPEG-1 Layer 3 file without an ID3 tag or with an ID3v1 tag";
    public const string MP3_ID3v1_2desc = "mp3\nMPEG-1 Layer 3 file without an ID3 tag or with an ID3v1 tag";
    public const string MP3_ID3v1_3desc = "mp3\nMPEG-1 Layer 3 file without an ID3 tag or with an ID3v1 tag";
    public const string MP3_ID3v2desc   = "mp3\nMP3 file with an ID3v2 container";
    public const string BMPdesc         = "bmp/dib\nBMP file, a bitmap format used mostly in the Windows world";
    public const string FLACdesc        = "flac\nFree Lossless Audio Codec";
    public const string MIDIdesc        = "mid/midi\nMIDI sound file";
    public const string CFBC_oldMSOfficedesc = "doc/xls/ppt/msg\nCompound File Binary Format, a container format used for document by older versions of Microsoft Office(doc/xls/ppt/msg). It is however an open format used by other programs as well.";
    public const string GZIPdesc        = "gz/tar.gz\nGZIP compressed file";
    public const string WEBMdesc        = "mkv/mka/mks/mk3d/webm\nMatroska media container, including WebM";
    public const string XMLdesc         = "XML\neXtensible Markup Language when using the ASCII character encoding";
    public const string RTFdesc         = "rtf\nRich Text Format";
    public const string MP4desc         = "mp4\nISO Base Media file (MPEG-4)";
    public const string FLVdesc         = "flv\nFlash Video file";
    public const string WinRegistrydesc = "dat\nWindows Registry file(dat)";
}
