
# Creating a New HK Scene from Unity

This loosely documents how to add a new scene into Hollow Knight based on my experience developing Casino Knight.
I'm largely following the tutorial listed in the [Modding API docs](https://radiance.synthagen.net/apidocs/_images/NewScene.html) but using a slightly updated tool.
These docs were created back in 2020, so its been a while since its been redocumented.

## TLDR

- Create a new scene in HKWorldEdit2
- Either create a TileMap from scratch by using an empty and 2D edge colliders or copy one from another scene and modify (prefered)
- Add in a an object that represents the enterance / exit with a child object that is a Harzard respawn marker
- Save scene and export it using the Asset Bundle Browser
- In your mod on init load asset bundle and create a new custom scene object using Stachel
- Add a OnSceneChange hook and when the scene is changed to our custom scene set things up including reset materials, initialize gates, etc.

## Programs Needed

- [Unity 2020.2.2f1](https://unity.com/releases/editor/archive) to edit the scene. Note that we need this exact version to probably match what was used to make the game. Do not install the visual studio editor. Keep the version you installed following the [modding docs](https://prashantmohta.github.io/ModdingDocs/getting-started.html). Note that Hollow Knight was once built with Unity 2017 but later updated to 2020.
- [HKWorldEdit2](https://github.com/nesrak1/HKWorldEdit2) Unity project that has all the scenes and assets ripped from Hollow Knight. Much easier to use this instead of dealing with dependency errors or an assert ripper.
- [Asset Bundler Browser](https://github.com/Unity-Technologies/AssetBundles-Browser) to pack up unity assets into a bundle that can then be loaded by the mod. The tool inside the HKWorld edit doesn't work. Install this extension by downloading the release then installing it via the unity package manager.
- [Visual Studio](https://prashantmohta.github.io/ModdingDocs/getting-started.html) set up and a base mod created. Make sure it loads inside hollow knight. Make sure the `csproj` is configured for any dependencies.
- [Satchel](https://prashantmohta.github.io/ModdingDocs/getting-started.html) provides some convinient functions to use when creating new scenes.

## Creating a New Scene in Unity

1. Open the HKWorldEdit2 folder with Unity 2022. This should open without any errors.
2. We will use an existing TileMap from a HK scene as the base for our new scene. Find a scene to start off with, I will be using the mapper's shop room.
3. Open up the scene, then with your scene highlighted in hierarchy viewer, click right and save the scene as your custom scene for the mod (Eg. CustomScene).
4. Delete everything but the "TileMap Render Data", "TileMap" and Gate Objects. The Render Data will have child objects that will have `Edge Colliders 2D` on them. These are the level bounds.
5. Modify the level bounds to your needs, then save.

## Exporting the New Scene

1. Open the asset bundle browser via Window > AssetBundler Browser
2. Drag the scene object into the left side of the pop-up window, this should create a bundle. The primatives should pop up in the browser on the right.
3. Click the build tab, select os and location (with a mod set up this should be your resources folder).
    - If you a re-building, you may need to click "Force rebuild" under the advanced options
4. This will create a set of four files:
    - Resources
    - Resources.manifest
    - customscene
    - customscene.manifest
    
    The only one we really need is the customscene one which should be the only file with an actual memory footprint.

## Loading a New Scene in HK

1. In your mod folder, move the asset bundle into the \<Mod Name\>/src/Resources
2. This folder is created by the HK mod extension template so is already set up to include the `csproj` config file.
    But if not add something like the following in:
    ```xml
    <ItemGroup>
        <EmbeddedResource Include="Resources\*" />
    </ItemGroup>
    ```
3. With scenes we want to load the scene immediately so it will be registered by the game as a new scene:

    ```csharp
    string sceneResourceName = "<Mod Name>.Resources.customscene";
    Assembly asm = Assembly.GetExecutingAssembly();
    using (Stream s = asm.GetManifestResourceStream(sceneResourceName))
    {
        byte[] buffer = new byte[s.Length];
        s.Read(buffer, 0, buffer.Length);
        s.Dispose();
        Log.Debug("Loading bundle: " + sceneResourceName);
        casinoBundle = AssetBundle.LoadFromMemory(buffer);
    }
    sceneNamePath = casinoBundle.GetAllScenePaths()[0];
    ```

4. Now we want to create a custom scene object using [Satchel core](https://github.com/PrashantMohta/Satchel/blob/master/Core.cs#L177) which will help manage / setup key objects needed for the new scene to function.
This does require the use of a `GameObject refTileMap` and `GameObject refSceneManager` [preloaded objects](https://prashantmohta.github.io/ModdingDocs/preloads.html) from other scenes.
Use Unity for help locate these objects and add these to the main modding class:

    ```csharp
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        ...
        GameObject refTileMap =  preloadedObjects["Room_mapper"]["TileMap"];
        GameObject refSceneManager =  preloadedObjects["Room_mapper"]["_SceneManager"];

        casinoInterior = satchelCore.GetCustomScene("CasinoScene", refTileMap, refSceneManager);
        casinoInterior.OnLoaded += SceneOnload;

        CustomSceneManagerSettings settings = new SceneUtils.CustomSceneManagerSettings(refSceneManager.GetComponent<SceneManager>());
        casinoInterior.Config(40, 25, settings);
    }

    public override List<(string, string)> GetPreloadNames()
    {
        return new List<(string, string)>
        {
            ("Room_mapper","TileMap"),
            ("Room_mapper","_SceneManager"),
        };
    }
    ```

5. We will also want to create a `activeSceneChanged` call back to then set up the custom scene if we transition to it:

```csharp
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        ...

        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
    }

    ...

    private void OnSceneChange(Scene from, Scene to)
    {
        var currentScene = to.name;
        if (currentScene == "CasinoScene")
        {
            Log("We have entered our custom scene!")
        }
    }

```

6. At this point the steps are reseting the materials of the objects in the scene then setting up the gates.
Gates are a bit involved, so I will leave that for another document or someone else.