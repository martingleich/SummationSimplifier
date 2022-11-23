using System;
using System.Collections.Generic;

namespace SummationSimplifier
{
	/// <summary>
	/// A expression depending on one parameter.
	/// </summary>
	public abstract record Expression
	{
		/// <summary>
		/// The value of the passed parameter
		/// </summary>
		public static readonly Expression Parameter = new ParameterT();
		/// <summary>
		/// A constant integer value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static Expression Constant(int value) => new ConstantT(value);
		/// <summary>
		/// Sum a sequence of values from <paramref name="from"/> to <paramref name="to"/>
		/// </summary>
		/// <param name="from">The first value of the index variable.</param>
		/// <param name="to">The last value of the index variable</param>
		/// <param name="body">The values that are summed, depending on the index variable</param>
		/// <returns></returns>
		public static Expression Summation(Expression from, Expression to, System.Linq.Expressions.Expression<Func<Expression, Expression>> body)
		{
			var uniqueName = body.Parameters[0].Name;
			var realBody = body.Compile()(new Index(uniqueName));
			return new SummationT(from, to, uniqueName, realBody);
		}

		/// <summary>
		/// Multiply two expressions.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Expression operator *(Expression left, Expression right) => new Product(left, right);
		/// <summary>
		/// Add two expressions.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Expression operator +(Expression left, Expression right) => new Addition(left, right);
		/// <summary>
		/// Take the negative of an expression.
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>
		public static Expression operator -(Expression arg) => -1 * arg;
		/// <summary>
		/// Subtract two expressions.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static Expression operator -(Expression left, Expression right) => left + -right;
		/// <summary>
		/// Convert an integer into a constant expression.
		/// </summary>
		/// <param name="value"></param>
		public static implicit operator Expression(int value) => Constant(value);

		protected abstract class Context<T>
		{
			/// <summary>
			/// Get the value of the passed Parameter
			/// </summary>
			public abstract T Parameter { get; }
			/// <summary>
			/// Get the value of a index variable.
			/// </summary>
			/// <param name="name"></param>
			/// <returns></returns>
			public abstract T Index(string name);
			public sealed class Root : Context<T>
			{
				public Root(T parameter)
				{
					Parameter = parameter;
				}

				public override T Parameter { get; }
				public override T Index(string name)
				{
					throw new ArgumentException($"Unknown index {name}.");
				}
			}
			public sealed class Indexed : Context<T>
			{
				private readonly Context<T> _baseContext;
				private readonly KeyValuePair<string, T> _index;
				public Indexed(Context<T> baseContext, KeyValuePair<string, T> index)
				{
					_baseContext = baseContext;
					_index = index;
				}

				public override T Index(string name)
				{
					if (_index.Key == name)
						return _index.Value;
					else
						return _baseContext.Index(name);
				}
				public override T Parameter => _baseContext.Parameter;
			}
		}
		private sealed record ParameterT : Expression
		{
			protected override int Degree(Context<int> context) => context.Parameter;
			protected override int Evaluate(Context<int> ctx) => ctx.Parameter;
		}
		private sealed record Index(string Name) : Expression
		{
			protected override int Degree(Context<int> context) => context.Index(Name);
			protected override int Evaluate(Context<int> ctx) => ctx.Index(Name);
		}
		private sealed record Addition(Expression Left, Expression Right) : Expression
		{
			protected override int Degree(Context<int> context) => Math.Max(Left.Degree(context), Right.Degree(context));
			protected override int Evaluate(Context<int> ctx) => Left.Evaluate(ctx) + Right.Evaluate(ctx);
		}
		private sealed record Product(Expression Left, Expression Right) : Expression
		{
			protected override int Degree(Context<int> context) => Left.Degree(context) + Right.Degree(context);
			protected override int Evaluate(Context<int> ctx) => Left.Evaluate(ctx) * Right.Evaluate(ctx);
		}
		private sealed record ConstantT(int Value) : Expression
		{
			protected override int Degree(Context<int> context) => 0;
			protected override int Evaluate(Context<int> ctx) => Value;
		}
		private sealed record SummationT(Expression From, Expression To, string IndexVariableName, Expression Body) : Expression
		{
			protected override int Degree(Context<int> context)
			{
				int from = From.Degree(context);
				int to = To.Degree(context);
				int count = Math.Max(from, to); // Overestimate.
				return Body.Degree(new Context<int>.Indexed(context, KeyValuePair.Create(IndexVariableName, count))) + count;
			}
			protected override int Evaluate(Context<int> ctx)
			{
				int from = From.Evaluate(ctx);
				int to = To.Evaluate(ctx);
				int total = 0;
				for (int i = from; i <= to; ++i)
					total += Body.Evaluate(new Context<int>.Indexed(ctx, KeyValuePair.Create(IndexVariableName, i)));
				return total;
			}
		}

		protected abstract int Evaluate(Context<int> context);
		protected abstract int Degree(Context<int> context);

		/// <summary>
		/// Get an upper-bound estimation of the degree of the polynomial required to calculate this expression.
		/// </summary>
		/// <returns></returns>
		public int GetDegree() => Degree(new Context<int>.Root(1));
		/// <summary>
		/// Evaluate the expression at a point.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public int Evaluate(int value) => Evaluate(new Context<int>.Root(value));
	}
}
