using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;

namespace Wonderland_Private_Server.DataManagement.DataFiles
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct SceneInfo
    {
        
        public UInt16 SceneID;
        public byte SceneNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] SceneName;
        public string Name
        {
            get
            {
                return ASCIIEncoding.ASCII.GetString(SceneName).Replace("\0",string.Empty).Trim();
            }
        }
        public byte UnknownByte1;
        public byte UnknownByte2;
        public byte SoundMediaNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] SoundMediaName;
        public byte Control;
        //Binary => bit8=>Unknown, bit7=>Prohibit Tents, bit6=>Unknown
        //          bit5=>Prohibit Team, bit4=>Unknown bit3=>Can not go back to spawn points,
        //          bit2=>Prohibit Stalls, bit1=>Prohibit PK
        public byte UnknownByte3;
        public byte SceneEffects;
        //Value(not binary): 0=>None, 1=>Cherry, 2=>Snow, 3=>Smoke, 4=>Thunder
        //                   5=>Leaves, 6=>Bubbles, 7=>No Results
        public byte UnknownByte4;
        public byte UnknownByte5;
        public UInt16 SceneID1; // repeated (for unknown reasons)
        public UInt16 UnknownWord1;
        public UInt16 UnknownWord2;
        public UInt16 UnknownWord3;
        public UInt16 UnknownWord4;
        public UInt16 UnknownWord5;
        public UInt16 UnknownWord6;
        public UInt16 UnknownWord7;
        public UInt16 UnknownWord8;
        public UInt16 UnknownWord9;
        public byte UnknownByte6;
        public byte UnknownByte7;
        public UInt16 SceneID2; // repeated (for unknown reasons)
        public UInt16 UnknownWord10;
        public UInt16 UnknownWord11;
        public UInt16 UnknownWord12;
        public UInt16 UnknownWord13;
        public UInt16 UnknownWord14;
        public UInt16 UnknownWord15;
        public UInt16 UnknownWord16;
        public UInt16 UnknownWord17;
        public UInt16 UnknownWord18;
        public byte UnknownByte8;
        public byte UnknownByte9;
        public UInt16 UnknownWord19;
        public UInt16 UnknownWord20;
        public UInt16 UnknownWord21;
        public UInt16 UnknownWord22;
        public UInt16 UnknownWord23;
        public UInt16 UnknownWord24;
        public UInt16 UnknownWord25;
        public byte InstanceMaxPlayers;
        public byte InstanceGuildRestricted; // 0=>No, 1=>Yes
        public byte UnknownByte10;
        public byte UnknownByte11;
        public byte UnknownByte12;
        public UInt16 InstanceMarkID;
        public UInt16 InstanceLevelRequirement;
        public UInt16 InstanceTimeLimit;
        public UInt16 InstanceCoordinateX;
        public UInt16 InstanceCoordinateY;
        public UInt32 UnknownDword1;
        public UInt32 UnknownDword2;
        public UInt32 UnknownDword3;
        public UInt32 UnknownDword4;
        public UInt32 UnknownDword5;
    }

    public class SceneNotFoundException : Exception
    {
        public SceneNotFoundException()
        {
        }

        public SceneNotFoundException(string message)
            : base(message)
        {
        }

        public SceneNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class SceneDataFile
    {
        public static List<SceneInfo> m_List = new List<SceneInfo>();
        private static readonly object m_Lock = new object();

        public SceneDataFile()
        {
        }

        protected T ReadFromScenes<T>(Stream fs)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            fs.Read(buffer, 0, Marshal.SizeOf(typeof(T)));
            GCHandle Handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T RetVal = (T)Marshal.PtrToStructure(Handle.AddrOfPinnedObject(), typeof(T));

            return RetVal;
        }

        protected void DecodeScene32(ref UInt32 val)
        {
            val = Convert.ToUInt32((val ^ 0x062B7BA7) - 9);
        }

        protected void DecodeScene16(ref UInt16 val)
        {
            val = Convert.ToUInt16((val ^ 0xEA6C) - 9);
        }

        protected void DecodeScene8(ref byte val)
        {
            val = Convert.ToByte((val ^ 0x2C) - 9);
        }

        public bool LoadScenes(string file)
        {
           Utilities.LogServices.Log("loading Scence data.....");
            try
            {
                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    long FileLen = fs.Length;

                    if (m_List.Count > 0) { lock (m_Lock) { m_List.Clear(); } }

                    while (FileLen >= Marshal.SizeOf(typeof(SceneInfo)))
                    {
                        SceneInfo NewScene = ReadFromScenes<SceneInfo>(fs);

                        DecodeScene16(ref NewScene.SceneID);
                        DecodeScene8(ref NewScene.UnknownByte1);
                        DecodeScene8(ref NewScene.UnknownByte2);
                        DecodeScene8(ref NewScene.Control);
                        DecodeScene8(ref NewScene.UnknownByte3);
                        DecodeScene8(ref NewScene.SceneEffects);
                        DecodeScene8(ref NewScene.UnknownByte4);
                        DecodeScene8(ref NewScene.UnknownByte5);
                        DecodeScene16(ref NewScene.SceneID1);
                        DecodeScene16(ref NewScene.UnknownWord1);
                        DecodeScene16(ref NewScene.UnknownWord2);
                        DecodeScene16(ref NewScene.UnknownWord3);
                        DecodeScene16(ref NewScene.UnknownWord4);
                        DecodeScene16(ref NewScene.UnknownWord5);
                        DecodeScene16(ref NewScene.UnknownWord6);
                        DecodeScene16(ref NewScene.UnknownWord7);
                        DecodeScene16(ref NewScene.UnknownWord8);
                        DecodeScene16(ref NewScene.UnknownWord9);
                        DecodeScene8(ref NewScene.UnknownByte6);
                        DecodeScene8(ref NewScene.UnknownByte7);
                        DecodeScene16(ref NewScene.SceneID2);
                        DecodeScene16(ref NewScene.UnknownWord10);
                        DecodeScene16(ref NewScene.UnknownWord11);
                        DecodeScene16(ref NewScene.UnknownWord12);
                        DecodeScene16(ref NewScene.UnknownWord13);
                        DecodeScene16(ref NewScene.UnknownWord14);
                        DecodeScene16(ref NewScene.UnknownWord15);
                        DecodeScene16(ref NewScene.UnknownWord16);
                        DecodeScene16(ref NewScene.UnknownWord17);
                        DecodeScene16(ref NewScene.UnknownWord18);
                        DecodeScene8(ref NewScene.UnknownByte8);
                        DecodeScene8(ref NewScene.UnknownByte9);
                        DecodeScene16(ref NewScene.UnknownWord19);
                        DecodeScene16(ref NewScene.UnknownWord20);
                        DecodeScene16(ref NewScene.UnknownWord21);
                        DecodeScene16(ref NewScene.UnknownWord22);
                        DecodeScene16(ref NewScene.UnknownWord23);
                        DecodeScene16(ref NewScene.UnknownWord24);
                        DecodeScene16(ref NewScene.UnknownWord25);
                        DecodeScene8(ref NewScene.InstanceMaxPlayers);
                        DecodeScene8(ref NewScene.InstanceGuildRestricted);
                        DecodeScene8(ref NewScene.UnknownByte10);
                        DecodeScene8(ref NewScene.UnknownByte11);
                        DecodeScene8(ref NewScene.UnknownByte12);
                        DecodeScene16(ref NewScene.InstanceMarkID);
                        DecodeScene16(ref NewScene.InstanceLevelRequirement);
                        DecodeScene16(ref NewScene.InstanceTimeLimit);
                        DecodeScene16(ref NewScene.InstanceCoordinateX);
                        DecodeScene16(ref NewScene.InstanceCoordinateY);
                        DecodeScene32(ref NewScene.UnknownDword1);
                        DecodeScene32(ref NewScene.UnknownDword2);
                        DecodeScene32(ref NewScene.UnknownDword3);
                        DecodeScene32(ref NewScene.UnknownDword4);
                        DecodeScene32(ref NewScene.UnknownDword5);

                        for (int i = 0; i < 10; i++)
                        {
                            byte tmp = NewScene.SceneName[19 - i];
                            NewScene.SceneName[19 - i] = NewScene.SceneName[i];
                            NewScene.SceneName[i] = tmp;
                        }

                        lock (m_Lock)
                        {
                            m_List.Add(NewScene);
                        }

                        FileLen -= Marshal.SizeOf(typeof(SceneInfo));
                    }
                    fs.Close();
                    fs.Dispose();
                }
                Utilities.LogServices.Log("done");
                return true;
            }
            catch (Exception e)
            {
                Utilities.LogServices.Log("failed");
                Utilities.LogServices.Log(e);
                return false;
            }
        }

        public bool isMatch(object src)
        {
            foreach (SceneInfo r in m_List)
            {
                if (r.SceneID == 12000)
                {
                }
                Type myType = r.GetType();
                IList<FieldInfo> props = new List<FieldInfo>(myType.GetFields());

                foreach (FieldInfo prop in props)
                {
                    object propValue = prop.GetValue(r);

                    // Do something with propValue
                    if (propValue.ToString() == src.ToString())
                    {

                    }
                }
            }
            return false;
        }
        public SceneInfo GetSceneByID(UInt16 SceneID)
        {
            SceneInfo RetVal = new SceneInfo();
            bool bFound = false;
            try
            {
                lock (m_Lock)
                {
                    foreach (SceneInfo s in m_List)
                    {
                        if (s.SceneID == SceneID)
                        {
                            RetVal = s;
                            bFound = true;
                            break;
                        }
                    }
                }
            }
            catch { }

            if (!bFound) { Utilities.LogServices.Log(new Exception( "Scene => " + SceneID.ToString() + " could not be found.")); }

            return RetVal;
        }

        public SceneInfo GetSceneByName(string SceneName)
        {
            SceneInfo RetVal = new SceneInfo();
            bool bFound = false;

            lock (m_Lock)
            {
                foreach (SceneInfo s in m_List)
                {
                    if (string.Compare(Encoding.ASCII.GetString(s.SceneName), SceneName) == 0)
                    {
                        RetVal = s;
                        bFound = true;
                        break;
                    }
                }
            }
            if (!bFound) { Utilities.LogServices.Log(new Exception("Scene => " + SceneName + " could not be found.")); }

            return RetVal;
        }

        public void UnloadScenes() { lock (m_Lock) { m_List.Clear(); } }
    }
}
