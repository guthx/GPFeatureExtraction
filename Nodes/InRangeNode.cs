using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using Emgu.CV.Structure;
using GPFeatureExtraction.Data;
using GPFeatureExtraction.Problem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPFeatureExtraction.Nodes
{
    [ECConfiguration("ec.nodes.InRangeNode")]
    public class InRangeNode : GPNode
    {
        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
        {
            var p = (FeatureExtractionProblem2)problem;
            Children[0].Eval(state, thread, input, stack, individual, problem);
            Children[1].Eval(state, thread, input, stack, individual, problem);
            int r1 = ((ProblemData)input).range + 60;
            Children[2].Eval(state, thread, input, stack, individual, problem);
            int r2 = ((ProblemData)input).range + 160;

            for (int w = 0; w < 64; w++)
                for (int h = 0; h < 64; h++)
                {
                    if (p.currentImage[thread][w, h].Intensity < r1 || p.currentImage[thread][w, h].Intensity > r2)
                    {
                        p.currentImage[thread][w, h] = new Gray(0);
                    }
                }
        }

        public override string ToString()
        {
            return "IN_RANGE";
        }
    }
}
