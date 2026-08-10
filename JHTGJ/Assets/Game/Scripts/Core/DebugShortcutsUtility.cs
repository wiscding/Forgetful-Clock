namespace JHTGJ.Core
{
    public static class DebugShortcutsUtility
    {
        public static bool IsActive(bool enabledInInspector)
        {
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            return false;
#else
            return enabledInInspector;
#endif
        }
    }
}
