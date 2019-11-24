using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Drawing;
using System.Windows.Interop;

namespace Курсовая
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Bitmap PipeImageStart = Properties.Resources.Start;
        private readonly Bitmap PipeImageStartWater = Properties.Resources.StartWater;
        private readonly Bitmap PipeImageEnd = Properties.Resources.End;
        private readonly Bitmap PipeImageEndWater = Properties.Resources.EndWater;
        private readonly Bitmap PipeImageL = Properties.Resources.L;
        private readonly Bitmap PipeImageLWater = Properties.Resources.LWater;
        private readonly Bitmap PipeImageI = Properties.Resources.I;
        private readonly Bitmap PipeImageIWater = Properties.Resources.IWater;
        private readonly string messageWin = "Победа!";
        private readonly string messageLose = "Поражение!";
        bool IsClickAble = true;//для контроля нажатия во время течения воды
                
        enum ConectionType//для логики взаимодействия труб. указывает, откуда будет течь вода
        {
            North = 1,//верх
            East,//право
            South,//низ
            West//лево
        };
        class PipeObject//ячейка
        {
            public Canvas Canvas;
            public RotateTransform rotateTransform;
            public char image;
        }
        int i = 0, j = 0; //переменные для обозначения двумерного массива
        DispatcherTimer timerForWater = new DispatcherTimer();//для потока воды
        DispatcherTimer timer;//для таймера во время игры
        byte secForTimer;//время для таймера
        Canvas startPipe, endPipe;//для изменения картинок начальной и конечной труб
        byte maxRow, maxColumn;//макимальное количество столбцов и строк
        Grid grid_sourse;//для получения ячеек
        private ConectionType conection = ConectionType.North;//указывает с какой стороны будет течь вода относительно данной ячейки
        Random random = new Random();//для генерации случайных чисел
        PipeObject[,] MasCanvas;//массив ячеек 


        public MainWindow()
        {
            InitializeComponent();
            this.Background = CreateBrushFromBitmap(Properties.Resources.wall);
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
        /// <summary>
        /// Таймер во время игры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Timer.Value >= Timer.Maximum)
            {
                timer.Stop();
                GoWater(null,null);
            }
            else Timer.Value += Timer.Maximum/ secForTimer / 100;
        }
        /// <summary>
        /// Делает изображение из ресурса доступным для присвоения
        /// </summary>
        /// <param name="bmp">изображение из ресурса</param>
        /// <returns></returns>
        public static System.Windows.Media.Brush CreateBrushFromBitmap(Bitmap bmp)
        {
            return new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
        }

        /// <summary>
        /// заполнение массива ссылками на ячейки
        /// </summary>
        private void TransferCanvasesFromGridToArray()
        {
            int i = 0, j = 0;
            foreach (Canvas v in grid_sourse.Children)
            {
                if (v.Name.StartsWith("Pipe_Start"))
                {
                    v.Background = (ImageBrush)CreateBrushFromBitmap(PipeImageStart);
                }
                else if (v.Name.StartsWith("Pipe_End"))
                {
                    
                    v.Background = (ImageBrush)CreateBrushFromBitmap(PipeImageEnd);
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
        /// <summary>
        /// Подготовка ячеек к игре.
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
                                    v.Background = (ImageBrush)CreateBrushFromBitmap(PipeImageL);
                                    MasCanvas[i, j].image = 'L';
                                }
                                break;
                            default:
                                {
                                    v.Background = (ImageBrush)CreateBrushFromBitmap(PipeImageI);
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
        /// <summary>
        /// Изменение данных о трубе в ячейке
        /// </summary>
        /// <param name="pipeObject">ячейка</param>
        /// <param name="angle">угол поворота по часовой</param>
        /// <param name="conectionType">куда будет выходить эта труба</param>
        /// <param name="image">путь к картинке</param>
        /// <param name="lineOffset">изменение строки</param>
        /// <param name="columnOffset">изменение столбца</param>
        private void DoPipeDate(PipeObject pipeObject, int angle, ConectionType conectionType, Bitmap image, sbyte lineOffset, sbyte columnOffset)
        {
            try
            {
                pipeObject.Canvas.Background = (ImageBrush)CreateBrushFromBitmap(image);
                if (image == PipeImageL) pipeObject.image = 'L'; else pipeObject.image = 'I';
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
        /// Случайная генерация труб L вверх или вниз
        /// </summary>
        /// <param name="angleUp"></param>
        /// <param name="angleDown"></param>
        private void GoUpOrDpwn(int angleUp, int angleDown)
        {
            if (random.Next(2) == 0) DoPipeDate(MasCanvas[i, j], angleUp, ConectionType.South, PipeImageL, -1, 0);//вверх
                else DoPipeDate(MasCanvas[i, j], angleDown, ConectionType.North, PipeImageL, 1, 0);//вниз                
        }
        /// <summary>
        /// Случайная генерация труб L влево или вправо
        /// </summary>
        /// <param name="angelLeft"></param>
        /// <param name="angelRight"></param>
        private void GoLeftOrRight(int angelLeft, int angelRight)
        {
            if (random.Next(2) == 0) DoPipeDate(MasCanvas[i, j], angelLeft, ConectionType.East, PipeImageL, 0, -1);//влево
                else DoPipeDate(MasCanvas[i, j], angelRight, ConectionType.West, PipeImageL, 0, 1);//вправо            
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
                                DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, PipeImageL, 0, 1);//вправо
                            }
                            else //последняя ячейка
                            {
                                DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, 1, 0);//вниз
                            }
                        }
                        else //не последний ряд
                        {
                            if (j == 0)//левый столбец
                            {
                                if (MasCanvas[i, j + 1].image != 0)//если справа есть труба, то только вниз
                                {
                                    DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, 1, 0);//вниз
                                }
                                else//если справа пусто, то вниз или вправо
                                {
                                    if (MasCanvas[i + 1, j].image != 0)//если снизу есть труба, то вправо
                                    {
                                        DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, PipeImageL, 0, 1);//вправо
                                    }
                                    else//если снизу нет трубы, то вправо или вниз
                                    {
                                        if (random.Next(2) == 0)
                                        {
                                            DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, PipeImageL, 0, 1);//вправо
                                        }
                                        else
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, 1, 0);//вниз
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
                                        DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, PipeImageL, 0, -1);//влево
                                    }
                                    else
                                    {
                                        if (MasCanvas[i, j - 1].image != 0)//если слева есть труба, то только вниз
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, 1, 0);//вниз
                                        }
                                        else//если слева нет трубы, то вниз или влево
                                        {
                                            if (random.Next(2) == 0)
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, PipeImageL, 0, -1);//влево
                                            }
                                            else
                                            {
                                                DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, 1, 0);//вниз
                                            }
                                        }
                                    }
                                }
                                else//все, кроме нижнего, левого и правого рядов.
                                {
                                    if (MasCanvas[i, j - 1].image != 0)//если слева есть труба; снизу и справа - нет
                                    {
                                        if (MasCanvas[i, j + 1].image != 0)//если справа есть труба, то вниз
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, 1, 0);//вниз
                                        }
                                        else//справа нет трубы
                                        {
                                            if (MasCanvas[i, j - 1].image != 0)//снизу есть труба, то вправо
                                            {
                                                DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, PipeImageL, 0, 1);//вправо
                                            }
                                            else//если справа трубы нет, то вниз или вправо
                                            {
                                                if (random.Next(2) == 0)
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, PipeImageL, 0, 1);//вправо
                                                }
                                                else
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, 1, 0);//вниз
                                                }
                                            }
                                        }
                                    }
                                    else//слева трубы нет
                                    {
                                        if (MasCanvas[i, j - 1].image != 0)//снизу есть труба, 
                                        {
                                            if (MasCanvas[i, j + 1].image != 0)//если справа есть труба
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, PipeImageL, 0, -1);//влево
                                            }
                                            else//справа нет трубы, то влево или вправо
                                            {
                                                GoLeftOrRight(90, 180);
                                            }
                                        }
                                        else//справа трубы нет, то вниз, влево или вправо
                                        {
                                            switch (random.Next(3))
                                            {
                                                case 0: DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, PipeImageL, 0, -1); break;//влево
                                                case 1: DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, PipeImageL, 0, 1); break;//вправо
                                                case 2: DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, 1, 0); break;//вниз
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
                            DoPipeDate(MasCanvas[i, j], 270, ConectionType.North, PipeImageL, 1, 0);//вниз
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
                                            DoPipeDate(MasCanvas[i, j], 270, ConectionType.North, PipeImageL, 1, 0);//вниз
                                        }
                                        else//слева нет трубы
                                        {
                                            if (MasCanvas[i + 1, j].image != 0)//снизу есть труба, то влево
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, conection, PipeImageI, 0, -1);//влево
                                            }
                                            else//если слева трубы нет, то вниз или влево
                                            {
                                                if (random.Next(2) == 0)
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 90, conection, PipeImageI, 0, -1);//влево
                                                }
                                                else
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 270, ConectionType.North, PipeImageL, 1, 0);//вниз
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
                                                DoPipeDate(MasCanvas[i, j], 180, ConectionType.South, PipeImageL, -1, 0);//вверх
                                            }
                                            else//снизу нет трубы, то вниз или вверх
                                            {
                                                GoUpOrDpwn(180, 270);
                                            }
                                        }
                                        else//слева трубы нет, то вниз, вверх или влево
                                        {
                                            switch (random.Next(3))
                                            {
                                                case 0: DoPipeDate(MasCanvas[i, j], 270, ConectionType.North, PipeImageL, 1, 0); break;//вниз
                                                case 1: DoPipeDate(MasCanvas[i, j], 180, ConectionType.South, PipeImageL, -1, 0); break;//вверх
                                                case 2: DoPipeDate(MasCanvas[i, j], 90, conection, PipeImageI, 0, -1); break;//влево 
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
                            DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, PipeImageL, 0, 1);//вправо
                        }
                        else //не верхний ряд
                        {
                            if (j == 0)//левый столбец
                            {
                                if (MasCanvas[i, j + 1].image != 0)//если справа есть труба, то вверх
                                {
                                    DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, -1, 0);//вверх
                                }
                                else//если справа пусто, то вверх или вправо
                                {
                                    if (MasCanvas[i - 1, j].image != 0)//если сверху есть труба, то вправо
                                    {
                                        DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, PipeImageL, 0, 1);//вправо
                                    }
                                    else//если сверху нет трубы, то вправо или вверх
                                    {
                                        if (random.Next(2) == 0)
                                        {
                                            DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, PipeImageL, 0, 1);//вправо
                                        }
                                        else
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, -1, 0);//вверх
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
                                        DoPipeDate(MasCanvas[i, j], 0, ConectionType.East, PipeImageL, 0, -1);//влево
                                    }
                                    else
                                    {
                                        if (MasCanvas[i, j - 1].image != 0)//если слева есть труба, то только вверх
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, -1, 0);//вверх
                                        }
                                        else//если слева нет трубы, то вверх или влево
                                        {
                                            if (random.Next(2) == 0)
                                            {
                                                DoPipeDate(MasCanvas[i, j], 0, ConectionType.East, PipeImageL, 0, -1);//влево
                                            }
                                            else
                                            {
                                                DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, -1, 0);//вверх
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
                                            DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, -1, 0);//вверх
                                        }
                                        else//справа нет трубы
                                        {
                                            if (MasCanvas[i - 1, j].image != 0)//сверху есть труба, то вправо
                                            {
                                                DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, PipeImageL, 0, 1);//вправо
                                            }
                                            else//если справа трубы нет, то вверх или вправо
                                            {
                                                if (random.Next(2) == 0)
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, PipeImageL, 0, 1);//вправо
                                                }
                                                else
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, -1, 0);//вверх
                                                }
                                            }
                                        }
                                    }
                                    else//слева трубы нет
                                    {
                                        if (MasCanvas[i - 1, j].image != 0)//сверху есть труба
                                        {
                                            if (MasCanvas[i, j + 1].image != 0)//справа есть труба, то влево
                                            {
                                                DoPipeDate(MasCanvas[i, j], 0, ConectionType.East, PipeImageL, 0, -1);
                                            }
                                            else//справа нет трубы, то ввправо или влево
                                            {
                                                GoLeftOrRight(0, 270);
                                            }
                                                                                      
                                        }
                                        else//справа трубы нет, то вверх, влево или вправо
                                        {
                                            switch (random.Next(3))
                                            {
                                                case 0: DoPipeDate(MasCanvas[i, j], 0, ConectionType.East, PipeImageL, 0, -1); break;//влево
                                                case 1: DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, PipeImageL, 0, 1); break;//вправо 
                                                case 2: DoPipeDate(MasCanvas[i, j], 0, conection, PipeImageI, -1, 0); break;//вверх
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
                            DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, PipeImageL, 1, 0);//вниз
                        }
                        else //не правый столбец
                        {
                            if (i == 0)//верхний ряд
                            {
                                if (MasCanvas[i, j + 1].image != 0)//если справа есть труба, то только вниз
                                {
                                    DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, PipeImageL, 1, 0);//вниз
                                }
                                else//если справа пусто, то вниз или вправо
                                {
                                    if (MasCanvas[i + 1, j].image != 0)//если снизу есть труба, то вправо
                                    {
                                        DoPipeDate(MasCanvas[i, j], 90, conection, PipeImageI, 0, 1);//вправо
                                    }
                                    else//если снизу нет трубы, то вправо или вниз
                                    {
                                        if (random.Next(2) == 0)
                                        {
                                            DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, PipeImageL, 1, 0);//вниз
                                        }
                                        else
                                        {
                                            DoPipeDate(MasCanvas[i, j], 90, conection, PipeImageI, 0, 1);//вправо
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
                                        DoPipeDate(MasCanvas[i, j], 90, conection, PipeImageI, 0, 1);//вправо
                                    }
                                    else
                                    {
                                        if (MasCanvas[i, j - 1].image != 0)//если справа есть труба, то  вверх
                                        {
                                            DoPipeDate(MasCanvas[i, j], 90, ConectionType.South, PipeImageL, -1, 0);//вверх
                                        }
                                        else//если сверху нет трубы, то вправо илли вверх
                                        {
                                            if (random.Next(2) == 0)
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, ConectionType.South, PipeImageL, -1, 0);//вверх
                                            }
                                            else
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, conection, PipeImageI, 0, 1);//вправо
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
                                            DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, PipeImageL, 1, 0);//вниз
                                        }
                                        else//справа нет трубы
                                        {
                                            if (MasCanvas[i + 1, j].image != 0)//снизу есть труба, то вправо
                                            {
                                                DoPipeDate(MasCanvas[i, j], 90, conection, PipeImageI, 0, 1);//вправо
                                            }
                                            else//если справа трубы нет, то вниз или вправо
                                            {
                                                if (random.Next(2) == 0)
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 90, conection, PipeImageI, 0, 1);//вправо
                                                }
                                                else
                                                {
                                                    DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, PipeImageL, 1, 0);//вниз
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
                                                DoPipeDate(MasCanvas[i, j], 90, ConectionType.South, PipeImageL, -1, 0);//вверх
                                            }
                                            else//снизу нет трубы, то вниз или вверх
                                            {
                                                GoUpOrDpwn(90, 0);
                                            }
                                        }
                                        else//справа трубы нет, то вниз, вверх или вправо
                                        {
                                            switch (random.Next(3))
                                            {
                                                case 0: DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, PipeImageL, 1, 0); break;//вниз 
                                                case 1: DoPipeDate(MasCanvas[i, j], 90, ConectionType.South, PipeImageL, -1, 0); break;//вверх
                                                case 2: DoPipeDate(MasCanvas[i, j], 90, conection, PipeImageI, 0, 1); break;//вправо 
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
            Menu.Visibility = Visibility.Hidden;
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
                if (rt.Angle == -90) rt.Angle = 270; 
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
                IsClickAble = false;
                timer.Stop();
                i = 0; j = 0;
                startPipe.Background = (ImageBrush)CreateBrushFromBitmap(PipeImageStartWater);
                timerForWater = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 250) };
                timerForWater.Tick += TimerForWater_Tick;
                timerForWater.Start();
            }
        }
        /// <summary>
        /// Оповещение о победе/поражении и выполнение соответствующих действий
        /// </summary>
        /// <param name="Message">текст оповещения</param>
        private void ShowMessage(string Message)
        {
            timerForWater.Stop();
            MessageBox.Show(Message, Message.Substring(0, Message.Length - 1));
            if (Message == messageWin)
            {
                if (Timer.Value == Timer.Maximum) Score.Content = Convert.ToString(Convert.ToDouble(Score.Content.ToString()) + 1);
                Score.Content = Convert.ToString(Convert.ToDouble(Score.Content.ToString()) + (Timer.Maximum - Timer.Value) * 7 * 8 / 1000);
                StartNewGame(20);
            }
            else  GoToMenu_Click(null, null);            
        }
        /// <summary>
        /// Уровень выбран
        /// </summary>
        /// <param name="GridSourse">Grid-иассив</param>
        /// <param name="StartPipe">начальная труба</param>
        /// <param name="EndPipe">Конечная труба</param>
        /// <param name="rows">кол-во строк</param>
        /// <param name="colomns">кол-во столбцов</param>
        /// <param name="time">время в сек</param>
        private void LevelChoiced(Grid GridSourse, Canvas StartPipe, Canvas EndPipe, byte rows, byte colomns, byte time)
        {
            ChoiceLevel.Visibility = Visibility.Hidden;
            Game.Visibility = Visibility.Visible;
            Menu.Visibility = Visibility.Hidden;
            Score.Content = "0";
            grid_sourse = GridSourse;
            startPipe = StartPipe;
            endPipe = EndPipe;
            maxRow = rows;
            maxColumn = colomns;
            GridSourse.Visibility = Visibility.Visible;
            MasCanvas = new PipeObject[maxRow, maxColumn];//массив ячеек 
            StartNewGame(time);
        }
        /// <summary>
        /// выбран легкий режим игры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Easy_Click(object sender, RoutedEventArgs e)
        {
            LevelChoiced(PipesGame5x6, Pipe_Start5x6, Pipe_End5x6 , 5, 6, 15);
        }
        /// <summary>
        /// выбран средний режим игры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Medium_Click(object sender, RoutedEventArgs e)
        {
            LevelChoiced(PipesGame7x8, Pipe_Start7x8, Pipe_End7x8, 7, 8, 20);
        }
        /// <summary>
        /// выбран тяжелый режим игры
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hard_Click(object sender, RoutedEventArgs e)
        {
            LevelChoiced(PipesGame10x12, Pipe_Start10x12, Pipe_End10x12, 10, 12, 25);
        }
        /// <summary>
        /// нажата кнопка "Отмена"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Сancel_Click(object sender, RoutedEventArgs e)
        {
            ChoiceLevel.Visibility = Visibility.Hidden;
            Menu.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Логика изменения труб относительно течения воды
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerForWater_Tick(object sender, EventArgs e)
        {
            if (i == maxRow && j == maxColumn - 1)
            {
                endPipe.Background = (ImageBrush)CreateBrushFromBitmap(PipeImageEndWater);
                ShowMessage(messageWin);
            }
            else
            {
                if (i < 0 || i > maxRow | j < 0 || j > maxColumn - 1)
                {
                    ShowMessage(messageLose);
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
                                        case 90: DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, PipeImageLWater, 0, -1); break;
                                        case 180: DoPipeDate(MasCanvas[i, j], 180, ConectionType.West, PipeImageLWater, 0, 1); break;
                                        default: ShowMessage(messageLose); break;
                                    }
                                }
                                break;
                            case ConectionType.East:
                                {
                                    switch (MasCanvas[i, j].rotateTransform.Angle % 360)
                                    {
                                        case 180: DoPipeDate(MasCanvas[i, j], 180, ConectionType.South, PipeImageLWater, -1, 0); break;
                                        case 270: DoPipeDate(MasCanvas[i, j], 270, ConectionType.North, PipeImageLWater, 1, 0); break;
                                        default: ShowMessage(messageLose); break;
                                    }
                                }
                                break;
                            case ConectionType.South:
                                {
                                    switch (MasCanvas[i, j].rotateTransform.Angle % 360)
                                    {
                                        case 270: DoPipeDate(MasCanvas[i, j], 270, ConectionType.West, PipeImageLWater, 0, 1); break;
                                        case 0: DoPipeDate(MasCanvas[i, j], 0, ConectionType.East, PipeImageLWater, 0, -1); break;
                                        default: ShowMessage(messageLose); break;
                                    }
                                }
                                break;
                            case ConectionType.West:
                                {
                                    switch (MasCanvas[i, j].rotateTransform.Angle % 360)
                                    {
                                        case 0: DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, PipeImageLWater, 1, 0); break;
                                        case 90: DoPipeDate(MasCanvas[i, j], 90, ConectionType.South, PipeImageLWater, -1, 0); break;
                                        default: ShowMessage(messageLose); break;
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
                                        DoPipeDate(MasCanvas[i, j], 0, ConectionType.North, PipeImageIWater, 1, 0);
                                    }
                                    else ShowMessage(messageLose);
                                }
                                break;
                            case ConectionType.East:
                                {
                                    if (MasCanvas[i, j].rotateTransform.Angle % 180 == 90)
                                    {
                                        DoPipeDate(MasCanvas[i, j], 90, ConectionType.East, PipeImageIWater, 0, -1);
                                    }
                                    else ShowMessage(messageLose);
                                }
                                break;
                            case ConectionType.South:
                                {
                                    if (MasCanvas[i, j].rotateTransform.Angle % 180 == 0)
                                    {
                                        DoPipeDate(MasCanvas[i, j], 0, ConectionType.South, PipeImageIWater, -1, 0);
                                    }
                                    else ShowMessage(messageLose);
                                }
                                break;
                            case ConectionType.West:
                                {
                                    if (MasCanvas[i, j].rotateTransform.Angle % 180 == 90)
                                    {
                                        DoPipeDate(MasCanvas[i, j], 90, ConectionType.West, PipeImageIWater, 0, 1);
                                    }
                                    else ShowMessage(messageLose);
                                }
                                break;
                        }
                    }
                }
            }
        }//Timer_tick        
    }
}

//оптимизация повторяющегося кода
//отделить модель и вывод
//интернационализация
