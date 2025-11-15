using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FeedBackApp.Core.DomainModels.QuestionTemplateModels
{
    public sealed class AnswerOptions<TValue> : IReadOnlyDictionary<int, TValue>
    {
        private Dictionary<int, TValue> Options { get; } = [];
        private int SeedKey { get; set; } = 1;

        public int Add(TValue value)
        {
            int key = SeedKey++;
            Options[key] = value;
            return key;
        }

        public TValue this[int key] => Options[key];

        public IEnumerable<int> Keys => Options.Keys;

        public IEnumerable<TValue> Values => Options.Values;

        public int Count => Options.Count;

        public bool ContainsKey(int key) => Options.ContainsKey(key);

        public bool TryGetValue(int key, [MaybeNullWhen(false)] out TValue value) =>
            Options.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator() =>
            Options.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IReadOnlyList<int> KeyList => [.. Options.Keys];

        public IReadOnlyList<TValue> ValueList => [.. Options.Values];

        public bool ContainsValue(TValue value) => Options.ContainsValue(value);
    }
}
