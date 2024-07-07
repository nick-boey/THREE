﻿/**
 * This WinForms project  and Example templates were created by referring to Three.cs( (https://github.com/lathoub/three.cs).  
 * */
using ImGuiNET;
using OpenTK.Graphics.ES30;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using THREEExample;
using THREEExample.ThreeImGui;

namespace FormsDemo
{
    public partial class Form1 : Form
    {
        private Example? currentExample;
        private ImGuiManager imGuiManager;
        //private int SleepTime = 5;
        private System.Windows.Forms.Timer _timer;
        private int timeInterval = 10;
        public Form1()
        {
            InitializeComponent();
            glControl.MouseWheel += glControl_MouseWheel;
            //this.WindowState = FormWindowState.Maximized;
        }



        //void Application_Idle(object sender, EventArgs e)
        //{
        //    //while (glControl.IsIdle)
        //    {
        //        Render();

        //        Thread.Sleep(SleepTime);
        //    }
        //}
        private void Render()
        {
            this.glControl.MakeCurrent();
            if (null != currentExample)
            {
                currentExample.Render();

                if (currentExample != null && currentExample.AddGuiControlsAction != null)
                {
                    ImGui.NewFrame();
                    currentExample.ShowGUIControls();

                    ImGui.Render();
                    imGuiManager.ImGui_ImplOpenGL3_RenderDrawData(ImGui.GetDrawData());

                    //this.currentExample.renderer.state.currentProgram = -1;
                    //this.currentExample.renderer.bindingStates.currentState = this.currentExample.renderer.bindingStates.defaultState;
                }
            }

            //stats.update();

            this.glControl.SwapBuffers();
        }
        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }
        private void LoadExampleFromAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var attributes = type.GetCustomAttributes(false);

                foreach (var exampleType in attributes)
                {
                    if (exampleType is ExampleAttribute)
                    {
                        var example = exampleType as ExampleAttribute;

                        TreeNode rootItem = null;
                        foreach (var item in treeView1.Nodes)
                        {
                            var header = example.Category.ToString() + " " + string.Format("{0}", example.Subcategory);
                            if ((item as TreeNode).Text.Equals(header))
                            {
                                rootItem = item as TreeNode;
                                break;
                            }
                        }


                        if (rootItem == null)
                        {
                            rootItem = new TreeNode();
                            rootItem.Text = example.Category.ToString() + " " + string.Format("{0}", example.Subcategory);
                            treeView1.Nodes.Add(rootItem);

                        }


                        var treeItem = new TreeNode();
                        treeItem.Text = example.Title;



                        treeItem.Tag = new ExampleInfo(type, example);

                        rootItem.Nodes.Add(treeItem);
                    }
                }
            }
            treeView1.Sort();
            //treeView1.ExpandAll();
            treeView1.SelectedNode = treeView1.Nodes[0];
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                ActivateNode(e.Node);
            }
        }
        void ActivateNode(TreeNode node)
        {
            if (node == null)
                return;

            if (node.Tag == null)
            {
                if (node.IsExpanded)
                    node.Collapse();
                else
                    node.Expand();
            }
            else
            {
                RunSample((ExampleInfo)node.Tag);
            }
        }
        void RunSample(ExampleInfo e)
        {
            if (null != currentExample)
            {
                currentExample.Dispose();

                currentExample = null;

                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                }
                statusStrip1.Tag = statusStrip1.Text = string.Empty;

            }

            //Application.Idle -= Application_Idle;

            currentExample = (Example)Activator.CreateInstance(e.Example);
            if (null != currentExample)
            {
                this.glControl.MakeCurrent();

                currentExample.Load(this.glControl);
                currentExample.imGuiManager = imGuiManager;
                _timer = new System.Windows.Forms.Timer();
                _timer.Interval = timeInterval;
                _timer.Tick += (sender, e) =>
                {
                    Render();
                };
                _timer.Start();

                statusStrip1.Text = e.Example.Name.Replace("_", " - ");
                statusStrip1.Tag = e.Example.Name;


                GL.Viewport(0, 0, glControl.ClientSize.Width, glControl.ClientSize.Height);
                currentExample.OnResize(new ResizeEventArgs(glControl.ClientSize.Width, glControl.ClientSize.Height));

                //Application.Idle += Application_Idle;
            }
        }

        private void glControl_Load(object sender, EventArgs e)
        {
            Type t = typeof(Example);
            LoadExampleFromAssembly(Assembly.GetAssembly(t));

            Text =
                GL.GetString(StringName.Vendor) + " " +
                GL.GetString(StringName.Renderer) + " " +
                GL.GetString(StringName.Version);
#if NET6_0_OR_GREATER
            this.glControl.Profile = OpenTK.Windowing.Common.ContextProfile.Compatability;
#endif
            statusStrip1.Text = string.Empty;

            imGuiManager = new ImGuiManager(glControl);
        }

        private void glControl_Resize(object sender, EventArgs e)
        {

            var control = sender as GLControl;

            if (control.ClientSize.Height == 0)
                control.ClientSize = new Size(control.ClientSize.Width, 1);

            GL.Viewport(0, 0, control.ClientSize.Width, control.ClientSize.Height);

            if (currentExample != null)
                currentExample.OnResize(new ResizeEventArgs(control.ClientSize.Width, control.ClientSize.Height));

        }

        private void glControl_Paint(object sender, PaintEventArgs e)
        {
            this.Render();
        }

        private void glControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentExample != null)
                currentExample.KeyDown((OpenTK.Windowing.GraphicsLibraryFramework.Keys)e.KeyCode);
        }

        private void glControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentExample == null) return;
            if (currentExample.controls == null) return;
            bool lbutton_down = false;
            bool rbutton_down = false;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    lbutton_down = true;
                    break;
                case MouseButtons.Right:
                    rbutton_down = true;
                    break;
            }
            if (lbutton_down || rbutton_down)
            {               
                if (lbutton_down)
                {
                    currentExample.controls.Control_MouseDown(0, e.X, e.Y);
                }
                else
                {
                    currentExample.controls.Control_MouseDown(2, e.X, e.Y);
                }
            }           
        }

        private void glControl_MouseEnter(object sender, EventArgs e)
        {

        }

        private void glControl_MouseHover(object sender, EventArgs e)
        {

        }

        private void glControl_MouseLeave(object sender, EventArgs e)
        {

        }

        private void glControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentExample == null) return;
            if (currentExample.controls == null) return;
            currentExample.controls.Control_MouseMove(e.X, e.Y);
        }

        private void glControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (currentExample == null) return;
            if (currentExample.controls == null) return;
            currentExample.controls.Control_MouseUp();
        }
        private void glControl_MouseWheel(object? sender, MouseEventArgs e)
        {
            if (currentExample == null) return;
            currentExample.controls.Control_MouseWheel(e.Delta);
        }

    }
}
