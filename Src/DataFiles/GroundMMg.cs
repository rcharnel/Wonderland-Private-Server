using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Wonderland_Private_Server.DataManagement.DataFiles
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct GroundNodeInfo
    {
        public string Name
        {
            get
            {
                return ASCIIEncoding.ASCII.GetString(SceneName).Replace("\0", string.Empty).Trim();
            }
        }
        public byte SceneNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public byte[] SceneName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        
        public byte[] UnknownBytes;
        public UInt32 Offset;
        public UInt32 SceneLength;
    }

    public struct SceneLayout
    {
        public UInt32 MaxX_32;
        public UInt32 MaxY_32;
        public UInt16 MaxX_16;
        public UInt16 MaxY_16;
        public List<byte[]> Scene;
    }

    public class NodeNotFoundException : Exception
    {
        public NodeNotFoundException()
        {
        }

        public NodeNotFoundException(string message)
            : base(message)
        {
        }

        public NodeNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public class GroundMMGDataFile
    {
        private List<GroundNodeInfo> m_NodeList;
        private readonly object m_Lock;

        public GroundMMGDataFile()
        {
            m_NodeList = new List<GroundNodeInfo>();
            m_Lock = new object();
        }

        protected T ReadNode<T>(Stream fs)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            fs.Read(buffer, 0, Marshal.SizeOf(typeof(T)));

            GCHandle Handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T RetVal = (T)Marshal.PtrToStructure(Handle.AddrOfPinnedObject(), typeof(T));
            Handle.Free();

            return RetVal;
        }

        public bool LoadGroundNodes(string loc)
        {
            try
            {
                using (Stream fs = new FileStream(loc, FileMode.Open, FileAccess.Read))
                {
                    long FileLen = fs.Length;
                    fs.Position = FileLen - 2;

                    byte[] Tmp = new byte[2];
                    fs.Read(Tmp, 0, 2);

                    UInt16 Entries = BitConverter.ToUInt16(Tmp, 0);

                    UInt32 Len = (UInt32)(Entries * Marshal.SizeOf(typeof(GroundNodeInfo)));

                    fs.Position = (FileLen - 2) - Len;

                    UInt32 Read = 0;
                    while (Read < Len)
                    {
                        GroundNodeInfo NewNode = ReadNode<GroundNodeInfo>(fs);

                        lock (m_Lock)
                        {
                            m_NodeList.Add(NewNode);
                        }

                        Read += (UInt32)(Marshal.SizeOf(typeof(GroundNodeInfo)));
                    }

                    fs.Close();
                    fs.Dispose();
                }
                return true;
            }
            catch (Exception ex) { return false; }
        }

        public List<GroundNodeInfo> GetNodeList()
        {
            List<GroundNodeInfo> RetVal = new List<GroundNodeInfo>();

            lock (m_Lock)
            {
                RetVal = m_NodeList;
            }

            return RetVal;
        }

        public GroundNodeInfo GetNodeBySceneName(string SceneName)
        {
            GroundNodeInfo RetVal = new GroundNodeInfo();
            bool bFound = false;

            lock (m_Lock)
            {
                foreach (GroundNodeInfo g in m_NodeList)
                {
                    if (string.Compare(g.Name, (SceneName+".map")) == 0)
                    {
                        RetVal = g;
                        bFound = true;
                        break;
                    }
                }
            }

            if (!bFound) throw (new NodeNotFoundException("Node => \"" + SceneName + "\" could not be found."));

            return RetVal;
        }

        public SceneLayout GetSceneLayout(GroundNodeInfo GroundNode)
        {
            SceneLayout RetVal = new SceneLayout();

            try
            {
                using (Stream fs = new FileStream(@"C:\\Program Files\\Wonderland Online\\data\\Ground.MMG", FileMode.Open, FileAccess.Read))
                {
                    fs.Position = GroundNode.Offset;

                    byte[] Tmp = new byte[8];
                    fs.Read(Tmp, 0, 8);

                    RetVal.MaxX_32 = BitConverter.ToUInt32(Tmp, 0);
                    RetVal.MaxY_32 = BitConverter.ToUInt32(Tmp, 4);

                    fs.Read(Tmp, 0, 1);
                    fs.Position = GroundNode.Offset + 8 + (Tmp[0] * 6) + 1;

                    fs.Read(Tmp, 0, 4);
                    RetVal.MaxX_16 = BitConverter.ToUInt16(Tmp, 0);
                    RetVal.MaxY_16 = BitConverter.ToUInt16(Tmp, 2);

                    RetVal.Scene = new List<byte[]>();
                    while (RetVal.Scene.Count < RetVal.MaxX_16)
                    {
                        byte[] yTmp = new byte[RetVal.MaxY_16 + 1];
                        fs.Read(yTmp, 0, RetVal.MaxY_16);
                        RetVal.Scene.Add(yTmp);
                    }

                    fs.Close();
                    fs.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return RetVal;
        }

        public bool IsInWalkingBounds(int x, int y, SceneLayout Layout)
        {
            return (Layout.Scene[x / 20][y / 20] == 0 ||
                Layout.Scene[x / 20][y / 20] == 8);
        }

        public void UnloadGroundNodes() { lock (m_Lock) { m_NodeList.Clear(); } }
    }
}
