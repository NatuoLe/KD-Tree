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
        if (length == 0)
            return null;
        int axis = findBestAxis(offset, length); // depth % 3;
        System.Array.Sort<Vector3, int>(points, ids, offset, length, COMPS[axis]);
        int mid = length >> 1;
        return new Point()
        {
            mid = offset + mid,
            axis = axis,
            deleted = false, // 初始化为未删除
            smaller = build(offset, mid, depth + 1),
            larger = build(offset + mid + 1, length - (mid + 1), depth + 1)
        };
    }

    public void delete(int id)
    {
        _root = delete(_root, id, 0);
    }

    private Point delete(Point p, int id, int depth)
    {
        if (p == null)
            return null;

        if (ids[p.mid] == id)
        {
            // 标记节点为删除
            p.deleted = true;
        }
        else
        {
            int axis = depth % 3;
            if (points[p.mid][axis] > points[id][axis])
            {
                p.smaller = delete(p.smaller, id, depth + 1);
            }
            else
            {
                p.larger = delete(p.larger, id, depth + 1);
            }
        }

        return p;
    }

    public int nearest(Vector3 point)
    {
        return ids[nearest(point, _root, 0)];
    }

    /*private int nearest(Vector3 point, Point p, int depth)
    {
        if (p == null)
            return -1;
        var axis = p.axis; //depth % 3;
        int leaf;
        var dist2mid = points[p.mid][axis] - point[axis];
        Debug.Log($"dis:维度{axis},point:{p.mid},point_{ points[p.mid][axis]}");
        if (dist2mid > 0)
        {
            leaf = nearest(point, p.smaller, depth + 1);
            var sqDist2leaf = sqDist(point, leaf);
            if (sqDist2leaf > dist2mid * dist2mid)
                leaf = closer(point, leaf, nearest(point, p.larger, depth + 1));
        }
        else
        {
            leaf = nearest(point, p.larger, depth + 1);
            var sqDist2leaf = sqDist(point, leaf);
            if (sqDist2leaf > dist2mid * dist2mid)
                leaf = closer(point, leaf, nearest(point, p.smaller, depth + 1));
        }

        return closer(point, leaf, p.mid);
    }*/
    private int nearest(Vector3 point, Point p, int depth) {
        if (p == null || p.deleted)
            return -1;

        var axis = p.axis;
        int leaf;
        var dist2mid = points[p.mid][axis] - point[axis];

        if (dist2mid > 0) {
            // 递归搜索左子树
            leaf = nearest(point, p.smaller, depth + 1);
            if (leaf == -1 || sqDist(point, leaf) > dist2mid * dist2mid) {
                // 如果左子树没有找到有效节点，或者当前中点更近，则考虑右子树
                leaf = closer(point, leaf, p.mid);
                if (leaf == -1 || sqDist(point, leaf) > dist2mid * dist2mid) {
                    // 如果中点也不行，递归搜索右子树
                    leaf = closer(point, leaf, nearest(point, p.larger, depth + 1));
                }
            }
        } else {
            // 递归搜索右子树
            leaf = nearest(point, p.larger, depth + 1);
            if (leaf == -1 || sqDist(point, leaf) > dist2mid * dist2mid) {
                // 如果右子树没有找到有效节点，或者当前中点更近，则考虑左子树
                leaf = closer(point, leaf, p.mid);
                if (leaf == -1 || sqDist(point, leaf) > dist2mid * dist2mid) {
                    // 如果中点也不行，递归搜索左子树
                    leaf = closer(point, leaf, nearest(point, p.smaller, depth + 1));
                }
            }
        }

        return leaf;
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
        Debug.Log($"i0:{i0},i1{i1}");
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
        float minx, miny, minz, maxx, maxy, maxz;
        minx = miny = minz = float.MaxValue;
        maxx = maxy = maxz = float.MinValue;
        for (var i = 0; i < length; i++)
        {
            var p = points[i + offset];
            if (p.x < minx)
                minx = p.x;
            else if (maxx < p.x)
                maxx = p.x;
            if (p.y < miny)
                miny = p.y;
            else if (maxy < p.y)
                maxy = p.y;
            if (p.z < minz)
                minz = p.z;
            else if (maxz < p.z)
                maxz = p.z;
        }

        var size = new Vector3(maxx - minx, maxy - miny, maxz - minz);
        if (size.x < size.y)
        {
            if (size.y < size.z)
                return AXIS_Z;
            else
                return AXIS_Y;
        }
        else
        {
            if (size.x < size.z)
                return AXIS_Z;
            else
                return AXIS_X;
        }
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