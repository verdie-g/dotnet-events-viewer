using System.Collections;

namespace NetTrace;

/// <summary>Shares field-name lookups across events with the same layout; only values are per event.</summary>
internal sealed class EventPayload(EventPayload.Layout layout, object[] values) : IReadOnlyDictionary<string, object>
{
    public int Count => values.Length;
    public IEnumerable<string> Keys => layout.Indexes.Keys;
    public IEnumerable<object> Values => values;
    public object this[string key] => values[layout.Indexes[key]];
    public bool ContainsKey(string key) => layout.Indexes.ContainsKey(key);

    public bool TryGetValue(string key, out object value)
    {
        if (layout.Indexes.TryGetValue(key, out int index))
        {
            value = values[index];
            return true;
        }
        value = default!;
        return false;
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        for (int i = 0; i < values.Length; i++)
            yield return new KeyValuePair<string, object>(layout.Names[i], values[i]);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal sealed class Layout
    {
        public string[] Names { get; }
        public Dictionary<string, int> Indexes { get; } = new(StringComparer.Ordinal);
        public int[] ValueIndexes { get; }

        public Layout(IReadOnlyList<EventFieldDefinition> fields)
        {
            ValueIndexes = new int[fields.Count];
            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (field.TypeCode == NetTraceTypeCode.Padding32)
                {
                    ValueIndexes[i] = -1;
                    continue;
                }
                if (!Indexes.TryGetValue(field.Name, out int index))
                {
                    index = Indexes.Count;
                    Indexes.Add(field.Name, index);
                }
                ValueIndexes[i] = index;
            }
            Names = Indexes.Keys.ToArray();
        }
    }
}
