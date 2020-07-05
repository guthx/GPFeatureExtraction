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
    [ECConfiguration("ec.nodes.OrNode")]
    public class OrNode : GPNode
    {
        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
        {
            var p = (FeatureExtractionProblem)problem;
            var c0 = p.currentImage[thread].Copy();
            Children[0].Eval(state, thread, input, stack, individual, problem);
            var c1 = p.currentImage[thread].Copy();
            c0.CopyTo(p.currentImage[thread]);
            Children[1].Eval(state, thread, input, stack, individual, problem);
            p.currentImage[thread] = p.currentImage[thread].Or(c1);
        }

        public override string ToString()
        {
            return "OR";
        }
    }
}
