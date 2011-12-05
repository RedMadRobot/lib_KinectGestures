using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;

namespace KinectHandSwypes
{
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
        private int PrimaryScreenWidth = (int)SystemParameters.PrimaryScreenWidth;
        private int PrimaryScreenHeight = (int)SystemParameters.PrimaryScreenWidth;

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
                if (ZoomOut != null)
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
                    var scaledJoint = Joints[JointID.HandRight].ScaleTo(PrimaryScreenWidth, PrimaryScreenHeight, .3f, .2f);
                    MouseCoords(this, new PointEventArgs(new Point(scaledJoint.Position.X, scaledJoint.Position.Y)));
                    return;
                }
                if ((LeftHand.NearEnd < (Joints[JointID.ShoulderCenter].Position.Z - Joints[JointID.HandLeft].Position.Z)) && ((Joints[JointID.ShoulderCenter].Position.Z - Joints[JointID.HandLeft].Position.Z) < LeftHand.FarEnd))
                {
                    var scaledJoint = Joints[JointID.HandLeft].ScaleTo(PrimaryScreenWidth, PrimaryScreenHeight, .3f, .2f);
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
        public void ChangeMode(RecognitionMode InpMode, int ScreenWidth, int ScreenHeight)
        {
            PrimaryScreenHeight = ScreenHeight;
            PrimaryScreenWidth = ScreenWidth;
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
        public event ProgressEventHandler RPress;
        public event ProgressEventHandler LPress;
        public event ProgressEventHandler RPressComplete;
        public event ProgressEventHandler LPressComplete;
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
            RightHand.Press += new ProgressEventHandler(RightHand_Press);
            RightHand.PressComplete += new ProgressEventHandler(RightHand_PressComplete);
            LeftHand.Press += new ProgressEventHandler(LeftHand_Press);
            LeftHand.PressComplete += new ProgressEventHandler(LeftHand_PressComplete);
        }

        void LeftHand_PressComplete(object sender, ProgressEventArgs a)
        {
            if (LPressComplete != null)
            {
                LPressComplete(this, a);
            }
        }

        void LeftHand_Press(object sender, ProgressEventArgs a)
        {
            if (LPress != null)
            {
                LPress(this, a);
            }
        }

        void RightHand_PressComplete(object sender, ProgressEventArgs a)
        {
            if (RPressComplete != null)
            {
                RPressComplete(this, a);
            }
        }

        void RightHand_Press(object sender, ProgressEventArgs a)
        {
            if (RPress != null)
            {
                RPress(this, a);
            }
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
        public event ProgressEventHandler Press;
        public event ProgressEventHandler PressComplete;
        #endregion
        #region Thresholds
        private float GestureRadiusThresh = 0.15f; //ignore movement if beyond this threshold
        private float GestureVertThresh = 0.3f;
        private float GestureHorizThresh = 0.3f;
        public float NearEnd = 0.4f;
        public float FarEnd = 0.8f;
        private double CommitTime = 2000;
        #endregion
        #region Privates
        private Microsoft.Research.Kinect.Nui.Vector Init; // position of the hand on entering the active zone
        private Microsoft.Research.Kinect.Nui.Vector Position;
        private float GestProgress;

        private bool HorizGest = false;
        private bool VertGest = false;
        private bool ActiveFlag = false;
        private bool ControlDisabled = false; //if the gesture is complete and waiting for leaving the active zone
        private DateTime StartTime;
        private bool Timer;
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
            Timer = false;
            if (Press != null)
                Press(this, new ProgressEventArgs(0.0f));
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
            {
                if (!Timer)
                {
                    StartTime = DateTime.Now;
                    Timer = true;
                }
                else
                {
                    TimeSpan Interval;
                    Interval = DateTime.Now - StartTime;
                    if (Interval.TotalMilliseconds >= CommitTime+500.0f)
                    {
                        ControlDisabled = true;
                        if (PressComplete != null)
                            PressComplete(this, new ProgressEventArgs(1.0f));
                        CompleteGesture();
                    }
                    else
                    {
                        if (Press != null && Interval.TotalMilliseconds>=500)
                            Press(this, new ProgressEventArgs((float)((Interval.TotalMilliseconds-500.0f) / CommitTime)));
                    }
                }
                return;
            }
            if (Timer)
            {
                Timer = false;
                if (Press != null)
                    Press(this, new ProgressEventArgs(0.0f));
            }
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
}
