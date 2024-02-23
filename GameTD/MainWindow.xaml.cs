using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GameTD
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer enemyGenerationTimer;
        private int hearts = 10;
        private int coins = 100;
        private int towerCost = 100;
        private int score = 0;
        private List<int> highScore = new List<int>();
        private readonly List<Tank> tanks = new List<Tank>();
        private readonly List<Tower> towers = new List<Tower>();
        private string status = "New Game";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartGame()
        {
            InitializeTimer();
            hearts = 10;
            LifeText.Text = string.Format("{0} / 10 жизней", hearts);
            coins = 100;
            CoinText.Text = string.Format("{0} монет", coins);
            towerCost = 100;
            TowerCostText.Text = string.Format("Следующая башня стоит: {0}", towerCost);
            score = 0;
            ScoreText.Text = string.Format("Ваш счёт: {0}", score);
        }

        private void StopGame()
        {
            enemyGenerationTimer.Stop();
            status = "New Game";
            StatusText.Text = string.Format("Вы проиграли. Ваш счёт: {0}. Нажмите ЛКМ, чтобы начать снова.", score);
            foreach (var tank in tanks)
            {
                GameCanvas.Children.Remove(tank);
                try
                {
                    var sb = (Storyboard)tank.FindResource("Ride");
                    sb.Stop();
                    sb.Remove();
                }
                catch { }
            }
            foreach (var tower in towers)
            {
                GameCanvas.Children.Remove(tower);
                try
                {
                    var sb = (Storyboard)tower.FindResource("Charge");
                    sb.Stop();
                    sb.Remove();
                }
                catch { }
            }
            tanks.Clear();
            towers.Clear();
        }

        private void AddScore(int add)
        {
            score += add;
            ScoreText.Text = string.Format("Ваш счёт: {0}", score);
        }

        private void AddCoins(int add)
        {
            coins += add;
            CoinText.Text = string.Format("{0} монет", coins);
        }

        private void NextTowerCost()
        {
            towerCost = (int)Math.Round(towerCost * 1.15);
            TowerCostText.Text = string.Format("Следующая башня стоит: {0}", towerCost);
        }

        private void SubstractLife()
        {
            --hearts;
            LifeText.Text = string.Format("{0} / 10 жизней", hearts);
            if (hearts <= 0)
            {
                AddHighscore();
                StopGame();
            }
        }

        private void AddHighscore()
        {
            highScore.Add(score);
            highScore.Sort((x, y) => y.CompareTo(x));
            HighscoreFirstPlaceText.Text = string.Format("1. {0}", highScore.Count > 0 ? highScore[0] : 0);
            HighscoreSecondPlaceText.Text = string.Format("2. {0}", highScore.Count > 1 ? highScore[1] : 0);
            HighscoreThirdPlaceText.Text = string.Format("3. {0}", highScore.Count > 2 ? highScore[2] : 0);
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(null);
            if (coins >= towerCost && IsEmptyPosition(pos) && status == "Playing")
            {
                AddTower(pos);
                NextTowerCost();
            }
        }

        private bool IsEmptyPosition(Point point)
        {
            for (int i = 0; i < towers.Count; ++i)
            {
                var tower = towers[i];
                if (point.X > Canvas.GetLeft(tower) - 10
                    && point.Y > Canvas.GetTop(tower) - 25
                    && point.X < Canvas.GetLeft(tower) + tower.ActualWidth + 50
                    && point.Y < Canvas.GetTop(tower) + tower.ActualHeight + 25)
                {
                    return false;
                }
            }
            if (point.X > Canvas.GetLeft(road1) - 15
                && point.Y > Canvas.GetTop(road1) - 30
                && point.X < Canvas.GetLeft(road1) + road1.ActualWidth + 45
                && point.Y < Canvas.GetTop(road1) + road1.ActualHeight + 30)
            {
                return false;
            }
            if (point.X > Canvas.GetLeft(road2) - 15
                && point.Y > Canvas.GetTop(road2) - 30
                && point.X < Canvas.GetLeft(road2) + road2.ActualWidth + 45
                && point.Y < Canvas.GetTop(road2) + road2.ActualHeight + 30)
            {
                return false;
            }
            if (point.X > Canvas.GetLeft(road3) - 15
                && point.Y > Canvas.GetTop(road3) - 30
                && point.X < Canvas.GetLeft(road3) + road3.ActualWidth + 45
                && point.Y < Canvas.GetTop(road3) + road3.ActualHeight + 30)
            {
                return false;
            }
            return true;
        }

        private void InitializeTimer()
        {
            enemyGenerationTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 4)
            };
            enemyGenerationTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            EnemyTimerText.Text = string.Format("Периодичность врагов: {0}", enemyGenerationTimer.Interval.TotalSeconds);
            enemyGenerationTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            AddTank();
            enemyGenerationTimer.Stop();
            if (enemyGenerationTimer.Interval.TotalSeconds > 2)
            {
                enemyGenerationTimer.Interval = enemyGenerationTimer.Interval.Subtract(new TimeSpan(500000));
            }
            else if (enemyGenerationTimer.Interval.TotalSeconds > 1)
            {
                enemyGenerationTimer.Interval = enemyGenerationTimer.Interval.Subtract(new TimeSpan(250000));
            }
            else if (enemyGenerationTimer.Interval.TotalMilliseconds > 150)
            {
                enemyGenerationTimer.Interval = enemyGenerationTimer.Interval.Subtract(new TimeSpan(100000));
            }
            EnemyTimerText.Text = string.Format("Периодичность врагов: {0}", enemyGenerationTimer.Interval.TotalSeconds);
            enemyGenerationTimer.Start();
        }

        private void AddTank()
        {
            var tank = new Tank();
            Canvas.SetLeft(tank, 900);
            Canvas.SetTop(tank, 120 + new Random().Next(-10, 10));
            GameCanvas.Children.Add(tank);
            tanks.Add(tank);
            StartTankAnimation(tank);
        }

        private void AddTower(Point point)
        {
            var tower = new Tower();
            Canvas.SetLeft(tower, point.X - 50);
            Canvas.SetTop(tower, point.Y - 30);
            GameCanvas.Children.Add(tower);
            var charge = (Storyboard)tower.FindResource("Charge");
            charge.Completed += (sender, e) => SpawnBullet(sender, e, tower, charge);
            charge.Begin();
            towers.Add(tower);
            AddScore(10);
            AddCoins(-towerCost);
        }

        private void SpawnBullet(object _s, EventArgs _e, Tower tower, Storyboard charge)
        {
            if (!towers.Contains(tower)) return;
            var bullet = new Bullet();
            var tank = GetNearestTankInRadius(tower);
            if (tank != null)
            {
                Canvas.SetLeft(bullet, Canvas.GetLeft(tower));
                Canvas.SetTop(bullet, Canvas.GetTop(tower));
                GameCanvas.Children.Add(bullet);
                StartBulletAnimation(bullet, tank);
            }
            charge.Begin();
            AddScore(1);
        }

        private void DestroyTank(object sender, EventArgs _e, Bullet bullet, Tank tank)
        {
            var clockGroup = (ClockGroup)sender;
            var storyboard = (Storyboard)clockGroup.Timeline;
            storyboard.Stop();
            storyboard.Remove();
            try
            {
                var ride = (Storyboard)tank.FindResource("Ride");
                ride.Stop();
                ride.Remove();
            } catch
            {

            }
            GameCanvas.Children.Remove(tank);
            GameCanvas.Children.Remove(bullet);
            tanks.Remove(tank);
            AddScore(100);
            AddCoins(15);
        }

        private Tank GetNearestTankInRadius(Tower tower)
        {
            if (tanks.Count <= 0)
            {
                return null;
            }
            var nearestTank = tanks[0];
            for (int i = 1; i < tanks.Count; ++i)
            {
                var tankToCompare = tanks[i];
                var currentDistance = GetDistanceBetween(nearestTank, tower);
                var compareDistance = GetDistanceBetween(tankToCompare, tower);
                if (currentDistance > compareDistance)
                {
                    nearestTank = tankToCompare;
                }
            }
            if (GetDistanceBetween(nearestTank, tower) > tower.radius)
            {
                return null;
            }
            return nearestTank;
        }

        private double GetDistanceBetween(UIElement first, UIElement second)
        {
            double firstX = Canvas.GetLeft(first);
            double secondX = Canvas.GetLeft(second);
            double firstY = Canvas.GetTop(first);
            double secondY = Canvas.GetTop(second);
            double distance = Math.Sqrt(Math.Pow(firstX - secondX, 2) + Math.Pow(firstY - secondY, 2));
            return distance;
        }

        private void TankWayCoompleted(object _sender, EventArgs _e, Tank tank)
        {
            if (!tanks.Contains(tank)) return;
            SubstractLife();
            tanks.Remove(tank);
            GameCanvas.Children.Remove(tank);
        }

        private void StartTankAnimation(Tank tank)
        {
            var storyboard = new Storyboard
            {
                Name = "Ride",
            };
            var firstX = Canvas.GetLeft(tank);
            var secondX = 480 + new Random().Next(-10, 10);
            var thirdX = -50;
            var firstY = Canvas.GetTop(tank);
            var secondY = 640 + new Random().Next(-10, 10);
            var animationFirstPath = new DoubleAnimation
            {
                From = firstX,
                To = secondX,
                Duration = TimeSpan.FromSeconds(5)
            };
            Storyboard.SetTarget(animationFirstPath, tank);
            Storyboard.SetTargetProperty(animationFirstPath, new PropertyPath(Canvas.LeftProperty));
            storyboard.Children.Add(animationFirstPath);
            var animationSecondPath = new DoubleAnimation
            {
                From = firstY,
                To = secondY,
                BeginTime = TimeSpan.FromSeconds(5),
                Duration = TimeSpan.FromSeconds(5)
            };
            Storyboard.SetTarget(animationSecondPath, tank);
            Storyboard.SetTargetProperty(animationSecondPath, new PropertyPath(Canvas.TopProperty));
            storyboard.Children.Add(animationSecondPath);
            var animationRotate = new DoubleAnimation
            {
                From = 270,
                To = 180,
                BeginTime = TimeSpan.FromSeconds(4.9),
                Duration = TimeSpan.FromSeconds(0.35)
            };
            Storyboard.SetTarget(animationRotate, tank);
            Storyboard.SetTargetProperty(animationRotate, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            storyboard.Children.Add(animationRotate);
            var animationThirdPath = new DoubleAnimation
            {
                From = secondX,
                To = thirdX,
                BeginTime = TimeSpan.FromSeconds(10),
                Duration = TimeSpan.FromSeconds(5)
            };
            Storyboard.SetTarget(animationThirdPath, tank);
            Storyboard.SetTargetProperty(animationThirdPath, new PropertyPath(Canvas.LeftProperty));
            storyboard.Children.Add(animationThirdPath);
            var animationRotateTwo = new DoubleAnimation
            {
                From = 180,
                To = 270,
                BeginTime = TimeSpan.FromSeconds(9.9),
                Duration = TimeSpan.FromSeconds(0.35)
            };
            Storyboard.SetTarget(animationRotateTwo, tank);
            Storyboard.SetTargetProperty(animationRotateTwo, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            storyboard.Children.Add(animationRotateTwo);
            storyboard.Completed += (sender, e) => TankWayCoompleted(sender, e, tank);
            storyboard.Begin();
        }

        private void StartBulletAnimation(Bullet bullet, Tank tank)
        {
            var storyboard = new Storyboard
            {
                Name = "Thrown"
            };
            var animationX = new DoubleAnimation
            {
                From = Canvas.GetLeft(bullet) + 20,
                To = Canvas.GetLeft(tank) - 5,
                Duration = TimeSpan.FromMilliseconds(175)
            };
            Storyboard.SetTarget(animationX, bullet);
            Storyboard.SetTargetProperty(animationX, new PropertyPath(Canvas.LeftProperty));
            storyboard.Children.Add(animationX);
            var animationY = new DoubleAnimation
            {
                From = Canvas.GetTop(bullet) + 20,
                To = Canvas.GetTop(tank) - 5,
                Duration = TimeSpan.FromMilliseconds(175)
            };
            Storyboard.SetTarget(animationY, bullet);
            Storyboard.SetTargetProperty(animationY, new PropertyPath(Canvas.TopProperty));
            storyboard.Children.Add(animationY);
            storyboard.Completed += (sender, e) => DestroyTank(sender, e, bullet, tank);
            storyboard.Begin();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (status == "New Game")
            {
                StartGame();
                StatusText.Text = "";
                status = "Playing";
            }
        }
    }
}
