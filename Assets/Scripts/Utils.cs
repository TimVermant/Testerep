using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class Utils
    {
        public static bool IsInRange(int i, int min, int max)
        {
            return i >= min && i < max;
        }
    }
}