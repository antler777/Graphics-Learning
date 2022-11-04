using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ColorAdjustmentFeeature : ScriptableRendererFeature
{
   
    [System.Serializable]
    public class Setting
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public Material material;
        public float _Brightness = 1;
        public float _Saturation = 1;
        public float _Contrast = 1;
        [Range(0.05f, 3.0f)]
        public float _VignetteIntensity = 1.5f;
        [Range(1.0f, 6.0f)]
        public float _VignetteRoundness = 5.0f;
        [Range(0.05f, 5.0f)]
        public float _VignetteSmoothness = 5.0f;
        [Range(0.0f, 1.0f)]
        public float _HueShift = 0.0f;
    }
  
    public Setting settings = new Setting();

    public class ColorAdjustmentRenderPass : ScriptableRenderPass
    {
        //获取材质球参数
        public ColorAdjustmentFeeature.Setting settings;
        //两张RT的ID
        RenderTargetIdentifier source;
        RenderTargetIdentifier destination;
       //创建临时ID
        int temporaryRTId = Shader.PropertyToID("_TempRT");
        
        //摄像机初始化，每次使用调用这个函数
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //rendererData存储了渲染所需要的所有数据，然后获取cameradata的数据餐后renderer，然后就是colortarget
            source = renderingData.cameraData.renderer.cameraColorTarget;
            //获取当前摄像机RT的格式（信息）
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            //获取临时RT纹理，通过id访问RT
            cmd.GetTemporaryRT(temporaryRTId, descriptor);
            destination = new RenderTargetIdentifier(temporaryRTId);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("ColorAdjustment");

            settings.material.SetFloat("_Brightness", settings._Brightness);
            settings.material.SetFloat("_Saturation", settings._Saturation);
            settings.material.SetFloat("_Contrast", settings._Contrast);
            settings.material.SetFloat("_VignetteIntensity", settings._VignetteIntensity);
            settings.material.SetFloat("_VignetteRoundness", settings._VignetteRoundness);
            settings.material.SetFloat("_VignetteSmoothness", settings._VignetteSmoothness);
            settings.material.SetFloat("_HueShift", settings._HueShift);


            //把color tex放到临时的RT里面
            Blit(cmd, source, destination, settings.material, 0);
            //然后放回去
            Blit(cmd, destination, source);
            //执行这两个指令
            context.ExecuteCommandBuffer(cmd);
            //释放指令
            CommandBufferPool.Release(cmd);
        }
    }
    ColorAdjustmentRenderPass colorAdjustmentPass;

    public override void Create()
    {
        colorAdjustmentPass = new ColorAdjustmentRenderPass();
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
      if (settings.material == null)
        {
            Debug.LogError("Material NUll");
            return;
        }
        colorAdjustmentPass.settings = settings;
        colorAdjustmentPass.renderPassEvent = settings.renderPassEvent;
        renderer.EnqueuePass(colorAdjustmentPass);
    }


}
