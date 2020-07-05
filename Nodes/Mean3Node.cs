using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using GPFeatureExtraction.Problem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPFeatureExtraction.Nodes
{
    [ECConfiguration("ec.nodes.Mean3Node")]
    public class Mean3Node : GPNode
    {
        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
        {
            var p = (FeatureExtractionProblem2)problem;
            Children[0].Eval(state, thread, input, stack, individual, problem);
            p.imageTransformer.TransformImage(p.currentImage[thread], Image.ImageTransformer.TransformationType.MEAN3);
        }

        public override string ToString()
        {
            return "MEAN 3x3";
        }
    }
}
