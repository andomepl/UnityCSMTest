using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MyCascaedShadow
{

    public class ShadowRenderer
    {


        static readonly int cascadesMatrices = Shader.PropertyToID("cascadesMatrices");
        static readonly int cascadesNums = Shader.PropertyToID("cascadesNums");


        CommandBuffer commandBuffer = new CommandBuffer();


        private string PassName = "My Shadow";


        private RenderTexture[] shadowTextures = null;

        private Action action;

        private List<Renderer> shadowObjects;

        [SerializeField]
        private Material shadowCaster;
        public ShadowRenderer(CascadeSettings cascadeSettings,List<Renderer> renderers
            , List<Matrix4x4> matrices,List<Matrix4x4> cameraMatrices ,int cascades, Light directionLight, ref Rect rect)
        {
            shadowTextures = new RenderTexture[cascadeSettings.split];

            int size = cascadeSettings.casacdesize;
            int nums = cascadeSettings.split;
            for (int i = 0; i < nums; i++)
            {
                shadowTextures[i] = new RenderTexture(size / (i + 1), size / (i + 1), 24);
                shadowTextures[i].format = RenderTextureFormat.RFloat;
                shadowTextures[i].Create();
            }

            shadowObjects= renderers;

            shadowCaster=new Material(Shader.Find("My/ShadowCaster"));
            shadowCaster.hideFlags = HideFlags.HideAndDontSave;

            Render(matrices, cameraMatrices,cascades, directionLight,ref rect);
            Camera.main.AddCommandBuffer(CameraEvent.BeforeDepthTexture, commandBuffer);
        }


        public void Render(List<Matrix4x4> matrices, List<Matrix4x4> cameraMatrices, int cascades,Light directionLight,ref Rect rect)
        {
            commandBuffer.Clear();
            commandBuffer.SetGlobalMatrixArray(cascadesMatrices, matrices);
            commandBuffer.SetGlobalInt(cascadesNums, cascades);




            for (int i = 0; i < cascades; i++)
            {
                commandBuffer.BeginSample(PassName+i);
                commandBuffer.SetProjectionMatrix(matrices[i]);
                commandBuffer.SetViewMatrix(cameraMatrices[i]);
                commandBuffer.SetViewProjectionMatrices(cameraMatrices[i],GL.GetGPUProjectionMatrix(matrices[i], false));
                commandBuffer.SetViewport(rect);
                commandBuffer.SetRenderTarget(shadowTextures[i]);

                foreach (Renderer renderer in shadowObjects)
                {
                    commandBuffer.DrawRenderer(renderer, shadowCaster);
                 
                }
                
                commandBuffer.EndSample(PassName+i);

            }


            //Graphics.ExecuteCommandBuffer(commandBuffer);
        }




        public void Destroy()
        {
            commandBuffer.Dispose();
            foreach (var tex in shadowTextures)
            {
                tex.Release();
            }

        }



    }



}
