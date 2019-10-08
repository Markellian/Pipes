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

namespace Курсовая
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        class PipeObject
        {
           public Canvas Canvas;
           public RotateTransform rotateTransform;
        }
        RotateTransform rt = new RotateTransform();
        public MainWindow()
        {
            InitializeComponent();

            
            Random random = new Random();
            PipeObject[,] MasCanvas = new PipeObject[7, 8];
            int i = 0, j = 0;
            foreach (Canvas v in PipesGame.Children)
            {
                if (v.Name == "Pipe_Start")
                {

                }
                else if (v.Name == "Pipe_End")
                {

                }
                else
                {                  
                    
                    switch (random.Next(2))
                    {
                        case 0:
                            {
                                ImageBrush ib = new ImageBrush
                                {
                                    ImageSource = new BitmapImage(new Uri(@"C:\Users\Nikke.tv\Desktop\Учеба\3.1\Курсовая\Курсовая\image\l.jpg", UriKind.Relative))
                                };
                                v.Background = ib;
                            }
                            break;
                        default:
                            {
                                ImageBrush ic = new ImageBrush
                                {
                                    ImageSource = new BitmapImage(new Uri(@"C:\Users\Nikke.tv\Desktop\Учеба\3.1\Курсовая\Курсовая\image\I.jpg", UriKind.Relative))
                                };
                                v.Background = ic;
                            }
                            break;
                    }
                    MasCanvas[i, j] = new PipeObject
                    {
                        Canvas = v,
                        rotateTransform = new RotateTransform
                        {
                            CenterX = 0.5,
                            CenterY = 0.5,
                            Angle = random.Next(4) * 90
                        }
                    };
                    MasCanvas[i, j].Canvas.Background.RelativeTransform = MasCanvas[i, j].rotateTransform;
                    
                    if (++j == 8)
                    {
                        j = 0;
                        i++;
                    }
                }
            }
            
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GoToMenu_Click(object sender, RoutedEventArgs e)
        {
            Game.Visibility = Visibility.Hidden;
            Menu.Visibility = Visibility.Visible;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Game.Visibility = Visibility.Visible;
            Menu.Visibility = Visibility.Hidden;
        }      

        private void LeftButtonClickPipe(object sender, MouseButtonEventArgs e)
        {
            Canvas canvas = (Canvas)sender;
            RotateTransform rt = (RotateTransform)canvas.Background.RelativeTransform;
            rt.Angle += 90;
        }

        private void RightButtonClickPipe(object sender, MouseButtonEventArgs e)
        {
            Canvas canvas = (Canvas)sender;
            RotateTransform rt = (RotateTransform)canvas.Background.RelativeTransform;
            rt.Angle -= 90;
        }
    }
}
