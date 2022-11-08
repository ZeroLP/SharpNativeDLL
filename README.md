
#  SharpNativeDLL
NativeAOT assisted native .NET DLL injection

This is an example project to inject a NAOT compiled native .NET DLL.
It implements a custom UnmanagedCallersOnly DllMain entrypoint which handles DLL_PROCESS_ATTACH protocol, and can be modified to further accept other protocols.

REQUIREMENTS: Currently .NET 7.0 Preview and Visual Studio 2022 Preview - 17.4.0 Preview 6.0
  
#  Limitations
The limitations for this particular implementation is outlined in the [official documentation for NativeAOT](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/#limitations-of-native-aot-deployment)

The most notable limitation is that it is bound to x64 and ARM64 architectures.


#  Screenshot
![](https://github.com/ZeroLP/SharpNativeDLL/blob/main/Example.png)