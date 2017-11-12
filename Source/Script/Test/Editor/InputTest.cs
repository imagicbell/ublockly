using NUnit.Framework;

namespace UBlockly.Test
{
    public class InputTest
    {
        [Test]
        public void TestAppendFieldSimple()
        {
            var workspace = new Workspace();
            var block = new Block(workspace);
            var input = new Input(Define.EConnection.DummyInput, "INPUT", block);
            var field1 = new FieldLabel("first", "#1");
            var field2 = new FieldLabel("second", "#2");

            Assert.AreEqual(0, input.FieldRow.Count);
            
            //actual test
            input.AppendField(field1);
            Assert.AreEqual(1, input.FieldRow.Count);
            Assert.AreEqual(field1, input.FieldRow[0]);
            Assert.AreEqual("first", input.FieldRow[0].Name);
            Assert.AreEqual(block, field1.SourceBlock);

            input.AppendField(field2);
            Assert.AreEqual(2, input.FieldRow.Count);
            Assert.AreEqual(field2, input.FieldRow[1]);
            Assert.AreEqual("second", input.FieldRow[1].Name);
            Assert.AreEqual(block, field2.SourceBlock);

            workspace = null;
        }

        [Test]
        public void TestAppendFieldString()
        {
            var workspace = new Workspace();
            var block = new Block(workspace);
            var input = new Input(Define.EConnection.DummyInput, "INPUT", block);
            var labelText = "label";
            
            Assert.AreEqual(0, input.FieldRow.Count);
            
            //actual test
            input.AppendField(labelText, "name");
            Assert.AreEqual(1, input.FieldRow.Count);
            Assert.AreEqual(typeof(FieldLabel), input.FieldRow[0].GetType());
            Assert.AreEqual(labelText, input.FieldRow[0].GetValue());
            Assert.AreEqual("name", input.FieldRow[0].Name);
            
            workspace = null;
        }

        [Test]
        public void TestAppendFieldPrefix()
        {
            var workspace = new Workspace();
            var block = new Block(workspace);
            var input = new Input(Define.EConnection.DummyInput, "INPUT", block);
            var prefix = new FieldLabel(null, "prefix");
            var field = new FieldLabel(null, "field");
            //field.PrefixField = prefix;
            
            //todo

            workspace = null;
        }

        [Test]
        public void TestAppendFieldSuffix()
        {
            var workspace = new Workspace();
            var block = new Block(workspace);
            var input = new Input(Define.EConnection.DummyInput, "INPUT", block);
            
            //todo
            
            workspace = null;
        }

        [Test]
        public void TestInsertFieldAtSimple()
        {
            var workspace = new Workspace();
            var block = new Block(workspace);
            var input = new Input(Define.EConnection.DummyInput, "INPUT", block);

            var before = new FieldLabel(null, "before");
            var after = new FieldLabel(null, "after");
            var between = new FieldLabel("name", "between");
            input.AppendField(before);
            input.AppendField(after);
            
            //preconditions
            Assert.AreEqual(2, input.FieldRow.Count);
            Assert.AreEqual(before, input.FieldRow[0]);
            Assert.AreEqual(after, input.FieldRow[1]);

            input.InsertFieldAt(1, between);
            Assert.AreEqual(3, input.FieldRow.Count);
            Assert.AreEqual(before, input.FieldRow[0]);
            Assert.AreEqual(between, input.FieldRow[1]);
            Assert.AreEqual("name", input.FieldRow[1].Name);
            Assert.AreEqual(after, input.FieldRow[2]);
        }
        
        [Test]
        public void TestInsertFieldAtString()
        {
            var workspace = new Workspace();
            var block = new Block(workspace);
            var input = new Input(Define.EConnection.DummyInput, "INPUT", block);
            
            var before = new FieldLabel(null, "before");
            var after = new FieldLabel(null, "after");
            string labelText = "label";
            input.AppendField(before);
            input.AppendField(after);
            
            //preconditions
            Assert.AreEqual(2, input.FieldRow.Count);
            Assert.AreEqual(before, input.FieldRow[0]);
            Assert.AreEqual(after, input.FieldRow[1]);

            input.InsertFieldAt(1, labelText, "name");
            Assert.AreEqual(3, input.FieldRow.Count);
            Assert.AreEqual(before, input.FieldRow[0]);
            Assert.AreEqual(typeof(FieldLabel), input.FieldRow[1].GetType());
            Assert.AreEqual(labelText, input.FieldRow[1].GetValue());
            Assert.AreEqual("name", input.FieldRow[1].Name);
            Assert.AreEqual(after, input.FieldRow[2]);
        }

        [Test]
        public void TestInsertFieldAtPrefx()
        {
            //todo: 
        }

        [Test]
        public void TestInsertFieldAtSuffix()
        {
            //todo:
        }
    }
}
