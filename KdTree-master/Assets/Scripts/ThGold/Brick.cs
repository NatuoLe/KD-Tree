using System;
using UnityEngine;

public class Brick: MonoBehaviour
{
    private kdController kdController;
    private KdTreeTest KdTreeTest;
    public bool inHatchery
    {
        get
        {
            return hatchery;
        }
        set
        {
            hatchery = value;
            if (hatchery)
            {
                kdController?.Delete(this);
            }
        }
    }

    [SerializeField] private bool hatchery;

    public void Init(kdController kdController)
    {
        this.kdController = kdController;
    }
    public void Init(KdTreeTest KdTreeTest)
    {
        this.KdTreeTest = KdTreeTest;
    }

    private void Awake()
    {
        name = gameObject.transform.position.ToString();
    }
}