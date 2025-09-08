using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Runtime.InteropServices;

namespace ZebraSender
{
    class Program
    {
        // --- Winspool imports & structs ---
        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", SetLastError = true)]
        static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool StartDocPrinter(IntPtr hPrinter, int level, ref DOC_INFO_1 di);

        [DllImport("winspool.drv", SetLastError = true)]
        static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", SetLastError = true)]
        static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct DOC_INFO_1
        {
            public string pDocName;
            public string pOutputFile;
            public string pDatatype; // must be "RAW" for ZPL
        }

        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No file path provided. Usage: ZebraSender <file.ykss|file.zpl>");
                return 1;
            }

            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File not found: {filePath}");
                return 1;
            }

            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "printer_config.json");
            string printerName = LoadPrinterName(configPath);

            try
            {
                // Read raw bytes (avoid BOM/encoding surprises).
                // If your .ykss is actually ZPL text, this is fine.
                // If it’s a wrapper format, transform it to ZPL bytes here before sending.
                byte[] data = File.ReadAllBytes(filePath);

                // Optional: strip UTF-8 BOM if present
                if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
                {
                    byte[] withoutBom = new byte[data.Length - 3];
                    Buffer.BlockCopy(data, 3, withoutBom, 0, withoutBom.Length);
                    data = withoutBom;
                }

                SendRawToPrinter(printerName, data, Path.GetFileName(filePath));
                Console.WriteLine("Label sent successfully.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }

        static string LoadPrinterName(string configPath)
        {
            if (!File.Exists(configPath))
            {
                Console.WriteLine("Config file not found. Using default Windows default printer.");
                // Returning null means: try system default later (you can implement that)
                return GetDefaultPrinterOrNull();
            }

            try
            {
                string json = File.ReadAllText(configPath, Encoding.UTF8);
                var config = JsonSerializer.Deserialize<PrinterConfig>(json);
                string name = config?.printerName;

                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("'printerName' missing in config. Using system default.");
                    return GetDefaultPrinterOrNull();
                }
                return name;
            }
            catch
            {
                Console.WriteLine("Failed to read config. Using system default printer.");
                return GetDefaultPrinterOrNull();
            }
        }

        static void SendRawToPrinter(string printerName, byte[] bytes, string jobName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
            {
                // Fall back to default printer discovery
                printerName = GetDefaultPrinterOrThrow();
            }

            if (!OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero))
            {
                int err = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Could not open printer '{printerName}'. Win32Error={err}");
            }

            try
            {
                var di = new DOC_INFO_1
                {
                    pDocName = string.IsNullOrWhiteSpace(jobName) ? "ZPL Job" : jobName,
                    pOutputFile = null,
                    pDatatype = "RAW"
                };

                if (!StartDocPrinter(hPrinter, 1, ref di))
                {
                    int err = Marshal.GetLastWin32Error();
                    throw new InvalidOperationException($"StartDocPrinter failed. Win32Error={err}");
                }

                try
                {
                    if (!StartPagePrinter(hPrinter))
                    {
                        int err = Marshal.GetLastWin32Error();
                        throw new InvalidOperationException($"StartPagePrinter failed. Win32Error={err}");
                    }

                    try
                    {
                        // Pin the bytes and write them
                        IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
                        try
                        {
                            Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);

                            if (!WritePrinter(hPrinter, unmanagedPointer, bytes.Length, out int written))
                            {
                                int err = Marshal.GetLastWin32Error();
                                throw new InvalidOperationException($"WritePrinter failed after {written}/{bytes.Length} bytes. Win32Error={err}");
                            }

                            if (written != bytes.Length)
                            {
                                throw new IOException($"Partial write: {written}/{bytes.Length} bytes.");
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(unmanagedPointer);
                        }
                    }
                    finally
                    {
                        EndPagePrinter(hPrinter);
                    }
                }
                finally
                {
                    EndDocPrinter(hPrinter);
                }
            }
            finally
            {
                ClosePrinter(hPrinter);
            }
        }

        // Tries to get default printer; returns null if unavailable
        [DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool GetDefaultPrinter(StringBuilder pszBuffer, ref int pcchBuffer);

        static string GetDefaultPrinterOrNull()
        {
            int size = 0;
            GetDefaultPrinter(null, ref size);
            if (size == 0) return null;

            var sb = new StringBuilder(size);
            if (GetDefaultPrinter(sb, ref size))
                return sb.ToString();

            return null;
        }

        static string GetDefaultPrinterOrThrow()
        {
            string name = GetDefaultPrinterOrNull();
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("No printer specified and no default printer is set.");
            return name;
        }

        class PrinterConfig
        {
            public string printerName { get; set; }
        }
    }
}
