using System;
using System.Web.UI.WebControls;
using System.Drawing;
using System.IO;
using Tesseract;
using Emgu.CV;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;
using Emgu.CV.Structure;
using System.Linq;

namespace Invoice_processesing_application
{
    public partial class wf1 : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {

        }
        public string filename;
        public string fullfilename { get; set; }
        public string targetpth;
         public String[] wrds;
        public String imd;
        public FileStream file;
        public void UploadButton_Click(object sender, EventArgs e)
        {
            filename = Path.GetFileName(fl_upload.FileName);

            Session["imd"] = Guid.NewGuid().ToString() + "-" + filename;
            Session["targetpth"] = Server.MapPath(@"~/temp/");
            fl_upload.SaveAs(Server.MapPath(@"~/temp/") + Session["imd"]);
            Session["fullfilename"] = Session["targetpth"].ToString() +Session["imd"];
           // file = new FileStream(fullfilename, FileMode.Open);
            ImageBox.ImageUrl = "temp/"+Session["imd"].ToString();
           

        }
        public void ProcessButton_Click(object sender, EventArgs e)
        {
            if(Session["fullfilename"]!=null)
            {
                fullfilename = Session["fullfilename"].ToString();
                
            }

            file = new FileStream(fullfilename, FileMode.Open);
            if (Session["targetpth"] != null)
            {
                targetpth = Session["targetpth"].ToString();

            }
            
            try
            {
                
                using (Bitmap img = new Bitmap(file))
                {
                    using (Bitmap finalimg = processimage(img))
                    {
                        finalimg.Save(targetpth + imd + "erode.png");
                        
                        
                    }
                    var testImagePath = targetpth + imd + "erode.png";


                    using (var engine = new TesseractEngine(Server.MapPath(@"./tessdata"), "swe1", EngineMode.Default,"config"))
                    {
                        using (var img2 = Pix.LoadFromFile(testImagePath))
                        {

                            using (var page = engine.Process(img2, PageSegMode.SingleBlock))

                            {
                                engine.SetVariable("tessedit_char_blacklist", ";|!'[]?”*()»&@^=<>_''~");

                                string str = String.Format("{0:P}", page.GetText());
                                str = str.Replace(',', '.');
                                if (String.IsNullOrEmpty(str)) { }
                                else
                                {
                                    string[] words = { "MOMS", "NETTO", "MASTER", "CARD", "Belopp", "VISA", "KORTKOP", "Netto", "PAYMENT" };
                                    string[] strwords = str.Split('\n', ' ');
                                    for (int i = 0; i < strwords.Count() - 2; i++)
                                    {
                                        string start = strwords[i];
                                        for (int j = 0; j < words.Count(); j++)
                                        {
                                            string end = words[j];
                                            int score = EditDistance(start, end);
                                            if (score < 2 && (score > 1))
                                            {
                                                int index = str.IndexOf(start);
                                                int count = str.Length;
                                                string src = str.Remove(index, count).Insert(index, words[j]);

                                            }
                                        }


                                    }
                                    doprocess(str);
                                }
                                page.Dispose();
                                img2.Dispose();
                            }
                        }
                        engine.Dispose();
                    }
                    GC.Collect();
                    ImageBox.ImageUrl = "temp/"+ "erode.png";
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = ex.Message;
                StatusLabel.Visible = true;

            }

        }
        public void doprocess(string str)
        {
            double VAT1=0.00;
            double VAT2 = 0.00;
            double VAT3 = 0.00;
            mer_txtbx.Text = getmername(str).Trim(':',';',',','.','/','\\','}','{','[',']','`','+','-','^','"', '—','0','1','2','3','4','5','6','7','8','9');
            dt_txtbx.Text = getdate(str);
            net_txtbx.Text = getcompanyno(str);
            amt_txtbx.Text= getamount(str);
            
            if (Isvat25(str))
            {
                vat25bx.Text = "25.00";
                VAT1 = Math.Round(Convert.ToDouble(getamount(str))-(((Convert.ToDouble(getamount(str)) * 100.00) / 125.00)),2);
            }
            else
            {
                vat25bx.Text = "0.00";
            }
            if (Isvat16(str))
            {
                vat16bx.Text = "16.00";
                VAT2 = Math.Round(Convert.ToDouble(getamount(str)) - (((Convert.ToDouble(getamount(str)) * 100.00) / 116.00)), 2);
            }
            else
            {
                vat16bx.Text = "0.00";
            }
            if (Isvat12(str))
            {
                vat12bx.Text = "12.00";
                VAT3 = Math.Round(Convert.ToDouble(getamount(str)) - ((Convert.ToDouble(getamount(str)) * 100.00) / 112.00), 2);
            }
            else
            {
                vat12bx.Text = "0.00";
            }
            if (Isvat12(str) && Isvat16(str))
            {
                vat12bx.Text = "12.00";
                vat16bx.Text = "16.00";
                VAT3 = Math.Round(Convert.ToDouble(getamount(str)) - ((Convert.ToDouble(getamount(str)) * 100.00) / 112.00), 2);
                VAT2= Math.Round(Convert.ToDouble(getamount(str)) - (((Convert.ToDouble(getamount(str)) * 100.00) / 116.00)-VAT3), 2);

            }
            if(Isvat16(str)&&Isvat25(str))
            {
                vat16bx.Text = "16.00";
                vat25bx.Text = "25.00";
                VAT2= Math.Round(Convert.ToDouble(getamount(str)) - (((Convert.ToDouble(getamount(str)) * 100.00) / 116.00)), 2);
                VAT1= Math.Round(Convert.ToDouble(getamount(str)) - (((Convert.ToDouble(getamount(str)) * 100.00) / 125.00))-VAT2, 2);
            }
            if (Isvat25(str) && Isvat12(str))
            {
                vat25bx.Text = "25.00";
                vat12bx.Text = "12.00";
                VAT1= Math.Round(Convert.ToDouble(getamount(str)) - (((Convert.ToDouble(getamount(str)) * 100.00) / 125.00)), 2);
                VAT3= Math.Round(Convert.ToDouble(getamount(str)) - ((Convert.ToDouble(getamount(str)) * 100.00) / 112.00)-VAT1, 2);
            }
            vt1_amt.Text = VAT1.ToString();
            vt2_amt0.Text = VAT2.ToString();
            vt3_amt1.Text = VAT3.ToString();
            nt_amt.Text = (Convert.ToDouble(getamount(str)) - Convert.ToDouble(VAT1) - Convert.ToDouble(VAT2) - Convert.ToDouble(VAT3)).ToString();
            if (new[] { "MASTERCARD", "MASTER", "KORT", "KORTKOP", "master", "card", "kort", "kortkop", "visa", "VISA" }.Any(c => str.Contains(c)))
            {
                pt_type.Text = "CARD";
            }
            else
            {
                pt_type.Text = "CASH";
            }

        }
        public Bitmap processimage(Bitmap img)
        {
            if (Session["targetpth"] != null)
            {
                targetpth = Session["targetpth"].ToString();

            }
            Image<Gray, byte> grayImage = new Image<Gray, byte>(img);
            Image<Gray, byte> binarize = new Image<Gray, byte>(img.Width,img.Height,new Gray(0));
            CvInvoke.cvThreshold(grayImage.Ptr,binarize.Ptr,50,255,Emgu.CV.CvEnum.THRESH.CV_THRESH_OTSU);
            grayImage.Dispose();
            using (MemStorage store = new MemStorage())
            {
                Contour<System.Drawing.Point> largestContour = null;
                Double largestarea = 0;
                for (Contour<System.Drawing.Point> contours =binarize.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_NONE, Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_TREE, store); contours != null; contours = contours.HNext)
                {
                    if (contours.Area > largestarea)
                    {
                        largestarea = contours.Area;
                        largestContour = contours;
                    }
                }
                Rectangle r = largestContour.BoundingRectangle;
                int w;
                if(r.Width > binarize.Width)
                {
                    w=binarize.Width;
                }
                else
                {
                   w= r.Width;
                }
                int h;
                if (r.Height > binarize.Height)
                {
                    h = binarize.Height;
                }
                else
                {
                    h= r.Height;
                }
               
                
                Bitmap cropped = CropImage(binarize.ToBitmap(), r.X, r.Y,w ,h);
                Bitmap cropped1 = featureext(cropped);
                cropped1.Save(targetpth + "newly.png");
                Image<Bgr, byte> image = new Image<Bgr, byte>(cropped1);
                 image.SmoothGaussian(3, 3, 34.3, 45.3);
               // Image<Bgr, byte> newbit = new Image<Bgr, byte>(image.Size);
                image.Erode(2);
                LineSegment2D[] lines = image.HoughLinesBinary(
1, //Distance resolution in pixel-related units
Math.PI / 45.0, //Angle resolution measured in radians. ******
100, //threshold
30, //min Line width
10 //gap between lines
)[0]; //Get the lines from the first channel

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
                Bgr g = new Bgr(Color.White);

                image.Rotate(-avg, g);

                image=test(image,targetpth + "pers.png",r);
                Bitmap dilated = image.ToBitmap();
                dilated.Save(targetpth+"dilated.png");
                cropped.Dispose();
                cropped1.Dispose();
                return dilated;
            }
        }
        public Bitmap featureext(Bitmap bmp)
        {
            int height = bmp.Height;
            int width = bmp.Width;
            int midheight = height / 2;
            int midwidth = width / 2;
            int x = Convert.ToInt32((midwidth * 3.7) / 4);
            int y= Convert.ToInt32((midheight * 3.7) / 4);
            int Y = midheight - y;
            int X = midwidth - x;
            int Y1 = midheight + y;
            int X1 = midwidth + x;
            int FW = X1 - X;
            int FH = Y1 - Y;
            Bitmap bmmp=CropImage(bmp, X, Y, FW, FH);
            return bmmp;
        }
        public string getdate(string str)
        {
            string date=null;
            string time=null;
            Regex time1 = new Regex(@"\d{2}:\d{2}:\d{2}");
            Regex date1 = new Regex(@"(\d+)[-.\/](\d+)[-.\/](\d+)");
            Match m = date1.Match(str);
            Match m1 = time1.Match(str);
          
          
            if (m.Success)
            {
                string dt = m.Value;
                date = dt.ToString();               
            }
            if (m1.Success)
            {
                string tm = m1.Value;
                time = tm.ToString();              
            }
            date = date + " " + time;
            return date;
        }
        public string getcompanyno(string str)
        {
            string total=null;
           
                Regex r = new Regex(@"\d{6}[-—]\d{4}");
                Match m = r.Match(str);
                if (m.Success)
                {
                    total = m.Value;

                }
            
            
            return total;
        }
        public string getamount(string str)

