using System.Collections.Generic;
using UnityEngine;
using BlockStackTypes;
using System.Linq;
using System;
using System.Collections;

public class Utilities
{
    public static T RandomItem<T>(T[] array)
    {
        return array[UnityEngine.Random.Range(0, array.Length)];
    }

    public static void SortByYPos(ref List<Transform> currentCups)
    {
        for (int i = 0; i < currentCups.Count; i++)
        {
            for (int j = i + 1; j < currentCups.Count; j++)
            {
                if (currentCups[j].position.y < currentCups[i].position.y)
                    (currentCups[j], currentCups[i]) = (currentCups[i], currentCups[j]);
            }
        }
    }

    public static Transform DebugVector3Point(Vector3 finalPos, float Scale = 0.2f, Color? color = null)
    {
        if (color == null) color = Color.red;
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = finalPos;
        sphere.transform.localScale = Vector3.one * Scale;
        sphere.GetComponent<MeshRenderer>().material.color = color.Value;

        return sphere.transform;
    }

    public static CupGrid3D DeepCopyGrid(CupGrid3D original)
    {
        CupGrid3D copy = new(original.Width, original.Height);

        for (int x = 0; x < original.Width; x++)
        {
            for (int y = 0; y < original.Height; y++)
            {
                var originalStack = original.Grid[x].CupStacks[y];
                var copiedStack = new CupStack(0);

                copiedStack.Cups = originalStack.Cups
                    .Select(cup => new BlockStackTypes.Cup(cup.Color))
                    .ToList();

                copy.Grid[x].CupStacks[y] = copiedStack;
            }
        }

        return copy;
    }

    public static void ExecuteAfter(MonoBehaviour mb, float duration, Action afterAction)
    {
        mb.StartCoroutine(ExecuteAfterCoroutine(duration, afterAction));
    }

    private static IEnumerator ExecuteAfterCoroutine(float duration, Action afterAction)
    {
        yield return new WaitForSeconds(duration);
        afterAction();
    }

    public static int GetRandomLevelNumber()
    {
        int min = Manager.Instance.MinLevelNumber;
        int max = Manager.Instance.MaxLevelNumber;
        int last = PlayerPrefs.GetInt("LastRandom", -1);
        int attempt = 0;
        int newRandom;

        do
        {
            newRandom = UnityEngine.Random.Range(min, max + 1);
            attempt++;
        }
        while (newRandom == last && attempt < 10);

        PlayerPrefs.SetInt("LastRandom", newRandom);
        PlayerPrefs.Save();
        return newRandom;
    }

    public static void AssignLayerRecursively(Transform root, int layer)
    {
        root.gameObject.layer = layer;

        foreach (Transform child in root)
            AssignLayerRecursively(child, layer);
    }

    public static bool CanPurchasePowerup()
    {
        int CurrentCoins = PlayerPrefs.GetInt("coins", 0);
        if (CurrentCoins >= 500)
            return true;

        return false;
    }
}