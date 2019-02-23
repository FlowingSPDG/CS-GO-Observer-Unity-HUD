using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.IO.Pipes;

// TODO: Fix CreateEvent handle leakage.

namespace advancedfx
{

    /// <summary>
    /// Do not edit this class, it is likely to get updated, you will loose your changes often.
    /// 
    /// This class manages the inter-opreation between with HLAE / AfxHookSource.<br />
    ///  
    /// It is a bit more complicated, because we must not eliminate the advantage of CS:GO's queued rendering
    /// (our main thread syncs with the render thread of the game and we take info from the game's engine thread on a dedicated thread.).
    /// </summary>
    public class Interop
    {

        //
        // Public:

        public enum D3DFORMAT : UInt32
        {
            D3DFMT_UNKNOWN = 0,

            D3DFMT_R8G8B8 = 20,
            D3DFMT_A8R8G8B8 = 21,
            D3DFMT_X8R8G8B8 = 22,
            D3DFMT_R5G6B5 = 23,
            D3DFMT_X1R5G5B5 = 24,
            D3DFMT_A1R5G5B5 = 25,
            D3DFMT_A4R4G4B4 = 26,
            D3DFMT_R3G3B2 = 27,
            D3DFMT_A8 = 28,
            D3DFMT_A8R3G3B2 = 29,
            D3DFMT_X4R4G4B4 = 30,
            D3DFMT_A2B10G10R10 = 31,
            D3DFMT_A8B8G8R8 = 32,
            D3DFMT_X8B8G8R8 = 33,
            D3DFMT_G16R16 = 34,
            D3DFMT_A2R10G10B10 = 35,
            D3DFMT_A16B16G16R16 = 36,

            D3DFMT_A8P8 = 40,
            D3DFMT_P8 = 41,

            D3DFMT_L8 = 50,
            D3DFMT_A8L8 = 51,
            D3DFMT_A4L4 = 52,

            D3DFMT_V8U8 = 60,
            D3DFMT_L6V5U5 = 61,
            D3DFMT_X8L8V8U8 = 62,
            D3DFMT_Q8W8V8U8 = 63,
            D3DFMT_V16U16 = 64,
            D3DFMT_A2W10V10U10 = 67,

            //D3DFMT_UYVY                 = MAKEFOURCC('U', 'Y', 'V', 'Y'),
            //D3DFMT_R8G8_B8G8            = MAKEFOURCC('R', 'G', 'B', 'G'),
            //D3DFMT_YUY2                 = MAKEFOURCC('Y', 'U', 'Y', '2'),
            //DFMT_G8R8_G8B8            = MAKEFOURCC('G', 'R', 'G', 'B'),
            //D3DFMT_DXT1                 = MAKEFOURCC('D', 'X', 'T', '1'),
            //D3DFMT_DXT2                 = MAKEFOURCC('D', 'X', 'T', '2'),
            //D3DFMT_DXT3                 = MAKEFOURCC('D', 'X', 'T', '3'),
            //D3DFMT_DXT4                 = MAKEFOURCC('D', 'X', 'T', '4'),
            //D3DFMT_DXT5                 = MAKEFOURCC('D', 'X', 'T', '5'),

            D3DFMT_D16_LOCKABLE = 70,
            D3DFMT_D32 = 71,
            D3DFMT_D15S1 = 73,
            D3DFMT_D24S8 = 75,
            D3DFMT_D24X8 = 77,
            D3DFMT_D24X4S4 = 79,
            D3DFMT_D16 = 80,

            D3DFMT_D32F_LOCKABLE = 82,
            D3DFMT_D24FS8 = 83,

            //#if !defined(D3D_DISABLE_9EX)
            D3DFMT_D32_LOCKABLE = 84,
            D3DFMT_S8_LOCKABLE = 85,
            //#endif // !D3D_DISABLE_9EX

            D3DFMT_L16 = 81,

            D3DFMT_VERTEXDATA = 100,
            D3DFMT_INDEX16 = 101,
            D3DFMT_INDEX32 = 102,

            D3DFMT_Q16W16V16U16 = 110,

            //D3DFMT_MULTI2_ARGB8         = MAKEFOURCC('M','E','T','1'),

            D3DFMT_R16F = 111,
            D3DFMT_G16R16F = 112,
            D3DFMT_A16B16G16R16F = 113,

            D3DFMT_R32F = 114,
            D3DFMT_G32R32F = 115,
            D3DFMT_A32B32G32R32F = 116,

            D3DFMT_CxV8U8 = 117,

            //#if !defined(D3D_DISABLE_9EX)
            D3DFMT_A1 = 118,
            D3DFMT_A2B10G10R10_XR_BIAS = 119,
            D3DFMT_BINARYBUFFER = 199,
            //#endif // !D3D_DISABLE_9EX

            D3DFMT_INTZ = 1515474505,

            //D3DFMT_FORCE_DWORD          =0x7fffffff
        };

        public enum D3DPOOL : UInt32
        {
            D3DPOOL_DEFAULT = 0,
            D3DPOOL_MANAGED = 1,
            D3DPOOL_SYSTEMMEM = 2,
            D3DPOOL_SCRATCH = 3,

            //D3DPOOL_FORCE_DWORD = 0x7fffffff
        }

        public enum D3DMULTISAMPLE_TYPE
        {
            D3DMULTISAMPLE_NONE = 0,
            D3DMULTISAMPLE_NONMASKABLE = 1,
            D3DMULTISAMPLE_2_SAMPLES = 2,
            D3DMULTISAMPLE_3_SAMPLES = 3,
            D3DMULTISAMPLE_4_SAMPLES = 4,
            D3DMULTISAMPLE_5_SAMPLES = 5,
            D3DMULTISAMPLE_6_SAMPLES = 6,
            D3DMULTISAMPLE_7_SAMPLES = 7,
            D3DMULTISAMPLE_8_SAMPLES = 8,
            D3DMULTISAMPLE_9_SAMPLES = 9,
            D3DMULTISAMPLE_10_SAMPLES = 10,
            D3DMULTISAMPLE_11_SAMPLES = 11,
            D3DMULTISAMPLE_12_SAMPLES = 12,
            D3DMULTISAMPLE_13_SAMPLES = 13,
            D3DMULTISAMPLE_14_SAMPLES = 14,
            D3DMULTISAMPLE_15_SAMPLES = 15,
            D3DMULTISAMPLE_16_SAMPLES = 16,
            //D3DMULTISAMPLE_FORCE_DWORD = 0xffffffff
        }

