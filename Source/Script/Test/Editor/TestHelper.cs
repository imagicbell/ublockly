using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace UBlockly.Test
{
    /// <summary>
    /// TODO:rename 2 BlocklyTestHelper
    /// </summary>
    public static class TestHelper
    {
        /// <summary>
        /// Check that two arrays have the same content.
        /// </summary>
        public static void IsEqualArrays<T>(T[] array1, T[] array2)
        {
            Assert.AreEqual(array1.Length, array2.Length);
            for (int i = 0; i < array1.Length; i++)
            {
                Assert.AreEqual(array1[i], array2[i]);
            }
        }

        /// <summary>
        /// Check that two list have the same content.
        /// </summary>
        public static void IsEqualList<T>(List<T> list1, List<T> list2)
        {
            Assert.AreEqual(list1.Count, list2.Count);
            for (int i = 0; i < list1.Count; i++)
            {
                Assert.AreEqual(list1[i], list2[i]);
            }
        }

        /// <summary>
        /// Check if a variable with the given values exists.
        /// </summary>
        /// <param name="container">{Blockly.Workspace|Blockly.VariableMap}</param>
        public static void CheckVariableValues(object container, string name, string type, string id)
        {
            VariableMap variableMap = container as VariableMap;
            if (variableMap == null)
            {
                Workspace workspace = container as Workspace;
                if (workspace != null)
                    variableMap = workspace.VariableMap;
            }

            if (variableMap == null)
                throw new Exception("calls TestHelper.CheckVariableValues, argument container must be Worspace or VariableMap");

            VariableModel variable = variableMap.GetVariableById(id);
            Assert.NotNull(variable);
            Assert.AreEqual(name, variable.Name);
            Assert.AreEqual(type, variable.Type);
            Assert.AreEqual(id, variable.ID);
        }
    }
}
