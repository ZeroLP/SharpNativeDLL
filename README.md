#  SharpNativeDLL
NativeAOT assisted native .NET DLL injection

This is an example project to inject a NAOT compiled native .NET DLL.
It implements a custom UnmanagedCallersOnly DllMain entrypoint which handles DLL_PROCESS_ATTACH protocol, and can be modified to further accept other protocols. 

# Screenshot
![](https://github.com/ZeroLP/SharpNativeDLL/blob/main/Example.png)