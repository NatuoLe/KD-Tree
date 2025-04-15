using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    }

    // Update is called once per frame
    void Update()
    {
        // 获取球的位置
        Vector3 ballPosition = ball.position;

        // 找到距离球最近的 Brick
        int nearestIndex = _tree.nearest(ballPosition);

        // 如果找到的 Brick 的 inHatchery 属性为 true，则继续查找下一个最近的 Brick
        int loopEnd = 0 ;
        while (nearestIndex != -1)
        {
            Brick nearestBrick = bricks.FirstOrDefault(brick => brick.gameObject.GetInstanceID() == nearestIndex);
            if (nearestBrick != null && !nearestBrick.inHatchery)
            {
                Debug.Log($"Nearest Brick Position: {nearestBrick.name}");
                break;
            }
            else
            {
                nearestIndex = _tree.nearest(ballPosition); // 调用排除当前节点的最近邻搜索
            }

            loopEnd++;
            if (loopEnd >20)
            {
                break;
            }
        }

        if (nearestIndex == -1)
        {
            Debug.Log("No valid Brick found.");
        }
    }

    public void Delete(Brick brick)
    {
        for (int i = 0; i < bricks.Length; i++)
        {
            if (bricks[i] == brick)
            {
                _tree.delete(i);
            }
        }
    }
}