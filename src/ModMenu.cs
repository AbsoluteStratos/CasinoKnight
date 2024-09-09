using GlobalEnums;
using UnityEngine.UI;
using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using Satchel.BetterMenus;

using System;
using UnityEngine;
using MenuButton = Satchel.BetterMenus.MenuButton;
using InControl;
using StratosLogging;

namespace CasinoKnight
{
    public static class ModMenu
    {

        public static MenuScreen GetMenuScreen(Menu MenuRef, MenuScreen modListMenu, ModToggleDelegates? modtoggledelegates)
        {
            //Create a new MenuRef if it's not null
            MenuRef ??= new Menu(
                        name: "Treasure Hunt", //the title of the menu screen, it will appear on the top center of the screen 
                        elements: new Element[]
                        {
                            new MenuButton(
                            name: "My Logging Button",
                            description: "A menu button",
                            submitAction: (_) => Log.Info("A button was pressed")),
                        }
            );

            //uses the GetMenuScreen function to return a menuscreen that MAPI can use. 
            //The "modlistmenu" that is passed into the parameter can be any menuScreen that you want to return to when "Back" button or "esc" key is pressed 
            return MenuRef.GetMenuScreen(modListMenu);
        }
    }
}