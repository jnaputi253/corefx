// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace System.Text.Tests
{
    public static class UnicodeScalarTests
    {
        [Theory]
        [MemberData(nameof(GeneralTestData_BmpCodePoints_NoSurrogates))]
        public static void Ctor_Char_Valid(GeneralTestData testData)
        {
            UnicodeScalar scalar = new UnicodeScalar(checked((char)testData.ScalarValue));

            Assert.Equal((uint)testData.ScalarValue, scalar.Value);
            Assert.Equal(testData.IsAscii, scalar.IsAscii);
            Assert.Equal(testData.IsBmp, scalar.IsBmp);
            Assert.Equal(testData.Plane, scalar.Plane);
        }

        [Theory]
        [MemberData(nameof(BmpCodePoints_SurrogatesOnly))]
        public static void Ctor_Char_Invalid_Throws(char scalarValue)
        {
            Assert.Throws<ArgumentOutOfRangeException>("ch", () => new UnicodeScalar(scalarValue));
        }

        [Theory]
        [MemberData(nameof(GeneralTestData_BmpCodePoints_NoSurrogates))]
        [MemberData(nameof(GeneralTestData_SupplementaryCodePoints_ValidOnly))]
        public static void Ctor_Int32_Valid(GeneralTestData testData)
        {
            UnicodeScalar scalar = new UnicodeScalar((int)testData.ScalarValue);

            Assert.Equal((uint)testData.ScalarValue, scalar.Value);
            Assert.Equal(testData.IsAscii, scalar.IsAscii);
            Assert.Equal(testData.IsBmp, scalar.IsBmp);
            Assert.Equal(testData.Plane, scalar.Plane);
        }

        [Theory]
        [MemberData(nameof(BmpCodePoints_SurrogatesOnly))]
        [MemberData(nameof(SupplementaryCodePoints_InvalidOnly))]
        public static void Ctor_Int32_Invalid_Throws(int scalarValue)
        {
            Assert.Throws<ArgumentOutOfRangeException>("scalarValue", () => new UnicodeScalar(scalarValue));
        }

        [Theory]
        [MemberData(nameof(GeneralTestData_BmpCodePoints_NoSurrogates))]
        [MemberData(nameof(GeneralTestData_SupplementaryCodePoints_ValidOnly))]
        public static void Ctor_UInt32_Valid(GeneralTestData testData)
        {
            UnicodeScalar scalar = new UnicodeScalar((uint)testData.ScalarValue);

            Assert.Equal((uint)testData.ScalarValue, scalar.Value);
            Assert.Equal(testData.IsAscii, scalar.IsAscii);
            Assert.Equal(testData.IsBmp, scalar.IsBmp);
            Assert.Equal(testData.Plane, scalar.Plane);
        }

        [Theory]
        [MemberData(nameof(BmpCodePoints_SurrogatesOnly))]
        [MemberData(nameof(SupplementaryCodePoints_InvalidOnly))]
        public static void Ctor_UInt32_Invalid_Throws(int scalarValue)
        {
            Assert.Throws<ArgumentOutOfRangeException>("scalarValue", () => new UnicodeScalar((uint)scalarValue));
        }

        [Theory]
        [InlineData('A', 'a', -1)]
        [InlineData('A', 'A', 0)]
        [InlineData('a', 'A', 1)]
        [InlineData(0x10000, 0x10000, 0)]
        [InlineData('\uFFFD', 0x10000, -1)]
        [InlineData(0x10FFFF, 0x10000, 1)]
        public static void CompareTo_And_ComparisonOperators(int first, int other, int expectedSign)
        {
            UnicodeScalar a = new UnicodeScalar(first);
            UnicodeScalar b = new UnicodeScalar(other);

            Assert.Equal(expectedSign, Math.Sign(a.CompareTo(b)));
            Assert.Equal(expectedSign < 0, a < b);
            Assert.Equal(expectedSign <= 0, a <= b);
            Assert.Equal(expectedSign > 0, a > b);
            Assert.Equal(expectedSign >= 0, a >= b);
        }


        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(0x10FFFF, 0x10FFFF, true)]
        [InlineData(0xFFFD, 0xFFFD, true)]
        [InlineData(0xFFFD, 0xFFFF, false)]
        [InlineData('a', 'a', true)]
        [InlineData('a', 'A', false)]
        [InlineData('a', 'b', false)]
        public static void Equals_OperatorEqual_OperatorNotEqual(int first, int other, bool expected)
        {
            UnicodeScalar a = new UnicodeScalar(first);
            UnicodeScalar b = new UnicodeScalar(other);

            Assert.Equal(expected, Object.Equals(a, b));
            Assert.Equal(expected, a.Equals(b));
            Assert.Equal(expected, a == b);
            Assert.NotEqual(expected, a != b);
        }

        [Theory]
        [InlineData(0)]
        [InlineData('a')]
        [InlineData('\uFFFD')]
        [InlineData(0x10FFFF)]
        public static void GetHashCodeTests(int scalarValue)
        {
            Assert.Equal(scalarValue, new UnicodeScalar(scalarValue).GetHashCode());
        }

        [Theory]
        [MemberData(nameof(IsValidTestData))]
        public static void IsValid(int scalarValue, bool expectedIsValid)
        {
            Assert.Equal(expectedIsValid, UnicodeScalar.IsValid(scalarValue));
        }

        [Fact]
        public static void ReplacementChar()
        {
            Assert.Equal((uint)0xFFFD, UnicodeScalar.ReplacementChar.Value);
        }

        [Theory]
        [MemberData(nameof(GeneralTestData_BmpCodePoints_NoSurrogates))]
        [MemberData(nameof(GeneralTestData_SupplementaryCodePoints_ValidOnly))]
        public static void ToUtf8_And_ToUtf16(GeneralTestData testData)
        {
            UnicodeScalar scalar = new UnicodeScalar(testData.ScalarValue);

            // UTF-8

            Span<byte> utf8Buffer = stackalloc byte[4];
            Assert.Equal(testData.Utf8Sequence.Length, scalar.Utf8SequenceLength);
            Assert.Equal(testData.Utf8Sequence.Length, scalar.ToUtf8(utf8Buffer));
            SpanAssert.Equal(testData.Utf8Sequence, utf8Buffer.Slice(0, testData.Utf8Sequence.Length));
            Assert.Equal(new Utf8String(testData.Utf8Sequence), scalar.ToUtf8String());

            // UTF-16

            Span<char> utf16Buffer = stackalloc char[2];
            Assert.Equal(testData.Utf16Sequence.Length, scalar.Utf16SequenceLength);
            Assert.Equal(testData.Utf16Sequence.Length, scalar.ToUtf16(utf16Buffer));
            SpanAssert.Equal(testData.Utf16Sequence, utf16Buffer.Slice(0, testData.Utf16Sequence.Length));
            Assert.Equal(new string(testData.Utf16Sequence), scalar.ToString());
        }

        [Theory]
        [MemberData(nameof(GeneralTestData_BmpCodePoints_NoSurrogates))]
        [MemberData(nameof(GeneralTestData_SupplementaryCodePoints_ValidOnly))]
        public static void TryCreate_Int32_Valid(GeneralTestData testData)
        {
            Assert.True(UnicodeScalar.TryCreate((int)testData.ScalarValue, out UnicodeScalar result));
            Assert.Equal((uint)testData.ScalarValue, result.Value);
        }

        [Theory]
        [MemberData(nameof(BmpCodePoints_SurrogatesOnly))]
        [MemberData(nameof(SupplementaryCodePoints_InvalidOnly))]
        public static void TryCreate_Int32_Invalid(int scalarValue)
        {
            Assert.False(UnicodeScalar.TryCreate((int)scalarValue, out UnicodeScalar result));
            Assert.Equal((uint)0, result.Value);
        }

        [Theory]
        [MemberData(nameof(GeneralTestData_BmpCodePoints_NoSurrogates))]
        [MemberData(nameof(GeneralTestData_SupplementaryCodePoints_ValidOnly))]
        public static void TryCreate_UInt32_Valid(GeneralTestData testData)
        {
            Assert.True(UnicodeScalar.TryCreate((uint)testData.ScalarValue, out UnicodeScalar result));
            Assert.Equal((uint)testData.ScalarValue, result.Value);
        }

        [Theory]
        [MemberData(nameof(BmpCodePoints_SurrogatesOnly))]
        [MemberData(nameof(SupplementaryCodePoints_InvalidOnly))]
        public static void TryCreate_UInt32_Invalid(int scalarValue)
        {
            Assert.False(UnicodeScalar.TryCreate((uint)scalarValue, out UnicodeScalar result));
            Assert.Equal((uint)0, result.Value);
        }

        public static IEnumerable<object[]> GeneralTestData_BmpCodePoints_NoSurrogates()
        {
            yield return new object[]
            {
                // first BMP code point / first ASCII code point
                new GeneralTestData
                {
                    ScalarValue = 0x0000,
                    IsAscii = true,
                    IsBmp = true,
                    Plane = 0,
                    Utf16Sequence = new char[] { '\u0000' },
                    Utf8Sequence = new byte[] { 0x00 }
                }
            };

            yield return new object[]
            {
                // last ASCII code point
                new GeneralTestData
                {
                    ScalarValue = 0x007F,
                    IsAscii = true,
                    IsBmp = true,
                    Plane = 0,
                    Utf16Sequence = new char[] { '\u007F' },
                    Utf8Sequence = new byte[] { 0x7F }
                }
            };

            yield return new object[] {
                // first non-ASCII code point / first UTF-8 two-code unit code point
                new GeneralTestData
                {
                    ScalarValue = 0x0080,
                    IsAscii = false,
                    IsBmp = true,
                    Plane = 0,
                    Utf16Sequence = new char[] { '\u0080' },
                    Utf8Sequence = new byte[] { 0xC2, 0x80 }
                }
            };

            yield return new object[] {
                // last UTF-8 two-code unit code point
                new GeneralTestData
                {
                    ScalarValue = 0x07FF,
                    IsAscii = false,
                    IsBmp = true,
                    Plane = 0,
                    Utf16Sequence = new char[] { '\u07FF' },
                    Utf8Sequence = new byte[] { 0xDF, 0xBF }
                }
            };

            yield return new object[] {
                // first UTF-8 three-code unit code point
                new GeneralTestData
                {
                    ScalarValue = 0x0800,
                    IsAscii = false,
                    IsBmp = true,
                    Plane = 0,
                    Utf16Sequence = new char[] { '\u0800' },
                    Utf8Sequence = new byte[] { 0xE0, 0xA0, 0x80 }
                }
            };

            yield return new object[] {
                // last code point before the surrogate range
                new GeneralTestData
                {
                    ScalarValue = 0xD7FF,
                    IsAscii = false,
                    IsBmp = true,
                    Plane = 0,
                    Utf16Sequence = new char[] { '\uD7FF' },
                    Utf8Sequence = new byte[] { 0xED, 0x9F, 0xBF }
                }
            };

            yield return new object[] {
                // first code point after the surrogate range
                new GeneralTestData
                {
                    ScalarValue = 0xE000,
                    IsAscii = false,
                    IsBmp = true,
                    Plane = 0,
                    Utf16Sequence = new char[] { '\uE000' },
                    Utf8Sequence = new byte[] { 0xEE, 0x80, 0x80 }
                }
            };

            yield return new object[] {
                // replacement character
                new GeneralTestData
                {
                    ScalarValue = 0xFFFD,
                    IsAscii = false,
                    IsBmp = true,
                    Plane = 0,
                    Utf16Sequence = new char[] { '\uFFFD' },
                    Utf8Sequence = new byte[] { 0xEF, 0xBF, 0xBD }
                }
            };

            yield return new object[] {
                // last BMP code point / last UTF-8 two-code unit code point
                new GeneralTestData
                {
                    ScalarValue = 0xFFFF,
                    IsAscii = false,
                    IsBmp = true,
                    Plane = 0,
                    Utf16Sequence = new char[] { '\uFFFF' },
                    Utf8Sequence = new byte[] { 0xEF, 0xBF, 0xBF }
                }
            };
        }

        public static IEnumerable<object[]> GeneralTestData_SupplementaryCodePoints_ValidOnly()
        {
            yield return new object[]
            {
                // first BMP code point / first ASCII code point
                new GeneralTestData
                {
                    ScalarValue = 0x10000,
                    IsAscii = false,
                    IsBmp = false,
                    Plane = 1,
                    Utf16Sequence = new char[] { '\uD800', '\uDC00' },
                    Utf8Sequence = new byte[] { 0xF0, 0x90, 0x80, 0x80 }
                }
            };

            yield return new object[]
            {
                // last supplementary code point
                new GeneralTestData
                {
                    ScalarValue = 0x10FFFF,
                    IsAscii = false,
                    IsBmp = false,
                    Plane = 16,
                    Utf16Sequence = new char[] { '\uDBFF', '\uDFFF' },
                    Utf8Sequence = new byte[] { 0xF4, 0x8F, 0xBF, 0xBF }
                }
            };
        }
        public static IEnumerable<object[]> IsValidTestData()
        {
            foreach (var obj in GeneralTestData_BmpCodePoints_NoSurrogates().Concat(GeneralTestData_SupplementaryCodePoints_ValidOnly()))
            {
                yield return new object[] { ((GeneralTestData)obj[0]).ScalarValue /* value */, true /* isValid */ };
            }

            foreach (var obj in BmpCodePoints_SurrogatesOnly().Concat(SupplementaryCodePoints_InvalidOnly()))
            {
                yield return new object[] { Convert.ToInt32(obj[0], CultureInfo.InvariantCulture) /* value */, false /* isValid */ };
            }
        }

        public static IEnumerable<object[]> BmpCodePoints_SurrogatesOnly()
        {
            yield return new object[] { '\uD800' }; // first high surrogate code point
            yield return new object[] { '\uDBFF' }; // last high surrogate code point
            yield return new object[] { '\uDC00' }; // first low surrogate code point
            yield return new object[] { '\uDFFF' }; // last low surrogate code point
        }

        public static IEnumerable<object[]> SupplementaryCodePoints_InvalidOnly()
        {
            yield return new object[] { (int)-1 }; // negative code points are disallowed
            yield return new object[] { (int)0x110000 }; // just past the end of the allowed code point range
            yield return new object[] { int.MaxValue }; // too large
        }

        public class GeneralTestData
        {
            public int ScalarValue;
            public bool IsAscii;
            public bool IsBmp;
            public int Plane;
            public char[] Utf16Sequence;
            public byte[] Utf8Sequence;
        }
    }
}