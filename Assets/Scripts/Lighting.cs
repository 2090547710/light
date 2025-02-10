using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 光照组件
public class Lighting : MonoBehaviour
{
    [Range(1, 30)] public float Radius = 5f;

    
    
    void OnEnable()
    {
        LightingManager.RegisterLight(this);
    }


    void OnDisable()
    {
        LightingManager.UnregisterLight(this);
    }


    public void ApplyLighting()
    {
        Vector2 position = new Vector2(transform.position.x, transform.position.z);
        LightingManager.tree.MarkIlluminatedArea(position, Radius);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0,0,0);
        Vector3 center = transform.position;
        float theta = 0;
        int segments = 36;
        

        for(int i = 0; i < segments; i++){
            Vector3 pos1 = center + new Vector3(
                Mathf.Cos(theta) * Radius,
                0,
                Mathf.Sin(theta) * Radius
            );
            theta += (2f * Mathf.PI) / segments;
            Vector3 pos2 = center + new Vector3(
                Mathf.Cos(theta) * Radius,
                0,
                Mathf.Sin(theta) * Radius
            );
            Gizmos.DrawLine(pos1, pos2);
        }
    }
}