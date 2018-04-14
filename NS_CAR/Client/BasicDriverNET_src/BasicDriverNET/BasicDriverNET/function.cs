using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicDriverNET
{

    public static class Function
    {
        public static double Binary(this double value, Fce function)
        {
            switch (function)
            {
                case Fce.Binary:
                    {
                        return value > 0 ? 1 : 0;
                    }

                case Fce.Logistic:
                    {
                        return 1 / (1 + Math.Pow(Math.E, -value));
                    }
                default:
                    return value;
            }

        }

    }
}
