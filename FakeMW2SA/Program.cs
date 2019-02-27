using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace FakeMW2SA
{
    class Program
    {
        public static Object thisLock = new Object();
        public static string myexternalip = new WebClient().DownloadString("http://icanhazip.com").Trim();
        public static List<PlayerModel> players = new List<PlayerModel>();
        public static int playerID = 0;
        public static bool geoip = false;
        public static int apicalls = 0;
        public static int partystate = 0;
        public static int memberjoin = 0;
        public static int csrf = new Random().Next();
        public static List<string> ipaddresses = new List<string>();
        public static int ipindex = 0;
        public static void addipaddress(string item)
        {

            lock (thisLock)
            {
                ipaddresses.Insert(0, item);
                if (ipaddresses.Count >= 100)
                {
                    ipaddresses.RemoveRange(50, 50);
                }
            }
            
        }
        static void Main(string[] args)
        {
            if (!HasAdministratorPrivileges())
            {
                Console.WriteLine("Please run as administrator. Press any key to close.");
                Console.ReadKey();
                return;
            }
            handler = new ConsoleEventDelegate(ConsoleEventCallback);
            SetConsoleCtrlHandler(handler, true);
            FakeMW2SA.HttpClient.Start();
            FakeMW2SA.Sniffer.Start();
            WriteOnBottomLine("0");

        }
        static bool ConsoleEventCallback(int eventType)
        {
            if (eventType == 2)
            {
                FakeMW2SA.Utils.Clearfirewall();
            }
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        private static bool HasAdministratorPrivileges()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        public static string playersinpacket = "0";
        //This function writes some statistics to the bottom of the cmd winndow
        public static void WriteOnBottomLine(string text)
        {
            if (text == "memberjoin") { memberjoin += 1; }
            else if (text == "partystate") { partystate += 1; }
            else if (text == "apicalls") { apicalls += 1; }
            else { playersinpacket = text + "  "; }

            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1;
            Console.Write("Partystate packets: {0} | Memberjoin packets: {1} | Api calls: {2} | Players in last partystate: {3}", partystate, memberjoin, apicalls, playersinpacket);
            // Restore previous position
            Console.SetCursorPosition(x, y);
        }

    }

}
