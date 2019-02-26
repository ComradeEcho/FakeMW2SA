using PacketDotNet;
using SharpPcap;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace FakeMW2SA
{
    class Sniffer
    {
        public static List<string> localipaddresses = new List<string>();


        private const int ReadTimeoutMilliseconds = 5000;
        public static void Run()
        {
            FakeMW2SA.Utils.runCommand("route delete * 12.34.56.78");
            var devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                throw new Exception("No interfaces found! Make sure WinPcap is installed.");
            }
            foreach (SharpPcap.WinPcap.WinPcapDevice dev in devices)
            {
                Console.Out.WriteLine("{0}", dev.Description);

                foreach (SharpPcap.LibPcap.PcapAddress addr in dev.Addresses)
                {
                    if (addr.Addr != null && addr.Addr.ipAddress != null)
                    {
                        localipaddresses.Add(addr.Addr.ipAddress.ToString());
                    }
                }
            }
            foreach (var device in devices)
            {
                if (device == null) continue;
                device.OnPacketArrival += DeviceOnOnPacketArrival;
                device.Open(DeviceMode.Promiscuous, ReadTimeoutMilliseconds);
                // tcpdump filter to capture only game packets
                device.Filter = "udp and port 28960";
                Action action = device.Capture;
                action.BeginInvoke(ar => action.EndInvoke(ar), null);



            }
        }
        public static void DeviceOnOnPacketArrival(object sender, CaptureEventArgs captureEventArgs)
        {
            int numberofplayers = 0;
            var packet = Packet.ParsePacket(captureEventArgs.Packet.LinkLayerType, captureEventArgs.Packet.Data);
            var eth = (EthernetPacket)packet;
            var hex = BitConverter.ToString(eth.BytesHighPerformance.Bytes).Replace("-", string.Empty);
            var DestIP = new IPAddress(long.Parse(FakeMW2SA.Utils.ReverseBytes(hex.Substring(60, 8)), System.Globalization.NumberStyles.AllowHexSpecifier)).ToString();
            var SourceIP = new IPAddress(long.Parse(FakeMW2SA.Utils.ReverseBytes(hex.Substring(52, 8)), System.Globalization.NumberStyles.AllowHexSpecifier)).ToString();
            if (!localipaddresses.Contains(SourceIP)){ FakeMW2SA.Program.addipaddress(SourceIP); }
            //memberjoin
            if (hex.Contains(@"6D656D62"))
            {
                FakeMW2SA.Program.WriteOnBottomLine("memberjoin");
                string namehex;
                Match match = Regex.Match(hex, @"(?:[0-9a-fA-F][0-9a-fA-F])+?0{48}.{16}([0-9a-fA-F]+?)0000");
                while (match.Success)
                {
                    if (match.Groups[1].Value.Length % 2 != 0)
                    {
                        namehex = match.Groups[1].Value + "0";
                    }
                    else
                    {
                        namehex = match.Groups[1].Value;
                    }
                    byte[] dBytes = FakeMW2SA.Utils.StringToByteArray(namehex);
                    string ASCIIresult = System.Text.Encoding.ASCII.GetString(dBytes);
                    string utf8result = System.Text.Encoding.UTF8.GetString(dBytes);
                    match = match.NextMatch();
                    PlayerModel player;
                    player = new PlayerModel(SourceIP, "0", true) { playerprimaryid = FakeMW2SA.Program.playerID, personaname = utf8result};
                    if ((FakeMW2SA.Program.players.Find(x => x.ip == SourceIP) == null))
                    {
                    FakeMW2SA.Program.playerID++;
                    player.partyID = FakeMW2SA.Utils.findPartyID();
                    FakeMW2SA.Program.players.Add(player);
                    } 
                }
            }
            //partystate
            if (hex.Contains(@"70617274797374617465"))
            {
                FakeMW2SA.Program.WriteOnBottomLine("partystate");
                FakeMW2SA.Utils.sethost(SourceIP);
                string playerpatern = @"0{10}.{40}0{48}.{28}";
                MatchCollection matches2;
                Regex playerregex = new Regex(playerpatern);
                matches2 = playerregex.Matches(hex);
                var IDlist = new List<string>();
                for (int ctr = 0; ctr < matches2.Count; ctr++)
                {
                    IDlist.Add(long.Parse(FakeMW2SA.Utils.ReverseBytes(matches2[ctr].Value.Substring(10, 16)), System.Globalization.NumberStyles.HexNumber).ToString());
                }
                for (int ctr = 0; ctr < matches2.Count; ctr++)
                {
                    var partystatesteamid = long.Parse(FakeMW2SA.Utils.ReverseBytes(matches2[ctr].Value.Substring(10, 16)), System.Globalization.NumberStyles.HexNumber).ToString();
                    var partystateip = new IPAddress(long.Parse(FakeMW2SA.Utils.ReverseBytes(matches2[ctr].Value.Substring(34, 8)), System.Globalization.NumberStyles.AllowHexSpecifier)).ToString();


                    PlayerModel player;
                    if ((FakeMW2SA.Program.players.Find(x => x.steamid == partystatesteamid) == null))
                    {
                        player = new PlayerModel(partystateip, partystatesteamid, false) { playerprimaryid = FakeMW2SA.Program.playerID };
                        FakeMW2SA.Program.playerID++;
                        FakeMW2SA.Program.players.Add(player);
                    }
                    else
                    {
                        player = FakeMW2SA.Program.players.Find(x => x.steamid == partystatesteamid);
                    }
                    if ((FakeMW2SA.Program.players.Find(x => x.unknown1 == int.Parse(matches2[ctr].Value.Substring(98, 8), System.Globalization.NumberStyles.HexNumber)) == null))
                    {
                        player.partyID = FakeMW2SA.Utils.findPartyID();
                    }
                    else
                    {
                        if (int.Parse(matches2[ctr].Value.Substring(98, 8), System.Globalization.NumberStyles.HexNumber) == 0)
                        {
                            player.partyID = FakeMW2SA.Utils.findPartyID();
                        }
                        else
                        {
                            player.partyID = (FakeMW2SA.Program.players.Find(x => x.unknown1 == int.Parse(matches2[ctr].Value.Substring(98, 8), System.Globalization.NumberStyles.HexNumber))).partyID;
                        }
                    }


                    TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                    int secondsSinceEpoch = (int)t.TotalSeconds;
                    player.updated = false;
                    player.lastupdated = Utils.GetEpochSeconds();



                    if (int.Parse(matches2[ctr].Value.Substring(120, 2), System.Globalization.NumberStyles.HexNumber) + 1 <= 70 &&
                        int.Parse(matches2[ctr].Value.Substring(122, 2), System.Globalization.NumberStyles.HexNumber) + 1 <= 11 &&
                        int.Parse(matches2[ctr].Value.Substring(118, 2), System.Globalization.NumberStyles.HexNumber) < 50 &&
                        int.Parse((matches2[ctr].Value.Substring(116, 2) + matches2[ctr].Value.Substring(114, 2)), System.Globalization.NumberStyles.HexNumber) < 10000 &&
                        int.Parse((matches2[ctr].Value.Substring(116, 2) + matches2[ctr].Value.Substring(114, 2)), System.Globalization.NumberStyles.HexNumber) % 10 == 0)
                    {
                        numberofplayers += 1;
                        player.level = int.Parse(matches2[ctr].Value.Substring(120, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                        player.presteige = int.Parse(matches2[ctr].Value.Substring(122, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                        player.deaths = int.Parse(matches2[ctr].Value.Substring(118, 2), System.Globalization.NumberStyles.HexNumber);
                        player.score = int.Parse((matches2[ctr].Value.Substring(116, 2) + matches2[ctr].Value.Substring(114, 2)), System.Globalization.NumberStyles.HexNumber);
                        player.missing = 1;
                    }
                    else
                    {
                        numberofplayers += 1;
                        player.level = int.Parse(matches2[ctr].Value.Substring(122, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                        player.presteige = int.Parse(matches2[ctr].Value.Substring(124, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                        player.deaths = int.Parse(matches2[ctr].Value.Substring(120, 2), System.Globalization.NumberStyles.HexNumber);
                        player.score = int.Parse((matches2[ctr].Value.Substring(118, 2) + matches2[ctr].Value.Substring(116, 2)), System.Globalization.NumberStyles.HexNumber);
                        player.missing = 0;
                    }
                    player.unknown1 = int.Parse(matches2[ctr].Value.Substring(98, 8), System.Globalization.NumberStyles.HexNumber);
                    player.unknown2 = int.Parse(FakeMW2SA.Utils.ReverseBytes(matches2[ctr].Value.Substring(106, 8)), System.Globalization.NumberStyles.HexNumber);
                }
                FakeMW2SA.Program.WriteOnBottomLine(numberofplayers.ToString());
                FakeMW2SA.Utils.callapis();
            }


        }
        public static void Start()
        {
            Thread a = new Thread(Run);
            a.Start();
        }

    }
}
