/****************************************************************************
 * Copyright (c) 2017 maoling@putao.com
****************************************************************************/
using System.Collections.Generic;
using System.Xml;

namespace UBlockly
{
    /// <summary>
    /// This mutator supports the procedures_ifreturn block, configuring the presence of the
    /// return value input and the block's disabled state.
    /// </summary>
    public class ProcedureIfReturnMutator : Mutator
    {
        public override bool NeedEditor
        {
            get { return true; }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            // TODO: Find parent definition. Set enabled/disabled state. Update return value input.
        }

        public override XmlElement ToXml()
        {
            return null;
        }

        public override void FromXml(XmlElement xmlElement)
        {
        }
    }
}
