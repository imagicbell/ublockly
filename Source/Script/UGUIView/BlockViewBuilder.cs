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
using UnityEngine;
using UnityEngine.UI;

namespace UBlockly.UGUI
{
    /// <summary>
    /// automatically build block view from block model
    /// both at runtime and in editor mode  
    /// </summary>
    public class BlockViewBuilder
    {
        //pivot top-left
        private static Vector2 BLOCK_PIVOT = new Vector2(0, 1);

        //anchor top-left
        private static Vector2 BLOCK_ANCHOR = new Vector2(0, 1);

        static void UniformRectTransform(RectTransform rectTrans)
        {
            rectTrans.pivot = BLOCK_PIVOT;
            rectTrans.anchorMin = rectTrans.anchorMax = BLOCK_ANCHOR;
            rectTrans.anchoredPosition3D = Vector3.zero;
            rectTrans.localScale = Vector3.one;
            rectTrans.localRotation = Quaternion.identity;
        }

        static T AddViewComponent<T>(GameObject viewObj) where T : BaseView
        {
            T view = viewObj.GetComponent<T>();
            if (view == null)
                view = viewObj.AddComponent<T>();
            view.InitComponents();
            return view;
        }

        public static GameObject BuildBlockView(Block block)
        {
            GameObject blockPrefab = BlockViewSettings.Get().PrefabRoot;
            if (block.OutputConnection != null) 
                blockPrefab = BlockViewSettings.Get().PrefabRootOutput;
            else if (block.PreviousConnection != null && block.NextConnection != null) 
                blockPrefab = BlockViewSettings.Get().PrefabRootPrevNext;
            else if (block.PreviousConnection != null)
                blockPrefab = BlockViewSettings.Get().PrefabRootPrev;
            else if (block.NextConnection != null) 
                blockPrefab = BlockViewSettings.Get().PrefabRootNext;

            GameObject blockObj = GameObject.Instantiate(blockPrefab);
            blockObj.name = "Block_" + block.Type;
            RectTransform blockTrans = blockObj.GetComponent<RectTransform>();
            UniformRectTransform(blockTrans);
            
            //blockview script
            BlockView blockView = AddViewComponent<BlockView>(blockObj);
            
            //block view's background image
            blockView.AddBgImage(blockObj.GetComponent<Image>());
            
            //block view's childs: connection, lineGroup
            Transform mutatorEntry = null;
            foreach (Transform child in blockTrans)
            {
                string childName = child.name.ToLower();
                if (childName.StartsWith("connection"))
                {
                    //connection node views
                    ConnectionView conView = AddViewComponent<ConnectionView>(child.gameObject);
                    blockView.AddChild(conView, 0);
                    
                    if (childName.EndsWith("output")) conView.ConnectionType = Define.EConnection.OutputValue;
                    else if (childName.EndsWith("prev")) conView.ConnectionType = Define.EConnection.PrevStatement;
                    else if (childName.EndsWith("next")) conView.ConnectionType = Define.EConnection.NextStatement;

                    //connection node view background color
                    Image image = child.GetComponent<Image>();
                    if (image != null) blockView.AddBgImage(image);
                }
                else if (childName.Equals("linegroup"))
                {
                    UniformRectTransform(child as RectTransform);
                    //lineGroup view
                    LineGroupView groupView = AddViewComponent<LineGroupView>(child.gameObject);
                    blockView.AddChild(groupView);
                }
                else if (childName.Equals("mutator_entry"))
                {
                    mutatorEntry = child;
                }
            }
            
            //check if has mutator entry
            if (mutatorEntry == null)
                throw new Exception("There should be a mutator_entry image under block view prefab");
            if (block.Mutator == null || !block.Mutator.NeedEditor)
                GameObject.DestroyImmediate(mutatorEntry.gameObject);
            else
                blockView.GetLineGroup(0).ReservedStartX = ((RectTransform) mutatorEntry).rect.width + BlockViewSettings.Get().ContentSpace.x;

            //block view's input views, including field's views
            BuildInputViews(block, blockView);

            //block view's layout, build from the very first field
            blockView.BuildLayout();
            
            //default background color
            blockView.ChangeBgColor(Color.blue);
            
            return blockObj;
        }

