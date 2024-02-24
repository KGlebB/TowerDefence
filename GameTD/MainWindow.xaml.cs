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
        private readonly List<int> highScore = new List<int>();
        private readonly List<Tank> tanks = new List<Tank>();
        private readonly List<Tower> towers = new List<Tower>();
        private string status = "New Game";

        public int Hearts
        {
            get { return hearts; }
            set { 
                hearts = value; 
                UpdateLifeText(); 
                CheckLose(); 
            }
        }

        public int Coins
        {
            get { return coins; }
            set { 
                coins = value; 
                UpdateCoinText(); 
            }
        }

        public int TowerCost
        {
            get { return towerCost; }
            set { 
                towerCost = value; 
                UpdateTowerCostText(); 
            }
        }

        public int Score
        {
            get { return score; }
            set { 
                score = value; 
                UpdateScoreText(); 
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateLifeText()
        {
            LifeText.Text = $"{Hearts} / 10 жизней";
        }

        private void UpdateCoinText()
        {
            CoinText.Text = $"{coins} монет";
        }

        private void UpdateScoreText()
        {
            ScoreText.Text = $"Ваш счёт: {score}";
        }

        private void UpdateTowerCostText()
        {
            TowerCostText.Text = $"Следующая башня стоит: {towerCost}";
        }

        private void StartGame()
        {
            InitializeTimer();
            Hearts = 10;
            Coins = 100;
            TowerCost = 100;
            Score = 0;
        }

        private void CheckLose()
        {

            if (Hearts <= 0)
            {
                HandleLose();
            }
        }

        private void HandleLose()
        {
            AddHighscore();
            SetLoseStatus();
            StopGame();
        }

        private void StopGame()
        {
            StopTimers();
            RemoveEntities(tanks, "Ride");
            RemoveEntities(towers, "Charge");
        }

        private void StopTimers()
        {
            enemyGenerationTimer.Stop();
        }

        private void SetLoseStatus()
        {
            status = "New Game";
            StatusText.Text = string.Format("Вы проиграли. Ваш счёт: {0}. Нажмите ЛКМ, чтобы начать снова.", score);
        }

        private void RemoveEntities<T>(List<T> entities, string storyboardResourceKey) where T : UserControl
        {
            foreach (var entity in entities)
            {
                GameCanvas.Children.Remove(entity);
                TryStopAndRemoveStoryboard(entity, storyboardResourceKey);
            }

            entities.Clear();
        }

        private void TryStopAndRemoveStoryboard(UserControl element, string storyboardResourceKey)
        {
            try
            {
                var sb = (Storyboard)element.FindResource(storyboardResourceKey);
                sb.Stop();
                sb.Remove();
            }
            catch { }
        }

        private void NextTowerCost()
        {
            TowerCost = (int)Math.Round(TowerCost * 1.15);
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
            if (Coins >= TowerCost && IsEmptyPosition(pos) && status == "Playing")
            {
                AddTower(pos);
            }
        }

        private bool IsEmptyPosition(Point point)
        {
            foreach (var tower in towers)
            {
                if (IsPointInsideElement(point, tower, 10, 25, 50, 25))
                {
                    return false;
                }
            }
            if (IsPointInsideElement(point, road1, 15, 25, 45, 30) ||
                IsPointInsideElement(point, road2, 15, 25, 45, 30) ||
                IsPointInsideElement(point, road3, 15, 25, 45, 30))
            {
                return false;
            }
            return true;
        }

        private bool IsPointInsideElement(Point point, FrameworkElement element, double leftOffset, double topOffset, double rightOffset, double bottomOffset)
        {
            var leftPos = double.IsNaN(Canvas.GetLeft(element)) ? 0 : Canvas.GetLeft(element);
            var topPos = double.IsNaN(Canvas.GetTop(element)) ? 0 : Canvas.GetTop(element);

            var left = leftPos - leftOffset;
            var top = topPos - topOffset;
            var right = leftPos + element.ActualWidth + rightOffset;
            var bottom = topPos + element.ActualHeight + bottomOffset;

            return point.X > left && point.Y > top && point.X < right && point.Y < bottom;
        }
        
        private void UpdateEnemyTimerText()
        {
            EnemyTimerText.Text = string.Format("Периодичность врагов: {0}", enemyGenerationTimer.Interval.TotalSeconds);
        }

        private void InitializeTimer()
        {
            enemyGenerationTimer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 4)
            };
            enemyGenerationTimer.Tick += new EventHandler(DispatcherTimer_Tick);
            UpdateEnemyTimerText();
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
            UpdateEnemyTimerText();
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
            Score += 10;
            Coins -= TowerCost;
            NextTowerCost();
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
            ++Score;
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
            Score += 100;
            Coins += 15;
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

        private double GetDistanceBetween(UserControl first, UserControl second)
        {
            double firstX = Canvas.GetLeft(first) + first.ActualWidth / 2;
            double secondX = Canvas.GetLeft(second) + second.ActualWidth / 2;
            double firstY = Canvas.GetTop(first) + first.ActualHeight / 2;
            double secondY = Canvas.GetTop(second) + second.ActualWidth / 2;
            double distance = Math.Sqrt(Math.Pow(firstX - secondX, 2) + Math.Pow(firstY - secondY, 2));
            return distance;
        }

        private void TankWayCoompleted(object _sender, EventArgs _e, Tank tank)
        {
            if (!tanks.Contains(tank)) return;
            --Hearts;
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
                BeginTime = TimeSpan.FromSeconds(4.8),
                Duration = TimeSpan.FromSeconds(5.2)
            };
            Storyboard.SetTarget(animationSecondPath, tank);
            Storyboard.SetTargetProperty(animationSecondPath, new PropertyPath(Canvas.TopProperty));
            storyboard.Children.Add(animationSecondPath);
            var animationRotate = new DoubleAnimation
            {
                From = 270,
                To = 180,
                BeginTime = TimeSpan.FromSeconds(4.7),
                Duration = TimeSpan.FromSeconds(0.7)
            };
            Storyboard.SetTarget(animationRotate, tank);
            Storyboard.SetTargetProperty(animationRotate, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            storyboard.Children.Add(animationRotate);
            var animationThirdPath = new DoubleAnimation
            {
                From = secondX,
                To = thirdX,
                BeginTime = TimeSpan.FromSeconds(9.8),
                Duration = TimeSpan.FromSeconds(5.2)
            };
            Storyboard.SetTarget(animationThirdPath, tank);
            Storyboard.SetTargetProperty(animationThirdPath, new PropertyPath(Canvas.LeftProperty));
            storyboard.Children.Add(animationThirdPath);
            var animationRotateTwo = new DoubleAnimation
            {
                From = 180,
                To = 270,
                BeginTime = TimeSpan.FromSeconds(9.7),
                Duration = TimeSpan.FromSeconds(0.7)
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
