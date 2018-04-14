using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicDriverNET
{
    public interface IDriver
    {
        Dictionary<String, float> drive(Dictionary<String, float> values);
    }
}
