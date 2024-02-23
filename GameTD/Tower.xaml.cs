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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameTD
{
    /// <summary>
    /// Логика взаимодействия для Tower.xaml
    /// </summary>
    public partial class Tower : UserControl
    {
        private Ellipse circle;
        public readonly double radius = 160;

        public Tower()
        {
            InitializeComponent();
            InitializeCircle();
        }

        private void InitializeCircle()
        {
            circle = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Visibility = Visibility.Hidden
            };
            Canvas.SetLeft(circle, -radius + 30);
            Canvas.SetTop(circle, -radius + 30);
            MainCanvas.Children.Add(circle);
        }

        private void ShowCircle()
        {
            circle.Visibility = Visibility.Visible;
        }

        private void HideCircle()
        {
            circle.Visibility = Visibility.Hidden;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowCircle();
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            HideCircle();
        }
    }
}
