using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;


namespace UBlockly.Test
{
	public class WorkspaceTest
	{
		private Workspace mWorkspace;

		public WorkspaceTest()
		{
			Define();
		}
		void Define()
		{
			JObject blockData = new JObject();
			blockData["type"] = "get_var_block";
			blockData["message0"] = "%1";
			
			JObject message0Value = new JObject();
			message0Value["type"] = "field_variable";
			message0Value["name"] = "VAR";

			blockData["args0"] = new JArray()
			{
				message0Value
			};


			Blockly.DefineBlocksWithJsonArray(new JArray()
			{
				blockData
			});
		}

		void WorkspaceTestSetup()
		{
			mWorkspace = new Workspace(new Workspace.WorkspaceOptions());
		}

		void WorkspaceTestTeardown()
		{
			mWorkspace.Dispose();
			mWorkspace = null;
		}

		Block CreateMockBlock(string variableName)
		{
			var block = mWorkspace.NewBlock("get_var_block");
			block.InputList[0].FieldRow[0].SetValue(variableName);
			return block;
		}
		
		[Test]
		public void TestEmptyWorkspace()
		{
			WorkspaceTestSetup();
			
			Assert.AreEqual(0,mWorkspace.GetTopBlocks(true).Count);
			Assert.AreEqual(0,mWorkspace.GetTopBlocks(false).Count);
			Assert.AreEqual(0,mWorkspace.GetAllBlocks().Count);
			mWorkspace.Clear();
			Assert.AreEqual(0,mWorkspace.GetTopBlocks(true).Count);
			Assert.AreEqual(0,mWorkspace.GetTopBlocks(false).Count);
			Assert.AreEqual(0,mWorkspace.GetAllBlocks().Count);
			
			WorkspaceTestTeardown();
		}

		[Test]
		public void TestFlatWorkspace()
		{
			WorkspaceTestSetup();
			
			var blockA = mWorkspace.NewBlock("");
			
			Assert.AreEqual(1,mWorkspace.GetTopBlocks(true).Count);
			Assert.AreEqual(1,mWorkspace.GetTopBlocks(false).Count);
			Assert.AreEqual(1,mWorkspace.GetAllBlocks().Count);
			
			var blockB = mWorkspace.NewBlock("");
			
			Assert.AreEqual(2,mWorkspace.GetTopBlocks(true).Count);
			Assert.AreEqual(2,mWorkspace.GetTopBlocks(false).Count);
			Assert.AreEqual(2,mWorkspace.GetAllBlocks().Count);
			
			blockA.Dispose();
			
			Assert.AreEqual(1,mWorkspace.GetTopBlocks(true).Count);
			Assert.AreEqual(1,mWorkspace.GetTopBlocks(false).Count);
			Assert.AreEqual(1,mWorkspace.GetAllBlocks().Count);
			
			mWorkspace.Clear();
			
			Assert.AreEqual(0,mWorkspace.GetTopBlocks(true).Count);
			Assert.AreEqual(0,mWorkspace.GetTopBlocks(false).Count);
			Assert.AreEqual(0,mWorkspace.GetAllBlocks().Count);
			
			WorkspaceTestTeardown();
		}

		/* Block limitation feature is not needed currently
		[Test]
		public void TestMaxBlocksWorkspace()
		{
			WorkspaceTestSetup();

			var blockA = mWorkspace.NewBlock("");
			var blockB = mWorkspace.NewBlock("");
			
			Assert.AreEqual(Blockly.INFINITY,mWorkspace.RemainningCapacity());
			mWorkspace.Options.MaxBlocks = 3;
			Assert.AreEqual(1,mWorkspace.RemainningCapacity());
			mWorkspace.Options.MaxBlocks = 2;
			Assert.AreEqual(0,mWorkspace.RemainningCapacity());
			mWorkspace.Options.MaxBlocks = 1;
			Assert.AreEqual(-1,mWorkspace.RemainningCapacity());
			mWorkspace.Options.MaxBlocks = 0;
			Assert.AreEqual(-2,mWorkspace.RemainningCapacity());
			mWorkspace.Clear();
			Assert.AreEqual(0,mWorkspace.RemainningCapacity());
			
			WorkspaceTestTeardown();
		}*/

