using System.Collections.Generic;
using UnityEngine;

public abstract class DataListSO<TEntry, TValue> : ScriptableObject
{
    protected abstract string getID(TEntry entry);
    protected abstract TValue getValue(TEntry entry);


    public List<TEntry> dataList;
    public Dictionary<string, TValue> dataDic;

    public void initialize()
    {
        dataDic = new Dictionary<string, TValue>();
        foreach (var entry in dataList)
        {
            string id = getID(entry);
            var value = getValue(entry);
            if (!string.IsNullOrEmpty(id) && !dataDic.ContainsKey(id))
            {
                dataDic.Add(id, value);
            }
        }
    }

    public TValue getData(string id)
    {
        if (dataDic == null)
            initialize();

        if (dataDic.TryGetValue(id, out TValue value))
        {
            return value;
        }

        return default;
    }
}
