using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace MK3MoveBook
{
    // Offsets are verified against the two exact x86 executables, not ROM RAM.
    // See research/findings/2026-09-07-empty-cheat-catalog.md.
    internal sealed class MameCheatEngineProfile
    {
        public readonly long MachinePointerAddress;
        public readonly long FrameFunctionAddress;
        public readonly long MachineStoreAddress;

        private MameCheatEngineProfile(long machine, long frame, long store)
        {
            MachinePointerAddress = machine;
            FrameFunctionAddress = frame;
            MachineStoreAddress = store;
        }

        public static MameCheatEngineProfile ForHash(string sha256)
        {
            switch (sha256)
            {
                case "194fa64e618774f301deff80c2cb6ec67e3e1b45f4ecd335e02a66643c11e5f6":
                    return new MameCheatEngineProfile(0x21ed710, 0xa65fb0, 0x8c9376);
                case "0d0dd7eeb81f289e49c7aff6e7bc79bbed05726a13554224ee090a79c27fdcce":
                    return new MameCheatEngineProfile(0x21e83f0, 0xa61690, 0x8c5a9f);
                default:
                    return null;
            }
        }
    }

    internal sealed class MameCheatEngineSample
    {
        public string State = "unavailable";
        public string RomId = "";
        public uint MachineAddress;
        public uint EngineAddress;
        public uint HeadAddress;
        public ulong FrameCount;
        public bool EmptyVerified;
        public bool Valid { get { return State == "running"; } }
    }

    internal sealed class MameCheatEngineReader
    {
        private readonly MameCheatEngineProfile profile;
        private readonly Func<long, int, byte[]> read;
        private MameCheatEngineSample previous;
        private DateTime lastFrameAdvanceUtc;

        internal MameCheatEngineReader(
            MameCheatEngineProfile profile, Func<long, int, byte[]> read)
        {
            this.profile = profile;
            this.read = read;
        }

        public static MameCheatEngineReader TryCreate(int processId, IntPtr handle)
        {
            try
            {
                using (Process process = Process.GetProcessById(processId))
                using (SHA256 sha = SHA256.Create())
                {
                    ProcessModule module = process.MainModule;
                    // These PE files have stripped relocations. Never apply their VAs elsewhere.
                    if (module.BaseAddress.ToInt64() != 0x400000) return null;
                    string hash;
                    using (FileStream stream = File.OpenRead(module.FileName))
                        hash = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                    MameCheatEngineProfile selected = MameCheatEngineProfile.ForHash(hash);
                    if (selected == null) return null;
                    return new MameCheatEngineReader(selected, delegate(long address, int size)
                    {
                        byte[] bytes = new byte[size];
                        IntPtr received;
                        if (!ReadProcessMemory(handle, new IntPtr(address), bytes, size, out received) ||
                            received.ToInt64() != size) return null;
                        return bytes;
                    });
                }
            }
            catch { return null; }
        }

        public MameCheatEngineSample Read(DateTime utcNow)
        {
            try
            {
                MameCheatEngineSample sample = ReadCurrent();
                if (!sample.Valid)
                {
                    previous = null;
                    return sample;
                }
                bool same = previous != null && previous.Valid &&
                    previous.MachineAddress == sample.MachineAddress &&
                    previous.EngineAddress == sample.EngineAddress &&
                    previous.RomId == sample.RomId &&
                    previous.HeadAddress == sample.HeadAddress;
                bool advanced = same && sample.FrameCount > previous.FrameCount;
                bool monotonic = same && sample.FrameCount >= previous.FrameCount;
                if (advanced) lastFrameAdvanceUtc = utcNow;
                // Require two coherent observations and a live callback. An unreadable,
                // initializing, stale or paused object is never proof of an empty engine.
                sample.EmptyVerified = sample.HeadAddress == 0 && monotonic &&
                    (advanced || previous.EmptyVerified) &&
                    utcNow >= lastFrameAdvanceUtc &&
                    (utcNow - lastFrameAdvanceUtc).TotalMilliseconds <= 500;
                previous = sample;
                return sample;
            }
            catch
            {
                previous = null;
                return new MameCheatEngineSample();
            }
        }

        private MameCheatEngineSample ReadCurrent()
        {
            MameCheatEngineSample unavailable = new MameCheatEngineSample();
            if (profile == null || !CodeMatches()) return unavailable;
            uint machine = UInt32(profile.MachinePointerAddress);
            if (!Pointer(machine)) return unavailable;
            byte[] header = Bytes(machine, 0x100);
            uint basename = BitConverter.ToUInt32(header, 0x2c);
            uint core = BitConverter.ToUInt32(header, 0xd4);
            uint engine = BitConverter.ToUInt32(header, 0xfc);
            if (!Pointer(basename) || !Pointer(core) || !Pointer(engine)) return unavailable;
            byte[] rom = Bytes(basename, 16);
            int end = Array.IndexOf<byte>(rom, 0);
            if (end <= 0) return unavailable;
            string romId = Encoding.ASCII.GetString(rom, 0, end);
            if (romId != "umk3tm" && romId != "umk3tm60" &&
                romId != "umk3uk" && romId != "umk3uk2") return unavailable;
            // mame_private.current_phase = MAME_PHASE_RUNNING (3); reject pending reset/exit.
            byte[] phase = Bytes(core, 8);
            if (BitConverter.ToInt32(phase, 0) != 3 || phase[4] != 0 ||
                phase[5] != 0 || phase[6] != 0) return unavailable;
            byte[] data = Bytes(engine, 0x110);
            uint head = BitConverter.ToUInt32(data, 0);
            ulong frames = BitConverter.ToUInt64(data, 8);
            if ((head != 0 && !Pointer(head)) || frames == 0 ||
                data[0x10a] == 0 || data[0x10a] > 50) return unavailable;
            // With no entries there can be no script-generated output strings.
            if (head == 0)
                for (int offset = 0x10; offset < 0xd8; offset += 4)
                    if (BitConverter.ToUInt32(data, offset) != 0) return unavailable;
            // Re-read ownership after the data so a ROM switch cannot certify freed memory.
            if (UInt32(profile.MachinePointerAddress) != machine ||
                UInt32(machine + 0x2cL) != basename ||
                UInt32(machine + 0xd4L) != core || UInt32(machine + 0xfcL) != engine ||
                UInt32(engine) != head || UInt32(core) != 3) return unavailable;
            return new MameCheatEngineSample
            {
                State = "running", RomId = romId, MachineAddress = machine,
                EngineAddress = engine, HeadAddress = head, FrameCount = frames
            };
        }

        private bool CodeMatches()
        {
            byte[] expected = {
                0x55, 0x89, 0xe5, 0x56, 0x53, 0x83, 0xec, 0x10,
                0x8b, 0x45, 0x08, 0x8b, 0xb0, 0xfc, 0, 0, 0,
                0xc6, 0x86, 0x0b, 0x01, 0, 0, 0
            };
            byte[] actual = Bytes(profile.FrameFunctionAddress, expected.Length);
            for (int i = 0; i < expected.Length; i++)
                if (actual[i] != expected[i]) return false;
            byte[] store = Bytes(profile.MachineStoreAddress, 6);
            return store[0] == 0x89 && store[1] == 0x15 &&
                BitConverter.ToUInt32(store, 2) == profile.MachinePointerAddress;
        }

        private byte[] Bytes(long address, int size)
        {
            byte[] bytes = read(address, size);
            if (bytes == null || bytes.Length != size) throw new IOException("Incomplete engine read.");
            return bytes;
        }

        private uint UInt32(long address) { return BitConverter.ToUInt32(Bytes(address, 4), 0); }
        private static bool Pointer(uint value) { return value >= 0x10000 && (value & 3) == 0; }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr process, IntPtr address, byte[] buffer, int size, out IntPtr bytesRead);
    }
}
