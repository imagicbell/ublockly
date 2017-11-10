using System;
using System.Text;
using System.Xml;

namespace UBlockly
{
    /// <summary>
    /// Mutator for dynamic modifying options for field dropdown
    /// no mutation process, but record the real options to xml, and read them from xml as well.
    /// </summary>
    public class DropdownOptionsMutator : Mutator
    {
        private const string OPTION_NAME = "options";
        
        public override bool NeedEditor
        {
            get { return false; }
        }

        public override XmlElement ToXml()
        {
            FieldDropdown dropdown = mBlock.GetField("MENU") as FieldDropdown;
            if (dropdown == null)
                throw new Exception("FieldDropDown \"MENU\" not found.");
            
            StringBuilder sb = new StringBuilder();
            foreach (FieldDropdownMenu option in dropdown.GetOptions())
            {
                sb.AppendFormat("{0},{1};", option.Text, option.Value);
            }
            
            XmlElement xmlElement = XmlUtil.CreateDom("mutation");
            xmlElement.SetAttribute("options", sb.ToString());
            
            return xmlElement;
        }

        public override void FromXml(XmlElement xmlElement)
        {
            FieldDropdown dropdown = mBlock.GetField("MENU") as FieldDropdown;
            if (dropdown == null)
                throw new Exception("FieldDropDown \"MENU\" not found.");

            if (xmlElement.HasAttribute("options"))
            {
                string optionText = xmlElement.GetAttribute("options");
                string[] options = optionText.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries);
                if (options.Length % 2 != 0)
                    throw new Exception(string.Format("Xml serialization for mutation {0} is damaged", MutatorId));

                FieldDropdownMenu[] menu = new FieldDropdownMenu[options.Length / 2];
                for (int i = 0; i < menu.Length; i++)
                {
                    menu[i].Text = options[i * 2];
                    menu[i].Value = options[i * 2 + 1];
                }
                dropdown.SetOptions(menu);
            }
        }
    }
}
