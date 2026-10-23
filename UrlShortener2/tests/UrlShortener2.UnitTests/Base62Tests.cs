using UrlShortener2.Utils;
using Xunit;

namespace UrlShortener2.UnitTests
{
	public class Base62Tests
	{
		[Theory]
		[InlineData(12, "00000C")]
		[InlineData(290, "00004g")]
		[InlineData(34120, "0008sK")]
		public void Encode_ProducesExpectedCode(long value, string expected)
		{
			Assert.Equal(expected, Base62.Encode(value));
		}

		[Theory]
		[InlineData(1)]
		[InlineData(12)]
		[InlineData(290)]
		[InlineData(34120)]
		[InlineData(999999999999L)]
		public void Decode_ReversesEncode(long value)
		{
			var encoded = Base62.Encode(value);
			Assert.Equal(value, Base62.Decode(encoded));
		}

		[Theory]
		[InlineData(1)]
		[InlineData(62)]
		[InlineData(10000000)] // big enough to exceed the 6-char padding
		public void Encode_IsAtLeastMinLength(long value)
		{
			Assert.True(Base62.Encode(value).Length >= 6);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(-1)]
		public void Encode_RejectsNonPositive(long value)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Base62.Encode(value));
		}

		[Theory]
		[InlineData("")]
		[InlineData(null)]
		public void Decode_RejectsEmptyOrNull(string? code)
		{
			Assert.Throws<ArgumentException>(() => Base62.Decode(code!));
		}

		[Fact]
		public void Decode_RejectsInvalidCharacter()
		{
			// '!' is not part of the Base62 alphabet.
			Assert.Throws<ArgumentException>(() => Base62.Decode("abc!23"));
		}
	}
}
