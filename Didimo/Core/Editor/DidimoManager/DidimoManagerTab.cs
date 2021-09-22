using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public abstract class DidimoManagerTab
{
    protected const int PADDING       = 10;
    protected const int PADDING_SMALL = 5;

    public abstract void Draw(DidimoManager manager);
    public abstract string GetTabName();
    public abstract int GetIndex();

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void DidReloadScripts()
    {
        // Get all types that inherit DidimoManagerTab, and register them as tabs for the DidimoManager
        var type = typeof(DidimoManagerTab);
        List<Type> tabTypes = new List<Type>();
        IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("Didimo"));
        foreach (Assembly assembly in assemblies)
        {
            try
            {
                Type[] types = assembly.GetTypes().Where(t => type.IsAssignableFrom(t) && type != t).ToArray();
                tabTypes.AddRange(types);
            }
            catch (ReflectionTypeLoadException e)
            {
                Type[] types = e.Types.Where(p => type.IsAssignableFrom(p) && type != p).ToArray();
                tabTypes.AddRange(types);
            }
        }

        foreach (var tabType in tabTypes.Where(t => t != null))
        {
            DidimoManager.AddTab(Activator.CreateInstance(tabType) as DidimoManagerTab);
        }
    }
}