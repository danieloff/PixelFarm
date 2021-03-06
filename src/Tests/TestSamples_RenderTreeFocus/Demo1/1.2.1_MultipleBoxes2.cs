﻿
//Apache2, 2014-present, WinterDev

using PixelFarm.Drawing;
using LayoutFarm.UI;
using LayoutFarm.CustomWidgets;
using System.Collections.Generic;

namespace LayoutFarm
{
    [DemoNote("1.2.1 MultipleBoxes")]
    public class Demo_MultipleBoxes2 : App
    {
        readonly Color[] _colors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Magenta, Color.Aqua };
        readonly Size[] _sizes = new Size[] { new Size(20, 20), new Size(10, 10), new Size(30, 20), new Size(10, 25) };
        readonly VerticalAlignment[] _vertAligns = new VerticalAlignment[] { VerticalAlignment.Top, VerticalAlignment.Middle, VerticalAlignment.Bottom };
        readonly RectUIAlignment[] _horAligns = new RectUIAlignment[] { RectUIAlignment.Begin, RectUIAlignment.Middle, RectUIAlignment.End };
        //readonly bool[] _hasSpecificWidths = new bool[] { true, false, true };
        readonly bool[] _hasSpecificWidths = new bool[] { true, true, true };
        readonly bool[] _hasSpecificHeights = new bool[] { true, false, true };

        protected override void OnStart(AppHost host)
        {
            //Test1(host);
            //Test2(host);
            Test3(host);
        }
        void Test3(AppHost host)
        {
            Box hostBox = new Box(250, 600);

            hostBox.HasSpecificWidthAndHeight = false;
            //hostBox.HasSpecificHeight = true;
            hostBox.SetLocation(10, 10);
            hostBox.BackColor = Color.White;
            host.AddChild(hostBox);
            //hostBox.ContentLayoutKind = BoxContentLayoutKind.VerticalStack;
            //hostBox.ContentLayoutKind = BoxContentLayoutKind.HorizontalStack;

            List<Box> temp1 = new List<Box>();
            hostBox.ContentLayoutKind = BoxContentLayoutKind.HorizontalFlow;
            {
                int y = 0;
                hostBox.SuspendLayout();
                hostBox.SuspendGraphicsUpdate();
                for (int i = 0; i < 4; ++i)
                {
                    Box child1 = new Box(20, 20);
                    hostBox.Add(child1);
                    child1.BackColor = _colors[i % _colors.Length];
                    child1.SetLocation(20, y);
                    temp1.Add(child1);
                }
                hostBox.ResumeLayout();
                hostBox.ResumeGraphicsUpdate();
            }

            hostBox.RemoveChild(temp1[0]);
        }
        void Test2(AppHost host)
        {
            Box hostBox = new Box(250, 600);

            hostBox.HasSpecificWidthAndHeight = false;
            //hostBox.HasSpecificHeight = true;
            hostBox.SetLocation(10, 10);
            hostBox.BackColor = Color.White;
            host.AddChild(hostBox);
            //hostBox.ContentLayoutKind = BoxContentLayoutKind.VerticalStack;
            //hostBox.ContentLayoutKind = BoxContentLayoutKind.HorizontalStack;
            hostBox.ContentLayoutKind = BoxContentLayoutKind.HorizontalFlow;
            {
                int y = 0;
                for (int i = 0; i < 10; ++i)
                {
                    Box child1 = new Box(100, 100);
                    child1.SetLocation(20, y);
                    hostBox.Add(child1);
                }
            }

            LayoutUpdateArgs args = new LayoutUpdateArgs();
            args.AvailableWidth = hostBox.Width;
            hostBox.CalculateMinimumSize(args);
            hostBox.CalculateMinimumSize(args); //test

            int calculatedW = hostBox.CalculatedMinWidth;
            int calculatedH = hostBox.CalculatedMinHeight;

            hostBox.SetSize(calculatedW, calculatedH);
            //hostBox.PerformContentLayout(args);

            //int w = hostBox.Width;
            //int h = hostBox.Height;
        }
        void Test1(AppHost host)
        {


            ListBox listBox1 = new ListBox(100, 200);
            listBox1.SetLocation(500, 20);
            host.AddChild(listBox1);


            Box hostBox = new Box(400, 600);
            hostBox.SetLocation(10, 10);
            hostBox.BackColor = Color.White;
            hostBox.ContentLayoutKind = BoxContentLayoutKind.HorizontalStack;
            //hostBox.ContentLayoutKind = BoxContentLayoutKind.VerticalStack;
            //hostBox.ContentLayoutKind = BoxContentLayoutKind.HorizontalFlow;
            host.AddChild(hostBox);


            int boxX = 0;
            int boxY = 0;

            hostBox.SuspendGraphicsUpdate();
            for (int i = 0; i < 10; ++i)
            {
                Size s = _sizes[i % _sizes.Length];
                var box = new Box(s.Width, s.Height);
                box.BackColor = _colors[i % _colors.Length];
                box.VerticalAlignment = _vertAligns[i % _vertAligns.Length];
                box.HorizontalAlignment = _horAligns[i % _horAligns.Length];
                box.HasSpecificWidth = _hasSpecificWidths[i % _hasSpecificWidths.Length];
                box.HasSpecificHeight = _hasSpecificHeights[i % _hasSpecificHeights.Length];

                if (!box.HasSpecificHeight)
                {
                    box.BackColor = KnownColors.Black;
                }

                box.SetMargins(1);
                box.SetLocation(boxX, boxY);
                box.MouseDown += Box_MouseDown;
                hostBox.Add(box);
                boxY += 30;
                boxX += 20;
            } 
            //temp1
            LayoutUpdateArgs updateArgs = new LayoutUpdateArgs();
            hostBox.PerformContentLayout(updateArgs);
            hostBox.ResumeGraphicsUpdate();
            hostBox.InvalidateGraphics();

            host.AddChild(hostBox);
        }
        private void Box_MouseDown(object sender, UIMouseDownEventArgs e)
        {
            if (sender is Box box)
            {
#if DEBUG
                box.CurrentPrimaryRenderElement.dbugBreak = true;
#endif
                box.BackColor = PixelFarm.Drawing.Color.White;
            }
        }
    }
}
