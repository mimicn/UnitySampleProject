using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sorting;

public class SortTest : MonoBehaviour
{
    public int[] items;
    public float wait;
    [SerializeField]
    private int[] sorteditem;
    [SerializeField]
    private int[] c_sorteditem;
    [SerializeField]
    private int[] c2_sorteditem;
    [SerializeField]
    private int[] c3_sorteditem;

    public class IntComparer : Comparer<int>
    {
        public override int Compare(int x, int y)
        {//+ is "x greater than y".
            return x - y;
        }
    }

    // Use this for initialization
    void Start () {
        List<int> sortlist = new List<int>(items);
        sortlist.Sort();
        sorteditem = sortlist.ToArray();

        IntComparer com = new IntComparer();
        sortlist = new List<int>(items);
        sortlist.Sort(com);
        c_sorteditem = sortlist.ToArray();

        c3_sorteditem = new int[items.Length];
        System.Array.Copy(items, c3_sorteditem, items.Length);
        SortClass sortclass = new SortClass();
        sortclass.Sort<int>(
            c3_sorteditem,
            com,
            (_b) => { },
            (_b) => { }
        );

        StartCoroutine(Sort());
    }
    IEnumerator Sort()
    {
        SortClass sortclass = new SortClass();
        IntComparer com = new IntComparer();
        yield return sortclass.SortUseIEnumerator<int>(
            items,
            com,
            (_b) => { },
            (_b) => {
                c2_sorteditem = items;
            },
            new WaitForSeconds(wait)
        );
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
