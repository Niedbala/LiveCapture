using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformsExample
{
    class Mat
    {
        public static ValueType Add(ValueType a, ValueType b)
        {
            var wynik = Convert.ToDouble(a) + Convert.ToDouble(b);
            return (ValueType)wynik;
        }

        public static ValueType Diff(ValueType a, ValueType b)
        {
            var wynik = Convert.ToDouble(a) - Convert.ToDouble(b);
            return (ValueType)wynik;
        }

        public static ValueType Prod(ValueType a, ValueType b)
        {
            var wynik = Convert.ToDouble(a) * Convert.ToDouble(b);
            return (ValueType)wynik;
        }

        public static ValueType Div(ValueType a, ValueType b)
        {
            var wynik = Convert.ToDouble(a) / Convert.ToDouble(b);
            return (ValueType)wynik;
        }

        public static ValueType Oper(ValueType a, ValueType b, string oper)
        {
            var wynik = (ValueType)0;
            switch(oper)
            {
                case "+" : wynik = Mat.Add(a, b); break;
                case "-": wynik = Mat.Diff(a, b); break;
                case "x": wynik = Mat.Prod(a, b); break;
                case "/": wynik = Mat.Div(a, b); break;


            }

            return wynik;
        }
    }
}
