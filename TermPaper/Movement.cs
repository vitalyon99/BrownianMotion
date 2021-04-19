using System;
using System.Drawing;
using System.Windows.Forms;

namespace TermPaper
{
    public class Movement
    {
        #region Properties and fields

        // На цьому обєкті буде відбуватися малювання.
        public PictureBox TargetPictureBox { get; set; }
        private Timer Timer;
        // Розмір поля для руху(int).
        public Size Size { get; set; }
        // Масив із силками на обєкти куль.
        private Ball[] Balls;
        // Площа поля.
        public double Area => Size.Width * Size.Height;
        public double AreaAllBalls { get; set; }
        public event EventHandler<BallTouchedSideArgs> BallTouchedSide;
        public event EventHandler<BallTouchedToBallArgs> BallTouchedToBall;

        #endregion

        public Movement(PictureBox pictureBox, int veryBig, int big, int normal, int small, int verySmall)
        {
            TargetPictureBox = pictureBox;
            TargetPictureBox.MouseClick += PictureBoxClick;
            Size = new Size(TargetPictureBox.Width, TargetPictureBox.Height);
            CreateBalls(veryBig, big, normal, small , verySmall);
            PlaceBalls();
            TargetPictureBox.Paint += TargetPictureBoxPaint;
            TargetPictureBox.Invalidate();
            Timer = new Timer
            {
                Interval = 10
            };
            Timer.Tick += TimerTick;
            BallTouchedSide += BallTouchedSideHandler;
            BallTouchedToBall += BallTouchedToBallHandler;
            Timer.Start();
        }

        #region Methods

        public void CreateBalls(int veryBig, int big, int normal, int small, int verySmall)
        {
            int sum = veryBig + big + normal + small + verySmall;
            Balls = new Ball[sum];
            for (int i = 0; i < sum; i++)
            {
                if (i < verySmall)
                {
                    Balls[i] = new Ball((int)BallSize.VerySmall,
                        Vector.CreateRandomVector((int)BallSpeed.VerySmall))
                    {
                        Color = Brushes.SeaGreen
                    };
                    continue;
                }
                if (i < (verySmall + small))
                {
                    Balls[i] = new Ball((int)BallSize.Small,
                       Vector.CreateRandomVector((int)BallSpeed.Small))
                    {
                        Color = Brushes.SeaGreen
                    };
                    continue;
                }
                if (i < (verySmall + small + normal))
                {
                    Balls[i] = new Ball((int)BallSize.Normal,
                       Vector.CreateRandomVector((int)BallSpeed.Normal))
                    {
                        Color = Brushes.SeaGreen
                    };
                    continue;
                }
                if(i < (verySmall + small + normal + big))
                {
                    Balls[i] = new Ball((int)BallSize.Big,
                       Vector.CreateRandomVector((int)BallSpeed.Big))
                    {
                        Color = Brushes.SeaGreen
                    };
                    continue;
                }
                if (i < sum)
                {
                    Balls[i] = new Ball((int)BallSize.VeryBig,
                       Vector.CreateRandomVector((int)BallSpeed.VeryBig))
                    {
                        Color = Brushes.SeaGreen
                    };                  
                }
            }
        }

        // Розміщує кулі рандомним чином.
        public void PlaceBalls()
        {
            // sumArea - Площа усіх квадратів які описані навколо кульок.
            double sumArea = 0;
            for(int k = 0; k < Balls.Length; k++)
            {
                sumArea += 4 * Math.Pow(Balls[k].Radius, 2); 
            }
            AreaAllBalls = sumArea;
            if(sumArea > Area / 2)
            {
                throw new ArgumentOutOfRangeException(message:"Кульки займають забагато площі",
                    paramName:nameof(Balls));
            }
            Random random = new Random();
            int i = Balls.Length-1;
            while ( i >= 0)
            {
                int radius = (int)Math.Ceiling(Balls[i].Radius);
                Point point = new Point(random.Next(radius , Size.Width - radius ), 
                    random.Next(radius , Size.Height - radius ));
                bool goodLocation = true;
                for (int k = i + 1; k < Balls.Length; k++)
                {
                    if (Ball.IsIntersected(point, radius, Balls[k].Center, Balls[k].Radius))
                    {
                        goodLocation = false;
                    }
                }
                if (goodLocation)
                {
                    Balls[i].Center = point;
                    i--;
                }
            }
        }

        public void Draw(Graphics graphics)
        {
            for(int i = 0; i < Balls.Length; i++)
            {
                graphics.FillEllipse(Balls[i].Color, x: (float)Balls[i].Center.X - (float)Balls[i].Radius,
                    y: (float)Balls[i].Center.Y - (float)Balls[i].Radius, height:(float)Balls[i].Radius*2, 
                    width:(float)Balls[i].Radius*2);
            }
        }

