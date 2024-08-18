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

namespace StudyKnight
{
    public static class ModMenu
    {
        public static MappableKey myKey;
        public static InControl.PlayerAction Action;

        // This is based on the debug UI menu
        // https://github.com/TheMulhima/HollowKnight.DebugMod/blob/master/Source/ModMenu.cs#L12
        // Typically people use Satchel to not deal with this complexity
        // https://prashantmohta.github.io/ModdingDocs/Satchel/BetterMenus/better-menus.html
        public static MenuBuilder CreateMenuScreen(MenuScreen modListMenu)
        {
            Action<MenuSelectable> CancelAction = selectable => UIManager.instance.UIGoToDynamicMenu(modListMenu);
            return new MenuBuilder(UIManager.instance.UICanvas.gameObject, "Fart Knight Menu")
                .CreateTitle("Fart Knight Menu", MenuTitleStyle.vanillaStyle)
                .CreateContentPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 903f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -60f)
                    )
                ))
                .CreateControlPane(RectTransformData.FromSizeAndPos(
                    new RelVector2(new Vector2(1920f, 259f)),
                    new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -502f)
                    )
                ))
                .SetDefaultNavGraph(new GridNavGraph(1))
                .AddContent(
                    RegularGridLayout.CreateVerticalLayout(105f),
                    c =>
                    {
                        c.AddKeybind(
                                "Scrub Key",
                                StudyKnight.GS.KeyBinds.ScrubScene,
                                new KeybindConfig
                                {
                                    Label = "Scrub Key"
                                },
                                out var myKey
                            )
                        .AddKeybind(
                                "Study Animation Key",
                                StudyKnight.GS.KeyBinds.StudyAnimation,
                                new KeybindConfig
                                {
                                    Label = "Study Key"
                                },
                                out var myKey2
                            );
                    })
                .AddControls(
                    new SingleContentLayout(new AnchoredPosition(
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f),
                        new Vector2(0f, -64f)
                    )), c => c.AddMenuButton(
                        "BackButton",
                        new MenuButtonConfig
                        {
                            Label = "Back",
                            CancelAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                            SubmitAction = _ => UIManager.instance.UIGoToDynamicMenu(modListMenu),
                            Style = MenuButtonStyle.VanillaStyle,
                            Proceed = true
                        }));
        }
    }
}