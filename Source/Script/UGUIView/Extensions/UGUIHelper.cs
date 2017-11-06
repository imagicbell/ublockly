using UnityEngine;
using UnityEngine.UI;

namespace PTGame.Blockly.UGUI
{
    public static class UGUIHelper
    {
        /// <summary>
        /// Calculate the width of UI text
        /// </summary>
        /// <param name="textComponent">the UI.Text component where the text is put into</param>
        /// <param name="text">The text for calculation</param>
        public static int CalculateTextWidth(this Text textComponent, string text)
        {
            int width = 0;
            Font font = textComponent.font;
            int fontSize = textComponent.fontSize;
            font.RequestCharactersInTexture(text, fontSize, textComponent.fontStyle);
            CharacterInfo characterInfo;
            for (int i = 0; i < text.Length; i++)
            {
                font.GetCharacterInfo(text[i], out characterInfo, fontSize);
                width += characterInfo.advance;
            }
            return width;
        }
    }
}