using Newtonsoft.Json.Linq;
using UnityEngine;
using NUnit.Framework;

namespace UBlockly.Test
{
    public class JsonTest
    {
        /// <summary>
        /// Ensure a block can be instantiated from a JSON definition.
        /// </summary>
        [Test]
        public void TestJsonMinimal()
        {
            var BLOCK_TYPE = "test_json_minimal";

            var workspace = new Workspace();

            JObject blockDefine = new JObject();
            blockDefine["type"] = BLOCK_TYPE;

            JArray jsonArray = new JArray()
            {
                blockDefine
            };


            Blockly.DefineBlocksWithJsonArray(jsonArray);

            Block block = workspace.NewBlock(BLOCK_TYPE);
            Assert.IsTrue(string.Equals(BLOCK_TYPE, block.Type));

            block.Dispose();
            workspace.Dispose();
            Blockly.Dispose();
        }

        /// <summary>
        /// Ensure message0 creates an input.
        /// </summary>
        [Test]
        public void TestJsonMessage0()
        {
            var BLOCK_TYPE = "test_json_message0";
            var MESSAGE0 = "message0";

            var workspace = new Workspace();

            var blockData = new JObject();
            blockData["type"] = BLOCK_TYPE;
            blockData["message0"] = MESSAGE0;

            Blockly.DefineBlocksWithJsonArray(new JArray()
            {
                blockData
            });

            Block block = workspace.NewBlock(BLOCK_TYPE);

            Assert.AreEqual(1, block.InputList.Count);
            Assert.AreEqual(1, block.InputList[0].FieldRow.Count);
            var textField = block.InputList[0].FieldRow[0];
            Assert.IsTrue(textField is FieldLabel);
            Assert.IsTrue(string.Equals(MESSAGE0, textField.GetText()));

            block.Dispose();
            workspace.Dispose();
            Blockly.Dispose();
        }


        /// <summary>
        /// Ensure message1 creates a new input.
        /// </summary>
        [Test]
        public void TestJsonMessage1()
        {
            var BLOCK_TYPE = "test_json_message1";
            var MESSAGE0 = "message0";
            var MESSAGE1 = "message1";

            var workspace = new Workspace();

            var blockData = new JObject();
            blockData["type"] = BLOCK_TYPE;
            blockData["message0"] = MESSAGE0;
            blockData["message1"] = MESSAGE1;

            Blockly.DefineBlocksWithJsonArray(new JArray()
            {
                blockData
            });

            Block block = workspace.NewBlock(BLOCK_TYPE);
            Assert.AreEqual(2, block.InputList.Count);

            Assert.AreEqual(1, block.InputList[0].FieldRow.Count);
            var textField = block.InputList[0].FieldRow[0];
            Assert.IsTrue(textField is FieldLabel);
            Assert.IsTrue(string.Equals(MESSAGE0, textField.GetText()));

            Assert.AreEqual(1, block.InputList[1].FieldRow.Count);
            textField = block.InputList[1].FieldRow[0];
            Assert.IsTrue(textField is FieldLabel);
            Assert.IsTrue(string.Equals(MESSAGE1, textField.GetText()));

            block.Dispose();
            workspace.Dispose();
            Blockly.Dispose();
        }
    }
}
