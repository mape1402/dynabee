namespace DynaBee.Tests.Tools
{
    using global::DynaBee.Tools;
    using System;
    using Xunit;

    namespace DynaBee.Tests
    {
        public class ImmutableTests
        {
            [Fact]
            public void Constructor_Throws_If_ValidationFunction_Is_Null()
            {
                Assert.Throws<ArgumentNullException>(() => new Immutable<string>(null));
            }

            [Fact]
            public void DefaultConstructor_Considers_Default_Value_Invalid()
            {
                var immutable = new Immutable<int>();
                Assert.False(immutable.IsValid());
            }

            [Fact]
            public void Set_Valid_Value_Stores_Value()
            {
                var immutable = new Immutable<string>(s => !string.IsNullOrWhiteSpace(s));
                immutable.Set("hello");
                Assert.Equal("hello", immutable.Value);
            }

            [Fact]
            public void Set_Throws_If_Value_Invalid()
            {
                var immutable = new Immutable<int>(x => x > 0);
                var ex = Assert.Throws<ArgumentException>(() => immutable.Set(0));
                Assert.Contains("Invalid value", ex.Message);
            }

            [Fact]
            public void Set_Throws_If_Already_Set()
            {
                var immutable = new Immutable<string>(s => !string.IsNullOrEmpty(s));
                immutable.Set("hello");
                var ex = Assert.Throws<InvalidOperationException>(() => immutable.Set("world"));
                Assert.Contains("already been set", ex.Message);
            }

            [Fact]
            public void IsValid_Returns_True_After_Setting_Valid_Value()
            {
                var immutable = new Immutable<int>(x => x > 0);
                immutable.Set(42);
                Assert.True(immutable.IsValid());
            }

            [Fact]
            public void Implicit_Operator_Returns_Value()
            {
                var immutable = new Immutable<string>(s => !string.IsNullOrEmpty(s));
                immutable.Set("test");

                string result = immutable;
                Assert.Equal("test", result);
            }
        }
    }

}
