using System.Collections.Generic;

public static class ListExtensions
{
    public static T GetRandom<T>(this List<T> list, bool remove = false)
    {
        if (list == null || list.Count == 0) return default;

        int index = UnityEngine.Random.Range(0, list.Count);
        T item = list[index];

        if (remove)
        {
            list.RemoveAt(index);
        }

        return item;
    }

    /// <summary>
    /// Gets a random item from the list, but reduces the probability of selecting the specified index by the given factor (0-1).
    /// </summary>
        /// <summary>
    /// Gets a random item from the list, but reduces the probability of selecting the specified value by the given factor (0-1).
    /// </summary>
    public static T GetRandomWithBias<T>(this List<T> list, T valueToReduce, float reductionFactor, bool remove = false)
    {
        if (list == null || list.Count == 0) return default;
        int indexToReduce = list.IndexOf(valueToReduce);
        if (indexToReduce == -1) return list.GetRandom(remove);
        if (reductionFactor < 0f) reductionFactor = 0f;
        if (reductionFactor > 1f) reductionFactor = 1f;
    
        // Build weights
        float[] weights = new float[list.Count];
        for (int i = 0; i < list.Count; i++)
            weights[i] = 1f;
        weights[indexToReduce] = reductionFactor;
    
        // Sum weights
        float totalWeight = 0f;
        for (int i = 0; i < weights.Length; i++)
            totalWeight += weights[i];
    
        // Pick random
        float r = UnityEngine.Random.value * totalWeight;
        for (int i = 0; i < weights.Length; i++)
        {
            if (r < weights[i])
            {
                T item = list[i];
                if (remove) list.RemoveAt(i);
                return item;
            }
            r -= weights[i];
        }
    
        // Fallback
        return list.GetRandom(remove);
    }
    // ...existing code...
}
