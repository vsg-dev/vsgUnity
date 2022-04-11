/* <editor-fold desc="MIT License">

Copyright(c) 2022 Christian Schott (InstruNEXT GmbH)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

</editor-fold> */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using vsgUnity.Native;

namespace vsgUnity
{
    public static class LightConverter 
    {
        public static bool CreateLightData(Light light, out LightData lightData) 
        {
            lightData = new LightData();
            // common
            lightData.eyeCoordinateFrame = false;
            lightData.position = NativeUtils.ToNative(Vector3.zero);
            lightData.color = NativeUtils.ToNative(getLightColor(light));
            lightData.intensity = light.intensity;
            Vector3 lightDirection = Vector3.forward;//light.transform.forward; // TODO: local or world-space forward?
            //CoordSytemConverter.Convert(ref lightDirection);
            lightData.direction = NativeUtils.ToNative(lightDirection);

            // type specific
            switch (light.type)
            {
                case LightType.Point:
                    lightData.type = 0;
                    break;
                case LightType.Directional:
                    lightData.type = 1;
                    break;
                case LightType.Spot:
                    lightData.type = 2;
                    lightData.innerAngle = light.innerSpotAngle * .5f;
                    lightData.outerAngle = light.spotAngle * .5f;
                    break;
                default:
                    NativeLog.WriteLine("Unsupported light type: " + light.type);
                    return false;
            }
            return true;
        }

        public static bool CreateAmbientLight(out LightData light)
        {
            switch (RenderSettings.ambientMode) {
                case AmbientMode.Flat:
                    light = CreateAmbientLight(RenderSettings.ambientLight, RenderSettings.ambientIntensity);
                    return true;
                default:
                    light = new LightData();
                    NativeLog.WriteLine("Unsupported ambient light mode: " + RenderSettings.ambientMode);
                    return false;
            }
        }

        public static LightData CreateAmbientLight(Color color, float intensity)
        {
            LightData lightData = new LightData();
            lightData.type = 3;
            lightData.eyeCoordinateFrame = true;
            lightData.position = NativeUtils.ToNative(Vector3.zero);
            lightData.color = NativeUtils.ToNative(color);
            lightData.intensity = intensity;
            return lightData;
        }

        private static Color getLightColor(Light light) 
        {
            if (GraphicsSettings.lightsUseColorTemperature && GraphicsSettings.lightsUseLinearIntensity) {
                return Mathf.CorrelatedColorTemperatureToRGB(light.colorTemperature);
            } else {
                return light.color;
            }
        }
    }
}
