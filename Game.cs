using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace ACMX.Games.Pongnect
{
    class Game
    {
        public Rect field { get; private set; }
        public Ball ball { get; private set; }
        public Block paddleLeft { get; private set; }
        public Block paddleRight { get; private set; }
        public int scoreLeft { get; private set; }
        public int scoreRight {get; private set; }

        private int leftUserId = -1, rightUserId = -1;

        private List<Block> bounds = new List<Block>();

        private int BOUND_MARGIN;
        private InteractionStream interactionStream;
        private int PADDLEWIDTH;
        private int PADDLEHEIGHT;
        private const int BALLSPEED = 10;

        private Block leftPaddle;
        private Block rightPaddle;

        private bool pauseBall = true;

        private Timer quitTimer = new Timer(5000);
        private KinectSensor sensor;

        public Game(KinectSensor sensor, Rect field)
        {
            this.field = field;
            BOUND_MARGIN = (int)field.Height/10;
            PADDLEWIDTH = (int)field.Width/100;
            PADDLEHEIGHT = (int)field.Height/6;
            //BALLSPEED = (int)field.Width / 100;
            // TODO: Setup kinect callbacks
            if (sensor != null)
            {
                this.sensor = sensor;
                sensor.SkeletonStream.Enable();
                sensor.DepthStream.Enable();
                sensor.DepthFrameReady += handleDepthFrame;
                sensor.SkeletonFrameReady += handleSkeletonFrame;
                interactionStream = new InteractionStream(sensor, new PongnectInteractionClient());
                interactionStream.InteractionFrameReady += handleInteractionFrame;
                sensor.Start();
            }
            // Setup ball
            newBall();
            // Setup boundaries
            bounds.Add(new Block(field.Width, BOUND_MARGIN, new Point(field.Width / 2, -BOUND_MARGIN/2), false));
            bounds.Add(new Block(field.Width, BOUND_MARGIN, new Point(field.Width / 2, field.Height+BOUND_MARGIN/2), false));
            // TOOD: Setup paddles
            leftPaddle = new Block(PADDLEWIDTH, PADDLEHEIGHT, new Point(PADDLEWIDTH/2, field.Height/2), false);
            rightPaddle = new Block(PADDLEWIDTH, PADDLEHEIGHT, new Point(field.Width - PADDLEWIDTH / 2, field.Height / 2), false);
            // Setup quit callback
            quitTimer.Elapsed += delegate(System.Object o, ElapsedEventArgs eea)
            {
                if (leftUserId == -1 && rightUserId == -1)
                {
                    Application.Current.Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Normal);
                    if (null != sensor)
                    {
                        sensor.Stop();
                    }
                }
            };
            quitTimer.Start();
        }


        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void handleSkeletonFrame(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            // Acquire data from the SkeletonFrameReadyEventArgs...
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                {
                    return;
                }
                skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];

                try
                {
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    var accelerometerReading = sensor.AccelerometerGetCurrentReading();
                    interactionStream.ProcessSkeleton(skeletons, accelerometerReading, skeletonFrame.Timestamp);
                }
                catch (InvalidOperationException)
                {
                    // SkeletonFrame functions may throw when the sensor gets
                    // into a bad state.  Ignore the frame in that case.
                }
            }
        }


        /// <summary>
        /// Event handler for the Kinect sensor's DepthFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void handleDepthFrame(object sender, DepthImageFrameReadyEventArgs e)
        {
            // Acquire data from DepthImageFrameReadyEventArgs and do stuff with it
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                    return;

                try
                {
                    interactionStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                }
                catch (InvalidOperationException)
                {
                    // DepthFrame functions may throw when the sensor gets
                    // into a bad state.  Ignore the frame in that case.
                }
            }
        }

        private void handleInteractionFrame(object sender, InteractionFrameReadyEventArgs e)
        {
            using (InteractionFrame frame = e.OpenInteractionFrame())
            {
                if (frame == null)
                {
                    return;
                }
                UserInfo[] interactionData = new UserInfo[InteractionFrame.UserInfoArrayLength];
                frame.CopyInteractionDataTo(interactionData);
                bool sawLeftUserThisFrame = false;
                bool sawRightuserThisFrame = false;
                foreach (UserInfo u in interactionData)
                {
                    if (u.SkeletonTrackingId != 0)
                    {
                        if (leftUserId == -1)
                        {
                            leftUserId = u.SkeletonTrackingId;
                        }
                        else if (rightUserId == -1 && leftUserId != u.SkeletonTrackingId)
                        {
                            rightUserId = u.SkeletonTrackingId;
                        }
                        
                        if (leftUserId == u.SkeletonTrackingId)
                        {
                            sawLeftUserThisFrame = true;
                            foreach (InteractionHandPointer pointer in u.HandPointers)
                            {
                                if (!pointer.IsPrimaryForUser)
                                {
                                    continue;
                                }
                                double y = pointer.Y;
                                if (y < 0) y = 0;
                                if (y > 1) y = 1;
                                y = y * (field.Height - leftPaddle.height) + leftPaddle.height / 2;
                                leftPaddle.loc = new Point(leftPaddle.loc.X, y);
                            }
                        }
                        else if (rightUserId == u.SkeletonTrackingId)
                        {
                            sawRightuserThisFrame = true;
                            foreach (InteractionHandPointer pointer in u.HandPointers)
                            {
                                if (!pointer.IsPrimaryForUser)
                                {
                                    continue;
                                }
                                double y = pointer.Y;
                                if (y < 0) y = 0;
                                if (y > 1) y = 1;
                                y = y * (field.Height - rightPaddle.height) + rightPaddle.height / 2;
                                rightPaddle.loc = new Point(rightPaddle.loc.X, y);
                            }
                        }
                    }
                    if (!sawLeftUserThisFrame || !sawRightuserThisFrame)
                    {
                        pauseBall = true;
                        if (!sawLeftUserThisFrame)
                        {
                            leftUserId = -1;
                        }
                        if (!sawRightuserThisFrame)
                        {
                            rightUserId = -1;
                        }
                    }
                    if (!sawLeftUserThisFrame && !sawRightuserThisFrame)
                    {
                        quitTimer.Start();
                    }
                    if (sawLeftUserThisFrame && sawRightuserThisFrame)
                    {
                        pauseBall = false;
                        quitTimer.Stop();
                    }
                }
            }

        }

        public void Update()
        {
            if (pauseBall)
            {
                return;
            }
            for (double movement = Math.Sqrt(ball.vel.X * ball.vel.X + ball.vel.Y * ball.vel.Y); movement > 0; movement -= PADDLEWIDTH)
            {
                ball.limitedMove(movement);
                if (ball.loc.X < 0 || ball.loc.X > field.Width)
                {
                    newBall();
                }
                foreach (Block b in bounds) {
                    ball.collideOutside(b.getCollisionBox());
                }
                ball.collideOutside(leftPaddle.getCollisionBox());
                ball.collideOutside(rightPaddle.getCollisionBox());
            }
        }

        public void newBall()
        {
            ball = new Ball(new Point(field.Width / 2, field.Height / 2), new Point(BALLSPEED, -BALLSPEED));
        }


        internal void Draw(DrawingContext dc)
        {
            // Clip the drawing space
            dc.PushClip(new RectangleGeometry(new Rect(0, 0, field.Width, field.Height)));
            // Draw empty white canvas to fill screen
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, field.Width, field.Height));
            // Draw ball
            dc.DrawEllipse(Brushes.White, null, ball.loc, Ball.BALL_RADIUS, Ball.BALL_RADIUS);
            // Draw paddles
            dc.DrawRectangle(Brushes.White, null, leftPaddle.getCollisionBox());
            dc.DrawRectangle(Brushes.White, null, rightPaddle.getCollisionBox());
        }
    }
}
