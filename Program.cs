using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.Simple;
using Emgu.CV;
using Emgu.CV.Structure;
using GPFeatureExtraction.Image;
using GPFeatureExtraction.Problem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GPFeatureExtraction
{
    class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            ECActivator.AddSourceAssemblies(new[] { Assembly.GetAssembly(typeof(IEvolutionState)), Assembly.GetAssembly(typeof(FeatureExtractionProblem2)) });
            IEvolutionState state = Evolve.Initialize(Evolve.LoadParameterDatabase(new[] { "-file", @"Parameters\problem2.params" }), 11);
            state.Run(EvolutionState.C_STARTED_FRESH);
            var problem = (FeatureExtractionProblem2)state.Evaluator.p_problem;
           // var treeLog1 = state.Output.AddLog(@"F:\Gesty\logs\gvtree_gen1.txt");
            //var treeLog2 = state.Output.AddLog(@"F:\Gesty\logs\gvtree_gen10.txt");
            //var ecjGraph = state.Output.AddLog(@"F:\Gesty\logs\ecjGraph.txt");
            //var bestOfRun = ((SimpleStatistics)state.Statistics).BestOfRun[0];
            //((GPIndividual)bestOfRun).Trees[0].PrintTree(state, ecjGraph);
            //((GPIndividual)bestOfRun).Trees[0].PrintStyle = GPTree.PRINT_STYLE_DOT;
            //((GPIndividual)bestOfRun).Trees[0].PrintTreeForHumans(state, treeLog);
            
            var input = problem.Input;
            var stack = problem.Stack;
            var stats = (SimpleStatistics)state.Statistics;
            var bestOfRun = (GPIndividual)stats.BestOfRun[0];
            var treeReader = new StreamReader(@"F:\Gesty\logs\ecjGraph.txt");
            bestOfRun.Trees[0].ReadTree(state, treeReader);
            bestOfRun.Evaluated = false;
            

            var imageList = new string[] {
                @"F:\Gesty\problem2\grayscale\A\a\color_0_0002.png",
                @"F:\Gesty\problem2\grayscale\A\b\color_1_0002.png",
                @"F:\Gesty\problem2\grayscale\A\c\color_2_0002.png",
                @"F:\Gesty\problem2\grayscale\A\f\color_5_0002.png",
                @"F:\Gesty\problem2\grayscale\A\h\color_7_0002.png"
            };
            int i = 0;
            foreach(string image in imageList)
            {
                var dir = @"F:\Gesty\testy\examples";
                var tempImg = new Image<Gray, Byte>(image);
                tempImg.CopyTo(problem.currentImage[0]);
                tempImg.CopyTo(problem.originalImage[0]);
                bestOfRun.Trees[0].Child.Eval(state, 0, input, stack, bestOfRun, problem);
                tempImg.Save(dir + @"\" + i + ".png");
                problem.currentImage[0].Save(dir + @"\" + i + "_trans.png");
                ImageTransformer.GetSquareSuperpixelImages(problem.currentImage[0], dir, i.ToString(), 8);
                i++;
            }
            problem.Evaluate(state, bestOfRun, 0, 0);
            Console.WriteLine(bestOfRun.Fitness);
            /*
            var confMatFile = new StreamWriter(@"F:\Gesty\testy\confmatBest.csv");
            for (int x = 0; x < 10; x++)
            {
                var line = new StringBuilder();
                for (int j = 0; j < 10; j++)
                {
                    line.Append(problem.confMat[x, j].ToString() + ';');
                }
                confMatFile.WriteLine(line.ToString().Trim(';'));
            }
            confMatFile.Close();
            */
            
            // Console.WriteLine(bestOfRun.Fitness);

            /*
            var tempImg = new Image<Gray, Byte>(@"F:\Gesty\problem2\grayscale\A\a\color_0_0002.png");
            tempImg.CopyTo(problem.currentImage[0]);
            tempImg.CopyTo(problem.originalImage[0]);
            ((GPIndividual)bestOfRun).Trees[0].Child.Eval(state, 0, input, stack, bestOfRun, problem);
            problem.currentImage[0].Save(@"F:\Gesty\testy\transformed.png");
            */

            /*
            var gesty = new string[8] { "piesc", "dlon", "1p", "2p", "3p", "4p", "5p", "kciuk" };
            var imageIndex = 0;
            foreach (string gest in gesty)
            {
                var dir = @"F:\Gesty\superpixel\200x200\" + gest;

                //oryginalny
                var tempImg = new Image<Gray, Byte>(problem.imageList[imageIndex]);
                tempImg.CopyTo(problem.currentImage[0]);
                tempImg.CopyTo(problem.originalImage[0]);
                tempImg.Dispose();
                bestOfRun.Trees[0].Child.Eval(state, 0, input, stack, bestOfRun, problem);
                problem.imageTransformer.GetSuperpixelImages(problem.currentImage[0], dir, "oryginalny");
                imageIndex += 101;

                //obrocony
                var tempImg2 = new Image<Gray, Byte>(problem.imageList[imageIndex]);
                tempImg2.CopyTo(problem.currentImage[0]);
                tempImg2.CopyTo(problem.originalImage[0]);
                tempImg2.Dispose();
                bestOfRun.Trees[0].Child.Eval(state, 0, input, stack, bestOfRun, problem);
                problem.imageTransformer.GetSuperpixelImages(problem.currentImage[0], dir, "obrocony");
                imageIndex += 101;

                //zaklocony
                var tempImg3 = new Image<Gray, Byte>(problem.imageList[imageIndex]);
                tempImg3.CopyTo(problem.currentImage[0]);
                tempImg3.CopyTo(problem.originalImage[0]);
                tempImg3.Dispose();
                bestOfRun.Trees[0].Child.Eval(state, 0, input, stack, bestOfRun, problem);
                problem.imageTransformer.GetSuperpixelImages(problem.currentImage[0], dir, "zaklocony");
                imageIndex += 11;
            }
            */
            Console.ReadKey();

        }
    }
}