        public void Stop()
        {
            Timer.Stop();
        }

        public void Start()
        {
            Timer.Start();
        }
        // 
        public void Destroy()
        {
            Timer.Stop();
            Timer.Enabled = false;
            TargetPictureBox.Paint -= TargetPictureBoxPaint;
            TargetPictureBox.MouseClick -= PictureBoxClick;
        }

        #endregion

        #region Event Handlers

        private void TargetPictureBoxPaint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            Draw(graphics);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            for(int i = 0; i < Balls.Length; i++)
            {             
                Ball ball = Balls[i];
                ball.Move(Timer.Interval / 1000.0);
                // Чи дотикається кулька до вернього краю.
                if(ball.Center.Y <= ball.Radius)
                {
                    BallTouchedSide(ball, new BallTouchedSideArgs(Sides.Top, ball.Center.Y, 
                        Vector.AngleBetweenVectors(ball.Speed, new Vector(1,0))));
                }
                // Чи дотикається кулька до нижнього краю.
                if (ball.Center.Y >= (Size.Height - ball.Radius))
                {
                    BallTouchedSide(ball, new BallTouchedSideArgs(Sides.Bottom, Size.Height - ball.Center.Y,
                       Vector.AngleBetweenVectors(ball.Speed, new Vector(1, 0))));
                }
                // Чи дотикається кулька до лівого краю.
                if (ball.Center.X <= ball.Radius)
                {
                    BallTouchedSide(ball, new BallTouchedSideArgs(Sides.Left, ball.Center.X,
                        Vector.AngleBetweenVectors(ball.Speed, new Vector(0, 1))));
                }
                // Чи дотикається кулька до правого краю.
                if (ball.Center.X >= (Size.Width - ball.Radius))
                {
                    BallTouchedSide(ball, new BallTouchedSideArgs(Sides.Right, Size.Width - ball.Center.X,
                        Vector.AngleBetweenVectors(ball.Speed, new Vector(0, 1))));
                }
                for (int k = 0; k < Balls.Length; k++)
                {
                    if (k == i)
                    {
                        continue;
                    }
                    // Чи дотикається кулька до інших куль.
                    if (ball.IsIntersected(Balls[k]))
                    {
                        BallTouchedToBall(ball, new BallTouchedToBallArgs(ball, Balls[k]));
                    }
                }
            }
            // Перемалювання компоненти.
            TargetPictureBox.Invalidate();
        }

        private void PictureBoxClick(object sender, EventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            if(Timer.Enabled == true)
            {
                Timer.Stop();
            }
            else
            {
                Timer.Start();
            }
        }

        // Коли кулька дотикається до країв тоді змінюється напрям вектора її швидкості.
        private void BallTouchedSideHandler(object sender, BallTouchedSideArgs e)
        {
            Ball ball = (Ball)sender;
            double DistanceToSise = e.DistanceToSide;
            double DistanceForMove = ball.Radius - DistanceToSise + 0.001;
            switch (e.Side)
            {
                case Sides.Bottom:
                    ball.Center.Y -= DistanceForMove;
                    DistanceToSise = Size.Height - ball.Center.Y;
                    //
                    if (DistanceToSise < ball.Radius) throw new Exception();
                    ball.Speed.Coordinates.Y = -ball.Speed.Coordinates.Y;
                    break;
                case Sides.Top:
                    ball.Center.Y += DistanceForMove;
                    DistanceToSise = ball.Center.Y;
                    //
                    if (DistanceToSise < ball.Radius) throw new Exception();
                    ball.Speed.Coordinates.Y = -ball.Speed.Coordinates.Y;
                    break;
                case Sides.Left:
                    ball.Center.X += DistanceForMove;
                    DistanceToSise = ball.Center.X;
                    //
                    if (DistanceToSise < ball.Radius) throw new Exception();
                    ball.Speed.Coordinates.X = -ball.Speed.Coordinates.X;
                    break;
                case Sides.Right:
                    ball.Center.X -= DistanceForMove;
                    DistanceToSise = Size.Width - ball.Center.X;
                    //
                    if (DistanceToSise < ball.Radius) throw new Exception();
                    ball.Speed.Coordinates.X = -ball.Speed.Coordinates.X;
                    break;
            }
        }

        // Розсуває дві кульки які перетнулися.
        private void SetNormalDistanceBetweenTouchedBalls(Ball MainBall, Ball TargetBall)
        {
            Vector X = new Vector(MainBall.Center, TargetBall.Center); 
            double DistanceBetweetBalls = Point.DistanceBetweenPoints(MainBall.Center, TargetBall.Center);
            double DistanceForMove = MainBall.Radius + TargetBall.Radius - DistanceBetweetBalls + 0.001;

            X *= -(1 / X.Length);
            X *= DistanceForMove;

            MainBall.Center.X += X.Coordinates.X;
            MainBall.Center.Y += X.Coordinates.Y;
        }

