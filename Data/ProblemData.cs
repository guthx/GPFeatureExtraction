using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPFeatureExtraction.Data
{
    [ECConfiguration("ec.data.FeatureExtractionData")]
    public class ProblemData : GPData
    {
        public override void CopyTo(GPData gpd) { }
    }
}