		[Test]
		public void GetWorkspaceById()
		{
			var workspaceA = new Workspace();
			var workspaceB = new Workspace();
			
			Assert.AreEqual(workspaceA,Workspace.GetByID(workspaceA.Id));
			Assert.AreEqual(workspaceB,Workspace.GetByID(workspaceB.Id));
			Assert.AreEqual(null,Workspace.GetByID("I do not exist."));
			workspaceA.Dispose();
			Assert.AreEqual(null,Workspace.GetByID(workspaceA.Id));
			Assert.AreEqual(workspaceB,Workspace.GetByID(workspaceB.Id));
			
			workspaceB.Dispose();
			workspaceA.Dispose();
		}

		[Test]
		public void GetBlockById()
		{
			WorkspaceTestSetup();

			var blockA = mWorkspace.NewBlock("");
			var blockB = mWorkspace.NewBlock("");

			Assert.AreEqual(blockA, mWorkspace.GetBlockById(blockA.ID));
			Assert.AreEqual(blockB, mWorkspace.GetBlockById(blockB.ID));
			Assert.AreEqual(null, mWorkspace.GetBlockById("I do not exist."));
			
			blockA.Dispose();
			
			Assert.AreEqual(null,mWorkspace.GetBlockById(blockA.ID));
			Assert.AreEqual(blockB,mWorkspace.GetBlockById(blockB.ID));
			
			mWorkspace.Clear();
			
			Assert.AreEqual(null, mWorkspace.GetBlockById(blockB.ID));
			
			WorkspaceTestTeardown();
		}

		[Test]
		public void TestDeleteVariableInternalTrivial()
		{
			WorkspaceTestSetup();

			var var1 = mWorkspace.CreateVariable("name1", "type1", "id1");
			mWorkspace.CreateVariable("name2", "type2", "id2");
			CreateMockBlock("name1");
			CreateMockBlock("name1");
			CreateMockBlock("name2");
			
			mWorkspace.DeleteVariableInternal(var1);
			var variable = mWorkspace.GetVariable("name1");
			var blockVarName = mWorkspace.TopBlocks[0].GetVars()[0];
			Assert.IsTrue(variable == null);
			CheckVariableValues(mWorkspace,"name2","type2","id2");
			Assert.IsTrue(string.Equals("name2", blockVarName));
			
			WorkspaceTestTeardown();
		}

		/// <summary>
		/// TODO(marisaleung): Test the alert for deleting a variable that is a procedure.
		/// </summary>
		[Test]
		public void TestUpdateVariableStoreTrivialNoClear()
		{
			WorkspaceTestSetup();
			mWorkspace.CreateVariable("name1", "type1", "id1");
			mWorkspace.CreateVariable("name2", "type2", "id2");
			
			mWorkspace.UpdateVariableStore(false,new List<string>()
			{
				"name1","name2"
			});
				
			CheckVariableValues(mWorkspace,"name1","type1","id1");
			CheckVariableValues(mWorkspace,"name2","type2","id2");
			WorkspaceTestTeardown();
		}

		[Test]
		public void TestVariableStorenameNotInvariableMapNoClear()
		{
			WorkspaceTestSetup();

			mWorkspace.CreateVariable("name1", "", "1");
			mWorkspace.UpdateVariableStore(false,new List<string>
			{
				"name1"
			});
			CheckVariableValues(mWorkspace,"name1","","1");
			
			WorkspaceTestTeardown();
		}

		[Test]
		public void TestUpdateVariableStoreClearAndAllInUse()
		{
			WorkspaceTestSetup();

			mWorkspace.CreateVariable("name1", "type1", "id1");
			mWorkspace.CreateVariable("name2", "type2", "id2");
			
			mWorkspace.UpdateVariableStore(true,new List<string>()
			{
				"name1","name2"
			});
			
			CheckVariableValues(mWorkspace,"name1","type1","id1");
			CheckVariableValues(mWorkspace,"name2","type2","id2");
			
			WorkspaceTestTeardown();
		}
		
