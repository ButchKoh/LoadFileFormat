using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Ionic.Zlib;　
///参考:
///https://www.setsuki.com/hsp/ext/png.htm
///https://pmt.sourceforge.io/specs/png-1.2-pdg-h20.html

public class LoadPng 
{
    public string LoadStart(byte[] data)
    {        
        return LoadPngDetailInfo(data);
    }
    
    string LoadPngDetailInfo(byte[] data)
    {
        PNG.IHDR ihdr = new PNG.IHDR();
        string log = "";
        try
        {
            System.Text.ASCIIEncoding ascii = new System.Text.ASCIIEncoding();
            System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding();

            byte[] pngHeader  = PNG.GetPngHeaderFromWholeData(data);
            if (!Util.Compare(pngHeader,Signatures.PNG)) return "this file is not png image";

            #region チャンク読み込み
            int byteCount = 8;//png headerの長さ分だけオフセット
            while (byteCount < data.Length)
            {
                int tmpLength       = Util.ByteArray2Int32(Util.CopyFromArray(data, byteCount, 4, 0));
                byte[] tmpChunkType = Util.CopyFromArray(data, byteCount + 4, 4, 0);
                byte[] chunk        = Util.CopyFromArray(data, byteCount, tmpLength + 12, 0);

                if (Util.Compare(tmpChunkType, PNG.Chunk.IHDR))
                {
                    byte[] IHDRChunk = PNG.GetIHDRFromWholeData(data);
                    byte[] width     = PNG.IHDR.GetWidthFromIHDR(IHDRChunk);
                    byte[] height    = PNG.IHDR.GetHeightFromIHDR(IHDRChunk);
                    ihdr.width = Util.ByteArray2Int32(width);
                    ihdr.height = Util.ByteArray2Int32(height);
                    ihdr.bitDepth = IHDRChunk[16];
                    ihdr.colorTypeInt = IHDRChunk[17];
                    ihdr.compMethodInt = IHDRChunk[18];
                    ihdr.filterMethodInt = IHDRChunk[19];
                    ihdr.interlaceMethodInt = IHDRChunk[20];
                    log += "IHDR Chunk   : イメージヘッダ\n"
                         + " - width".PadRight(15) + ":"      + ihdr.width + " pixels"  + "\n"
                         + " - height".PadRight(15) + ":"     + ihdr.height + " pixels" + "\n"
                         + " - bit depth".PadRight(15) + ":"  + ihdr.bitDepth + " bits" + "\n"
                         + " - color type".PadRight(15) + ":" + ihdr.colorTypeStr       + "\n"
                         + " - compression".PadRight(15) + ":"+ ihdr.compMethodStr      + "\n"
                         + " - filter".PadRight(15) + ":"     + ihdr.filterMethodStr    + "\n"
                         + " - interlace".PadRight(15) + ":"  + ihdr.interlaceMethodStr + "\n";
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.PLTE)){
                    string tmpLog = "";
                    for (int i = 0; i < (chunk.Length - 12) / 3; i++){
                        tmpLog += " - palette(No." + (i + 1) + ")" + ":" + chunk[8 + i * 3] + ", " + chunk[8 + i * 3 + 1] + ", " + chunk[8 + i * 3 + 2] + "\n";
                    }
                    log += "PLTE Chunk   :パレット\n" + " - length".PadRight(15) + ":" + tmpLength + "\n" + tmpLog;
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.IDAT)){
                    log += "IDAT Chunk   :イメージデータ\n"
                        + " - length".PadRight(15).PadRight(15) + ":" + tmpLength + "(" + (byteCount + 8).ToString() + "-" + (byteCount + 8 + tmpLength).ToString() + ")\n";
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.IEND)){
                    log += "IEND Chunk   :終端" + "\n" + " - length".PadRight(15) + ":" + tmpLength + "\n";
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.tRNS)){
                    string tmpLog = "";
                    if (ihdr.colorTypeInt == 0){
                        for (int i = 0; i < chunk.Length - 12; i++){
                            tmpLog += " - Alpha(gray level = " + i + ")" + chunk[8 + i] + "\n";
                        }
                    }
                    else if (ihdr.colorTypeInt == 2){
                        for (int i = 0; i < chunk.Length - 12; i++){
                            tmpLog += " - Alpha(palette No. = " + i + ")" + chunk[8 + i] + "\n";
                        }
                    }
                    else if (ihdr.colorTypeInt == 3){
                        for (int i = 0; i < (chunk.Length - 12) / 3; i++){
                            tmpLog += " - Alpha(RGB No. = " + i + ")" + chunk[8 + i * 3] + ", " + chunk[8 + i * 3 + 1] + ", " + chunk[8 + i * 3 + 2] + "\n";
                        }
                    }
                    log += "tRNS Chunk   :透明度\n" + " - length".PadRight(15) + ":" + tmpLength + "\n" + tmpLog;
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.gAMA)){
                    byte[] gamma = Util.CopyFromArray(chunk, 8, 4, 0);
                    int gAMA = Util.ByteArray2Int32(gamma);
                    log += "gAMA Chunk   :イメージガンマ\n" + " - length".PadRight(15) + ":" + tmpLength
                        + "\n" + " - value" + ":γ = 100000 / " + gAMA + " = " + 100000f / gAMA + "\n";
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.cHRM)){
                    string tmpLog = "";
                    tmpLog += " - white(x, y)" + ":(" + Util.ByteArray2Int32(chunk, 8, 4) / 100000;
                    tmpLog += ", " + Util.ByteArray2Int32(chunk, 12, 4) / 100000 + ")\n";
                    tmpLog += " - red(x, y)"   + ":(" + Util.ByteArray2Int32(chunk, 16, 4) / 100000;
                    tmpLog += ", " + Util.ByteArray2Int32(chunk, 20, 4) / 100000 + ")\n";
                    tmpLog += " - green(x, y)" + ":(" + Util.ByteArray2Int32(chunk, 24, 4) / 100000;
                    tmpLog += ", " + Util.ByteArray2Int32(chunk, 28, 4) / 100000 + ")\n";
                    tmpLog += " - blue(x, y)"  + ":(" +  Util.ByteArray2Int32(chunk, 32, 4) / 100000;
                    tmpLog += ", " + Util.ByteArray2Int32(chunk, 36, 4) / 100000 + ")\n";
                    log += "cHRM Chunk   :基礎色度(https://ja.wikipedia.org/wiki/CIE_1931_%E8%89%B2%E7%A9%BA%E9%96%93)\n" + " - length".PadRight(15) + ":" + tmpLength + "\n"
                        + tmpLog;
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.sRGB)){
                    string tmpLog = "";
                    if (chunk[8] == 0) tmpLog += "Perceptual";
                    else if (chunk[8] == 1) tmpLog += "Relative Colorimetric";
                    else if (chunk[8] == 2) tmpLog += "Saturation";
                    else if (chunk[8] == 3) tmpLog += "Absolute Colorimetric";
                    else tmpLog += "";
                    log += "sRGB Chunk   :標準RGBカラースペース\n" + " - length".PadRight(15) + ":" + tmpLength + "\n" + " - Rendering Intent" + ":" + tmpLog + "\n";
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.iCCP)){
                    byte[] ChunkData = Util.CopyFromArray(chunk, 8, chunk.Length - 12, 0);
                    int index = Array.IndexOf(ChunkData, (byte)0);
                    byte[] tmpName = Util.CopyFromArray(chunk, 8, index, 0);
                    string profileName = ascii.GetString(Util.CopyFromArray(chunk, 8, index, 0));
                    byte[] compressedBuffer = Util.CopyFromArray(ChunkData, profileName.Length + 2, ChunkData.Length - 2 - tmpName.Length, 0);
                    string compressedProfile = utf8.GetString(Util.Deflate(compressedBuffer));

                    log += "iCCP Chunk   :組み込みICCプロフィール\n" + " - length".PadRight(15) + ":" + tmpLength
                        + "\n" + " - profile name" + ":" + profileName
                        + "\n" + " - comp method" + ":" + (chunk[8 + profileName.Length] == 0 ? "Deflate" : "unknown") + "\n";
                    //+ "\n" + " - compressed profile" + ":" + compressedProfile +"\n";
                    //文字コード周り未完
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.tEXt)){
                    byte[] tEXt = Util.CopyFromArray(data, byteCount + 8, tmpLength, 0);
                    int sep = Array.IndexOf(tEXt, (byte)0);
                    log += "tEXt Chunk   :テキストデータ"
                        + "\n" + " - length".PadRight(15) + ":" + tmpLength
                        + "\n" + " - keyword".PadRight(15) + ":" + ascii.GetString(Util.CopyFromArray(tEXt, 0, sep, 0))
                        + "\n" + " - content".PadRight(15) + ":" + utf8.GetString(Util.CopyFromArray(tEXt, sep + 1, tmpLength - sep - 1, 0)) + "\n";
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.zTXt)){
                    byte[] zTXt = Util.CopyFromArray(data, byteCount + 8, tmpLength, 0);
                    int sep = Array.IndexOf(zTXt, (byte)0);
                    log += "zTXt Chunk   :圧縮されたテキストデータ" 
                        + "\n" + " - length".PadRight(15) + ":" + tmpLength
                        + "\n" + " - keyword".PadRight(15) + ":" + ascii.GetString(Util.CopyFromArray(zTXt, 0, sep, 0))
                        + "\n" + " - content".PadRight(15) + ":" + utf8.GetString(ZlibStream.UncompressBuffer(Util.CopyFromArray(zTXt, sep + 2, tmpLength - sep - 2, 0))) + "\n";
                    //解凍したい
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.iTXt)){
                    byte[] ChunkData = Util.CopyFromArray(chunk, 8, chunk.Length - 12, 0);
                    int index1 = Array.IndexOf(ChunkData, (byte)0);
                    byte[] keyword = Util.CopyFromArray(ChunkData, 8, index1, 0);
                    byte[] remain1 = Util.CopyFromArray(ChunkData, keyword.Length + 1, ChunkData.Length - 1 - keyword.Length, 0);
                    byte[] remain2 = Util.CopyFromArray(remain1, 2, remain1.Length - 2, 0);
                    byte[] languageTag, remain3;
                    if (remain2[0] == 0){
                        languageTag = new byte[4] { 0x4e, 0x6f, 0x6e, 0x65 };
                        remain3 = Util.CopyFromArray(remain2, 1, remain2.Length - 1, 0);
                    }
                    else{
                        int index2 = Array.IndexOf(remain2, (byte)0);
                        languageTag = Util.CopyFromArray(remain2, 0, index2, 0);
                        remain3 = Util.CopyFromArray(remain2, languageTag.Length, remain2.Length - 1 - languageTag.Length, 0);
                    }
                    byte[] translationKeyword, content;
                    if (remain3[0] == 0){
                        translationKeyword = new byte[4] { 0x4e, 0x6f, 0x6e, 0x65 };
                        content = Util.CopyFromArray(remain3, 1, remain3.Length - 1, 0);
                    }
                    else{
                        int index3 = Array.IndexOf(remain3, (byte)0);
                        translationKeyword = Util.CopyFromArray(remain3, 0, index3, 0);
                        content = Util.CopyFromArray(remain3, translationKeyword.Length, remain3.Length - 1 - translationKeyword.Length, 0);
                    }
                    log += "iTXt Chunk   :国際的なテキストデータ\n" + " - length\t " + ":" + tmpLength
                        + "\n" + " - keyword" + ":" + utf8.GetString(keyword)
                        + "\n" + " - comp flag" + ":" + remain1[0]//(compFlag == 0 ? "not compressed" " + ":"compressed")
                        + "\n" + " - comp type" + ":" + remain1[1]//(compType == 0 ? "Deflate" " + ":"unknown")
                        + "\n" + " - language tag" + ":" + utf8.GetString(languageTag)
                        + "\n" + " - translation keyword" + ":" + utf8.GetString(translationKeyword)
                        + "\n" + " - content" + ":" + utf8.GetString(content) + "\n";
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.bKGD)){
                    string tmpLog = "";
                    if (ihdr.colorTypeInt == 3){
                        log += Convert.ToInt32(chunk[8]) + " (palette number)";
                    }//palette color
                    else if (ihdr.colorTypeInt == 0 || ihdr.colorTypeInt == 4){
                        byte[] grayLevel = Util.CopyFromArray(chunk, 8, 2, 0);
                        tmpLog += Util.ByteArray2Int32(grayLevel) + "(gray level)";
                    }//gray scale
                    else if (ihdr.colorTypeInt == 2 || ihdr.colorTypeInt == 6){
                        tmpLog += Util.ByteArray2Int32(chunk,8,2)  + ", "
                                + Util.ByteArray2Int32(chunk,10,2) + ", "
                                + Util.ByteArray2Int32(chunk,12,2) + " (RGB)";
                    }//true color
                    else tmpLog += "";
                    log += "bKGD Chunk   :背景色\n" + " - length".PadRight(15) + ":" + tmpLength + "\n" + " - background color" + ":" + tmpLog + "\n";
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.pHYs)){
                    log += "pHYs Chunk   :物理的なピクセル寸法"
                        + "\n" + " - length".PadRight(25) + ":" + tmpLength
                        + "\n" + " - unit".PadRight(25) + ":" + (data[byteCount + 16] == 1 ? "[m]" : "unknown")
                        + "\n" + " - pixel per unit(x)".PadRight(25) + ":" + Util.ByteArray2Int32(data, byteCount + 8, 4)
                        + "\n" + " - pixel per unit(y)".PadRight(25) + ":" + Util.ByteArray2Int32(data, byteCount + 12, 4) + "\n";
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.sBIT)){
                    string tmpLog = "";
                    if (ihdr.colorTypeInt == 0 || ihdr.colorTypeInt == 4) tmpLog += chunk[8] + "(gray scale)";
                    if (ihdr.colorTypeInt == 2 || ihdr.colorTypeInt == 3 || ihdr.colorTypeInt == 6) tmpLog += chunk[8] + ", " + chunk[9] + ", " + chunk[10] + "(RGB)";
                    if (ihdr.colorTypeInt == 4) tmpLog += ", " + chunk[9] + "(Alpha)";
                    if (ihdr.colorTypeInt == 6) tmpLog += ", " + chunk[11] + "(Alpha)";

                    log += "sBIT Chunk   :有効なビット\n" + " - length".PadRight(15) + ":" + tmpLength + "\n" + " - Significant bits" + ":" + tmpLog + "\n";
                }
                if (Util.Compare(tmpChunkType, PNG.Chunk.sPLT)){
                    log += "sPLT Chunk   :推奨パレット\n" + " - length".PadRight(15) + ":" + tmpLength + "\n";
                }//未完(関連情報不足)
                if (Util.Compare(tmpChunkType, PNG.Chunk.hIST)){
                    log += "hIST Chunk   :パレットヒストグラム\n" + " - length".PadRight(15) + ":" + tmpLength + "\n";
                }//未完(関連情報不足)
                if (Util.Compare(tmpChunkType, PNG.Chunk.tIME)){
                    log += "tIME Chunk   :イメージの最終更新時間\n" + " - length".PadRight(15) + ":" + tmpLength + "\n" + " - content" + ":"
                        + (short)(chunk[8] * 256 + chunk[9]) + "年" + (short)chunk[10] + "月" + (short)chunk[11] + "日"
                                          + (short)chunk[12] + "時" + (short)chunk[13] + "分" + (short)chunk[14] + "秒\n";
                }
                //データからCRC部分読み込み
                long CRC1 = Util.ByteArray2Int64(Util.CopyFromArray(data, byteCount + 8 + tmpLength, 4, 0));
                //実際にCRCを計算
                long CRC2 = Util.calcCRC(Util.CopyFromArray(data, byteCount + 4, tmpLength + 4, 0));
                //実際の計算と格納データが等しいか
                log += " - CRC check".PadRight(15) + ":" + (CRC1 == CRC2 ? "OK" : "XXX") + "\n";

                byteCount += (tmpLength + 12);
                log += "\n";
                GC.Collect();
            }
            #endregion
        }
        catch (Exception e)
        {
            log = e.Message;
        }
        return log;
    }
}

