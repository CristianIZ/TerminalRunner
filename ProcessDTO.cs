using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UI
{
    public class ProcessDTO
    {
        public Process Process { get; set; }
        public string Name { get; set; }
        public ProcessType Type { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
