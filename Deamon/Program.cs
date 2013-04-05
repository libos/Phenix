using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
namespace PhenixDeamon
{
    static class Program
    {
        static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new ProcessDeamon() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
