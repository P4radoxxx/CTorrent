using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

public static class ObjectSearch
{
    public static int FindIndexByPropertyValue(object[] objects, string propertyName, object targetValue)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            object obj = objects[i];
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);

            if (propertyInfo != null)
            {
                object propertyValue = propertyInfo.GetValue(obj);

                if (object.Equals(propertyValue, targetValue))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public static int FindIndexByPropertyValue<T>(List<T> list, string propertyName, object targetValue)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T item = list[i];

            PropertyInfo propertyInfo = item.GetType().GetProperty(propertyName);

            if (propertyInfo != null)
            {
                object propertyValue = propertyInfo.GetValue(item);

                if (object.Equals(propertyValue, targetValue))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public static int FindIndexByPropertyValue<T>(ObservableCollection<T> collection, string propertyName, object targetValue)
    {
        for (int i = 0; i < collection.Count; i++)
        {
            T item = collection[i];

            PropertyInfo propertyInfo = item.GetType().GetProperty(propertyName);

            if (propertyInfo != null)
            {
                object propertyValue = propertyInfo.GetValue(item);

                if (object.Equals(propertyValue, targetValue))
                {
                    return i;
                }
            }
        }

        return -1;
    }
}
