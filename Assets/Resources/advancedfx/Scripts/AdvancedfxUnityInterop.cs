using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

/*
 
QuakeRoll(r) =
/ 1,  0,   0, 0 \
| 0, cr, -sr, 0 |
| 0, sr,  cr, 0 |
\ 0,  0,   0, 1 /

QuakePitch(p) =
/ cp, 0, sp, 0 \
|  0, 1,  0, 0 |
|-sp, 0, cp, 0 |
\  0, 0,  0, 1 /

QuakeYaw(y) =
/ cy, -sy, 0, 0 \
| sy,  cy, 0, 0 |
|  0,   0, 1, 0 |
\  0,   0, 0, 1 /

QuakeScale(a,b,c) =
/ a, 0, 0, 0 \
| 0, b, 0, 0 |
| 0, 0, c, 0 |
\ 0, 0, 0, 1 /

QuakeTranslate(u,v,w) =
/ 1, 0, 0, u \
| 0, 1, 0, v |
| 0, 0, 1, w |
\ 0, 0, 0, 1 /

QuakeRTS = QuakeYaw(y) * QuakePitch(p) * QuakeRoll(r) * QuakeTranslate(u,v,w) * QuakeScale(a,b,c)

u =  100 / 2.54 * unityZ
v = -100 / 2.54 * unityX
w =  100 / 2.54 * unityY

*/

public class AdvancedfxUnityInterop : MonoBehaviour, advancedfx.Interop.IImplementation
{
    public string pipeName = "advancedfxInterop";

    public volatile bool suspended = false;

    public Int32 Version {
        get {
            return interOp.Version;
        }
    }

    public string PipeName {
        get {
            return interOp.PipeName;
        }
        set {
            interOp.PipeName = value;
        }
    }

    public void Awake() {

        if (IntPtr.Zero == GetModuleHandle("AfxHookUnity.dll"))
        {
            Debug.LogError("AfxHookUnity.dll is not injected. It needs to be injected into Unity early.");
        }

        if (!AfxHookUnityInit(2))
            Debug.LogError("AfxHookUnityInit failed (version mismatch or init failed).");

        Application.runInBackground = true; // don't sleep when not having focus
        QualitySettings.vSyncCount = 0; // render as fast as possible

        interOp = new advancedfx.Interop(this);
        interOp.PipeName = pipeName;

        Camera cam = GetComponent<Camera>();

        if (null != cam)
        {
            cam.allowHDR = false;
            cam.allowMSAA = false;
            cam.allowDynamicResolution = false;
        }

        //this.testTexture = Resources.Load("advancedfx/Textures/logo2009") as Texture;
        //Debug.Log(this.testTexture);
        this.drawDepthMaterial = Resources.Load("advancedfx/Materials/DrawDepth") as Material;
    }

    private Texture testTexture;

    public void OnEnable() {

        interOp.OnEnable();
    }

    public void OnDisable() {
        interOp.OnDisable();
    }

    public void OnDestroy() {

    }

    public void Update()
    {
        interOp.Update();
    }

    //
    // advancedfx.Interop.IImplentation:

    void advancedfx.Interop.ILogging.Log(object message) {
        Debug.Log(message, this);
    }

    void advancedfx.Interop.ILogging.LogException(Exception exception) {
        Debug.LogError(exception, this);
    }

    void advancedfx.Interop.IImplementation.ConnectionLost() {

        ReleaseSurfaces();
    }

