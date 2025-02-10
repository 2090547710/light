using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 新增光照系统管理器
public class LightingManager : MonoBehaviour
{
        
    public static QuadTree tree { get; set; }
    private static List<Lighting> activeLights = new List<Lighting>();
    
    void LateUpdate()
    {
        tree.ResetIllumination();
        foreach (var light in activeLights)
        {
            light.ApplyLighting();
        }
    }


    public static void RegisterLight(Lighting light)
    {
        if (!activeLights.Contains(light))
        {
            activeLights.Add(light);
        }
    }

    public static void UnregisterLight(Lighting light)
    {
        activeLights.Remove(light);
    }

    void OnDrawGizmos()
    {
        tree?.DrawGizmos();
    }

}
