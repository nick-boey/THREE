﻿using ImGuiNET;
using OpenTK;
using THREE;
using System.Diagnostics;
using THREEExample.ThreeImGui;
using Vector3 = THREE.Vector3;
using Vector2 = THREE.Vector2;
namespace THREEExample.Learning.Chapter11
{
    [Example("07-shader-pass-simple", ExampleCategory.LearnThreeJS, "Chapter11")]
    public class ShaderPassSimpleExample : EffectComposerTemplate
    {
        float amount = 1.0f;
        float opacity = 1.0f;
        float brightness = 0.0f;
        float contrast = 0.0f;
        float hue = 0.0f;
        float saturation = 0.0f;
        float sides = 6.0f;
        float angle = 0.0f;
        THREE.Color defaultColor = THREE.Color.Hex(0x000000);
        float luminosityThreshold = 1.0f;
        float smoothWidth = 1.0f;
        float defaultOpacity = 0.0f;
        int side = 1;

        Vector2 pixelResolution = new Vector2(256, 256);

        float pixelSize = 1.0f;

        float rgbAngle = 0.0f;
        float rgbAmount = 0.005f;

        Vector2 sobelResolution = new Vector2(256,256);

        float vignetteOffset=1.0f;
        float darkness=1.0f;
        Color color = new Color(0.5f, 0.5f, 1f);
        Vector3 powRGB = new Vector3(2, 2, 2);
        Vector3 mulRGB = new Vector3(1, 1, 1);
        Vector3 addRGB = new Vector3(0, 0, 0);
        Vector2 aspect = new Vector2(512, 512);
        ShaderPass sepiaShader;
        ShaderPass bleachByPassFilter;
        ShaderPass brightnessContrastShader;
        ShaderPass colorifyShader;
        ShaderPass colorCorrectionShader;
        ShaderPass freiChenShader;
        ShaderPass gammaCorrectionShader;
        ShaderPass hueSaturationShader;
        ShaderPass kaleidoShader;
        ShaderPass luminosityHighPassShader;
        ShaderPass luminosityShader;
        ShaderPass mirrorShader;
        ShaderPass pixelShader;
        ShaderPass rgbShiftShader;
        ShaderPass sobelOperatorShader;
        ShaderPass vignetteShader;

