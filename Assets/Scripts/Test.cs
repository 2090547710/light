using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public QuadTree tree;
    public GameObject[] objects;
    public Bounds queryBounds;
 

    

    // Start is called before the first frame update
    void Start()
    {

        // 初始化四叉树（中心点，区域尺寸，节点容量，最大深度）
        tree = new QuadTree(
            Vector2.zero, 
            new Vector2(100, 100), 
            4, 
            5);

        // 插入对象
        foreach (var obj in objects)
        {
            tree.Insert(obj);
        }
        check();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 查询区域（示例区域）
    public void check(){

        // 创建查询区域（注意使用XZ平面坐标）
        Vector3 center = new Vector3(-10, 0, 14); // y坐标可设为任意值（不影响查询）
        Vector3 size = new Vector3(10, 0, 10); // 在XZ平面创建10x10的方形区域
        queryBounds = new Bounds(center, size);

        // 执行查询
        List<GameObject> foundObjects = tree.QueryArea(queryBounds);

        // 处理结果（示例：输出对象信息）
        foreach (var obj in foundObjects)
        {
            Vector3 pos = obj.transform.position;
            Debug.Log($"找到对象：{obj.name} 位置：X={pos.x}, Z={pos.z}");
        }

        
    }

    void OnDrawGizmos()
    {
        if (tree != null)
        {
            tree.DrawGizmos();
        }
        
        if (queryBounds != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(queryBounds.center, queryBounds.size);
        }
  
    }
}