        [Flags]
        public enum D3DUSAGE : UInt32
        {
            D3DUSAGE_RENDERTARGET = 0x00000001,
            D3DUSAGE_DEPTHSTENCIL = 0x00000002,
            D3DUSAGE_DYNAMIC = 0x00000200
        }

        public struct Afx4x4
        {
            public Single M00;
            public Single M01;
            public Single M02;
            public Single M03;
            public Single M10;
            public Single M11;
            public Single M12;
            public Single M13;
            public Single M20;
            public Single M21;
            public Single M22;
            public Single M23;
            public Single M30;
            public Single M31;
            public Single M32;
            public Single M33;
        }

        public interface IFrameInfo
        {
            Int32 FrameCount { get; }

            /// <summary>
            /// Unpaused client frame time.
            /// </summary>
            Single AbsoluteFrameTime { get; }

            /// <summary>
            /// Client game time.
            /// </summary>
            Single CurTime { get; }

            /// <summary>
            /// Client frame time (can be 0 e.g. if paused).
            /// </summary>
            Single FrameTime { get; }

            Int32 Width { get; }
            Int32 Height { get; }
            Afx4x4 WorldToViewMatrix { get; }
            Afx4x4 WorldToScreenMatrix { get; }
        }


        private class FrameInfo : IFrameInfo
        {
            Int32 IFrameInfo.FrameCount { get { return m_FrameCount; } }
            Single IFrameInfo.AbsoluteFrameTime { get { return m_AbsoluteFrameTime; } }
            Single IFrameInfo.CurTime { get { return m_CurTime; } }
            Single IFrameInfo.FrameTime { get { return m_FrameTime; } }
            Int32 IFrameInfo.Width { get { return m_Width; } }
            Int32 IFrameInfo.Height { get { return m_Height; } }
            Afx4x4 IFrameInfo.WorldToViewMatrix { get { return m_WorldToViewMatrix; } }
            Afx4x4 IFrameInfo.WorldToScreenMatrix { get { return m_WorldToScreenMatrix; } }

            public Int32 FrameCount { get { return m_FrameCount; } set { m_FrameCount = value; } }
            public Single AbsoluteFrameTime { get { return m_AbsoluteFrameTime; } set { m_AbsoluteFrameTime = value; } }
            public Single CurTime { get { return m_CurTime; } set { m_CurTime = value; } }
            public Single FrameTime { get { return m_FrameTime; } set { m_FrameTime = value; } }
            public Int32 Width { get { return m_Width; } set { m_Width = value; } }
            public Int32 Height { get { return m_Height; } set { m_Height = value; } }
            public Afx4x4 WorldToViewMatrix { get { return m_WorldToViewMatrix; } set { m_WorldToViewMatrix = value; } }
            public Afx4x4 WorldToScreenMatrix { get { return m_WorldToScreenMatrix; } set { m_WorldToScreenMatrix = value; } }

            Int32 m_FrameCount;
            Single m_AbsoluteFrameTime;
            Single m_CurTime;
            Single m_FrameTime;
            Int32 m_Width;
            Int32 m_Height;
            Afx4x4 m_WorldToViewMatrix;
            Afx4x4 m_WorldToScreenMatrix;
        }

        public enum RenderType : int
        {
            Unknown = 0,
            Sky = 1,
            Normal = 2,
            Shadow = 3
        }

        public interface ITextureInfo
        {
            UInt32 TextureID { get; }
            String TextureGroup { get; }
            String TextureName { get; }
            UInt32 D3D9Width { get; }
            UInt32 D3D9Height { get; }
            UInt32 D3D9Levels { get; }
            D3DUSAGE D3D9Usage { get; }
            D3DFORMAT D3D9Format { get; }
            D3DPOOL D3D9Pool { get; }
            IntPtr SharedHandle { get; }
        }

        private class TextureInfo : ITextureInfo
        {
            UInt32 ITextureInfo.TextureID { get { return m_TextureID; } }
            String ITextureInfo.TextureGroup { get { return m_TextureGroup; } }
            String ITextureInfo.TextureName { get { return m_TextureName; } }
            UInt32 ITextureInfo.D3D9Width { get { return m_D3D9Width; } }
            UInt32 ITextureInfo.D3D9Height { get { return m_D3D9Height; } }
            UInt32 ITextureInfo.D3D9Levels { get { return m_D3D9Levels; } }
            D3DUSAGE ITextureInfo.D3D9Usage { get { return m_D3D9Usage; } }
            D3DFORMAT ITextureInfo.D3D9Format { get { return m_D3D9Format; } }
            D3DPOOL ITextureInfo.D3D9Pool { get { return m_D3D9Pool; } }
            IntPtr ITextureInfo.SharedHandle { get { return m_SharedHandle; } }

            public UInt32 TextureID { get { return m_TextureID; } set { m_TextureID = value; } }
            public String TextureGroup { get { return m_TextureGroup; } set { m_TextureGroup = value; } }
            public String TextureName { get { return m_TextureName; } set { m_TextureName = value; } }
            public UInt32 D3D9Width { get { return m_D3D9Width; } set { m_D3D9Width = value; } }
            public UInt32 D3D9Height { get { return m_D3D9Height; } set { m_D3D9Height = value; } }
            public UInt32 D3D9Levels { get { return m_D3D9Levels; } set { m_D3D9Levels = value; } }
            public D3DUSAGE D3D9Usage { get { return m_D3D9Usage; } set { m_D3D9Usage = value; } }
            public D3DFORMAT D3D9Format { get { return m_D3D9Format; } set { m_D3D9Format = value; } }
            public D3DPOOL D3D9Pool { get { return m_D3D9Pool; } set { m_D3D9Pool = value; } }
            public IntPtr SharedHandle { get { return m_SharedHandle; } set { m_SharedHandle = value; } }

            private UInt32 m_TextureID;
            private String m_TextureGroup;
            private String m_TextureName;
            private UInt32 m_D3D9Width;
            private UInt32 m_D3D9Height;
            private UInt32 m_D3D9Levels;
            private D3DUSAGE m_D3D9Usage;
            private D3DFORMAT m_D3D9Format;
            private D3DPOOL m_D3D9Pool;
            private IntPtr m_SharedHandle;
        }

        public interface ISurfaceInfo
        {
            IntPtr SharedHandle { get; }
            UInt32 Width { get; }
            UInt32 Height { get; }
            D3DUSAGE Usage { get; }
            D3DFORMAT Format { get; }
            D3DPOOL Pool { get; }
            D3DMULTISAMPLE_TYPE MultiSampleType { get; }
            UInt32 MultiSampleQuality { get; }
        }

