using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
public static class EnumExtensions
{
    /// <summary>
    /// 現在のインスタンスで 1 つ以上のビット フィールドが設定されているかどうかを判断します
    /// </summary>
    public static bool HasFlag(this Enum self, Enum flag)
    {
        if (self.GetType() != flag.GetType())
        {
            throw new ArgumentException("flag の型が、現在のインスタンスの型と異なっています。");
        }
        var selfValue = Convert.ToUInt64(self);
        var flagValue = Convert.ToUInt64(flag);
        return (selfValue & flagValue) == flagValue;
    }
}
namespace Sorting
{
    public enum SortItemState
    {
        NoState = 0,
        Comparison = 1,//比較対象
        Exchanged = 1 << 1,//交換対象
        Pivot = 1 << 2,//クイックソートなど、特定の比較対象がある場合のフラグ
        DisableArea = 1 << 3,//分割統治法や特定のブロックごとに比較方法が変わる際に、使用しないエリアやブロック。
        SortEnd = 1 << 4,//ソート終了
        Debug = 1 << 8,//デバッグ
    }
    public enum SortState
    {
        NoState = 0,
        Compare = 1,//比較
        Exchange = 1 << 2,//交換
        Complete = 1 << 3,//完了
        Start = 1 << 4,//最初
        Debug = 1 << 8,//デバッグ
    }
    public enum SortType
    {
        Buble,
        Quick,
        RandomQuick,
        Insert,
        Bitonic_UseGPGPUDemo,
        Bitonic_NotUseGPGPUDemo,
    }
    /// <summary>アクションに対するコールバック</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sort_state"></param>
    /// <param name="sort_items"></param>
    /// <param name="sort_item_states"></param>
    public delegate void SendActionMessage<T>(SortState sort_state, T[] sort_items, SortItemState[] sort_item_states);
    /// <summary>整列終了のコールバック</summary>
    public delegate void SendCompleteMessage();
    
    public  class SortParameter<T>
    {
        public SendCompleteMessage cb_complete = new SendCompleteMessage(() => { });
        public SendActionMessage<T> cb_action = new SendActionMessage<T>((a,b,c) => { });
        public Comparer<T> comparer;
        protected SortParameter() { }
        public SortParameter(Comparer<T> comparer)
        {
            this.comparer = comparer;
        }
        public SortParameter(Comparer<T> comparer, Action cb_complete_rmd, Action<SortState, T[], SortItemState[]> cb_action_rmd)
            : this(comparer)
        {
            this.cb_complete = new SendCompleteMessage(cb_complete_rmd);
            this.cb_action = new SendActionMessage<T>(cb_action_rmd);
        }
    }
    public class BubleSortParameter<T> : SortParameter<T>
    {
        private BubleSortParameter() { }
        public BubleSortParameter(Comparer<T> comparer, Action cb_complete_rmd, Action<SortState, T[], SortItemState[]> cb_action_rmd)
            : base(comparer, cb_complete_rmd, cb_action_rmd) { }
    }
    public class QuickSortParameter<T> : SortParameter<T>
    {
        private QuickSortParameter() { }
        public QuickSortParameter(Comparer<T> comparer, Action cb_complete_rmd, Action<SortState, T[], SortItemState[]> cb_action_rmd)
            : base(comparer, cb_complete_rmd, cb_action_rmd) { }
    }
    public class BytonicSortParameter<T> : SortParameter<T>
    {
        private BytonicSortParameter() { }
        public BytonicSortParameter(Comparer<T> comparer, Action cb_complete_rmd, Action<SortState, T[], SortItemState[]> cb_action_rmd)
            : base(comparer, cb_complete_rmd, cb_action_rmd) { }
    }
    /// <summary>
    /// 整列クラス
    /// </summary>
    /// <typeparam name="T">ソートを行うオブジェクト</typeparam>
    public class SortClass<T>
    {
        T swap_temp;
        public SortClass() { }
        SortItemState[] dirty_items;
        YieldInstruction waiter;
        // コールバックとして、ソート済みリストとダーティフラグを、配列(且つ参照型)で返す.
        /// <summary>
        /// ソート関数
        /// </summary>
        /// <param name="sort_item">ソートを行う配列(これが直接書き換えられます)</param>
        /// <param name="comparer">ソートルール：Compare(x,y)が正ならばxが後に来るように定義してください</param>
        /// <param name="i_one_sort_cb">
        /// 一回の動作(主に値の入れ替えと入れ替え判定)で呼ばれるコールバック。
        /// 第一引数は、交換が行われたか否か。第二引数は交換された場合のダーティフラグ。
        /// </param>
        /// <param name="i_complete_cb">ソートが完了したときに呼ばれるコールバック</param>
        /// <param name="sort_type">ソートの種類</param>
        public void Sort(T[] sort_item, Comparer<T> comparer,
            SortParameter<T> sort_parameter, Sorting.SortType sort_type)
        {
            IEnumerator enumerator = SortUseIEnumerator(sort_item, sort_parameter, new WaitForSeconds(0), sort_type);
            RunIEnumerator(enumerator);
        }
        /// <summary>
        /// ソート関数
        /// </summary>
        /// <param name="sort_item">ソートを行う配列(これが直接書き換えられます)</param>
        /// <param name="comparer">ソートルール：Compare(x,y)が正ならばxが後に来るように定義してください</param>
        /// <param name="i_one_sort_cb">
        /// 一回の動作(主に値の入れ替えと入れ替え判定)で呼ばれるコールバック。
        /// 第一引数は、交換が行われたか否か。第二引数は交換された場合のダーティフラグ。
        /// </param>
        /// <param name="i_complete_cb">ソートが完了したときに呼ばれるコールバック</param>
        /// <param name="i_wait_for_second">一回のソートで待つ条件</param>
        /// <param name="sort_type">ソートの種類</param>
        /// <returns></returns>
        public IEnumerator SortUseIEnumerator(
            T[] sort_item,
            SortParameter<T> sort_parameter,
            WaitForSeconds i_wait_for_second, Sorting.SortType sort_type)
        {
            waiter = i_wait_for_second;
            if (sort_item.Length > 1)
            {
                yield return IEnumStartSort(sort_item, sort_parameter, sort_type);

                sort_parameter.cb_complete();
            }
        }

