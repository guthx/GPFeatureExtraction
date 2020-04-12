using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace GPFeatureExtraction.Image
{
    public class ImageTransformer
    {

        public enum TransformationType
        {
            ERODE,
            DILATE,
            OPEN,
            CLOSE,
            TOPHAT,
            BLACKHAT,
            GRADIENT
        }

        private int Width;
        private int Height;
        private string SourceDirectory;
        private string TargetDirectory;

        public ImageTransformer(int w, int h, string sd, string td)
        {
            Width = w;
            Height = h;
            SourceDirectory = sd;
            TargetDirectory = td;
        }

        public Image<Gray, Byte> RescaleImage(string imageName)
        {
            string imagePath = SourceDirectory + @"\" + imageName;
            var image = new Image<Gray, Byte>(imagePath);
            var contours = new VectorOfVectorOfPoint();
            var hierarchy = new Mat();
            CvInvoke.FindContours(image, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            double largestArea = 0;
            int index = -1;
            for (int i = 0; i < contours.Size; i++)
            {
                double a = CvInvoke.ContourArea(contours[i], false);
                if (a > largestArea)
                {
                    largestArea = a;
                    index = i;
                }
            }
            VectorOfPoint poly = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(contours[index], poly, 3, true);
            Rectangle rect = CvInvoke.BoundingRectangle(poly);
            image.ROI = rect;
            var croppedImage = new Image<Gray, Byte>(image.Width, image.Height);
            var resizedImage = new Image<Gray, Byte>(Width, Height);
            croppedImage = image.Copy();
            CvInvoke.Resize(croppedImage, resizedImage, new Size(Width, Height));
            return resizedImage;
        }

        public void RescaleAndSaveImages()
        {
            var images = Directory.GetFiles(SourceDirectory);
            foreach (string image in images)
            {
                string ext = Path.GetExtension(image);
                if (ext.Equals(".BMP") || ext.Equals(".JPG") || ext.Equals(".PNG") || ext.Equals(".bmp") || ext.Equals(".jpg") || ext.Equals(".png"))
                {
                    string fileName = System.IO.Path.GetFileName(image);
                    string targetPath = TargetDirectory + @"\" + fileName;
                    var rescaledImage = RescaleImage(fileName);
                    rescaledImage.Save(targetPath);
                }
            }
        }

        public int[] GetSuperpixelFeatures(Image<Gray, Byte> image, int regionSize = 20, float ratio = 10.0f)
        {
            var pixelator = new Emgu.CV.XImgproc.SupperpixelSLIC(image, Emgu.CV.XImgproc.SupperpixelSLIC.Algorithm.SLICO, regionSize, ratio);
           // var pixelator = new Emgu.CV.XImgproc.SuperpixelLSC(image, regionSize, ratio);
            pixelator.Iterate();
            var labels = new Mat();
            pixelator.GetLabels(labels);
            var labelsArray = new int[labels.Rows, labels.Cols];
            labelsArray = (int[,])labels.GetData();
            var superpixelColors = new int[pixelator.NumberOfSuperpixels];
            var pixelCount = new int[pixelator.NumberOfSuperpixels];
            var imageArray = new byte[image.Width, image.Height, 1];
            imageArray = image.Data;
            for (int i = 0; i < labels.Rows; i++)
                for (int j = 0; j < labels.Cols; j++)
                {
                    var label = labelsArray[i, j];
                    superpixelColors[label] += imageArray[i, j, 0];
                    pixelCount[label] += 1;
                }
            for (int i = 0; i < superpixelColors.Length; i++)
            {
                if (pixelCount[i] != 0)
                    superpixelColors[i] /= pixelCount[i];
            }
            return superpixelColors;
        }

        public int[] GetSuperpixelFeatures(string imageName, int regionSize = 20, float ratio = 10.0f)
        {
            var image = new Image<Gray, Byte>(SourceDirectory + @"\" + imageName);
            var pixelator = new Emgu.CV.XImgproc.SupperpixelSLIC(image, Emgu.CV.XImgproc.SupperpixelSLIC.Algorithm.SLICO, regionSize, ratio);
            pixelator.Iterate();
            var labels = new Mat();
            pixelator.GetLabels(labels);
            var labelsArray = new int[labels.Rows, labels.Cols];
            labelsArray = (int[,])labels.GetData();
            var superpixelColors = new int[pixelator.NumberOfSuperpixels];
            var pixelCount = new int[pixelator.NumberOfSuperpixels];
            var imageArray = new byte[image.Width, image.Height, 1];
            imageArray = image.Data;
            for (int i = 0; i < labels.Rows; i++)
                for (int j = 0; j < labels.Cols; j++)
                {
                    var label = labelsArray[i, j];
                    superpixelColors[label] += imageArray[i, j, 0];
                    pixelCount[label] += 1;
                }
            for (int i = 0; i < superpixelColors.Length; i++)
            {
                if (pixelCount[i] != 0)
                    superpixelColors[i] /= pixelCount[i];
            }
            return superpixelColors;
        }

        public void TransformImage(Image<Gray, Byte> image, TransformationType type)
        {
            var kernel = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));
            switch (type)
            {
                case TransformationType.ERODE:
                    CvInvoke.Erode(image, image, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                    break;
                case TransformationType.DILATE:
                    CvInvoke.Dilate(image, image, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                    break;
                case TransformationType.OPEN:
                    CvInvoke.MorphologyEx(image, image, Emgu.CV.CvEnum.MorphOp.Open, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                    break;
                case TransformationType.CLOSE:
                    CvInvoke.MorphologyEx(image, image, Emgu.CV.CvEnum.MorphOp.Close, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                    break;
                case TransformationType.TOPHAT:
                    CvInvoke.MorphologyEx(image, image, Emgu.CV.CvEnum.MorphOp.Tophat, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                    break;
                case TransformationType.BLACKHAT:
                    CvInvoke.MorphologyEx(image, image, Emgu.CV.CvEnum.MorphOp.Blackhat, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                    break;
                case TransformationType.GRADIENT:
                    CvInvoke.MorphologyEx(image, image, Emgu.CV.CvEnum.MorphOp.Gradient, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
                    break;
            }
        }


    }
}
