using NUnit.Framework;
using Grid.Domain;

namespace Tests
{
    public class GridCoordTests
    {
        [Test]
        public void Constructor_SetsXAndZ()
        {
            var coord = new GridCoord(3, 7);
            Assert.AreEqual(3, coord.X);
            Assert.AreEqual(7, coord.Z);
        }

        [Test]
        public void Zero_IsOrigin()
        {
            var zero = GridCoord.Zero;
            Assert.AreEqual(0, zero.X);
            Assert.AreEqual(0, zero.Z);
        }

        [Test]
        public void Addition_SumsComponents()
        {
            var a = new GridCoord(2, 3);
            var b = new GridCoord(5, 1);
            var result = a + b;
            Assert.AreEqual(7, result.X);
            Assert.AreEqual(4, result.Z);
        }

        [Test]
        public void Addition_WithZero_ReturnsSameCoord()
        {
            var coord = new GridCoord(4, 9);
            var result = coord + GridCoord.Zero;
            Assert.AreEqual(coord.X, result.X);
            Assert.AreEqual(coord.Z, result.Z);
        }

        [Test]
        public void Addition_NegativeComponents_Works()
        {
            var a = new GridCoord(5, 5);
            var b = new GridCoord(-3, -2);
            var result = a + b;
            Assert.AreEqual(2, result.X);
            Assert.AreEqual(3, result.Z);
        }
    }
}