        /// <summary>
        /// 同期処理(非コルーチン)でIEnumeratorを実行する。
        /// </summary>
        /// <remarks>
        /// yield returnでIEnumeratorが返されれば、それも再帰的に実行する。
        /// Wait関連は無視される。
        /// </remarks>
        /// <param name="enumerator">動かすIEnumerator</param>
        private void RunIEnumerator(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is IEnumerator)
                {
                    RunIEnumerator((IEnumerator)enumerator.Current);
                }
            }
        }
        private IEnumerator IEnumStartSort(T[] sort_item, SortParameter<T> sort_parameter, SortType sortType)
        {
            dirty_items = new SortItemState[sort_item.Length];
            sort_parameter.cb_action(SortState.Start, sort_item, dirty_items);
            switch (sortType)
            {
                case SortType.Buble:
                    if (sort_parameter is BubleSortParameter<T>)
                    {
                        yield return IEnumBubleSort(sort_item, sort_parameter as BubleSortParameter<T>);
                    }
                    else
                    {
                        //エラーを入れたい
                    }
                    break;
                case SortType.Insert:
                    break;
                case SortType.Bitonic_NotUseGPGPUDemo:
                    if (sort_parameter is BytonicSortParameter<T>)
                    {
                        if (Mathf.IsPowerOfTwo(sort_item.Length))
                        {
                            yield return IEnumBitonicNotUseGPGPUDemo(sort_item, sort_parameter as BytonicSortParameter<T>, 0, sort_item.Length);
                        }
                        else
                        {
                            //アイテム数の、べき乗切り上げ.
                            int itemlength_pow2 = Mathf.NextPowerOfTwo(sort_item.Length);
                            //アイテム数の、べき乗切り上げの指数.
                            int itemlength_log2 = Mathf.RoundToInt(Mathf.Log(itemlength_pow2, 2));
                            //最大値探索
                            int max_index = 0;
                            for (int k = 1; k < sort_item.Length; ++k)
                            {
                                if (sort_parameter.comparer.Compare(sort_item[k], sort_item[max_index]) > 0)
                                {
                                    max_index = k;
                                }
                            }
                            //2の冪要素数の配列作成
                            T[] sort_item_org = sort_item;
                            sort_item = new T[itemlength_pow2];
                            System.Array.Copy(sort_item_org, sort_item, sort_item_org.Length);
                            for (int k = sort_item_org.Length; k < sort_item.Length; ++k)
                            {
                                sort_item[k] = sort_item[max_index];
                            }
                            SortItemState[] dirty_items_origin = dirty_items;
                            dirty_items = new SortItemState[sort_item.Length];
                            yield return IEnumBitonicNotUseGPGPUDemo(sort_item, sort_parameter as BytonicSortParameter<T>, 0, sort_item.Length);
                            System.Array.Copy(sort_item, sort_item_org, sort_item_org.Length);
                            System.Array.Copy(dirty_items, dirty_items_origin, dirty_items_origin.Length);
                            sort_item = sort_item_org;
                        }
                    }
                    break;
                case SortType.Quick:
                    if (sort_parameter is QuickSortParameter<T>)
                    {
                        yield return IEnumQuickSort(sort_item, sort_parameter as QuickSortParameter<T>, 0, sort_item.Length);
                    }
                    break;
                case SortType.RandomQuick:
                    if (sort_parameter is QuickSortParameter<T>)
                    {
                        yield return IEnumRandomQuickSort(sort_item, sort_parameter as QuickSortParameter<T>, 0, sort_item.Length);
                    }
                    break;
            }
            sort_parameter.cb_action(SortState.Complete, sort_item, dirty_items);
        }
        private IEnumerator IEnumBubleSort(T[] sort_item, BubleSortParameter<T> sort_parameter)
        {
            int item_length = sort_item.Length;
            int max_itelate = item_length;
            int last_change_index = max_itelate;
            Action<int, int> Exchange = (l_tmp, r_tmp) => {
                swap_temp = sort_item[l_tmp]; sort_item[l_tmp] = sort_item[r_tmp]; sort_item[r_tmp] = swap_temp;
                last_change_index = r_tmp;
                dirty_items[l_tmp] = dirty_items[r_tmp] |= SortItemState.Exchanged;
                sort_parameter.cb_action(SortState.Exchange | SortState.Compare, sort_item, dirty_items);
                dirty_items[l_tmp] = dirty_items[r_tmp] &= ~SortItemState.Exchanged;
            };
            while (last_change_index != 0)
            {
                last_change_index = 0;
                for (int k = 1; k < max_itelate; ++k)
                {
                    dirty_items[k - 1] = dirty_items[k] |= SortItemState.Comparison;
                    if (sort_parameter.comparer.Compare(sort_item[k - 1], sort_item[k]) > 0)
                    {
                        Exchange(k-1, k);
                    }
                    else
                    {
                        sort_parameter.cb_action(SortState.Compare, sort_item, dirty_items);
                    }
                    dirty_items[k - 1] = dirty_items[k] &= ~SortItemState.Comparison;
                    yield return waiter;
                }
                for (int k = last_change_index; k < max_itelate; ++k)
                {
                    dirty_items[k] = SortItemState.SortEnd;
                }
                max_itelate = last_change_index;
            }
        }

        /// <summary>
        /// クイックソートのベース関数
        /// </summary>
        /// <param name="sort_item">[in,out]ソートアイテム</param>
        /// <param name="comparer">[in]比較演算</param>
        /// <param name="left">[in]ソートする左端(含む)</param>
        /// <param name="right">[in]ソートする右端(含まず)</param>
        /// <param name="pivot">[in]比較対象値</param>
        /// <param name="next_pivot_index">[out]比較対象との境界位置(左側列の再右端要素)</param>
        /// <returns></returns>
        private IEnumerator IEnumQuickSortBase(T[] sort_item, QuickSortParameter<T> sort_parameter, int left, int right, int pivot, int[] next_pivot_index)
        {
            int back_ite = right;
            int dst_pivot = back_ite;
            int front_ite = left - 1;
            while (true)
            {//両端から、入れ替えの必要な二つを探して交換する
                if(front_ite >= left) { dirty_items[front_ite] |= SortItemState.Comparison; }
                while (true)
                {
                    if (front_ite + 1 < back_ite)
                    {
                        --back_ite;
                        dirty_items[back_ite] |= SortItemState.Comparison;
                        sort_parameter.cb_action(SortState.Compare, sort_item, dirty_items);
                        dirty_items[back_ite] &= ~SortItemState.Comparison;
                        if (sort_parameter.comparer.Compare(sort_item[pivot], sort_item[back_ite]) > 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        next_pivot_index[0] = back_ite - 1;
                        if (front_ite >= left) { dirty_items[front_ite] &= ~SortItemState.Comparison; }
                        yield break;
                    }
                }
                if (front_ite >= left) { dirty_items[front_ite] &= ~SortItemState.Comparison; }
                dirty_items[back_ite] |= SortItemState.Comparison;
                while (true)
                {
                    if (front_ite + 1 < back_ite)
                    {
                        ++front_ite;
                        dirty_items[front_ite] |= SortItemState.Comparison;
                        sort_parameter.cb_action(SortState.Compare, sort_item, dirty_items);
                        dirty_items[front_ite] &= ~SortItemState.Comparison;
                        if (sort_parameter.comparer.Compare(sort_item[pivot], sort_item[front_ite]) < 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        next_pivot_index[0] = back_ite;
                        dirty_items[back_ite] &= ~SortItemState.Comparison;
                        yield break;
                    }
                }
                dirty_items[back_ite] &= ~SortItemState.Comparison;
                swap_temp = sort_item[front_ite]; sort_item[front_ite] = sort_item[back_ite]; sort_item[back_ite] = swap_temp;
                dirty_items[front_ite] = dirty_items[back_ite] |= SortItemState.Exchanged | SortItemState.Comparison;
                sort_parameter.cb_action(SortState.Exchange, sort_item, dirty_items);
                dirty_items[front_ite] = dirty_items[back_ite] &= ~(SortItemState.Exchanged | SortItemState.Comparison);
                yield return waiter;
            }
        }
        private IEnumerator IEnumQuickSort(T[] sort_item, QuickSortParameter<T> sort_parameter, int left, int right)
        {
            int[] pivot_index = new int[1];
            //先頭に対して、大小を分ける。.
            dirty_items[left] = SortItemState.Pivot;
            yield return IEnumQuickSortBase(sort_item, sort_parameter, left + 1, right, left, pivot_index);
            dirty_items[left] = SortItemState.NoState;

            int back_ite = pivot_index[0] - 1;
            if (pivot_index[0] <= left) { }
            else
            {
                {//先頭と境界を入れ替える
                    swap_temp = sort_item[left]; sort_item[left] = sort_item[pivot_index[0]]; sort_item[pivot_index[0]] = swap_temp;
                    dirty_items[left] = dirty_items[pivot_index[0]] |= SortItemState.Exchanged | SortItemState.Comparison;
                    sort_parameter.cb_action(SortState.Exchange, sort_item, dirty_items);
                    dirty_items[left] = dirty_items[pivot_index[0]] &= ~(SortItemState.Exchanged | SortItemState.Comparison);
                    yield return waiter;
                }

            }
            dirty_items[pivot_index[0]] = SortItemState.SortEnd;
            if (pivot_index[0] > left)
            {
                for (int k = right - 1; k > pivot_index[0]; --k)
                {
                    dirty_items[k] |= SortItemState.DisableArea;
                }
                yield return IEnumQuickSort(sort_item, sort_parameter, left, pivot_index[0]);
                for (int k = right - 1; k > pivot_index[0]; --k)
                {
                    dirty_items[k] &= ~SortItemState.DisableArea;
                }
            }
            //この時点で左は完成してる
            if (pivot_index[0] + 1 < right)
            {
                yield return IEnumQuickSort(sort_item, sort_parameter, pivot_index[0] + 1, right);
            }
        }
        private IEnumerator IEnumRandomQuickSort(T[] sort_item, QuickSortParameter<T> sort_parameter, int left, int right)
        {
            int[] pivot_index = new int[1];
            {
                int rand_index = Mathf.FloorToInt(UnityEngine.Random.Range(left, right - float.Epsilon));
                if(left != rand_index)
                {
                    swap_temp = sort_item[left]; sort_item[left] = sort_item[rand_index]; sort_item[rand_index] = swap_temp;
                    dirty_items[left] = dirty_items[rand_index] |= SortItemState.Exchanged | SortItemState.Comparison;
                    sort_parameter.cb_action(SortState.Exchange, sort_item, dirty_items);
                    dirty_items[left] = dirty_items[rand_index] &= ~(SortItemState.Exchanged | SortItemState.Comparison);
                }
                else
                {
                    dirty_items[rand_index] |= SortItemState.Comparison;
                    sort_parameter.cb_action(SortState.Compare, sort_item, dirty_items);
                    dirty_items[rand_index] &= ~SortItemState.Comparison;
                }
            }
            //先頭に対して、大小を分ける。.
            dirty_items[left] |= SortItemState.Pivot;
            yield return IEnumQuickSortBase(sort_item, sort_parameter, left + 1, right, left, pivot_index);
            dirty_items[left] &= ~SortItemState.Pivot;

            int back_ite = pivot_index[0] - 1;
            if (pivot_index[0] <= left) { }
            else
            {
                {//先頭と境界を入れ替える
                    swap_temp = sort_item[left]; sort_item[left] = sort_item[pivot_index[0]]; sort_item[pivot_index[0]] = swap_temp;
                    dirty_items[left] = dirty_items[pivot_index[0]] |= SortItemState.Exchanged | SortItemState.Comparison;
                    sort_parameter.cb_action(SortState.Exchange, sort_item, dirty_items);
                    dirty_items[left] = dirty_items[pivot_index[0]] &= ~(SortItemState.Exchanged | SortItemState.Comparison);
                    yield return waiter;
                }

            }
            dirty_items[pivot_index[0]] = SortItemState.SortEnd;
            if (pivot_index[0] > left)
            {
                for (int k = right - 1; k > pivot_index[0]; --k)
                {
                    dirty_items[k] |= SortItemState.DisableArea;
                }
                yield return IEnumRandomQuickSort(sort_item, sort_parameter, left, pivot_index[0]);
                for (int k = right - 1; k > pivot_index[0]; --k)
                {
                    dirty_items[k] &= ~SortItemState.DisableArea;
                }
            }
            //この時点で左は完成してる
            if (pivot_index[0] + 1 < right)
            {
                yield return IEnumRandomQuickSort(sort_item, sort_parameter, pivot_index[0] + 1, right);
            }
        }

        private IEnumerator IEnumBitonicNotUseGPGPUDemo(T[] sort_item, BytonicSortParameter<T> sort_parameter, int left, int right)
        {
            int item_length = sort_item.Length;
            // 2の冪乗でなければ計算しない(5)
            if (((item_length - 1) & item_length) != 0) { yield break; }
            for (int d_i = 0; d_i < dirty_items.Length; ++d_i)
            {
                dirty_items[d_i] |= SortItemState.DisableArea;
            }
            Action<int, int> ExchangeItemEvent = (l_tmp, r_tmp) => {
                dirty_items[l_tmp] = dirty_items[r_tmp] |= SortItemState.Exchanged;
                sort_parameter.cb_action(SortState.Exchange | SortState.Compare, sort_item, dirty_items);
                dirty_items[l_tmp] = dirty_items[r_tmp] &= ~SortItemState.Exchanged;
            };
            Action NoExchangeItemEvent = () => {
                sort_parameter.cb_action(SortState.Compare, sort_item, dirty_items);
            };
            int comp_l, comp_r, block, step;
            for (block = 2; block <= item_length; block *= 2)//各ブロックの大きさ.
            {
                for (step = block / 2; step >= 1; step /= 2)
                {
                    for (comp_l = 0; comp_l < item_length; comp_l++)
                    {
                        comp_r = comp_l ^ step; // (1)
                        if (comp_r > comp_l)
                        { // (2)
                            dirty_items[comp_l] = dirty_items[comp_r] |= SortItemState.Comparison;
                            T v1 = sort_item[comp_l];T v2 = sort_item[comp_r];
                            if ((comp_l & block) != 0)
                            { // (3)
                                if (sort_parameter.comparer.Compare (v1, v2) < 0)
                                { // (4)
                                    sort_item[comp_r] = v1; sort_item[comp_l] = v2;
                                    ExchangeItemEvent(comp_l, comp_r);
                                }
                                else { NoExchangeItemEvent(); }
                            }
                            else
                            {
                                if (sort_parameter.comparer.Compare(v1, v2) > 0)
                                {
                                    sort_item[comp_r] = v1; sort_item[comp_l] = v2;
                                    ExchangeItemEvent(comp_l, comp_r);
                                }
                                else { NoExchangeItemEvent(); }
                            }
                            dirty_items[comp_l] = dirty_items[comp_r] &= ~SortItemState.Comparison;
                            yield return waiter;
                        }
                    }
                }
            }
            for (int d_i = 0; d_i < dirty_items.Length; ++d_i)
            {
                dirty_items[d_i] = SortItemState.SortEnd;
            }
            yield return waiter;
        }
    }
}
