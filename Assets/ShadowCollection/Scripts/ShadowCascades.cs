using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyCascaedShadow;
using UnityEngine.Experimental.Rendering;
using System;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.Experimental.GlobalIllumination;
using System.Linq;
using static MyCascaedShadow.CascadeSettings;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]

public class ShadowCascades : MonoBehaviour
{
    [SerializeField]
    private CascadeSettings cascadeSettings=new CascadeSettings();


    
    private List<Matrix4x4> cascadeShadowMatrices;
    private List<Matrix4x4> cascadeCameraMatrices;
    private Camera m_camera;

    float[] fars ;
    float[] nears ;

#if UNITY_EDITOR
    private bool display=false;

    public bool Display
    {
        get => display;
        set => display = value;

    }
#endif
    public Camera camera
    {
        get
        {
            if (m_camera == null)
            {
                m_camera=GetComponent<Camera>();
            }
            return m_camera;
        }
    }

    Rect rect = new Rect(0, 0, 1, 1);




    class Frustum
    {
        public Vector3[] farCorners;
        public Vector3[] nearCorners;

        public Frustum()
        {
            farCorners=new Vector3[4];
            nearCorners=new Vector3[4];
        }
    }

#if UNITY_EDITOR
    private List<Frustum> mainCameraFrustum;

    private List<Frustum> lightFrustum;
#endif

    private void OnEnable()
    {


    }


    [SerializeField]
    private Light light;

  
   // private ShadowRenderer shadowRenderer;

    private List<Renderer> renderers=new List<Renderer>();


    static readonly int cascadesMatrices = Shader.PropertyToID("cascadesMatrices");
    static readonly int tocameraMatrices = Shader.PropertyToID("cascadesWorldToCameraMatrices");
    static readonly int cascadesNums = Shader.PropertyToID("cascadesNums");


    CommandBuffer commandBuffer;


    private string PassName = "My Shadow";


    private RenderTexture[] shadowTextures = null;

    private Action action;

    private List<Renderer> shadowObjects;

    private Camera lightcamera;
    [SerializeField]
    private Material shadowCaster;
    void Start()
    {
        renderers = UnityEngine.Object.FindObjectsOfType<Renderer>().ToList();

        renderers.ForEach(delegate(Renderer render)
        {
            Debug.Log(render.gameObject.name);
        }
        );

        Initialize(cascadeSettings.split, cascadeSettings.casacdesize);

        //shadowRenderer = new ShadowRenderer(cascadeSettings,renderers,cascadeShadowMatrices,
        //    cascadeCameraMatrices,cascadeSettings.split,light,ref rect);

        shadowTextures = new RenderTexture[cascadeSettings.split];

        int size = cascadeSettings.casacdesize;
        int nums = cascadeSettings.split;
        for (int i = 0; i < nums; i++)
        {
            shadowTextures[i] = new RenderTexture(size , size ,24);
            shadowTextures[i].format = RenderTextureFormat.R16;
            shadowTextures[i].Create();
        }

        shadowObjects = renderers;

        shadowCaster = new Material(Shader.Find("My/ShadowCaster"));
        shadowCaster.hideFlags = HideFlags.HideAndDontSave;

        commandBuffer=new CommandBuffer();
        commandBuffer.name = "CSMTest";

        //lightcamera.CopyFrom(camera);

        //lightcamera.

        //RenderRegister();
        //Camera.main.AddCommandBuffer(CameraEvent.BeforeDepthTexture, commandBuffer);
        //cascadeSettings.dataUpdate = this.Initialize;

    }



    // Update is called once per frame
    void Update()
    {
        UpdateFrustum();


        RenderRegister();
       // shadowRenderer.Render(cascadeShadowMatrices, cascadeSettings.split,light,ref rect);
    }


