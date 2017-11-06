using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace PTGame.Blockly
{
    /// <summary>
    /// An optional function that is called to validate any constraints on what the user entered. 
    /// Takes the new text as an argument and returns either the accepted text, a replacement text, or null to abort  the change.
    /// </summary>
    public delegate string FieldValidator(string text);
    
    /// <summary>
    /// field model
    /// inherit from Observable, where int is the updated field value
    /// </summary>
    public abstract class Field : Observable<string>
    {
        /// <summary>
        /// Abstract class for an editable field.
        /// </summary>
        /// <param name="fieldName">The unique name of the field, usually defined in json block.</param>
        protected Field(string fieldName)
        {
            this.Name = fieldName;
        }

        /// <summary>
        /// Maximum characters of text to display before adding an ellipsis.
        /// </summary>
        protected const int MAX_DISPLAY_LENGTH = 50;

        /// <summary>
        /// Name of field, Unique within each block. Static labels are usually unnamed
        /// Can't be changed different with that defined in block!!!!!
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Visible text to display.
        /// </summary>
        protected string mText;

        /// <summary>
        /// Block this field is attached to. Starts as null,then in set in init.
        /// </summary>
        public Block SourceBlock { get; protected set; }

        /// <summary>
        /// Validation function called when user edits an editable field.
        /// </summary>
        protected FieldValidator mValidator;

        /// <summary>
        /// None-breaking space
        /// </summary>
        public const char NBSP = '\u00A0';

        /// <summary>
        /// Prefix labels
        /// </summary>
        public Field PrefixField;
        
        /// <summary>
        /// suffix labels
        /// </summary>
        public Field SuffixField;

        /// <summary>
        /// Attach this field to a block
        /// </summary>
        /// <param name="block"></param>
        public virtual void SetSourceBlock(Block block)
        {
            if (SourceBlock == block) 
                return;
            if (SourceBlock != null)
                throw new Exception("Field already bound to a block, can't bound to another block");
            this.SourceBlock = block;
        }

        /// <summary>
        /// Sets a new validation function for editable fields.
        /// </summary>
        public void SetValidator(FieldValidator handler)
        {
            this.mValidator = handler;
        }

        /// <summary>
        /// Gets the validation function for editable fields.
        /// </summary>
        public FieldValidator GetValidator()
        {
            return this.mValidator;
        }

        /// <summary>
        /// Validates a change.  Does nothing.  Subclasses may override this.
        /// </summary>
        protected virtual string ClassValidator(string text)
        {
            return text;
        }

        /// <summary>
        /// Calls the validation function for this field, as well as all the validation
        /// function for the field's class and its parents.
        /// </summary>
        public string CallValidator(string text)
        {
            string classResult = ClassValidator(text);
            if (classResult == null)
                return null;

            text = classResult;

            FieldValidator userValidator = GetValidator();
            if (userValidator != null)
            {
                string userResult = userValidator(text);
                if (userResult == null)
                {
                    // User validator rejects value.  Game over.
                    return null;
                }
                text = userResult;
            }
            
            return text;
        }

        /// <summary>
        /// Get the text from this field as displayed on screen.  
        /// May differ from getText due to ellipsis, and other formatting.
        /// </summary>
        public string GetDisplayText()
        {
            string text = mText;
            
            if (string.IsNullOrEmpty(text))
                return NBSP.ToString();

            if (text.Length > MAX_DISPLAY_LENGTH)
                text = text.Substring(0, MAX_DISPLAY_LENGTH - 2) + '\u2026';
            text = text.Replace(' ', NBSP);
            if (SourceBlock.RTL)
                text += '\u200F';
            return text;
        }

        /// <summary>
        /// Get the text from this field.
        /// </summary>
        public virtual string GetText()
        {
            return mText;
        }

        /// <summary>
        /// Set the text in this field.  Trigger a rerender of the source block.
        /// </summary>
        public virtual void SetText(string newText)
        {
            if (string.IsNullOrEmpty(newText) || string.Equals(newText, mText))
            {
                // No change if null.
                return;
            }

            mText = newText;
            //notify observers
            FireUpdate(mText);
        }
        
        /// <summary>
        /// By default there is no difference between the human-readable text and
        /// the language-neutral values. Subclasses (such as dropdown) may define this.
        /// </summary>
        /// <returns></returns>
        public virtual string GetValue()
        {
            return GetText();
        }
        
        /// <summary>
        /// By default there is no difference between the human-readable text and 
        /// the language-neutral values. Subclasses (such as drop down) maydefine this.
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void SetValue(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                // No change if null.
                return;
            }

            var oldValue = this.GetValue();
            if (string.Equals(oldValue,newValue))
                return;
            
            this.SetText(newValue);
        }
                
        /// <summary>
        /// Dispose of all Dom objects belonging to this editable field.
        /// </summary>
        public virtual void Dispose()
        {
            this.SourceBlock = null;
            this.mValidator = null;
        }
    }
}