        {
            string total = null;
            string[] words = str.Split('\n');
            List<double> ab = new List<double>();
            foreach (string word in words)
            {
                Regex r = new Regex(@"(\d+)[.](\d+)");
                Match m = r.Match(word);
               
                if (m.Success)
                {
                    ab.Add(Convert.ToDouble(m.Groups[0].Value));
                   
                }
                             
            }
            ab.Sort();
            if (ab.Count >= 2)
            {
                total = ab[ab.Count - 1].ToString();
            }

            return total;
        }
        public string getmername(string str)
        {
            string merchant;
            string[] words = str.Split();
            merchant = words[0]+" "+words[1];
            return merchant;
        }
        public bool Isvat12(string str)
        {
            Regex r = new Regex(@"(12)[%xz]");
            Match m = r.Match(str);
            wrds = str.Split('\n', ' ');
            var tax = Array.IndexOf(wrds, "MOMS");
            var results = Array.IndexOf(wrds, "12.00");

            if (m.Success)
            {
                return true;
            }
            if (tax < results)
            {
                return true;
            }
            else
            {
                return false;

            }
        }
        public bool Isvat16(string str)
        {
            Regex r = new Regex(@"(16)[%xz]");
            Match m = r.Match(str);
            wrds = str.Split('\n', ' ');
            var tax = Array.IndexOf(wrds, "MOMS");
            var results = Array.IndexOf(wrds, "16.00");

            if (m.Success)
            {
                return true;
            }
            if (tax < results)
            {
                return true;
            }
            else
            {
                return false;

            }
        }
        public bool Isvat25(string str)
        {
            Regex r = new Regex(@"(25)[%xz]");
            Match m = r.Match(str);
            wrds = str.Split('\n', ' ');
            var tax = Array.IndexOf(wrds, "MOMS");
            var results = Array.IndexOf(wrds, "25.00");
            if (m.Success)
            {
                return true;
            }
            if (tax < results)
            {
                return true;
            }
            else
            {
                return false;

            }
        }
        public static Bitmap CropImage(Bitmap source, int x, int y, int width, int height)
        {
            Rectangle crop = new Rectangle(x, y, width, height);

            var bmp = new Bitmap(crop.Width, crop.Height);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            }
            return bmp;
        }
        public static int EditDistance(string original, string modified)
        {
            int len_orig = original.Length;
            int len_diff = modified.Length;

            var matrix = new int[len_orig + 1, len_diff + 1];
            for (int i = 0; i <= len_orig; i++)
                matrix[i, 0] = i;
            for (int j = 0; j <= len_diff; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= len_orig; i++)
            {
                for (int j = 1; j <= len_diff; j++)
                {
                    int cost = modified[j - 1] == original[i - 1] ? 0 : 1;
                    var vals = new int[] {
                matrix[i - 1, j] + 1,
                matrix[i, j - 1] + 1,
                matrix[i - 1, j - 1] + cost
            };
                    matrix[i, j] = vals.Min();
                    if (i > 1 && j > 1 && original[i - 1] == modified[j - 2] && original[i - 2] == modified[j - 1])
                        matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + cost);
                }
            }
            return matrix[len_orig, len_diff];
        }
        public Image<Bgr, byte> test(Image<Bgr, byte> image, String path,Rectangle rectang)
        {

            Image<Bgr, byte> image2 = new Image<Bgr, byte>(image.Size);
            PointF[] srcs = new PointF[4];
            srcs[0] = new PointF(rectang.X, rectang.Y);
            srcs[1] = new PointF((rectang.X+rectang.Width),(rectang.Y));
            srcs[2] = new PointF((rectang.X ),(rectang.Y + rectang.Height));
            srcs[3] = new PointF((rectang.X + rectang.Width),(rectang.Y +rectang.Height));
            float resultwidth = (srcs[0].X - srcs[1].X);
            float bottomwidth = (srcs[3].X-srcs[2].X);
            if(bottomwidth>resultwidth)
            { resultwidth = bottomwidth; }
            float resultheight = (srcs[2].Y - srcs[1].Y);
            float bottomheight = (srcs[3].Y-srcs[0].Y);
            if (bottomheight > resultheight)
            { resultheight = bottomheight; }

            PointF[] dsts = new PointF[4];
            dsts[0] = new PointF(rectang.X-10, rectang.Y-10);
            dsts[1] = new PointF((rectang.X + rectang.Width-10), (rectang.Y-10));
            dsts[2] = new PointF((rectang.X-10), (rectang.Y + rectang.Height-10));
            dsts[3] = new PointF((rectang.X + rectang.Width-10), (rectang.Y + rectang.Height-10));

            HomographyMatrix mywarpmat = CameraCalibration.GetPerspectiveTransform(srcs, dsts);
            Image<Bgr, byte> newImage = image.WarpPerspective(mywarpmat, Emgu.CV.CvEnum.INTER.CV_INTER_NN, Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, new Bgr(0, 0, 0));
            return newImage;
        }

    }

}