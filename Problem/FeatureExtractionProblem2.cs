using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.GP.Koza;
using BraneCloud.Evolution.EC.Simple;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.ML;
using GPFeatureExtraction.Data;
using GPFeatureExtraction.Image;
//using LibSVMsharp;
//using LibSVMsharp.Extensions;
//using LibSVMsharp.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GPFeatureExtraction.Problem
{
    [ECConfiguration("ec.problems.FeatureExtractionProblem2")]
    public class FeatureExtractionProblem2 : GPProblem, ISimpleProblem
    {
        public Image<Gray, Byte>[] currentImage;
        public Image<Gray, Byte>[] originalImage;
        // public SVMParameter Parameter;
        public ProblemData Input;
        public int NumOfImagesPerCategory;
        public int SuperpixelSize;
        public int NumOfFeatures;
        public int NumOfClasses;
        public string[] CategoryDirs;
        public ImageTransformer imageTransformer;
        public double[,] confMat;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            confMat = new double[10, 10];
            imageTransformer = new ImageTransformer(0, 0, "", "");
            NumOfImagesPerCategory = 100;
            SuperpixelSize = 8;
            NumOfClasses = 10;
            NumOfFeatures = 64 * 64 / (SuperpixelSize * SuperpixelSize);
            CategoryDirs = Directory.EnumerateDirectories(@"F:\Gesty\problem2\grayscale").ToArray();
            currentImage = new Image<Gray, Byte>[4];
            originalImage = new Image<Gray, Byte>[4];
            for (int i = 0; i < 4; i++)
            {
                currentImage[i] = new Image<Gray, Byte>(64, 64);
                originalImage[i] = new Image<Gray, Byte>(64, 64);
            }
            // Parameter = new SVMParameter();
            //Parameter.Type = SVMType.C_SVC;
            //Parameter.Kernel = SVMKernelType.POLY;
            base.Setup(state, paramBase);
            Input = (ProblemData)state.Parameters.GetInstanceForParameterEq(
                paramBase.Push(P_DATA), null, typeof(ProblemData));
            Input.Setup(state, paramBase.Push(P_DATA));
            // imageTransformer.RescaleAndSaveImages();
            // imageList = Directory.GetFiles(@"F:\Gesty\rescaled");
        }

        public override object Clone()
        {
            var myobj = (FeatureExtractionProblem2)(base.Clone());
            return myobj;
        }
        public void Evaluate(IEvolutionState state, Individual ind, int subpop, int threadnum)
        {
            if (!ind.Evaluated)
            {
                int counter = 0;
                var features = new int[5, NumOfImagesPerCategory * NumOfClasses, NumOfFeatures];
                var labels = new int[5, NumOfImagesPerCategory * NumOfClasses];

                for (int i = 0; i < 5; i++)
                {
                    counter = 0;
                    var categoryDir = CategoryDirs[i];
                    var subcategoryDirs = Directory.EnumerateDirectories(categoryDir).ToArray();
                    for (int j = 0; j < NumOfClasses; j++)
                    {
                        var subcategoryDir = subcategoryDirs[j];
                        var images = Directory.GetFiles(subcategoryDir);
                        for (int k = 0; k < NumOfImagesPerCategory; k++)
                        {
                            var tempImage = new Image<Gray, Byte>(images[k]);
                         
                            tempImage.CopyTo(currentImage[threadnum]);
                            tempImage.CopyTo(originalImage[threadnum]);
                            tempImage.Dispose();
                          
                            ((GPIndividual)ind).Trees[0].Child.Eval(state, threadnum, Input, Stack, ((GPIndividual)ind), this);
                        
                            int[] imageFeatures = ImageTransformer.GetSquareSuperpixelFeatures(currentImage[threadnum], SuperpixelSize);
                            for (int x = 0; x < imageFeatures.Length; x++)
                            {
                                features[i, counter, x] = imageFeatures[x];
                                labels[i, counter] = j + 1;
                            }
                            counter++;
                        }
                    }
                }
                /*
                var trainDataFile = new StreamWriter(@"F:\Gesty\problem2\features\traindata" + threadnum + ".txt");
                var testDataFile = new StreamWriter(@"F:\Gesty\problem2\features\testdata" + threadnum + ".txt");

                for(int i=0; i<4; i++)
                {
                    for(int j=0; j<1000; j++)
                    {
                        var line = new StringBuilder();
                        line.Append(labels[i, j].ToString() + " ");
                        for (int k=0; k<NumOfFeatures; k++)
                        {
                            line.Append((k + 1).ToString() + ":" + features[i, j, k].ToString() + " ");
                        }
                        trainDataFile.WriteLine(line.ToString().Trim());
                    }
                }
                for (int j = 0; j < 1000; j++)
                {
                    var line = new StringBuilder();
                    line.Append(labels[4, j].ToString() + " ");
                    for (int k = 0; k < NumOfFeatures; k++)
                    {
                        line.Append((k + 1).ToString() + ":" + features[4, j, k].ToString() + " ");
                    }
                    testDataFile.WriteLine(line.ToString().Trim());
                }
                trainDataFile.Close();
                testDataFile.Close();
                */
                var confMatI = new double[10, 10];
                double accuracy = 0;
                for (int x = 0; x < 5; x++)
                {
                    var trainData = new Matrix<float>(NumOfImagesPerCategory * NumOfClasses * 4, NumOfFeatures);
                    var trainClasses = new Matrix<int>(NumOfImagesPerCategory * NumOfClasses * 4, 1);
                    var testData = new Matrix<float>(NumOfImagesPerCategory * NumOfClasses, NumOfFeatures);
                    var testClasses = new Matrix<int>(NumOfImagesPerCategory * NumOfClasses, 1);
                    //trainData


                    int imageCount = 0;
                    for (int i = 0; i < 5; i++)
                    {
                        if (i != x)
                        {
                            for (int j = 0; j < NumOfImagesPerCategory * NumOfClasses; j++)
                            {
                                for (int k = 0; k < NumOfFeatures; k++)
                                {
                                    trainData[imageCount, k] = features[i, j, k];
                                    trainClasses[imageCount, 0] = labels[i, j];
                                }
                                imageCount++;
                            }
                            
                        } else
                        {
                            for (int j = 0; j < NumOfImagesPerCategory * NumOfClasses; j++)
                            {
                                for (int k = 0; k < NumOfFeatures; k++)
                                {
                                    testData[j, k] = features[i, j, k];
                                    testClasses[j, 0] = labels[i, j];
                                }

                            }
                        }        
                    }


                    Emgu.CV.ML.SVM model = new Emgu.CV.ML.SVM();


                    var predictions = new Matrix<float>(NumOfImagesPerCategory * NumOfClasses, 1);
                    var trainData2 = new TrainData(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainClasses);
                    model.Type = Emgu.CV.ML.SVM.SvmType.CSvc;
                    model.SetKernel(Emgu.CV.ML.SVM.SvmKernelType.Poly);
                    model.TermCriteria = new MCvTermCriteria(10000, 0.001);
                    model.Degree = 3;
                    model.Gamma = 0.001;
                    model.Coef0 = 0;
                    model.C = 1000;
                    model.Nu = 0.5;
                    model.P = 0.1;
                    model.Train(trainData2);
                    model.Predict(testData, predictions);
                    // var predictionsArray = (float[,])predictions.GetData();
                    int correctPredictions = 0;
                    for (int i = 0; i < predictions.Rows; i++)
                    {
                        if ((int)predictions[i, 0] == testClasses[i, 0])
                            correctPredictions++;
                        var predictedLabel = (int)predictions[i, 0];
                        var trueLabel = testClasses[i, 0];
                        confMatI[predictedLabel - 1, trueLabel - 1]++;
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            confMat[i, j] = (confMatI[i, j] / 500) * 100;
                        }
                    }
                    if (correctPredictions > 0)
                        accuracy += 100 * ((double)correctPredictions / (double)predictions.Rows);

                }
                
                /*
                for(int i=0; i<NumOfImagesPerCategory*NumOfClasses*4; i++)
                {
                    for(int j=0; j<NumOfFeatures; j++)
                    {
                        //trainData[i, j] = ((trainData[i, j] - 0) / (255 - 0)) * (1 + 1) - 1;
                    }
                    trainClasses[i, 0] = ((trainClasses[i, 0] - 1) / (NumOfClasses - 1)) * (1 + 1) - 1;
                }
                */
                
                //testData
                /*
                for (int j = 0; j < NumOfImagesPerCategory * NumOfClasses; j++)
                {
                    var line = new StringBuilder();
                    line.Append(labels[4, j] + " ");
                    for (int k = 0; k < NumOfFeatures; k++)
                    {
                        line.Append(k + 1 + ":" + features[4, j, k] + " ");
                    }
                    testData.WriteLine(line.ToString().Trim());
                }
                
                trainData.Close();
                testData.Close();
                */
                
                //predictions.Dispose();
                
                /*
                var netData = new SharpLearning.Containers.Matrices.F64Matrix(NumOfImagesPerCategory * NumOfClasses * 4, NumOfFeatures);
                var netTargets = new double[NumOfImagesPerCategory * NumOfClasses * 4];
                int imageCount = 0;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < NumOfImagesPerCategory * NumOfClasses; j++)
                    {
                        for (int k = 0; k < NumOfFeatures; k++)
                        {
                            netData[imageCount, k] = features[i, j, k];
                            netTargets[imageCount] = labels[i, j];
                        }
                        imageCount++;
                    }
                }
                */
                /*
                var CVNeuralNet = new Emgu.CV.ML.ANN_MLP();
                
                CVNeuralNet.TermCriteria = new MCvTermCriteria(10000, 0.001);
                var layerSizes = new Matrix<int>(new int[4] { NumOfFeatures, NumOfFeatures * 10, NumOfFeatures*5, 1 });
                CVNeuralNet.SetLayerSizes(layerSizes);
                CVNeuralNet.SetTrainMethod(ANN_MLP.AnnMlpTrainMethod.Rprop);
                CVNeuralNet.SetActivationFunction(ANN_MLP.AnnMlpActivationFunction.SigmoidSym);
                CVNeuralNet.BackpropMomentumScale = 0.01;
                CVNeuralNet.BackpropWeightScale = 0.2;
                var trainData2 = new TrainData(trainData, Emgu.CV.ML.MlEnum.DataLayoutType.RowSample, trainClasses);
                var predictions = new Matrix<float>(NumOfImagesPerCategory * NumOfClasses*4, 1);
                CVNeuralNet.Train(trainData2);
                CVNeuralNet.Predict(trainData, predictions);
                */
                /*
                var net = new NeuralNet();
                net.Add(new InputLayer(NumOfFeatures));
                net.Add(new DropoutLayer(0.5));
                net.Add(new DenseLayer(NumOfFeatures * 4, SharpLearning.Neural.Activations.Activation.Sigmoid));
                net.Add(new DenseLayer(NumOfFeatures * 4, SharpLearning.Neural.Activations.Activation.Sigmoid));
                net.Add(new DropoutLayer(0.5));
                net.Add(new SoftMaxLayer(NumOfClasses));
                var learner = new ClassificationNeuralNetLearner(net, new SquareLoss());
                var model = learner.Learn(netData, netTargets);
                var predictions = model.Predict(netData);
                int correctPredictions = 0;
                for (int i = 0; i < predictions.Length; i++)
                {
                    if ((int)predictions[i] == netTargets[i])
                        correctPredictions++;
                }
                if (correctPredictions > 0)
                    accuracy = 100 * ((double)correctPredictions / (double)predictions.Length);
                    */
                /*
                var problem = SVMProblemHelper.Load(@"F:\Gesty\problem2\features\traindata" + threadnum + ".txt");
                var testProblem = SVMProblemHelper.Load(@"F:\Gesty\problem2\features\testdata" + threadnum + ".txt");
                var model = problem.Train(Parameter);
                double[] target = testProblem.Predict(model);
                double accuracy = testProblem.EvaluateClassificationProblem(target);
                */
                var f = ((KozaFitness)ind.Fitness);
                f.SetStandardizedFitness(state, (float)(100 - (accuracy/5)));
                ind.Evaluated = true;

                var transFeatures = new StreamWriter(@"F:\Gesty\testy\transFeatures.csv");
                for (int i1 = 0; i1<1000; i1++)
                {
                    
                    for (int i2 = 0; i2<5; i2++)
                    {
                        var line = new StringBuilder();
                        line.Append(labels[i2, i1].ToString() + ',');
                        for(int i3 = 0; i3 < 64; i3++)
                        {
                            line.Append(features[i2, i1, i3].ToString() + ',');
                        }
                        transFeatures.WriteLine(line.ToString().Trim(','));
                    }
                }
                transFeatures.Close();
            }
        }
    }
}
