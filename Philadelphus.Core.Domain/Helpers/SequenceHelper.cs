using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers
{
    public static class SequenceHelper
    {
        public static int Interval { get; } = 10;
        public static int GetLastSequence(IEnumerable<int> sequences)
        {
            return (int)Math.Round(value: (double)sequences.Max() / 10, MidpointRounding.ToPositiveInfinity) * 10 + Interval;
        }
    }
}
