using NUnit.Framework;

namespace UBlockly.Test
{
    public class FieldAngleTest
    {
        [Test]
        public void TestFieldAngleConstructor()
        {
            Assert.AreEqual(new FieldAngle(null).GetValue(), "0");
            Assert.AreEqual(new FieldAngle(null, "").GetValue(), "0");
            Assert.AreEqual(new FieldAngle(null, new Number(1)).GetValue(), "1");
            Assert.AreEqual(new FieldAngle(null, new Number(1.5f)).GetValue(), "1.5");
            Assert.AreEqual(new FieldAngle(null, "2").GetValue(), "2");
            Assert.AreEqual(new FieldAngle(null, "2.5").GetValue(), "2.5");
            Assert.AreEqual(new FieldAngle(null, "bad").GetValue(), "0");
            Assert.AreEqual(new FieldAngle(null, new Number()).GetValue(), "0");
        }
    }
}
