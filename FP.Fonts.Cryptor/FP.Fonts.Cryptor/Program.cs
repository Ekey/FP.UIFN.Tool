using FP.Fonts.Cryptor.FileSystem.Package;

namespace FP.Fonts.Cryptor
{
    class Program
    {
        static void Main(String[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Forspoken Fonts Cryptor");
            Console.WriteLine("(c) 2026 Ekey (h4x0r) / v{0}\n", Utils.iGetApplicationVersion());
            Console.ResetColor();

            if (args.Length != 3)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[Usage]");
                Console.WriteLine("    FP.Fonts.Cryptor <m_Mode> <m_InFile> <m_OutFile>\n");
                Console.WriteLine("    m_Mode - Mode for work (See below for information)");
                Console.WriteLine("    m_File - Source file");
                Console.WriteLine("    m_OutFile - Destination file\n");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("[Modes]");
                Console.WriteLine("    <-d or -decrypt> - Decrypt file");
                Console.WriteLine("    <-e or -encrypt> - Encrypt file\n");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[Examples]");
                Console.WriteLine("    FP.Fonts.Cryptor -d D:\\dnp17forgame.uifn D:\\dnp17forgame.uifn.dec");
                Console.WriteLine("    FP.Fonts.Cryptor -e D:\\dnp17forgame.uifn.dec D:\\dnp17forgame.uifn");
                Console.ResetColor();
                return;
            }

            String m_Mode = args[0];
            String m_Input = args[1];
            String m_Output = args[2];

            switch (m_Mode)
            {
                case "-d":
                case "-decrypt": FontDecrypt.iDoIt(m_Input, m_Output); break;
                case "-e":
                case "-encrypt": FontEncrypt.iDoIt(m_Input, m_Output); break;
                default: Utils.iSetError("[ERROR]: Unknown mode is selected " + m_Mode); return;
            }
        }
    }
}
