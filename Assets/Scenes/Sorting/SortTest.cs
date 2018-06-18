using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sorting;
using UnityEngine.UI;

public class SortTest : MonoBehaviour
{
    public Sorting.SortType sort_type = SortType.Quick;
    public bool dstint_text;
    public Text dst_text;
    public bool random_array = false;
    public int[] items;
    public float wait;
    public int min_int = 1;
    public int max_int = 100;
    [SerializeField]
    private int[] sorteditem;
    [SerializeField]
    private int[][] c_sorteditems;
    [SerializeField]
    private int[] cor_sorteditem;

    public class IntComparer : Comparer<int>
    {
        public override int Compare(int x, int y)
        {//+ is "x greater than y".
            return x - y;
        }
    }

    // Use this for initialization
    void Start () {
        RunTestSort();
    }

    public void RunTestSort()
    {
        dst_text.text = string.Empty;
        if (random_array)
        {//配列初期化.
            int length = items.Length;
            for (int k = 0; k < length; ++k)
            {
                items[k] = Mathf.FloorToInt(Random.Range(min_int, max_int + 1 - float.Epsilon));
            }
        }
        //for (int k = 0; k < items.Length; ++k)
        //{
        //    dst_text.text += (items[k].ToString("D3") + "\t");
        //}
        //dst_text.text += "\n";
#if true //通常のソート
        List<int> sortlist = new List<int>(items);
        IntComparer com = new IntComparer();
        sortlist.Sort(com);
        sorteditem = sortlist.ToArray();
#endif

        StartCoroutine(Sort());
    }

    IEnumerator Sort()
    {
        SortClass<int> sortclass = new SortClass<int>();
        IntComparer com = new IntComparer();
        int n = 0;
        SortParameter<int> sort_param = null;
        switch (sort_type)
        {
            case SortType.Buble:
                {
                    sort_param = new BubleSortParameter<int>(
                        com,
                        () => { },
                        (state, sorting_items, item_states) =>
                        {
                            ++n;
                            if (dstint_text) { DistSortArray(state, sorting_items, item_states); }
                        }
                        );
                }
                break;
            case SortType.Quick:
            case SortType.RandomQuick:
                {
                    sort_param = new QuickSortParameter<int>(
                        com,
                        () => { },
                        (state, sorting_items, item_states) =>
                        {
                            ++n;
                            if (dstint_text) { DistSortArray(state, sorting_items, item_states); }
                        }
                        );
                }
                break;
            case SortType.Bitonic_NotUseGPGPUDemo:
                {
                    sort_param = new BytonicSortParameter<int>(
                        com,
                        () => { },
                        (state, sorting_items, item_states) =>
                        {
                            ++n;
                            if (dstint_text) { DistSortArray(state, sorting_items, item_states); }
                        }
                        );
                }
                break;
        }
        yield return sortclass.SortUseIEnumerator(
            items,
            sort_param,
            new WaitForSeconds(wait),
            sort_type
        );
        Debug.Log("操作回数：" + n + "回");
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void DistSortArray(SortState state, int[] items, SortItemState[] item_states)
    {
        dst_text.text += state.HasFlag(SortState.Compare) ? "●" : "○";
        dst_text.text += state.HasFlag(SortState.Exchange) ? "●" : "○";
        dst_text.text += state.HasFlag(SortState.Complete) ? "●" : "○";
        dst_text.text += state.HasFlag(SortState.Debug) ? "●" : "○";
        dst_text.text += "\t";
        for (int k = 0; k < items.Length; ++k)
        {
            if (item_states[k].HasFlag(SortItemState.Exchanged))
            {
                dst_text.text += "<color=#ff0000ff>" + (items[k].ToString("D3") + "</color>\t");
            }
            else if (item_states[k].HasFlag(SortItemState.Comparison))
            {
                dst_text.text += "<color=#0000ffff>" + (items[k].ToString("D3") + "</color>\t");
            }
            else if (item_states[k].HasFlag(SortItemState.SortEnd))
            {
                dst_text.text += "<color=#00ff00ff>" + (items[k].ToString("D3") + "</color>\t");
            }
            else if (item_states[k].HasFlag(SortItemState.DisableArea))
            {
                dst_text.text += "<color=#cccccc99>" + (items[k].ToString("D3") + "</color>\t");
            }
            else if (item_states[k].HasFlag(SortItemState.Pivot))
            {
                dst_text.text += "<color=#ff00ffff>" + (items[k].ToString("D3") + "</color>\t");
            }
            else
            {
                dst_text.text += (items[k].ToString("D3") + "\t");
            }
        }
        dst_text.text += "\n";
    }
}
