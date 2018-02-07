/****************************************************************************

Copyright 2016 sophieml1989@gmail.com

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

****************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace UBlockly
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
        /// Name of field, Unique within each block. Static labels are usually unnamed
        /// Can't be changed different with that defined in block!!!!!
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Visible text to display.
        /// </summary>
        protected string mText;
        
        /// <summary>
        /// if the field content is an image, configured in block json 
        /// </summary>
        public bool IsImage { get; protected set; }

        /// <summary>
        /// Block this field is attached to. Starts as null,then in set in init.
        /// </summary>
        public Block SourceBlock { get; protected set; }

        /// <summary>
        /// Validation function called when user edits an editable field.
        /// </summary>
        protected FieldValidator mValidator;

        /// <summary>
        /// Prefix labels
        /// </summary>
        public Field PrefixField;
        
        /// <summary>
        /// suffix labels
        /// </summary>
        public Field SuffixField;

        private string mType = null;
        /// <summary>
        /// Type of the field, unique for each field class, defined in block json
        /// </summary>
        public string Type
        {
            get
            {
                if (string.IsNullOrEmpty(mType))
                {
                    Type classType = this.GetType();
                    MethodInfo methodInfo = classType.GetMethod("CreateFromJson", BindingFlags.Static | BindingFlags.NonPublic);
                    if (methodInfo == null)
                        throw new Exception(string.Format(
                            "There is no static function \"CreateFromJson\" for creating field in class {0}. Please add one", classType));

                    var attrs = methodInfo.GetCustomAttributes(typeof(FieldCreatorAttribute), false);
                    if (attrs.Length == 0)
                        throw new Exception(string.Format(
                            "You should add a \"FieldCreatorAttribute\" to static method \"CreateFromJson\" in class {0}.", classType));
                    mType = ((FieldCreatorAttribute) attrs[0]).FieldType;
                }
                return mType;
            }
        }

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
