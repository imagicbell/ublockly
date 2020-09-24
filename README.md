# UBlocky

See the follows for details:

1. http://imagicbell.github.io/unity/2017/10/11/blockly-one.html
2. http://imagicbell.github.io/unity/2017/10/14/blockly-two.html
3. http://imagicbell.github.io/unity/2017/10/22/blockly-three.html
4. http://imagicbell.github.io/unity/2017/10/31/blockly-four.html


Have fun with [2 game demo](https://github.com/imagicbell/ublocklygame).

## HOW TO RUN

1. open **Examples** in Unity.
2. Hit menu **UBlockly/Build Block Prefabs** and wait to finish prefabs building.
3. open scene **UGUIDemo**.
4. Hit play and have fun. 

There offered two UI themes: google blockly style and scratch style. Default is scratch(I prefer~).

![](Doc/google_ui.png)

![](Doc/sratch_ui.png)

To change themes:

1. Drag the prefab **UBlocklyData/Toolboxs/Prefabs/ClassicToolbox** under the scene object **Canvas/Workspace** (Make sure the scene **UGUIDemo** is opened).

2. Delete or hide the other theme prefab **ScratchToolbox**.

3. Attach the **ClassicToolbox** to the component **WorkspaceView** on scene object **Workspace**.

4. Hit play and have a look.

   ![](Doc/changeui_1.png)

![](Doc/changeui_2.png)



## How to add customerized blocks

1. Add json definition for the block. Please follow the format of the built-in blocks under `ublockly/Source/JsonBlocks/`.

   1. If you want to add a block in the existing category, just add the json definition in the existing json files.

   2. The `type` of the block must follow the format: `[category]_nameOfBlock`. 

      e.g. `coroutine_wait_time`, `coroutine` is the name of the category, which is also the name of the json file; `wait_time` is the name of the block.

   3. If you want to add a new category. 

      1. Create a new json file anywhere you want. Then add its reference to the `BlockJsonFiles` in `BlockResSettings`. Of course you can remove a category from it.
      2. If you want the toolbox to show the newly added category, you should add the new category to the toolbox config file named `toolbox_default`. Of course, you can customized the toolbox config by adding your own config file.
      3. Add a translation for the new category name in `en.json`. Or you can create your own i18n file for a new language.

      ![](Doc/blockressettings.png)

2. Interprete the execute code for the newly added block in C#. Please follow the built-in blocks under `ublockly/Source/Script/CodeDB/CSharp`.

   

## Contributors

Greatly appreciate the help by:

[liangxie](https://github.com/liangxieq)

[dtknowlove](https://github.com/dtknowlove)

