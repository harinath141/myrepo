using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using AForge.Imaging.Filters;

namespace MserTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Image<Bgr, Byte> img = new Image<Bgr, Byte>(@"D:\HARI\PanImages\OS07670104.jpg");
            MSERDetector mser = new MSERDetector();
            Image<Gray, byte> gray = img.Convert<Gray, byte>();
            BradleyLocalThresholding br = new BradleyLocalThresholding();
            Bitmap newth = br.Apply(gray.ToBitmap());
            //newth.Save(@"D:\HARI\PanImages\OS07670104_thresholded.png");
            Image<Bgr, byte> gray_image = img.Copy();
            Image<Gray, byte> newgray = new Image<Gray, byte>(newth);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfRect rects = new VectorOfRect();
            mser.DetectRegions(newgray, contours, rects);
            mser.Dispose();
            // CvInvoke.GroupRectangles(rects,2,0.5);
            List<Rectangle> rct = new List<Rectangle>(rects.Size);
            rct = rects.ToArray().ToList();
            rct = rct.OrderBy(x => x.X).ToList();
           // Bitmap bmp=newbitmap(gray_image.ToBitmap());
           
            for (int i = 0; i < rct.Count; i++)
            {
               
                if (contours[i].Size > 1200 && rct[i].Height > 15 &&rct[i].Width>10)
                {
                    

                    //Draw rectangles
                    //CvInvoke.Rectangle(gray_image, rct[i], new MCvScalar(0, 0, 255), 1);
                   
                }
            }
            gray_image.Save(@"D:\HARI\PanImages\OS07670104_new.png");
            #region disposable objects
            rects.Dispose();
            contours.Dispose();
            rects.Dispose();
            img.Dispose();
            gray.Dispose();
            gray.Dispose();
            #endregion
        }
        public static void CopyRegionIntoImage(Bitmap srcBitmap, Rectangle srcRegion, ref Bitmap destBitmap, Rectangle destRegion)
        {
            using (Graphics grD = Graphics.FromImage(destBitmap))
            {
                grD.DrawImage(srcBitmap, destRegion, srcRegion, GraphicsUnit.Pixel);
               
            }
        }

        public Bitmap newbitmap(Bitmap DrawArea )
        {
            Bitmap blank = new Bitmap(DrawArea.Width, DrawArea.Height);
            Graphics g = Graphics.FromImage(blank);
            g.Clear(Color.White);
            g.DrawImage(blank, 0, 0, DrawArea.Width, DrawArea.Height);
            return blank;
        }

        public Rectangle mergeRectangle(Rectangle a, Rectangle b)
        {
            int x_dist=a.X - b.X;
            Rectangle r;
            if (x_dist > 2)
            {
                r = MergeRectangles(a, b);
            }
            else
            {
                r = b;
            }
            return r;
        }

        private Rectangle MergeRectangles( Rectangle a, Rectangle b)
        {
            int a;
        }
    }
}


