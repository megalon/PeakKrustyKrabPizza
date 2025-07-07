using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PEAKLib.Core;
using PEAKLib.Items;
using Photon.Pun;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeakKrustyKrabPizza;

[BepInAutoPlugin]
[BepInDependency("com.github.PEAKModding.PEAKLib.Core", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("com.github.PEAKModding.PEAKLib.Items", BepInDependency.DependencyFlags.HardDependency)]
public partial class Plugin : BaseUnityPlugin
{
    public static Plugin Instance { get; private set; } = null!;
    internal static ManualLogSource Log { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }
    internal static AssetBundle Bundle { get; set; } = null!;
    internal static GameObject PizzaPrefab { get; set; } = null!;
    internal static ModDefinition Definition { get; set; } = null!;

    private void Awake()
    {
        Instance = this;
        Log = Logger;
        Definition = ModDefinition.GetOrCreate(Info.Metadata);

        string AssetBundlePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "krustykrabpizza");

        Bundle = AssetBundle.LoadFromFile(AssetBundlePath);

        PizzaPrefab = Bundle.LoadAsset<GameObject>("Krusty Krab Pizza.prefab");

        Item pizza = PizzaPrefab.GetComponent<Item>();
        new ItemContent(pizza).Register(Definition);

        // Log our awake here so we can see it in LogOutput.log file
        Log.LogInfo($"Plugin {Name} is loaded!");

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        Log.LogInfo(newScene.name);

        if (newScene.name.ToLower().StartsWith("level_") || newScene.name == "WilIsland")
        {
            TryInstantiate("Krusty Krab Pizza.prefab");
            TryInstantiate("Krusty Krab Pizza");
        }
    }

    private void TryInstantiate(string name)
    {
        try
        {
            Item component = PhotonNetwork.InstantiateItemRoom(name, new Vector3(14.4139996f, 1.77900004f, -382.032013f), Quaternion.identity).GetComponent<Item>();
            component.ForceSyncForFrames();
            if (component != null)
            {
                component.GetComponent<PhotonView>().RPC("SetKinematicRPC", RpcTarget.AllBuffered, true, component.transform.position, component.transform.rotation);
            }
        } catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }
}