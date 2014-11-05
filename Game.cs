using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private int playerLeftId, playerRightId;

        private KinectSensor sensor;

        private List<Block> bounds = new List<Block>();

        private const int BOUND_MARGIN = 100;
        private InteractionStream interactionStream;
        private const int PADDLEWIDTH = 10;

        public Game(KinectSensor sensor, Rect field)
        {
            this.field = field;
            // TODO: Setup kinect callbacks
            sensor.SkeletonStream.Enable();
            sensor.DepthStream.Enable();
            interactionStream = new InteractionStream(sensor, new PongnectInteractionClient());
            interactionStream.InteractionFrameReady += handleInteractionFrame;
            // Setup ball
            newBall();
            // Setup boundaries
            bounds.Add(new Block(field.Width, BOUND_MARGIN, new Point(field.Width / 2, -BOUND_MARGIN/2), false));
            bounds.Add(new Block(field.Width, BOUND_MARGIN, new Point(field.Width / 2, field.Height+BOUND_MARGIN/2), false));
            // TOOD: Setup paddles
        }

        private void handleInteractionFrame(object sender, InteractionFrameReadyEventArgs e)
        {
            // TODO: Handle Input

        }

        public void Update()
        {
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
            }
        }

        public void newBall()
        {
            ball = new Ball(new Point(field.Width / 2, field.Height / 2), new Point(10, -10));
        }


        internal void Draw(DrawingContext dc)
        {
            // Clip the drawing space
            dc.PushClip(new RectangleGeometry(new Rect(0, 0, field.Width, field.Height)));
            // Draw empty white canvas to fill screen
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, field.Width, field.Height));
            // Draw ball
            dc.DrawEllipse(Brushes.White, null, ball.loc, Ball.BALL_RADIUS, Ball.BALL_RADIUS);
        }
    }
}
