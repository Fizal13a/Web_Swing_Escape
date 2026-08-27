using System.Collections.Generic;
using UnityEngine;

public class StepPopupPool : MonoBehaviour
{
    [SerializeField] private StepPopup prefab;
    [SerializeField] private int poolSize = 10;


    private readonly Queue<StepPopup> pool = new();


    private void Awake()
    {
        for(int i=0;i<poolSize;i++)
        {
            var item = Instantiate(prefab, transform);
            item.gameObject.SetActive(false);

            pool.Enqueue(item);
        }
    }


    public StepPopup Get()
    {
        var item = pool.Dequeue();

        item.gameObject.SetActive(true);

        pool.Enqueue(item);

        return item;
    }
}
