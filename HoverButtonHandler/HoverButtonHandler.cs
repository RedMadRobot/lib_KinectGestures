using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xaml;
using System.Windows;
using Coding4Fun.Kinect.Wpf.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace HoverButtonHandler
{
    public class ButtonHandler
    {
        private HoverButton[] ButtonHeap;
        private HoverButton Progress;
        private DateTime StartTime;
        private bool Active = false;
        private Point pcoord;
        public event HoverEventHandler ButtonPress;
        public void PassButtons(HoverButton[] Buttons, HoverButton ProgressImage)
        {
            ButtonHeap = Buttons;
            Progress = ProgressImage;
            Progress.RenderTransformOrigin = new Point(0.5, 0.5);
        }
        public void Iteration(Point MouseCoord)
        {
            foreach (HoverButton b in ButtonHeap)
            {
                Point bcoord = b.PointToScreen(new Point());
                if ((bcoord.X <= MouseCoord.X) && (MouseCoord.X <= bcoord.X +b.ActualWidth) && (bcoord.Y <= MouseCoord.Y) && (MouseCoord.Y <= bcoord.Y+b.ActualHeight))
                {
                    //cursor is over button b
                    if (b.Opacity == 0.8)
                    //if (Active == true)
                    {
                        TimeSpan Interval;
                        Interval = DateTime.Now - StartTime;
                        if (Interval.TotalMilliseconds >= b.TimeInterval+500.0f)
                        {
                            //button is pressed
                            ButtonPress(this, new HoverEventArgs(b));
                            Progress.Visibility = Visibility.Hidden;
                            b.Opacity = 1.0;
                            //Active = false;
                        }
                        else
                        {
                            //rotate the image
                            if (Interval.TotalMilliseconds >= 500.0f)
                            {
                                TransformGroup tg = new TransformGroup();
                                tg.Children.Add(new RotateTransform(360 * (double)((Interval.TotalMilliseconds - 500.0f) / b.TimeInterval)));
                                tg.Children.Add(new TranslateTransform(MouseCoord.X , MouseCoord.Y));
                                Progress.RenderTransform = tg;
                                //Progress.LayoutTransform = tg;
                                Progress.Visibility = Visibility.Visible;
                            }
                        }
                    }
                    else
                    {
                        b.Opacity = 0.8;
                        //Active = true;
                        StartTime = DateTime.Now;
                    }
                }
                else
                {
                    if (b.Opacity == 0.8)
                    {
                        RotateTransform r = new RotateTransform();
                        r.Angle = 0;
                        Progress.RenderTransform = r;
                        Progress.Visibility = Visibility.Hidden;
                        b.Opacity = 1.0;
                        //Active = false;
                    }
                }
            }
        }
        public class HoverEventArgs : EventArgs
        {
            public HoverEventArgs(HoverButton f)
            {
                msg = f;
            }
            private HoverButton msg;
            public HoverButton Button
            {
                get { return msg; }
            }
        }
        public delegate void HoverEventHandler(object sender, HoverEventArgs a);
    }
}