        public static LineGroupView BuildNewLineGroup(BlockView blockView)
        {
            GameObject groupObj = new GameObject("LineGroup");
            RectTransform groupTrans = groupObj.AddComponent<RectTransform>();
            groupTrans.SetParent(blockView.transform);
            UniformRectTransform(groupTrans);

            LineGroupView groupView = AddViewComponent<LineGroupView>(groupObj);
            blockView.AddChild(groupView);

            return groupView;
        }

        /// <summary>
        /// Build Input views, including field views, and calucate block size
        /// including dispose old input views with disposed input models
        /// </summary>
        public static void BuildInputViews(Block block, BlockView blockView)
        {
            bool inputsInline = block.GetInputsInline();
            LineGroupView groupView = blockView.GetLineGroup(0); 
            
            //1. check dispose old inputviews
            List<InputView> oldInputViews = blockView.GetInputViews();
            foreach (InputView view in oldInputViews)
            {
                if (!block.InputList.Contains(view.Input))
                {
                    view.UnBindModel();
                    GameObject.DestroyImmediate(view.gameObject);
                }
            }
            
            //2. build new inputviews
            for (int i = 0; i < block.InputList.Count; i++)
            {
                Input input = block.InputList[i];
                
                // build new line group view 
                bool newLine = i > 0 &&
                               (!inputsInline ||
                                input.Connection != null && input.Connection.Type == Define.EConnection.NextStatement);
                if (newLine)
                {
                    groupView = blockView.GetLineGroup(i);
                    if (groupView == null)
                        groupView = BuildNewLineGroup(blockView);
                }

                // build input view
                bool needBuild = true;
                foreach (InputView view in oldInputViews)
                {
                    if (view.Input == input)
                    {
                        needBuild = false;
                        if (view.Parent == null)
                        {
                            //bug fixed: this view may be removed from parent by removing its old previous sibling
                            groupView.AddChild(view);
                        }
                        else if (view.Parent != groupView)
                        {
                            //bug fixed: need to remove from the original groupview, maybe it's different now
                            view.Parent.RemoveChild(view);
                            groupView.AddChild(view);
                        }
                        break;
                    }
                }
                if (needBuild)
                {
                    InputView inputView = BuildInputView(input, groupView, blockView);
                    groupView.AddChild(inputView, newLine ? 0 : i);
                    if (Application.isPlaying)
                    {
                        inputView.BindModel(input);
                    }
                    else
                    {
                        //static build block only needs to bind fields' model for initializing fields' properties
                        for (int j = 0; j < inputView.Childs.Count; j++)
                        {
                            FieldView fieldView = inputView.Childs[j] as FieldView;
                            if (fieldView != null)
                                fieldView.BindModel(input.FieldRow[j]);
                        }
                    }
                }
            }
            
            //3. dispose group view without children
            for (int i = blockView.Childs.Count - 1; i >= 0; i--)
            {
                BaseView view = blockView.Childs[i];
                if (view.Type == ViewType.LineGroup && view.Childs.Count == 0)
                    GameObject.DestroyImmediate(view.gameObject);
            }
        }

