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
using System.Threading;
using System.Windows.Threading;
using System.IO;

namespace Курсовая
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string wayPipeImageStart = @"C:\Users\Nikke.tv\Desktop\Учеба\3.1\Курсовая\Курсовая\image\Start.jpg";
        private readonly string wayPipeImageStartWater = @"C:\Users\Nikke.tv\Desktop\Учеба\3.1\Курсовая\Курсовая\image\StartWater.jpg";
        private readonly string wayPipeImageEnd = @"C:\Users\Nikke.tv\Desktop\Учеба\3.1\Курсовая\Курсовая\image\End.jpg";
        private readonly string wayPipeImageEndWater = @"C:\Users\Nikke.tv\Desktop\Учеба\3.1\Курсовая\Курсовая\image\EndWater.jpg";
        private readonly string wayPipeImageL = @"C:\Users\Nikke.tv\Desktop\Учеба\3.1\Курсовая\Курсовая\image\L.jpg";
        private readonly string wayPipeImageLWater = @"C:\Users\Nikke.tv\Desktop\Учеба\3.1\Курсовая\Курсовая\image\LWater.jpg";
        private readonly string wayPipeImageI = @"C:\Users\Nikke.tv\Desktop\Учеба\3.1\Курсовая\Курсовая\image\I.jpg";
        private readonly string wayPipeImageIWater = @"C:\Users\Nikke.tv\Desktop\Учеба\3.1\Курсовая\Курсовая\image\IWater.jpg";
        bool IsClickAble = true;//для контроля нажатия во время течения воды
                
        enum ConectionType
        {
            North = 1,//верх
            East,//право
            South,//низ
            West//лево
        };
        class PipeObject
        {
            public Canvas Canvas;
            public RotateTransform rotateTransform;
            public char image;
        }
        int i = 0, j = 0; //переменные для обозначения двумерного массива во вшешнем событии = timer_tick
        DispatcherTimer timerForWater = new DispatcherTimer();//для потока воды
        DispatcherTimer timer;
        byte secForTimer;
        Canvas startPipe, endPipe;
        byte maxRow, maxColumn;
        Grid grid_sourse;
        private ConectionType conection = ConectionType.North;//указывает с какой стороны будет течь вода относительно данной ячейки
        Random random = new Random();//для генерации случайных чисел
        PipeObject[,] MasCanvas;//массив ячеек 


        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// подготовка к новой игре
        /// </summary>
        private void StartNewGame(byte second)
        {
            IsClickAble = true;
            GenerateMassivOfPipes();
            Timer.Value = 0;
            secForTimer = second;
            timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 10) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Timer.Value >= Timer.Maximum)
            {
                timer.Stop();
                GoWater(null,null);
            }
            else Timer.Value += 10000/ secForTimer / 100;
        }

        /// <summary>
        /// заполнение массива ссылками на ячейки
        /// </summary>
        private void TransferCanvasesFromGridToArray()
        {
            int i = 0, j = 0;
            try
            {
                foreach (Canvas v in grid_sourse.Children)
                {
                    if (v.Name.StartsWith("Pipe_Start"))
                    {
                        try
                        {
                            ImageBrush image = new ImageBrush
                            {
                                ImageSource = new BitmapImage(new Uri(wayPipeImageStart, UriKind.Relative))
                            };
                            v.Background = image;
                        }
                        catch (FileNotFoundException) { };
                    }
                    else if (v.Name.StartsWith("Pipe_End"))
                    {
                        ImageBrush image = new ImageBrush
                        {
                            ImageSource = new BitmapImage(new Uri(wayPipeImageEnd, UriKind.Relative))
                        };
                        v.Background = image;
                    }
                    else
                    {
                        MasCanvas[i, j] = new PipeObject
                        {
                            Canvas = v,
                            rotateTransform = new RotateTransform
                            {
                                CenterX = 0.5,
                                CenterY = 0.5,
                            }
                        };
                        if (++j == maxColumn)
                        {
                            j = 0;
                            i++;
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Изображения не найдены", "Ошибка!");
                Close();
            }
        }
        /// <summary>
        /// Добавление ячеек в массив. Добавления изображения в каждую ячейку
        /// </summary>
        private void GenerateMassivOfPipes()
        {
            i = 0;
            j = 0;
            while (!(i == maxRow && j == maxColumn-1))//повторять, пока не будет создана цельная цепочка
            {
                TransferCanvasesFromGridToArray();
                conection = ConectionType.North;
                i = 0;
                j = 0;
                conection = ConectionType.North;
                bool continueGenerate = true;
                while (continueGenerate)//создание цепочки
                {
                    if (!(i == maxRow && j == maxColumn-1) && MasCanvas[i, j].image == 0) PipeChainLogic(maxRow,maxColumn);
                    else continueGenerate = false;
                }
            }
            i = 0;
            j = 0;
            try
            {
                foreach (Canvas v in grid_sourse.Children)
                {
                    /// id вынести нельзя, тк в середине кода есть к нему обращение
                    if (!v.Name.StartsWith("Pipe_"))
                    {
                        if (MasCanvas[i, j].image == 0)
                        {
                            switch (random.Next(2))
                            {
                                case 0:
                                    {
                                        ImageBrush image = new ImageBrush
                                        {
                                            ImageSource = new BitmapImage(new Uri(wayPipeImageL, UriKind.Relative))
                                        };
                                        v.Background = image;
                                        MasCanvas[i, j].image = 'L';
                                    }
                                    break;
                                default:
                                    {
                                        ImageBrush image = new ImageBrush
                                        {
                                            ImageSource = new BitmapImage(new Uri(wayPipeImageI, UriKind.Relative))
                                        };
                                        v.Background = image;
                                        MasCanvas[i, j].image = 'I';
                                    }
                                    break;
                            }
                        }
                        MasCanvas[i, j].rotateTransform.Angle = random.Next(4) * 90;
                        MasCanvas[i, j].Canvas.Background.RelativeTransform = MasCanvas[i, j].rotateTransform;
                        if (++j == maxColumn)
                        {
                            j = 0;
                            i++;
                        }
                    }
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("Изображения не найдены", "Ошибка!");
                Close();
            }

        }
        /// <summary>
        /// Изменение данных о трубе в ячейку
        /// </summary>
        /// <param name="pipeObject">ячейка</param>
        /// <param name="angle">угол поворота по часовой</param>
        /// <param name="conectionType">куда будет выходить эта труба</param>
        /// <param name="way">путь к картинке</param>
        /// <param name="lineOffset">изменение строки</param>
        /// <param name="columnOffset">изменение столбца</param>
        private void DoPipeDate(PipeObject pipeObject, int angle, ConectionType conectionType, string way, sbyte lineOffset, sbyte columnOffset)
        {
            try
            {
                ImageBrush image = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(way, UriKind.Relative))
                };
                pipeObject.Canvas.Background = image;
                if (way == wayPipeImageL) pipeObject.image = 'L'; else pipeObject.image = 'I';
                pipeObject.rotateTransform.Angle = angle;
                pipeObject.Canvas.Background.RelativeTransform = MasCanvas[i, j].rotateTransform;
                conection = conectionType;
                i += lineOffset;
                j += columnOffset;
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("Изображения не найдены", "Ошибка!");
                Close();
            }
        }
        /// <summary>
        /// Логика для создания цепочки труб
        /// </summary>
        private void PipeChainLogic(byte maxRow, byte maxColumn)
        {
            maxRow--;
            maxColumn--;
            switch (conection)
            {
                case ConectionType.North:
                    {
                        if (i == maxRow)//нижняя строка
                        {
                            if (j != maxColumn) //вся нижняя линяя кроме последней ячейки
                            {
                                DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, wayPipeImageL, 0, 1);//вправо
                            }
                            else //последняя ячейка
                            {
                                DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, 1, 0);//вниз
                            }
                        }
                        else //не последний ряд
                        {
                            if (j == 0)//левый столбец
                            {
                                if (MasCanvas[i, j + 1].image != 0)//если справа есть труба, то только вниз
                                {
                                    DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, 1, 0);//вниз
                                }
                                else//если справа пусто, то вниз или вправо
                                {
                                    if (MasCanvas[i + 1, j].image != 0)//если снизу есть труба, то вправо
                                    {
                                        DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, wayPipeImageL, 0, 1);//вправо
                                    }
                                    else//если снизу нет трубы, то вправо или вниз
                                    {
                                        if (random.Next(2) == 0)
                                        {
                                            DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, wayPipeImageL, 0, 1);//вправо
                                        }
                                        else
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, 1, 0);//вниз
                                        }
                                    }
                                }
                            }
                            else//все кроме левого столбца
                            {
                                if (j == maxColumn)//правый столбец
                                {
                                    if (MasCanvas[i + 1, j].image != 0)//если снизу есть труба, то влево
                                    {
                                        DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, wayPipeImageL, 0, -1);//влево
                                    }
                                    else
                                    {
                                        if (MasCanvas[i, j - 1].image != 0)//если слева есть труба, то только вниз
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, 1, 0);//вниз
                                        }
                                        else//если слева нет трубы, то вниз или влево
                                        {
                                            if (random.Next(2) == 0)
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, wayPipeImageL, 0, -1);//влево
                                            }
                                            else
                                            {
                                                DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, 1, 0);//вниз
                                            }
                                        }
                                    }
                                }
                                else//все, кроме нижнего, левого и правого рядов.
                                {
                                    if (MasCanvas[i, j - 1].image != 0)//если слева есть труба
                                    {
                                        if (MasCanvas[i, j + 1].image != 0)//если справа есть труба, то вниз
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, 1, 0);//вниз
                                        }
                                        else//справа нет трубы
                                        {
                                            if (MasCanvas[i, j - 1].image != 0)//снизу есть труба, то вправо
                                            {
                                                DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, wayPipeImageL, 0, 1);//вправо
                                            }
                                            else//если справа трубы нет, то вниз или вправо
                                            {
                                                if (random.Next(2) == 0)
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, wayPipeImageL, 0, 1);//вправо
                                                }
                                                else
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, 1, 0);//вниз
                                                }
                                            }
                                        }
                                    }
                                    else//слева трубы нет
                                    {
                                        if ((MasCanvas[i, j + 1].image != 0))//если справа есть труба
                                        {
                                            if (MasCanvas[i, j - 1].image != 0)//снизе есть труба, то влево
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, wayPipeImageL, 0, -1);//влево
                                            }
                                            else//снизу нет трубы, то вниз или влево
                                            {
                                                if (random.Next(2) == 0)
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, wayPipeImageL, 0, -1);//влево
                                                }
                                                else
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, 1, 0);//вниз
                                                }
                                            }
                                        }
                                        else//справа трубы нет, то вниз, влево или вправо
                                        {
                                            switch (random.Next(3))
                                            {
                                                case 0: DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, wayPipeImageL, 0, -1); break;//влево
                                                case 1: DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, wayPipeImageL, 0, 1); break;//вправо
                                                case 2: DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, 1, 0); break;//вниз
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } break;
                case ConectionType.East:
                    {
                        if (j == 0)//левый столбец, то вниз
                        {
                            DoPipeDate(MasCanvas[i, j], 270, ConectionType.North, wayPipeImageL, 1, 0);//вниз
                        }
                        else //не левый столбец
                        {
                            if (i != 0)//все кроме верхнего и левого рядов
                            {
                                if (i != maxRow)//все, кроме нижнего, верхнего и левого рядов.
                                {
                                    if (MasCanvas[i - 1, j].image != 0)//если сверху есть труба
                                    {
                                        if (MasCanvas[i, j - 1].image != 0)//если слева есть труба, то вниз
                                        {
                                            DoPipeDate(MasCanvas[i, j], 270, ConectionType.North, wayPipeImageL, 1, 0);//вниз
                                        }
                                        else//слева нет трубы
                                        {
                                            if (MasCanvas[i + 1, j].image != 0)//снизу есть труба, то влево
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, conection, wayPipeImageI, 0, -1);//влево
                                            }
                                            else//если слева трубы нет, то вниз или влево
                                            {
                                                if (random.Next(2) == 0)
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 90, conection, wayPipeImageI, 0, -1);//влево
                                                }
                                                else
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 270, ConectionType.North, wayPipeImageL, 1, 0);//вниз
                                                }
                                            }
                                        }
                                    }
                                    else//сверху трубы нет
                                    {
                                        if ((MasCanvas[i, j - 1].image != 0))//если слева есть труба
                                        {
                                            if (MasCanvas[i + 1, j].image != 0)//снизу есть труба, то вверх
                                            {
                                                DoPipeDate(MasCanvas[i, j], 180, ConectionType.South, wayPipeImageL, -1, 0);//вверх
                                            }
                                            else//снизу нет трубы, то вниз или вверх
                                            {
                                                if (random.Next(2) == 0)
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 180, ConectionType.South, wayPipeImageL, -1, 0);//вверх
                                                }
                                                else
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 270, ConectionType.North, wayPipeImageL, 1, 0);//вниз
                                                }
                                            }
                                        }
                                        else//слева трубы нет, то вниз, вверх или влево
                                        {
                                            switch (random.Next(3))
                                            {
                                                case 0: DoPipeDate(MasCanvas[i, j], 270, ConectionType.North, wayPipeImageL, 1, 0); break;//вниз
                                                case 1: DoPipeDate(MasCanvas[i, j], 180, ConectionType.South, wayPipeImageL, -1, 0); break;//вверх
                                                case 2: DoPipeDate(MasCanvas[i, j], 90, conection, wayPipeImageI, 0, -1); break;//влево 
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } break;
                case ConectionType.South:
                    {
                        if (i == 0)//верхняя строка
                        {
                            DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, wayPipeImageL, 0, 1);//вправо
                        }
                        else //не верхний ряд
                        {
                            if (j == 0)//левый столбец
                            {
                                if (MasCanvas[i, j + 1].image != 0)//если справа есть труба, то вверх
                                {
                                    DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, -1, 0);//вверх
                                }
                                else//если справа пусто, то вверх или вправо
                                {
                                    if (MasCanvas[i - 1, j].image != 0)//если сверху есть труба, то вправо
                                    {
                                        DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, wayPipeImageL, 0, 1);//вправо
                                    }
                                    else//если сверху нет трубы, то вправо или вверх
                                    {
                                        if (random.Next(2) == 0)
                                        {
                                            DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, wayPipeImageL, 0, 1);//вправо
                                        }
                                        else
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, -1, 0);//вверх
                                        }
                                    }
                                }
                            }
                            else//все кроме левого столбца
                            {
                                if (j == maxColumn)//правый столбец
                                {
                                    if (MasCanvas[i - 1, j].image != 0)//если сверху есть труба, то влево
                                    {
                                        DoPipeDate(MasCanvas[i, j], 0, ConectionType.East, wayPipeImageL, 0, -1);//влево
                                    }
                                    else
                                    {
                                        if (MasCanvas[i, j - 1].image != 0)//если слева есть труба, то только вверх
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, -1, 0);//вверх
                                        }
                                        else//если слева нет трубы, то вверх или влево
                                        {
                                            if (random.Next(2) == 0)
                                            {
                                                DoPipeDate(MasCanvas[i, j], 0, ConectionType.East, wayPipeImageL, 0, -1);//влево
                                            }
                                            else
                                            {
                                                DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, -1, 0);//вверх
                                            }
                                        }
                                    }
                                }
                                else//все, кроме верхнего, левого и правого рядов.
                                {
                                    if (MasCanvas[i, j - 1].image != 0)//если слева есть труба
                                    {
                                        if (MasCanvas[i, j + 1].image != 0)//если справа есть труба, то вверх
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, -1, 0);//вверх
                                        }
                                        else//справа нет трубы
                                        {
                                            if (MasCanvas[i - 1, j].image != 0)//сверху есть труба, то вправо
                                            {
                                                DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, wayPipeImageL, 0, 1);//вправо
                                            }
                                            else//если справа трубы нет, то вверх или вправо
                                            {
                                                if (random.Next(2) == 0)
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, wayPipeImageL, 0, 1);//вправо
                                                }
                                                else
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, -1, 0);//вверх
                                                }
                                            }
                                        }
                                    }
                                    else//слева трубы нет
                                    {
                                        if ((MasCanvas[i, j + 1].image != 0))//если справа есть труба
                                        {
                                            if (MasCanvas[i - 1, j].image != 0)//сверху есть труба, то влево
                                            {
                                                DoPipeDate(MasCanvas[i, j], 0, ConectionType.East, wayPipeImageL, 0, -1);//влево
                                            }
                                            else//сверху нет трубы, то вверх или влево
                                            {
                                                if (random.Next(2) == 0)
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 0, ConectionType.East, wayPipeImageL, 0, -1);//влево
                                                }
                                                else
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, -1, 0);//вверх
                                                }
                                            }
                                        }
                                        else//справа трубы нет, то вверх, влево или вправо
                                        {
                                            switch (random.Next(3))
                                            {
                                                case 0: DoPipeDate(MasCanvas[i, j], 0, ConectionType.East, wayPipeImageL, 0, -1); break;//влево
                                                case 1: DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, wayPipeImageL, 0, 1); break;//вправо 
                                                case 2: DoPipeDate(MasCanvas[i, j], 0, conection, wayPipeImageI, -1, 0); break;//вверх
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    } break;
                case ConectionType.West:
                    {
                        if (j == maxColumn)//правый столбец, то вниз
                        {
                            DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, wayPipeImageL, 1, 0);//вниз
                        }
                        else //не правый столбец
                        {
                            if (i == 0)//верхний ряд
                            {
                                if (MasCanvas[i, j + 1].image != 0)//если справа есть труба, то только вниз
                                {
                                    DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, wayPipeImageL, 1, 0);//вниз
                                }
                                else//если справа пусто, то вниз или вправо
                                {
                                    if (MasCanvas[i + 1, j].image != 0)//если снизу есть труба, то вправо
                                    {
                                        DoPipeDate(MasCanvas[i, j], 90, conection, wayPipeImageI, 0, 1);//вправо
                                    }
                                    else//если снизу нет трубы, то вправо или вниз
                                    {
                                        if (random.Next(2) == 0)
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, wayPipeImageL, 1, 0);//вниз
                                        }
                                        else
                                        {
                                            DoPipeDate(MasCanvas[i, j], 90, conection, wayPipeImageI, 0, 1);//вправо
                                        }
                                    }
                                }
                            }
                            else//все кроме верхнего и левого рядов
                            {
                                if (i == maxRow)//нижний ряд
                                {
                                    if (MasCanvas[i - 1, j].image != 0)//если сверху есть труба, то вправо
                                    {
                                        DoPipeDate(MasCanvas[i, j], 90, conection, wayPipeImageI, 0, 1);//вправо
                                    }
                                    else
                                    {
                                        if (MasCanvas[i, j - 1].image != 0)//если справа есть труба, то  вверх
                                        {
                                            DoPipeDate(MasCanvas[i, j], 90, ConectionType.South, wayPipeImageL, -1, 0);//вверх
                                        }
                                        else//если сверху нет трубы, то вправо илли вверх
                                        {
                                            if (random.Next(2) == 0)
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, ConectionType.South, wayPipeImageL, -1, 0);//вверх
                                            }
                                            else
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, conection, wayPipeImageI, 0, 1);//вправо
                                            }
                                        }
                                    }
                                }
                                else//все, кроме нижнего, верхнего и правого рядов.
                                {
                                    if (MasCanvas[i - 1, j].image != 0)//если сверху есть труба
                                    {
                                        if (MasCanvas[i, j + 1].image != 0)//если справа есть труба, то вниз
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, wayPipeImageL, 1, 0);//вниз
                                        }
                                        else//справа нет трубы
                                        {
                                            if (MasCanvas[i + 1, j].image != 0)//снизу есть труба, то вправо
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, conection, wayPipeImageI, 0, 1);//вправо
                                            }
                                            else//если справа трубы нет, то вниз или вправо
                                            {
                                                if (random.Next(2) == 0)
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 90, conection, wayPipeImageI, 0, 1);//вправо
                                                }
                                                else
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, wayPipeImageL, 1, 0);//вниз
                                                }
                                            }
                                        }
                                    }
                                    else//сверху трубы нет
                                    {
                                        if ((MasCanvas[i, j + 1].image != 0))//если справа есть труба
                                        {
                                            if (MasCanvas[i + 1, j].image != 0)//снизу есть труба, то вверх
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, ConectionType.South, wayPipeImageL, -1, 0);//вверх
                                            }
                                            else//снизу нет трубы, то вниз или вверх
                                            {
                                                if (random.Next(2) == 0)
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 90, ConectionType.South, wayPipeImageL, -1, 0);//вверх
                                                }
                                                else
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, wayPipeImageL, 1, 0);//вниз
                                                }
                                            }
                                        }
                                        else//справа трубы нет, то вниз, вверх или вправо
                                        {
                                            switch (random.Next(3))
                                            {
                                                case 0: DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, wayPipeImageL, 1, 0); break;//вниз 
                                                case 1: DoPipeDate(MasCanvas[i, j], 90, ConectionType.South, wayPipeImageL, -1, 0); break;//вверх
                                                case 2: DoPipeDate(MasCanvas[i, j], 90, conection, wayPipeImageI, 0, 1); break;//вправо 
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } break;
            }
        }
        
        /// <summary>
        /// Нажата кнопка "Выход"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitFromGame(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// Нажата кнопка "Меню". Выйти в меню
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToMenu_Click(object sender, RoutedEventArgs e)
        {
            Game.Visibility = Visibility.Hidden;
            Menu.Visibility = Visibility.Visible;
            PipesGame5x6.Visibility = Visibility.Hidden;
            PipesGame7x8.Visibility = Visibility.Hidden;
            PipesGame10x12.Visibility = Visibility.Hidden;
            timerForWater.Stop();//на случай, если выйти в меню во время течения воды
            timer.Stop();//остановить текущий таймер
        }
        /// <summary>
        /// Нажата кнопка "Играть". Сделать окно игры видимым. Сгенерировать массив труб
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoToGame_Click(object sender, RoutedEventArgs e)
        {
            ChoiceLevel.Visibility = Visibility.Visible;            
        }
        /// <summary>
        /// На трубу нажали левой кнопкой мыши. 
        /// Повернуть трубу на 90 градусов по часовой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeftButtonClickPipe(object sender, MouseButtonEventArgs e)
        {
            if (IsClickAble)
            {
                Canvas canvas = (Canvas)sender;
                RotateTransform rt = (RotateTransform)canvas.Background.RelativeTransform;
                rt.Angle += 90;
                rt.Angle %= 360;
            }
        }
        /// <summary>
        /// На трубу нажали правой кнопкой мыши. 
        /// Повернуть трубу на 90 градусов против часовой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RightButtonClickPipe(object sender, MouseButtonEventArgs e)
        {
            if (IsClickAble)
            {
                Canvas canvas = (Canvas)sender;
                RotateTransform rt = (RotateTransform)canvas.Background.RelativeTransform;
                rt.Angle -= 90;
                rt.Angle %= 360;
            }
        }
        /// <summary>
        /// Нажата труба с вентелем. Начало потока воды
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoWater(object sender, MouseButtonEventArgs e)
        {
            if (IsClickAble)
            {
                try
                {
                    IsClickAble = false;
                    timer.Stop();
                    i = 0; j = 0;
                    ImageBrush image = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri(wayPipeImageStartWater, UriKind.Relative))
                    };
                    startPipe.Background = image;
                    timerForWater = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 250) };
                    timerForWater.Tick += TimerForWater_Tick;
                    timerForWater.Start();
                }
                catch (System.IO.FileNotFoundException)
                {
                    MessageBox.Show("Изображения не найдены", "Ошибка!");
                    Close();
                }
            }
        }
        /// <summary>
        /// Оповещение о победе/поражении
        /// </summary>
        /// <param name="Message">текст оповещения</param>
        private void ShowMessage(string Message)
        {
            timerForWater.Stop();
            MessageBox.Show(Message);
            if (Message == "Победа!")
            {
                if (Timer.Value == Timer.Maximum) Score.Content = Convert.ToString(Convert.ToDouble(Score.Content.ToString()) + 50);
                Score.Content = Convert.ToString(Convert.ToDouble(Score.Content.ToString()) + (Timer.Maximum - Timer.Value) * 7 * 8 / 1000);
                StartNewGame(20);
            }
            else 
            {
                GoToMenu_Click(null, null);
            }
            
        }

        private void Easy_Click(object sender, RoutedEventArgs e)
        {
            ChoiceLevel.Visibility = Visibility.Hidden;
            Game.Visibility = Visibility.Visible;
            Menu.Visibility = Visibility.Hidden;
            Score.Content = "0";
            grid_sourse = PipesGame5x6;
            startPipe = Pipe_Start5x6;
            endPipe = Pipe_End5x6;
            maxRow = 5;
            maxColumn = 6;
            PipesGame5x6.Visibility = Visibility.Visible;
            MasCanvas = new PipeObject[maxRow, maxColumn];//массив ячеек 
            StartNewGame(15);
        }

        private void Medium_Click(object sender, RoutedEventArgs e)
        {
            ChoiceLevel.Visibility = Visibility.Hidden;
            Game.Visibility = Visibility.Visible;
            Menu.Visibility = Visibility.Hidden;
            Score.Content = "0";
            startPipe = Pipe_Start7x8;
            endPipe = Pipe_End7x8;
            maxRow = 7;
            maxColumn = 8;            
            grid_sourse = PipesGame7x8;
            PipesGame7x8.Visibility = Visibility.Visible;
            MasCanvas = new PipeObject[maxRow, maxColumn];//массив ячеек 
            StartNewGame(20);
        }

        private void Hard_Click(object sender, RoutedEventArgs e)
        {
            ChoiceLevel.Visibility = Visibility.Hidden;
            Game.Visibility = Visibility.Visible;
            Menu.Visibility = Visibility.Hidden;
            Score.Content = "0";
            grid_sourse = PipesGame10x12;
            startPipe = Pipe_Start10x12;
            endPipe = Pipe_End10x12;
            maxRow = 10;
            maxColumn = 12;
            PipesGame10x12.Visibility = Visibility.Visible;
            MasCanvas = new PipeObject[maxRow, maxColumn];//массив ячеек 
            StartNewGame(25);
        }

        private void Сancel_Click(object sender, RoutedEventArgs e)
        {
            ChoiceLevel.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Логика изменения труб относительно течения воды
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerForWater_Tick(object sender, EventArgs e)
        {
            if (i == maxRow && j == maxColumn-1)
            {
                try
                {
                    ImageBrush image = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri(wayPipeImageEndWater, UriKind.Relative))
                    };
                    endPipe.Background = image;
                    ShowMessage("Победа!");
                }
                catch (System.IO.FileNotFoundException)
                {
                    MessageBox.Show("Изображения не найдены", "Ошибка!");
                    Close();
                }
            }
            else
            {
                if (i < 0 || i > maxRow | j < 0 || j > maxColumn-1)
                {
                    ShowMessage("Поражение");
                }
                else
                {
                    if (MasCanvas[i, j].image == 'L')
                    {
                        switch (conection)
                        {
                            case ConectionType.North:
                                {
                                    switch (MasCanvas[i, j].rotateTransform.Angle % 360)
                                    {
                                        case 90:  DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, wayPipeImageLWater, 0, -1); break;
                                        case 180: DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, wayPipeImageLWater, 0, 1); break;
                                        default: ShowMessage("Поражение"); break;
                                    }
                                }
                                break;
                            case ConectionType.East:
                                {
                                    switch (MasCanvas[i, j].rotateTransform.Angle % 360)
                                    {
                                        case 180: DoPipeDate(MasCanvas[i, j], 180, ConectionType.South, wayPipeImageLWater, -1, 0); break;
                                        case 270: DoPipeDate(MasCanvas[i, j], 270, ConectionType.North, wayPipeImageLWater, 1, 0); break;
                                        default: ShowMessage("Поражение"); break;
                                    }
                                }
                                break;
                            case ConectionType.South:
                                {
                                    switch (MasCanvas[i, j].rotateTransform.Angle % 360)
                                    {
                                        case 270: DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, wayPipeImageLWater, 0, 1); break; 
                                        case 0:   DoPipeDate(MasCanvas[i, j], 0, ConectionType.East, wayPipeImageLWater, 0, -1); break;
                                        default: ShowMessage("Поражение"); break;
                                    }
                                }
                                break;
                            case ConectionType.West:
                                {
                                    switch (MasCanvas[i, j].rotateTransform.Angle % 360)
                                    {
                                        case 0:  DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, wayPipeImageLWater, 1, 0); break; 
                                        case 90: DoPipeDate(MasCanvas[i, j], 90, ConectionType.South, wayPipeImageLWater, -1, 0); break;
                                        default: ShowMessage("Поражение"); break;
                                    }
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (conection)
                        {
                            case ConectionType.North:
                                {
                                    if (MasCanvas[i, j].rotateTransform.Angle % 180 == 0)
                                    {
                                        DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, wayPipeImageIWater, 1, 0);
                                    }
                                    else ShowMessage("Поражение");
                                }
                                break;
                            case ConectionType.East:
                                {
                                    if (MasCanvas[i, j].rotateTransform.Angle % 180 == 90)
                                    {
                                        DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, wayPipeImageIWater, 0, -1);
                                    }
                                    else ShowMessage("Поражение");
                                }
                                break;
                            case ConectionType.South:
                                {
                                    if (MasCanvas[i, j].rotateTransform.Angle % 180 == 0)
                                    {
                                        DoPipeDate(MasCanvas[i, j], 0, ConectionType.South, wayPipeImageIWater, -1, 0);
                                    }
                                    else ShowMessage("Поражение");
                                }
                                break;
                            case ConectionType.West:
                                {
                                    if (MasCanvas[i, j].rotateTransform.Angle % 180 == 90)
                                    {
                                        DoPipeDate(MasCanvas[i, j], 90, ConectionType.West, wayPipeImageIWater, 0, 1);
                                    }
                                    else ShowMessage("Поражение");
                                }
                                break;
                        }
                    }
                }
            }
        }//Timer_tick        
    }
}
