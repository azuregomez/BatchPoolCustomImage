using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchApp
{
    class Program
    {
        static void Main(string[] args)
        {
            BatchAgent batchAgent = new BatchAgent(BatchConfig.FromAppSettings());
            batchAgent.Execute();
        }

    }
}
