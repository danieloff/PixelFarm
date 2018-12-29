﻿//MIT, 2016-present, WinterDev 

using System;
using OpenTK.Graphics.ES20;
namespace PixelFarm.DrawingGL
{
    class RectFillShader : ShaderBase
    {
        ShaderVtxAttrib2f a_position;
        ShaderVtxAttrib4f a_color;
        ShaderUniformMatrix4 u_matrix;
        int _orthoviewVersion = -1;

        public RectFillShader(ShaderSharedResource shareRes)
            : base(shareRes)
        {
            //NOTE: during development, 
            //new shader source may not recompile if you don't clear cache or disable cache feature
            //like...
            //EnableProgramBinaryCache = false; 

            if (!LoadCompiledShader())
            {
                //vertex shader source
                string vs = @"        
                    attribute vec2 a_position;     
                    attribute vec4 a_color;
                    uniform mat4 u_mvpMatrix; 
                    varying vec4 v_color;
 
                    void main()
                    {
                        gl_Position = u_mvpMatrix* vec4(a_position[0],a_position[1],0,1); 
                        v_color= a_color;
                    }
                    ";
                //fragment source
                string fs = @"
                        precision mediump float;
                        varying vec4 v_color; 
                        void main()
                        {
                            gl_FragColor = v_color;
                        }
                    ";
                if (!_shaderProgram.Build(vs, fs))
                {
                    throw new NotSupportedException();
                }

                SaveCompiledShader();
            }


            a_position = _shaderProgram.GetAttrV2f("a_position");
            a_color = _shaderProgram.GetAttrV4f("a_color");
            u_matrix = _shaderProgram.GetUniformMat4("u_mvpMatrix");
        }

        void CheckViewMatrix()
        {
            int version = 0;
            if (_orthoviewVersion != (version = _shareRes.OrthoViewVersion))
            {
                _orthoviewVersion = version;
                u_matrix.SetData(_shareRes.OrthoView.data);
            }
        }
        public void Render(float[] v2fArray, float[] colors)
        {
            SetCurrent();
            CheckViewMatrix();
            //----------------------------------------------------
            a_position.LoadPureV2f(v2fArray);
            a_color.LoadPureV4f(colors);
            GL.DrawArrays(BeginMode.Triangles, 0, v2fArray.Length / 2);
        }
    }
}