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
    [ECConfiguration("ec.nodes.CloseNode")]
    public class CloseNode : GPNode
    {
        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
        {
            var p = (FeatureExtractionProblem)problem;
            Children[0].Eval(state, thread, input, stack, individual, problem);
            p.imageTransformer.TransformImage(p.currentImage, Image.ImageTransformer.TransformationType.CLOSE);
        }

        public override string ToString()
        {
            return "CLOSE";
        }
    }
}
