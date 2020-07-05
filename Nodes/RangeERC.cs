using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.Util;
using GPFeatureExtraction.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPFeatureExtraction.Nodes
{
    [ECConfiguration("ec.nodes.RangeERC")]
    public class RangeERC : ERC
    {
        public int value;

        public override string ToStringForHumans()
        {
            return value.ToString();
        }

        public override string ToString()
        {
            return "ERC(range)";
        }
        public override string Encode()
        {
            return Code.Encode(value);
        }

        public override bool Decode(DecodeReturn dret)
        {
            int pos = dret.Pos;
            String data = dret.Data;
            Code.Decode(dret);
            if (dret.Type != DecodeReturn.T_INT)
            {
                dret.Data = data;
                dret.Pos = pos;
                return false;
            }
            value = (int)dret.L;
            return true;
        }

        public override void Eval(IEvolutionState state, int thread, GPData input, ADFStack stack, GPIndividual individual, IProblem problem)
        {
            ((ProblemData)input).range = value;
        }

        public override bool NodeEquals(GPNode node)
        {
            return (node.GetType() == this.GetType() && ((RangeERC)node).value == value);
        }

        public override void ResetNode(IEvolutionState state, int thread)
        {
            value = state.Random[thread].NextInt(20) + 1;
        }
    }
}
