using System.Collections.Generic;
using UnityEngine;

public class BillboardManager : MonoBehaviour
{
    public static BillboardManager Instance { get; private set; }

    private readonly List<Transform> billboards = new();

    private Camera targetCamera;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        targetCamera = Camera.main;
    }

    public void Register(Transform billboard)
    {
        if (!billboards.Contains(billboard))
            billboards.Add(billboard);
    }

    public void Unregister(Transform billboard)
    {
        billboards.Remove(billboard);
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;

            if (targetCamera == null)
                return;
        }

        Vector3 cameraPosition = targetCamera.transform.position;

        for (int i = billboards.Count - 1; i >= 0; i--)
        {
            if (billboards[i] == null)
            {
                billboards.RemoveAt(i);
                continue;
            }

            Transform billboard = billboards[i];

            Vector3 direction = cameraPosition - billboard.position;

            if (direction.sqrMagnitude < 0.001f)
                continue;

            billboard.rotation = Quaternion.LookRotation(direction);
        }
    }
}