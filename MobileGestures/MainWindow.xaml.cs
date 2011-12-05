﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Research.Kinect.Nui;
using KinectHandSwypes;

namespace MobileGestures
{
    public partial class MainWindow : Window
    {
        Runtime nui;
        HandSwypes Hands = new HandSwypes();

        public MainWindow()
        {
            InitializeComponent();
            Hands.RSwypeRight += new ProgressEventHandler(Hands_RSwypeRight);
            Hands.RSwypeLeft += new ProgressEventHandler(Hands_RSwypeLeft);
            Hands.RSwypeUp += new ProgressEventHandler(Hands_RSwypeUp);
            Hands.RSwypeDown += new ProgressEventHandler(Hands_RSwypeDown);
            Hands.RSwypeDownComplete += new ProgressEventHandler(Hands_RSwypeDownComplete);
            Hands.RSwypeUpComplete += new ProgressEventHandler(Hands_RSwypeUpComplete);
            Hands.RSwypeRightComplete += new ProgressEventHandler(Hands_RSwypeRightComplete);
            Hands.RSwypeLeftComplete += new ProgressEventHandler(Hands_RSwypeLeftComplete);
            Hands.LSwypeRight += new ProgressEventHandler(Hands_LSwypeRight);
            Hands.LSwypeLeft += new ProgressEventHandler(Hands_LSwypeLeft);
            Hands.LSwypeUp += new ProgressEventHandler(Hands_LSwypeUp);
            Hands.LSwypeDown += new ProgressEventHandler(Hands_LSwypeDown);
            Hands.LSwypeDownComplete += new ProgressEventHandler(Hands_LSwypeDownComplete);
            Hands.LSwypeUpComplete += new ProgressEventHandler(Hands_LSwypeUpComplete);
            Hands.LSwypeRightComplete += new ProgressEventHandler(Hands_LSwypeRightComplete);
            Hands.LSwypeLeftComplete += new ProgressEventHandler(Hands_LSwypeLeftComplete);
            Hands.ZoomIn += new ProgressEventHandler(Hands_ZoomIn);
            Hands.ZoomOut += new ProgressEventHandler(Hands_ZoomOut);
            Hands.MouseCoords += new PointEventHandler(Hands_MouseCoords);
        }

        #region EventHandlers

        void Hands_ZoomOut(object sender, ProgressEventArgs a)
        {
            Layout2.Width /= 1.1d;
            Layout2.Height /= 1.1d;
            Canvas.SetLeft(Layout2, (1920.0 - Layout2.Width) / 2.0);
            Canvas.SetTop(Layout2, (1080.0 - Layout2.Height) / 2.0);
        }

        void Hands_MouseCoords(object sender, PointEventArgs a)
        {
            Canvas.SetLeft(kinectButton, a.Point.X);
            Canvas.SetTop(kinectButton, a.Point.Y);
            if (a.Point.X < 80.0d && a.Point.Y < 80.0d)
            {
                Hands.ChangeMode(HandSwypes.RecognitionMode.SWYPE);
                kinectButton.Visibility = Visibility.Hidden;
                exitLogo.Visibility = Visibility.Hidden;
            }
        }

        void Hands_ZoomIn(object sender, ProgressEventArgs a)
        {
            Layout2.Width *= 1.1d;
            Layout2.Height *= 1.1d;
            Canvas.SetLeft(Layout2, (1920.0 - Layout2.Width) / 2.0);
            Canvas.SetTop(Layout2, (1080.0 - Layout2.Height) / 2.0);
        }

        void Hands_RSwypeLeftComplete(object sender, ProgressEventArgs a)
        {
            ImageSource temp = Layout1.Source;
            Layout1.Source = Layout2.Source;
            Layout2.Source = Layout3.Source;
            Layout3.Source = temp;
            Canvas.SetLeft(Layout1, -Layout1.Width);
            Canvas.SetLeft(Layout2, (1920-Layout2.Width)/2);
            Canvas.SetLeft(Layout3, 1920);
        }

