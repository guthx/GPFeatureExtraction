using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.Simple;
using Emgu.CV;
using Emgu.CV.Structure;
using GPFeatureExtraction.Data;
using GPFeatureExtraction.Image;
using LibSVMsharp;
using LibSVMsharp.Extensions;
using LibSVMsharp.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPFeatureExtraction.Problem
{
    [ECConfiguration("ec.problems.FeatureExtractionProblem")]
    public class FeatureExtractionProblem : GPProblem, ISimpleProblem
    {
        public ImageTransformer imageTransformer;
        public Image<Gray, Byte>[] currentImage;
        public Image<Gray, Byte>[] originalImage;
        public SVMParameter Parameter;
        public ProblemData Input;
        public string[] imageList;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            imageTransformer = new ImageTransformer(200, 200, @"F:\Gesty\bin", @"F:\Gesty\rescaled");
            currentImage = new Image<Gray, Byte>[4];
            originalImage = new Image<Gray, Byte>[4];
            for(int i = 0; i < 4; i++)
            {
                currentImage[i] = new Image<Gray, Byte>(200, 200);
                originalImage[i] = new Image<Gray, Byte>(200, 200);
            }
            Parameter = new SVMParameter();
            Parameter.Type = SVMType.C_SVC;
            Parameter.Kernel = SVMKernelType.POLY;
            base.Setup(state, paramBase);
            Input = (ProblemData)state.Parameters.GetInstanceForParameterEq(
                paramBase.Push(P_DATA), null, typeof(ProblemData));
            Input.Setup(state, paramBase.Push(P_DATA));
            // imageTransformer.RescaleAndSaveImages();
            // imageList = Directory.GetFiles(@"F:\Gesty\rescaled");
            imageTransformer.RescaleAndRotate();
            imageList = Directory.GetFiles(@"F:\Gesty\rotated");
        }

        public override object Clone()
        {
            var myobj = (FeatureExtractionProblem)(base.Clone());
            return myobj;
        }
        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (!ind.Evaluated)
            {
                int imageIndex;
                var SVMTrainData = new StreamWriter(@"F:\Gesty\features\traindata" + threadnum + ".txt");
                var SVMTestData = new StreamWriter(@"F:\Gesty\features\testdata" + threadnum + ".txt");
                for(imageIndex = 0; imageIndex < imageList.Length; imageIndex++)
                {
                    var image = new Image<Gray, Byte>(imageList[imageIndex]);
                    var line = new StringBuilder();
                    image.CopyTo(currentImage[threadnum]);
                    image.CopyTo(originalImage[threadnum]);
                    image.Dispose();
                    ((GPIndividual)ind).Trees[0].Child.Eval(state, threadnum, Input, Stack, ((GPIndividual)ind), this);
                    //int[] features = imageTransformer.GetSuperpixelFeatures(currentImage[threadnum]);
                    int[] features = ImageTransformer.GetSquareSuperpixelFeatures(currentImage[threadnum], 25);

                    line.Append((imageIndex / 213) + 1 + " ");
                    for(int i=1; i<=features.Length; i++)
                    {
                        line.Append(i + ":" + features[i-1] + " ");
                    }
                    var lineString = line.ToString().Trim();
                    if (imageIndex % 2 == 0)
                        SVMTrainData.WriteLine(lineString);
                    else
                        SVMTestData.WriteLine(lineString);
                }
                SVMTrainData.Close();
                SVMTestData.Close();
                var problem = SVMProblemHelper.Load(@"F:\Gesty\features\traindata" + threadnum + ".txt");
                var testProblem = SVMProblemHelper.Load(@"F:\Gesty\features\testdata" + threadnum + ".txt");
                var model = problem.Train(Parameter);
                double[] target = testProblem.Predict(model);
                double accuracy = testProblem.EvaluateClassificationProblem(target);
                var f = ((KozaFitness)ind.Fitness);
                f.SetStandardizedFitness(state, (float)(100 - accuracy));
                ind.Evaluated = true;
            }
        }
    }
}
