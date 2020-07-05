using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using GPFeatureExtraction.Image;
using GPFeatureExtraction.Problem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPFeatureExtraction.Nodes
{
    [ECConfiguration("ec.nodes.MaxPooling4Node")]
    public class MaxPooling4Node : GPNode
    {
        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
        {
            var p = (FeatureExtractionProblem2)problem;
            Children[0].Eval(state, thread, input, stack, individual, problem);
            ImageTransformer.MaxPooling4(p.currentImage[thread]);
        }

        public override string ToString()
        {
            return "MAX_POOLING_4";
        }
    }
}
