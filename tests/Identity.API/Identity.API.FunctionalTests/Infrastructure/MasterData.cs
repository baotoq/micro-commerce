using System;

namespace Identity.API.FunctionalTests.Infrastructure
{
    public static class MasterData
    {
        public static string CurrentUserId = Guid.NewGuid().ToString();
    }
}