        private class SurfaceInfo : ISurfaceInfo
        {
            IntPtr ISurfaceInfo.SharedHandle { get { return m_SharedHandle; } }
            UInt32 ISurfaceInfo.Width { get { return m_Width; } }
            UInt32 ISurfaceInfo.Height { get { return m_Height; } }
            D3DUSAGE ISurfaceInfo.Usage { get { return m_Usage; } }
            D3DFORMAT ISurfaceInfo.Format { get { return m_Format; } }
            D3DPOOL ISurfaceInfo.Pool { get { return m_Pool; } }
            D3DMULTISAMPLE_TYPE ISurfaceInfo.MultiSampleType { get { return m_MultiSampleType;  } }
            UInt32 ISurfaceInfo.MultiSampleQuality { get { return m_MultiSampleQuality; } }

            public IntPtr SharedHandle { get { return m_SharedHandle; } set { m_SharedHandle = value; } }
            public UInt32 Width { get { return m_Width; } set { m_Width = value; } }
            public UInt32 Height { get { return m_Height; } set { m_Height = value; } }
            public D3DUSAGE Usage { get { return m_Usage; } set { m_Usage = value; } }
            public D3DFORMAT Format { get { return m_Format; } set { m_Format = value; } }
            public D3DPOOL Pool { get { return m_Pool; } set { m_Pool = value; } }
            public D3DMULTISAMPLE_TYPE MultiSampleType { get { return m_MultiSampleType; } set { m_MultiSampleType = value; } }
            public UInt32 MultiSampleQuality { get { return m_MultiSampleQuality; } set { m_MultiSampleQuality = value; } }

            private IntPtr m_SharedHandle;
            private UInt32 m_Width;
            private UInt32 m_Height;
            private D3DUSAGE m_Usage;
            private D3DFORMAT m_Format;
            private D3DPOOL m_Pool;
            private D3DMULTISAMPLE_TYPE m_MultiSampleType;
            private UInt32 m_MultiSampleQuality;
        }

        public interface IRenderInfo
        {
            RenderType Type { get; }

            /// <remarks>Can be null if not available, so handle this.</remarks>
            Nullable<IntPtr> FbSurfaceHandle { get; }

            /// <remarks>Can be null if not available, so handle this.</remarks>
            Nullable<IntPtr> FbDepthSurfaceHandle { get; }

            /// <remarks>Can be null if not available, so handle this! Assume this to happen especially at start-up!</remarks>
            IFrameInfo FrameInfo { get; }
        }

        private class RenderInfo : IRenderInfo
        {
            RenderType IRenderInfo.Type { get { return m_Type; } }
            Nullable<IntPtr> IRenderInfo.FbSurfaceHandle { get { return m_FbSurfaceHandle; } }
            Nullable<IntPtr> IRenderInfo.FbDepthSurfaceHandle { get { return m_FbDepthSurfaceHandle; } }
            IFrameInfo IRenderInfo.FrameInfo { get { return m_FrameInfo; } }

            public RenderType Type { get { return m_Type; } set { m_Type = value; } }
            public Nullable<IntPtr> FbSurfaceHandle { get { return m_FbSurfaceHandle; } set { m_FbSurfaceHandle = value; } }
            public Nullable<IntPtr> FbDepthSurfaceHandle { get { return m_FbDepthSurfaceHandle; } set { m_FbDepthSurfaceHandle = value; } }
            public IFrameInfo FrameInfo { get { return m_FrameInfo; } set { m_FrameInfo = value; } }

            private RenderType m_Type;
            private Nullable<IntPtr> m_FbSurfaceHandle;
            private Nullable<IntPtr> m_FbDepthSurfaceHandle;
            private IFrameInfo m_FrameInfo;
        }

        public interface ICommand
        {
            String this[int index] { get; }
            int Count { get;  }
        }

        public class Command : ICommand
        {
            String ICommand.this[int index] { get { return m_Args[index]; } }
            int ICommand.Count { get { return m_Args.Count; } }

            public void AddArg(String arg)
            {
                m_Args.Add(arg);
            }

            private List<String> m_Args = new List<String>();
        }

        public interface ICommandArray
        {
            ICommand this[int index] { get; }
            int Count { get; }
        }

        public class CommandArray : ICommandArray
        {
            ICommand ICommandArray.this[int index] { get { return m_Cmds[index]; } }
            int ICommandArray.Count { get { return m_Cmds.Count; } }

            public void AddCmd(Command cmd)
            {
                m_Cmds.Add(cmd);
            }

            private List<Command> m_Cmds = new List<Command>();
        }

        public interface ILogging
        {
            /// <remarks>Must be threadsafe!</remarks>
            void Log(object message);

            /// <remarks>Must be threadsafe!</remarks>
            void LogException(Exception exception);
        }

        public interface IImplementation : ILogging
        {
            /// <summary>
            /// Connection has been lost, reset state, free shared textures etc.
            /// </summary>
            void ConnectionLost();

            /// <summary>
            /// Rendering has to be done.
            /// </summary>
            /// <param name="renderInfo">Info about the rendering requested.</param>
            void Render(IRenderInfo renderInfo);

            /// <summary>
            /// Offers a shared surface for usage.
            /// </summary>
            /// <param name="info">Info about the surface offerred.</param>
            void RegisterSurface(ISurfaceInfo info);

            /// <summary>
            /// A surface surface must be released (if in use)!
            /// </summary>
            /// <param name="surfaceHandle">The handle of the surface.</param>
            void ReleaseSurface(IntPtr surfaceHandle);

            /// <summary>
            /// Offers a shared texture for usage.
            /// </summary>
            /// <param name="info">Info about the texture offerred.</param>
            /// <remarks>This is currently not used!</remarks>
            void RegisterTexture(ITextureInfo info);

            /// <summary>
            /// A shared texture must be released (if in use)!
            /// </summary>
            /// <param name="textureId">The ID of the texture.</param>
            /// <remarks>This is currently not used!</remarks>
            void ReleaseTexture(UInt32 textureId);

            /// <summary>
            /// Warning, you are reponsible for the thread syncronization required!<br />
            /// Command batch from the client engine thread.
            /// These are sent and received before the current network frame is renderred.
            /// </summary>
            /// <param name="commands">
            ///  Commands sent from client. Commands that have "afx" as first argument are reserved for internal will not be passed through to you.
            ///  It will be empty when the client sent no commands.
            ///  </param>
            ///  <returns>
            ///  null for no reply or command strings to reply (will be executed in client console).
            ///  </returns>
            IList<String> EngineThreadCommands(ICommandArray commands);
        }

