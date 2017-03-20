using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DJIProtocolTest.Models.Helpers
{
    public static class MyDebug
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        /// <summary>
        /// Writes a new line to the debugger output. Includes caller name and line number.
        /// </summary>
        public static void WriteLine(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0,
    [CallerMemberName] string caller = null)
        {
            Debug.WriteLine($"{message} [{caller}/line:{lineNumber}]");
        }

        /// <summary>
        /// Writes a new line to the console.
        /// </summary>
        public static void ConsoleWriteLine(string message)
        {
            AllocConsole();
            Console.WriteLine(message);
        }

        /// <summary>
        /// Appends a string to the console.
        /// </summary>
        public static void ConsoleWrite(string message)
        {
            AllocConsole();
            Console.Write(message);
        }

        public static void ToFile(string message)
        {

        }

    }
}