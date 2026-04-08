using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GovcoreBse.Common.Models;

public static class IdGenerator
{
    private static int _counter;

    private static SemaphoreSlim _lock = new SemaphoreSlim(1);


    public static async void GetLock()
    {
        await _lock.WaitAsync();
    }

    public static void Release()
    {
        _lock.Release();
    }

    public static uint GetNewId()
    {
        uint newId = unchecked((uint)System.Threading.Interlocked.Increment(ref _counter));
        if (newId == 0)
        {
            _counter = 0;
            return GetNewId();
            //throw new System.Exception("Whoops, ran out of identifiers");
        }
        return newId;
    }
}