        public ShaderPassSimpleExample() : base()
        {

        }
        public override void Load(GLControl control)
        {
            Debug.Assert(null != control);

            glControl = control;
            this.renderer = new THREE.GLRenderer();

            this.renderer.Context = control.Context;
            this.renderer.Width = control.Width;
            this.renderer.Height = control.Height;

            this.renderer.Init();

            stopWatch.Start();

            InitRenderer();

            InitCamera();

            InitCameraController();

            imGuiManager = new ImGuiManager(this.glControl);

            scene.Background = TextureLoader.Load("../../../../assets/textures/bg/starry-deep-outer-space-galaxy.jpg");

            (earth, pivot) = Util11.AddEarth(scene);

            renderPass = new RenderPass(scene, camera);
            var effectCopy = new ShaderPass(new CopyShader());
            effectCopy.RenderToScreen = true;
            
           

            bleachByPassFilter = new ShaderPass(new BleachBypassShader());
            brightnessContrastShader = new ShaderPass(new BrightnessContrastShader());
            colorifyShader = new ShaderPass(new ColorifyShader());
            colorifyShader.uniforms["color"] = new Uniform { { "value", color } };

            colorCorrectionShader = new ShaderPass(new ColorCorrectionShader());
            freiChenShader = new ShaderPass(new FreiChenShader());

            gammaCorrectionShader = new ShaderPass(new GammaCorrectionShader());
            hueSaturationShader = new ShaderPass(new HueSaturationShader());
            kaleidoShader = new ShaderPass(new KaleidoShader());
            luminosityHighPassShader = new ShaderPass(new LuminosityHighPassShader());
            luminosityHighPassShader.Enabled = false;
            luminosityShader = new ShaderPass(new LuminosityShader());
            mirrorShader = new ShaderPass(new MirrorShader());
            pixelShader = new ShaderPass(new PixelShader());
            (pixelShader.uniforms["resolution"] as Uniform)["value"] = new Vector2(256, 256);
            rgbShiftShader = new ShaderPass(new RGBShiftShader());
            sepiaShader = new ShaderPass(new SepiaShader());
            sepiaShader.uniforms["amount"] = new Uniform { { "value", 0.8f } };
            sobelOperatorShader = new ShaderPass(new SobelOperatorShader());
            (sobelOperatorShader.uniforms["resolution"] as Uniform)["value"] = new Vector2(256, 256);
            vignetteShader = new ShaderPass(new VignetteShader());

            composer = new EffectComposer(renderer);
            composer.AddPass(renderPass);
            composer.AddPass(bleachByPassFilter);
            composer.AddPass(brightnessContrastShader);
            composer.AddPass(colorifyShader);
            composer.AddPass(colorCorrectionShader);
            composer.AddPass(freiChenShader);
            composer.AddPass(gammaCorrectionShader);
            composer.AddPass(hueSaturationShader);
            composer.AddPass(kaleidoShader);
            composer.AddPass(luminosityHighPassShader);
            composer.AddPass(luminosityShader);
            composer.AddPass(mirrorShader);
            composer.AddPass(pixelShader);
            composer.AddPass(rgbShiftShader);            
            composer.AddPass(sepiaShader);
            composer.AddPass(sobelOperatorShader);
            composer.AddPass(vignetteShader);
            composer.AddPass(effectCopy);
        }
        public override void Render()
        {
            if (!imGuiManager.ImWantMouse) controls.Enabled=true;
            else controls.Enabled = false;
            controls.Update();
            earth.Rotation.Y += 0.001f;
            pivot.Rotation.Y += -0.0003f;
            composer.Render();
            ShowControls();
        }
        public override void ShowControls()
        {
            ImGui.NewFrame();
            ImGui.Begin("Controls");
            AddBleachBypassShader(bleachByPassFilter);
            AddBrightnessContrastShader(brightnessContrastShader);
            AddColorifyShaderControls(colorifyShader);
            AddColorCorrectionShaderControls(colorCorrectionShader);
            AddFreiChenShaderControl(freiChenShader);
            AddGammaCorrectionShaderControl(gammaCorrectionShader);
            AddHueSaturationShaderControl(hueSaturationShader);
            AddKaleidoShaderControl(kaleidoShader);
            AddLuminosityHighPassControl(luminosityHighPassShader);
            AddLuminosityShaderControl(luminosityShader);
            AddMirrorShaderControl(mirrorShader);
            AddPixelShaderControl(pixelShader);
            AddRGBShiftShaderControl(rgbShiftShader);
            AddSepiaShader(sepiaShader);
            AddSobelOperatorShaderControl(sobelOperatorShader);
            AddVignetteShaderControl(vignetteShader);
            ImGui.End();
            ImGui.Render();
            imGuiManager.ImGui_ImplOpenGL3_RenderDrawData(ImGui.GetDrawData());
        }

