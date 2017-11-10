using NUnit.Framework;

namespace UBlockly.Test
{
    public class GeneratorTest
    {
        [Test]
        public void TestPrefix()
        {
            var generator = new CSharpGenerator(null);
            Assert.AreEqual("", generator.PrefixLines("", ""), "Prefix nothing.");
            Assert.AreEqual("@Hello", generator.PrefixLines("Hello", "@"), "Prefix a word");
            Assert.AreEqual("12Hello\n", generator.PrefixLines("Hello\n", "12"), "Prefix one line");
            Assert.AreEqual("***Hello\n***World\n", generator.PrefixLines("Hello\nWorld\n", "***"), "Prefix two lines");
        }
    }
}
