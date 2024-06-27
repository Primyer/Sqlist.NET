using System.Collections;

namespace Sqlist.NET.Utilities;

/// <summary>
///     Initializes a new instance of the <see cref="BulkParameters"/> class.
/// </summary>
public class BulkParameters(IEnumerable<object> objects) : IEnumerable<KeyValuePair<object?, Type>[]>
{
    private readonly IEnumerable<KeyValuePair<object?, Type>[]> _params = CreateParameters(objects);

    public IEnumerator<KeyValuePair<object?, Type>[]> GetEnumerator()
    {
        return _params.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private static IEnumerable<KeyValuePair<object?, Type>[]> CreateParameters(IEnumerable<object> objects)
    {
        foreach (var obj in objects)
        {
            var oType = obj.GetType();
            var props = oType.GetProperties();
            var array = new KeyValuePair<object?, Type>[props.Length];

            for (var i = 0; i < array.Length; i++)
            {
                var value = props[i].GetValue(obj);
                array[i] = KeyValuePair.Create(value, props[i].PropertyType);
            }

            yield return array;
        }
    }
}
