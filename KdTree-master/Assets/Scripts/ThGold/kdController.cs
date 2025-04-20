using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class kdController : MonoBehaviour
{
    public KdTree _tree;

    [SerializeField] private Brick[] bricks;

    public Transform ball;

    // Start is called before the first frame update
    void Start()
    {
        _tree = new KdTree();
        _tree.build(bricks.Select(brick => brick.transform.position).ToArray(),
            bricks.Select(brick => brick.gameObject.GetInstanceID()).ToArray());
        foreach (var brick in bricks)
        {
            brick.Init(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        return;
    }

    public void Check()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start(); // 开始计时
        // 获取球的位置
        Vector3 ballPosition = ball.position;
        // 找到距离球最近的 Brick

        int nearestIndex = _tree.nearest(ballPosition);
        Brick nearestBrick = bricks.FirstOrDefault(brick => brick.gameObject.GetInstanceID() == nearestIndex);
        stopwatch.Stop(); // 停止计时
        TimeSpan elapsedTime = stopwatch.Elapsed;
        Debug.Log($"代码执行时间：{elapsedTime.TotalMilliseconds} 毫秒");

        stopwatch.Start(); // 开始计时
        // 调用方法找到最近的砖块
        Brick closestBrick = FindClosestBrick();
        if (closestBrick != null)
        {
            Debug.Log(
                $"最近的砖块是：{closestBrick.name}，距离为：{Vector3.Distance(ball.position, closestBrick.transform.position)}");
        }

        stopwatch.Stop(); // 停止计时
        elapsedTime = stopwatch.Elapsed;
        Debug.Log($"代码执行时间：{elapsedTime.TotalMilliseconds} 毫秒");
        if (nearestBrick != null && !nearestBrick.inHatchery)
        {
            Debug.Log($"Nearest Brick Position: {nearestBrick.name}");
            nearestBrick.Delete();
        }
    }

    public void Delete(Brick brick)
    {
        for (int i = 0; i < bricks.Length; i++)
        {
            if (bricks[i] == brick)
            {
                _tree.delete(bricks[i].gameObject.GetInstanceID());
            }
        }
    }

    private Brick FindClosestBrick()
    {
        if (bricks == null || bricks.Length == 0)
        {
            Debug.LogWarning("没有砖块可供查找");
            return null;
        }

        Brick closestBrick = bricks[0]; // 假设第一个砖块是最接近的
        float minDistance = Vector3.Distance(ball.position, closestBrick.transform.position); // 计算初始最小距离

        foreach (Brick brick in bricks)
        {
            float distance = Vector3.Distance(ball.position, brick.transform.position); // 计算当前砖块与球的距离
            if (distance < minDistance)
            {
                minDistance = distance; // 更新最小距离
                closestBrick = brick; // 更新最近的砖块
            }
        }

        return closestBrick;
    }

    public void Print()
    {
        _tree.PrintTree();
    }
}