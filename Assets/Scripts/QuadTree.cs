using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class QuadTree
{
    ////////////////////////////////////////////////////////////////
    // 四叉树节点类
    private class QuadTreeNode
    {
        // 节点边界（中心点 + 尺寸）

        public Vector2 Center { get; private set; }
        public Vector2 Size { get; private set; }
        
        // 子节点（四个象限）
        public QuadTreeNode[] Children { get; private set; }
        
        // 存储对象的最大容量
        public int Capacity { get; private set; }
        
        // 当前存储的对象列表
        public List<GameObject> Objects = new List<GameObject>();

        // 简化为光照可见状态
        public bool IsIlluminated = false;

        public QuadTreeNode(Vector2 center, Vector2 size, int capacity)
        {
            Center = center;
            Size = size;
            Capacity = capacity;
        }

        // 分裂节点为四个子节点
        public void Split()
        {
            Children = new QuadTreeNode[4];
            Vector2 quarterSize = Size * 0.5f;
            Vector2 halfSize = Size * 0.25f;

            // 计算四个子节点的中心点
            Children[0] = new QuadTreeNode(
                new Vector2(Center.x + halfSize.x, Center.y + halfSize.y),
                quarterSize, Capacity);

            Children[1] = new QuadTreeNode(
                new Vector2(Center.x - halfSize.x, Center.y + halfSize.y),
                quarterSize, Capacity);

            Children[2] = new QuadTreeNode(
                new Vector2(Center.x - halfSize.x, Center.y - halfSize.y),
                quarterSize, Capacity);

            Children[3] = new QuadTreeNode(
                new Vector2(Center.x + halfSize.x, Center.y - halfSize.y),
                quarterSize, Capacity);
        }

        // 检查点是否在节点范围内
        public bool Contains(Vector2 point)
        {
            return Mathf.Abs(point.x - Center.x) <= Size.x * 0.5f &&
                   Mathf.Abs(point.y - Center.y) <= Size.y * 0.5f;
        }
    }

    ////////////////////////////////////////////////////////////////

    // 根节点和最大深度
    private QuadTreeNode root;
    private int maxDepth;

    public QuadTree(Vector2 center, Vector2 size, int capacity, int maxDepth = 5, bool preSplit = false)
    {
        root = new QuadTreeNode(center, size, capacity);
        this.maxDepth = maxDepth;
        
        // 新增预分裂功能
        if(preSplit)
        {
            PreSplitRecursive(root, 0);
        }
    }

    // 新增预分裂方法
    private void PreSplitRecursive(QuadTreeNode node, int currentDepth)
    {
        if(currentDepth >= maxDepth) return;
        
        node.Split();
        foreach(var child in node.Children)
        {
            PreSplitRecursive(child, currentDepth + 1);
        }
    }

    // 插入对象到四叉树
    public bool Insert(GameObject obj, int currentDepth = 0)
    {
        Vector3 objPosition = obj.transform.position;
        Vector2 position = new Vector2(objPosition.x, objPosition.z);
        return InsertRecursive(root, position, obj, currentDepth);
    }

    private bool InsertRecursive(QuadTreeNode node, Vector2 position, GameObject obj, int depth)
    {
        if (!node.Contains(position)) return false;

        if (node.Children == null)
        {
            if (node.Objects.Count < node.Capacity || depth >= maxDepth)
            {
                node.Objects.Add(obj);
                return true;
            }

            node.Split();
            RedistributeObjects(node);
        }

        foreach (var child in node.Children)
        {
            if (InsertRecursive(child, position, obj, depth + 1))
            {
                return true;
            }
        }
        return false;
    }

    // 重新分配对象到子节点
    private void RedistributeObjects(QuadTreeNode node)
    {
        List<GameObject> objectsToRedistribute = new List<GameObject>(node.Objects);
        node.Objects.Clear();

        foreach (var obj in objectsToRedistribute)
        {
            Vector3 objPos = obj.transform.position;
            Vector2 position = new Vector2(objPos.x, objPos.z);
            
            bool redistributed = false;

            // 尝试将对象分配到子节点
            foreach (var child in node.Children)
            {
                if (child.Contains(position))
                {
                    child.Objects.Add(obj);
                    redistributed = true;
                    break;
                }
            }

            // 如果无法分配到任何子节点，保留在父节点
            if (!redistributed)
            {
                node.Objects.Add(obj);
            }
        }
    }

    // 查询区域内的对象
    public List<GameObject> QueryArea(Bounds area)
    {
        List<GameObject> results = new List<GameObject>();
        QueryAreaRecursive(root, area, ref results);
        return results;
    }

    private void QueryAreaRecursive(QuadTreeNode node, Bounds area, ref List<GameObject> results)
    {
        // 创建节点对应的AABB边界
        Rect nodeRect = new Rect(
            node.Center.x - node.Size.x * 0.5f,
            node.Center.y - node.Size.y * 0.5f,
            node.Size.x,
            node.Size.y);

        // 创建3D边界框用于检测（XZ平面）
        Bounds nodeBounds = new Bounds(
            new Vector3(node.Center.x, 0, node.Center.y), // 中心点转换
            new Vector3(node.Size.x, 0, node.Size.y));    // 尺寸转换

        // 使用修正后的边界进行检测
        if (!area.Intersects(nodeBounds))
        {
            return;
        }

        // 如果有子节点则递归查询
        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                QueryAreaRecursive(child, area, ref results);
            }
        }
        else
        {
            // 添加当前节点内的有效对象
            foreach (var obj in node.Objects)
            {
                Vector3 objPos = obj.transform.position;
                if (area.Contains(new Vector3(objPos.x, 0, objPos.z)))
                {
                    results.Add(obj);
                }
            }
        }
    }

    // 调试绘制
    public void DrawGizmos()
    {
        DrawNodeGizmos(root, 0);
    }

    private void DrawNodeGizmos(QuadTreeNode node, int depth)
    {
        if (node == null) return;

        // 根据深度设置不同颜色
        // Color[] depthColors = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan };
        // Gizmos.color = depthColors[Mathf.Clamp(depth, 0, depthColors.Length - 1)];

        // 绘制节点边界
        Vector3 center = new Vector3(node.Center.x, 0, node.Center.y);
        Vector3 size = new Vector3(node.Size.x, 0.1f, node.Size.y);
        // Gizmos.DrawWireCube(center, size);

        // 显示对象数量
        // GUIStyle style = new GUIStyle();
        // style.normal.textColor = Gizmos.color;
        // Handles.Label(
        //     new Vector3(node.Center.x, 0,node.Center.y), 
        //     $"{node.Objects.Count}",
        //     style);

        // 根据光照状态改变颜色
         // 半透明填充（alpha值0.2）
        Gizmos.color = node.IsIlluminated ? new Color(1, 0.9f, 0.5f, 1.0f) : new Color(0,0,0,0.1f);
        Gizmos.DrawCube(center, size * 1.0f);

        // 递归绘制子节点
        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                DrawNodeGizmos(child, depth + 1);
            }
        }
    }

    // 新增移除方法
    public bool Remove(GameObject obj)
    {
        Vector3 pos = obj.transform.position;
        Vector2 position = new Vector2(pos.x, pos.z);
        return RemoveRecursive(root, position, obj);
    }

    private bool RemoveRecursive(QuadTreeNode node, Vector2 position, GameObject obj)
    {
        if (!node.Contains(position)) return false;

        // 尝试在当前节点移除
        if (node.Objects.Remove(obj))
        {
            return true;
        }

        // 如果有子节点则递归查找
        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                if (RemoveRecursive(child, position, obj))
                {
                    return true;
                }
            }
        }
        return false;
    }

    // 新增光照标记方法
    public void MarkIlluminatedArea(Vector2 center, float radius)
    {
        MarkIlluminatedRecursive(root, center, radius);
    }

    private void MarkIlluminatedRecursive(QuadTreeNode node, Vector2 center, float radius)
    {
        Rect nodeRect = new Rect(
            node.Center.x - node.Size.x/2,
            node.Center.y - node.Size.y/2,
            node.Size.x,
            node.Size.y);
        
        if (!CircleRectOverlap(center, radius, nodeRect)) return;

        // 只在叶子节点进行标记
        if (node.Children == null)
        {
            node.IsIlluminated = true;
        }
        else
        {
            // 非叶子节点继续向下传播
            foreach (var child in node.Children)
            {
                MarkIlluminatedRecursive(child, center, radius);
            }
        }
    }

    // 圆形与矩形碰撞检测
    private bool CircleRectOverlap(Vector2 circlePos, float radius, Rect rect)
    {
        float dx = Mathf.Abs(circlePos.x - rect.center.x);
        float dy = Mathf.Abs(circlePos.y - rect.center.y);

        if (dx > (rect.width/2 + radius)) return false;
        if (dy > (rect.height/2 + radius)) return false;

        if (dx <= (rect.width/2)) return true;
        if (dy <= (rect.height/2)) return true;

        float cornerDistSq = Mathf.Pow(dx - rect.width/2, 2) +
                           Mathf.Pow(dy - rect.height/2, 2);

        return cornerDistSq <= (radius * radius);
    }

    // 重置光照状态
    public void ResetIllumination()
    {
        ResetIlluminationRecursive(root);
    }

    private void ResetIlluminationRecursive(QuadTreeNode node)
    {
        node.IsIlluminated = false;
        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                ResetIlluminationRecursive(child);
            }
        }
    }
}

