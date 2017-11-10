using NUnit.Framework;
using UnityEngine;

namespace UBlockly.Test
{
    public class FieldVariableTest
    {
        private Workspace workspace = null;
        private void Setup()
        {
            workspace = new Workspace();
        }

        private void TearDown()
        {
            workspace.Dispose();
        }

        private Block MockBlock()
        {
            Block block = new Block();
            block.Workspace = workspace;
            return block;
        }
        
        [Test]
        public void TestFieldVariable_Constructor()
        {
            var ws = new Workspace();
            var fieldVar = new FieldVariable(null, "name1");
            Assert.AreEqual("name1", fieldVar.GetText());
            ws.Dispose();
        }

        [Test]
        public void TestFieldVariable_SetValueMatchId()
        {
            Setup();
            
            workspace.CreateVariable("name2", null, "id2");
            var fieldVar = new FieldVariable(null, "name1");
            fieldVar.SetSourceBlock(MockBlock());
            
            fieldVar.SetValue("id2");
            Assert.AreEqual("name2", fieldVar.GetText());
            Assert.AreEqual("id2", fieldVar.GetRealValue());
            
            TearDown();
        }

        [Test]
        public void TestFieldVariable_SetValueMatchName()
        {
            Setup();

            workspace.CreateVariable("name2", null, "id2");
            var fieldVar = new FieldVariable(null, "name1");
            fieldVar.SetSourceBlock(MockBlock());
            
            fieldVar.SetValue("name2");
            Assert.AreEqual("name2", fieldVar.GetText());
            Assert.AreEqual("id2", fieldVar.GetRealValue());
            
            TearDown();
        }
        
        [Test]
        public void TestFieldVariable_SetValueNoVariable()
        {
            Setup();

            var fieldVar = new FieldVariable(null, "name1");
            fieldVar.SetSourceBlock(MockBlock());
            
            fieldVar.SetValue("id1");
            Assert.AreEqual("id1", fieldVar.GetText());
            Assert.AreEqual("id1", fieldVar.GetRealValue());
            
            TearDown();
        }

        [Test]
        public void TestFieldVariable_DropdownCreateVariablesExist()
        {
            Setup();

            workspace.CreateVariable("name1", "", "id1");
            workspace.CreateVariable("name2", "", "id2");
            
            var fieldVar = new FieldVariable(null, "name1");
            fieldVar.SetSourceBlock(MockBlock());
            fieldVar.SetText("name1");

            FieldDropdownMenu[] options = fieldVar.GetOptions();
            Assert.AreEqual("name1", options[0].Text);
            Assert.AreEqual("id1", options[0].Value);
            Assert.AreEqual("name2", options[1].Text);
            Assert.AreEqual("id2", options[1].Value);
            
            TearDown();
        }      
    }
}
