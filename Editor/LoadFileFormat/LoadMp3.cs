using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;


/// 参考
/// http://takaaki.info/works/id3info/
/// https://id3.org/Developer%20Information
/// https://www.diva-portal.org/smash/get/diva2:830195/FULLTEXT01.pdf
/// https://id3.org/Developer%20Information
/// http://www.mp3-tech.org/programmer/frame_header.html
/// https://akabeko.me/blog/memo/mp3/id3v2-frame-detail/
/// 

public class LoadMp3
{
    public string tmpLog = "";
    string log = "";
    int byteCount = 0;
    int tagEnd;

    int[,] BitrateTable = new int[5, 16] { { 0,32,64,96,128,160,192,224,256,288,320,352,384,416,448,1111},
                                           { 0,32,48,56, 64, 80, 96,112,128,160,192,224,256,320,384,1111},
                                           { 0,32,40,48, 56, 64, 80, 96,112,128,160,192,224,256,320,1111},
                                           { 0,32,48,56, 64, 80, 96,112,128,144,160,176,192,224,256,1111},
                                           { 0, 8,16,24, 32, 40, 48, 56, 64, 80, 96,112,128,144,160,1111} };
    int[,] SamplingRateTable = new int[3, 4] { { 44100, 48000, 32000, 0 },
                                               { 22050, 24000, 16000, 0 },
                                               { 11025, 12000,  8000, 0 } };
    string[] Genre = new string[192]
                    {"Blues",                 "Classic Rock",     "Country",         "Dance",           "Disco",          "Funk",               "Grunge",               "Hip-Hop",          "Jazz",          "Metal",
                     "New Age",               "Oldies",           "Other",           "Pop",             "R&B",            "Rap",                "Reggae",               "Rock",             "Techno",        "Industrial",
                     "Alternative",           "Ska",              "Death Metal",     "Pranks",          "Soundtrack",     "Euro-Techno",        "Ambient",              "Trip-Hop",         "Vocal",         "Jazz-funk",
                     "Fusion",                "Trance",           "Classical",       "Instrumental",    "Acid",           "House",              "Game",                 "Sound Clip",       "Gospel",        "Noise",
                     "Alt. Rock",             "Bass",             "Soul",            "Punk",            "Space",          "Meditative",         "Instrumental pop",     "Instrumental rock","Ethnic",        "Gothic",
                     "Darkwave",              "Techno-Industrial","Electronic",      "Pop-folk",        "Eurodance",      "Dream",              "Southern Rock",        "Comedy",           "Cult",          "Gangsta",    
                     "Top 40",                "Christian Rap",    "Pop/Funk",        "Jungle",          "Native American","Cabaret",            "New Wave",             "Psychedelic",      "Rave",          "Showtunes",  
                     "Trailer",               "Lo-Fi",            "Tribal",          "Acid Punk",       "Acid Jazz",      "Polka",              "Retro",                "Musical",          "Rock & Roll",   "Hard Rock",  
                     "Folk",                  "Folk-Rock",        "National Folk",   "Swing",           "Fast Fusion",    "Bebob",              "Latin",                "Revival",          "Celtic",        "Bluegrass",  
                     "Avantgarde",            "Gothic Rock",      "Progressive Rock","Psychedelic Rock","Symphonic Rock", "Slow Rock",          "Big Band",             "Chorus",           "Easy Listening","Acoustic",   
                     "Humour",                "Speech",           "Chanson",         "Opera",           "Chamber Music",  "Sonata",             "Symphony",             "Booty Bass",       "Primus",        "Porn Groove",
                     "Satire",                "Slow Jam",         "Club",            "Tango",           "Samba",          "Folklore",           "Ballad",               "Power Ballad",     "Rhythmic Soul", "Freestyle",  
                     "Duet",                  "Punk Rock",        "Drum Solo",       "A capella",       "Euro-House",     "Dance Hal",          "Goa",                  "Drum & Bass",      "Club-House",    "Hardcore",   
                     "Terror",                "Indie",            "BritPop",         "Afro-punk",       "Polsk Punk",     "Beat",               "Christian gangsta rap","Heavy Metal",      "Black Metal",   "Crossover",  
                     "Contemporary Christian","Christian Rock",   "Merengue",        "Salsa",           "Thrash Metal",   "Anime",              "JPop",                 "Synthpop",         "Abstract",      "Art Rock",   
                     "Baroque",               "Bhangra",          "Big beat",        "Breakbeat",       "Chillout",       "Downtempo",          "Dub",                  "EBM",              "Eclectic",      "Electro",    
                     "Electroclash",          "Emo",              "Experimental",    "Garage",          "Global",         "IDM",                "Illbient",             "Industro-Goth",    "Jam Band",      "Krautrock",  
                     "Leftfield",             "Lounge",           "Math Rock",       "New Romantic",    "Nu-Breakz",      "Post-Punk",          "Post-Rock",            "Psytrance",        "Shoegaze",      "Space Rock", 
                     "Trop Rock",             "World Music",      "Neoclassical",    "Audiobook",       "Audio theatre",  "Neue Deutsche Welle","Podcast",              "Indie-Rock",       "G-Funk",        "Dubstep",    
                     "Garage Rock",           "Psybient" };
    System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
    System.Text.UnicodeEncoding unicode = new System.Text.UnicodeEncoding(bigEndian: false, byteOrderMark: true);
    System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();
    System.Text.Encoding unicodebe = System.Text.Encoding.GetEncoding(1201);//unicode big endian
    System.Text.Encoding ISO8859_1 = System.Text.Encoding.GetEncoding(28591);//ISO 8859-1
    System.Text.Encoding ISO2201_jp = System.Text.Encoding.GetEncoding(50220);//日本語euc-jp
    System.Text.Encoding Shift_Jis = System.Text.Encoding.GetEncoding(932);//shift-jis

