using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sqlist.NET.Utilities;
public class BulkParameters : IEnumerable<KeyValuePair<object?, Type>[]>
{
    private readonly IEnumerable<KeyValuePair<object?, Type>[]> _params;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BulkParameters"/> class.
    /// </summary>
    public BulkParameters(IEnumerable<object> objects)
    {
        _params = objects.Select(obj =>
        {
            var oType = obj.GetType();
            var props = oType.GetProperties();
            var array = new KeyValuePair<object?, Type>[props.Length];

            for (var i = 0; i < array.Length; i++)
            {
                var value = props[i].GetValue(obj);
                array[i] = KeyValuePair.Create(value, props[i].PropertyType);
            }

            return array;
        });
    }

    public IEnumerator<KeyValuePair<object?, Type>[]> GetEnumerator()
    {
        return _params.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
