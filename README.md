# MenuItemsFinder

What problem solved:
There are a lot of menuitems in a project. Sometimes their menupathes are changed. And you are angry that you need to search. Or menuitempath has a very big construction and difficult to navigate

Profit: You don't need to remember place of MenuItems like "Window/Analisis/Profiler" or "Tools/Category/SubCategory/SubSubCategory/WhatExactlyINeed" and it could be changed in a new version of Unity or a plugin.
Unity: checked 2018.2.8f1, scripting runtime version net 3.5

This tool provide an actual collection of all menuitems from the current project and fast navigate by key words. There is hotkey MacOs= (cmd + shift+ m) , Win = (ctrl + shift+M). Or you can click SKTools->MenuItems Finder after it
You see this:
<div align="center">
    <img src="https://github.com/FoxsterDev/SKTools.MenuItemsFinder/blob/master/Editor%20Resources/view2.png"/>
</div>

Please enter some name of menuitem in searchtoolbar. You see all menuitems from the project that contains your input. After closing window it will ne saved to prefs. Prefs is a json local file and placed inside MenuItemsFinder Editor folder , and your prefs could be available on different machines. 


