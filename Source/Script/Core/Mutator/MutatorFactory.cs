/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace PTGame.Blockly
{
    public static class MutatorFactory
    {
        private static Dictionary<string, Type> mutatorDict = new Dictionary<string, Type>()
        {
            {"controls_if_mutator", typeof(IfElseMutator)},
            {"text_join_mutator", typeof(ItemListMutator)},
            {"lists_create_with_item_mutator", typeof(ItemListMutator)},
            {"math_is_divisibleby_mutator", typeof(MathIsDivisibleByMutator)},
            {"index_at_mutator", typeof(IndexAtMutator)},
            
            {"procedures_defnoreturn_mutator", typeof(ProcedureDefinitionMutator)},
            {"procedures_defreturn_mutator", typeof(ProcedureDefinitionMutator)},
            {"procedures_callnoreturn_mutator", typeof(ProcedureCallMutator)},
            {"procedures_callreturn_mutator", typeof(ProcedureCallMutator)},
            {"procedures_ifreturn_mutator", typeof(ProcedureIfReturnMutator)},
        };

        /// <summary>
        /// mutator factory method
        /// </summary>
        public static Mutator Create(string mutatorId)
        {
            Type type;
            if (mutatorDict.TryGetValue(mutatorId, out type))
            {
                Mutator mutator = Activator.CreateInstance(type) as Mutator;
                mutator.MutatorId = mutatorId;
                return mutator;
            }
            return null;
        }
    }
}