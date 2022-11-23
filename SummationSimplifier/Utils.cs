using System;
using System.Linq;

namespace SummationSimplifier
{
	public static class Utils
	{
		/// <summary>
		/// Get the least-common-multiple of two values
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static int Lcm(int a, int b) => (a / Gcd(a, b)) * b;

		/// <summary>
		/// Get the greatest-common-divisor of two values.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static int Gcd(int a, int b)
		{
			while (b != 0)
			{
				var tmp = b;
				b = a % b;
				a = tmp;
			}
			return a;
		}

		/// <summary>
		/// Convert a unsigned integer into a string, that is made up of superscript characters.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public static string ToSuperscript(uint c, bool unicode)
		{
			if (!unicode)
				return "^" + c.ToString();
			if (c == 0)
				return ToSuperscriptDigit(0);
			string result = "";
			while (c != 0)
			{
				uint d = c % 10;
				result += ToSuperscriptDigit(d);
				c /= 10;
			}
			return new string(result.Reverse().ToArray());

			static string ToSuperscriptDigit(uint c) => c switch
			{
				0 => "⁰",
				1 => "¹",
				2 => "²",
				3 => "³",
				4 => "⁴",
				5 => "⁵",
				6 => "⁶",
				7 => "⁷",
				8 => "⁸",
				9 => "⁹",
				_ => throw new ArgumentException($"Cannot be converted to superscript '{c}'."),
			};
		}
	}
}
