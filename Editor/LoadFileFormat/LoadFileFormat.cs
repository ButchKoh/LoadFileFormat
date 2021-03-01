using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;

public class LoadFileFormat : EditorWindow
{
    [MenuItem("Tools/Load File Format")]
    static void Open() 
    {
        GetWindow<LoadFileFormat>();
    }
    EditorSettings setting;
    private bool writeLog,openLink, logPrepare;
    public static bool breakCoRoutine;
    private UnityEngine.Object source;
    private byte[] data;
    System.Text.ASCIIEncoding ascii;
    private int language;
    private string log;
    public static string guiMessage = "";
    private string link = "";
    string texturePath;
    DateTime startTime;


    private void OnGUI()
    {

        setting = Resources.Load<EditorSettings>("settings");
        language = setting.language;
        logPrepare = false;
        ascii = new System.Text.ASCIIEncoding();

        #region GUI
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(language==1?"Put in the file here":"ファイルを指定");
        if (source != null) Undo.RecordObject(source, "source object");
        source = EditorGUILayout.ObjectField(source, typeof(UnityEngine.Object), false);
        EditorGUILayout.HelpBox((language == 1 ? "The currently supported formats are followings" : "現在サポート中のファイル形式")
            + ":\nJPEG, PNG, GIF, BMP, MP3, ", MessageType.None);
        GUILayout.Space(15);
        writeLog = EditorGUILayout.ToggleLeft(language == 1 ? "Write log to text file." : "出力をテキストに書き込む", writeLog);
        openLink = EditorGUILayout.ToggleLeft(language == 1 ? "Open reference web page." : "参考リンクを開く", openLink);
        EditorGUI.EndChangeCheck();

        GUILayout.Space(15);
        EditorGUI.BeginChangeCheck();
        EditorGUI.BeginDisabledGroup(source == null);
        bool button = GUILayout.Button(language == 1 ? "start" : "開始");
        EditorGUI.EndDisabledGroup();
        #endregion

        texturePath = AssetDatabase.GetAssetPath(source);
        if (EditorGUI.EndChangeCheck())
        {

            try
            {
                startTime = DateTime.Now;
                log = "";
                guiMessage = "";
                breakCoRoutine = false;
                data = File.ReadAllBytes(texturePath);
                byte[] MagicNumber = Util.CopyFromArray(data, 0, 16, 0);

                //JPEG(Joint Photographic Experts Group)
                //ISO/IEC 10918
                //ITU-T T.81
                //JIS X 4301(https://kikakurui.com/x4/X4301-1995-01.html)
                if (Util.CheckMagicNumber(MagicNumber, Signatures.JPEG))
                {
                    log += "FILE TYPE : JPEG IMAGE\n";
                    log += Signatures.JPEGdesc + "\n\n";
                    log += "path : " + texturePath + "\n";
                    LoadJpg lj = new LoadJpg();
                    log += lj.LoadStart(data);
                    link = language == 1 ? "http://vip.sugovica.hu/Sardi/kepnezo/JPEG%20File%20Layout%20and%20Format.htm"
                                         : "https://www.setsuki.com/hsp/ext/jpg.htm";
                    logPrepare = true;
                }
                //PNG(Portable Network Graphics)
                //RFC 2083(https://tools.ietf.org/html/rfc2083)
                //ISO/IEC 15948
                //W3C http://www.libpng.org/pub/png/spec/iso/index-object.html
                else if (Util.CheckMagicNumber(MagicNumber, Signatures.PNG))
                {
                    log += "FILE TYPE : PNG IMAGE\n";
                    log += Signatures.PNGdesc + "\n\n";
                    log += "path : " + texturePath + "\n";
                    LoadPng lp = new LoadPng();
                    log += lp.LoadStart(data);
                    link = language == 1 ? "https://pmt.sourceforge.io/specs/png-1.2-pdg-h20.html"
                                         : "https://www.setsuki.com/hsp/ext/png.htm";
                    logPrepare = true;
                }
                //BITMAP(Microsoft Windows Bitmap Image)
                //.bmp,(.dib?)
                //DIB(Device Independent Bitmap):BMPをメインメモリに展開した形式
                else if (Util.CheckMagicNumber(MagicNumber, Signatures.BMP))
                {
                    log += "FILE TYPE : BITMAP IMAGE\n";
                    log += Signatures.BMPdesc + "\n\n";
                    log += "path : " + texturePath + "\n";
                    LoadBmp lb = new LoadBmp();
                    log += lb.LoadStart(data);
                    link = language == 1 ? "http://www.ece.ualberta.ca/~elliott/ee552/studentAppNotes/2003_w/misc/bmp_file_format/bmp_file_format.htm"
                                         : "http://sunpillar.jf.land.to/bekkan/data/files/bmp.html";
                    logPrepare = true;
                }
                //GIF(Graphics Interchange Format/GIF87, GIF87a, GIF89a)
                //https://www.w3.org/Graphics/GIF/spec-gif89a.txt
                else if (Util.CheckMagicNumber(MagicNumber, Signatures.GIF87a) |
                         Util.CheckMagicNumber(MagicNumber, Signatures.GIF89a))
                {
                    log += "FILE TYPE : GIF IMAGE\n";
                    log += Signatures.GIF89adesc + "\n\n";
                    log += "path : " + texturePath + "\n";
                    LoadGif lg = new LoadGif();
                    this.StartCoroutine(this.LoadGifImage(data));
                    link = language == 1 ? "http://www.awm.jp/~yoya/cache/www.onicos.com/staff/iz/formats/gif.html.orig"
                                         : "http://www.tohoho-web.com/wwwgif.htm";
                    if (logPrepare)
                    {
                        log += lg.tmpLog;
                        logPrepare = true;
                    }
                }
                //MP3(MPEG-1 Audio Layer-3)
                //ID3v2.3
                //ISO/IEC 11172-3 / ISO/IEC 13818-3
                else if (Util.CheckMagicNumber(MagicNumber, Signatures.MP3_ID3v2)|
                    Util.CheckMagicNumber(MagicNumber, Signatures.MP3_ID3v1_1)|
                    Util.CheckMagicNumber(MagicNumber, Signatures.MP3_ID3v1_2)| 
                    Util.CheckMagicNumber(MagicNumber, Signatures.MP3_ID3v1_3))
                {
                    log += "FILE TYPE : MP3\n";
                    log += Signatures.MP3_ID3v2desc + "\n\n";
                    log += "path : " + texturePath + "\n";
                    log += "(about encoding proplem(japanese article) : " + "https://www.wa2c.com/wp/2015/11/23131700" + ")\n";
                    log += "日本語入力等に関して、フォーマット内の指定の文字コードを無視したShift-JISでの入力が原因で文字化けしている場合があります(本来の仕様で利用可能なのはLatin-1、UTF-16、UTF-16BE、UTF-8のみ)。\n\n";
                    LoadMp3 lm = new LoadMp3();
                    log += lm.LoadMPEGFrameHeader(data) + "\n\n";
                    log += lm.LoadID3Info(data);
                    link = language == 1 ? "https://www.diva-portal.org/smash/get/diva2:830195/FULLTEXT01.pdf"
                                         : "http://takaaki.info/wp-content/uploads/2013/01/ID3v2.3.0J.html";
                    logPrepare = true;
                }
                else
                {
                    if (Util.CheckMagicNumber(MagicNumber, Signatures.SHEBANG))        log = Signatures.SHEBANGdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.LIBCAP1))   log = Signatures.LIBCAP1desc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.LIBCAP2))   log = Signatures.LIBCAP2desc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.PCAPNG))    log = Signatures.PCAPNGdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.SQLITE))    log = Signatures.SQLITEdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.ICON))      log = Signatures.ICONdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.t3GPP))     log = Signatures.t3GPPdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.LZWcomp))   log = Signatures.LZWcompdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.LZHcomp))   log = Signatures.LZHcompdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.BZIP2comp)) log = Signatures.BZIP2compdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.GIF87a))    log = Signatures.GIF87adesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.GIF89a))    log = Signatures.GIF89adesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.TIFF_LE))   log = Signatures.TIFF_LEdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.TIFF_BE))   log = Signatures.TIFF_BEdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.CANONraw))  log = Signatures.CANONrawdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.OpenEXR))   log = Signatures.OpenEXRdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.JPEG))      log = Signatures.JPEGdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.LZIP))      log = Signatures.LZIPdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.DOS_MZ))    log = Signatures.DOS_MZdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.ZIPbased))  log = Signatures.ZIPbaseddesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.ZIPea))     log = Signatures.ZIPeadesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.ZIPsa))     log = Signatures.ZIPsadesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.RAR1_5))    log = Signatures.RAR1_5desc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.RAR5_0))    log = Signatures.RAR5_0desc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.PNG))       log = Signatures.PNGdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.PDF))       log = Signatures.PDFdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.Win_ASF))   log = Signatures.Win_ASFdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.OGG))       log = Signatures.OGGdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.PHOTOSHOP)) log = Signatures.PHOTOSHOPdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.RIFF))
                    {
                        if (Util.CheckMagicNumber2(MagicNumber, Signatures.WAVE))       log = Signatures.WAVEdesc;
                        else if (Util.CheckMagicNumber2(MagicNumber, Signatures.AVI))   log = Signatures.AVIdesc;
                        else if (Util.CheckMagicNumber2(MagicNumber, Signatures.WEBP))  log = Signatures.WEBPdesc;
                        else if (Util.CheckMagicNumber2(MagicNumber, Signatures.CDA))   log = Signatures.CDAdesc;
                        else if (Util.CheckMagicNumber2(MagicNumber, Signatures.QCP))   log = Signatures.QCPdesc;
                        else if (Util.CheckMagicNumber2(MagicNumber, Signatures.ANI))   log = Signatures.ANIdesc;
                        else log = "UNKNOWN";
                    }
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.MP3_ID3v1_1))      log = Signatures.MP3_ID3v1_1desc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.MP3_ID3v1_2))      log = Signatures.MP3_ID3v1_2desc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.MP3_ID3v1_3))      log = Signatures.MP3_ID3v1_2desc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.MP3_ID3v2))        log = Signatures.MP3_ID3v2desc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.BMP))              log = Signatures.BMPdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.FLAC))             log = Signatures.FLACdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.MIDI))             log = Signatures.MIDIdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.CFBC_oldMSOffice)) log = Signatures.CFBC_oldMSOfficedesc;                                                                                                                                                                        
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.GZIP))             log = Signatures.GZIPdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.WEBM))             log = Signatures.WEBMdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.XML))              log = Signatures.XMLdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.RTF))              log = Signatures.RTFdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.MP4))              log = Signatures.MP4desc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.FLV))              log = Signatures.FLVdesc;
                    else if (Util.CheckMagicNumber(MagicNumber, Signatures.WinRegistry))      log = Signatures.WinRegistrydesc;
                    else                                                                      log = "UNKNOWN";
                    log = (language == 1 ? "This file type is not supported yet\nThe file has one of signature of following formats : " :
                                            "未対応の形式のファイルですが、以下の形式の内のどれかのマジックナンバー(signature)を持っています。\n") + log;
                    logPrepare = true;
                    writeLog = false;
                    openLink = false;
                }
                if (log.EndsWith("UNKNOWN"))
                {
                    try
                    {
                        string[] ext = texturePath.Split('.');
                        if (ext[ext.Length - 1] == texturePath) throw new Exception();
                        log = log.Replace("UNKNOWN", "No signature\nExtension: ." + ext[ext.Length - 1]);
                    }
                    catch (Exception)
                    {
                        log = language == 1 ? "This file type is not supported yet\nThe file has neither signature nor extension." :
                                              "未対応の形式のファイルです。指定のファイルには既知のマジックナンバー(signature)も拡張子もありません。";
                    }
                }
                Output();
                
            }
            catch(Exception){}
        }

        if(GUILayout.Button(language == 1 ? "reset" : "リセット")){
            breakCoRoutine = true;
            source = null;
            guiMessage = "All values has reset.";
            EditorUtility.ClearProgressBar();
        }

        GUILayout.Space(15);
        EditorGUILayout.LabelField(language == 1 ? "How to use" : "使い方");
        string tmp = language == 1 ? "Specify the file on the object field, then press the [start] button to log file format(or metadata) on the console window. "
                               + "If the toggle button [Write log to text file] is on, the log message will be written as a .txt file as well, in the same directory as the file you specified. "
                               + "If the toggle button [Open reference web page] is on, a reference web page will be opened after its logging." 
                               : "[開始]を押すと指定したファイルのフォーマットやメタデータをConsoleウィンドウに出力します。"
                               + "[出力をテキストに書き込む]にチェックを入れると指定ファイルと同じディレクトリに同じログを.txtで書き出し出来ます。"
                               + "[参考リンクを開く]にチェックを入れるとログを出した後参考ページを開きます。";
        EditorGUILayout.HelpBox(tmp, MessageType.None);
        if (guiMessage.Length != 0) EditorGUILayout.HelpBox(guiMessage, MessageType.Info);
        
    }

    //GIFはサイズ大きいので
    private IEnumerator LoadGifImage(byte[] data) 
    {
        LoadGif lg = new LoadGif();
        yield return this.StartCoroutine(lg.LoadGifDetailInfo(data));
        
        //clearしないと表示され続ける
        EditorUtility.ClearProgressBar();
        
        log += lg.tmpLog;
        logPrepare = true;
        Output();
    }


    private void Output()
    {
        if (logPrepare&!breakCoRoutine)
        {
            Debug.Log(log);
            if (writeLog)
            {
                string path = texturePath.Replace(".", "_") + "_DELAIL_LOG.txt";
                log += "\n" + DateTime.Now + "\n";
                TimeSpan ts = DateTime.Now - startTime;
                log += "Calculation Tile : " + ts.TotalSeconds + " [sec]\n";
                File.WriteAllText(path, BitConverter.ToString(data));
                //File.WriteAllText(path, log);
                AssetDatabase.Refresh();
                guiMessage = ".txt file has created!\n" + path;
            } else guiMessage = "Finish loading!";

            if (openLink) Application.OpenURL(link);
        }
        EditorUtility.ClearProgressBar();
    }
}
