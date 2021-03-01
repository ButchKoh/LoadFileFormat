using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

/// 参考
/// http://www.awm.jp/~yoya/cache/www.onicos.com/staff/iz/formats/gif.html.orig
/// https://www.setsuki.com/hsp/ext/gif.htm
/// http://www.tohoho-web.com/wwwgif.htm#ApplicationExtension
/// https://docstore.mik.ua/orelly/web2/wdesign/ch23_05.htm

public class LoadGif
{
    public string tmpLog = "";
    public static int byteCount;

    public IEnumerator LoadGifDetailInfo(byte[] data)
    {
        string log = "";
        System.Text.ASCIIEncoding ascii     = new System.Text.ASCIIEncoding();
        System.Text.UnicodeEncoding unicode = new System.Text.UnicodeEncoding();
        System.Text.UTF8Encoding utf8       = new System.Text.UTF8Encoding();

        #region File Header:固定
        byte[] commonInfo = new byte[13];
        Array.Copy(data, 0, commonInfo, 0, 13);
        string tmp = string.Format("{0,8}", Convert.ToString(commonInfo[10], 2)).Replace(" ", "0");

        GIFHeader.signature                 = ascii.GetString(Util.CopyFromArray(commonInfo, 0, 3, 0));
        GIFHeader.version                   = ascii.GetString(Util.CopyFromArray(commonInfo, 3, 3, 0));
        GIFHeader.logicalScreenWidth        = (commonInfo[6] + commonInfo[7] * 256);
        GIFHeader.logicalScreenHeight       = (commonInfo[8] + commonInfo[9] * 256);
        GIFHeader.globalColorTableFlag      = tmp.Substring(0, 1) == "1" ? true : false;
        GIFHeader.colorResolution           = Convert.ToInt32(tmp.Substring(1, 3), 2) + 1;
        GIFHeader.sortFlag                  = tmp.Substring(0, 1) == "1" ? true : false;
        GIFHeader.sizeOfGlobalColorTable    = (int)Mathf.Pow(2, (Convert.ToInt32(tmp.Substring(1, 3), 2) + 1)) * 3;
        GIFHeader.backgroundColorIndex      = (int)commonInfo[11];
        GIFHeader.pixelAspectRatio          = ((int)commonInfo[12] + 15) / 64;

        log += "[GIF Header]";
        log += "\n" + " - GIF signature".PadRight(25)           + ":" + GIFHeader.signature;
        log += "\n" + " - GIF version".PadRight(25)             + ":" + GIFHeader.version;
        log += "\n" + " - Logical Screen Width".PadRight(25)    + ":" + GIFHeader.logicalScreenWidth;
        log += "\n" + " - Logical Screen Height".PadRight(25)   + ":" + GIFHeader.logicalScreenHeight;
        log += "\n" + " - Global Clor Table".PadRight(25)       + ":" + GIFHeader.globalColorTableFlag;
        log += "\n" + " - Color Resolution".PadRight(25)        + ":" + GIFHeader.colorResolution;
        log += "\n" + " - Sort Flag".PadRight(25)               + ":" + GIFHeader.sortFlag;
        log += "\n" + " - Size of GCT".PadRight(25)             + ":" + GIFHeader.sizeOfGlobalColorTable;
        log += "\n" + " - BG Color Index".PadRight(25)          + ":" + GIFHeader.backgroundColorIndex;
        log += "\n" + " - Pixel Aspect Ratio".PadRight(25)      + ":" + (GIFHeader.pixelAspectRatio == 0 ? "not given" : GIFHeader.pixelAspectRatio.ToString());
        byteCount = 13;
        #endregion

        #region Global Color Table：任意
        byte[] GlobalColorTable;
        if (GIFHeader.globalColorTableFlag)
        {
            GlobalColorTable = new byte[GIFHeader.sizeOfGlobalColorTable];
            Array.Copy(data, 14, GlobalColorTable, 0, GlobalColorTable.Length);
            log += "\n\n[Global Color Table 1-" + (GlobalColorTable.Length / 3) + "]";
            string[] GCT = new string[64];
            int GCTlength = 64;

            EditorUtility.DisplayProgressBar("[" + (byteCount * 100) / data.Length + "%] Loading File Content", "Wait a second. (" + byteCount + " / " + data.Length + ")", (float)byteCount / data.Length);

            for (int i = 0; i < GCTlength; i++)
            {
                int j = i * 3;
                while (j < GlobalColorTable.Length)
                {
                    GCT[i] += string.Format("{0,3} : ", (j / 3) + 1);
                    GCT[i] += string.Format("{0,3}", ((int)GlobalColorTable[j])) + "/";
                    GCT[i] += string.Format("{0,3}", ((int)GlobalColorTable[j + 1])) + "/";
                    GCT[i] += string.Format("{0,3}", ((int)GlobalColorTable[j + 2])) + " \t";
                    j += GCTlength * 3;
                }
            }
            for (int i = 0; i < GCTlength; i++) log += (GCT[i] == null ? "" : "\n") + GCT[i];
        }
        else GlobalColorTable = new byte[0];

        #endregion

        byteCount += GlobalColorTable.Length;

        int imageBlockCounter = 1;
        //Block：不定
        while (byteCount < data.Length)
        {
            //progress bar
            EditorUtility.DisplayProgressBar("[" + (byteCount * 100) / data.Length + "%] Loading File Content", "Wait a second. (" + byteCount + " / " + data.Length + ")", (float)byteCount / data.Length);
            if (data[byteCount] == 0x21)
            {
                if (data[byteCount + 1] == 0xF9)
                {
                    log += "\n\n[Graphic Control Extension] " + byteCount + "-";
                    log += "\n" + " - Block Size".PadRight(30) + ":" + data[byteCount + 2];//0x04

                    tmp = string.Format("{0,8}", Convert.ToString(data[byteCount + 3], 2)).Replace(" ", "0");
                    int dispose = Convert.ToInt32(tmp.Substring(3, 3), 2);
                    string disposalMethod = (dispose == 0 ? "Unspecified" :
                                            (dispose == 1 ? "Do not Dispose" :
                                            (dispose == 2 ? "Restore to Background" :
                                            (dispose == 3 ? "Restore to Previous" : "unknown"))));
                    log += "\n" + " - Disposal Method".PadRight(30)         + ":" + disposalMethod;
                    log += "\n" + " - User Input".PadRight(30)              + ":" + (tmp.Substring(6, 1) == "0" ? false : true);
                    log += "\n" + " - Transparent Color".PadRight(30)       + ":" + (tmp.Substring(7, 1) == "0" ? false : true);
                    log += "\n" + " - Delay Time".PadRight(30)              + ":" + (data[byteCount + 4] + data[byteCount + 5] * 256);
                    log += "\n" + " - Transparent Color Index".PadRight(30) + ":" + data[byteCount + 6];
                    byteCount += 7;
                    log += "\nBLOCK TERMINATOR : " + (data[byteCount] == 0 ? "OK" : "XXX") + "(" + byteCount + ")";

                }
                else if (data[byteCount + 1] == 0xFE)
                {
                    log += "\n\n[Comment Extension] " + byteCount + "-";
                    log += "\n" + " - Block Size".PadRight(15) + ":" + data[byteCount + 2];
                    byte[] Comment = new byte[data[byteCount + 2]];
                    Array.Copy(data, byteCount + 3, Comment, 0, Comment.Length);
                    log += "\n" + " - Content".PadRight(15) + ":" + ascii.GetString(Comment).Replace("\n", "\n\t");
                    byteCount += data[byteCount + 2] + 2;
                }
                else if (data[byteCount + 1] == 0x01)
                {
                    log += "\n\n[Plain Text Extension] " + byteCount + "-";
                    log += "\n" + " - Text Grid Left Position".PadRight(30) + ":" + (data[byteCount + 3] + data[byteCount + 4] * 256);
                    log += "\n" + " - Text Grid Top Position".PadRight(30)  + ":" + (data[byteCount + 5] + data[byteCount + 6] * 256); 
                    log += "\n" + " - Text Grid Width".PadRight(30)         + ":" + (data[byteCount + 7] + data[byteCount + 8] * 256); 
                    log += "\n" + " - Text Grid Height".PadRight(30)        + ":" + (data[byteCount + 9] + data[byteCount + 10] * 256);
                    log += "\n" + " - Character Cell Width".PadRight(30)    + ":" + data[byteCount + 11];
                    log += "\n" + " - Character Cell Height".PadRight(30)   + ":" + data[byteCount + 12];
                    log += "\n" + " - Text Grid Width".PadRight(30)         + ":" + data[byteCount + 13];
                    log += "\n" + " - Text Grid Height".PadRight(30)        + ":" + data[byteCount + 14];
                    byte[] pt = new byte[data[byteCount + 2]];
                    Array.Copy(data, byteCount + 15, pt, 0, pt.Length);
                    log += "\nascii : " + ascii.GetString(pt);
                    log += "\nbase64 : " + Convert.ToBase64String(pt);
                    log += "\nunicode : " + unicode.GetString(pt);
                    log += "\nutf-8 : " + utf8.GetString(pt);
                    byteCount += data[byteCount + 2] + 2;
                }
                else if (data[byteCount + 1] == 0xFF)
                {
                    byte[] appid = new byte[8];
                    Array.Copy(data, byteCount + 3, appid, 0, 8);
                    byte[] aac = new byte[3];
                    Array.Copy(data, byteCount + 11, aac, 0, 3);

                    log += "\n\n[Application Extension] " + byteCount + "-";
                    log += "\n" + " - Block Size".PadRight(35) + ":" + data[byteCount + 2];
                    log += "\n" + " - Application Identifier".PadRight(35) + ":" + ascii.GetString(appid);
                    log += "\n" + " - Application Authentication Code".PadRight(35) + ":" + ascii.GetString(aac);
                    byteCount += 14;
                    while (true)
                    {
                        log += "\n" + " - Sub-block Data Size".PadRight(35) + ":" + data[byteCount] + "(" + byteCount + "-" + (data[byteCount] + byteCount - 1) + ")";
                        byteCount += data[byteCount];
                        if (data[byteCount + 1] == 0) break;
                    }
                    log += "\nBLOCK TERMINATOR : " + (data[byteCount] == 0 ? "OK" : "XXX") + "(" + byteCount + ")";
                    byteCount++;
                }
                else
                {
                    byteCount++;
                }
            }
            else if (data[byteCount] == 0x2C)
            {
                log += "\n\n[Image Block(" + imageBlockCounter + ")] " + byteCount + "-";
                imageBlockCounter++;
                tmp = string.Format("{0,8}", Convert.ToString(data[byteCount + 9], 2)).Replace(" ", "0");
                log += "\n" + " - Image Left Position".PadRight(30) + ":" + (data[byteCount + 1] + data[byteCount + 2] * 256);
                log += "\n" + " - Image Top Position".PadRight(30)  + ":" + (data[byteCount + 3] + data[byteCount + 4] * 256);
                log += "\n" + " - Image Width".PadRight(30)         + ":" + (data[byteCount + 5] + data[byteCount + 6] * 256);
                log += "\n" + " - Image Height".PadRight(30)        + ":" + (data[byteCount + 7] + data[byteCount + 8] * 256);
                log += "\n" + " - Local Color Table".PadRight(30)   + ":" + (tmp.Substring(0, 1) == "1" ? true : false).ToString();
                log += "\n" + " - Interlace".PadRight(30)           + ":" + (tmp.Substring(1, 1) == "1" ? true : false).ToString();
                log += "\n" + " - Sort".PadRight(30)                + ":" + (tmp.Substring(2, 1) == "1" ? true : false).ToString();
                log += "\n" + " - Size of LCT".PadRight(30)         + ":" + (Mathf.Pow(2, Convert.ToInt32(tmp.Substring(5, 3), 2) + 1)).ToString() + " colors";
                int LCT;
                if (tmp.Substring(0, 1) == "1") LCT = (int)Mathf.Pow(2, Convert.ToInt32(tmp.Substring(5, 3), 2) + 1) * 3;
                else LCT = 0;

                byteCount += 10 + LCT;
                log+= "\n" + " - LZW Minimum Code Size".PadRight(30) + ":" + data[byteCount];
                byteCount++;
                int count = 0, totalsize = 0;
                while (true)
                {
                    count++;
                    totalsize += data[byteCount];
                    byteCount += 1 + data[byteCount];
                    if (data[byteCount] == 0) break;
                }
                log += "\n" + " - Number of Blocks".PadRight(30) + ":" + count;
                log += "\n" + " - Total Size".PadRight(30) + ":" + totalsize;
            }
            else if (data[byteCount] == 0x3B)
            {
                log += "\n\n[Trailer(0x3B)]";
                byteCount++;
            }
            else
            {
                byteCount++;
            }
            if (LoadFileFormat.breakCoRoutine) {
                yield break; 
            }
            LoadFileFormat.guiMessage = "Please wait";
            yield return "Please wait";
        }
        tmpLog = log;
        yield return null;
    }

}

public static class GIFHeader
{
    public static string signature;
    public static string version;
    public static int logicalScreenWidth;
    public static int logicalScreenHeight;
    public static bool globalColorTableFlag;
    public static int colorResolution;
    public static bool sortFlag;
    public static int sizeOfGlobalColorTable;
    public static int backgroundColorIndex;
    public static float pixelAspectRatio;
}

