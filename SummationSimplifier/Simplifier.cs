using System;
using System.Collections.Immutable;
using System.Linq;

namespace SummationSimplifier
{
	public static class Simplifier
	{
		public static RationalPolynomial Simplify(Expression expression)
		{
			if (expression is null)
			{
				throw new ArgumentNullException(nameof(expression));
			}

			var degree = expression.GetDegree();
			Func<int, int> eval = expression.Evaluate;
			var coeff = Enumerable.Range(0, degree + 1).Select(eval).ToImmutableArray();
			return RationalPolynomial.MakeLagrangeAtNaturals(coeff).Simplified();
		}
	}
}
