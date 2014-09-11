using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Wonderland_Private_Server.DataManagement.DataFiles
{
    public class cCompound2Dat
    {
        public List<cBuildElement> buildList;

        System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
        public bool Loaded = false;
        public cCompound2Dat()
        {
            buildList = new List<cBuildElement>();
        }
        public void ConsoleLoad(string path)
        {
            Console.WriteLine("Loading Compound Data..."); timer.Restart();

            buildList.Clear();
            if (!File.Exists(path)) return;
            byte[] data = File.ReadAllBytes(path);
            int max = data.Length / 65;
            Console.Write(string.Format("Loaded 0 out of {0}",max));
            for (int n = 0; n < max; n++)
            {
                cBuildElement e = new cBuildElement();
                e.Load(data, n);
                buildList.Add(e);
                Console.SetCursorPosition(7, Console.CursorTop);
                Console.Write(string.Format("Loaded {0} out of {1}",n+1,max));
            }
            timer.Stop();
            Console.WriteLine("Operaation took -> " + timer.Elapsed.ToString());
            if (Loaded)
                Console.WriteLine("Loading Done");
            else
                Console.WriteLine("Loading Failed");
        }
        public bool Load(string path)
        {
            buildList.Clear();
            if (!File.Exists(path)) return false;
            byte[] data = File.ReadAllBytes(path);
            int max = data.Length / 65;
            for (int n = 0; n < max; n++)
            {
                cBuildElement e = new cBuildElement();
                e.Load(data, n);
                buildList.Add(e);
            }
            timer.Stop();
            return Loaded;
        }

        public UInt16 GetBuildCode(UInt16 resultID)
        {
            UInt16 ret = 0;
            foreach (cBuildElement b in buildList)
            {
                if (b.resultID == resultID) return ret;
                ret++;
            }
            return 0;
        }

    }

    public class cBuildElement
    {
        public UInt16 buildCode;

        public UInt16 resultID;
        public UInt16 planID;
        public byte unknownByte;
        public UInt16 toolID;
        public byte ammtRecv;
        public byte unknownByte0;
        public byte unknownByte1;
        public byte unknownByte2;
        public UInt16 materialID1;
        public byte materialAmmt1;
        public UInt16 materialID2;
        public byte materialAmmt2;
        public UInt16 materialID3;
        public byte materialAmmt3;
        public UInt16 materialID4;
        public byte materialAmmt4;
        public UInt16 materialID5;
        public byte materialAmmt5;
        public UInt16 buildTime;
        public byte unknownByte3;
        public byte unknownByte4;
        public byte unknownByte5;
        public byte unknownByte6;
        public byte unknownByte7;
        public byte unknownByte8;
        public byte unknownByte9;
        public UInt16 unknownWord0;
        public UInt16 unknownWord1;
        public UInt16 unknownWord2;
        public UInt16 unknownWord3;
        public UInt16 unknownWord4;
        public UInt32 unknownDWord0;
        public UInt32 unknownDWord1;
        public UInt32 unknownDWord2;
        public UInt32 unknownDWord3;
        public UInt32 unknownDWord4;

        public void Load(byte[] data, int index)
        {
            int ptr = index * 65;
            resultID = wordXor(getWord(data, ptr)); ptr += 2;
            if (resultID == 41142)
            {
            }
            planID = wordXor(getWord(data, ptr)); ptr += 2;
            unknownByte = byteXor(data[ptr]); ptr++;
            toolID = wordXor(getWord(data, ptr)); ptr += 2;
            ammtRecv = byteXor(data[ptr]); ptr++;
            unknownByte0 = data[ptr]; ptr++;
            unknownByte1 = data[ptr]; ptr++;
            unknownByte2 = data[ptr]; ptr++;
            materialID1 = wordXor(getWord(data, ptr)); ptr += 2;
            materialAmmt1 = byteXor(data[ptr]); ptr++;
            materialID2 = wordXor(getWord(data, ptr)); ptr += 2;
            materialAmmt2 = byteXor(data[ptr]); ptr++;
            materialID3 = wordXor(getWord(data, ptr)); ptr += 2;
            materialAmmt3 = byteXor(data[ptr]); ptr++;
            materialID4 = wordXor(getWord(data, ptr)); ptr += 2;
            materialAmmt4 = byteXor(data[ptr]); ptr++;
            materialID5 = wordXor(getWord(data, ptr)); ptr += 2;
            materialAmmt5 = byteXor(data[ptr]); ptr++;
            unknownByte3 = byteXor(data[ptr]); ptr++;
            buildTime = wordXor(getWord(data, ptr)); ptr += 2;
            unknownByte4 = byteXor(data[ptr]); ptr++;
            unknownByte5 = byteXor(data[ptr]); ptr++;
            unknownByte6 = byteXor(data[ptr]); ptr++;
            unknownByte7 = byteXor(data[ptr]); ptr++;
            unknownByte8 = byteXor(data[ptr]); ptr++;
            unknownByte9 = byteXor(data[ptr]); ptr++;
            unknownWord0 = wordXor(getWord(data, ptr)); ptr += 2;
            unknownWord1 = wordXor(getWord(data, ptr)); ptr += 2;
            unknownWord2 = wordXor(getWord(data, ptr)); ptr += 2;
            unknownWord3 = wordXor(getWord(data, ptr)); ptr += 2;
            unknownWord4 = wordXor(getWord(data, ptr)); ptr += 2;
            unknownDWord0 = dwordXor(getDWord(data, ptr)); ptr += 4;
            unknownDWord1 = dwordXor(getDWord(data, ptr)); ptr += 4;
            unknownDWord2 = dwordXor(getDWord(data, ptr)); ptr += 4;
            unknownDWord3 = dwordXor(getDWord(data, ptr)); ptr += 4;
            unknownDWord4 = dwordXor(getDWord(data, ptr)); ptr += 4;

        }
        private UInt16 getWord(byte[] data, int ptr)
        {
            return (UInt16)((data[ptr + 1] << 8) + data[ptr]);
        }
        private UInt32 getDWord(byte[] data, int ptr)
        {
            return (UInt32)((data[ptr + 3] << 24) + (data[ptr + 2] << 16) + (data[ptr + 1] << 8) + data[ptr]);
        }
        private byte byteXor(byte v) { return (byte)((v ^ 0xD3) - 3); }
        private UInt16 wordXor(UInt16 v) { return (UInt16)((v ^ 0xFBBC) - 3); }
        private UInt32 dwordXor(UInt32 v) { return (UInt32)((v ^ 0x0A06F965) - 3); }
        public cBuildElement Copy()
        {
            cBuildElement e = new cBuildElement();
            e.buildCode = buildCode;
            e.resultID = resultID;
            e.planID = planID;
            e.unknownByte = unknownByte;
            e.toolID = toolID;
            e.ammtRecv = ammtRecv;
            e.unknownByte0 = unknownByte0;
            e.unknownByte1 = unknownByte1;
            e.unknownByte2 = unknownByte2;
            e.materialID1 = materialID1;
            e.materialAmmt1 = materialAmmt1;
            e.materialID2 = materialID2;
            e.materialAmmt2 = materialAmmt2;
            e.materialID3 = materialID3;
            e.materialAmmt3 = materialAmmt3;
            e.materialID4 = materialID4;
            e.materialAmmt4 = materialAmmt4;
            e.materialID5 = materialID5;
            e.materialAmmt5 = materialAmmt5;
            e.buildTime = buildTime;
            e.unknownByte3 = unknownByte3;
            e.unknownByte4 = unknownByte4;
            e.unknownByte5 = unknownByte5;
            e.unknownByte6 = unknownByte6;
            e.unknownByte7 = unknownByte7;
            e.unknownByte8 = unknownByte8;
            e.unknownByte9 = unknownByte9;
            e.unknownWord0 = unknownWord0;
            e.unknownWord1 = unknownWord1;
            e.unknownWord2 = unknownWord2;
            e.unknownWord3 = unknownWord3;
            e.unknownWord4 = unknownWord4;
            e.unknownDWord0 = unknownDWord0;
            e.unknownDWord1 = unknownDWord1;
            e.unknownDWord2 = unknownDWord2;
            e.unknownDWord3 = unknownDWord3;
            e.unknownDWord4 = unknownDWord4;
            return e;
        }
    }

}