    void Initialize(int num,int size)
    {

        UpdateCascades(num);

        if (cascadeShadowMatrices == null)
        {
            cascadeShadowMatrices = new List<Matrix4x4>(num);

            for (int i = 0; i < num; i++)
            {
                cascadeShadowMatrices.Add(new Matrix4x4());
            }
        }

       
        if (cascadeCameraMatrices == null)
        {
            cascadeCameraMatrices = new List<Matrix4x4>(num);

            for (int i = 0; i < num; i++)
            {
                cascadeCameraMatrices.Add(new Matrix4x4());
            }
        }
#if UNITY_EDITOR
        if (mainCameraFrustum == null)
        {
            mainCameraFrustum = new List<Frustum>(num);

            for (int i = 0; i < num; i++)
            {
                mainCameraFrustum.Add(new Frustum());
            }
        }

        if (lightFrustum == null)
        {
            lightFrustum = new List<Frustum>(num);

            for (int i = 0; i < num; i++)
            {
                lightFrustum.Add(new Frustum());
            }
        }
#endif

    }

    private Vector3[] Position4=new Vector3[4];

    public void UpdateFrustum()
    {
        for (int i = 0;i < cascadeSettings.split; i++)
        {
            #region One
            camera.CalculateFrustumCorners(rect, nears[i], Camera.MonoOrStereoscopicEye.Mono, mainCameraFrustum[i].nearCorners);
            camera.CalculateFrustumCorners(rect, fars[i], Camera.MonoOrStereoscopicEye.Mono, mainCameraFrustum[i].farCorners);

            Vector3[] farsWorld= new Vector3[4];
            Vector3[] nearsWorld= new Vector3[4];

            camera.transform.TransformPoints(mainCameraFrustum[i].farCorners, farsWorld);
            camera.transform.TransformPoints(mainCameraFrustum[i].nearCorners, nearsWorld);

            //for(int j=0; j < 4; j++)
            //{
            //    lightFrustum[i].farCorners[j] = light.transform.worldToLocalMatrix.MultiplyPoint3x4(farsWorld[j]);
            //    lightFrustum[i].nearCorners[j] = light.transform.worldToLocalMatrix.MultiplyPoint3x4(nearssWorld[j]);
            //}


            


            //light.transform.InverseTransformPoints(farsWorld,lightFrustum[i].farCorners);
            //light.transform.InverseTransformPoints(nearsWorld, lightFrustum[i].nearCorners);
            var minx = Mathf.Min(
                farsWorld[0].x, nearsWorld[0].x,
                farsWorld[1].x, nearsWorld[1].x,
                farsWorld[2].x, nearsWorld[2].x,
                farsWorld[3].x, nearsWorld[3].x);
            var maxx = Mathf.Max(
                farsWorld[0].x, nearsWorld[0].x,
                farsWorld[1].x, nearsWorld[1].x,
                farsWorld[2].x, nearsWorld[2].x,
                farsWorld[3].x, nearsWorld[3].x);
            var miny = Mathf.Min(
                farsWorld[0].y, nearsWorld[0].y,
                farsWorld[1].y, nearsWorld[1].y,
                farsWorld[2].y, nearsWorld[2].y,
                farsWorld[3].y, nearsWorld[3].y);
            var maxy = Mathf.Max(
                farsWorld[0].y, nearsWorld[0].y,
                farsWorld[1].y, nearsWorld[1].y,
                farsWorld[2].y, nearsWorld[2].y,
                farsWorld[3].y, nearsWorld[3].y);
            var minz = Mathf.Min(
                farsWorld[0].z, nearsWorld[0].z,
                farsWorld[1].z, nearsWorld[1].z,
                farsWorld[2].z, nearsWorld[2].z,
                farsWorld[3].z, nearsWorld[3].z);
            var maxz = Mathf.Max(
                farsWorld[0].z, nearsWorld[0].z,
                farsWorld[1].z, nearsWorld[1].z,
                farsWorld[2].z, nearsWorld[2].z,
                farsWorld[3].z, nearsWorld[3].z);


            //Debug.Log($"minz-maxz: +{minz}+' '+{maxz}");

            #endregion

            #region Two
            var tr = light.transform;


            var center = new Vector3() 
            {
                x = (minx+maxx)*0.5f,
                y = (miny+maxy)*0.5f,
                z = (minz+maxz)*0.5f,
            };


            var position = center - tr.forward*Mathf.Abs(maxz-minz);

            Position4[i]= position;

            //position += new Vector3(0, 0, 20f);
            var lookMatrix = Matrix4x4.LookAt(
            position,
            position + tr.forward,
            Vector3.up
            );

            var scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            cascadeCameraMatrices[i] = scaleMatrix*lookMatrix.inverse;//worldTolightCamera;



            Vector3[] tempFars = new Vector3[4]
            {
                lookMatrix.inverse.MultiplyPoint(new Vector3(minx,miny,maxz)),
                lookMatrix.inverse.MultiplyPoint(new Vector3(minx,maxy,maxz)),
                lookMatrix.inverse.MultiplyPoint(new Vector3(maxx,maxy,maxz)),
                lookMatrix.inverse.MultiplyPoint(new Vector3(maxx,miny,maxz)),
            };

            Vector3[] tempNears = new Vector3[4]
            {
                lookMatrix.inverse.MultiplyPoint(new Vector3(minx,miny,minz)),
                lookMatrix.inverse.MultiplyPoint(new Vector3(minx,maxy,minz)),
                lookMatrix.inverse.MultiplyPoint(new Vector3(maxx,maxy,minz)),
                lookMatrix.inverse.MultiplyPoint(new Vector3(maxx,miny,minz)),
            };


            minx = Mathf.Min(
                tempFars[0].x, tempNears[0].x,
                tempFars[1].x, tempNears[1].x,
                tempFars[2].x, tempNears[2].x,
                tempFars[3].x, tempNears[3].x
                );            
            maxx = Mathf.Max(
                tempFars[0].x, tempNears[0].x,
                tempFars[1].x, tempNears[1].x,
                tempFars[2].x, tempNears[2].x,
                tempFars[3].x, tempNears[3].x
                );
            miny= Mathf.Min(
                tempFars[0].y, tempNears[0].y,
                tempFars[1].y, tempNears[1].y,
                tempFars[2].y, tempNears[2].y,
                tempFars[3].y, tempNears[3].y
                );
            maxy = Mathf.Max(
                tempFars[0].y, tempNears[0].y,
                tempFars[1].y, tempNears[1].y,
                tempFars[2].y, tempNears[2].y,
                tempFars[3].y, tempNears[3].y
                );
            minz = Mathf.Min(
                tempFars[0].z, tempNears[0].z,
                tempFars[1].z, tempNears[1].z,
                tempFars[2].z, tempNears[2].z,
                tempFars[3].z, tempNears[3].z
                );
            maxz = Mathf.Max(
                tempFars[0].z, tempNears[0].z,
                tempFars[1].z, tempNears[1].z,
                tempFars[2].z, tempNears[2].z,
                tempFars[3].z, tempNears[3].z
                );


            cascadeShadowMatrices[i] = Matrix4x4.Ortho(minx, maxx, miny, maxy, minz, maxz);
            //cascadeShadowMatrices[i] = GL.  (cascadeShadowMatrices[i], false);


            //Debug.Log($"µÚ{i}µÄorthogonal");
            //Debug.Log(GL.GetGPUProjectionMatrix(cascadeShadowMatrices[i],false));
#if UNITY_EDITOR
            lightFrustum[i].farCorners = new Vector3[]
            {
                new Vector3(minx,miny,maxz),
                new Vector3(minx,maxy,maxz),
                new Vector3(maxx,maxy,maxz),
                new Vector3(maxx,miny,maxz),
            };

            lightFrustum[i].nearCorners = new Vector3[]
            {
                new Vector3(minx,miny,minz),
                new Vector3(minx,maxy,minz),
                new Vector3(maxx,maxy,minz),
                new Vector3(maxx,miny,minz),
            };
#endif
            #endregion



        }

    }


