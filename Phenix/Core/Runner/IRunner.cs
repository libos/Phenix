using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phenix.Core.Runner
{
    interface IRunner
    {
        void Run(Task aTask);
        void taskCallback(); 
    }
}