        void Hands_RSwypeRightComplete(object sender, ProgressEventArgs a)
        {
            ImageSource temp = Layout2.Source;
            Layout2.Source = Layout1.Source;
            Layout1.Source = Layout3.Source;
            Layout3.Source = temp;
            Canvas.SetLeft(Layout1, -Layout1.Width);
            Canvas.SetLeft(Layout2, (1920 - Layout2.Width) / 2);
            Canvas.SetLeft(Layout3, 1920);
        }

        void Hands_RSwypeUpComplete(object sender, ProgressEventArgs a)
        {
            ImageSource temp = Layout4.Source;
            Layout4.Source = Layout2.Source;
            Layout2.Source = Layout5.Source;
            Layout5.Source = temp;
            Canvas.SetTop(Layout4, -563);
            Canvas.SetTop(Layout2, 259);
            Canvas.SetTop(Layout5, 1080);
        }

        void Hands_RSwypeDownComplete(object sender, ProgressEventArgs a)
        {
            ImageSource temp = Layout5.Source;
            Layout5.Source = Layout2.Source;
            Layout2.Source = Layout4.Source;
            Layout4.Source = temp;
            Canvas.SetTop(Layout4, -563);
            Canvas.SetTop(Layout2, 259);
            Canvas.SetTop(Layout5, 1080);
        }

        void Hands_RSwypeDown(object sender, ProgressEventArgs a)
        {
            ResetScale(Layout2);
            Canvas.SetTop(Layout4, -563 + (563 + 259) * a.Progress);
            Canvas.SetTop(Layout2, 259 + (1080-259) * a.Progress);
            Canvas.SetTop(Layout5, 1080);
        }

        void Hands_RSwypeUp(object sender, ProgressEventArgs a)
        {
            ResetScale(Layout2);
            Canvas.SetTop(Layout4, -563);
            Canvas.SetTop(Layout2, 259 - (563+259) * a.Progress);
            Canvas.SetTop(Layout5, 1080 - (1080 - 259) * a.Progress);
        }

        void Hands_RSwypeLeft(object sender, ProgressEventArgs a)
        {
            ResetScale(Layout2);
            Canvas.SetLeft(Layout1, -Layout1.Width);
            Canvas.SetLeft(Layout2, 460 - (1000+460) * a.Progress);
            Canvas.SetLeft(Layout3, 1920 - (1920-460) * a.Progress);
        }

        void Hands_RSwypeRight(object sender, ProgressEventArgs a)
        {
            ResetScale(Layout2);
            Canvas.SetLeft(Layout1, -1000 +(1000+460)*a.Progress);
            Canvas.SetLeft(Layout2, 460 + (1920-460)*a.Progress);
            Canvas.SetLeft(Layout3, 1920);
        }

        void Hands_LSwypeLeftComplete(object sender, ProgressEventArgs a)
        {
            ImageSource temp = Layout1.Source;
            Layout1.Source = Layout2.Source;
            Layout2.Source = Layout3.Source;
            Layout3.Source = temp;
            Canvas.SetLeft(Layout1, -Layout1.Width);
            Canvas.SetLeft(Layout2, (1920 - Layout2.Width) / 2);
            Canvas.SetLeft(Layout3, 1920);
            Hands.ChangeMode(HandSwypes.RecognitionMode.CURSOR);
            kinectButton.Visibility = Visibility.Visible;
            exitLogo.Visibility = Visibility.Visible;
        }

        void Hands_LSwypeRightComplete(object sender, ProgressEventArgs a)
        {
            ImageSource temp = Layout2.Source;
            Layout2.Source = Layout1.Source;
            Layout1.Source = Layout3.Source;
            Layout3.Source = temp;
            Canvas.SetLeft(Layout1, -Layout1.Width);
            Canvas.SetLeft(Layout2, (1920 - Layout2.Width) / 2);
            Canvas.SetLeft(Layout3, 1920);
        }