    public string LoadID3Info(byte[] data)
    {
        byteCount = 0;

        //ID3v2
        if (ascii.GetString(new byte[] { data[0], data[1], data[2] }) == "ID3")
        {
            tagEnd = ((data[6] << 21) | (data[7] << 14) | (data[8] << 7) | data[9]) + 10;
            log += "[METADATA]\n";
            log += " - tag\t: ID3v2." + data[3] + "." + data[4] + "\n";
            log += " - flags\t:" + string.Format("{0,8}", Convert.ToString(data[5], 2)).Replace(" ", "0") + "\n";
            log += "\t - Unsynchronisation:".PadRight(25) + (data[5] >> 7 == 1 ? true : false) + "\n";
            log += "\t - Extended header:".PadRight(25) + (data[5] >> 6 == 1 ? true : false) + "\n";
            log += "\t - Experimental indicator:".PadRight(25) + (data[5] >> 5 == 1 ? true : false) + "\n";
            log += "\t - Footer present:".PadRight(25) + (data[5] >> 4 == 1 ? true : false) + "\n";
            log += "\t - tag size:".PadRight(25) + tagEnd + "\n\n";
            byteCount += 10;
            while (byteCount <= tagEnd)
            {
                try
                {
                    EditorUtility.DisplayProgressBar("[1/2] Loading File Content", "Wait a second. (" + byteCount + " / " + data.Length + ")", (float)byteCount / data.Length);
                    byte[] frameHead = new byte[] { data[byteCount], data[byteCount + 1], data[byteCount + 2], data[byteCount + 3] };
                    string frameType = ascii.GetString(frameHead);
                    int tmpLength = BitConverter.ToInt32(new byte[] { data[byteCount + 7], data[byteCount + 6], data[byteCount + 5], data[byteCount + 4] }, 0);
                    
                    //Audioデータに入ったらループを抜ける
                    if (Util.CheckMagicNumber(frameHead, Signatures.MP3_ID3v1_1) | Util.CheckMagicNumber(frameHead, Signatures.MP3_ID3v1_2) | Util.CheckMagicNumber(frameHead, Signatures.MP3_ID3v1_3)) break;

                    log += frameType + ":";
                    byte[] frame = new byte[tmpLength];
                    Array.Copy(data, byteCount + 10, frame, 0, tmpLength);
                    byte[] frameOriginal = Util.CopyFromArray(frame, 0, frame.Length, 0);
                    switch (frameType)
                    {
                        case "AENC":
                            log += "Audio encryption\n";
                            break;
                        case "APIC":
                            log += "Attached picture\n";
                            break;
                        case "ASPI"://Added in v2.4
                            log += "Audio seek point index\n";
                            break;
                        case "COMM":
                            log += "Comments\n";
                            break;
                        case "COMR":
                            log += "Commercial frame\n";
                            break;
                        case "ENCR":
                            log += "Encryption method registration\n";
                            break;
                        case "EQUA": //->EQU2(v2.4)
                            log += "Equalization\n";
                            break;
                        case "EQU2"://Added in v2.4
                            log += "Equalization\n";
                            break;
                        case "ETCO":
                            log += "Event timing codes\n";
                            break;
                        case "GEOB":
                            log += "General encapsulated object\n";
                            break;
                        case "GRID":
                            log += "Group identification registration\n";
                            break;
                        case "IPLS": //->TMCL+TIPL(v2.4)
                            log += "Involved people list\n";
                            break;
                        case "LINK":
                            log += "Linked information\n";
                            break;
                        case "MCDI":
                            log += "Music CD identifier\n";
                            break;
                        case "MLLT":
                            log += "MPEG location lookup table\n";
                            break;
                        case "OWNE":
                            log += "Ownership frame\n";
                            break;
                        case "PRIV":
                            log += "Private frame\n";
                            break;
                        case "PCNT":
                            log += "Play counter\n";
                            break;
                        case "POPM":
                            log += "Popularimeter\n";
                            break;
                        case "POSS":
                            log += "Position synchronisation frame\n";
                            break;
                        case "RBUF":
                            log += "Recommended buffer size\n";
                            break;
                        case "RGAD"://UNOFFICIAL
                            log += "Replay gain adjustment(UNOFFICIAL)\n";
                            break;
                        case "RVAD"://->RVA2(v2.4)
                            log += "Relative volume adjustment\n";
                            break;
                        case "RVA2"://Added in v2.4
                            log += "Relative volume adjustment\n";
                            break;
                        case "RVRB":
                            log += "Reverb\n";
                            break;
                        case "SEEK"://Added in v2.4
                            log += "Seek frame\n";
                            break;
                        case "SIGN"://Added in v2.4
                            log += "Signature frame\n";
                            break;
                        case "SYLT":
                            log += "Synchronized lyric/text\n";
                            break;
                        case "SYTC":
                            log += "Synchronized tempo codes\n";
                            break;
                        case "TALB":
                            log += "Album/Movie/Show title\n";//
                            break;
                        case "TBPM":
                            log += "Beats per minute (BPM)\n";//
                            break;
                        case "TCMP"://UNOFFICIAL
                            log += "iTunes compilation flag(UNOFFICIAL)\n";
                            break;
                        case "TCOM":
                            log += "Composer\n";//
                            break;
                        case "TCON":
                            log += "Content type\n";//
                            break;
                        case "TCOP":
                            log += "Copyright message\n";//
                            break;
                        case "TDAT"://->TDRC(v2.4)
                            log += "Recording Date\n";//
                            break;
                        case "TDEN"://Added in v2.4//
                            log += "Encoding time\n";
                            break;
                        case "TDLY":
                            log += "Playlist delay\n";//
                            break;
                        case "TDOR"://Added in v2.4(<-TORY)
                            log += "Original release year\n";
                            break;
                        case "TDRC"://Added in v2.4(<-TYER+TDAT+TIME+TRDA)
                            log += "Recordning Date\n";
                            break;
                        case "TDRL"://Added in v2.4//
                            log += "Release time\n";
                            break;
                        case "TDTG"://Added in v2.4//
                            log += "Tagging time\n";
                            break;
                        case "TENC":
                            log += "Encoded by\n";//
                            break;
                        case "TEXT":
                            log += "Lyricist/Text writer\n";//
                            break;
                        case "TFLT":
                            log += "File type\n";//
                            break;
                        case "TIME"://->TDRC(v2.4)
                            log += "Time\n";//
                            break;
                        case "TIPL"://Added in v2.4//
                            log += "Involved people list\n";
                            break;
                        case "TIT1":
                            log += "Content group description\n";//
                            break;
                        case "TIT2":
                            log += "Title/songname/content description\n";//
                            break;
                        case "TIT3":
                            log += "Subtitle/Description refinement\n";//
                            break;
                        case "TKEY":
                            log += "Initial key\n";//
                            break;
                        case "TLAN":
                            log += "Language(s)\n";//
                            break;
                        case "TLEN":
                            log += "Length\n";//
                            break;
                        case "TMCL"://Added in v2.4//
                            log += "Musician credits list\n";
                            break;
                        case "TMED":
                            log += "Media type\n";//
                            break;
                        case "TMOO"://Added in v2.4
                            log += "Mood\n";//
                            break;
                        case "TOAL":
                            log += "Original album/movie/show title\n";//
                            break;
                        case "TOFN":
                            log += "Original filename\n";//
                            break;
                        case "TOLY":
                            log += "Original lyricist(s)/text writer(s)\n";//
                            break;
                        case "TOPE":
                            log += "Original artist(s)/performer(s)\n";//
                            break;
                        case "TORY"://->TDOR(v2.4)
                            log += "Original release year\n";//
                            break;
                        case "TOWN":
                            log += "File owner/licensee\n";//
                            break;
                        case "TPE1":
                            log += "Lead performer(s)/Soloist(s)\n";//
                            break;
                        case "TPE2":
                            log += "Band/orchestra/accompaniment\n";//
                            break;
                        case "TPE3":
                            log += "Conductor/performer refinement\n";//
                            break;
                        case "TPE4":
                            log += "Interpreted, remixed, or otherwise modified by\n";//
                            break;
                        case "TPOS":
                            log += "Part of a set\n";//
                            break;
                        case "TPRO"://Added in v2.4
                            log += "Produced notice\n";//
                            break;
                        case "TPUB":
                            log += "Publisher\n";//
                            break;
                        case "TRCK":
                            log += "Track number/Position in set\n";//
                            break;
                        case "TRDA"://->TDRC(v2.4)
                            log += "Recording dates(supplemental)\n";//
                            break;
                        case "TRSN":
                            log += "Internet radio station name\n";//
                            break;
                        case "TRSO":
                            log += "Internet radio station owner\n";//
                            break;
                        case "TSIZ"://(v2.3only!!)
                            log += "Size\n";//
                            break;
                        case "TSOA"://Added in v2.4
                            log += "Album sort order\n";
                            break;
                        case "TSOC"://UNOFFICIAL
                            log += "iTunes composer sort ordrer(UNOFFICIAL)\n";
                            break;
                        case "TSOP"://Added in v2.4
                            log += "Performer sort order\n";
                            break;
                        case "TSOT"://Added in v2.4
                            log += "Title sort order\n";
                            break;
                        case "TSO2"://UNOFFICIAL
                            log += "iTunes sort order(UNOFFICIAL)\n";
                            break;
                        case "TSRC":
                            log += "International Standard Recording Code (ISRC)\n";//
                            break;
                        case "TSSE":
                            log += "Software/Hardware and settings used for encoding\n";//
                            break;
                        case "TSST"://Added in v2.4
                            log += "Set subtitle\n";
                            break;
                        case "TYER"://->TDRC(v2.4)
                            log += "Year\n";//
                            break;
                      //"TXXX":ユーザ定義フレーム
                        case "UFID":
                            log += "Unique file identifier\n";
                            break;
                        case "USER":
                            log += "Terms of use\n";
                            break;
                        case "USLT":
                            log += "Unsynchronized lyric/text transcription\n";
                            break;

                        case "WCOM":
                            log += "Commercial information\n";
                            break;
                        case "WCOP":
                            log += "Copyright/Legal information\n";
                            break;
                        case "WOAF":
                            log += "Official audio file webpage\n";
                            break;
                        case "WOAR":
                            log += "Official artist/performer webpage\n";
                            break;
                        case "WOAS":
                            log += "Official audio source webpage\n";
                            break;
                        case "WORS":
                            log += "Official internet radio station homepage\n";
                            break;
                        case "WPAY":
                            log += "Payment\n";
                            break;
                        case "WPUB":
                            log += "Publishers official webpage\n";
                            break;
                        case "XRVA":
                            log += "Experimental RVA2";
                            break;
                      //"WXXX":ユーザ定義フレーム
                        default:
                            log += "User Defined/Not Defined\n";
                            break;
                    }
                    log +=　"\tlength : " + tmpLength + "(" + (byteCount + 9) + "-" + (byteCount + tmpLength + 9) + ")\n";
                    log += "\tflags : " + data[byteCount + 8] + "-" + data[byteCount + 9] + "\n";
                    if (frameType.Trim().StartsWith("T")| frameType.Trim().StartsWith("W"))
                    {
                        byte[] textFrame = Util.CopyFromArray(frame, 1, frame.Length - 1, 0);

                        //文字コード周りの問題： https:www.wa2c.com/wp/2015/11/23131700
                        if (textFrame.Length == 0 & frame[0] == 0x00) log += "\tvalue is empty:\n";
                        else if (frame[0] == 0 | frame[0] == 0x20) log += "\tISO 8859-1".PadRight(20) + " : " + ISO8859_1.GetString(textFrame) + "\n";
                        else if (frame[0] == 1) log += "\tUTF-16".PadRight(20) + " : " + unicode.GetString(textFrame) + "\n";
                        else if (frame[0] == 2) log += "\tUTF-16BE".PadRight(20) + " : " + unicodebe.GetString(textFrame) + "\n";
                        else if (frame[0] == 3) log += "\tUTF-8".PadRight(20) + " : " + utf8.GetString(textFrame) + "\n";
                        else
                        {
                            log += "\tThis frame does't contain encoding specification.\n";
                            log += "\tISO 8859-1".PadRight(11) + " : " + ISO8859_1.GetString(frame) + "\n";
                            log += "\tUTF-16".PadRight(11)     + " : " + unicode.GetString(frame) + "\n";
                            log += "\tUTF-16BE".PadRight(11)   + " : " + unicodebe.GetString(frame) + "\n";
                            log += "\tUTF-8".PadRight(11)      + " : " + utf8.GetString(frame) + "\n";
                            log += "\tShift-JIS".PadRight(11)  + " : " + Shift_Jis.GetString(frame) + "\n";
                        }
                        log += "\n";
                        byteCount += 10 + tmpLength;
                    }
                    else if (frameType == "POPM")
                    {
                        byte[] textFrame = Util.CopyFromArray(frame, 0, Array.IndexOf(frame, (byte)0), 0);
                        log += "\t" + ISO8859_1.GetString(textFrame) + " : "  +(Mathf.Round((int)frame[frame.Length - 1] / 64) + 1)+ "\n\n";
                        /*   0 -31:1
                         *  32- 95:2
                         *  96-159:3
                         * 160-223:4
                         * 224-255:5
                         */
                        byteCount += 10 + tmpLength;

                    }
                    else if (frameType == "APIC")
                    {
                        log += "\tframe size : " + frame.Length + "[B]\n\n";
                        byteCount += 10 + tmpLength;
                    }
                    else if (frameType == "UFID")
                    {
                        log += "\t" + utf8.GetString(frame).Replace("\n", "\n\t") + "\n\n";
                        byteCount += 10 + tmpLength;
                    }
                    else if (frameType == "GEOB")
                    {
                        string tmpStr = ISO8859_1.GetString(frame).Replace("  ", " ");
                        while (tmpStr.StartsWith(" ")) tmpStr = tmpStr.Substring(1);
                        string[] sep = tmpStr.Split(' ');
                        log += "\tMIME type : "   + sep[0] + "\n";
                        log += "\tname : "        + sep[1] + "\n";
                        log += "\tdescription : " + sep[2] + "\n\n";
                        byteCount += 10 + tmpLength;
                    }
                    else if (frameType == "COMM")
                    {
                        log += "\tlanguage : " + ISO8859_1.GetString(Util.CopyFromArray(frame, 1, 3, 0)) + "\n";
                        log += "\tencoding : " + frame[0] + "\n";
                        byte[] tmp1 = Util.CopyFromArray(frame, 4, frame.Length - 4, 0);
                        if (frame[0] == 0)
                        {
                            byte[] tmp2 = Util.CopyFromArray(tmp1, 0, Array.IndexOf(tmp1, (byte)0x00), 0);
                            byte[] tmp3 = Util.CopyFromArray(tmp1, Array.IndexOf(tmp1, (byte)0x00) + 1, tmp1.Length - tmp2.Length - 1, 0);
                            log += "\tdescription : " + ISO8859_1.GetString(tmp2) + "\n";
                            log += "\tcontent : " + ISO8859_1.GetString(tmp3) + "\n";
                        }
                        else if (frame[0] == 1)
                        {
                            byte[] tmp2 = Util.CopyFromArray(tmp1, 0, Array.IndexOf(tmp1, (byte)0x00), 0);
                            byte[] tmp3 = Util.CopyFromArray(tmp1, Array.IndexOf(tmp1, (byte)0x00) + 2, tmp1.Length - tmp2.Length - 2, 0);
                            log += "\tdescription : " + unicode.GetString(tmp2) + "\n";
                            log += "\tcontent : " + unicode.GetString(tmp3) + "\n";
                        }
                        else if (frame[0] == 2)
                        {
                            byte[] tmp2 = Util.CopyFromArray(tmp1, 0, Array.IndexOf(tmp1, (byte)0x00), 0);
                            byte[] tmp3 = Util.CopyFromArray(tmp1, Array.IndexOf(tmp1, (byte)0x00) + 2, tmp1.Length - tmp2.Length - 2, 0);
                            log += "\tdescription : " + unicodebe.GetString(tmp2) + "\n";
                            log += "\tcontent : " + unicodebe.GetString(tmp3) + "\n";
                        }
                        else if (frame[0] == 3)
                        {
                            byte[] tmp2 = Util.CopyFromArray(tmp1, 0, Array.IndexOf(tmp1, (byte)0x00), 0);
                            byte[] tmp3 = Util.CopyFromArray(tmp1, Array.IndexOf(tmp1, (byte)0x00) + 2, tmp1.Length - tmp2.Length - 2, 0);
                            log += "\tdescription : " + utf8.GetString(tmp2) + "\n";
                            log += "\tcontent : " + utf8.GetString(tmp3) + "\n";
                        }
                        else
                        {
                            log += "\tThis frame does't contain encoding specification." + frame[0] + "\n";
                            log += "\tISO-5559-1".PadRight(11) + " : " + ISO8859_1.GetString(frame) + "\n";
                            log += "\tUTF-16".PadRight(11)     + " : " + unicode.GetString(frame) + "\n";
                            log += "\tUTF-16BE".PadRight(11)   + " : " + unicodebe.GetString(frame) + "\n";
                            log += "\tUTF-8".PadRight(11)      + " : " + utf8.GetString(frame) + "\n";
                            log += "\tShift-JIS".PadRight(11)  + " : " + Shift_Jis.GetString(frame) + "\n";
                        }
                        log += "\n";
                        byteCount += 10 + tmpLength;
                    }
                    else
                    {
                        log += "\t" + BitConverter.ToString(Util.CopyFromArray(data, byteCount, 4, 0)) + "\n";
                        log += "\t" + frameType + "---\n";
                        byteCount += 10 + tmpLength;
                        break;
                    }
                }
                
                catch (Exception)
                {
                    //Debug.Log(e);
                    break;
                }
                finally
                {

                }
            }
        }
        else if (data[0] == 0xFF & (data[1] == 0xFB | data[1] == 0xF3 | data[1] == 0xF2))
        {
            byte[] TAG = Util.CopyFromArray(data, data.Length - 1 - 128, 128, 0);
            byte[] TAGplus = Util.CopyFromArray(data, data.Length - 1 - 227, 227, 0);
            if (ISO8859_1.GetString(Util.InitialPart(TAG, 3)) == "TAG") {  
                log += "[METADATA]\n";
                log += " - tag\t: ID3v1\n";
                log += "Title : " + ISO8859_1.GetString(Util.CopyFromArray(TAG, 3, 30, 0)) + "\n";
                log += "Artist : " + ISO8859_1.GetString(Util.CopyFromArray(TAG, 33, 30, 0)) + "\n";
                log += "Album : " + ISO8859_1.GetString(Util.CopyFromArray(TAG, 63, 30, 0)) + "\n";
                log += "Date : " + ISO8859_1.GetString(Util.CopyFromArray(TAG, 93, 4, 0)) + "\n";
                log += "Comment : " + ISO8859_1.GetString(Util.CopyFromArray(TAG, 97, 30, 0)) + "\n";
                if(TAG[125]==(byte)0x00) log += "Track : " + TAG[126].ToString() + "\n";
                log += "Genre : " + Genre[(uint)TAG[127]] + "\n\n";
            }
            else if (ISO8859_1.GetString(Util.InitialPart(TAGplus, 4)) == "TAG+")
            {
                log += "[METADATA]\n";
                log += " - tag\t: ID3v1\n";
                log += "Title : " + ISO8859_1.GetString(Util.CopyFromArray(TAGplus, 4, 60, 0)) + "\n";
                log += "Artist : " + ISO8859_1.GetString(Util.CopyFromArray(TAGplus, 64, 60, 0)) + "\n";
                log += "Album : " + ISO8859_1.GetString(Util.CopyFromArray(TAGplus, 124, 60, 0)) + "\n";
                log += "Speed : " + (TAG[184] == 0 ? "Unspecified" : TAGplus[184] == 1 ? "Slow" : TAGplus[184] == 2 ? "Middle" : 
                                                                     TAGplus[184] == 3 ? "Fast" : TAGplus[184] == 4 ? "Hard Core" : "Unknown") + "\n";
                log += "Genre : " + ISO8859_1.GetString(Util.CopyFromArray(TAGplus, 185, 30, 0)) + "\n";
                log += "Start : " + ISO8859_1.GetString(Util.CopyFromArray(TAGplus, 215, 6, 0)) + "\n";
                log += "End : "   + ISO8859_1.GetString(Util.CopyFromArray(TAGplus, 221, 6, 0)) + "\n";
                if (TAG[125] == (byte)0x00) log += "Track : " + TAG[126].ToString() + "\n";
                log += "Genre : " + Genre[(uint)TAG[127]] + "\n\n";
            } 
            else return "this file doesn't contain either ID3v1 tag or ID3v2 tag.\n\n"+BitConverter.ToString(Util.InitialPart(TAG, 3));
        }
        else log = "this file doesn't contain either ID3v1 tag or ID3v2 tag.\n\n";

        return log;
    }

