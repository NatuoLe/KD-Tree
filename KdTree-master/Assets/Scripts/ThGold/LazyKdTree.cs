using UnityEngine;
using System.Collections.Generic;

public class LazyKdTree
{
    private Node root;
    private Vector3[] points;
    private int[] ids;
    private int splitThreshold = 16; // 分割阈值

    public class Node
    {
        public int start;          // 数据起始索引
        public int length;          // 数据数量
        public int axis = -1;      // 分割轴（-1表示未分割）
        public float splitValue;    // 分割值
        public Node left;          // 左子树
        public Node right;         // 右子树
        public HashSet<int> deletedIds = new HashSet<int>(); // 删除标记
    }

    // 初始化树
    public void Build(Vector3[] points, int[] ids)
    {
        this.points = points;
        this.ids = ids;
        root = new Node
        {
            start = 0,
            length = points.Length
        };
    }

    // 删除指定ID的点
    public void Delete(int id)
    {
        DeleteRecursive(root, id);
    }

// 修改LazyKdTree的DeleteRecursive方法
    private bool DeleteRecursive(Node node, int id)
    {
        if (node == null) return false;

        // 未分割节点处理
        if (!IsSplit(node))
        {
            bool deleted = false;
            for (int i = node.start; i < node.start + node.length; i++)
            {
                if (ids[i] == id && !node.deletedIds.Contains(id))
                {
                    node.deletedIds.Add(id);
                    deleted = true;
                    break; // 找到后立即退出
                }
            }
            return deleted;
        }

        // 已分割节点处理
        bool foundInLeft = DeleteRecursive(node.left, id);
        bool foundInRight = foundInLeft ? false : DeleteRecursive(node.right, id);
    
        // 同步更新当前节点的删除标记
        if (foundInRight && ids[FindMidIndex(node)] == id)
        {
            node.deletedIds.Add(id);
        }
    
        return foundInLeft || foundInRight;
    }

    // 最近邻查询
    public int FindNearest(Vector3 queryPoint)
    {
        var nearest = new NearestInfo();
        SearchNearest(root, queryPoint, nearest);
        return nearest.id;
    }

    private void SearchNearest(Node node, Vector3 queryPoint, NearestInfo nearest)
    {
        if (node == null) return;

        if (!IsSplit(node))
        {
            TrySplit(node);
            if (!IsSplit(node))
            {
                // 修复：严格比较所有点的距离
                for (int i = node.start; i < node.start + node.length; i++)
                {
                    if (node.deletedIds.Contains(ids[i])) continue;

                    float sqDist = (queryPoint - points[i]).sqrMagnitude;
                    //Debug.Log($"Checking point {points[i]}, distance={sqDist}, current nearest={nearest.id}");
                    if (sqDist < nearest.sqDistance)
                    {
                        nearest.sqDistance = sqDist;
                        nearest.id = ids[i]; // 确保记录的是最近点的ID
                    }
                }

                return;
            }
        }

        // 已分割节点的处理
        int mid = FindMidIndex(node);
        float diff = queryPoint[node.axis] - points[mid][node.axis];
        
        // 先搜索更近的分支
        Node first = diff < 0 ? node.left : node.right;
        Node second = diff < 0 ? node.right : node.left;
        
        SearchNearest(first, queryPoint, nearest);
        
        // 如果可能跨越分割面，则搜索另一边
        if (nearest.sqDistance > diff * diff)
        {
            SearchNearest(second, queryPoint, nearest);
        }
    }

    // 动态分割节点
    private void TrySplit(Node node)
    {
        if (IsSplit(node) || node.length <= splitThreshold) return;
        
        // 选择分割维度
        Vector3 min = Vector3.positiveInfinity;
        Vector3 max = Vector3.negativeInfinity;
        
        for (int i = node.start; i < node.start + node.length; i++)
        {
            Vector3 p = points[i];
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }
        
        Vector3 size = max - min;
        node.axis = size.x > size.y ? 
                   (size.x > size.z ? 0 : 2) : 
                   (size.y > size.z ? 1 : 2);

        // 找到中位数
        int mid = node.start + node.length / 2;
        QuickSelect(node.start, node.start + node.length - 1, mid, node.axis);
        
        // 创建子节点
        node.left = new Node
        {
            start = node.start,
            length = mid - node.start
        };
        
        node.right = new Node
        {
            start = mid + 1,
            length = node.start + node.length - (mid + 1)
        };
        
        node.splitValue = points[mid][node.axis];
    }

    // 快速选择算法
    private void QuickSelect(int left, int right, int k, int axis)
    {
        while (left < right)
        {
            int pivot = Partition(left, right, axis);
            if (pivot == k) break;
            if (k < pivot) right = pivot - 1;
            else left = pivot + 1;
        }
    }

    private int Partition(int left, int right, int axis)
    {
        float pivotValue = points[right][axis];
        int storeIndex = left;
        for (int i = left; i < right; i++)
        {
            if (points[i][axis] < pivotValue)
            {
                Swap(i, storeIndex);
                storeIndex++;
            }
        }
        Swap(storeIndex, right);
        return storeIndex;
    }

    private void Swap(int i, int j)
    {
        (points[i], points[j]) = (points[j], points[i]);
        (ids[i], ids[j]) = (ids[j], ids[i]);
    }

    // 辅助方法
    private bool IsSplit(Node node) => node.axis != -1;
    private int FindMidIndex(Node node) => node.start + node.length / 2;
    private float GetCoordinate(int id, int axis)
    {
        for (int i = 0; i < ids.Length; i++)
            if (ids[i] == id) return points[i][axis];
        return float.NaN;
    }

    private class NearestInfo
    {
        public int id = -1;
        public float sqDistance = float.MaxValue;
    }
}