using System.Text;

namespace UrlShortener2.Utils
{
	public static class Base62
	{
		private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		/// <summary>
		/// Encodes a numer to string.
		/// Needs to be positive value, left-padded with '0' up to minLength (the assignment requires 6-10 char codes).
		/// </summary>
		/// <param name="value">The number of the mapping, that we will return as an alphanumeric string</param>
		/// <param name="minLength"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static string Encode(long value, int minLength = 6)
		{
			//logic:
			//example 12
			//  12 % 62 = 12 -> Alphabet[12] = 'C'; 12 / 62 = 0 -> loop ends (1 digit)
			//  digits low-to-high = ['C'], reversed = "C" -> padded to minLength 6 = "00000C"
			//example 290
			//  290 % 62 = 42 -> Alphabet[42] = 'g'; 290 / 62 = 4
			//  4 % 62 = 4 -> Alphabet[4] = '4'; 4 / 62 = 0 -> loop ends (2 digits)
			//  digits low-to-high = ['g','4'], reversed = "4g" -> padded = "00004g"
			//example 34120
			//  34120 % 62 = 20 -> Alphabet[20] = 'K'; 34120 / 62 = 550
			//  550 % 62 = 54 -> Alphabet[54] = 's'; 550 / 62 = 8
			//  8 % 62 = 8 -> Alphabet[8] = '8'; 8 / 62 = 0 -> loop ends (3 digits)
			//  digits low-to-high = ['K','s','8'], reversed = "8sK" -> padded = "0008sK"


			//validation
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
			}


			var chars = new List<char>();
			while (value > 0)
			{
				// value % 62 is the next base62 "digit" (0-61) — the remainder of dividing by 62.
				// Alphabet[...] turns that digit into its character ('0'-'9', 'A'-'Z', 'a'-'z').
				chars.Add(Alphabet[(int)(value % 62)]);

				// Integer division drops the digit we just extracted, shifting everything
				// one place to the right — same as dropping the last digit in base 10 by dividing by 10.
				value = value / 62;
			}

			// We extracted digits least-significant-first (right to left), so reverse before reading as a string.
			chars.Reverse();

			var encoded = new string(chars.ToArray());
			return encoded.Length >= minLength ? encoded : encoded.PadLeft(minLength, Alphabet[0]);
		}

		/// <summary>
		/// Decodes a base62 string (as produced by <see cref="Encode"/>) back into its numeric value.
		/// Leading padding characters (Alphabet[0], i.e. '0') contribute 0 and don't affect the result.
		/// </summary>
		/// <param name="code">The base62 string to decode.</param>
		/// <returns>The decoded value.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static long Decode(string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				throw new ArgumentException("Value must not be null or empty.", nameof(code));
			}

			long value = 0;
			foreach (var c in code)
			{
				var digit = Alphabet.IndexOf(c);
				if (digit < 0)
				{
					throw new ArgumentException($"'{c}' is not a valid base62 character.", nameof(code));
				}

				// Each character is read left to right, most-significant digit first —
				// the reverse of Encode: shift what we have so far up one place (x62), then add the new digit.
				value = value * 62 + digit;
			}

			return value;
		}
	}
}