		[Test]
		public void TestUpdateVariableStoreClearAndOneInUse()
		{
			WorkspaceTestSetup();

			mWorkspace.CreateVariable("name1", "type1", "id1");
			mWorkspace.CreateVariable("name2", "type2", "id2");
			
			mWorkspace.UpdateVariableStore(true,new List<string>()
			{
				"name1"
			});
			
			CheckVariableValues(mWorkspace,"name1","type1","id1");
			var variable = mWorkspace.GetVariable("name2");
			Assert.IsNull(variable);
			
			WorkspaceTestTeardown();
		}

		/*[Test]
		public void TestAddTopBlockTrivialFlyoutIsTrue()
		{
			WorkspaceTestSetup();

			mWorkspace.IsFlyout = true;

			var block = CreateMockBlock(null);
			mWorkspace.RemoveTopBlock(block);
//			setUpMockMethod(mockControl_, Blockly.Variables, 'allUsedVariables', [block],
//				[['name1']]);
//			setUpMockMethod(mockControl_, Blockly.utils, 'genUid', null, ['1']);

			mWorkspace.AddTopBlock(block);
			// TODO:需要Mock 所以没法测试
//			var varList = block.GetVars();
//			var variable = mWorkspace.GetVariable(varList[0]);
//
//			CheckVariableValues(mWorkspace, variable.Name, variable.Type, variable.ID);
			WorkspaceTestTeardown();
		}*/

		[Test]
		public void TestClearTrivial()
		{
			WorkspaceTestSetup();
			mWorkspace.CreateVariable("name1", "type1", "id1");
			mWorkspace.CreateVariable("name2", "type2", "id2");
//			setUpMockMethod(mockControl_, Blockly.Events, 'setGroup', [true, false],
//			null);
			mWorkspace.Clear();
			var topBlocksLength = mWorkspace.TopBlocks.Count;
			var varMapLength = mWorkspace.VariableMap.mVariableMap.Count;
			Assert.AreEqual(0,topBlocksLength);
			Assert.AreEqual(0,varMapLength);
			WorkspaceTestTeardown();
		}

		[Test]
		public void TestClearNoVariables()
		{
			// Expect 'renameVariable' to create new variable with newName.
//			setUpMockMethod(mockControl_, Blockly.Events, 'setGroup', [true, false],
//			null);
			WorkspaceTestSetup();
			mWorkspace.Clear();
			var topBlocksLength = mWorkspace.TopBlocks.Count;
			var varMapLength = mWorkspace.VariableMap.mVariableMap.Count;
			Assert.AreEqual(0,topBlocksLength);
			Assert.AreEqual(0,varMapLength);
			WorkspaceTestTeardown();
		}

		[Test]
		public void TestRenameVariableNoBlocks()
		{
			// Expect 'renameVariable" to create new variable with newName.
			WorkspaceTestSetup();
			var oldName = "name1";
			var newName = "name2";
			// Mocked setGroup to ensure oly one call to the mocked genUid.
//			setUpMockMethod(mockControl_, Blockly.Events, 'setGroup', [true, false],
//			null);
//			setUpMockMethod(mockControl_, Blockly.utils, 'genUid', null, ['1']);
			mWorkspace.RenameVariable(oldName,newName);
			var variable = mWorkspace.GetVariable(newName);
			CheckVariableValues(mWorkspace,"name2","",variable.ID);
			var oldVariable = mWorkspace.GetVariable(oldName);
			Assert.IsTrue(oldVariable == null);
			WorkspaceTestTeardown();
		}

		[Test]
		public void TestRenameVariableSameNameNoBlocks()
		{
			// Exper 'renameVariable' to create new variable with newName.
			WorkspaceTestSetup();
			var name = "name1";
			mWorkspace.CreateVariable(name, "type1", "id1");
			
			mWorkspace.RenameVariable(name,name);
			CheckVariableValues(mWorkspace,name,"type1","id1");
			WorkspaceTestTeardown();	
		} 
		
