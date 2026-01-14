using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AuthBI.Helpers
{
    public static class TempDataExtensions
    {
        public static void Success(this ITempDataDictionary tempData, string message)
        {
            tempData["Success"] = message;
        }

        public static void Error(this ITempDataDictionary tempData, string message)
        {
            tempData["Error"] = message;
        }
    }
}
