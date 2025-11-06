using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xeon.Localization
{
    /// <summary>
    /// キー参照可能な ScriptableObject ベースデータの共通処理基底クラス
    /// </summary>
    /// <typeparam name="TEntry">IKeyEntry 実装型</typeparam>
    public abstract class KeyedDatabaseBase<TEntry> : ScriptableObject where TEntry : IKeyEntry
    {
        /// <summary>インスペクタ編集用の生エントリ配列</summary>
        [SerializeField, Tooltip("エントリ一覧 (インスペクタ編集用)")] protected TEntry[] serializedEntries;
        /// <summary>OnEnable 時に自動キャッシュ構築するか</summary>
        [SerializeField, Tooltip("OnEnable 時に自動ビルド")] protected bool buildCacheOnEnable = true;
        /// <summary>重複キーは後勝ちか 先勝ちは false</summary>
        [SerializeField, Tooltip("重複キー後勝ち (true) / 先勝ち (false)")] protected bool duplicateKeyLastWins = true;

        /// <summary>ソート済み一意エントリ配列</summary>
        protected TEntry[] sortedUniqueEntries = Array.Empty<TEntry>();
        /// <summary>キャッシュ構築済みか</summary>
        protected bool isCacheBuilt;
        /// <summary>直前検索キー</summary>
        protected string lastSearchedKey;
        /// <summary>直前検索インデックス</summary>
        protected int lastSearchedIndex = -1;

        protected virtual void OnEnable()
        {
            if (buildCacheOnEnable)
            {
                BuildCacheIfNeeded();
            }
        }

        /// <summary>事前ウォームアップ用メソッド</summary>
        public void Prewarm() => BuildCacheIfNeeded(force: true);

        /// <summary>キャッシュを必要に応じて構築する</summary>
        protected void BuildCacheIfNeeded(bool force = false)
        {
            if (isCacheBuilt && !force)
            {
                return;
            }
            if (serializedEntries == null || serializedEntries.Length == 0)
            {
                sortedUniqueEntries = Array.Empty<TEntry>();
                isCacheBuilt = true;
                lastSearchedKey = null;
                lastSearchedIndex = -1;
                return;
            }
            var filteredEntries = new List<TEntry>(serializedEntries.Length);
            foreach (var entry in serializedEntries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Key))
                {
                    continue;
                }
                filteredEntries.Add(entry);
            }
            filteredEntries.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
            var uniqueEntries = new List<TEntry>(filteredEntries.Count);
            TEntry pendingEntry = default;
            string currentKey = null;
            bool hasPendingEntry = false;
            for (int i = 0; i < filteredEntries.Count; i++)
            {
                var currentEntry = filteredEntries[i];
                if (currentKey == null || !string.Equals(currentEntry.Key, currentKey, StringComparison.Ordinal))
                {
                    if (hasPendingEntry)
                    {
                        uniqueEntries.Add(pendingEntry);
                    }
                    currentKey = currentEntry.Key;
                    pendingEntry = currentEntry;
                    hasPendingEntry = true;
                }
                else if (duplicateKeyLastWins)
                {
                    pendingEntry = currentEntry; // 後勝ち
                }
            }
            if (hasPendingEntry)
            {
                uniqueEntries.Add(pendingEntry);
            }
            sortedUniqueEntries = uniqueEntries.ToArray();
            isCacheBuilt = true;
            lastSearchedKey = null;
            lastSearchedIndex = -1;
            AfterBuild();
        }

        /// <summary>
        /// ビルド完了後フック。派生側で追加初期化が必要ならオーバーライド。
        /// </summary>
        protected virtual void AfterBuild() { }

        /// <summary>キーをキャッシュ付きで探索する</summary>
        protected int FindKeyIndexCached(string key)
        {
            if (lastSearchedIndex >= 0 && lastSearchedKey == key)
            {
                return lastSearchedIndex;
            }
            int index = BinarySearchKey(key);
            if (index >= 0)
            {
                lastSearchedKey = key;
                lastSearchedIndex = index;
            }
            return index;
        }

        /// <summary>二分探索でキーのインデックスを返す 見つからなければ -1</summary>
        protected int BinarySearchKey(string key)
        {
            int low = 0;
            int high = sortedUniqueEntries.Length - 1;
            while (low <= high)
            {
                int middleIndex = low + ((high - low) >> 1);
                int comparison = string.Compare(sortedUniqueEntries[middleIndex].Key, key, StringComparison.Ordinal);
                if (comparison == 0)
                {
                    return middleIndex;
                }
                if (comparison < 0)
                {
                    low = middleIndex + 1;
                }
                else
                {
                    high = middleIndex - 1;
                }
            }
            return -1;
        }

        /// <summary>キーに対応するエントリを取得する</summary>
        protected bool TryGetEntry(string key, out TEntry foundEntry)
        {
            foundEntry = default;
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            if (!isCacheBuilt)
            {
                BuildCacheIfNeeded();
            }
            int index = FindKeyIndexCached(key);
            if (index < 0)
            {
                return false;
            }
            foundEntry = sortedUniqueEntries[index];
            return true;
        }

        /// <summary>キーの存在を判定する</summary>
        public bool ContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            if (!isCacheBuilt)
            {
                BuildCacheIfNeeded();
            }
            return FindKeyIndexCached(key) >= 0;
        }
    }
}
