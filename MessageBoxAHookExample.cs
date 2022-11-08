using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpNativeDLL
{
    public class MessageBoxAHookExample
    {
        private const uint DLL_PROCESS_DETACH = 0,
                           DLL_PROCESS_ATTACH = 1,
                           DLL_THREAD_ATTACH = 2,
                           DLL_THREAD_DETACH = 3;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int MessageBoxA(IntPtr hWnd, string text, string caption, uint type);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);


        [UnmanagedCallersOnly(EntryPoint = "DllMain", CallConvs = new[] { typeof(CallConvStdcall) })]
        public static bool DllMain(IntPtr hModule, uint ul_reason_for_call, IntPtr lpReserved)
        {
            switch (ul_reason_for_call)
            {
                case DLL_PROCESS_ATTACH:
                    HookMessageBoxA();
                    MessageBoxA(0, "DLL_PROCESS_ATTACH", "Your kind @", 0);
                    break;
                default:
                    break;
            }
            return true;
        }

        private static byte[] OriginalFunctionInstructions;
        private static IntPtr OriginalFunctionAddress;

        private static byte[] HookInstructions = new byte[]
        {
            0x49, 0xBA,                                            //movabs r10
            0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA,        //[QWORD]   
            0x41, 0xFF, 0xE2                                       //jmp r10
        };

        private unsafe static void HookMessageBoxA()
        {
            delegate* unmanaged[Cdecl]<nint*, char*, char*, nuint, int> hookedMessageBoxA = &HMessageBoxA;
            OriginalFunctionAddress = NativeLibrary.GetExport(NativeLibrary.Load("User32.dll"), "MessageBoxA");

            //Copy original instructions
            OriginalFunctionInstructions = new byte[HookInstructions.Length];
            fixed (byte* origIptr = OriginalFunctionInstructions)
                CopyMemory((byte*)OriginalFunctionAddress, origIptr, OriginalFunctionInstructions.Length);

            //Hook
            fixed(void* hkIstrPtr = HookInstructions)
            {
                VirtualProtect(OriginalFunctionAddress, (nuint)HookInstructions.Length, 0x40 /*PAGE_EXECUTE_READWRITE*/, out uint oldProt);

                *(IntPtr*)((IntPtr)hkIstrPtr + 2) = (IntPtr)hookedMessageBoxA;

                CopyMemory((byte*)hkIstrPtr, (byte*)OriginalFunctionAddress, HookInstructions.Length);
            }

            Console.WriteLine($"Hooked MessageBoxA at {OriginalFunctionAddress:X}.");
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private unsafe static int HMessageBoxA(nint* handle, char* lpText, char* lpCaption, nuint uType)
        {
            Console.WriteLine($"MessageBoxA called -> handle: {(handle != null ? $"{*handle:X}" : "0")}, {Marshal.PtrToStringAnsi((nint)lpText)}, {Marshal.PtrToStringAnsi((nint)lpCaption)}, {uType:x}");

            //Modify arguments
            string newlpText = "LOL";
            fixed (byte* nStrPtr = Encoding.ASCII.GetBytes(newlpText))
                lpText = (char*)nStrPtr;

            string newlpCaption = "This is hooked";
            fixed (byte* nStrPtr = Encoding.ASCII.GetBytes(newlpCaption))
                lpCaption = (char*)nStrPtr;

            //Unhook
            fixed (void* origIstrPtr = OriginalFunctionInstructions)
                CopyMemory((byte*)origIstrPtr, (byte*)OriginalFunctionAddress, OriginalFunctionInstructions.Length);

            //Get original function
            delegate* unmanaged[Cdecl]<nint*, char*, char*, nuint, int> originalMessageBoxA = (delegate* unmanaged[Cdecl]<nint*, char*, char*, nuint, int>)OriginalFunctionAddress;

            //Hook the function back
            /*int retVal = originalMessageBoxA(handle, lpText, lpCaption, uType);
            
            //Modif return value
            retVal = 69;

            fixed (void* hkInstrPtr = HookInstructions)
                CopyMemory((byte*)hkInstrPtr, (byte*)OriginalFunctionAddress, HookInstructions.Length);

            return retVal;*/

            //Call original function
            return originalMessageBoxA(handle, lpText, lpCaption, uType);
        }

        //memcpy
        private unsafe static void CopyMemory(byte* source, byte* destination, int copyLength)
        {
            byte* pDest = destination;
            byte* pSrc = source;

            for (int i = 0; i < copyLength; i++)
                *(pDest + i) = *(pSrc + i);
        }
    }
}