        // Коли дві кульки стикаються змінюються значення векторів їх швидкостей.
        private void BallTouchedToBallHandler(object sender, BallTouchedToBallArgs e)
        {
            Ball MainBall = e.MainBall;
            Ball TargetBall = e.TargetBall;
            SetNormalDistanceBetweenTouchedBalls(MainBall, TargetBall);

            // Переходимо до нової системи координат - де вісь Х напрямлена через центри двох куль.

            // Координати(у старій(основній) системі координат) напрямного вектора осі 
            // X(нової системи координат).
            Vector X = new Vector(MainBall.Center, TargetBall.Center);
            X *= (1 / X.Length);
            // Координати(у старій(основній) системі координат) напрямного вектора осі 
            // Y(нової системи координат).
            Vector Y = new Vector(X.Coordinates.Y, -X.Coordinates.X);
          
            // Визначення проекцій на нові осі Х та Y векторів швидкостей кульок.
            double MainProjectionX = MainBall.Speed.Length * Vector.CosBetweenVectors(MainBall.Speed, X);
            double MainProjectionY = MainBall.Speed.Length * Vector.CosBetweenVectors(MainBall.Speed, Y);
            double TargetProjectionX = TargetBall.Speed.Length * Vector.CosBetweenVectors(TargetBall.Speed, X);
            double TargetProjectionY = TargetBall.Speed.Length * Vector.CosBetweenVectors(TargetBall.Speed, Y);
            // Перерахування проекцій векторів швидкостей на вісь X(за формулами).
            double SumMass = MainBall.Mass + TargetBall.Mass;
            double MainSpeed = ((MainProjectionX * (MainBall.Mass - TargetBall.Mass) / SumMass)
                + (TargetProjectionX * (2 * TargetBall.Mass) / SumMass));
            double TargetSpeed = MainProjectionX + MainSpeed - TargetProjectionX;

            // Дивна конструкція - але вона фіксить один баг.
            if ((MainProjectionX * TargetProjectionX >= 0) && (MainProjectionX < TargetProjectionX))
            {

            }
            else
            {
                // Повернення до старої системи координат.
                Vector MainVectorProjectionX = X * MainSpeed;
                Vector MainVectorProjectionY = Y * MainProjectionY;
                Vector TargetVectorProjectionX = X * TargetSpeed;
                Vector TargetVectorProjectionY = Y * TargetProjectionY;
                // Перерахування векторів швидкостей кульок.
                MainBall.Speed = MainVectorProjectionX + MainVectorProjectionY;
                TargetBall.Speed = TargetVectorProjectionX + TargetVectorProjectionY;
            }
        }

