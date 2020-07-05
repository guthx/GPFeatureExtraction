using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.Util;
using GPFeatureExtraction.Problem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPFeatureExtraction.Nodes
{
    [ECConfiguration("ec.nodes.MorphNode")]
    public class MorphNode : ERC
    {
        public int morphType;
        public override void CheckConstraints(IEvolutionState state, int tree, GPIndividual typicalIndividual, IParameter individualBase)
        {
            //base.CheckConstraints(state, tree, typicalIndividual, individualBase);
        }
        public override string ToString()
        {
            return ((Image.ImageTransformer.TransformationType)morphType).ToString();
        }
        public override string ToStringForHumans()
        {
            return ((Image.ImageTransformer.TransformationType)morphType).ToString();
        }
        public override string Encode()
        {
            return Code.Encode(morphType);
        }
        public override bool Decode(DecodeReturn dret)
        {
            int pos = dret.Pos;
            String data = dret.Data;
            Code.Decode(dret);
            if(dret.Type != DecodeReturn.T_INT)
            {
                dret.Data = data;
                dret.Pos = pos;
                return false;
            }
            morphType = (int)dret.L;
            return true;
        }

        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
        {
            var image = ((FeatureExtractionProblem)problem).currentImage[thread];
            var transformer = ((FeatureExtractionProblem)problem).imageTransformer;
            transformer.TransformImage(image, (Image.ImageTransformer.TransformationType)morphType);
            Children[0].Eval(state, thread, input, stack, individual, problem);
        }

        public override void MutateERC(IEvolutionState state, int thread)
        {
            morphType = state.Random[thread].NextInt(7);
        }

        public override bool NodeEquals(GPNode node)
        {
            return (node.GetType() == this.GetType() && ((MorphNode)node).morphType == morphType);
        }

        public override void ResetNode(IEvolutionState state, int thread)
        {
            morphType = state.Random[thread].NextInt(7);
        }
    }
}
