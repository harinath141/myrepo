using AForge.Imaging;
using AForge.Imaging.Filters;
using Emgu.CV;
using Emgu.CV.Structure;
using RestApi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using OCR;
using Emgu.CV.CvEnum;
using TensorFlow;
using Emgu.CV.Features2D;
using Emgu.CV.Util;

namespace RestApi.Controllers
{
    public class HomeController : ApiController
    {
        private static string path = ConfigurationManager.AppSettings["Path"];
        private static string root = HttpContext.Current.Server.MapPath("~/temp");
        private static string dict= HttpContext.Current.Server.MapPath("~/tessdata");
        public StringBuilder result =new StringBuilder();
        public BradleyLocalThresholding Tfilter = new BradleyLocalThresholding();

        /*-----------------------------Upload method---------------------------------------------------------------------------
         public static void SetFile(string serviceurl,byte[] filearray,String filename)
         {
         try
         {
            using(var client=new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        using(var content=new MultiFormDataContent())
                            {
                                var filecontent=new ByteArrayConent(fileArray);
                                fileContent.Headers.ContentDisposition=new ContentDispositionHeaderValue("attachment");
                                {FileName=fileName};
                                content.Add(fileContent);
                                var result=client.PostAsync(serviceUrl,content).Result;
                            }
                  }
         }
         catch(Exception ex)
         {throw ex;}
         }
         -----------------------------------------------------------------------------------------------------------------------*/
        [HttpPost]
        [Route("api/home/upload")]
        public async Task<HttpResponseMessage> upload()
        {
            HttpResponseMessage response;
            string OcrResult = null;
           
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpRequestException(HttpStatusCode.UnsupportedMediaType.ToString());
            try
            {
                var streamProvider = new MultipartFormDataStreamProvider(root);
                await Request.Content.ReadAsMultipartAsync(streamProvider);
                ApplicationDetails Appdet = new ApplicationDetails();
                Appdet.Appno = streamProvider.FormData.GetValues("Appno").FirstOrDefault();
                Appdet.firstname = streamProvider.FormData.GetValues("firstname").FirstOrDefault();
                Appdet.lastname = streamProvider.FormData.GetValues("lastname").FirstOrDefault();
                Appdet.middlename = streamProvider.FormData.GetValues("middlename").FirstOrDefault();
                Appdet.dob = streamProvider.FormData.GetValues("dob").FirstOrDefault();
                if (!Directory.Exists(path + "\\" + Appdet.Appno))
                    Directory.CreateDirectory(path + "\\" + Appdet.Appno + "\\");
                foreach (MultipartFileData fileData in streamProvider.FileData)
                {
                    if (string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "This request is not properly formatted");
                    }
                    string fileName = fileData.Headers.ContentDisposition.FileName;
                    if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                    {
                        fileName = fileName.Trim('"');
                    }
                    if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                    {
                        fileName = Path.GetFileName(fileName);
                    }
                    File.Move(fileData.LocalFileName, Path.Combine(path + "\\" + Appdet.Appno, fileName));
                }
              
                    OcrResult = NameOcr(path + "\\" + Appdet.Appno + "\\");
                
               
                
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.ExpectationFailed, ex.InnerException);
            }
            response = Request.CreateResponse(HttpStatusCode.OK, OcrResult);
            return response;
        }
        public string NameOcr(string directorypath)
        {
            string result=null;
            IEnumerable<string> files = Directory.GetFiles(directorypath).Where(a => a.Contains("AgeProof"));
            foreach (var file in files)
            {

                result = doocr(file);   
            }
            return result;
        }
        private String doocr(string file)
        {
            try
            {
                Task<string> DocAngle = Task.Factory.StartNew(() => { return detectangle(file); });
                Image<Bgr, Byte> img = new Image<Bgr, byte>(file);
                 Task.WaitAll(DocAngle);
                Image<Bgr, byte> rotatedImage=null;
                switch (DocAngle.Result.ToString())
                {
                    case "0":
                        rotatedImage = img;
                        break;
                    case "90":
                        RotateBilinear rf = new RotateBilinear(90);
                        rotatedImage=new Image<Bgr,byte>(rf.Apply(img.ToBitmap()));
                        break;
                    case "180":
                        rotatedImage=img.Rotate(180, new Bgr(255, 255, 255));
                        break;
                    case "270":
                        RotateBilinear rf1 = new RotateBilinear(0);
                        rotatedImage = new Image<Bgr, byte>(rf1.Apply(img.ToBitmap()));
                        break;
                    default:
                        rotatedImage = img;
                        break;
                }
                Image<Gray, byte> Grayimage = rotatedImage.SmoothMedian(3).Convert<Gray, byte>();
                double angle;
                using (Image<Gray, Byte> canny = new Image<Gray, byte>(Grayimage.Size))
                {
                    CvInvoke.Canny(Grayimage, canny, 80, 120);
                    angle = skewangle(canny);
                }   
                Tfilter.ApplyInPlace(Grayimage.Bitmap);
                
                //Mat SE = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
                //Grayimage = Grayimage.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Erode, SE, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(255));
                //Grayimage = Grayimage.MorphologyEx(Emgu.CV.CvEnum.MorphOp.Dilate, SE, new Point(-1, -1),1, Emgu.CV.CvEnum.BorderType.Default, new MCvScalar(255));
                Grayimage.Rotate(angle, new Gray(255));
                using (Bitmap b = FindTextRegion(Grayimage))
                {
                    OcrLib ocr = new OcrLib();
                    string res = ocr.InitializeTesseract(b, dict, "eng", "°\\}<!¢;+{,©@_|«»?.$%*~-(€'", Emgu.CV.OCR.PageSegMode.Auto);
                    result.Append(res);
                }                  
            }
            catch (Exception ex)
            {
                result.Append("Exception: " + ex);

            }
            return result.ToString();

        }
        public double skewangle(Image<Gray, byte> img)
        {
            //Image<Gray, byte> image = new Image<Gray, byte>(bmp);
            img.SmoothGaussian(3, 3, 34.3, 45.3);
            img.Erode(2);
            LineSegment2D[] lines = img.HoughLinesBinary(
1, //Distance resolution in pixel-related units
Math.PI / 45.0, //Angle resolution measured in radians. ******
100, //threshold
30, //min Line width
10 //gap between lines
)[0]; //Get the lines from the first channel
            img.Dispose();
            double[] angle = new double[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                double result = (double)(lines[i].P2.Y - lines[i].P1.Y) / (lines[i].P2.X - lines[i].P1.X);
                angle[i] = Math.Atan(result) * 57.2957795;

            }
            double avg = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                avg += angle[i];
            }
            avg = avg / lines.Length;
            return avg;
        }
        public string detectangle(string file)
        {
            string Finalangle = null;
            // var modelFile = HttpContext.Current.Server.MapPath("~/output.pb");
            var modelFile = @"N:\OcrApi\bin\output.pb";
            var labelsFile = @"N:\OcrApi\bin\labels.txt";
           // var labelsFile = HttpContext.Current.Server.MapPath("~/labels.txt");
            var labels = File.ReadAllLines(labelsFile);
            var modelContent = File.ReadAllBytes(modelFile);
            var graph = new TFGraph();
            GC.Collect();
            graph.Import(modelContent, "");
            using (var session = new TFSession(graph))
            {
                try
                    {
                    FileClassificationResult fileResult = new FileClassificationResult();
                        var tensor = CreateTensorFromImageFile(File.ReadAllBytes(file));
                        var runner = session.GetRunner();
                        TFOutput classificationLayer = graph["final_result"][0];
                        TFOutput bottleneckLayer = graph["pool_3"][0];
                        TFOutput tIn = graph["DecodeJpeg"][0];
                        runner.AddInput(tIn, tensor).Fetch(classificationLayer, bottleneckLayer);
                        var output = runner.Run();

                        // Bottleneck result.
                        var result = output[1];
                        var values = ((Single[][][][])result.GetValue(jagged: true))[0][0][0];
                        fileResult.Bottleneck = values;

                        // Classification result.
                        result = output[0];

                        var probabilities = ((float[][])result.GetValue(jagged: true))[0];

                        var predictions = new List<Prediction>();
                        for (int i = 0; i < probabilities.Length; i++)
                        {
                            predictions.Add(new Prediction
                            {
                                ClassificationId = i,
                                Label = (i >= labels.Length) ? "Unknown" : labels[i],
                                Percent = probabilities[i]
                            });
                        }
                        fileResult.Predictions = predictions;
                        fileResult.File = file;
                        graph.Dispose();
                            var predictionsFinal = fileResult.Predictions.OrderByDescending(m => m.Percent)
                            .Where(m => m.Percent >= 0.10).Take(1);
                            foreach (var prediction in predictionsFinal)
                            {

                                Finalangle = prediction.Label;
                                
                            }
                }
                    catch (Exception ex)
                    {
                    throw ex;
                    }
                return Finalangle;
            }
           

        }
        static TFTensor CreateTensorFromImageFile(byte[] contents)
        {
            // DecodeJpeg uses a scalar String-valued tensor as input.
            var inputTensor = TFTensor.CreateString(contents);
            TFGraph graph = new TFGraph();
            TFOutput input = graph.Placeholder(TFDataType.String);
            TFOutput output = graph.DecodeJpeg(contents: input, channels: 3);

            using (var session = new TFSession(graph))
            {
                var tensor = session.Run(
                    inputs: new[] { input },
                    inputValues: new[] { inputTensor },
                    outputs: new[] { output });
                return tensor[0];
            }
        }
        private Bitmap FindTextRegion(Image<Gray,byte> grayimage)
        {
            MSERDetector mser = new MSERDetector();
            //BradleyLocalThresholding br = new BradleyLocalThresholding();
            //Bitmap newth = br.Apply(grayimage.ToBitmap());
            //Image<Gray, byte> newgray = new Image<Gray, byte>(newth);
            //newth.Dispose();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfRect rects = new VectorOfRect();
            mser.DetectRegions(grayimage, contours, rects);
            mser.Dispose();
            List<Rectangle> rt = new List<Rectangle>();
            for (int i = 0; i < Convert.ToInt16(rects.Size); i++)
            {
                int area = rects[i].Height * rects[i].Width;
                if (area > 150 && rects[i].Height > 10 && rects[i].Width > 5)
                {
                    rt.Add(rects[i]);
               }
            }
            Bitmap b= DrawFilledRectangle(grayimage.Width, grayimage.Height);
            foreach (Rectangle r in rt)
            {

                CopyRegionIntoImage(grayimage.Bitmap, r, ref b, r);
            }
            //b.Save(@"D:\HARI\PanImages\OS076701_new.png");
            #region disposable objects
            rects.Dispose();
            contours.Dispose();
            #endregion
            return b;
        }
        private Bitmap DrawFilledRectangle(int x, int y)
        {
            Bitmap bmp = new Bitmap(x, y);
            using (Graphics graph = Graphics.FromImage(bmp))
            {
                Rectangle ImageSize = new Rectangle(0, 0, x, y);
                graph.FillRectangle(Brushes.White, ImageSize);
            }
            return bmp;
        }
        public static void CopyRegionIntoImage(Bitmap srcBitmap, Rectangle srcRegion, ref Bitmap destBitmap, Rectangle destRegion)
        {
            using (Graphics grD = Graphics.FromImage(destBitmap))
            {
                grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
            }
        }
    }
}