        #endregion
    }

    public class Ball
    {
        #region Static members

        public static bool IsIntersected(Point point1, double radius1, Point point2, double radius2)
        {
            if (Point.DistanceBetweenPoints(point1, point2) <= (radius1 + radius2))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Properties and fields

        // Розташування кулі.
        public Point Center { get; set; }
        private double radius;
        public double Radius
        {
            private set
            {
                radius = (value < 0) ? -value : value;
            }
            get => radius;
        }
        public Vector Speed { get; set; }
        public double Mass => Radius;
        public Brush Color {get; set;}

        #endregion

        #region Constructors

        public Ball(double radius)
        {
            Radius = radius;
            Speed = new Vector();
        }
        public Ball(double radius, Vector speed)
        {
            Radius = radius;
            Speed = speed;
        }

        #endregion

        public bool IsIntersected(Ball ball)
        {
            if(Point.DistanceBetweenPoints(this.Center, ball.Center) <= (this.Radius + ball.Radius))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Move(double time)
        {
            Center.X += (Speed.Coordinates.X * time);
            Center.Y += (Speed.Coordinates.Y * time);
        }
    }

    public class Point
    {
        #region Operators

        public static double DistanceBetweenPoints(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow(point1.X - point2.X, 2) +
                Math.Pow(point1.Y - point2.Y, 2));
        }

        // Перегрузка оператора - (для віднімання двох точок).
        public static Point operator -(Point left, Point right)
        {
            return new Point(left.X - right.X, left.Y - right.Y);
        }
        // Перегрузка оператора + (для додавання двох точок).
        public static Point operator +(Point left, Point right)
        {
            return new Point(left.X + right.X, left.Y + right.Y);
        }

        #endregion

        // Координати точки.
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class Vector
    {
        #region Operators

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.Coordinates.X + b.Coordinates.X, a.Coordinates.Y + b.Coordinates.Y);
        }
        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.Coordinates.X - b.Coordinates.X, a.Coordinates.Y - b.Coordinates.Y);
        }
        public static double operator *(Vector a, Vector b)
        {
            return ((a.Coordinates.X * b.Coordinates.X) + (a.Coordinates.Y * b.Coordinates.Y));
        }
        public static Vector operator *(Vector vector, double k)
        {
            return new Vector(vector.Coordinates.X * k, vector.Coordinates.Y * k);
        }

        #endregion

        private static Random random = new Random();
        // Створити рандомний вектор із вказаною довжиною.
        public static Vector CreateRandomVector(double length)
        {
            // Генерація рандомного числа у діапазоні від -length до length. 
            double minValue = -length;
            double maxValue = length;
            double randomX = (random.NextDouble() * (maxValue - minValue)) + (minValue);
            // Визначення другої координати вектора відповідно до першої(згенерованої рандомно)
            // (щоб довжина нового вектора відповідала параметру length).
            double Y = Math.Sqrt(Math.Pow(length, 2) - Math.Pow(randomX, 2));
            bool isNegative = Convert.ToBoolean(random.Next(0, 2));
            Y = (isNegative) ? -Y : Y;
            return new Vector(randomX, Y);
        }

        public static double AngleBetweenVectors(Vector vector1, Vector vector2)
        {
            if ((vector1.Length == 0.0) || (vector2.Length == 0.0)) return 0;
            return Math.Acos((vector1 * vector2) / (vector1.Length * vector2.Length));
        }

        public static double CosBetweenVectors(Vector vector1, Vector vector2)
        {
            if ((vector1.Length == 0.0) || (vector2.Length == 0.0)) return 0;
            return ((vector1 * vector2) / (vector1.Length * vector2.Length));
        }

        // Координатa вектора.
        public Point Coordinates { get; private set; }
        // Довжина вектора.
        public double Length => Math.Sqrt(Math.Pow(Coordinates.X, 2) + Math.Pow(Coordinates.Y, 2));

        #region Constructors

        public Vector()
        {

        }

        public Vector(Point coordinates)
        {
            Coordinates = coordinates;
        }
        public Vector(double x, double y)
        {
            Coordinates = new Point(x, y);
        }
        public Vector(Point begin, Point end)
        {
            Coordinates = new TermPaper.Point(end.X - begin.X, end.Y - begin.Y);
        }
        #endregion

        // Повертає вектор на angle градусів.
        public void Rotate(double angle)
        {
            Point point = new Point(Coordinates.X, Coordinates.Y);
            Coordinates.X = (point.X * Math.Cos(angle)) - (point.Y * Math.Sin(angle));
            Coordinates.Y = (point.X * Math.Sin(angle)) + (point.Y * Math.Cos(angle));
            //Coordinates.X = (point.X * Math.Cos(angle)) + (point.Y * Math.Sin(angle));
            //Coordinates.Y = (point.Y * Math.Cos(angle)) - (point.Y * Math.Sin(angle));
        }
    }

    public enum BallSize
    {
        VerySmall = 3,
        Small = 5,
        Normal = 12,
        Big = 20,
        VeryBig = 45
    }

    public enum BallSpeed
    {
        VerySmall = 250,
        Small = 200,
        Normal = 150,
        Big = 100,
        VeryBig = 50
    }

    public enum Sides
    {
        Top,
        Bottom,
        Right,
        Left
    }

    // Обєкт цього класу міститеме інформацію про те до якого краю доторкнулася кулька.
    // (Потрібен для обробника події  BallTouchedSide).
    public class BallTouchedSideArgs : EventArgs
    {
        public Sides Side { get; set; }
        public double DistanceToSide { get; set; }
        public double AngleBetweenSpeedAndSide { get; set; }

        public BallTouchedSideArgs(Sides side, double distanceToSide, double angleBetweenSpeedAndSide)
        {
            Side = side;
            DistanceToSide = distanceToSide;
            AngleBetweenSpeedAndSide = angleBetweenSpeedAndSide;
        }
    }

    // Обєкт цього класу міститеме інформацію про те які дві кульки зіштовхнулися.
    // (Потрібен для обробника події  BallTouchedToBall).
    public class BallTouchedToBallArgs : EventArgs
    {
        public Ball MainBall { get; set; }
        // Кулька на яку наштовхнулася MainBall.
        public Ball TargetBall { get; set; }

        public BallTouchedToBallArgs(Ball mainBall, Ball targetBall)
        {
            MainBall = mainBall;
            TargetBall = targetBall;
        }
    }
}
