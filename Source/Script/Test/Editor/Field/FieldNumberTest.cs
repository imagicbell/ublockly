using NUnit.Framework;

namespace UBlockly.Test
{
    public class FieldNumberTest
    {
        [Test]
        public void TestFieldNumberConstructor()
        {
            var field = new FieldNumber(null);
            Assert.AreEqual("0", field.GetValue());
            
            //numeric values
            field = new FieldNumber(null, new Number(1), Number.NaN, Number.NaN);
            Assert.AreEqual(field.GetValue(), "1");
            field = new FieldNumber(null, new Number(1.5f), Number.NaN, Number.NaN);
            Assert.AreEqual(field.GetValue(), "1.5");
            
            //string values
            field = new FieldNumber(null, "2");
            Assert.AreEqual(field.GetValue(), "2");
            field = new FieldNumber(null, "2.5");
            Assert.AreEqual(field.GetValue(), "2.5");

            field = new FieldNumber(null, new Number(0), new Number(-128), new Number(127));
            Assert.AreEqual(field.GetValue(), "0");
            Assert.AreEqual(field.Min.Value, -128);
            Assert.AreEqual(field.Max.Value, 127);
            
            field = new FieldNumber(null, "bad");
            Assert.AreEqual(field.GetValue(), "0");
        }
    }
}