        public Interop(IImplementation implementation)
        {
            this.implementation = implementation;
        }

        public void OnEnable()
        {

            PipeConnect();
        }

        public void OnDisable()
        {

            PipeDisconnect();
        }

        public void Update()
        {
            Interlocked.Exchange(ref watchDogMs, 0);

            if (null != pipeServer && pipeServer.Connect())
            {
                try
                {
                    if (waitingForConnection)
                    {
                        // Is new connection.

                        implementation.Log("Pipe connected.");

                        //IntPtr ownProcessHandle = System.Diagnostics.Process.GetCurrentProcess ().Handle;

                        //long clientProcessId = 0;

                        //if(!GetNamedPipeClientProcessId(pipeServer.SafePipeHandle.DangerousGetHandle(), out clientProcessId))
                        //	throw new ApplicationException("Could not get process id of client.");

                        //System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById((int)clientProcessId);
                        //if(null == process)
                        //	throw new ApplicationException("Could not get client process from id.");

                        //IntPtr clientProcessHandle = process.Handle;

                        // Check if our version is supported by client:

                        implementation.Log("Writing version.");

                        pipeServer.WriteInt32(version, cancellationToken);

                        implementation.Log("Flushing version.");

                        pipeServer.Flush(cancellationToken);

                        implementation.Log("Waiting for version reply.");

                        bool versionSupported = pipeServer.ReadBoolean(cancellationToken);

                        if (!versionSupported)
                            throw new ApplicationException("Version " + version + " not supported by client.");

                        // Supply server info required by client:

                        pipeServer.WriteBoolean(Environment.Is64BitProcess, cancellationToken);

                        pipeServer.Flush(cancellationToken);

                        //

                        waitingForConnection = false;
                    }

                    bool done = false;

                    while (!done)
                    {
                        DrawingMessage drawingMessage = (DrawingMessage)pipeServer.ReadInt32(cancellationToken);

                        switch (drawingMessage)
                        {
                            case DrawingMessage.DrawingThreadBeforeHud:
                                {
                                    RenderInfo renderInfo = new RenderInfo();

                                    renderInfo.Type = RenderType.Normal;

                                    IntPtr fbSurfaceHandle = pipeServer.ReadHandle(cancellationToken);
                                    renderInfo.FbSurfaceHandle = IntPtr.Zero != fbSurfaceHandle ? new Nullable<IntPtr>(fbSurfaceHandle) : null;

                                    IntPtr fbDepthSurfaceHandle = pipeServer.ReadHandle(cancellationToken);
                                    renderInfo.FbDepthSurfaceHandle = IntPtr.Zero != fbDepthSurfaceHandle ? new Nullable<IntPtr>(fbDepthSurfaceHandle) : null;

                                    IFrameInfo frameInfo = null;

                                    Int32 frameCount = pipeServer.ReadInt32(cancellationToken);

                                    Boolean frameInfoSent = pipeServer.ReadBoolean(cancellationToken);

                                    if (frameInfoSent)
                                    {
                                        // Frameinfo will become available soon, wait for it.

                                        do
                                        {
                                            while (!frameInfoQueueEnqueued.WaitOne(1000))
                                            {
                                                if (!engineThread.IsAlive)
                                                    throw new ApplicationException("Engine message handling thread died.");

                                                lock (frameInfoQueue)
                                                {
                                                    if (0 < frameInfoQueue.Count)
                                                        break;
                                                }
                                            }

                                            lock (frameInfoQueue)
                                            {
                                                while (0 < frameInfoQueue.Count)
                                                {
                                                    IFrameInfo curFrameInfo = frameInfoQueue.Peek();

                                                    Int32 cmp = frameCount - curFrameInfo.FrameCount;

                                                    if (cmp > 0)
                                                    {
                                                        // This is an old info, skip
                                                        frameInfoQueue.Dequeue();
                                                    }
                                                    else if (cmp < 0)
                                                    {
                                                        // Too far ahead, missing info, abort
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        // Exactly right info, let's go!
                                                        frameInfo = curFrameInfo;
                                                        frameInfoQueue.Dequeue();
                                                        break;
                                                    }
                                                }
                                            }
                                        } while (null == frameInfo);
                                    }

                                    renderInfo.FrameInfo = frameInfo;

                                    try
                                    {
                                        implementation.Render(renderInfo);
                                    }
                                    catch(Exception e)
                                    {
                                        implementation.LogException(e);
                                    }

                                    // Signal done (this is important so we can sync the drawing!):
                                    pipeServer.WriteBoolean(true, cancellationToken);
                                    pipeServer.Flush(cancellationToken);
                                    
                                    done = true; // We are done for this frame.
                                }
                                break;

                            case DrawingMessage.NewTexture:
                                {
                                    TextureInfo textureInfo = new TextureInfo();

                                    // TextureId:
                                    textureInfo.TextureID = (UInt32)pipeServer.ReadInt32(cancellationToken);
                                    textureInfo.TextureGroup = pipeServer.ReadUTF8String(cancellationToken);
                                    textureInfo.TextureName = pipeServer.ReadUTF8String(cancellationToken);
                                    textureInfo.D3D9Width = (UInt32)pipeServer.ReadInt32(cancellationToken);
                                    textureInfo.D3D9Height = (UInt32)pipeServer.ReadInt32(cancellationToken);
                                    textureInfo.D3D9Levels = (UInt32)pipeServer.ReadInt32(cancellationToken);
                                    textureInfo.D3D9Usage = (D3DUSAGE)(UInt32)pipeServer.ReadInt32(cancellationToken);
                                    textureInfo.D3D9Format = (D3DFORMAT)(UInt32)pipeServer.ReadInt32(cancellationToken);
                                    textureInfo.D3D9Pool = (D3DPOOL)(UInt32)pipeServer.ReadInt32(cancellationToken);
                                    textureInfo.SharedHandle = pipeServer.ReadHandle(cancellationToken);

                                    implementation.RegisterTexture(textureInfo);
                                }
                                break;

                            case DrawingMessage.ReleaseTexture:
                                {
                                    UInt32 textureId = (UInt32)pipeServer.ReadInt32(cancellationToken);
                                    implementation.ReleaseTexture(textureId);

                                    // Confirm the release to the client waiting:

                                    pipeServer.WriteBoolean(true, cancellationToken);
                                    pipeServer.Flush(cancellationToken);
                                }
                                break;

                            case DrawingMessage.NewSurface:
                                {
                                    SurfaceInfo surfaceInfo = new SurfaceInfo();

                                    // TextureId:
                                    surfaceInfo.SharedHandle = pipeServer.ReadHandle(cancellationToken);
                                    surfaceInfo.Width = (UInt32)pipeServer.ReadInt32(cancellationToken);
                                    surfaceInfo.Height = (UInt32)pipeServer.ReadInt32(cancellationToken);
                                    surfaceInfo.Usage = (D3DUSAGE)(UInt32)pipeServer.ReadInt32(cancellationToken);
                                    surfaceInfo.Format = (D3DFORMAT)(UInt32)pipeServer.ReadInt32(cancellationToken);
                                    surfaceInfo.Pool = (D3DPOOL)(UInt32)pipeServer.ReadInt32(cancellationToken);
                                    surfaceInfo.MultiSampleType = (D3DMULTISAMPLE_TYPE)(UInt32)pipeServer.ReadInt32(cancellationToken);
                                    surfaceInfo.MultiSampleQuality = (UInt32)pipeServer.ReadInt32(cancellationToken);

                                    implementation.RegisterSurface(surfaceInfo);
                                }
                                break;

                            case DrawingMessage.ReleaseSurface:
                                {
                                    IntPtr surfaceHandle = pipeServer.ReadHandle(cancellationToken);
                                    implementation.ReleaseSurface(surfaceHandle);

                                    // Confirm the release to the client waiting:

                                    pipeServer.WriteBoolean(true, cancellationToken);
                                    pipeServer.Flush(cancellationToken);
                                }
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    implementation.LogException(e);
                    PipeDisconnect();
                    PipeConnect();
                }
            }
        }

        public Int32 Version
        {
            get
            {
                return version;
            }
        }

        public string PipeName
        {
            get
            {
                return pipeName;
            }
            set
            {
                pipeName = value;
            }
        }

        //
        // Private:

        private enum DrawingMessage : int
        {
            DrawingThreadBeforeHud = 1,
            NewTexture = 2,
            ReleaseTexture = 3,
            NewSurface = 4,
            ReleaseSurface = 5
        };

        private IImplementation implementation;

        private const Int32 version = 1;
        private string pipeName = "advancedfxInterop";

        bool waitingForConnection = false;

        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        private int watchDogMs = 0;
        private const int watchDogCancelAfterMs = 20000;
        private Thread watchDogThread = null;

        private void WatchDog()
        {
            while (true)
            {
                int oldTicks = System.Environment.TickCount;

                Thread.Sleep(1000);

                if (Interlocked.Add(ref watchDogMs, System.Environment.TickCount - oldTicks) >= watchDogCancelAfterMs)
                    break;
            }

            cancellationTokenSource.Cancel();
        }

        private AutoResetEvent frameInfoQueueEnqueued = new AutoResetEvent(false);
        private Queue<IFrameInfo> frameInfoQueue = new Queue<IFrameInfo>();

        private void AddFrameInfo(IFrameInfo frameInfo)
        {
            lock (frameInfoQueue)
            {
                frameInfoQueue.Enqueue(frameInfo);
            }
            frameInfoQueueEnqueued.Set();
        }

        private MyNamedPipeServer pipeServer = null;
        private EngineThread engineThread = null;

        private void PipeConnect()
        {

            PipeDisconnect();

            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                cancellationToken = cancellationTokenSource.Token;

                Interlocked.Exchange(ref watchDogMs, 0);
                watchDogThread = new Thread(WatchDog);
                watchDogThread.Start();

                engineThread = new EngineThread(this);
                engineThread.Init();

                pipeServer = new MyNamedPipeServer(pipeName, this.implementation);

                waitingForConnection = true;

                implementation.Log("Waiting for connection.");
            }
            catch (Exception e)
            {
                implementation.LogException(e);
                PipeDisconnect();
            }
        }

        private void PipeDisconnect()
        {
            implementation.ConnectionLost();

            Interlocked.Exchange(ref watchDogMs, watchDogCancelAfterMs);

            try
            {
                if (null != watchDogThread && watchDogThread.IsAlive)
                {
                    watchDogThread.Join();
                }
            }
            finally
            {
                watchDogThread = null;
            }

            try
            {
                if (null != engineThread)
                {
                    engineThread.Dispose();
                }
            }
            finally
            {
                engineThread = null;
            }

            try
            {
                if (null != pipeServer)
                {
                    pipeServer.Dispose();
                }
            }
            catch (Exception e)
            {
                implementation.LogException(e);
            }
            finally
            {
                pipeServer = null;
            }

            waitingForConnection = false;

            implementation.Log("Pipes closed.");
        }

        private class EngineThread : IDisposable
        {

            public EngineThread(Interop interOp)
            {
                this.interOp = interOp;
                this.cancellationToken = interOp.cancellationToken;
            }

            public void Init()
            {
                pipeServer = new MyNamedPipeServer(interOp.pipeName + "_engine", interOp.implementation);

                thread = new Thread(ThreadWorker);
                thread.Start();
            }

            public bool IsAlive
            {
                get
                {
                    return null != thread ? thread.IsAlive : false;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            bool disposed = false;

            protected virtual void Dispose(bool disposing)
            {

                if (disposed || !disposing) return;

                try
                {
                    if (null != thread && thread.IsAlive)
                    {
                        thread.Join();
                    }
                }
                catch (Exception e)
                {
                    interOp.implementation.LogException(e);
                }
                finally
                {
                    thread = null;
                }

                disposed = true;
            }

            private enum EngineMessage : int
            {
                LevelInitPreEntity = 1,
                LevelShutDown = 2,
                BeforeFrameStart = 3,
                BeforeHud = 4,
                AfterFrameRenderEnd = 5,
                EntityCreated = 6,
                EntityDeleted = 7
            };

            private enum BeforFrameRenderStartServerMessage : int
            {
                EOT = 0,
                RequestEntityList = 1
            };

            private CancellationToken cancellationToken;
            private Interop interOp;
            private MyNamedPipeServer pipeServer = null;
            private Thread thread = null;

             private void ThreadWorker()
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested && !pipeServer.Connect())
                    {
                        Thread.Sleep(10);
                    }

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        EngineMessage engineMessage = (EngineMessage)pipeServer.ReadInt32(cancellationToken);

                        switch (engineMessage)
                        {
                            case EngineMessage.BeforeFrameStart:
                                {
                                    // Read incoming commands from client:

                                    CommandArray commands = new CommandArray();

                                    UInt32 commandCount = pipeServer.ReadCompressedUInt32(cancellationToken);

                                    while (0 < commandCount)
                                    {
                                        Command command = new Command();

                                        UInt32 argCount = pipeServer.ReadCompressedUInt32(cancellationToken);

                                        while (0 < argCount)
                                        {
                                            command.AddArg(pipeServer.ReadUTF8String(cancellationToken));

                                            --argCount;
                                        }

                                        commands.AddCmd(command);

                                        --commandCount;
                                    }

                                    IList<String> reply = interOp.implementation.EngineThreadCommands(commands);
                                    if(null == reply)
                                    {
                                        pipeServer.WriteCompressedUInt32(0, cancellationToken);
                                    }
                                    else
                                    {
                                        pipeServer.WriteCompressedUInt32((UInt32)reply.Count, cancellationToken);
                                        for(int i=0; i < reply.Count; ++i)
                                        {
                                            pipeServer.WriteStringUTF8(reply[i], cancellationToken);
                                        }
                                    }

                                    pipeServer.Flush(cancellationToken);
                                }
                                break;
                            case EngineMessage.BeforeHud:
                                {
                                    FrameInfo frameInfo = new FrameInfo();

                                    frameInfo.FrameCount = pipeServer.ReadInt32(cancellationToken);
                                    frameInfo.AbsoluteFrameTime = pipeServer.ReadSingle(cancellationToken);
                                    frameInfo.CurTime = pipeServer.ReadSingle(cancellationToken);
                                    frameInfo.FrameTime = pipeServer.ReadSingle(cancellationToken);

                                    frameInfo.Width = pipeServer.ReadInt32(cancellationToken);
                                    frameInfo.Height = pipeServer.ReadInt32(cancellationToken);

                                    Afx4x4 worldToViewMatrix = new Afx4x4();

                                    worldToViewMatrix.M00 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M01 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M02 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M03 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M10 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M11 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M12 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M13 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M20 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M21 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M22 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M23 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M30 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M31 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M32 = pipeServer.ReadSingle(cancellationToken);
                                    worldToViewMatrix.M33 = pipeServer.ReadSingle(cancellationToken);

                                    frameInfo.WorldToViewMatrix = worldToViewMatrix;

                                    Afx4x4 worldToScreenMatrix = new Afx4x4();

                                    worldToScreenMatrix.M00 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M01 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M02 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M03 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M10 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M11 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M12 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M13 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M20 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M21 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M22 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M23 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M30 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M31 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M32 = pipeServer.ReadSingle(cancellationToken);
                                    worldToScreenMatrix.M33 = pipeServer.ReadSingle(cancellationToken);

                                    frameInfo.WorldToScreenMatrix = worldToScreenMatrix;

                                    this.interOp.AddFrameInfo(frameInfo);
                                }
                                break;
                            case EngineMessage.EntityCreated:
                                {
                                    Int32 entityHandle = pipeServer.ReadInt32(cancellationToken);
                                }
                                break;
                            case EngineMessage.EntityDeleted:
                                {
                                    Int32 entityHandle = pipeServer.ReadInt32(cancellationToken);
                                }
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    interOp.implementation.LogException(e);
                }
                finally
                {
                    pipeServer.Dispose();
                }
            }
        }

        //[DllImport("kernel32.dll", SetLastError = true)]
        //static extern bool GetNamedPipeClientProcessId(IntPtr Pipe, out long ClientProcessId);

        //[Flags]
        //public enum DuplicateOptions : uint
        //{
        //	DUPLICATE_CLOSE_SOURCE = (0x00000001),// Closes the source handle. This occurs regardless of any error status returned.
        //	DUPLICATE_SAME_ACCESS = (0x00000002), //Ignores the dwDesiredAccess parameter. The duplicate handle has the same access as the source handle.
        //}

        //[DllImport("kernel32.dll", SetLastError=true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
        //	IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle,
        //	uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

        // In case you wonder: we use two pipes instead of a duplex pipe in order to be able to read and write at same time from different processes without deadlocking.
        private class MyNamedPipeServer : IDisposable
        {
            enum State
            {
                Waiting,
                Connected
            }

            private ILogging logging;

            private IntPtr pipeHandle;

            private OVERLAPPED overlappedRead;
            private OVERLAPPED overlappedWrite;
            private GCHandle gcOverlappedRead;
            private GCHandle gcOverlappedWrite;

            private State state = State.Waiting;

            private IntPtr readBuffer;
            private IntPtr writeBuffer;

            private const int readBufferSize = 512;
            private const int writeBufferSize = 512;

            public MyNamedPipeServer(string pipeName, ILogging logging)
            {
                this.logging = logging;

                readBuffer = Marshal.AllocHGlobal(readBufferSize);
                writeBuffer = Marshal.AllocHGlobal(writeBufferSize);

                overlappedRead = new OVERLAPPED();
                gcOverlappedRead = GCHandle.Alloc(overlappedRead, GCHandleType.Pinned);
                overlappedRead.hEvent = CreateEvent(IntPtr.Zero, true, true, null);

                overlappedWrite = new OVERLAPPED();
                gcOverlappedWrite = GCHandle.Alloc(overlappedWrite, GCHandleType.Pinned);
                overlappedWrite.hEvent = CreateEvent(IntPtr.Zero, true, true, null);

                pipeHandle = CreateNamedPipe(
                    "\\\\.\\pipe\\" + pipeName,
                    (uint)(PipeOpenModeFlags.PIPE_ACCESS_INBOUND | PipeOpenModeFlags.PIPE_ACCESS_OUTBOUND | PipeOpenModeFlags.FILE_FLAG_OVERLAPPED),
                    (uint)(PipeModeFlags.PIPE_READMODE_BYTE | PipeModeFlags.PIPE_TYPE_BYTE | PipeModeFlags.PIPE_WAIT | PipeModeFlags.PIPE_REJECT_REMOTE_CLIENTS),
                    1,
                    (uint)writeBufferSize,
                    (uint)readBufferSize,
                    5000,
                    IntPtr.Zero);

                if (INVALID_HANDLE_VALUE != overlappedRead.hEvent
                    && INVALID_HANDLE_VALUE != pipeHandle
                    && false == ConnectNamedPipe(pipeHandle, ref overlappedRead))
                {
                    switch ((uint)Marshal.GetLastWin32Error())
                    {
                        case ERROR_IO_PENDING:
                            state = State.Waiting;
                            break;
                        case ERROR_PIPE_CONNECTED:
                            state = State.Connected;
                            SetEvent(overlappedRead.hEvent);
                            break;
                        default:
                            Dispose();
                            throw new System.ApplicationException("MyNamedPipeServer: Error: " + Marshal.GetLastWin32Error());
                    }
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            bool disposed = false;

            protected virtual void Dispose(bool disposing)
            {
                if (disposed || !disposing) return;

                try
                {
                    if (INVALID_HANDLE_VALUE != pipeHandle) CloseHandle(pipeHandle);
                    if (INVALID_HANDLE_VALUE != overlappedWrite.hEvent) CloseHandle(overlappedWrite.hEvent);
                    if (INVALID_HANDLE_VALUE != overlappedRead.hEvent) CloseHandle(overlappedRead.hEvent);
                }
                finally {
                    gcOverlappedWrite.Free();
                    gcOverlappedRead.Free();
                    Marshal.FreeHGlobal(writeBuffer);
                    Marshal.FreeHGlobal(readBuffer);
                }

                disposed = true;
            }

            public bool Connect()
            {
                if (State.Waiting == state)
                {
                    uint waitResult = WaitForSingleObject(overlappedRead.hEvent, 0);

                    if (WAIT_OBJECT_0 == waitResult)
                    {
                        uint cb;

                        if (!GetOverlappedResult(pipeHandle, ref overlappedRead, out cb, false))
                        {
                            throw new System.ApplicationException("Connect: GetOverlappedResult error: " + Marshal.GetLastWin32Error());
                        }

                        state = State.Connected;
                    }
                }

                return State.Connected == state;
            }

            public void ReadBytes(byte[] bytes, int offset, int length, CancellationToken cancellationToken)
            {
                while (true)
                {
                    uint bytesRead = 0;

                    if (!ReadFile(pipeHandle, readBuffer, (uint)Math.Min(readBufferSize, length), IntPtr.Zero, ref overlappedRead))
                    {
                        if (ERROR_IO_PENDING == (long)Marshal.GetLastWin32Error())
                        {
                            bool completed = false;

                            while (!completed)
                            {
                                uint result = WaitForSingleObject(overlappedRead.hEvent, 500);
                                switch (result)
                                {
                                    case WAIT_OBJECT_0:
                                        completed = true;
                                        break;
                                    case WAIT_TIMEOUT:
                                        if (cancellationToken.IsCancellationRequested)
                                            throw new System.ApplicationException("ReadBytes: cancelled.");
                                        break;
                                    default:
                                        throw new System.ApplicationException("ReadBytes: WaitForSingleObject error.");
                                }
                            }
                        }
                        else
                        {
                            throw new System.ApplicationException("ReadBytes: ReadFile failed.");
                        }
                    }

                    if (!GetOverlappedResult(pipeHandle, ref overlappedRead, out bytesRead, false))
                    {
                        throw new System.ApplicationException("ReadBytes: GetOverlappedResult failed: "+Marshal.GetLastWin32Error());
                    }

                    Marshal.Copy(readBuffer, bytes, offset, (int)bytesRead);

                    offset += (int)bytesRead;
                    length -= (int)bytesRead;

                    if (0 >= length)
                        break;
                }               
            }

            public byte[] ReadBytes(int length, CancellationToken cancellationToken)
            {

                byte[] result = new byte[length];

                ReadBytes(result, 0, length, cancellationToken);

                return result;
            }

            public Boolean ReadBoolean(CancellationToken cancellationToken)
            {
                return 0 != ReadByte(cancellationToken);
            }

            public Byte ReadByte(CancellationToken cancellationToken)
            {
                return ReadBytes(sizeof(Byte), cancellationToken)[0];
            }

            public SByte ReadSByte(CancellationToken cancellationToken)
            {
                return (SByte)ReadByte( cancellationToken);
            }

            public UInt32 ReadUInt32(CancellationToken cancellationToken)
            {
                return BitConverter.ToUInt32(ReadBytes(sizeof(UInt32), cancellationToken), 0);
            }

            public UInt32 ReadCompressedUInt32(CancellationToken cancellationToken)
            {
                Byte value = ReadByte(cancellationToken);

                if (value < Byte.MaxValue)
                    return value;

                return ReadUInt32(cancellationToken);
            }

            public Int32 ReadInt32(CancellationToken cancellationToken)
            {
                return BitConverter.ToInt32(ReadBytes(sizeof(Int32), cancellationToken), 0);
            }

            public Int32 ReadCompressedInt32(CancellationToken cancellationToken)
            {
                SByte value = ReadSByte(cancellationToken);

                if (value < SByte.MaxValue)
                    return value;

                return ReadInt32(cancellationToken);
            }

            public String ReadUTF8String(CancellationToken cancellationToken)
            {
                int length = (int)ReadCompressedUInt32(cancellationToken);

                return System.Text.UnicodeEncoding.UTF8.GetString(ReadBytes(length, cancellationToken));
            }

            public Single ReadSingle(CancellationToken cancellationToken)
            {

                return BitConverter.ToSingle(ReadBytes(sizeof(Single), cancellationToken), 0);
            }

            public IntPtr ReadHandle(CancellationToken cancellationToken)
            {
                Int32 intValue = ReadInt32(cancellationToken);

                return new IntPtr(intValue);
            }

            public void WriteBytes(byte[] bytes, int offset, int length, CancellationToken cancellationToken)
            {
                while (true)
                {
                    uint bytesWritten = 0;
                    uint bytesToWrite = (uint)Math.Min(writeBufferSize, length);

                    Marshal.Copy(bytes, offset, writeBuffer, (int)bytesToWrite);

                    if (!WriteFile(pipeHandle, writeBuffer, bytesToWrite, IntPtr.Zero, ref overlappedWrite))
                    {
                        if (ERROR_IO_PENDING == (long)Marshal.GetLastWin32Error())
                        {
                            bool completed = false;

                            while (!completed)
                            {
                                uint result = WaitForSingleObject(overlappedWrite.hEvent, 500);
                                switch (result)
                                {
                                    case WAIT_OBJECT_0:
                                        completed = true;
                                        break;
                                    case WAIT_TIMEOUT:
                                        if (cancellationToken.IsCancellationRequested)
                                            throw new System.ApplicationException("WriteBytes: cancelled.");
                                        break;
                                    default:
                                        throw new System.ApplicationException("WriteBytes: WaitForSingleObject error.");
                                }
                            }
                        }
                        else
                        {
                            throw new System.ApplicationException("WriteBytes: WriteFile failed.");
                        }
                    }

                    if (!GetOverlappedResult(pipeHandle, ref overlappedWrite, out bytesWritten, false))
                    {
                        throw new System.ApplicationException("WriteBytes: GetOverlappedResult failed.");
                    }

                    offset += (int)bytesWritten;
                    length -= (int)bytesWritten;

                    if (0 >= length)
                        break;
                }
            }

            public void WriteBytes(byte[] bytes, CancellationToken cancellationToken)
            {

                WriteBytes(bytes, 0, bytes.Length, cancellationToken);
            }

            public void Flush(CancellationToken cancellationToken)
            {
                if(!FlushFileBuffers(pipeHandle))
                    throw new System.ApplicationException("FlushFileBuffers failed.");
            }

            public void WriteBoolean(Boolean value, CancellationToken cancellationToken)
            {
                WriteByte(value ? (Byte)1 : (Byte)0, cancellationToken);
            }

            public void WriteByte(Byte value, CancellationToken cancellationToken)
            {
                WriteBytes(new Byte[1] { value }, cancellationToken);
            }

            public void WriteSByte(SByte value, CancellationToken cancellationToken)
            {
                WriteByte((Byte)value, cancellationToken);
            }

            public void WriteUInt32(UInt32 value, CancellationToken cancellationToken)
            {

                WriteBytes(BitConverter.GetBytes(value), cancellationToken);
            }

            public void WriteCompressedUInt32(UInt32 value, CancellationToken cancellationToken)
            {
                if(Byte.MinValue <= value && value <= Byte.MaxValue -1)
                {
                    WriteByte((Byte)value, cancellationToken);
                }
                else
                {
                    WriteByte(Byte.MaxValue, cancellationToken);
                    WriteUInt32(value, cancellationToken);
                }
            }

            public void WriteInt32(Int32 value, CancellationToken cancellationToken)
            {

                WriteBytes(BitConverter.GetBytes(value), cancellationToken);
            }

            public void WriteCompressedInt32(Int32 value, CancellationToken cancellationToken)
            {
                if(SByte.MinValue <= value && value <= SByte.MaxValue - 1)
                {
                    WriteSByte((SByte)value, cancellationToken);
                }
                else
                {
                    WriteSByte(SByte.MaxValue, cancellationToken);
                    WriteInt32(value, cancellationToken);
                }
            }

            public void WriteHandle(IntPtr value, CancellationToken cancellationToken)
            {

                WriteInt32(value.ToInt32(), cancellationToken);
            }

            public void WriteStringUTF8(String value, CancellationToken cancellationToken)
            {
                byte[] bytes = System.Text.UnicodeEncoding.UTF8.GetBytes(value);

                WriteCompressedUInt32((UInt32)bytes.Length, cancellationToken);
                WriteBytes(bytes, cancellationToken);
            }

            [Flags]
            public enum PipeOpenModeFlags : uint
            {
                PIPE_ACCESS_DUPLEX = 0x00000003,
                PIPE_ACCESS_INBOUND = 0x00000001,
                PIPE_ACCESS_OUTBOUND = 0x00000002,
                FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000,
                FILE_FLAG_WRITE_THROUGH = 0x80000000,
                FILE_FLAG_OVERLAPPED = 0x40000000,
                WRITE_DAC = 0x00040000,
                WRITE_OWNER = 0x00080000,
                ACCESS_SYSTEM_SECURITY = 0x01000000
            }

            [Flags]
            public enum PipeModeFlags : uint
            {
                //One of the following type modes can be specified. The same type mode must be specified for each instance of the pipe.
                PIPE_TYPE_BYTE = 0x00000000,
                PIPE_TYPE_MESSAGE = 0x00000004,
                //One of the following read modes can be specified. Different instances of the same pipe can specify different read modes
                PIPE_READMODE_BYTE = 0x00000000,
                PIPE_READMODE_MESSAGE = 0x00000002,
                //One of the following wait modes can be specified. Different instances of the same pipe can specify different wait modes.
                PIPE_WAIT = 0x00000000,
                PIPE_NOWAIT = 0x00000001,
                //One of the following remote-client modes can be specified. Different instances of the same pipe can specify different remote-client modes.
                PIPE_ACCEPT_REMOTE_CLIENTS = 0x00000000,
                PIPE_REJECT_REMOTE_CLIENTS = 0x00000008
            }

            [StructLayout(LayoutKind.Sequential)] // Be aware of 32 bit vs 64 bit here, LayoutKind.Explicit is tricky.
            public struct OVERLAPPED
            {
                public IntPtr Internal;
                public IntPtr InternalHigh;
                //public uint Offset;
                //public uint OffsetHigh;
                public IntPtr Pointer;
                public IntPtr hEvent;
            }

            const UInt32 ERROR_PIPE_CONNECTED = 535;
            const UInt32 ERROR_IO_PENDING = 997;

            const UInt32 INFINITE = 0xFFFFFFFF;
            const UInt32 WAIT_ABANDONED = 0x00000080;
            const UInt32 WAIT_OBJECT_0 = 0x00000000;
            const UInt32 WAIT_TIMEOUT = 0x00000102;

            private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr CreateNamedPipe(string lpName, uint dwOpenMode,
                uint dwPipeMode, uint nMaxInstances, uint nOutBufferSize, uint nInBufferSize,
                uint nDefaultTimeOut, IntPtr lpSecurityAttributes);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool ConnectNamedPipe(IntPtr hNamedPipe,
                [In] ref OVERLAPPED lpOverlapped);

            [DllImport("kernel32.dll", SetLastError = true)]
            //[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            //[SuppressUnmanagedCodeSecurity]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool CloseHandle(IntPtr hObject);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool WriteFile(IntPtr hFile, IntPtr lpBuffer,
                uint nNumberOfBytesToWrite, IntPtr lpNumberOfBytesWritten,
                 ref OVERLAPPED lpOverlapped);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool ReadFile(IntPtr hFile, IntPtr lpBuffer,
                uint nNumberOfBytesToRead, IntPtr lpNumberOfBytesRead, ref OVERLAPPED lpOverlapped);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool FlushFileBuffers(IntPtr hFile);

            [DllImport("kernel32.dll")]
            static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

            [DllImport("kernel32.dll")]
            static extern bool SetEvent(IntPtr hEvent);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern bool GetOverlappedResult(IntPtr hFile,
                [In] ref OVERLAPPED lpOverlapped,
                out uint lpNumberOfBytesTransferred, bool bWait);
        }
    }

}
