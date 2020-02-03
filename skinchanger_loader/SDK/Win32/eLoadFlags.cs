namespace skinchanger_loader.SDK.Win32
{
    internal enum eLoadFlags
    {
        NoFlags = 0x00,     // No flags
        ManualImports = 0x01,     // Manually map import libraries
        CreateLdrRef = 0x02,     // Create module references for native loader
        WipeHeader = 0x04,     // Wipe image PE headers
        HideVAD = 0x10,     // Make image appear as PAGE_NOACESS region
        MapInHighMem = 0x20,     // Try to map image in address space beyond 4GB limit
        RebaseProcess = 0x40,     // If target image is an .exe file, process base address will be replaced with mapped module value
        NoThreads = 0x80,     // Don't create new threads, use hijacking
        ForceRemap = 0x100,    // Force remapping module even if it's already loaded

        NoExceptions = 0x01000,  // Do not create custom exception handler
        PartialExcept = 0x02000,  // Only create Inverted function table, without VEH
        NoDelayLoad = 0x04000,  // Do not resolve delay import
        NoSxS = 0x08000,  // Do not apply SxS activation context
        NoTLS = 0x10000,  // Skip TLS initialization and don't execute TLS callbacks
        IsDependency = 0x20000,  // Module is a dependency
    };
}