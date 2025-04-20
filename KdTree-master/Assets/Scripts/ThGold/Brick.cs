using System;
using UnityEngine;

public class Brick: MonoBehaviour
{
    private kdController kdController;
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
                kdController.Delete(this);
                this.gameObject.SetActive(false);
            }
        }
    }

    [SerializeField] private bool hatchery;

    public void Init(kdController kdController)
    {
        this.kdController = kdController;
    }

    public void Delete()
    {
        inHatchery = true;
    }
    private void Awake()
    {
        
    }
}