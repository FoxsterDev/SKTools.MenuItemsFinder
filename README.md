# MenuItemsFinder

What problem solved:
There are a lot of menuitems in a project. Sometimes their menupathes are changed. And you are angry that you need to search. Or menuitempath has a very big construction and difficult to navigate

Features:
1) Navigate by key input
2) Execute some menuitem, just press the button with menuitem. If the menuitem has validating method. And it could not be validated you will see an info-dialog and the menuitem button will have gray color
3) Starred your specific items and to have fast access at the top of a finder
4) Set custom name for specific menuitem. Navigating will be by key + custom name (Profit: You don't need to remember place of MenuItems like "Window/General/Profiler" or "Tools/Category/SubCategory/SubSubCategory/WhatExactlyINeed" and it could be changed in a new version of Unity or a plugin.)
5) Open a file that contains menuitem. It is usefull when you want edit a menuitem. if a menuitem places into Assembly-CShar-Editor(or firstpass) or Editor AssemblyDefinition and their scripts are placed into Assets folder. You can open file that contains the menuitem. Else you can open a location with assembbly that contains it
6) Managing missed menuitems. When the project does not contain prefs menuitems
7) Supporting recompiling, it does not need to close and reopen a finder window. After recompiling a state of finder wndow will be restored.
7) All settings will be saved onto the prefs json file. You can reuse it 
* I tested on Mac and need recheck on Window. 

Unity: 
supported 5.6 and above
checked 2018.2.8f1, 2017.3.1p4 ,2017.2.2p4, 5.6.5p1, scripting runtime version net 3.5

This tool provide an actual collection of all menuitems from the current project and fast navigate by key words. There is hotkey MacOs= (cmd + shift+ m) , Win = (ctrl + shift+M). Or you can click SKTools->MenuItems Finder after it
You see this:
<div align="center">
    <img src="https://github.com/FoxsterDev/SKTools.MenuItemsFinder/blob/master/Editor%20Resources/view2.png"/>
</div>

Please enter some name of menuitem in searchtoolbar. You see all menuitems from the project that contains your input. After closing window it will ne saved to prefs. Prefs is a json local file and placed inside MenuItemsFinder Editor folder , and your prefs could be available on different machines. 

Download unity package [here](https://github.com/FoxsterDev/SKTools/blob/master/UnityPackages/MenuItemsFinder_v0.1.4.unitypackage)
