using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Client
{
    public class BiQuadFilterExt
    {
        public static BiQuadFilter Build(double a0, double a1, double a2, double b0, double b1, double b2)
        {
            Type[] paramTypes = new Type[] { typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double) };
            object[] paramValues = new object[] { a0, a1, a2, b0, b1, b2 };
            return Construct<BiQuadFilter>(paramTypes, paramValues);
        }

        public static T Construct<T>(Type[] paramTypes, object[] paramValues)
        {
            Type t = typeof(T);

            ConstructorInfo ci = t.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, paramTypes, null);

            return (T)ci.Invoke(paramValues);
        }
    }
}
