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
            GRADIENT,
            GAUSSIAN3,
            GAUSSIAN5,
            LAPLACE3,
            MEAN3,
            MEAN5
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

        public Image<Gray, Byte> RescaleImage(Image<Gray, Byte> image)
        {
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

        public static int[] GetSuperpixelFeatures(Image<Gray, Byte> image, int regionSize = 20, float ratio = 10.0f)
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

        public static int[] GetSquareSuperpixelFeatures(Image<Gray, Byte> image, int regionSize = 20)
        {
            var numOfPixels = (image.Width / regionSize) * (image.Height / regionSize);
            var labels = new int[image.Width, image.Height];
            var superpixelColors = new int[numOfPixels];
            var pixelCount = new int[numOfPixels];
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    labels[i, j] = 1 + i / regionSize + (image.Width / regionSize) * (j / regionSize);
                }
            }
            for (int i = 0; i < image.Width; i++)
                for (int j = 0; j < image.Height; j++)
                {
                    var label = labels[i, j];
                    superpixelColors[label - 1] += (int)image[i, j].Intensity;
                    pixelCount[label - 1] += 1;
                }
            for (int i = 0; i < superpixelColors.Length; i++)
            {
                if (pixelCount[i] != 0)
                    superpixelColors[i] /= pixelCount[i];
            }
            return superpixelColors;

        }

        public static void MaxPooling4(Image<Gray, Byte> image)
        {
            var pooledArray = new int[image.Width / 4, image.Height / 4];
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    var pixelVal = image[i, j].Intensity;
                    if (pooledArray[i / 4, j / 4] < pixelVal)
                        pooledArray[i / 4, j / 4] = (int)pixelVal;
                }
            }
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    image[i, j] = new Gray(pooledArray[i / 4, j / 4]);
                }
            }
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

        public void GetSuperpixelImages(Image<Gray, Byte> image, string dir, string imageName, int regionSize = 20, float ratio = 10.0f)
        {
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
            var maskedImage = new Image<Bgr, Byte>(image.Width, image.Height);
            // var boundaries = new Image<Bgr, Byte>(image.Width, image.Height);
            //image.CopyTo(maskedImage);
            CvInvoke.CvtColor(image, maskedImage, Emgu.CV.CvEnum.ColorConversion.Gray2Bgr);
            var mask = new Image<Gray, Byte>(image.Width, image.Height);
            var colorMask = new Image<Bgr, Byte>(image.Width, image.Height);
            colorMask = colorMask.Add(new Bgr(Color.Red));
            pixelator.GetLabelContourMask(mask);
            colorMask = colorMask.And(colorMask, mask);
            maskedImage = maskedImage.And(maskedImage, mask.Not());
            maskedImage = maskedImage.Add(colorMask);
            maskedImage.Save(dir + @"\" + imageName + "_borders.png");

            var superpixelImage = new Image<Gray, Byte>(image.Width, image.Height);
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    var pixelNum = labelsArray[i, j];
                    superpixelImage[i, j] = new Gray(superpixelColors[pixelNum]);
                }
            }

            superpixelImage.Save(dir + @"\" + imageName + "_meanColors.png");
        }

        public static void GetSquareSuperpixelImages(Image<Gray, Byte> image, string dir, string imageName, int regionSize = 20)
        {
            var numOfPixels = (image.Width / regionSize) * (image.Height / regionSize);
            var segmentedImage = new Image<Bgr, Byte>(image.Width, image.Height);
            CvInvoke.CvtColor(image, segmentedImage, Emgu.CV.CvEnum.ColorConversion.Gray2Bgr);
            var meanImage = new Image<Gray, Byte>(image.Width, image.Height);
            var labels = new int[image.Width, image.Height];
            var superpixelColors = new int[numOfPixels];
            var pixelCount = new int[numOfPixels];
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    labels[i, j] = 1 + i / regionSize + (image.Width / regionSize) * (j / regionSize);
                    if ((i % regionSize == 0 || j % regionSize == 0) && (i < image.Width - 1 && j < image.Height - 1))
                        segmentedImage[i, j] = new Bgr(Color.Red);
                }
            }
            for (int i = 0; i < image.Width; i++)
                for (int j = 0; j < image.Height; j++)
                {
                    var label = labels[i, j];
                    superpixelColors[label - 1] += (int)image[i, j].Intensity;
                    pixelCount[label - 1] += 1;
                }
            for (int i = 0; i < superpixelColors.Length; i++)
            {
                if (pixelCount[i] != 0)
                    superpixelColors[i] /= pixelCount[i];
            }

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    var pixelNum = labels[i, j];
                    meanImage[i, j] = new Gray(superpixelColors[pixelNum - 1]);
                }
            }
            meanImage.Save(dir + @"\" + imageName + "_meanColors.png");
            segmentedImage.Save(dir + @"\" + imageName + "_segmented.png");


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
                case TransformationType.GAUSSIAN3:
                    CvInvoke.GaussianBlur(image, image, new Size(3, 3), 0);
                    break;
                case TransformationType.GAUSSIAN5:
                    CvInvoke.GaussianBlur(image, image, new Size(5, 5), 0);
                    break;
                case TransformationType.LAPLACE3:
                    CvInvoke.Laplacian(image, image, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
                    break;
                case TransformationType.MEAN3:
                    CvInvoke.Blur(image, image, new Size(3, 3), new Point(-1, -1));
                    break;
                case TransformationType.MEAN5:
                    CvInvoke.Blur(image, image, new Size(5, 5), new Point(-1, -1));
                    break;
            }
        }

        public Image<Gray, Byte> RotateViaCenter(Image<Gray, Byte> image, double angle)
        {
            Image<Gray, Byte> rotatedImage = new Image<Gray, Byte>(image.Width, image.Height);
            var center = new PointF(image.Width / 2, image.Height / 2);
            var rotationMatrix = new Mat();
            CvInvoke.GetRotationMatrix2D(center, angle, 1.0, rotationMatrix);
            CvInvoke.WarpAffine(image, rotatedImage, rotationMatrix, image.Size);
            return rotatedImage;
        }

        public Image<Gray, Byte> NoiseImage(Image<Gray, Byte> image, double probability = 0.2)
        {
            var r = new Random();
            var noisedImage = new Image<Gray, Byte>(image.Width, image.Height);
            image.CopyTo(noisedImage);
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    var roll = r.NextDouble();
                    if (roll < probability)
                    {
                        if (noisedImage[i, j].Intensity == 0)
                        {
                            noisedImage[i, j] = new Gray(255);
                        }
                        else
                            noisedImage[i, j] = new Gray(0);
                    }
                }
            }
            return noisedImage;
        }

        public void RescaleAndRotate(double rotationProb = 0.5, double noiseProb = 0.1)
        {
            var r = new Random();
            var images = Directory.GetFiles(SourceDirectory);
            var target = @"F:\Gesty\rotated";
            int i = 0;
            foreach (string image in images)
            {
                string ext = Path.GetExtension(image);
                if (ext.Equals(".BMP") || ext.Equals(".JPG") || ext.Equals(".PNG") || ext.Equals(".bmp") || ext.Equals(".jpg") || ext.Equals(".png"))
                {
                    var originalImage = new Image<Gray, Byte>(image);
                    string fileName = System.IO.Path.GetFileName(image);
                    string targetPath = target + @"\" + i / 213 + fileName;
                    var rotationRoll = r.NextDouble();
                    if (rotationRoll < rotationProb)
                    {
                        var angle = r.NextDouble() * 50.0;
                        originalImage = RotateViaCenter(originalImage, angle);
                    }
                    var noiseRoll = r.NextDouble();
                    if (noiseRoll < noiseProb)
                    {
                        targetPath = target + @"\" + i / 213 + "n" + fileName;
                        originalImage = NoiseImage(originalImage, 0.05);
                    }
                    var rescaledImage = RescaleImage(originalImage);
                    rescaledImage.Save(targetPath);
                    i++;
                }
            }
        }






    }
}
