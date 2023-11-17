using System;
using System.Threading;

namespace DtronixPdf;

/// <summary>
/// The 'PdfActionSynchronizer' class is responsible for synchronizing the execution of actions and functions in a thread-safe manner.
/// It uses a semaphore to ensure that only one thread can execute the action or function at a time.
/// This class is used throughout the DtronixPdf library to ensure safe and synchronized access to shared resources.
/// </summary>
public class PdfActionSync
{
    public static PdfActionSync Default { get; } = new PdfActionSync();

    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public void SyncExec(Action action)
    {
        try
        {
            _semaphore.Wait();
            action();
        }
        catch
        {
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public T SyncExec<T>(Func<T> function)
    {
        try
        {
            _semaphore.Wait();
            var result = function();
            return result;
        }
        catch
        {
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public TRet SyncExec<TArg1, TRet>(Func<TArg1, TRet> function, TArg1 arg1)
    {
        try
        {
            _semaphore.Wait();
            var result = function(arg1);
            return result;
        }
        catch
        {
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public TRet SyncExec<TArg1, TArg2, TRet>(Func<TArg1, TArg2, TRet> function, TArg1 arg1, TArg2 arg2)
    {
        try
        {
            _semaphore.Wait();
            var result = function(arg1, arg2);
            return result;
        }
        catch
        {
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