        void Hands_LSwypeUpComplete(object sender, ProgressEventArgs a)
        {
            ImageSource temp = Layout4.Source;
            Layout4.Source = Layout2.Source;
            Layout2.Source = Layout5.Source;
            Layout5.Source = temp;
            Canvas.SetTop(Layout4, -563);
            Canvas.SetTop(Layout2, 259);
            Canvas.SetTop(Layout5, 1080);
        }

        void Hands_LSwypeDownComplete(object sender, ProgressEventArgs a)
        {
            ImageSource temp = Layout5.Source;
            Layout5.Source = Layout2.Source;
            Layout2.Source = Layout4.Source;
            Layout4.Source = temp;
            Canvas.SetTop(Layout4, -563);
            Canvas.SetTop(Layout2, 259);
            Canvas.SetTop(Layout5, 1080); 
        }

        void Hands_LSwypeDown(object sender, ProgressEventArgs a)
        {
        }

        void Hands_LSwypeUp(object sender, ProgressEventArgs a)
        {
        }

        void Hands_LSwypeLeft(object sender, ProgressEventArgs a)
        {
        }

        void Hands_LSwypeRight(object sender, ProgressEventArgs a)
        {
        }

        void ResetScale(Image Im)
        {
            if (Canvas.GetLeft(Im) + Im.Width / 2.0 == 1920.0 / 2.0)
            {
                Im.Width = 1000;
                Im.Height = 563;
                Canvas.SetLeft(Im, (1920.0 - Im.Width) / 2.0);
                Canvas.SetTop(Im, (1080.0 - Im.Height) / 2.0);
            }
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupKinect(7);
            Canvas.SetLeft(Layout1,-1000);
            Canvas.SetTop(Layout1, 259);
            Canvas.SetLeft(Layout2, 460);
            Canvas.SetTop(Layout2, 259);
            Canvas.SetLeft(Layout3, 1920);
            Canvas.SetTop(Layout3, 259);
            Canvas.SetLeft(Layout4, 460);
            Canvas.SetTop(Layout4, -563);
            Canvas.SetLeft(Layout5, 460);
            Canvas.SetTop(Layout5, 1080);
        }

        private void SetupKinect(int ElevateAngle)
        {
            if (Runtime.Kinects.Count == 0)
            {
                this.Title = "No Kinect connected";
            }
            else
            {
                //use first Kinect 
                nui = Runtime.Kinects[0];
                nui.Initialize(RuntimeOptions.UseSkeletalTracking);

                //nui.DepthStream.Open(ImageStreamType.Depth, 2, ImageResolution.Resolution320x240, ImageType.DepthAndPlayerIndex);
                //nui.VideoStream.Open(ImageStreamType.Video, 2, ImageResolution.Resolution640x480, ImageType.Color);
                nui.SkeletonEngine.TransformSmooth = true;
                nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
                //nui.DepthFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_DepthFrameReady);
                //nui.VideoFrameReady += new EventHandler<ImageFrameReadyEventArgs>(nui_VideoFrameReady);
                nui.NuiCamera.ElevationAngle = ElevateAngle;
                var parameters = new TransformSmoothParameters
                {
                    Smoothing = 0.3f, //0.75f,
                    Correction = 0.3f, //0.0f,
                    Prediction = 0.0f, //0.0f,
                    JitterRadius = 0.08f, //0.05f,
                    MaxDeviationRadius = 0.04f //0.04f
                };
                nui.SkeletonEngine.SmoothParameters = parameters;
            }
        }
        
        void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            SkeletonFrame allSkeletons = e.SkeletonFrame;
            //get the first tracked skeleton 
            SkeletonData skeleton = (from s in allSkeletons.Skeletons
                                     where s.TrackingState == SkeletonTrackingState.Tracked
                                     orderby s.UserIndex descending
                                     select s).FirstOrDefault();
            if (skeleton != null)
            {
                Hands.Iteration(skeleton.Joints);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            nui.Uninitialize();
        }
    }
}
