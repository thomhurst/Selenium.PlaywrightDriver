using System.Threading.Tasks;

namespace TomLonghurst.Selenium.PlaywrightWebDriver.Extensions;

public static class TaskExtensions
{
    public static T Synchronously<T>(this Task<T> task)
    {
        return task.GetAwaiter().GetResult();
    }
    
    public static void Synchronously(this Task task)
    {
        task.GetAwaiter().GetResult();
    }
}