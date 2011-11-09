using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Research.Kinect.Nui;
using Coding4Fun.Kinect.Wpf;

namespace MobileGestures
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

    public class Hand
    {
        #region Events
        public event ProgressEventHandler SwypeRight;
        public event ProgressEventHandler SwypeLeft;
        public event ProgressEventHandler SwypeUp;
        public event ProgressEventHandler SwypeDown;
        #endregion
        #region Thresholds
        private float GestureRadiusThresh = 0.15f; //ignore movement if beyond this threshold
        private float GestureVertThresh = 0.2f;
        private float GestureHorizThresh = 0.2f;
        private float NearEnd = 0.3f;
        private float FarEnd = 0.8f;
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
                    //SwypeUpComplete();
                }
                else
                {
                    //SwypeUp((Math.Abs(Position.Y - Init.Y) - GestureRadiusThresh) / GestureVertThresh);
                }
            }
            else
            {
                if (Math.Abs(Position.Y - Init.Y) - GestureRadiusThresh >= GestureVertThresh)
                {
                    ControlDisabled = true;
                    VertGest = false;
                    //SwypeDownComplete();
                }
                else
                {
                    //SwypeUp((Math.Abs(Position.Y - Init.Y) - GestureRadiusThresh) / GestureVertThresh);
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
                    //SwypeRightComplete();
                }
                else
                {
                    if (SwypeRight != null)
                    {
                        SwypeRight(this, new ProgressEventArgs((Math.Abs(Position.X - Init.X) - GestureRadiusThresh) / GestureHorizThresh));
                    }
                    //SwypeRight((Math.Abs(Position.X - Init.X) - GestureRadiusThresh) / GestureHorizThresh);
                }
            }
            else
            {
                if (Math.Abs(Position.X - Init.X) - GestureRadiusThresh >= GestureHorizThresh)
                {
                    ControlDisabled = true;
                    HorizGest = false;
                    //SwypeLeftComplete();
                }
                else
                {
                    //SwypeLeft((Math.Abs(Position.X - Init.X) - GestureRadiusThresh) / GestureHorizThresh);
                }
            }
            GestProgress = ((Position.Y - Init.Y) - GestureRadiusThresh) / GestureVertThresh;
        }
        #endregion

        public bool IsActive(Microsoft.Research.Kinect.Nui.Vector CurrentCoords, float ReferencePlaneZ)
        {
            Position = CurrentCoords;
            if ((NearEnd < (ReferencePlaneZ - CurrentCoords.Z)) && ((ReferencePlaneZ - CurrentCoords.Z) < FarEnd))
            {
                if (ControlDisabled)
                    return false;
                else
                {
                    if (!ActiveFlag)
                        Init = CurrentCoords;
                }
                return true;
            }
            else
            {
                ActiveFlag = false;
                ControlDisabled = false;
                CompleteGesture();
                return false;
            }
        }
        public void CompleteGesture()
        {
            if (GestProgress > 0.5f)
            {
                if (HorizGest)
                    ;//GestureCompleteRight message
                if (VertGest)
                    ;//GestureCompleteUp message
                GestProgress = 0.0f;
                HorizGest = VertGest = false;
                return;
            }
            if (GestProgress < -0.5f)
            {
                if (HorizGest)
                    ;//GestureCompleteLeft message
                if (VertGest)
                    ;//GestureCompleteDown message
                GestProgress = 0.0f;
                HorizGest = VertGest = false;
                return;
            }
            //GestureAborted message
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
    public class HandsLogic
    {
        #region Privates
        private Hand RightHand = new Hand();
        private Hand LeftHand = new Hand();

        private bool InDoubleGesture = false;
        private bool RActive = false;
        private bool LActive = false;
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
                //ZoomIn() message
                return;
            }
            if (PinchPrevDistance - Distance(Right, Left) > PinchProgressThresh)
            {
                PinchPrevDistance = Distance(Right, Left);
                //ZoomOut() message
                return;
            }
        }
        #endregion

        public void Iteration(JointsCollection Joints)
        {
            RActive = RightHand.IsActive(Joints[JointID.WristRight].Position, Joints[JointID.ShoulderCenter].Position.Z);
            LActive = LeftHand.IsActive(Joints[JointID.WristLeft].Position, Joints[JointID.ShoulderCenter].Position.Z);
            if (RActive && LActive)
            {
                if (InDoubleGesture)
                {
                    ProceedDoubleGesture(Joints[JointID.WristRight].Position, Joints[JointID.WristLeft].Position);
                }
                else
                {
                    RightHand.CompleteGesture();
                    LeftHand.CompleteGesture();
                    InDoubleGesture = true;
                    InitDoubleGesture(Joints[JointID.WristRight].Position, Joints[JointID.WristLeft].Position);
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
        public event ProgressEventHandler ZoomIn;
        public event ProgressEventHandler ZoomOut;
        #endregion
        public HandsLogic()
        {
            RightHand.SwypeRight += new ProgressEventHandler(RightHand_SwypeRight);
            RightHand.SwypeLeft += new ProgressEventHandler(RightHand_SwypeLeft);
            RightHand.SwypeUp += new ProgressEventHandler(RightHand_SwypeUp);
            RightHand.SwypeDown += new ProgressEventHandler(RightHand_SwypeDown);
            LeftHand.SwypeRight += new ProgressEventHandler(LeftHand_SwypeRight);
            LeftHand.SwypeLeft += new ProgressEventHandler(LeftHand_SwypeLeft);
            LeftHand.SwypeUp += new ProgressEventHandler(LeftHand_SwypeUp);
            LeftHand.SwypeDown += new ProgressEventHandler(LeftHand_SwypeDown);
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
        #endregion
        #endregion
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Hands.RSwypeRight += new ProgressEventHandler(Hands_RSwypeRight);
        }

        void Hands_RSwypeRight(object sender, ProgressEventArgs a)
        {
            throw new NotImplementedException();
        }
        Runtime nui;
        HandsLogic Hands = new HandsLogic();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupKinect();
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

                nui.SkeletonEngine.TransformSmooth = true;
                nui.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(nui_SkeletonFrameReady);
                nui.NuiCamera.ElevationAngle = 15;
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
                                     select s).FirstOrDefault();
            if (skeleton != null)
                Hands.Iteration(skeleton.Joints);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            nui.Uninitialize();
        }
    }
}
