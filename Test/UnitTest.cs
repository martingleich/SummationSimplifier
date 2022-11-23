using SummationSimplifier;
using System.Collections.Immutable;

namespace Test
{
    public class UnitTest
    {
        [Fact]
        public void ConstantFunction()
        {
            var polynomial = Simplifier.Simplify(Expression.Constant(12));
            Assert.Equal(new RationalPolynomial(ImmutableArray.Create(12), 1), polynomial); // f(x) = 12
        }
        [Fact]
        public void SumOfConstants()
        {
            var polynomial = Simplifier.Simplify(Expression.Summation(1, Expression.Parameter, i => 3));
            Assert.Equal(new RationalPolynomial(ImmutableArray.Create(0, 3), 1), polynomial); // f(x) = 3*x
        }
        [Fact]
        public void SumOfLinearTerms()
        {
            var polynomial = Simplifier.Simplify(Expression.Summation(1, Expression.Parameter, i => i));
            Assert.Equal(new RationalPolynomial(ImmutableArray.Create(0, 1, 1), 2), polynomial); // f(x) = (x² + x) / 2
        }
        [Fact]
        public void SumOfLinearTermsWithFactor()
        {
            var polynomial = Simplifier.Simplify(Expression.Summation(1, Expression.Parameter, i => 2*i));
            Assert.Equal(new RationalPolynomial(ImmutableArray.Create(0, 1, 1), 1), polynomial); // f(x) = x² + x
        }
        [Fact]
        public void SumOfSquares()
        {
            var polynomial = Simplifier.Simplify(Expression.Summation(1, Expression.Parameter, i => i*i));
            Assert.Equal(new RationalPolynomial(ImmutableArray.Create(0, 1, 3, 2), 6), polynomial); // f(x) = (2x³ + 3x² + x) / 6
        }
        [Fact]
        public void SumOfScaledParameter()
        {
            var polynomial = Simplifier.Simplify(Expression.Summation(1, 2*Expression.Parameter, i => i));
            Assert.Equal(new RationalPolynomial(ImmutableArray.Create(0, 1, 2), 1), polynomial); // f(x) = 2x² + x
        }
    }
}