    void advancedfx.Interop.IImplementation.Render (advancedfx.Interop.IRenderInfo renderInfo) {

        if (null != renderInfo.FrameInfo)
        {
            float absoluteFrameTime = renderInfo.FrameInfo.AbsoluteFrameTime;
            float frameTime = renderInfo.FrameInfo.FrameTime;

            //Debug.Log(absoluteFrameTime + " / " + frameTime);

            Time.timeScale = 0 == Time.unscaledDeltaTime ? 1 : frameTime / Time.unscaledDeltaTime;
        }
        else
        {
            Debug.Log("No time info available.");

            Time.timeScale = 1;
        }

        if (this.suspended)
            return;

        Camera cam = GetComponent<Camera> ();

        if (null == cam)
        {
            Debug.LogError("No camera set on component.");
            return;
        }

        if (!renderInfo.SurfaceId.HasValue)
        {
            Debug.LogError("Back buffer not set.");
            return;
        }

        SurfaceData surfaceData = null;
        
        if(!surfaceIdToSurfaceData.TryGetValue(renderInfo.SurfaceId.Value, out surfaceData))
        {
            Debug.LogError("Back buffer "+ renderInfo.SurfaceId.Value + " has not been registerred as surface.");
        }

        RenderTexture renderTexture = surfaceData.colorTexture;
        RenderTexture depthTexture = surfaceData.depthTexture;

        if (null == renderTexture)
        {
            Debug.LogError("No color surface.");
            return;
        }

        if(null == depthTexture)
        {
            Debug.LogWarning("No depth color texture surface.");
        }

        switch (renderInfo.Type)
        {
            case advancedfx.Interop.RenderType.Normal:
                {
                    int width = renderInfo.Width;
                    int height = renderInfo.Height;

                    advancedfx.Interop.Afx4x4 afxWorldToView = renderInfo.ViewMatrix;
                    Matrix4x4 d3d9QuakeWorldToView = new Matrix4x4();
                    d3d9QuakeWorldToView[0, 0] = afxWorldToView.M00;
                    d3d9QuakeWorldToView[0, 1] = afxWorldToView.M01;
                    d3d9QuakeWorldToView[0, 2] = afxWorldToView.M02;
                    d3d9QuakeWorldToView[0, 3] = afxWorldToView.M03;
                    d3d9QuakeWorldToView[1, 0] = afxWorldToView.M10;
                    d3d9QuakeWorldToView[1, 1] = afxWorldToView.M11;
                    d3d9QuakeWorldToView[1, 2] = afxWorldToView.M12;
                    d3d9QuakeWorldToView[1, 3] = afxWorldToView.M13;
                    d3d9QuakeWorldToView[2, 0] = afxWorldToView.M20;
                    d3d9QuakeWorldToView[2, 1] = afxWorldToView.M21;
                    d3d9QuakeWorldToView[2, 2] = afxWorldToView.M22;
                    d3d9QuakeWorldToView[2, 3] = afxWorldToView.M23;
                    d3d9QuakeWorldToView[3, 0] = afxWorldToView.M30;
                    d3d9QuakeWorldToView[3, 1] = afxWorldToView.M31;
                    d3d9QuakeWorldToView[3, 2] = afxWorldToView.M32;
                    d3d9QuakeWorldToView[3, 3] = afxWorldToView.M33;

                    advancedfx.Interop.Afx4x4 afxProjectionMatrix = renderInfo.ProjectionMatrix;
                    Matrix4x4 d3d9QuakeProjection = new Matrix4x4();
                    d3d9QuakeProjection[0, 0] = afxProjectionMatrix.M00;
                    d3d9QuakeProjection[0, 1] = afxProjectionMatrix.M01;
                    d3d9QuakeProjection[0, 2] = afxProjectionMatrix.M02;
                    d3d9QuakeProjection[0, 3] = afxProjectionMatrix.M03;
                    d3d9QuakeProjection[1, 0] = afxProjectionMatrix.M10;
                    d3d9QuakeProjection[1, 1] = afxProjectionMatrix.M11;
                    d3d9QuakeProjection[1, 2] = afxProjectionMatrix.M12;
                    d3d9QuakeProjection[1, 3] = afxProjectionMatrix.M13;
                    d3d9QuakeProjection[2, 0] = afxProjectionMatrix.M20;
                    d3d9QuakeProjection[2, 1] = afxProjectionMatrix.M21;
                    d3d9QuakeProjection[2, 2] = afxProjectionMatrix.M22;
                    d3d9QuakeProjection[2, 3] = afxProjectionMatrix.M23;
                    d3d9QuakeProjection[3, 0] = afxProjectionMatrix.M30;
                    d3d9QuakeProjection[3, 1] = afxProjectionMatrix.M31;
                    d3d9QuakeProjection[3, 2] = afxProjectionMatrix.M32;
                    
                    //Debug.Log(d3d9QuakeProjection);

                    if (null != renderInfo.FrameInfo)
                    {
                        const float unityToQuakeScaleFac = 100f / 2.54f;
                        Matrix4x4 unityToQuakeScale = Matrix4x4.Scale(new Vector3(unityToQuakeScaleFac, unityToQuakeScaleFac, unityToQuakeScaleFac));

                        Matrix4x4 unityToQuake = new Matrix4x4();
                        unityToQuake[0, 0] = 0; unityToQuake[0, 1] = 0; unityToQuake[0, 2] = 1; unityToQuake[0, 3] = 0;
                        unityToQuake[1, 0] = -1; unityToQuake[1, 1] = 0; unityToQuake[1, 2] = 0; unityToQuake[1, 3] = 0;
                        unityToQuake[2, 0] = 0; unityToQuake[2, 1] = 1; unityToQuake[2, 2] = 0; unityToQuake[2, 3] = 0;
                        unityToQuake[3, 0] = 0; unityToQuake[3, 1] = 0; unityToQuake[3, 2] = 0; unityToQuake[3, 3] = 1;

                        Matrix4x4 flipViewZ = new Matrix4x4();
                        flipViewZ[0, 0] = 1; flipViewZ[0, 1] = 0; flipViewZ[0, 2] = 0; flipViewZ[0, 3] = 0;
                        flipViewZ[1, 0] = 0; flipViewZ[1, 1] = 1; flipViewZ[1, 2] = 0; flipViewZ[1, 3] = 0;
                        flipViewZ[2, 0] = 0; flipViewZ[2, 1] = 0; flipViewZ[2, 2] = -1; flipViewZ[2, 3] = 0;
                        flipViewZ[3, 0] = 0; flipViewZ[3, 1] = 0; flipViewZ[3, 2] = 0; flipViewZ[3, 3] = 1;

                        Matrix4x4 unityToWorldViewInverse = (flipViewZ * (d3d9QuakeWorldToView * (unityToQuake * unityToQuakeScale))).inverse;

                        const double Rad2Deg = 180.0 / Math.PI;

                        Vector4 quakePosition = unityToWorldViewInverse.GetColumn(3);
                        cam.transform.position = new Vector3(quakePosition.x, quakePosition.y, quakePosition.z);

                        Quaternion rotation = unityToWorldViewInverse.rotation;
                        cam.transform.rotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);

                        Vector3 quakeScale = unityToWorldViewInverse.lossyScale;
                        cam.transform.localScale = new Vector3(quakeScale.x, quakeScale.y, quakeScale.z );

                        //cam.worldToCameraMatrix = unityToWorldView;

                        float C = d3d9QuakeProjection[2, 2]; // - far_plane/(far_plane - near_plane)
                        float D = d3d9QuakeProjection[2, 3]; // C * near_plane

                        //Debug.Log((D / C) + " / " + (D / (C + 1)));

                        cam.nearClipPlane = (D / C) / unityToQuakeScaleFac;
                        cam.farClipPlane = (D / (C + 1)) / unityToQuakeScaleFac;

                        cam.pixelRect = new Rect(0, 0, width, height);
                        cam.rect = new Rect(0, 0, width, height);

                        float horizontalFovRad = (float)Math.Atan(1.0 / d3d9QuakeProjection[0, 0]) * 2.0f;
                        float verticalFovDeg = (float)(2 * Math.Atan(Math.Tan(horizontalFovRad / 2.0) * height / (float)width) * Rad2Deg);

                        //Debug.Log(horizontalFovRad * Rad2Deg + " / " + verticalFovDeg);

                        cam.fieldOfView = verticalFovDeg;
                        cam.aspect = (0 != height) ? (width / (float)height) : 1.0f;

                        CommandBuffer afxWait = new CommandBuffer();
                        afxWait.name = "AfxHookUnity: wait for GPU sinynchronization.";
                        afxWait.IssuePluginEvent(AfxHookUnityGetRenderEventFunc(), 1);

                        cam.AddCommandBuffer(CameraEvent.AfterEverything, afxWait);

                        GL.invertCulling = true;

                        Matrix4x4 orgCamProjection = cam.projectionMatrix;
                        cam.projectionMatrix = GL.GetGPUProjectionMatrix(flipViewZ * orgCamProjection, true);

                        CameraClearFlags oldCameraClearFlags = cam.clearFlags;
                        cam.clearFlags = CameraClearFlags.Depth;

                        CommandBuffer afxDrawDepth = new CommandBuffer();
                        afxDrawDepth.name = "AfxHookUnity: Draw depth buffer color texture.";

                        float orthoZ = cam.nearClipPlane + (cam.nearClipPlane + cam.farClipPlane) / 2.0f;

                        var verticies = new Vector3[4] {
                            new Vector3(0, 0, orthoZ),
                            new Vector3(1, 0, orthoZ),
                            new Vector3(0, 1, orthoZ),
                            new Vector3(1, 1, orthoZ)
                        };

                        var uvs = new Vector2[4] {
                            new Vector2(0, 0),
                            new Vector2(1, 0),
                            new Vector2(0, 1),
                            new Vector2(1, 1),
                        };

                        var triangles = new int[6] {
                            0, 1, 2,
                            2, 1, 3,
                        };

                        var m = new Mesh();
                        m.vertices = verticies;
                        m.uv = uvs;
                        m.triangles = triangles;

                        this.drawDepthMaterial.mainTexture = depthTexture;

                        Vector4 zParams = new Vector4((D / C), (D / (C + 1)), 0);
                        this.drawDepthMaterial.SetVector("_ZParams", zParams);

                        afxDrawDepth.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                        afxDrawDepth.DrawMesh(m, GL.GetGPUProjectionMatrix(flipViewZ * Matrix4x4.Ortho(0,1,1,0,cam.nearClipPlane, cam.farClipPlane), true), this.drawDepthMaterial);

                        cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, afxDrawDepth);

                        cam.targetTexture = renderTexture;
                        cam.Render();

                        this.drawDepthMaterial.mainTexture = null;

                        AfxHookUnityWaitOne();

                        cam.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, afxDrawDepth);
                        cam.RemoveCommandBuffer(CameraEvent.AfterEverything, afxWait);

                        cam.targetTexture = null;

                        cam.clearFlags = oldCameraClearFlags;

                        cam.ResetProjectionMatrix();
                        cam.ResetAspect();

                        GL.invertCulling = false;
                    }
                }
                break;
        }
    }

    void advancedfx.Interop.IImplementation.RegisterSurface(advancedfx.Interop.ISurfaceInfo surfaceInfo, out IntPtr sharedColorTextureHandle, out IntPtr sharedDepthTextureHandle)
    {
        Debug.Log("Registering Surface: " + surfaceInfo.Id);

        SurfaceData surfaceData = new SurfaceData(surfaceInfo);

        surfaceIdToSurfaceData[surfaceInfo.Id] = surfaceData;

        sharedColorTextureHandle = surfaceData.sharedColorTextureHandle;
        sharedDepthTextureHandle = surfaceData.sharedDepthTextureHandle;
    }

    void advancedfx.Interop.IImplementation.ReleaseSurface(IntPtr surfaceId)
    {
        Debug.Log("Releasing Surface: " + surfaceId);

        this.ReleaseSurface(surfaceId);
    }

    IList<String> advancedfx.Interop.IImplementation.EngineThreadCommands(advancedfx.Interop.ICommandArray commands)
    {
        IList<String> reply = new List<String>();

        for (int i = 0; i < commands.Count; ++i)
        {

            advancedfx.Interop.ICommand command = commands[i];

            if (0 < command.Count)
            {
                if (2 <= command.Count && 0 == command[1].CompareTo("afx"))
                {
                    if (4 == command.Count && 0 == command[2].CompareTo("suspended"))
                    {
                        int value;

                        if (int.TryParse(command[3], out value))
                            this.suspended = 0 != value;

                        continue;
                    }
                }

                reply.Add(
                  "echo " + command[0] + " afx suspended 0|1 - Suspend / resume rendering.\n"
                );
            }

        }

        return reply;
    }

    //
    // Private:

    private Material drawDepthMaterial;

    private advancedfx.Interop interOp;

    [StructLayout(LayoutKind.Sequential)] // Be aware of 32 bit vs 64 bit here, LayoutKind.Explicit is tricky.
    public struct AFxHookUnityTextureInfo
    {
        public IntPtr Internal;
        public IntPtr InternalHigh;
        //public uint Offset;
        //public uint OffsetHigh;
        public IntPtr Pointer;
        public IntPtr hEvent;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport ("AfxHookUnity")]
	private static extern bool AfxHookUnityInit(int version);

    [DllImport("AfxHookUnity")]
    private static extern IntPtr AfxHookUnityGetSharedHandle(IntPtr d3d11ResourcePtr);

    [DllImport("AfxHookUnity")]
    private static extern void AfxHookUnityBeginCreateRenderTexture();

    [DllImport("AfxHookUnity")]
    private static extern void AfxHookUnityWaitOne();

    [DllImport("AfxHookUnity")]
    private static extern IntPtr AfxHookUnityGetRenderEventFunc();

    [DllImport("AfxHookUnity")]
    private static extern IntPtr AfxHookUnityGetRenderEventAndDataFunc();

    private class SurfaceData : IDisposable
    {
        public SurfaceData(advancedfx.Interop.ISurfaceInfo surfaceInfo)
        {
            this.surfaceInfo = surfaceInfo;

            RenderTexture colorTexture = null;
            IntPtr sharedColorTextureHandle = IntPtr.Zero;
            RenderTexture depthTexture = null;
            IntPtr sharedDepthTextureHandle = IntPtr.Zero;

            Nullable<RenderTextureDescriptor> rdesc;
            
            rdesc = GetRenderTextureDescriptor(surfaceInfo, false);
            if (rdesc.HasValue)
            {
                AfxHookUnityBeginCreateRenderTexture();
                colorTexture = new RenderTexture(rdesc.Value);
                colorTexture.Create();
                sharedColorTextureHandle = AfxHookUnityGetSharedHandle(colorTexture.GetNativeTexturePtr());
                Debug.Log("Color: " + colorTexture.GetNativeTexturePtr() + " -> " + sharedColorTextureHandle);
            }

            rdesc = GetRenderTextureDescriptor(surfaceInfo, true);
            if (rdesc.HasValue)
            {
                AfxHookUnityBeginCreateRenderTexture();
                depthTexture = new RenderTexture(rdesc.Value);
                depthTexture.Create();
                sharedDepthTextureHandle = AfxHookUnityGetSharedHandle(depthTexture.GetNativeTexturePtr());
                Debug.Log("Depth: " + depthTexture.GetNativeTexturePtr() + " -> " + sharedDepthTextureHandle);
            }

            this.colorTexture = colorTexture;
            this.sharedColorTextureHandle = sharedColorTextureHandle;
            this.depthTexture = depthTexture;
            this.sharedDepthTextureHandle = sharedDepthTextureHandle;
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

            if (colorTexture) colorTexture.Release();
            if (depthTexture) depthTexture.Release();

            disposed = true;
        }

        public readonly advancedfx.Interop.ISurfaceInfo surfaceInfo;
        public readonly RenderTexture colorTexture;
        public readonly IntPtr sharedColorTextureHandle;
        public readonly RenderTexture depthTexture;
        public readonly IntPtr sharedDepthTextureHandle;

        private Nullable<RenderTextureDescriptor> GetRenderTextureDescriptor(advancedfx.Interop.ISurfaceInfo fbSurfaceInfo, bool isDepth)
        {
            RenderTextureDescriptor desc = new RenderTextureDescriptor((int)fbSurfaceInfo.Width, (int)fbSurfaceInfo.Height);

            Debug.Log("GetRenderTextureDescriptor: " + fbSurfaceInfo.Id + "(" + (isDepth ? "depth" : "color") +")" + ": " + fbSurfaceInfo.Format + " (" + fbSurfaceInfo.Width + "," + fbSurfaceInfo.Height + ")");

            switch (fbSurfaceInfo.Format)
            {
                case advancedfx.Interop.D3DFORMAT.D3DFMT_A8R8G8B8:
                    desc.colorFormat = RenderTextureFormat.BGRA32;
                    break;
                default:
                    Debug.LogError("Unknown back buffer format: " + fbSurfaceInfo.Format);
                    return null;
            }

            desc.depthBufferBits = isDepth ? 0 : 32;

            desc.autoGenerateMips = false;
            desc.bindMS = false;
            desc.enableRandomWrite = false;
            desc.sRGB = false; // for windowed CS:GO at least (?).
            desc.msaaSamples = 1;
            desc.useMipMap = false;

            return new Nullable<RenderTextureDescriptor>(desc);
        }
    }

	private Dictionary<IntPtr, SurfaceData> surfaceIdToSurfaceData = new Dictionary<IntPtr, SurfaceData> ();

    private void ReleaseSurface(IntPtr surfaceId)
    {
        SurfaceData surfaceData = null;

        if (surfaceIdToSurfaceData.TryGetValue(surfaceId, out surfaceData))
        {
            surfaceData.Dispose();
        }

        surfaceIdToSurfaceData.Remove(surfaceId);
    }

    private void ReleaseSurfaces()
    {
       while(true)
       {
            IEnumerator<IntPtr> keyEnumerator = surfaceIdToSurfaceData.Keys.GetEnumerator();
            if (!keyEnumerator.MoveNext())
                return;

            ReleaseSurface(keyEnumerator.Current);
       }
    }	
}

