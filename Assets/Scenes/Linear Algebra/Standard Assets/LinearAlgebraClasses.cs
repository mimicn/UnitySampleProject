using UnityEngine;
using System.Collections;
namespace LinearAlgebra
{
    public interface Tai
    {
        
    }
    public class Complex
    {
        float Re, Im;
        public Complex(float z, float w){ Re = z; Im = w; }
        public static Complex operator +(Complex z, float w)
        {
            return new Complex(z.Re + w, z.Im);
        }
        public static Complex operator +(float z, Complex w)
        {
            return new Complex(z + w.Re, w.Im);
        }
        public static Complex operator +(Complex z, Complex w)
        {
            return new Complex(z.Re + w.Re, z.Im + w.Im);
        }
    }
    public class Polynomial//1次多項式
    {
        private int _dimention = -1;
        private float[] coefficients;
        public int dimention
        {
            get { return _dimention; }
            private set
            {
                if (value >= 0)
                {
                    if (dimention != value)
                    {
                        coefficients = new float[value + 1];
                    }
                    _dimention = value;
                }
            }
        }
        private Polynomial() { }
        public Polynomial(int dimention)
        {
            this.dimention = dimention;
        }
        public Polynomial(Polynomial a)
        {
            this.dimention = a.dimention;
            System.Array.Copy(a.coefficients, this.coefficients, this.dimention + 1);
        }
        public static Polynomial operator +(Polynomial a, Polynomial b)
        {
            int min_dim = Mathf.Min(a.dimention, b.dimention);
            bool a_more_b = a.dimention > b.dimention;
            Polynomial ret = new Polynomial(a_more_b ? a : b);
            if (a_more_b)
            {
                for (int k = 0; k <= min_dim; ++k)
                {
                    ret.coefficients[k] += b.coefficients[k];
                }
            }
            else
            {
                for (int k = 0; k <= min_dim; ++k)
                {
                    ret.coefficients[k] += a.coefficients[k];
                }
            }
            return ret;
        }
        public static Polynomial operator -(Polynomial a, Polynomial b)
        {
            return a + (-b);
        }
        public static Polynomial operator -(Polynomial a)
        {
            Polynomial ret = new Polynomial(a);
            for (int k = 0; k <= a.dimention; ++k)
            {
                ret.coefficients[k] += a.coefficients[k];
            }
            return ret;
        }
        //public T Substitution<T>(T x) where T : float//ホーナー法, int, long, double, byte, char
        //{
        //    T ret = (T)(object)0;
        //    if (x is float)
        //    {
        //        ret = (T)(object)SubstitutionForFloat((float)(object)x);
        //    }
        //    return ret;
        //}
        public float SubstitutionForFloat(float x)//ホーナー法
        {
            float ret = 0;
            for (int k = this.dimention; k >= 0; --k)
            {
                ret += ret * x + this.coefficients[k];
            }
            return ret;
        }

    }
    public class Vector<T>
    {

    }
}
