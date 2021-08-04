using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinance
{
    class Polynomial
    {
        List<(decimal coefficient, int pow)> P;
        public Polynomial(IEnumerable<(decimal coefficient, int pow)> P)
        {
            this.P = P.ToList();
        }
        public Polynomial D()
        {
            var aux = P.Where(x=>x.pow>0).Select(x=>(x.coefficient*x.pow,x.pow-1));
            return new Polynomial(aux);
        }
        public decimal Evaluate(decimal x)
        {
            return P.Sum(y => y.coefficient * (decimal)Math.Pow((double)x, y.pow));
        }
        public  Task<decimal> FindRoot(decimal x0 = 1m)
        {
            return Task.Run(() =>
            {
                decimal x1;
                while (true)
                {
                    var e = Evaluate(x0);
                    x1 = x0 - e / D().Evaluate(x0);
                    if (Math.Abs(e) < 0.01m)
                        break;
                    x0 = x1;
                }
                return x1;
            });
        }
    }
}
