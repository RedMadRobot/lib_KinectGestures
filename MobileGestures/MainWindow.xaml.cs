﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;
using Coding4Fun.Kinect.Wpf.Controls;
using MouseKeyboardLibrary;

namespace MobileGestures
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(float f)
        {
            msg = f;
        }
        private float msg;
        public float Progress
        {
            get { return msg; }
        }
    }
    public delegate void ProgressEventHandler(object sender, ProgressEventArgs a);
    public class PointEventArgs : EventArgs
    {
        public PointEventArgs(Point f)
        {
            msg = f;
        }
        private Point msg;
        public Point Point
        {
            get { return msg; }
        }
    }
    public delegate void PointEventHandler(object sender, PointEventArgs a);

    public class Hand
    {
        #region Events
        public event ProgressEventHandler SwypeRight;
        public event ProgressEventHandler SwypeLeft;
        public event ProgressEventHandler SwypeUp;
        public event ProgressEventHandler SwypeDown;
        public event ProgressEventHandler SwypeRightComplete;
        public event ProgressEventHandler SwypeLeftComplete;
        public event ProgressEventHandler SwypeUpComplete;
        public event ProgressEventHandler SwypeDownComplete;
        #endregion
        #region Thresholds
        private float GestureRadiusThresh = 0.15f; //ignore movement if beyond this threshold
        private float GestureVertThresh = 0.3f;
        private float GestureHorizThresh = 0.3f;
        public float NearEnd = 0.4f;
        public float FarEnd = 0.8f;
        #endregion
        #region Privates
        private Microsoft.Research.Kinect.Nui.Vector Init; // position of the hand on entering the active zone
        private Microsoft.Research.Kinect.Nui.Vector Position;
        private float GestProgress;

        private bool HorizGest = false;
        private bool VertGest = false;
        private bool ActiveFlag = false;
        private bool ControlDisabled = false; //if the gesture is complete and waiting for leaving the active zone
        private double Distance() //distance from Init position
        {
            return Math.Sqrt(Math.Pow(Position.X - Init.X, 2.0f) + Math.Pow(Position.Y - Init.Y, 2.0f));
        }

        private void ProceedVertical()
        {
            if ((Position.Y - Init.Y) > 0)
            {
                if (Math.Abs(Position.Y - Init.Y) - GestureRadiusThresh >= GestureVertThresh)
                {
                    ControlDisabled = true;
                    VertGest = false;
                    if (SwypeUpComplete != null)
                        SwypeUpComplete(this, new ProgressEventArgs(1.0f));
                }
                else
                {
                    if (SwypeUp != null)
                        SwypeUp(this, new ProgressEventArgs((Math.Abs(Position.Y - Init.Y) - GestureRadiusThresh) / GestureVertThresh));
                }
            }
            else
            {
                if (Math.Abs(Position.Y - Init.Y) - GestureRadiusThresh >= GestureVertThresh)
                {
                    ControlDisabled = true;
                    VertGest = false;
                    if (SwypeDownComplete != null)
                        SwypeDownComplete(this, new ProgressEventArgs(1.0f));
                }
                else
                {
                    if (SwypeDown != null)
                        SwypeDown(this, new ProgressEventArgs((Math.Abs(Position.Y - Init.Y) - GestureRadiusThresh) / GestureVertThresh));
                }
            }
            GestProgress = ((Position.Y - Init.Y) - GestureRadiusThresh) / GestureVertThresh;
        }
        private void ProceedHorizontal()
        {
            if ((Position.X - Init.X) > 0)
            {
                if (Math.Abs(Position.X - Init.X) - GestureRadiusThresh >= GestureHorizThresh)
                {
                    ControlDisabled = true;
                    HorizGest = false;
                    if (SwypeRightComplete != null)
                        SwypeRightComplete(this, new ProgressEventArgs(1.0f));
                }
                else
                {
                    if (SwypeRight != null)
                        SwypeRight(this, new ProgressEventArgs((Math.Abs(Position.X - Init.X) - GestureRadiusThresh) / GestureHorizThresh));
                }
            }
            else
            {
                if (Math.Abs(Position.X - Init.X) - GestureRadiusThresh >= GestureHorizThresh)
                {
                    ControlDisabled = true;
                    HorizGest = false;
                    if (SwypeLeftComplete != null)
                        SwypeLeftComplete(this, new ProgressEventArgs(1.0f));
                }
                else
                {
                    if (SwypeLeft != null)
                        SwypeLeft(this, new ProgressEventArgs((Math.Abs(Position.X - Init.X) - GestureRadiusThresh) / GestureHorizThresh));
                }
            }
            GestProgress = ((Position.X - Init.X) - GestureRadiusThresh) / GestureHorizThresh;
        }
        #endregion

        public bool IsActive(Microsoft.Research.Kinect.Nui.Vector CurrentCoords, float ReferencePlaneZ)
        {
            Position = CurrentCoords;
            if ((NearEnd < (ReferencePlaneZ - CurrentCoords.Z)) && ((ReferencePlaneZ - CurrentCoords.Z) < FarEnd))
            {
                if (ControlDisabled)
                    return false;
                if (!ActiveFlag)
                    Init = CurrentCoords;
                ActiveFlag = true;
                return true;
            }
            else
            {
                if (!ControlDisabled && ActiveFlag)
                    CompleteGesture();
                ActiveFlag = false;
                ControlDisabled = false;
                return false;
            }
        }
        public void CompleteGesture()
        {
            if (GestProgress > 0.5f)
            {
                if (HorizGest)
                    if (SwypeRightComplete != null)
                        SwypeRightComplete(this, new ProgressEventArgs(1.0f));
                if (VertGest)
                    if (SwypeUpComplete != null)
                        SwypeUpComplete(this, new ProgressEventArgs(1.0f));
                GestProgress = 0.0f;
                HorizGest = VertGest = false;
                return;
            }
            if (GestProgress < -0.5f)
            {
                if (HorizGest)
                    if (SwypeLeftComplete != null)
                        SwypeLeftComplete(this, new ProgressEventArgs(1.0f));
                if (VertGest)
                    if (SwypeDownComplete != null)
                        SwypeDownComplete(this, new ProgressEventArgs(1.0f));
                GestProgress = 0.0f;
                HorizGest = VertGest = false;
                return;
            }
            //rewrite GestureAborted message
            if (HorizGest)
            {
                if (SwypeRight != null)
                    SwypeRight(this, new ProgressEventArgs(0.0f));
                if (SwypeLeft != null)
                    SwypeLeft(this, new ProgressEventArgs(0.0f));
            }
            if (VertGest)
            {
                if (SwypeUp != null)
                    SwypeUp(this, new ProgressEventArgs(0.0f));
                if (SwypeDown != null)
                    SwypeDown(this, new ProgressEventArgs(0.0f));
            }
            GestProgress = 0.0f;
            HorizGest = VertGest = false;

        }
        public void ProceedGesture()
        {
            if (Distance() < GestureRadiusThresh)
                return;
            if (!(HorizGest || VertGest))
            {
                HorizGest = (Math.Abs(Position.X - Init.X)) > (Math.Abs(Position.Y - Init.Y));
                VertGest = !HorizGest;
            }
            if (HorizGest)
                ProceedHorizontal();
            if (VertGest)
                ProceedVertical();
        }
    }

    public class HandSwypes
    {
        #region Privates
        private Hand RightHand = new Hand();
        private Hand LeftHand = new Hand();

        private bool InDoubleGesture = false;
        private bool RActive = false;
        private bool LActive = false;
        public enum RecognitionMode { SWYPE, CURSOR, JOYSTICK }
        private RecognitionMode Mode = RecognitionMode.SWYPE;

        private double PinchProgressThresh = 0.05d;
        private double PinchPrevDistance;
        private double Distance(Microsoft.Research.Kinect.Nui.Vector Right, Microsoft.Research.Kinect.Nui.Vector Left)
        {
            return Math.Sqrt(Math.Pow(Right.X - Left.X, 2.0f) + Math.Pow(Right.Y - Left.Y, 2.0f) + Math.Pow(Right.Z - Left.Z, 2.0f));
        }

        private void InitDoubleGesture(Microsoft.Research.Kinect.Nui.Vector Right, Microsoft.Research.Kinect.Nui.Vector Left)
        {
            PinchPrevDistance = Distance(Right, Left);
            InDoubleGesture = true;
        }
        private void ProceedDoubleGesture(Microsoft.Research.Kinect.Nui.Vector Right, Microsoft.Research.Kinect.Nui.Vector Left)
        {
            if (Distance(Right, Left) - PinchPrevDistance > PinchProgressThresh)
            {
                PinchPrevDistance = Distance(Right, Left);
                if (ZoomIn != null)
                    ZoomIn(this, null);
                return;
            }
            if (PinchPrevDistance - Distance(Right, Left) > PinchProgressThresh)
            {
                PinchPrevDistance = Distance(Right, Left);
                if (ZoomOut!= null)
                    ZoomOut(this, null);
                return;
            }
        }
        #endregion

        public void Iteration(JointsCollection Joints)
        {
            #region Swype
            if (Mode == RecognitionMode.SWYPE)
            {
                RActive = RightHand.IsActive(Joints[JointID.HandRight].Position, Joints[JointID.ShoulderCenter].Position.Z);
                LActive = LeftHand.IsActive(Joints[JointID.HandLeft].Position, Joints[JointID.ShoulderCenter].Position.Z);
                if (RActive && LActive)
                {
                    if (InDoubleGesture)
                    {
                        ProceedDoubleGesture(Joints[JointID.HandRight].Position, Joints[JointID.HandLeft].Position);
                    }
                    else
                    {
                        RightHand.CompleteGesture();
                        LeftHand.CompleteGesture();
                        InDoubleGesture = true;
                        InitDoubleGesture(Joints[JointID.HandRight].Position, Joints[JointID.HandLeft].Position);
                    }
                    return;
                }
                if (RActive && !LActive)
                {
                    RightHand.ProceedGesture();
                    return;
                }
                if (!RActive && LActive)
                {
                    LeftHand.ProceedGesture();
                    return;
                }
            }
            #endregion
            #region Cursor
            if (Mode == RecognitionMode.CURSOR)
            {
                if ((RightHand.NearEnd < (Joints[JointID.ShoulderCenter].Position.Z - Joints[JointID.HandRight].Position.Z)) && ((Joints[JointID.ShoulderCenter].Position.Z - Joints[JointID.HandRight].Position.Z) < RightHand.FarEnd))
                {
                    var scaledJoint = Joints[JointID.HandRight].ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, .3f, .2f);
                    MouseCoords(this, new PointEventArgs(new Point(scaledJoint.Position.X, scaledJoint.Position.Y)));
                    return;
                }
                if ((LeftHand.NearEnd < (Joints[JointID.ShoulderCenter].Position.Z - Joints[JointID.HandLeft].Position.Z)) && ((Joints[JointID.ShoulderCenter].Position.Z - Joints[JointID.HandLeft].Position.Z) < LeftHand.FarEnd))
                {
                    var scaledJoint = Joints[JointID.HandLeft].ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, .3f, .2f);
                    MouseCoords(this, new PointEventArgs(new Point(scaledJoint.Position.X, scaledJoint.Position.Y)));
                    return;
                }
            }
            #endregion
            #region Joystick
            if (Mode == RecognitionMode.JOYSTICK)
            {
            }
            #endregion
        }
        public void ChangeMode(RecognitionMode InpMode)
        {
            Mode = InpMode;
        }
        #region Events
        #region Declarations
        public event ProgressEventHandler RSwypeRight;
        public event ProgressEventHandler RSwypeLeft;
        public event ProgressEventHandler RSwypeUp;
        public event ProgressEventHandler RSwypeDown;
        public event ProgressEventHandler LSwypeRight;
        public event ProgressEventHandler LSwypeLeft;
        public event ProgressEventHandler LSwypeUp;
        public event ProgressEventHandler LSwypeDown;
        public event ProgressEventHandler RSwypeRightComplete;
        public event ProgressEventHandler RSwypeLeftComplete;
        public event ProgressEventHandler RSwypeUpComplete;
        public event ProgressEventHandler RSwypeDownComplete;
        public event ProgressEventHandler LSwypeRightComplete;
        public event ProgressEventHandler LSwypeLeftComplete;
        public event ProgressEventHandler LSwypeUpComplete;
        public event ProgressEventHandler LSwypeDownComplete;
        public event ProgressEventHandler ZoomIn;
        public event ProgressEventHandler ZoomOut;
        public event PointEventHandler MouseCoords;
        #endregion
        public HandSwypes()
        {
            RightHand.SwypeRight += new ProgressEventHandler(RightHand_SwypeRight);
            RightHand.SwypeLeft += new ProgressEventHandler(RightHand_SwypeLeft);
            RightHand.SwypeUp += new ProgressEventHandler(RightHand_SwypeUp);
            RightHand.SwypeDown += new ProgressEventHandler(RightHand_SwypeDown);
            LeftHand.SwypeRight += new ProgressEventHandler(LeftHand_SwypeRight);
            LeftHand.SwypeLeft += new ProgressEventHandler(LeftHand_SwypeLeft);
            LeftHand.SwypeUp += new ProgressEventHandler(LeftHand_SwypeUp);
            LeftHand.SwypeDown += new ProgressEventHandler(LeftHand_SwypeDown);
            RightHand.SwypeRightComplete += new ProgressEventHandler(RightHand_SwypeRightComplete);
            RightHand.SwypeLeftComplete += new ProgressEventHandler(RightHand_SwypeLeftComplete);
            RightHand.SwypeUpComplete += new ProgressEventHandler(RightHand_SwypeUpComplete);
            RightHand.SwypeDownComplete += new ProgressEventHandler(RightHand_SwypeDownComplete);
            LeftHand.SwypeRightComplete += new ProgressEventHandler(LeftHand_SwypeRightComplete);
            LeftHand.SwypeLeftComplete += new ProgressEventHandler(LeftHand_SwypeLeftComplete);
            LeftHand.SwypeUpComplete += new ProgressEventHandler(LeftHand_SwypeUpComplete);
            LeftHand.SwypeDownComplete += new ProgressEventHandler(LeftHand_SwypeDownComplete);
        }
        #region ThrowThroughs
        void LeftHand_SwypeUp(object sender, ProgressEventArgs a)
        {
            if (LSwypeUp != null)
            {
                LSwypeUp(this, a);
            }
        }

        void LeftHand_SwypeDown(object sender, ProgressEventArgs a)
        {
            if (LSwypeDown != null)
            {
                LSwypeDown(this, a);
            }
        }

        void RightHand_SwypeDown(object sender, ProgressEventArgs a)
        {
            if (RSwypeDown != null)
            {
                RSwypeDown(this, a);
            }
        }

        void RightHand_SwypeUp(object sender, ProgressEventArgs a)
        {
            if (RSwypeUp != null)
            {
                RSwypeUp(this, a);
            }
        }

        void LeftHand_SwypeLeft(object sender, ProgressEventArgs a)
        {
            if (LSwypeLeft != null)
            {
                LSwypeLeft(this, a);
            }
        }

        void LeftHand_SwypeRight(object sender, ProgressEventArgs a)
        {
            if (LSwypeRight != null)
            {
                LSwypeRight(this, a);
            }
        }

        void RightHand_SwypeLeft(object sender, ProgressEventArgs a)
        {
            if (RSwypeLeft != null)
            {
                RSwypeLeft(this, a);
            }
        }

        void RightHand_SwypeRight(object sender, ProgressEventArgs a)
        {
            if (RSwypeRight != null)
            {
                RSwypeRight(this, a);
            }
        }

        void LeftHand_SwypeUpComplete(object sender, ProgressEventArgs a)
        {
            if (LSwypeUpComplete != null)
            {
                LSwypeUpComplete(this, a);
            }
        }

        void LeftHand_SwypeDownComplete(object sender, ProgressEventArgs a)
        {
            if (LSwypeDownComplete != null)
            {
                LSwypeDownComplete(this, a);
            }
        }

        void RightHand_SwypeDownComplete(object sender, ProgressEventArgs a)
        {
            if (RSwypeDownComplete != null)
            {
                RSwypeDownComplete(this, a);
            }
        }

        void RightHand_SwypeUpComplete(object sender, ProgressEventArgs a)
        {
            if (RSwypeUpComplete != null)
            {
                RSwypeUpComplete(this, a);
            }
        }

        void LeftHand_SwypeLeftComplete(object sender, ProgressEventArgs a)
        {
            if (LSwypeLeftComplete != null)
            {
                LSwypeLeftComplete(this, a);
            }
        }

        void LeftHand_SwypeRightComplete(object sender, ProgressEventArgs a)
        {
            if (LSwypeRightComplete != null)
            {
                LSwypeRightComplete(this, a);
            }
        }

        void RightHand_SwypeLeftComplete(object sender, ProgressEventArgs a)
        {
            if (RSwypeLeftComplete != null)
            {
                RSwypeLeftComplete(this, a);
            }
        }

        void RightHand_SwypeRightComplete(object sender, ProgressEventArgs a)
        {
            if (RSwypeRightComplete != null)
            {
                RSwypeRightComplete(this, a);
            }
        }
        #endregion
        #endregion
    }

    public partial class MainWindow : Window
    {
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

        Runtime nui;
        HandSwypes Hands = new HandSwypes();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupKinect();
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

        private void SetupKinect()
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
                nui.NuiCamera.ElevationAngle = 7;
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
