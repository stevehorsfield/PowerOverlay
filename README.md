# PowerOverlay

PowerOverlay is a utility to help speed repetitive tasks.

It is a configurable button menu of 24 buttons in a 5x5 grid. Each button can:

* Switch menu
* Perform one or more actions, such as:
  * Launch a program
  * Activate a window
  * Reposition a window (maximise, minimize, restore, fixed position, layout algorithm)
  * Send text to a window
  * Send keys, including control sequences to a window

Application windows can be targeted based on window title or application name or both.

Menus can be selected based on the current application when the overlay is activated (Win+F2).

Individual buttons can be styled or hidden.

![image](https://user-images.githubusercontent.com/5338720/177738522-0afee2c2-66ba-4170-baa1-48517bd93b6e.png)

![image](https://user-images.githubusercontent.com/5338720/177740068-336c1156-cd6b-49f6-9e49-deffebc82816.png)

Lots of configuration options. For example, for layout positioning:

![image](https://user-images.githubusercontent.com/5338720/177740479-1422a1fb-44fa-4329-bb98-c4d4001b904b.png)

Settings can be saved and loaded as JSON. Changes to the current state are automatically saved into a local cache and reloaded on launch.
