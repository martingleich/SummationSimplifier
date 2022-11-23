using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SummationSimplifier
{
    public readonly struct RationalPolynomial : IEquatable<RationalPolynomial>
    {
        public readonly ImmutableArray<int> Coefficents;
        public readonly int Divider;

        public RationalPolynomial(ImmutableArray<int> coefficents, int divider)
        {
            if (divider == 0)
                throw new ArgumentException($"Division by zero");
            Coefficents = coefficents;
            Divider = divider;
        }

        public static RationalPolynomial Sum(RationalPolynomial a, RationalPolynomial b)
        {
            if (a.Coefficents.Length < b.Coefficents.Length)
            {
                (b, a) = (a, b);
            }

            var commonDiv = Utils.Lcm(a.Divider, b.Divider);
            var bmul = commonDiv / b.Divider;
            var amul = commonDiv / a.Divider;
            var result = new int[a.Coefficents.Length];
            int p = 0;
            for (; p < b.Coefficents.Length; ++p)
                result[p] = a.Coefficents[p] * amul + b.Coefficents[p] * bmul;
            for (; p < a.Coefficents.Length; ++p)
                result[p] = a.Coefficents[p] * amul;
            return new(result.ToImmutableArray(), commonDiv);
        }

        public static RationalPolynomial Product(RationalPolynomial a, RationalPolynomial b)
        {
            var result = new int[a.Coefficents.Length + b.Coefficents.Length - 1];
            for (int pa = 0; pa < a.Coefficents.Length; ++pa)
            {
                for (int pb = 0; pb < b.Coefficents.Length; ++pb)
                {
                    result[pa + pb] += a.Coefficents[pa] * b.Coefficents[pb];
                }
            }
            return new(result.ToImmutableArray(), a.Divider * b.Divider);
        }

        public RationalPolynomial Simplified()
        {
            var gcd = Utils.Gcd(Coefficents.Aggregate(Utils.Gcd), Divider);
            if (Divider < 0 != gcd < 0)
                gcd = -gcd; // Prefer a positive divider
            if (gcd == 1)
                return this;

            var result = ImmutableArray.CreateRange(Coefficents, c => c / gcd);
            return new(result, Divider / gcd);
        }
        public override string ToString() => ToUnicodeString();
        public string ToAsciiString() => ToAnyString(false);
        public string ToUnicodeString() => ToAnyString(true);
        private string ToAnyString(bool unicode)
        {
            static (bool Sign, string Elem) ToString(uint p, bool sign, int c, bool unicode)
            {
                if (p == 0)
                {
                    return (sign, c.ToString());
                }
                else
                {
                    string exp = p == 1 ?
                        "x" :
                        "x" + Utils.ToSuperscript(p, unicode);
                    string elem = c == 1 ?
                        exp :
                        c.ToString() + exp;
                    return (sign, elem);
                }
            }
            bool isFirst = true;
            string value = "";
            for (int p = Coefficents.Length - 1; p >= 0; --p)
            {
                var c = Coefficents[p];
                if (c == 0)
                    continue;
                var (Sign, Elem) = ToString((uint)p, c < 0, Math.Abs(c), unicode);
                if (isFirst)
                {
                    if (Sign)
                        value += "-";
                    value += Elem;
                    isFirst = false;
                }
                else
                {
                    value += Sign ? " - " : " + ";
                    value += Elem;
                }
            }
            if (value.Length == 0)
                return "0";
            if (Divider != 1)
                return $"({value})/{Divider}";
            else
                return value;
        }

        public static readonly RationalPolynomial Zero = new(ImmutableArray<int>.Empty, 1);
        public static RationalPolynomial Integer(int value) => new(ImmutableArray.Create(value), 1);
        public static RationalPolynomial MakeLagrangeAtNaturals(ImmutableArray<int> values) // The values at 0,1,2,3,4,...
        {
            var result = Zero;
            for (int i = 0; i < values.Length; ++i)
            {
                var topPoly = Integer(values[i]);
                for (int j = 0; j < values.Length; ++j)
                {
                    if (i != j)
                    {
                        topPoly = Product(topPoly, new RationalPolynomial(ImmutableArray.Create(-j, 1), i - j));
                    }
                }
                result = Sum(result, topPoly);
            }
            return result;
        }

        public override bool Equals([NotNullWhen(true)] object obj) => throw new NotImplementedException();
        public bool Equals(RationalPolynomial other)
        {
            if (other.Divider != Divider)
                return false;
            if (other.Coefficents.Length != Coefficents.Length)
                return false;
            for (int i = 0; i < Coefficents.Length; ++i)
                if (other.Coefficents[i] != Coefficents[i])
                    return false;
            return true;
        }
        public override int GetHashCode()
        {
            HashCode h = default;
            h.Add(Coefficents.Length);
            foreach (var c in Coefficents)
                h.Add(c);
            h.Add(Divider);
            return h.ToHashCode();
        }

        public static bool operator ==(RationalPolynomial left, RationalPolynomial right) => left.Equals(right);

        public static bool operator !=(RationalPolynomial left, RationalPolynomial right) => !(left == right);
    }
}
