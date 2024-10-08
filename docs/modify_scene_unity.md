# Adding to HK Scenes from Unity

The following documents my efforts to modify an existing hollow knight scene via modifying it in Unity.
In my [previous mod](https://github.com/AbsoluteStratos/FartKnight) I used scripting to drop in a 2D panel and place a sprite on it, but it is pretty teadious even for a single object.
I'm largely following the tutorial listed in the [Modding API docs](https://radiance.synthagen.net/apidocs/_images/NewScene.html) but using a slightly updated tool.
These docs were created back in 2020, so its been a while since its been redocumented.

## TLDR

- Add objects into scene provided by HKWorldEdit2
- Make prefab of custom objects and export using Asset Bundle Browser 
- Add a OnSceneChange hook and load asset bundle when scene is the correct on
- Instantiate prefab with custom objects and reset materials

## Programs Needed

- [Unity 2020.2.2f1](https://unity.com/releases/editor/archive) to edit the scene. Note that we need this exact version to probably match what was used to make the game. Do not install the visual studio editor. Keep the version you installed following the [modding docs](https://prashantmohta.github.io/ModdingDocs/getting-started.html). Note that Hollow Knight was once built with Unity 2017 but later updated to 2020.
- [HKWorldEdit2](https://github.com/nesrak1/HKWorldEdit2) Unity project that has all the scenes and assets ripped from Hollow Knight. Much easier to use this instead of dealing with dependency errors or an assert ripper.
- [Asset Bundler Browser](https://github.com/Unity-Technologies/AssetBundles-Browser) to pack up unity assets into a bundle that can then be loaded by the mod. The tool inside the HKWorld edit doesn't work. Install this extension by downloading the release then installing it via the unity package manager.
- [Visual Studio](https://prashantmohta.github.io/ModdingDocs/getting-started.html) set up and a base mod created. Make sure it loads inside hollow knight. Make sure the `csproj` is configured for any dependencies.

## Modifying the Scene

1. Open the HKWorldEdit2 folder with Unity 2022. This should open without any errors.
2. Open a specific scene using HKEdit2 > Open Scene.
3. Lets add custom image into the scene. Add a new empty object into the scene via the hierarchy on the left and call this your mod. Think of this like a folder. This will be referred to as the "root". Place this empty at (0,0,0).
3. Then create another empty object inside that "folder" and add a "Sprite Render" component.
4. Place this child component in the scene where you want it.
5. Drag to root empty into your asset browser to create a prefab of it in the folder `Assets\Prefabs`.

## Exporting the New Object

1. In this case, because we are modifying a scene with a new object we will just bundle and export the object and place it into the scene of interest. Select the higher level empty created so its highlighted in the hierarchy window.
2. Note the position of the root object should still be (0,0,0).
3. Open the asset bundle browser via Window > AssetBundler Browser
4. Drag the prefab of the root object into the window, this should create a bundle named the prefab. The primatives should pop up in the browser on the right.
5. Click the build tab, select os and location (with a mod set up this should be your resources folder).
    - If you a re-building, you may need to click "Force rebuild" under the advanced options
6. This will create a set of four files:
    - Resources
    - Resources.manifest
    - root
    - root.manifest
    
    The only one we really need is the root one which should be the only file with an actual memory footprint.

## Loading the Scene in HK

1. In your mod folder, move the asset bundle into the \<Mod Name\>/src/Resources
2. This folder is created by the HK mod extension template so is already set up to include the `csproj` config file.
    But if not add something like the following in:
    ```xml
    <ItemGroup>
        <EmbeddedResource Include="Resources\*" />
    </ItemGroup>
    ```
3. Next we need to make a hook in the mode to trigger the placement of the mod in the scene you want. Looking at the [mod hooks api](https://hk-modding.github.io/api/api/Modding.ModHooks.html#events), we may consider using `ModHooks.SceneChanged` but this doesn't work currently.
    Instead we will use unity directly:

    ```csharp
    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Log("Initializing");
        Instance = this;
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChange;
        Log("Initialized");
    }

    private void OnSceneChange(Scene from, Scene to)
    {
        Log("Scene changed!! " + from.name + " to " + to.name);
    }
    ```

4. Here we can check name of `Scene to` to determine if we need to add the object from our bundle.
    One can use this little print out (with the debug mod) to figure out a scene name, this [google drive](https://drive.google.com/drive/folders/1VwVbCjU8uPV4V3cDu_Tr1TgEs01hMSFr) is also very useful.
    In the case of the dirthmouth the condition is the following:

    ```csharp
    public void OnSceneChange(Scene from, Scene to)
    {
        if (to.name == "Town"){

        }
    }
    ``` 

5. To load the asset bundle, we can use some of the code in the modding docs for adding scene (theres two options but heres one).
    Create a class variable for the `AssetBundle`, we only want to load this if its null.
    Note that the path to the resource is `<Mod name>.Resources.root`:

    ```csharp
    private AssetBundle ab = null; // Global variable in class

    ...
    if (ab == null)
    {
        string bundle_name = "<Mod name>.Resources.root";
        Assembly asm = Assembly.GetExecutingAssembly();
        using (Stream s = asm.GetManifestResourceStream(bundle_name))
        {
            byte[] buffer = new byte[s.Length];
            s.Read(buffer, 0, buffer.Length);
            s.Dispose();
            Log("Loading bundle: " + bundle_name);
            ab = AssetBundle.LoadFromMemory(buffer);
        }
    }
    ```

    Unsure what resources are available, use the following code to debug:
    
    ```csharp
    foreach (string name in asm.GetManifestResourceNames())
    {
        Log("Embedded resource: " + name);
    }
    ```

    For memory purposes we may want to unload the bundle when we exit the scene, this can be done with:

    ```csharp
    if (scene_2.name == "Town"){

    } else if (ab != null) {
        ab.Unload(true);
    }
    ```

6. This represents the bundled object, we will need to get assets out


    ```csharp
    // Note the path is based on where you saved the prefab
    objab = ab.LoadAsset<GameObject>("Assets/Prefabs/root.prefab");
    objab = GameObject.Instantiate(objab);
    objab.transform.position = Vector3.zero;
    objab.SetActive(true);
    ```

    Note that we set the position to zero, using the root empty placed at zero in unity allows us to not worry about placing the children objects.
    To find what assets are inside the bundle and where:

    ```csharp
    foreach (string name in ab.GetAllAssetNames())
    {
        Log("Asset: " + name);
    }
    ```

7. Now if you check the game, the object will likely appear but will be pink.
    This is a known bug documented in the [docs](https://radiance.synthagen.net/apidocs/_images/Assets.html?highlight=getexecutingassembly#using-our-loaded-stuff) which we can fix with the following:

    ```csharp
    for (int i = 0; i < objab.transform.childCount; i ++)
    {
        GameObject child = objab.transform.GetChild(i).gameObject;
        child.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
    }
    ```

    If your object has deeper children objects, you'll need to recursively do this.

8. Start up the game and the mod should be working with the object placed.

### Final Script

Putting this all together, your script will look something like the following:

> This won't actually run but rather gives you the steps.

```csharp
namespace MyFirstMod
{
    public class MyFirstMod: Mod 
    {
        private AssetBundle ab = null;
        private String scene_name = "Town";

        public override void Initialize()
        {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += sceneChange;
        }

        public void OnSceneChange(Scene from, Scene to)
        {
            Log("Scene changed!! " + from.name + " to " + to.name);
            if (to.name == scene_name){
                // Load asset bundle
                if (ab == null)
                {
                    string bundle_name = "<Mod name>.Resources.root";
                    Assembly asm = Assembly.GetExecutingAssembly();

                    // Debugging
                    foreach (string name in asm.GetManifestResourceNames())
                    {
                        Log("Embedded resource: " + name);
                    }

                    using (Stream s = asm.GetManifestResourceStream(bundle_name))
                    {
                        byte[] buffer = new byte[s.Length];
                        s.Read(buffer, 0, buffer.Length);
                        s.Dispose();
                        ab = AssetBundle.LoadFromMemory(buffer);
                    }
                }

                // Note the path is based on where you saved the prefab
                objab = ab.LoadAsset<GameObject>("Assets/Prefabs/root.prefab");
                objab = GameObject.Instantiate(objab);
                objab.transform.position = Vector3.zero;
                objab.SetActive(true);

                // Debugging
                foreach (string name in ab.GetAllAssetNames())
                {
                    Log("Asset: " + name);
                }

                // Reset materials
                for (int i = 0; i < objab.transform.childCount; i ++)
                {
                    GameObject child = objab.transform.GetChild(i).gameObject;
                    child.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));
                }

            } else if (ab != null) {
                ab.Unload(true);
            }
        }
    }
}
```