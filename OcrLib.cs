using Emgu.CV;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR
{
    class OcrLib
    {
        public  String InitializeTesseract(Bitmap bmp, String DataFile, String language, String blacklist, PageSegMode psm) //datapasth(@"D:\HARI\")    //language("eng") //blacklist("©.")
        {
            variableRes vr = new variableRes();
            string result=null;
            try
            {
            
                //vr.res = "";
                //String result = "";
                Image<Bgr, Byte> pic = new Image<Bgr, Byte>(bmp);
                // Image<Gray, Byte> imageInvert = new Image<Gray, Byte>(pic.Width, pic.Height);
                //Image<Gray, Byte> thresholded = imageInvert.ThresholdAdaptive(new Gray(255), Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC, Emgu.CV.CvEnum.ThresholdType.Binary, 1, new Gray(0.03));
                Tesseract _ocr;
                _ocr = new Tesseract(DataFile, language, OcrEngineMode.TesseractLstmCombined);
                _ocr.SetVariable("tessedit_char_whitelist", "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopkrstuvwxyz\\//");
                _ocr.PageSegMode = psm;
                _ocr.SetImage(pic);
                vr.res = _ocr.GetUTF8Text();
                _ocr.Dispose();
                CharChopper cp = new CharChopper();
                result = cp.trim(vr.res, StringAsEnumerable(blacklist));

            }
            catch (Exception ex) { throw ex.InnerException; }
            
            return result;
        }
        private static IEnumerable<char> StringAsEnumerable(string s)
        {
            foreach (char c in s)
            {
                yield return c;
            }
        }


    }
    class variableRes
    {

        public string res { get; set; }


    }
}