		[Test]
		public void TestRenameVariableOnlyOldNameBlockExists()
		{
			// Expect "renameVariable" to change oldName variable name to newName.
			WorkspaceTestSetup();
			var oldName = "name1";
			var newName = "name2";
			mWorkspace.CreateVariable(oldName, "type1", "id1");
			CreateMockBlock(oldName);

			mWorkspace.RenameVariable(oldName, newName);
			CheckVariableValues(mWorkspace,newName,"type1","id1");
			var variable = mWorkspace.GetVariable(oldName);
			var blockVarName = mWorkspace.TopBlocks[0].GetVars()[0];
			Assert.IsTrue(variable == null);
			Assert.IsTrue(string.Equals(newName,blockVarName));
			WorkspaceTestTeardown();
		}

		[Test]
		public void TestRenameVariableTwoVariablesSameType()
		{
			// Expect 'renameVariable' to change oldName variable name to newName.
			// Expect oldName block name to change to newName
			WorkspaceTestSetup();
			var oldName = "name1";
			var newName = "name2";
			mWorkspace.CreateVariable(oldName, "type1", "id1");
			mWorkspace.CreateVariable(newName, "type1", "id2");
			CreateMockBlock(oldName);
			CreateMockBlock(newName);
			
			mWorkspace.RenameVariable(oldName,newName);
			CheckVariableValues(mWorkspace,newName,"type1","id2");
			var variable = mWorkspace.GetVariable(oldName);
			var blockVarName1 = mWorkspace.TopBlocks[0].GetVars()[0];
			var blockVarName2 = mWorkspace.TopBlocks[1].GetVars()[0];
			Assert.IsTrue(variable == null);
			Assert.IsTrue(string.Equals(newName,blockVarName1));
			Assert.IsTrue(string.Equals(newName,blockVarName2));
			WorkspaceTestTeardown();
		}

		[Test]
		public void TestRenameVariableTwoVariablesDifferentType()
		{
			// Expect triggered error because of different types
			WorkspaceTestSetup();
			var oldName = "name1";
			var newName = "name2";
			mWorkspace.CreateVariable(oldName, "type1", "id1");
			mWorkspace.CreateVariable(newName, "type2", "id2");
			CreateMockBlock(oldName);
			CreateMockBlock(newName);
			
			mWorkspace.RenameVariable(oldName,newName);
			
			CheckVariableValues(mWorkspace,oldName,"type1","id1");
			CheckVariableValues(mWorkspace,newName,"type2","id2");
			var blockVarName1 = mWorkspace.TopBlocks[0].GetVars()[0];
			var blockVarName2 = mWorkspace.TopBlocks[1].GetVars()[0];
			Assert.IsTrue(string.Equals(oldName,blockVarName1));
			Assert.IsTrue(string.Equals(newName,blockVarName2));
			WorkspaceTestTeardown();
		}		

		[Test]
		public void TestRenameVariableOldCase()
		{
			// Expect triggered error because of different types
			WorkspaceTestSetup();
			var oldCase = "Name1";
			var newName = "name1";
			mWorkspace.CreateVariable(oldCase, "type1", "id1");
			CreateMockBlock(oldCase);
			
			mWorkspace.RenameVariable(oldCase,newName);
			CheckVariableValues(mWorkspace,newName,"type1","id1");
			var resultOldCase = mWorkspace.GetVariable(oldCase).Name;
			Assert.IsTrue(!string.Equals(oldCase,resultOldCase));
			WorkspaceTestTeardown();
		}

