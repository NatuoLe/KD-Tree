using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class KdTreeTest : MonoBehaviour
{
    LazyKdTree tree;
    [SerializeField] private Brick[] bricks;

    public Transform ball;

    void Start()
    {
        // 初始化数据
        Vector3[] points = bricks.Select(brick => brick.transform.position).ToArray() /* 你的位置数据 */;
        int[] ids = bricks.Select(brick => brick.gameObject.GetInstanceID()).ToArray(); /* 对应ID */
        /*// 调试输出所有点
        for(int i=0; i<points.Length; i++){
            Debug.Log($"Brick {i}: ID={ids[i]}, Pos={points[i]}, Dist={points[i].sqrMagnitude}");
        }*/
        // 构建树
        tree = new LazyKdTree();
        tree.Build(points, ids);
        // 查询示例
        /*Vector3 queryPoint = ball.transform.position;
        Debug.Log($"查询点坐标: {queryPoint}"); // 检查是否是(0,0,0)
        float startTime = Time.realtimeSinceStartup;
        int nearestId = tree.FindNearest(queryPoint);
        float elapsedTime = Time.realtimeSinceStartup - startTime;
        Debug.Log($"Nearest Brick Position: {GetBrickByIsId(nearestId).name}");
        Debug.Log($"最近邻查询耗时: {elapsedTime * 1000:F4} 毫秒");
        // 删除示例
        /*tree.Delete(nearestId);
        int newID = tree.FindNearest(queryPoint);
        Debug.Log($"Nearest Brick Position: {GetBrickByIsId(newID).name}");#1#*/
        /*TestKdTreeQuery();
        TestAllDistances();*/
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestAllDistances();
            TestKdTreeQuery();
        }
    }


    public GameObject GetBrickByIsId(int id)
    {
        for (int i = 0; i < bricks.Length; i++)
        {
            int _id = bricks[i].gameObject.GetInstanceID();
            if (_id == id)
            {
                return bricks[i].gameObject;
            }
        }

        Debug.Log("没找到 " + id);
        return null;
    }

    void TestKdTreeQuery()
    {
        Stopwatch sw = Stopwatch.StartNew();

        int nearestId = tree.FindNearest(ball.transform.position);
        Brick nearestBrick = bricks.FirstOrDefault(b => b.gameObject.GetInstanceID() == nearestId);
        float distance = nearestBrick ? Vector3.Distance(nearestBrick.transform.position, ball.position) : 0;

        sw.Stop();

        Debug.Log($"KD树查询耗时: {sw.Elapsed.TotalMilliseconds:F4}ms | " +
                  $"最近砖块: {nearestBrick?.name ?? "null"} " +
                  $"距离: {distance:F2}");
        tree.Delete(nearestId);
    }

    void TestAllDistances()
    {
        Stopwatch sw = Stopwatch.StartNew();

        float minDistance = float.MaxValue;
        Brick nearestBrick = null;

        foreach (var brick in bricks)
        {
            float dist = Vector3.Distance(brick.transform.position, ball.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestBrick = brick;
            }
        }

        sw.Stop();

        Debug.Log($"暴力搜索耗时: {sw.Elapsed.TotalMilliseconds:F4}ms | " +
                  $"最近砖块: {nearestBrick.name} " +
                  $"距离: {minDistance:F2}");
    
    }
}