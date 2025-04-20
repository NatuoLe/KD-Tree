using UnityEngine;
using System.Collections.Generic;

public class KdTree
{
    public Vector3[] points { get; private set; }
    public int[] ids { get; private set; }
    private Point _root;
    
    public void build(Vector3[] points, int[] ids)
    {
        this.points = points;
        this.ids = ids;
        _root = build(0, points.Length, 0);
    }

    private Point build(int offset, int length, int depth)
    {
        if (length == 0) return null;

        // 2D 专用：只在 X/Y 轴之间选择
        int axis = findBestAxis(offset, length); // 返回 0 (X) 或 1 (Y)
        System.Array.Sort<Vector3, int>(points, ids, offset, length, COMPS[axis]);

        int mid = length >> 1;
        return new Point()
        {
            mid = offset + mid,
            axis = axis,
            smaller = build(offset, mid, depth + 1),
            larger = build(offset + mid + 1, length - (mid + 1), depth + 1)
        };
    }

    public void delete(int id)
    {
        _root = delete(_root, id, 0);
    }

    private Point delete(Point p, int targetId, int depth)
    {
        if (p == null)
            return null;

        if (ids[p.mid] == targetId)  // 找到目标节点
        {
            p.deleted = true;  // 标记为删除
        }
        else
        {
            int axis = p.axis;  // 当前分割轴 (0:X, 1:Y, 2:Z)
        
            // 获取目标点的坐标（先找到它在 points 中的索引）
            int targetIndex = getIndex(targetId);  // 需要实现这个方法
            if (targetIndex == -1)
            {
                Debug.LogError($"ID {targetId} not found in points array!");
                return p;
            }

            // 比较当前节点和目标节点在当前轴上的坐标
            if (points[p.mid][axis] > points[targetIndex][axis])
            {
                p.smaller = delete(p.smaller, targetId, depth + 1);  // 目标点在左子树
            }
            else
            {
                p.larger = delete(p.larger, targetId, depth + 1);  // 目标点在右子树
            }
        }

        return p;
    }

    private int getIndex(int instanceId)
    {
        for (int i = 0; i < ids.Length; i++)
        {
            if (ids[i]== instanceId)
            {
                return i;
            }
        }

        return -1;
    }
    public int nearest(Vector3 point)
    {
        return ids[nearest(point, _root, 0)];
    }

    private int nearest(Vector3 point, Point p, int depth)
    {
        if (p == null)
            return -1;

        int best = -1;
        if (!p.deleted)
        {
            best = p.mid; // 当前节点有效时，初始化最佳候选
        }

        int axis = p.axis;
        // 计算分割距离
        float distToSplit = point[axis] - points[p.mid][axis];

        // 优先搜索更近的分支
        Point firstChild = distToSplit < 0 ? p.smaller : p.larger;
        Point secondChild = distToSplit < 0 ? p.larger : p.smaller;

        // 递归搜索第一子树
        int candidate = nearest(point, firstChild, depth + 1);
        best = closer(point, best, candidate);

        // 检查是否需要搜索第二子树
        float bestDist = sqDist(point, best);
        if (secondChild != null && (best == -1 || Mathf.Abs(distToSplit) < Mathf.Sqrt(bestDist)))
        {
            candidate = nearest(point, secondChild, depth + 1);
            best = closer(point, best, candidate);
        }

        return best;
    }
    private float sqDist(Vector3 point, int index)
    {
        if (index == -1)
            return float.PositiveInfinity;
        var dist = point - points[index];
        return dist.sqrMagnitude;
    }

    private int closer(Vector3 point, int i0, int i1)
    {
        //Debug.Log($"i0:{i0},i1{i1}");
        if ((i0 == -1) && (i1 == -1 ))
        {
            return -1;
        }

        if (i0 == -1)
            return i1;
        else if (i1 == -1)
            return i0;
        return sqDist(point, i0) < sqDist(point, i1) ? i0 : i1;
    }

    private int findBestAxis(int offset, int length)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        bool allZSame = true;
        float firstZ = points[offset].z;

        for (int i = 0; i < length; i++)
        {
            Vector3 p = points[offset + i];
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        
            // 检查 Z 轴是否全部相同
            if (allZSame && !Mathf.Approximately(p.z, firstZ))
            {
                allZSame = false;
                minZ = Mathf.Min(minZ, p.z);
                maxZ = Mathf.Max(maxZ, p.z);
            }
        }

        float rangeX = maxX - minX;
        float rangeY = maxY - minY;
        float rangeZ = allZSame ? -1 : (maxZ - minZ); // 如果 Z 全部相同，标记为无效

        // 选择有效的、跨度最大的轴
        if (rangeZ > rangeX && rangeZ > rangeY) return AXIS_Z;
        return (rangeX >= rangeY) ? AXIS_X : AXIS_Y;
        
    }
    public void PrintTree()
    {
        PrintTree(_root, 0);
    }

    private void PrintTree(Point p, int depth)
    {
        if (p == null)
            return;

        // 缩进表示层级
        string indent = new string(' ', depth * 4);

        // 打印当前节点信息
        string nodeInfo = $"{indent}Node (ID: {ids[p.mid]}, Point: {points[p.mid]}, Axis: {p.axis}, Deleted: {p.deleted})";
        Debug.Log(nodeInfo);

        // 递归打印左子树（smaller）
        PrintTree(p.smaller, depth + 1);

        // 递归打印右子树（larger）
        PrintTree(p.larger, depth + 1);
    }

    private class Point
    {
        public int mid;
        public int axis;
        public Point smaller;
        public Point larger;
        public bool deleted; // 添加删除标记
    }

    private const int AXIS_X = 0;
    private const int AXIS_Y = 1;
    private const int AXIS_Z = 2;
    private static readonly IComparer<Vector3>[] COMPS;

    static KdTree()
    {
        COMPS = new IComparer<Vector3>[]
        {
            new AxisComparer(AXIS_X),
            new AxisComparer(AXIS_Y),
            new AxisComparer(AXIS_Z)
        };
    }

    private class AxisComparer : IComparer<Vector3>
    {
        private int _axis;

        public AxisComparer(int axis)
        {
            _axis = axis;
        }

        public int Compare(Vector3 p0, Vector3 p1)
        {
            return p0[_axis] > p1[_axis] ? +1 : (p0[_axis] < p1[_axis] ? -1 : 0);
        }
    }
}