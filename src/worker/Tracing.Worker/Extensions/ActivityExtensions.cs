
namespace System.Diagnostics;

public static class ActivityExtensions
{
    public static Activity AddTagIfNew(this Activity activity, string key, object value)
    {
        if (value != null)
        {
            activity.AddTagIfNew(key, value.ToString());
        }
        return activity;
    }

    public static Activity AddTagIfNew(this Activity activity, string key, string value)
    {
        if (!string.IsNullOrEmpty(value) && !activity.Tags.Any(x => x.Key == key))
        {
            activity.AddTag(key, value);
        }
        return activity;
    }
}