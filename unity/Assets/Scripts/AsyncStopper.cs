using UnityEngine;
using System.Reflection;
using System.Threading;
using System;

[DisallowMultipleComponent]
public class AsyncStopper : MonoBehaviour
{
    void OnDestroy()
    {
        SetNewSyncContext();
    }

    static void SetNewSyncContext()
    {
        var constructor = SynchronizationContext.Current.GetType()
            .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {typeof(int)}, null);
        if (constructor == null) return;

        var newContext = constructor.Invoke(new object[] {Thread.CurrentThread.ManagedThreadId});
        SynchronizationContext.SetSynchronizationContext(newContext as SynchronizationContext);
    }
}