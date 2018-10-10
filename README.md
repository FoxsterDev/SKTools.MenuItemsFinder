# MenuItemsFinder

What problem solved:
There are a lot of menuitems in a project. Sometimes their menupathes are changed. And you are angry that you need to search. Or menuitempath has a very big construction and difficult to navigate

Features:
1) Navigate by key input
2) Execute some menuitem, just press the button with menuitem
3) Starred your specific items and to have fast access at the top of a finder
4) Set custom name for specific menuitem. Navigating will be by key + custom name
5) Open a file that contains menuitem. It is usefull when you want edit a menuitem. if a menuitem places into Assembly-CShar-Editor(or firstpass) or Editor AssemblyDefinition and their scripts are placed into Assets folder. You can open file that contains the menuitem. Else you can open a location with assembbly that contains it
6) Managing missed menuitems. When the project does not contain prefs menuitems
7) Supporting recompiling, it does not need to close and reopen a finder window. After recompiling a state of finder wndow will be restored.
7) All settings will be saved onto the prefs json file. You can reuse it 

Profit: You don't need to remember place of MenuItems like "Window/Analisis/Profiler" or "Tools/Category/SubCategory/SubSubCategory/WhatExactlyINeed" and it could be changed in a new version of Unity or a plugin.
Unity: checked 2018.2.8f1, scripting runtime version net 3.5

This tool provide an actual collection of all menuitems from the current project and fast navigate by key words. There is hotkey MacOs= (cmd + shift+ m) , Win = (ctrl + shift+M). Or you can click SKTools->MenuItems Finder after it
You see this:
<div align="center">
    <img src="https://github.com/FoxsterDev/SKTools.MenuItemsFinder/blob/master/Editor%20Resources/view2.png"/>
</div>

Please enter some name of menuitem in searchtoolbar. You see all menuitems from the project that contains your input. After closing window it will ne saved to prefs. Prefs is a json local file and placed inside MenuItemsFinder Editor folder , and your prefs could be available on different machines. 


