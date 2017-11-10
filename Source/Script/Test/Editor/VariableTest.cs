using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace UBlockly.Test
{
    /// <summary>
    /// test including: variableModel, variableMap
    /// </summary>
    public class VariableTest
    {
        #region Variable Model Test
        
        [Test]
        public void TestInitVariableModel()
        {
            Workspace workspace = new Workspace();    
            
            //test trivial
            VariableModel variable = new VariableModel(workspace, "test", "test_type", "test_id");
            Assert.AreEqual("test", variable.Name, "init variable with name \'test\'.");
            Assert.AreEqual("test_type", variable.Type, "init variable with type \'test_type\'.");
            Assert.AreEqual("test_id", variable.ID, "init variable with id \'test_id\'.");

            //test null type
            variable = new VariableModel(workspace, "test", null, "test_id");
            Assert.AreEqual("", variable.Type, "init variable with null type.");

            //test UndefinedType
            variable = new VariableModel(workspace, "test", "", "test_id");
            Assert.AreEqual("", variable.Type, "init variable with undefined type.");

            variable = new VariableModel(workspace, "test", "test_type", null);
            Assert.AreEqual("test", variable.Name);
            Assert.AreEqual("test_type", variable.Type);
            Assert.NotNull(variable.ID, "init variable with null id");

            variable = new VariableModel(workspace, "test", "test_type", "");
            Assert.AreEqual("test", variable.Name);
            Assert.AreEqual("test_type", variable.Type);
            Assert.NotNull(variable.ID, "init variable with undefined id");

            variable = new VariableModel(workspace, "test");
            Assert.AreEqual("test", variable.Name, "init variable with only name");
            Assert.AreEqual("", variable.Type, "init variable with only name");
            Assert.NotNull(variable.ID, "init variable with only name");

            workspace.Dispose();
        }

        #endregion
        
        
        #region Variable Map Test

        private VariableMap mVariableMap;
        private Workspace mWorkspace;
        
        void TestVariableMapSetup()
        {
            mWorkspace = new Workspace();
            mVariableMap = new VariableMap(mWorkspace);
        }

        void TestVariableMapTearDown()
        {
            mWorkspace.Dispose();
            mVariableMap = null;
        }

        [Test]
        public void TestGetVariable()
        {
            TestVariableMapSetup();

            VariableModel var_1 = mVariableMap.CreateVariable("name1", "type1", "id1");
            VariableModel var_2 = mVariableMap.CreateVariable("name2", "type1", "id2");
            VariableModel var_3 = mVariableMap.CreateVariable("name3", "type2", "id3");

            VariableModel result_1 = mVariableMap.GetVariable("name1");
            VariableModel result_2 = mVariableMap.GetVariable("name2");
            VariableModel result_3 = mVariableMap.GetVariable("name3");
            
            Assert.AreEqual(var_1, result_1);
            Assert.AreEqual(var_2, result_2);
            Assert.AreEqual(var_3, result_3);

            //test variable not found
            VariableModel result_null = mVariableMap.GetVariable("name4");
            Assert.Null(result_null);
            
            TestVariableMapTearDown();
        }

        [Test]
        public void TestGetVariableById()
        {
            TestVariableMapSetup();

            VariableModel var_1 = mVariableMap.CreateVariable("name1", "type1", "id1");
            VariableModel var_2 = mVariableMap.CreateVariable("name2", "type1", "id2");
            VariableModel var_3 = mVariableMap.CreateVariable("name3", "type2", "id3");

            VariableModel result_1 = mVariableMap.GetVariableById("id1");
            VariableModel result_2 = mVariableMap.GetVariableById("id2");
            VariableModel result_3 = mVariableMap.GetVariableById("id3");
            
            Assert.AreEqual(var_1, result_1);
            Assert.AreEqual(var_2, result_2);
            Assert.AreEqual(var_3, result_3);

            //test variable not found
            VariableModel result_null = mVariableMap.GetVariable("id4");
            Assert.Null(result_null);
            
            TestVariableMapTearDown();
        }

        [Test]
        public void TestCreateVariable()
        {
            TestVariableMapSetup();

            mVariableMap.CreateVariable("name1", "type1", "id1");
            TestHelper.CheckVariableValues(mVariableMap, "name1", "type1", "id1");
            
            //test create variable already exists----------------------------------------
            //Expect that when the variable already exists, the variableMap_ is unchanged.
            Assert.AreEqual(1, mVariableMap.GetVariableTypes().Count);
            Assert.AreEqual(1, mVariableMap.GetAllVariables().Count);

            mVariableMap.CreateVariable("name1");
            TestHelper.CheckVariableValues(mVariableMap, "name1", "type1", "id1");
            
            Assert.AreEqual(1, mVariableMap.GetVariableTypes().Count);
            Assert.AreEqual(1, mVariableMap.GetAllVariables().Count);
            //---------------------------------------------------------------------------
            
            //test create variable null and undefined type-------------------------------
            mVariableMap.CreateVariable("name2", null, "id2");
            mVariableMap.CreateVariable("name3", "", "id3");
            TestHelper.CheckVariableValues(mVariableMap, "name2", "", "id2");
            TestHelper.CheckVariableValues(mVariableMap, "name3", "", "id3");
            //---------------------------------------------------------------------------
            
            //test create variable null id-----------------------------------------------
            mVariableMap.CreateVariable("name4", "type4", null);
            Assert.NotNull(mVariableMap.GetVariable("name4").ID);
            //---------------------------------------------------------------------------
            
            //test create VariableId already exists--------------------------------------
            Assert.Throws<Exception>(() =>
            {
                mVariableMap.CreateVariable("name5", "type5", "id1");
            });
            //---------------------------------------------------------------------------
            
            //test create Variable mismatched Id and type--------------------------------
            Assert.Throws<Exception>(() =>
            {
                mVariableMap.CreateVariable("name1", "type2", "id1");
            });
            
            Assert.Throws<Exception>(() =>
            {
                mVariableMap.CreateVariable("name1", "type1", "id2");
            });
            //---------------------------------------------------------------------------
            
            //test create variable two same types----------------------------------------
            mVariableMap.CreateVariable("name6", "type6", "id6");
            mVariableMap.CreateVariable("name7", "type6", "id7");
            TestHelper.CheckVariableValues(mVariableMap, "name6", "type6", "id6");
            TestHelper.CheckVariableValues(mVariableMap, "name7", "type6", "id7");
            //---------------------------------------------------------------------------
            
            TestVariableMapTearDown();
        }

        [Test]
        public void TestGetVariablesOfType()
        {
            TestVariableMapSetup();
            
            var var_1 = mVariableMap.CreateVariable("name1", "type1", "id1");
            var var_2 = mVariableMap.CreateVariable("name2", "type1", "id2");
            mVariableMap.CreateVariable("name3", "type2", "id3");
            mVariableMap.CreateVariable("name4", "type3", "id4");

            var result_array_1 = mVariableMap.GetVariablesOfType("type1");
            var result_array_2 = mVariableMap.GetVariablesOfType("type5");

            TestHelper.IsEqualArrays<VariableModel>(new[] {var_1, var_2}, result_array_1.ToArray());
            TestHelper.IsEqualArrays(new VariableModel[] { }, result_array_2.ToArray());
            
            //get variables of type null-------------------------------------------------
            var var_6 = mVariableMap.CreateVariable("name6", "", "id6");
            var var_7 = mVariableMap.CreateVariable("name7", "", "id7");
            var var_8 = mVariableMap.CreateVariable("name8", null, "id8");
            var result_array_3 = mVariableMap.GetVariablesOfType(null);
            TestHelper.IsEqualArrays<VariableModel>(new[] {var_6, var_7, var_8}, result_array_3.ToArray());
            //---------------------------------------------------------------------------
            
            //get variables of type deleted----------------------------------------------
            var var_9 = mVariableMap.CreateVariable("name9", "type9", "id9");
            mVariableMap.DeleteVariable(var_9);
            TestHelper.IsEqualArrays(new VariableModel[] {}, mVariableMap.GetVariablesOfType("type9").ToArray());
            //---------------------------------------------------------------------------
            
            //get variables of type does not exist---------------------------------------
            TestHelper.IsEqualArrays(new VariableModel[] {}, mVariableMap.GetVariablesOfType("type0").ToArray());
            //---------------------------------------------------------------------------
            
            TestVariableMapTearDown();
        }

        [Test]
        public void TestGetVariableTypes()
        {
            TestVariableMapSetup();
            
            mVariableMap.CreateVariable("name1", "type1", "id1");
            mVariableMap.CreateVariable("name2", "type1", "id2");
            mVariableMap.CreateVariable("name3", "type2", "id3");
            mVariableMap.CreateVariable("name4", "type3", "id4");
            TestHelper.IsEqualArrays(new[] {"type1", "type2", "type3"}, mVariableMap.GetVariableTypes().ToArray());
            
            //get variable types none----------------------------------------------------
            mVariableMap.Clear();
            TestHelper.IsEqualArrays(new string[] { }, mVariableMap.GetVariableTypes().ToArray());
            //---------------------------------------------------------------------------
            
            TestVariableMapTearDown();
        }

        [Test]
        public void TestGetAllVariables()
        {
            TestVariableMapSetup();
            
            var var_1 = mVariableMap.CreateVariable("name1", "type1", "id1");
            var var_2 = mVariableMap.CreateVariable("name2", "type1", "id2");
            var var_3 = mVariableMap.CreateVariable("name3", "type2", "id3");
            TestHelper.IsEqualArrays(new[] {var_1, var_2, var_3}, mVariableMap.GetAllVariables().ToArray());
            
            mVariableMap.Clear();
            TestHelper.IsEqualArrays(new VariableModel[] { }, mVariableMap.GetAllVariables().ToArray());
            
            TestVariableMapTearDown();
        }

        #endregion
    }
}
