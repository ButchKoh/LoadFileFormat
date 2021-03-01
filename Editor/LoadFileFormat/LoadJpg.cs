using System;
using UnityEngine;
using System.Collections.Generic;

///参考：
///https://www.setsuki.com/hsp/ext/jpg.htm
///http://hp.vector.co.jp/authors/VA032610/
///http://vip.sugovica.hu/Sardi/kepnezo/JPEG%20File%20Layout%20and%20Format.htm
///https://qiita.com/tobira-code/items/63b9065a46208d0fd128#dqt-define-quantization-table
///

public class LoadJpg
{
    public string LoadStart(byte[] data)
    {
        return LoadJpgDetailInfo(data);
    }

    private string CheckMarker(byte data)
    {
        switch (data)
        {
            case 0x00:return "---";
            case 0x01:return "TEM";
            case 0x4F:return "SOC";
            case 0x51:return "SIZ";
            case 0x52:return "COD";
            case 0x53:return "COC";
            case 0x55:return "TLM";
            case 0x57:return "PLM";
            case 0x58:return "PLT";
            case 0x5C:return "QCD";
            case 0x5D:return "QCC";
            case 0x5E:return "RGN";
            case 0x5F:return "POC";
            case 0x60:return "PPM";
            case 0x61:return "PPT";
            case 0x63:return "CRG";
            case 0x64:return "COM";
            case 0x90:return "SOT";
            case 0x91:return "SOP";
            case 0x92:return "EPH";
            case 0x93:return "SOD";
            case 0xC0:return "SOF0 : (Start of frame type 0 segment, SOF0)";
            case 0xC1:return "SOF1";
            case 0xC2:return "SOF2 : (Start of frame type 2 segment, SOF2)";
            case 0xC3:return "SOF3";
            case 0xC4:return "DHT : (Define Huffman table segmet, DHT)";
            case 0xC5:return "SOF5";
            case 0xC6:return "SOF6";
            case 0xC7:return "SOF7";
            case 0xC8:return "JPG";
            case 0xC9:return "SOF9";
            case 0xCA:return "SOF10";
            case 0xCB:return "SOF11";
            case 0xCC:return "DAC";
            case 0xCD:return "SOF13";
            case 0xCE:return "SOF14";
            case 0xCF:return "SOF15";
            case 0xD0:return "RST0";
            case 0xD1:return "RST1";
            case 0xD2:return "RST2";
            case 0xD3:return "RST3";
            case 0xD4:return "RST4";
            case 0xD5:return "RST5";
            case 0xD6:return "RST6";
            case 0xD7:return "RST7";
            case 0xD8:return "SOI : (Start of image segment, SOI)";
            case 0xD9:return "EOI : (End of image segment, EOI)";
            case 0xDA:return "SOS : (Start of scan segment, SOS)";
            case 0xDB:return "DQT : (Define quantization table segment, DQT)";
            case 0xDC:return "DNL";
            case 0xDD:return "DRI";
            case 0xDE:return "DHP";
            case 0xDF:return "EXP";
            case 0xE0:return "APP0 : (Application type 0 segment, APP0)";
            case 0xE1:return "APP1";
            case 0xE2:return "APP2";
            case 0xE3:return "APP3";
            case 0xE4:return "APP4";
            case 0xE5:return "APP5";
            case 0xE6:return "APP6";
            case 0xE7:return "APP7";
            case 0xE8:return "APP8";
            case 0xE9:return "APP9";
            case 0xEA:return "APP10";
            case 0xEB:return "APP11";
            case 0xEC:return "APP12";
            case 0xED:return "APP13";
            case 0xEE:return "APP14";
            case 0xEF:return "APP15";
            case 0xF0:return "JPG0";
            case 0xF1:return "JPG1";
            case 0xF2:return "JPG2";
            case 0xF3:return "JPG3";
            case 0xF4:return "JPG4";
            case 0xF5:return "JPG5";
            case 0xF6:return "JPG6";
            case 0xF7:return "JPG7";
            case 0xF8:return "JPG8";
            case 0xF9:return "JPG9";
            case 0xFA:return "JPG10";
            case 0xFB:return "JPG11";
            case 0xFC:return "JPG12";
            case 0xFD:return "JPG13";
            case 0xFE:return "COM";
            default:
                if (data <= 0x4E)return "RES";
                else return "---";
        }
    }
    private string CreateLog(byte[] data,string type)
    {
        System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
        string subLog = "";
        if (type.Contains("APP0"))
        {
            subLog += " - ID".PadRight(20)            + " : " + (ascii.GetString(Util.CopyFromArray(data, 2, 5, 0)) == "JFIF\0" ? "JFIF\\0" : "XXX") + "\n";
            subLog += " - Version".PadRight(20)       + " : " + Convert.ToSingle(BitConverter.ToString(Util.CopyFromArray(data, 7, 2, 0)).Replace("-", ".")) + "\n";
            subLog += " - Unit".PadRight(20)          + " : " + (data[9] == 0 ? "None" : (data[9] == 1 ? "dpi" : "dpcm")) + "\n";
            subLog += " - X:Y density".PadRight(20)   + " : " + BitConverter.ToInt16(Util.CopyFromArray(data, 10, 2, 0), 0) + " : " + BitConverter.ToInt16(Util.CopyFromArray(data, 12, 2, 0), 0) + "\n";
            subLog += " - X:Y thumbnail".PadRight(20) + " : " + (int)data[14] + " : " + (int)data[15] + "\n";
            return subLog;
        }
        else if (type.Contains("APP"))
        {
            subLog += type;
            return subLog;
        }
        else if (type.Contains("DQT"))
        {
            subLog += " - Quantization Table Precision".PadRight(35) + " : " + (data[2].ToString().Substring(0, 1) == "0" ? "8" : "16") + "bits\n";
            subLog += " - Quantization Table Number".PadRight(35)    + " : " + data[2].ToString().PadLeft(2,'0').Substring(1, 1) + "\n";
            byte[] qts = Util.CopyFromArray(data, 3, 64, 0);
            byte[,] qt = new byte[8, 8];
            int byteCount = 0;
            subLog += " - Quantization Table:\n";
            for (int i = 1; i < 16; i++){
                int len=i, count = 0;
                if (i >= 8){
                    count++;
                    len = 16 - i;
                }
                //1DArray->zig-zag順に読み込んで2DArrayに
                for (int j = 0; j < len; j++){
                    int loop = (i % 2 == 1 ? 1 : (-1));                    
                    int index1 = (i <= 8 ? (loop == 1 ? j : (len - j - 1)) : (loop == 1 ? 7 - (len - j - 1) : 7 - j));
                    int index2 = (i <= 8 ? (loop == 1 ? (len - j - 1) : j) : (loop == 1 ? 7 - j : 7 - (len - j - 1)));
                    qt[index2, index1] = qts[byteCount];
                    byteCount++;
                }
            }
            for(int i = 0; i < 8; i++){
                subLog += "\t";
                for (int j = 0; j < 8; j++) subLog += qt[i, j].ToString().PadLeft(4);
                subLog += "\n";
            }
            return subLog;
        }
        else if (type.Contains("SOF"))
        {
            string tmpstr = type.Substring(3, 1);
            subLog += " - Frame Type".PadRight(30) + " : " + (tmpstr == "0" ? "baseline" : (tmpstr == "1" ? "expanded sequential" : (tmpstr == "2" ? "progressive" : (tmpstr == "3" ? "lossless" : "")))) + "\n";
            subLog += " - Precision".PadRight(30)  + " : " + data[2] + "\n";
            subLog += " - Image Size".PadRight(30) + " : " + (data[6] + data[5] * 256)
                                                   + " x " + (data[4] + data[3] * 256) + "\n";
            subLog += " - Number of Color Components".PadRight(30) + " : " + data[7] + (data[7] == 1 ? "(Gray Scale)" : (data[7] == 3 ? "(YCbCr or YIQ)" : (data[7] == 4 ? "(CMYK)" : "XXX"))) + "\n";
            for(int i = 0; i < data[7]; i++){
                int tmpint = data[8 + i * 3];
                subLog += "\t - ID".PadRight(10) + " : " + (tmpint == 1 ? "Y " : (tmpint == 2 ? "Cb" : (tmpint == 3 ? "Cr" : (tmpint == 4 ? "I " : (tmpint == 5 ? "Q " : "XXX"))))) + "\n\t\t";
                subLog += " - Vertical Sample Factor".PadRight(35)        + " : " + data[8 + i * 3 + 1].ToString().PadLeft(2,'0').Substring(0, 1) + "\n\t\t";
                subLog += " - Horizontal Sample Factor".PadRight(35)      + " : " + data[8 + i * 3 + 1].ToString().PadLeft(2,'0').Substring(1, 1) + "\n\t\t";
                subLog += " - Quantization Table Identifier".PadRight(35) + " : " + ((int)data[8 + i * 3 + 2]).ToString() + "\n";
            }
            return subLog;
        }
        else if (type.Contains("DHT")){
            byte[] bitsTable = new byte[16];
            int counter = 1;
            subLog += " - Huffman Table Class".PadRight(25) + " : " + (data[2].ToString().Substring(0, 1) == "0" ? "DC" : "AC") + "\n";
            subLog += " - Huffman Table Index".PadRight(25) + " : " + data[2].ToString().PadLeft(2, '0').Substring(1, 1) + "\n";
            subLog += " - Bits Table".PadRight(25) + " : ";
            byte[] ht = Util.CopyFromArray(data, 2 + 1 + 16, data.Length - 2 - 1 - 16, 0);
            
            for (int i = 0; i < 16; i++){
                subLog += ((int)data[i + 3]).ToString() + " ";
                bitsTable[i] = data[i + 3];
            }
            subLog += "\n - Huffman Code:\n";
            int code = 0;
            int i_last = 0;
            for (int i = 0; i < 16; i++)//i:長さ
            {
                if (bitsTable[i] != 0){//bitsTable[i]:ibitsのhuffmanコードがいくつあるか
                    for (int j = 0; j < bitsTable[i]; j++){
                        int size_changed = (i == i_last ? 0 : 1);
                        subLog += counter.ToString() + "\t"; 
                        if (size_changed == 1) code = code << 1;
                        subLog += (Convert.ToString(code, 2).Length == 1 ? "0" : "")  + Convert.ToString(code, 2) + "\n";
                        code++;
                        counter++;
                        i_last = i;
                    }
                }
            }
            return subLog;
        }
        else if (type.Contains("SOS"))
        {
            subLog += " - Number of color components" + " : " + data[2] + "\n";
            int i;
            for (i = 0; i < data[2]; i++)
            {
                subLog += "\t - ID".PadRight(15) + " : " + data[2 + i * 2 + 1] + "\n";
                subLog += "\t - AC table".PadRight(15) + " : " + data[2 + i * 2 + 2].ToString().PadLeft(2,'0').Substring(0, 1) + "\n";
                subLog += "\t - DC table".PadRight(15) + " : " + data[2 + i * 2 + 2].ToString().PadLeft(2,'0').Substring(1, 1);
                subLog += "\n";
            }
            subLog += " - Spectrum Selection".PadRight(25) + " : " + data[data.Length - 3] + " - " + data[data.Length - 2] + "\n";
            subLog += " - Successive approximation bit position(hight - low)" + " : " + (data[data.Length - 2] & 0b11110000) + " - " + (data[data.Length - 2] & 0b00001111) + "\n";
            return subLog;
        }
        else return "type not found";
    }
    string LoadJpgDetailInfo(byte[] data)
    {
        string log = "";
        long byteCount = 0;
        try
        {
            log += "JPG SOI: OK\n\n";

            //SOI,EOI以外の部分
            byte[] segments = Util.CopyFromArray(data, 2, data.Length - 4, 0);
            byteCount = 0;

            while (byteCount < (segments.Length))
            {
                int tmpLength = 1;
                if (segments[byteCount] == 0xFF
                    && CheckMarker(segments[byteCount + 1]) != "---")
                {
                    tmpLength = (segments[byteCount + 3] + segments[byteCount + 2] * 256);
                    byte[] thisSegment = Util.CopyFromArray(segments, byteCount + 2, tmpLength, 0);

                    string tmpLog = CheckMarker(segments[byteCount + 1]) + "\n - length" + " = " + tmpLength;
                    log += tmpLog + "\n" + CreateLog(thisSegment, CheckMarker(segments[byteCount + 1])) + "\n";
                }
                byteCount += tmpLength + 2;
            }
            if (data[data.Length - 2] == 0xFF &&CheckMarker( data[data.Length - 1]).Contains("EOI")) log += "JPG EOI: OK\n\n";
            else log += BitConverter.ToString(new byte[] { data[data.Length - 2], data[data.Length - 1] });
        }
        catch (Exception e)
        {
            log = e.Message;
        }
        return log;
    }
}