		[Test]
		public void TestRenameVariableTwoVariablesAndOldCase()
		{
			// Expect triggered error because of different types
			WorkspaceTestSetup();
			var oldName = "name1";
			var oldCase = "Name2";
			var newName = "name2";
			mWorkspace.CreateVariable(oldName, "type1", "id1");
			mWorkspace.CreateVariable(oldCase, "type1", "id2");
			CreateMockBlock(oldName);
			CreateMockBlock(oldCase);
			
			mWorkspace.RenameVariable(oldName,newName);
			
			CheckVariableValues(mWorkspace,newName,"type1","id2");
			var variable = mWorkspace.GetVariable(oldName);
			var resultOldCase = mWorkspace.GetVariable(oldCase).Name;
			var blockVarName1 = mWorkspace.TopBlocks[0].GetVars()[0];
			var blockVarName2 = mWorkspace.TopBlocks[1].GetVars()[0];
			Assert.IsTrue(variable == null);
			Assert.IsTrue(!string.Equals(oldCase,resultOldCase));
			Assert.IsTrue(string.Equals(newName,blockVarName1));
			Assert.IsTrue(string.Equals(newName,blockVarName2));
			WorkspaceTestTeardown();
		}
	

		/// <summary>
		/// Extra testing not requered for renameVariableById. It calls renameVariable
		/// and that has extensive testing.
		/// </summary>
		[Test]
		public void TestRenameVariableByIdTwoVariablesSameType()
		{
			// Expert 'renameVariableById' to change oldName variable name to newName.
			// Expert oldName block name to change to newName
			WorkspaceTestSetup();
			var oldName = "name1";
			var newName = "name2";
			mWorkspace.CreateVariable(oldName, "type1", "id1");
			mWorkspace.CreateVariable(newName, "type1", "id2");
			CreateMockBlock(oldName);
			CreateMockBlock(newName);
			
			mWorkspace.RenameVariableById("id1",newName);
			CheckVariableValues(mWorkspace,newName,"type1","id2");
			var variable = mWorkspace.GetVariable(oldName);
			var blockVarName1 = mWorkspace.TopBlocks[0].GetVars()[0];
			var blockVarName2 = mWorkspace.TopBlocks[1].GetVars()[0];
			Assert.IsTrue(variable == null);
			Assert.IsTrue(string.Equals(newName,blockVarName1));
			Assert.IsTrue(string.Equals(newName,blockVarName2));
			
			WorkspaceTestTeardown();
		}

		[Test]
		public void TestDeleteVariableTrivial()
		{
			WorkspaceTestSetup();

			mWorkspace.CreateVariable("name1", "type1", "id1");
			mWorkspace.CreateVariable("name2", "type1", "id2");
			CreateMockBlock("name1");
			CreateMockBlock("name2");
			
			mWorkspace.DeleteVariable("name1");
			CheckVariableValues(mWorkspace,"name2","type1","id2");
			var variable = mWorkspace.GetVariable("name1");
			var blockVarName = mWorkspace.TopBlocks[0].GetVars()[0];
			Assert.IsTrue(variable == null);
			Assert.IsTrue(string.Equals("name2", blockVarName));
			
			WorkspaceTestTeardown();
		}
		
		[Test]
		public void TestDeleteVariableByIDTrivial()
		{
			WorkspaceTestSetup();

			mWorkspace.CreateVariable("name1", "type1", "id1");
			mWorkspace.CreateVariable("name2", "type1", "id2");
			CreateMockBlock("name1");
			CreateMockBlock("name2");
			
			mWorkspace.DeleteVariableById("id1");
			Assert.AreEqual(mWorkspace.TopBlocks.Count, 1);
			CheckVariableValues(mWorkspace,"name2","type1","id2");
			var variable = mWorkspace.GetVariable("name1");
			var blockVarName = mWorkspace.TopBlocks[0].GetVars()[0];

			Assert.IsTrue(variable == null);
			Debug.Log(blockVarName);
			Assert.IsTrue(string.Equals("name2",blockVarName));
			
			WorkspaceTestTeardown();
		}

		// Test utilitys
		void CheckVariableValues(Workspace container, string name, string type, string id)
		{
			var varibale = container.GetVariableById(id);
			Assert.IsTrue(varibale != null);
			Assert.IsTrue(string.Equals(name,varibale.Name));
			Assert.IsTrue(string.Equals(type,varibale.Type));
			Assert.IsTrue(string.Equals(id,varibale.ID));
		}
	}
}
