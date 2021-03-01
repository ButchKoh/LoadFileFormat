using System;
using UnityEngine;

/// 参考
/// http://sunpillar.jf.land.to/bekkan/data/files/bmp.html#BITMAPFILEHEADER2
/// http://www5d.biglobe.ne.jp/noocyte/Programming/Windows/BmpFileFormat.html
/// http://www.umekkii.jp/data/computer/file_format/bitmap.cgi

public class LoadBmp 
{
    public string LoadStart(byte[] data)
    {
        return LoadGifDetailInfo(data);
    }
   
    string LoadGifDetailInfo(byte[] data)
    {
        string log = "";
        System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
        if (ascii.GetString(new byte[2] { data[0], data[1] }) == "BM")
        {
            #region file header
            byte[] bfSize       = Util.CopyFromArray(data, 2, 4, 0);
            byte[] bfOffBits    = Util.CopyFromArray(data, 10, 4, 0);
            byte[] biSize       = Util.CopyFromArray(data, 14, 4, 0);
            int offset          = (int)BitConverter.ToUInt32(bfOffBits, 0);

            log += "[BITMAPFILEHEADER]"
                + "\n - bfType".PadRight(15)    + ":OK"
                + "\n - bfSIze".PadRight(15)    + ":" + BitConverter.ToUInt32(bfSize, 0) + "[B]"
                + "\n - bfOffBits".PadRight(15) + ":" + offset + "[B]";
            #endregion
            #region Header Info Depends on Version
            int byteCount   = 13;
            bool TriOrQuad  = false;
            uint bihSize    = BitConverter.ToUInt32(biSize, 0);
            string sym      = (bihSize == 40 ? "bi" : (bihSize == 108 ? "bV4" : (bihSize == 124 ? "bV5" : "")));
            int tmp = 0, compression = 0, csType = 0, bitCount = 0, colors = 0, width = 0, height = 0;
            if (bihSize == 12)
            {
                width = (data[18] + data[19] * 256);
                height = (data[20] + data[21] * 256);
                bitCount = (data[24] + data[25] * 256);
                colors = (int)Mathf.Pow(2, (bitCount == 32 ? 24 : bitCount));

                log += "\n\n[BITMAPCOREHEADER (" + byteCount + "-)]"
                    + "\n - bcWidth".PadRight(15)    + ":" + width
                    + "\n - bcHeight".PadRight(15)   + ":" + height
                    + "\n - bcPlanes".PadRight(15)   + ":" + (data[22] + data[23] * 256)
                    + "\n - bcBitCount".PadRight(15) + ":" + bitCount;
                TriOrQuad = true;
            }
            else log += "\n\n[BITMAP"
                     + (bihSize == 40 ? "INFO" : (bihSize == 108 ? "V4" : (bihSize == 124 ? "V5" : "???")))
                     + "HEADER (" + byteCount + "-)]";
            if (bihSize >= 40)
            {
                byte[] Width         = Util.CopyFromArray(data, 18, 4, 0);
                byte[] Height        = Util.CopyFromArray(data, 22, 4, 0);
                byte[] Compression   = Util.CopyFromArray(data, 30, 4, 0);
                byte[] SizeImage     = Util.CopyFromArray(data, 34, 4, 0);
                byte[] XPelsPerMeter = Util.CopyFromArray(data, 38, 4, 0);
                byte[] YPelsPerMeter = Util.CopyFromArray(data, 42, 4, 0);
                byte[] ClrUsed       = Util.CopyFromArray(data, 46, 4, 0);
                byte[] ClrImportant  = Util.CopyFromArray(data, 50, 4, 0);

                width       = BitConverter.ToInt32(Width, 0);
                height      = BitConverter.ToInt32(Height, 0);
                bitCount    = (data[28] + data[29] * 256);
                colors      = (int)Mathf.Pow(2, (bitCount == 32 ? 24 : bitCount));
                compression = (int)BitConverter.ToUInt32(Compression, 0);

                log += "\n - " + sym + "Width".PadRight(15)       + ":" + width + "[pixels]";
                log += "\n - " + sym + "Height".PadRight(15)      + ":" + height + "[pixels]";
                log += "\n - " + sym + "Planes".PadRight(15)      + ":" + (data[26] + data[27] * 256);
                log += "\n - " + sym + "BitCount".PadRight(15)    + ":" + bitCount + "[bits] = " + colors + " colors";//1,4,8,(16),24,32
                log += "\n - " + sym + "Compression".PadRight(15) + ":" + (compression == 0 ? "BI_RGB(Not Compressed)" :
                                                          (compression == 1 ? "BI_RLE8(Run-Length-Encoded 8bits/pixel)" :
                                                          (compression == 2 ? "BI_RLE4(Run-Length-Encoded 4bits/pixel)" :
                                                          (compression == 3 ? "BI_BITFIELDS" : "unknown"))));
                tmp = (int)BitConverter.ToUInt32(SizeImage, 0);
                log += "\n - " + sym + "SizeImage".PadRight(15) + ":" + tmp + "[B] ≒ " 
                     + Mathf.Floor(tmp / 1024) + "[kB]";
                log += "\n - " + sym + "XPelsPerMeter".PadRight(15) + ":" + BitConverter.ToInt32(XPelsPerMeter, 0);
                log += "\n - " + sym + "YPelsPerMeter".PadRight(15) + ":" + BitConverter.ToInt32(YPelsPerMeter, 0);
                tmp = (int)BitConverter.ToUInt32(ClrUsed, 0);
                log += "\n - " + sym + "ClrUsed".PadRight(15) + ":" + 
                                              (tmp == 0           ? colors + " colors" :
                                              (tmp > 0 & tmp < 16 ? tmp + " colors" :
                                              (tmp >= 16          ? "color table size = " + tmp : "unknown:" + tmp)));
                tmp = (int)BitConverter.ToUInt32(ClrImportant, 0);
                log += "\n - " + sym + "ClrImportant".PadRight(15) + ":" + (tmp == 0 ? "All colors" : tmp.ToString());
            }
            if (bihSize >= 108)
            {
                if (compression == 3)
                {
                    byte[] RedMask   = Util.CopyFromArray(data, 54, 4, 0);
                    byte[] GreenMask = Util.CopyFromArray(data, 58, 4, 0);
                    byte[] BlueMask  = Util.CopyFromArray(data, 62, 4, 0);
                    byte[] AlphaMask = Util.CopyFromArray(data, 66, 4, 0);

                    log += "\n - " + sym + "RedMask".PadRight(15)   + ":0x" + BitConverter.ToString(RedMask, 0).Replace("-", "");
                    log += "\n - " + sym + "GreenMask".PadRight(15) + ":0x" + BitConverter.ToString(GreenMask, 0).Replace("-", "");
                    log += "\n - " + sym + "BlueMask".PadRight(15)  + ":0x" + BitConverter.ToString(BlueMask, 0).Replace("-", "");
                    log += "\n - " + sym + "AlphaMask".PadRight(15) + ":0x" + BitConverter.ToString(AlphaMask, 0).Replace("-", "");
                }
                byte[] CSType = Util.CopyFromArray(data, 70, 4, 0);
                csType = (int)BitConverter.ToUInt32(CSType, 0);
                log += "\n - " + sym + "CSType".PadRight(15) + ":" + 
                                             (csType == 0x00000000 ? "LCS_CALIBRATED_RGB" :
                                             (csType == 0x73524742 ? "LCS_sRGB" ://"sRGB"
                                             (csType == 0x57696E20 ? "LCS_WINDOWS_COLOR_SPACE" ://"Win "
                                             (csType == 0x4C494E4B ? "PROFILE_LINKED" ://"LINK"
                                             (csType == 0x4D424544 ? "PROFILE_EMBEDDED" : "unknown")))));////"MBED"

                if (csType == 0x000000)
                {
                    byte[] Endpoints = Util.CopyFromArray(data, 74, 36, 0);
                    int rx = BitConverter.ToInt32(Util.CopyFromArray(Endpoints, 0, 4, 0), 0), 
                        ry = BitConverter.ToInt32(Util.CopyFromArray(Endpoints, 4, 4, 0), 0), 
                        rz = BitConverter.ToInt32(Util.CopyFromArray(Endpoints, 8, 4, 0), 0),
                        gx = BitConverter.ToInt32(Util.CopyFromArray(Endpoints, 12, 4, 0), 0), 
                        gy = BitConverter.ToInt32(Util.CopyFromArray(Endpoints, 16, 4, 0), 0), 
                        gz = BitConverter.ToInt32(Util.CopyFromArray(Endpoints, 20, 4, 0), 0),
                        bx = BitConverter.ToInt32(Util.CopyFromArray(Endpoints, 24, 4, 0), 0), 
                        by = BitConverter.ToInt32(Util.CopyFromArray(Endpoints, 28, 4, 0), 0), 
                        bz = BitConverter.ToInt32(Util.CopyFromArray(Endpoints, 32, 4, 0), 0);
                    log += "\n - " + sym + "Endpoints\t";// + BitConverter.ToString(bV5Endpoints);//4byte(32bit整数値)*3(x,y,z)*3(r,g,b)(?)
                    log += ":R " + ((float)rx / (rx + ry + rz)) + " / " + ((float)ry / (rx + ry + rz)) + " / " + ((float)rz / (rx + ry + rz))+ "\n\t".PadRight(15) 
                         + ":G " + ((float)gx / (gx + gy + gz)) + " / " + ((float)gy / (gx + gy + gz)) + " / " + ((float)gz / (gx + gy + gz))+ "\n\t".PadRight(15) 
                         + ":B " + ((float)bx / (bx + by + bz)) + " / " + ((float)by / (bx + by + bz)) + " / " + ((float)bz / (bx + by + bz));

                    byte[] GammaRed   = Util.CopyFromArray(data, 110, 4, 0);
                    byte[] GammaGreen = Util.CopyFromArray(data, 114, 4, 0);
                    byte[] GammaBlue  = Util.CopyFromArray(data, 118, 4, 0);
                    log += "\n - " + sym + "GammaRed".PadRight(15)   + ":" + BitConverter.ToString(GammaRed);
                    log += "\n - " + sym + "GammaGreen".PadRight(15) + ":" + BitConverter.ToString(GammaGreen);
                    log += "\n - " + sym + "GammaBlue".PadRight(15)  + ":" + BitConverter.ToString(GammaBlue);
                }
            }
            if (bihSize >= 124)
            {
                byte[] Intent = Util.CopyFromArray(data, 122, 4, 0);
                int intent = BitConverter.ToInt32(Intent, 0);
                log += "\n - " + sym + "Intent" + "\n\t" + " - Value".PadRight(15) + ":" + 
                    (intent == 1 ? "LCS_GM_BUSINESS\n\t"         
                        + " - Intent".PadRight(15)  + ":Graphic\n\t" 
                        + " - ICC".PadRight(15)     + ":Saturation" :
                    (intent == 2 ? "LCS_GM_GRAPHICS\n\t"         
                        + " - Intent".PadRight(15)  + ":Proof\n\t"   
                        + " - ICC".PadRight(15)     + ":Relative Colorimetric" :
                    (intent == 4 ? "LCS_GM_IMAGES\n\t"           
                        + " - Intent".PadRight(15)  + ":Picture\n\t" 
                        + " - ICC".PadRight(15)     + ":Perceptual" :
                    (intent == 8 ? "LCS_GM_ABS_COLORIMETRIC\n\t" 
                        + " - Intent".PadRight(15)  + ":Match\n\t"   
                        + " - ICC".PadRight(15)     + ":Absolute Colorimetric" : "unknown"))));
                if (csType == 0x4C494E4B | csType == 0x4D424544)
                {
                    byte[] ProfileData = Util.CopyFromArray(data, 126, 4, 0);
                    byte[] ProfileSize = Util.CopyFromArray(data, 130, 4, 0);
                    log += "\n - " + sym + "ProfileData".PadRight(15) + ":" + BitConverter.ToString(ProfileData);
                    log += "\n - " + sym + "ProfileSize".PadRight(15) + ":" + BitConverter.ToString(ProfileSize);
                }
            }

            if (bihSize != 12 & bihSize != 40 & bihSize != 108 & bihSize != 124) log += "\n\n[UNKNOWN]" + byteCount + "/bihSize = " + bihSize;
            
            byteCount += (int)bihSize;
            string[] rgbQuad = new string[colors];
            #endregion
            #region Color Palette

            int tablelength = 64;
            int tablewidth = (colors / tablelength) - 1;
            if (bitCount <= 8)
            {
                log += "\n\n[RGB" + (TriOrQuad ? "TRIPLE" : "QUAD") + " (" + byteCount + "-)]\n";
                for(int i = 0; i < colors; i++)
                {
                    rgbQuad[i] = i.ToString().PadLeft(3) + " : " + (data[byteCount + 1] + "/"
                                                                  + data[byteCount + 2] + "/"
                                                                  + data[byteCount + 3]).PadRight(13);
                    if (i >= tablewidth * tablelength) rgbQuad[i] += "\n";
                    byteCount += TriOrQuad ? 3 : 4;
                }
            }
            for(int i = 0; i < tablelength; i++)
            {
                int j = i;
                while (j < colors)
                {
                    log += rgbQuad[j];
                    j += tablelength;
                }
                if (i > colors) break;
            }
            #endregion
            #region Image Data
            log += "\n\n[Image Data (" + byteCount + "-)]";
            log += "\n - length : " + (data.Length - offset) + " [B]\n";
            if (bitCount != 0)
            {
                log += (width * height) * bitCount / 8 + " [B] "
                    + "\n\t = " + (width * height) + " [pixels] x " + bitCount + " / " + "8" + " [B]"
                    + "\n\t = " + width + " x " + height + " [pixels] x " + (float)bitCount / 8 + " [B]( + padding)\n";
            }
            else log += "JPEG or PNG format.\n";
            #endregion
        }
        else
        {
            log = "this file is not bmp image";
        }

        return log;
    }

     
}
