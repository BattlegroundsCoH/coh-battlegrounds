using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Util;

public static class Numerics {

    public const double RAD2DEG = 57.29578;

    public static double Square(this double x) => x * x;

    public static float Square(this float x) => x * x;

    public static int Square(this int x) => x * x;

}
