using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Util;

namespace DeepMineMod
{
    class Utils
    {
        public static float BezierInterp(float p0, float p1, float p2, float p3, float t)
        {
            float r = 1f - t;
            float f0 = r * r * r;
            float f1 = r * r * t * 3;
            float f2 = r * t * t * 3;
            float f3 = t * t * t;
            return f0 * p0 + f1 * p1 + f2 * p2 + f3 * p3;
        }

        public class WalkerMethod<T>
        {
            /*
             * Translated from Ruby implementation at https://github.com/cantino/walker_method
             */
            private readonly T[] Items;
            private readonly float[] Weights;
            private readonly float TotalWeights;
            private readonly List<float> Probabilities;
            private readonly int Length;
            private readonly List<int> Inx;
            private readonly List<int> Short;
            private readonly List<int> Long;
            public static WalkerMethod<T> Ctor(IEnumerable<T> items, Func<T, float> getWeight)
            {
                var weights = items.Select(getWeight);
                return new WalkerMethod<T>(items, weights);
            }
            public WalkerMethod(IEnumerable<T> items, IEnumerable<float> weights)
            {
                Items = items.ToArray();
                Weights = weights.ToArray();
                TotalWeights = Weights.Sum();
                Length = items.Count();
                Probabilities = new List<float>();
                Inx = new List<int>();
                foreach (var w in Weights)
                {
                    Inx.Add(-1);
                    Probabilities.Add(w * Length / TotalWeights);
                }
                Short = new List<int>();
                Long = new List<int>();
                int i = 0;
                foreach (var p in Probabilities)
                {
                    if (p < 1)
                        Short.Add(i);
                    else
                        Long.Add(i);
                    i++;
                }
                while (Short.Count() > 0 && Long.Count() > 0)
                {
                    int j = Short.Pop();
                    int k = Long.Last();
                    Inx[j] = k;
                    Probabilities[k] -= (1 - Probabilities[j]);
                    if (Probabilities[k] < 1)
                    {
                        Short.Add(k);
                        Long.Pop();
                    }
                }
            }
            public T Random(System.Random random)
            {
                var u = random.NextDouble();
                var j = (int)(random.NextDouble() * Length);
                if (u <= Probabilities[j])
                    return Items[j];
                else
                    return Items[Inx[j]];
            }
        }
    }
}