        public static InputView BuildInputView(Input input, LineGroupView groupView, BlockView blockView)
        {
            GameObject inputPrefab;
            ConnectionInputViewType viewType;
            if (input.Type == Define.EConnection.NextStatement)
            {
                inputPrefab = BlockViewSettings.Get().PrefabInputStatement;
                viewType = ConnectionInputViewType.Statement;
            }
            else if (input.SourceBlock.InputList.Count > 1 && input.SourceBlock.GetInputsInline())
            {
                inputPrefab = BlockViewSettings.Get().PrefabInputValueSlot;
                viewType = ConnectionInputViewType.ValueSlot;
            }
            else
            {
                inputPrefab = BlockViewSettings.Get().PrefabInputValue;
                viewType = ConnectionInputViewType.Value;
            }
            
            GameObject inputObj = GameObject.Instantiate(inputPrefab);
            inputObj.name = "Input_" + (!string.IsNullOrEmpty(input.Name) ? input.Name : "");
            RectTransform inputTrans = inputObj.GetComponent<RectTransform>();
            inputTrans.SetParent(groupView.transform, false);
            UniformRectTransform(inputTrans);
            
            Transform conInputTrans = inputTrans.GetChild(0);

            InputView inputView = AddViewComponent<InputView>(inputObj);
            inputView.AlignRight = input.Align == Define.EAlign.Right;
            
            // build child field views of this input view 
            List<Field> fields = input.FieldRow;
            foreach (Field field in fields)
            {
                FieldView fieldView = BuildFieldView(field);
                inputView.AddChild(fieldView);
                RectTransform fieldTrans = fieldView.GetComponent<RectTransform>();
                UniformRectTransform(fieldTrans);
            }

            if (input.Type == Define.EConnection.DummyInput)
            {
                //dummy input doesn't need to have a connection point
                GameObject.DestroyImmediate(conInputTrans.gameObject);
            }
            else
            {
                ConnectionInputView conInputView = AddViewComponent<ConnectionInputView>(conInputTrans.gameObject);
                conInputView.ConnectionType = input.Type;
                conInputView.ConnectionInputViewType = viewType;
                inputView.AddChild(conInputView);

                conInputView.BgImage.raycastTarget = false;
                if (viewType != ConnectionInputViewType.ValueSlot)
                    blockView.AddBgImage(conInputView.BgImage);
            }

            return inputView;
        }

        public static FieldView BuildFieldView(Field field)
        {
            FieldView fieldView = null;
            GameObject fieldObj = null;
            
            Type fieldType = field.GetType();
            if (fieldType == typeof(FieldLabel))
            {
                fieldObj = GameObject.Instantiate(BlockViewSettings.Get().PrefabFieldLabel);
                fieldView = AddViewComponent<FieldLabelView>(fieldObj);
            }
            else if (fieldType == typeof(FieldTextInput))
            {
                fieldObj = GameObject.Instantiate(BlockViewSettings.Get().PrefabFieldInput);
                fieldView = AddViewComponent<FieldInputView>(fieldObj);
            }
            else if (fieldType == typeof(FieldVariable))
            {
                fieldObj = GameObject.Instantiate(BlockViewSettings.Get().PrefabFieldVariable);
                fieldView = AddViewComponent<FieldVariableView>(fieldObj);
            }
            else if (fieldType == typeof(FieldColour))
            {
                fieldObj = GameObject.Instantiate(BlockViewSettings.Get().PrefabFieldButton);
                fieldView = AddViewComponent<FieldColorView>(fieldObj);
            }
            else if (fieldType == typeof(FieldImage))
            {
                fieldObj = GameObject.Instantiate(BlockViewSettings.Get().PrefabFieldImage);
                fieldView = AddViewComponent<FieldImageView>(fieldObj);
            }
            else if (fieldType == typeof(FieldCheckbox))
            {
                fieldObj = GameObject.Instantiate(BlockViewSettings.Get().PrefabFieldCheckbox);
                fieldView = AddViewComponent<FieldCheckboxView>(fieldObj);
            }
            else
            {
                fieldObj = GameObject.Instantiate(BlockViewSettings.Get().PrefabFieldButton);
                fieldView = AddViewComponent<FieldButtonView>(fieldObj);
            }

            fieldObj.name = !string.IsNullOrEmpty(field.Name) ? "Field_" + field.Name : fieldObj.name.Substring(0, fieldObj.name.IndexOf("(Clone)"));
            return fieldView;
        }
    }
}