    public void UpdateCascades(int num)
    {
        var near = camera.nearClipPlane;
        var far = camera.farClipPlane;

        Debug.Log("Near: " + near + "\n" + "Far: " + far);

        fars = ArrayPool<float>.New(num);
        nears = ArrayPool<float>.New(num);


        if (num == 1)
        {
            fars[0] = far;
            nears[0] = near;
        }
        else if (num == 2)
        {
            nears[0] = near; nears[1] = far * 0.5f + near;
            fars[0] = nears[1]; fars[1] = far;
        }
        else if (num == 4)
        {
            nears[0] = near;
            nears[1] = far * 0.067f + near;
            nears[2] = far * 0.133f + far * 0.067f + near;
            nears[3] = far * 0.267f + far * 0.133f + far * 0.067f + near;

            fars[0] = far * 0.067f + near;
            fars[1] = far * 0.133f + far * 0.067f + near;
            fars[2] = far * 0.267f + far * 0.133f + far * 0.067f + near;
            fars[3] = far;

        }
    }
    void Clean()
    {
 
        if (cascadeShadowMatrices != null)
        {
            cascadeShadowMatrices.Clear();
            cascadeShadowMatrices = null;
        }

        if (cascadeCameraMatrices != null)
        {
            cascadeCameraMatrices.Clear();
            cascadeCameraMatrices = null;
        }
        if (fars != null)
        fars.Free();
        if (nears != null)
        nears.Free();
#if UNITY_EDITOR
        if (mainCameraFrustum != null)
        {
            mainCameraFrustum.Clear();
            mainCameraFrustum = null;
        }

        if (lightFrustum != null)
        {
            lightFrustum.Clear();
            lightFrustum = null;
        }
#endif

    }

