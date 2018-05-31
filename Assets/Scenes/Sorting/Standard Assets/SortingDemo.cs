﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Sorting
{
    public enum SortType
    {
        Buble,
        Quick,
        Insert
    }
    public class SortClass
    {
        public SortClass(){ }
        bool[] dirty_items;
        System.Action<bool, bool[]> one_sort_cb;
        System.Action<bool[]> complete_cb;
        WaitForSeconds wait_for_second;

        // コールバックとして、ソート済みリストとダーティフラグを、配列(且つ参照型)で返す.
        /// <summary>
        /// ソート関数
        /// </summary>
        /// <typeparam name="T">ソートを行うオブジェクト</typeparam>
        /// <param name="sort_item">ソートを行う配列(これが直接書き換えられます)</param>
        /// <param name="comparer">ソートルール：Compare(x,y)が正ならばxが後に来るように定義してください</param>
        /// <param name="i_one_sort_cb">
        /// 一回の動作(主に値の入れ替えと入れ替え判定)で呼ばれるコールバック。
        /// 第一引数は、交換が行われたか否か。第二引数は交換された場合のダーティフラグ。
        /// </param>
        /// <param name="i_complete_cb">ソートが完了したときに呼ばれるコールバック</param>
        public void Sort<T>(
            T[] sort_item,
            System.Collections.Generic.Comparer<T> comparer,
            System.Action<bool, bool[]> i_one_sort_cb,
            System.Action<bool[]> i_complete_cb)
        {
            one_sort_cb = i_one_sort_cb;
            complete_cb = i_complete_cb;
            wait_for_second = new WaitForSeconds(0);

            IEnumerator enumerator = IEnumStartSort<T>(sort_item, comparer, Sorting.SortType.Buble);
            RunIEnumerator(enumerator);

            complete_cb(dirty_items);
        }
        public IEnumerator SortUseIEnumerator<T>(
            T[] sort_item,
            System.Collections.Generic.Comparer<T> comparer,
            System.Action<bool, bool[]> i_one_sort_cb,
            System.Action<bool[]> i_complete_cb,
            WaitForSeconds i_wait_for_second)
        {
            one_sort_cb = i_one_sort_cb;
            complete_cb = i_complete_cb;
            wait_for_second = i_wait_for_second;

            yield return IEnumStartSort<T>(sort_item, comparer, Sorting.SortType.Buble);

            complete_cb(dirty_items);
        }


        private void RunIEnumerator(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                object obj = enumerator.Current;
                if (obj is IEnumerator)
                {
                    IEnumerator next_enumerator2 = (IEnumerator)obj;
                    RunIEnumerator(next_enumerator2);
                }
            }
        }
        private IEnumerator IEnumStartSort<T>(T[] sort_item, Comparer<T> comparer, SortType sortType)
        {
            switch (sortType)
            {
                case SortType.Buble:
                    yield return IEnumBubleSort(sort_item, comparer);
                    break;
                case SortType.Insert:
                    break;
                case SortType.Quick:
                    break;
            }
        }
        private IEnumerator IEnumBubleSort<T>(T[] sort_item, Comparer<T> comparer)
        {
#if false
            int item_length = sort_item.Length;
            dirty_items = new bool[item_length];
            int max_itelate = item_length;
            T temp;
            while (max_itelate > 0)
            {
                for (int k = 1; k < max_itelate; ++k)
                {
                    if (comparer.Compare(sort_item[k - 1], sort_item[k]) > 0)
                    {
                        temp = sort_item[k - 1];
                        sort_item[k - 1] = sort_item[k];
                        sort_item[k] = temp;
                        one_sort_cb(true, dirty_items);
                    }
                    else
                    {
                        one_sort_cb(true, dirty_items);
                    }
                    yield return wait_for_second;
                }
                --max_itelate;
            }
#else
            int item_length = sort_item.Length;
            dirty_items = new bool[item_length];
            int max_itelate = item_length;
            int last_change_index = max_itelate;
            T temp;
            while (last_change_index != 0)
            {
                last_change_index = 0;
                for (int k = 1; k < max_itelate; ++k)
                {
                    /*
                    Debug.Log("hikaku " + (k - 1) + " and " + (k) + "\n" +
                       sort_item[k - 1] + " " + sort_item[k] + " => " + comparer.Compare(sort_item[k - 1], sort_item[k]));
                       */
                    if (comparer.Compare(sort_item[k - 1], sort_item[k]) > 0)
                    {
                        temp = sort_item[k - 1];
                        sort_item[k - 1] = sort_item[k];
                        sort_item[k] = temp;
                        last_change_index = k;
                        one_sort_cb(true, dirty_items);
                    }
                    else
                    {
                        one_sort_cb(true, dirty_items);
                    }
                    yield return wait_for_second;
                }
                max_itelate = last_change_index;
            }
#endif
        }
    }
}