    public string LoadMPEGFrameHeader(byte[] data)
    {
        EditorUtility.DisplayProgressBar("[2/2] Loading File Content", "Wait a second. (" + byteCount + " / " + data.Length + ")", 0.5f);
        if(data[0]!=0xFF)byteCount = ((data[6] << 21) | (data[7] << 14) | (data[8] << 7) | data[9]) + 10;
        byte[] MPEGFrameHeader = new byte[4];
        Array.Copy(data, byteCount, MPEGFrameHeader, 0, 4);
        Array.Reverse(MPEGFrameHeader);
        uint mfh = BitConverter.ToUInt32(MPEGFrameHeader, 0);

        bool MP3SyncWord = ((mfh >> 21) & 0x000007FF) == 0x7FF ? true : false;
        uint tmp = ((mfh >> 19) & 0x00000003);
        string VersionID = tmp == 0x00 ? "MPEG Version 2.5" :
                           tmp == 0x01 ? "reserved" :
                           tmp == 0x10 ? "MPEG Version 2" : "MPEG Version 1";
        tmp = ((mfh >> 17) & 0x00000003);
        string Layer = tmp == 0x00 ? "reserved" :
                         tmp == 0x01 ? "Layer III" :
                         tmp == 0x10 ? "Layer II" : "Layer I";
        string ErrorProtection = ((mfh >> 16) & 0x00000001) == 0x01 ? "Non Protected" : "Protected by 16bit CRC";
        int index1 = (VersionID.Contains("1") & Layer == "Layer I") ? 0 :
                     (VersionID.Contains("1") & Layer == "Layer II") ? 1 :
                     (VersionID.Contains("1") & Layer == "Layer III") ? 2 :
                     (VersionID.Contains("2") & Layer == "Layer I") ? 3 :
                     (VersionID.Contains("2") & Layer.Contains("Layer I")) ? 4 : 5;
        uint index2 = ((mfh >> 12) & 0x0000000F);
        //Debug.Log(index1 + ":" + index2);
        int Bitrate = BitrateTable[index1, index2];
        index1 = VersionID == "MPEG Version 1" ? 0 :
                 VersionID == "MPEG Version 2" ? 1 :
                 VersionID == "MPEG Version 2.5" ? 2 : 3;
        index2 = ((mfh >> 9) & 0x00000003);
        int SamplingRate = SamplingRateTable[index1, index2];
        bool Padding = ((mfh >> 8) & 0x00000001) == 0x01 ? true : false;
        tmp = ((mfh >> 6) & 0x00000003);
        string ChannelMode = tmp == 0x00 ? "Stereo" :
                             tmp == 0x01 ? "Joint Stereo(Stereo)" :
                             tmp == 0x10 ? "Dual Chennel(2 Mono Chnnels)" :
                             tmp == 0x11 ? "Single Channel(Mono)" : "unknown";
        bool IntensityStereo = false, MSStereo = false;
        if (ChannelMode.Contains("Joint"))
        {
            IntensityStereo = ((mfh >> 5) & 0x00000001) == 1 ? true : false;
            MSStereo = ((mfh >> 4) & 0x00000001) == 1 ? true : false;
        }
        bool Copyrighted = ((mfh >> 3) & 0x00000001) == 1 ? true : false;
        bool Original = ((mfh >> 2) & 0x00000001) == 1 ? true : false;
        tmp = (mfh & 0x00000003);
        string Emphasis = tmp == 0x00 ? "none" :
                          tmp == 0x01 ? "50/15ms" :
                          tmp == 0x10 ? "reserved" :
                          tmp == 0x11 ? "CCIT J.17" : "unknown";
        tmpLog = "";
        tmpLog += "[MPEG FRAME HEADER]\n"
            + " - Frame sync".PadRight(30) + ":" + (MP3SyncWord ? "OK" : "XXX") + "\n"
            + " - MPEG Audio version ID".PadRight(30) + ":" + VersionID + "\n"
            + " - Layer description".PadRight(30) + ":" + Layer + "\n"
            + " - Protection".PadRight(30) + ":" + ErrorProtection + "\n"
            + " - Bitrate".PadRight(30) + ":" + (Bitrate == 0 ? "FREE" :
                                                                 Bitrate == 1111 ? "BAD" : (Bitrate + " [kbps]")) + "\n"
            + " - Sampling Rate Frequency".PadRight(30) + ":" + (SamplingRate == 0 ? "reserved" : (SamplingRate + " [Hz])")) + "\n"
            + " - Padding".PadRight(30) + ":" + (Padding ? "Padded woth one extra slot" : "Not padded") + "\n"
            + " - Channel Mode".PadRight(30) + ":" + ChannelMode + "\n";
        if (ChannelMode.Contains("Joint"))
        {
            tmpLog += "\t - Intensity stereo".PadRight(20) + ":" + (IntensityStereo ? "ON" : "OFF") + "\n"
                 + "\t - MS stereo".PadRight(20) + ":" + (MSStereo ? "ON" : "OFF") + "\n";
        }
        tmpLog += " - Copyright".PadRight(30) + ":" + (Copyrighted ? "Copyrighted" : "Not copyrighted") + "\n"
            + " - Original".PadRight(30) + ":" + (Original ? "Original media" : "Copy of original media") + "\n"
            + " - Emphasis".PadRight(30) + ":" + Emphasis + "\n";
        byteCount += 4;
        return tmpLog;
        //side info:デコード用の情報
    }
}