        private void AddRGBShiftShaderControl(ShaderPass pass)
        {
            if (ImGui.TreeNode("rgbShift"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if (ImGui.SliderFloat("angle", ref rgbAngle, 0.0f, 0.628f))
                {
                    (pass.uniforms["angle"] as Uniform)["value"] = rgbAngle;
                }
                if (ImGui.SliderFloat("amount", ref rgbAmount, 0.0f, 0.5f))
                {
                    (pass.uniforms["amount"] as Uniform)["value"] = rgbAmount;
                }
                ImGui.TreePop();
            }
        }

        private void AddLuminosityHighPassControl(ShaderPass pass)
        {
            System.Numerics.Vector3 color1 = new System.Numerics.Vector3(color.R, color.G, color.B);
            if (ImGui.TreeNode("LuminosityHighPass"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if (ImGui.ColorPicker3("defaultColor", ref color1))
                {
                    defaultColor.SetRGB(color1.X, color1.Y, color1.Z);
                    (pass.uniforms["defaultColor"] as Uniform)["value"] = defaultColor;
                }
                if (ImGui.SliderFloat("luminosityThreshold", ref luminosityThreshold, 0.0f, 2.0f))
                {
                    (pass.uniforms["luminosityThreshold"] as Uniform)["value"] = luminosityThreshold;
                }
                if (ImGui.SliderFloat("smoothWidth", ref smoothWidth, 0.0f, 2.0f))
                {
                    (pass.uniforms["smoothWidth"] as Uniform)["value"] = smoothWidth;
                }

                if (ImGui.SliderFloat("defaultOpacity", ref defaultOpacity, 0.0f, 1.0f))
                {
                    (pass.uniforms["defaultOpacity"] as Uniform)["value"] = defaultOpacity;
                }

                ImGui.TreePop();
            }
        }

        private void AddLuminosityShaderControl(ShaderPass pass)
        {
            if (ImGui.TreeNode("Luminosity"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                ImGui.TreePop();
            }
        }

        private void AddMirrorShaderControl(ShaderPass pass)
        {
            if (ImGui.TreeNode("Mirror"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if(ImGui.SliderInt("side",ref side, 0, 3))
                {
                    (pass.uniforms["side"] as Uniform)["value"] = side;
                }
                ImGui.TreePop();
            }
        }

        private void AddPixelShaderControl(ShaderPass pass)
        {
            if (ImGui.TreeNode("Pixel"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if (ImGui.SliderFloat("pixelSize", ref pixelSize, 0, 10))
                {
                    (pass.uniforms["pixelSize"] as Uniform)["value"] = pixelSize;
                }
                ImGui.Text("resolution");
                bool changed = false;
                if (ImGui.SliderFloat("x", ref pixelResolution.X, 2, 512))
                {
                    changed = true;
                }
                if (ImGui.SliderFloat("y", ref pixelResolution.Y, 2, 512))
                {
                    changed = true;
                }
                if (changed)
                {
                    (pass.uniforms["resolution"] as Uniform)["value"] = pixelResolution;
                }
                ImGui.TreePop();
            }
        }

        private void AddSobelOperatorShaderControl(ShaderPass pass)
        {
            if (ImGui.TreeNode("sobelOperator"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);               
                ImGui.Text("resolution");
                bool changed = false;
                if (ImGui.SliderFloat("x", ref sobelResolution.X, 2, 512))
                {
                    changed = true;
                }
                if (ImGui.SliderFloat("y", ref sobelResolution.Y, 2, 512))
                {
                    changed = true;
                }
                if (changed)
                {
                    (pass.uniforms["resolution"] as Uniform)["value"] = sobelResolution;
                }
                ImGui.TreePop();
            }
        }

        private void AddVignetteShaderControl(ShaderPass pass)
        {
            if (ImGui.TreeNode("Vignette"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if (ImGui.SliderFloat("offset", ref vignetteOffset, 0.0f, 10.0f))
                {
                    (pass.uniforms["offset"] as Uniform)["value"] = vignetteOffset;
                }
                if (ImGui.SliderFloat("darkness", ref darkness, 0.0f, 10.0f))
                {
                    (pass.uniforms["darkness"] as Uniform)["value"] = darkness;
                }
                ImGui.TreePop();
            }
        }

        private void AddKaleidoShaderControl(ShaderPass pass)
        {
            if (ImGui.TreeNode("Kaleido"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if (ImGui.SliderFloat("sides", ref sides, 0.0f, 20.0f))
                {
                    (pass.uniforms["sides"] as Uniform)["value"] = sides;
                }
                if (ImGui.SliderFloat("angle", ref angle, 0.0f, 6.28f))
                {
                    (pass.uniforms["angle"] as Uniform)["value"] = angle;
                }
                ImGui.TreePop();
            }
        }

        private void AddBleachBypassShader(ShaderPass pass)
        {

            if (ImGui.TreeNode("BleachBypass"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if (ImGui.SliderFloat("opacity", ref opacity, 0.0f, 2.0f))
                {
                    (pass.uniforms["opacity"] as Uniform)["value"] = opacity;
                }
                ImGui.TreePop();
            }
        }
        private void AddBrightnessContrastShader(ShaderPass pass)
        {
            if (ImGui.TreeNode("BrightnessContrast"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if (ImGui.SliderFloat("brightness", ref brightness, 0.0f, 1.0f))
                {
                    (pass.uniforms["brightness"] as Uniform)["value"] = brightness;
                }
                if (ImGui.SliderFloat("contrast", ref contrast, 0.0f, 1.0f))
                {
                    (pass.uniforms["contrast"] as Uniform)["value"] = contrast;
                }
                ImGui.TreePop();
            }
        }
        private void AddColorifyShaderControls(ShaderPass pass)
        {
            System.Numerics.Vector3 color1 = new System.Numerics.Vector3(color.R, color.G, color.B);
            if (ImGui.TreeNode("Colrify"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if (ImGui.ColorPicker3("color", ref color1))
                {
                    color.SetRGB(color1.X, color1.Y, color1.Z);
                    (pass.uniforms["color"] as Uniform)["value"] = color;
                }

                ImGui.TreePop();
            }
        }
        private void AddColorCorrectionShaderControls(ShaderPass pass)
        {
            System.Numerics.Vector3 color1 = new System.Numerics.Vector3(color.R, color.G, color.B);
            if (ImGui.TreeNode("ColorCorrection"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if (ImGui.TreeNode("powRGB"))
                {
                    bool changed = false;
                    if (ImGui.SliderFloat("x", ref powRGB.X, 0.0f, 5.0f))
                        changed = true;
                    if (ImGui.SliderFloat("y", ref powRGB.Y, 0.0f, 5.0f))
                        changed = true;
                    if (ImGui.SliderFloat("z", ref powRGB.Z, 0.0f, 5.0f))
                        changed = true;

                    if (changed)
                        (pass.uniforms["powRGB"] as Uniform)["value"] = powRGB;
                    ImGui.TreePop();
                }
                if (ImGui.TreeNode("mulRGB"))
                {
                    bool changed = false;
                    if (ImGui.SliderFloat("x", ref mulRGB.X, 0.0f, 5.0f))
                        changed = true;
                    if (ImGui.SliderFloat("y", ref mulRGB.Y, 0.0f, 5.0f))
                        changed = true;
                    if (ImGui.SliderFloat("z", ref mulRGB.Z, 0.0f, 5.0f))
                        changed = true;

                    if (changed)
                        (pass.uniforms["mulRGB"] as Uniform)["value"] = mulRGB;
                    ImGui.TreePop();
                }
                if (ImGui.TreeNode("addRGB"))
                {
                    bool changed = false;
                    if (ImGui.SliderFloat("x", ref addRGB.X, 0.0f, 1.0f))
                        changed = true;
                    if (ImGui.SliderFloat("y", ref addRGB.Y, 0.0f, 1.0f))
                        changed = true;
                    if (ImGui.SliderFloat("z", ref addRGB.Z, 0.0f, 1.0f))
                        changed = true;

                    if (changed)
                        (pass.uniforms["addRGB"] as Uniform)["value"] = addRGB;
                    ImGui.TreePop();
                }


                ImGui.TreePop();
            }
        }
        private void AddGammaCorrectionShaderControl(ShaderPass pass)
        {
            if (ImGui.TreeNode("GammaCorrection"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                ImGui.TreePop();
            }
        }
        private void AddFreiChenShaderControl(ShaderPass pass)
        {
            if (ImGui.TreeNode("Freichen"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if (ImGui.TreeNode("aspect"))
                {
                    bool changed = false;
                    if (ImGui.SliderFloat("x", ref aspect.X, 128.0f, 1024.0f))
                        changed = true;
                    if (ImGui.SliderFloat("y", ref aspect.Y, 128.0f, 1024.0f))
                        changed = true;
                   

                    if (changed)
                        (pass.uniforms["aspect"] as Uniform)["value"] = aspect;
                    ImGui.TreePop();
                }
                ImGui.TreePop();
            }
        }
        private void AddHueSaturationShaderControl(ShaderPass pass)
        {
            if (ImGui.TreeNode("HueSaturation"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if (ImGui.SliderFloat("hue", ref hue, -1.0f, 1.0f))
                {
                    (pass.uniforms["hue"] as Uniform)["value"] = hue;
                }
                if (ImGui.SliderFloat("saturation", ref saturation, -1.0f, 1.0f))
                {
                    (pass.uniforms["saturation"] as Uniform)["value"] = saturation;
                }
                ImGui.TreePop();
            }
        }
        private void AddSepiaShader(ShaderPass pass)
        {
            if (ImGui.TreeNode("sepia"))
            {
                ImGui.Checkbox("enabled", ref pass.Enabled);
                if (ImGui.SliderFloat("amount", ref amount, 0.0f, 10.0f))
                {
                    (pass.uniforms["amount"] as Uniform)["value"] = amount;
                }
                ImGui.TreePop();
            }
        }
        public override void Resize(System.Drawing.Size clientSize)
        {
            base.Resize(clientSize);
            camera.Aspect = this.glControl.AspectRatio;
            camera.UpdateProjectionMatrix();
        }
    }
}
