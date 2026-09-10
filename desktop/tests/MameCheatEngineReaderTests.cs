using System;
using System.Collections.Generic;
using System.Text;

namespace MK3MoveBook
{
    internal static class MameCheatEngineReaderTests
    {
        private static int checks;
        private static readonly DateTime Now = new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc);
        private static readonly string[] Hashes = {
            "194fa64e618774f301deff80c2cb6ec67e3e1b45f4ecd335e02a66643c11e5f6",
            "0d0dd7eeb81f289e49c7aff6e7bc79bbed05726a13554224ee090a79c27fdcce"
        };
        public static int Main()
        {
            try
            {
                Check(MameCheatEngineProfile.ForHash("unknown") == null, "unknown build fails closed");
                foreach (string hash in Hashes)
                foreach (string rom in new[] { "umk3tm", "umk3tm60", "umk3uk", "umk3uk2" })
                {
                    Memory m = new Memory(hash, rom);
                    Check(!m.Reader.Read(Now).EmptyVerified, "first empty sample is insufficient");
                    m.Frame(8550);
                    MameCheatEngineSample sample = m.Reader.Read(Now.AddMilliseconds(250));
                    Check(sample.Valid && sample.EmptyVerified && sample.RomId == rom && sample.HeadAddress == 0,
                        "live empty list " + rom);
                    Check(m.Reader.Read(Now.AddMilliseconds(400)).EmptyVerified, "between callbacks");
                    Check(!m.Reader.Read(Now.AddMilliseconds(800)).EmptyVerified, "frozen engine expires");
                    m.Frame(8551);
                    Check(m.Reader.Read(Now.AddMilliseconds(900)).EmptyVerified, "callback resumes");
                    m.Frame(10);
                    Check(!m.Reader.Read(Now.AddMilliseconds(950)).EmptyVerified, "counter reset revokes proof");
                    m.U32(Memory.Engine, 0x50000);
                    m.Frame(11);
                    sample = m.Reader.Read(Now.AddMilliseconds(1000));
                    Check(sample.Valid && !sample.EmptyVerified && sample.HeadAddress == 0x50000, "nonempty is not clean proof");
                    m.U32(Memory.Engine, 0); m.Frame(12);
                    Check(!m.Reader.Read(Now.AddMilliseconds(1050)).EmptyVerified, "head change needs new proof");
                }
                foreach (Action<Memory> corrupt in new Action<Memory>[] {
                    m => m.Bytes[m.Profile.FrameFunctionAddress] = 0x90,
                    m => m.Bytes[m.Profile.MachineStoreAddress] = 0x90,
                    m => m.U32(m.Profile.MachinePointerAddress, 0),
                    m => m.U32(Memory.Machine + 0xfc, 0),
                    m => m.U32(Memory.Core, 1),
                    m => m.U32(Memory.Core, 4),
                    m => m.Bytes[Memory.Core + 4] = 1,
                    m => m.Bytes[Memory.Core + 5] = 1,
                    m => m.Bytes[Memory.Core + 6] = 1,
                    m => m.Bytes[Memory.Engine + 0x10a] = 0,
                    m => m.Bytes[Memory.Engine + 0x10a] = 51,
                    m => m.U32(Memory.Engine + 0x10, 0x50000),
                    m => m.U32(Memory.Engine + 0xd4, 0x50000),
                    m => m.Frame(0),
                    m => m.Rom("unrecognized"),
                    m => m.Bytes.Remove(Memory.Engine + 20)
                })
                {
                    Memory m = new Memory(Hashes[0], "umk3uk2");
                    m.Reader.Read(Now); m.Frame(8550);
                    Check(m.Reader.Read(Now.AddMilliseconds(250)).EmptyVerified, "establish proof");
                    corrupt(m);
                    Check(!m.Reader.Read(Now.AddMilliseconds(300)).Valid, "bad memory revokes proof");
                }
                Memory switched = new Memory(Hashes[0], "umk3tm60");
                switched.Reader.Read(Now); switched.Frame(8550); switched.Reader.Read(Now.AddMilliseconds(250));
                switched.Rom("umk3uk2"); switched.Frame(8551);
                Check(!switched.Reader.Read(Now.AddMilliseconds(300)).EmptyVerified, "ROM identity change revokes proof");
                switched.Frame(8552);
                Check(switched.Reader.Read(Now.AddMilliseconds(350)).EmptyVerified, "new ROM observed twice");
                Memory torn = new Memory(Hashes[0], "umk3uk2");
                int reads = 0;
                var reader = new MameCheatEngineReader(torn.Profile, delegate(long address, int size) {
                    if (address == torn.Profile.MachinePointerAddress && ++reads == 2) return new byte[4];
                    return torn.Read(address, size);
                });
                Check(!reader.Read(Now).Valid, "ownership changed during read");
                Console.WriteLine("MameCheatEngineReader: " + checks + " checks passed.");
                return 0;
            }
            catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        }
        private static void Check(bool ok, string label) { checks++; if (!ok) throw new Exception(label); }
        private sealed class Memory
        {
            public const long Machine = 0x10000, Core = 0x20000, Engine = 0x30000, Name = 0x40000;
            public readonly Dictionary<long, byte> Bytes = new Dictionary<long, byte>();
            public readonly MameCheatEngineProfile Profile;
            public readonly MameCheatEngineReader Reader;
            public Memory(string hash, string rom)
            {
                Profile = MameCheatEngineProfile.ForHash(hash);
                Put(Profile.FrameFunctionAddress, new byte[] { 0x55,0x89,0xe5,0x56,0x53,0x83,0xec,0x10,0x8b,0x45,8,0x8b,0xb0,0xfc,0,0,0,0xc6,0x86,0xb,1,0,0,0 });
                Put(Profile.MachineStoreAddress, new byte[] {0x89,0x15,0,0,0,0});
                U32(Profile.MachineStoreAddress + 2, (uint)Profile.MachinePointerAddress);
                U32(Profile.MachinePointerAddress, (uint)Machine);
                Put(Machine, new byte[0x100]); Put(Core, new byte[8]); Put(Engine, new byte[0x110]);
                U32(Machine + 0x2c, (uint)Name); U32(Machine + 0xd4, (uint)Core); U32(Machine + 0xfc, (uint)Engine);
                U32(Core, 3); Bytes[Engine + 0x10a] = 25; Frame(8534); Rom(rom);
                Reader = new MameCheatEngineReader(Profile, Read);
            }
            public void U32(long address, uint value) { Put(address, BitConverter.GetBytes(value)); }
            public void Frame(ulong value) { Put(Engine + 8, BitConverter.GetBytes(value)); }
            public void Rom(string rom) { Put(Name, new byte[16]); Put(Name, Encoding.ASCII.GetBytes(rom)); }
            private void Put(long address, byte[] bytes) { for(int i=0;i<bytes.Length;i++) Bytes[address+i]=bytes[i]; }
            public byte[] Read(long address, int size)
            {
                byte[] bytes = new byte[size];
                for(int i=0;i<size;i++) if (!Bytes.TryGetValue(address+i,out bytes[i])) return null;
                return bytes;
            }
        }
    }
}