    public void RenderRegister()
    {

        commandBuffer.Clear();
        commandBuffer.SetGlobalMatrixArray(cascadesMatrices, cascadeShadowMatrices);
        commandBuffer.SetGlobalMatrixArray(tocameraMatrices, cascadeCameraMatrices);
        commandBuffer.SetGlobalInt(cascadesNums, cascadeSettings.split);

        for (int i = 0; i < cascadeSettings.split; i++)
        {
            commandBuffer.BeginSample(PassName + i);
            //commandBuffer.SetProjectionMatrix(cascadeShadowMatrices[i]);
            //commandBuffer.SetViewMatrix(cascadeCameraMatrices[i]);
            commandBuffer.SetRenderTarget(shadowTextures[i]);
            commandBuffer.ClearRenderTarget(true, true, Color.black);
            commandBuffer.SetViewProjectionMatrices(cascadeCameraMatrices[i],cascadeShadowMatrices[i]);
            commandBuffer.SetViewport(new Rect(0, 0, shadowTextures[i].width, shadowTextures[i].height));
            //commandBuffer.SetViewport(new Rect(0, 0, 1,1));
            


            foreach (Renderer renderer in shadowObjects)
            {
                commandBuffer.DrawRenderer(renderer, shadowCaster);

            }
            commandBuffer.EndSample(PassName + i);

        }


        Graphics.ExecuteCommandBuffer(commandBuffer);
    }
#if UNITY_EDITOR
    Color[] drawcolors = new Color[4]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow
    };
    private void OnDrawGizmos()
    {

        if (Display)
        {

            var scaleMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            scaleMatrix = scaleMatrix.inverse;
            for (int i=0; i < cascadeSettings.split; i++)
            {
                Gizmos.color = drawcolors[i];
                Vector3[] fars = new Vector3[4];
                Vector3[] nears = new Vector3[4];

                var frustum = lightFrustum[i];
                var matrix = cascadeCameraMatrices[i].inverse*scaleMatrix; //CameraToWorld


                for (int j = 0; j < 4; j++)
                {

                    fars[j] = matrix.MultiplyPoint(frustum.farCorners[j]);
                    nears[j] =matrix.MultiplyPoint(frustum.nearCorners[j]);

                    Gizmos.DrawLine(
                            fars[j],
                            nears[j]
                    );

                }

                Gizmos.DrawLineStrip(fars, true);
                Gizmos.DrawLineStrip(nears, true);

            }



            Gizmos.color = Color.white;
            foreach (var frustum in mainCameraFrustum)
            {
                Vector3[] fars = new Vector3[4];
                Vector3[] nears = new Vector3[4];

                camera.transform.TransformPoints(frustum.farCorners, fars);
                camera.transform.TransformPoints(frustum.nearCorners, nears);

                Gizmos.DrawLineStrip(fars, true);
                Gizmos.DrawLineStrip(nears, true);

                for (int i = 0; i < cascadeSettings.split; i++)
                {
                    Gizmos.DrawLine(
                        camera.transform.TransformPoint(frustum.farCorners[i]),
                        camera.transform.TransformPoint(frustum.nearCorners[i])
                        );
                }
            }




        }
    }


#endif

    private void OnDestroy()
    {
        Clean();
    }
}
