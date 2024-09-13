using Modding;
using Satchel.BetterMenus;

namespace CasinoKnight
{
    public static class ModMenu
    {
        public static Menu PrepareMenu( ModToggleDelegates modtoggledelegates)
        {
            //Create a new MenuRef if it's not null
            Menu MenuRef = new Menu(
                name: "Casino Knight", //the title of the menu screen, it will appear on the top center of the screen 
                elements: new Element[]
                {
                        new TextPanel("Options",800f),

                        new HorizontalOption(
                        "Sound Effects", "Toggle gamblecore sounds",
                        new string[]{"On","Off"},
                        (setting) => { CasinoKnight.GS.EnableSFX = (setting == 0); },
                        () => { if( CasinoKnight.GS.EnableSFX) { return 0; } else { return 1;} }
                    )
                }
            );

            //uses the GetMenuScreen function to return a menuscreen that MAPI can use. 
            //The "modlistmenu" that is passed into the parameter can be any menuScreen that you want to return to when "Back" button or "esc" key is pressed 
            return MenuRef;
        }
    }
}