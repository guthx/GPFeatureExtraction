using BraneCloud.Evolution.EC;
using BraneCloud.Evolution.EC.Configuration;
using GPFeatureExtraction.Problem;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            ECActivator.AddSourceAssemblies(new[] { Assembly.GetAssembly(typeof(IEvolutionState)), Assembly.GetAssembly(typeof(FeatureExtractionProblem)) });
            IEvolutionState state = Evolve.Initialize(Evolve.LoadParameterDatabase(new[] { "-file", @"Parameters\koza.params" }), 2);
           
            state.Run(EvolutionState.C_STARTED_FRESH);

        }
    }
}
