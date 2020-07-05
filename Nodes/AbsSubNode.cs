using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using Emgu.CV;
using Emgu.CV.Structure;
using GPFeatureExtraction.Problem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPFeatureExtraction.Nodes
{
    [ECConfiguration("ec.nodes.AbsSubNode")]
    public class AbsSubNode : GPNode
    {
        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
        {
            var p = (FeatureExtractionProblem2)problem;
            var c0 = p.currentImage[thread].Copy();
            Children[0].Eval(state, thread, input, stack, individual, problem);
            Image<Gray, float> c1 = new Image<Gray, float>(64, 64);
            c1 = p.currentImage[thread].Convert<Gray, float>();
            c0.CopyTo(p.currentImage[thread]);
            Children[1].Eval(state, thread, input, stack, individual, problem);
            var c2 = new Image<Gray, float>(64, 64);
            c2 = p.currentImage[thread].Convert<Gray, float>();
            p.currentImage[thread] = c2.AbsDiff(c1).Convert<Gray, Byte>();
            //CvInvoke.AbsDiff(p.currentImage[thread], c1, p.currentImage[thread]);
        }

        public override string ToString()
        {
            return "ABS_SUB";
        }
    }
}
