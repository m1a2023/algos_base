using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace algos_base
{
    public partial class Task01 : Page
    {
        private Rectangle[] rectangles;
        private int[] numbers;
        private bool isPaused = false;
        private bool isSorting = false;
        private bool isBackPressed = false;
        private double _delay = 500;

        public Task01()
        {
            InitializeComponent();
            PauseButton.IsEnabled = false;
        }
        
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateBarPositions();
        }
        
        private async Task Delay()
        {
            int delay = (int)DelaySlider.Value;
            await Task.Delay(delay);
        }

        private void PreviousPageButtonClick(object sender, RoutedEventArgs e)
        {
            isBackPressed = true;
            NavigationService.GoBack();
        }
        
        private void CountTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (CountTextBox.Text == "Elements amount")
            {
                CountTextBox.Text = "";
                CountTextBox.Foreground = Brushes.Black;
            }
        }

        private void CountTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CountTextBox.Text))
            {
                CountTextBox.Text = "Elements amount";
                CountTextBox.Foreground = Brushes.Gray;
            }
        }
        
        private async void OnSelectionSortClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseInput(out int[] array)) return;

            numbers = array;
            rectangles = new Rectangle[numbers.Length];
            SortingCanvas.Children.Clear();
            
            DrawBars();

            LogListBox.Items.Clear();
            
            SetButtonsEnabled(false);

            isSorting = true;
            PauseButton.IsEnabled = true;

            await SelectionSort(numbers);
            isSorting = false;
            
            SetButtonsEnabled(true);
            
            PauseButton.IsEnabled = false;

            MessageBox.Show("Sorting Completed!", "Success");
        }

        private async void OnInsertionSortClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseInput(out int[] array)) return;

            numbers = array;
            rectangles = new Rectangle[numbers.Length];
            SortingCanvas.Children.Clear();
            
            DrawBars();

            LogListBox.Items.Clear();
            
            SetButtonsEnabled(false);

            isSorting = true;
            PauseButton.IsEnabled = true;

            await InsertionSort(numbers);
            isSorting = false;
            
            SetButtonsEnabled(true);
            
            PauseButton.IsEnabled = false;

            MessageBox.Show("Sorting Completed!", "Success");
        }
        
        private async void OnHeapSortClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseInput(out int[] array)) return;

            numbers = array;
            rectangles = new Rectangle[numbers.Length];
            SortingCanvas.Children.Clear();
    
            DrawBars();

            LogListBox.Items.Clear();
    
            SetButtonsEnabled(false);

            isSorting = true;
            PauseButton.IsEnabled = true;

            await HeapSort(numbers);
            isSorting = false;
    
            SetButtonsEnabled(true);
    
            PauseButton.IsEnabled = false;

            MessageBox.Show("Sorting Completed!", "Success");
        }

        private async void OnQuickSortClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseInput(out int[] array)) return;

            numbers = array;
            rectangles = new Rectangle[numbers.Length];
            SortingCanvas.Children.Clear();
    
            DrawBars();

            LogListBox.Items.Clear();
    
            SetButtonsEnabled(false);

            isSorting = true;
            PauseButton.IsEnabled = true;

            await QuickSort(numbers, 0, numbers.Length - 1);
            isSorting = false;
    
            SetButtonsEnabled(true);
    
            PauseButton.IsEnabled = false;

            MessageBox.Show("Sorting Completed!", "Success");
        }
        
        private async Task HeapSort(int[] array)
        {
            int n = array.Length;

            // Построение кучи (rearrange array)
            for (int i = n / 2 - 1; i >= 0; i--)
            {
                await Heapify(array, n, i);
            }

            // Один за другим извлекаем элементы из кучи
            for (int i = n - 1; i > 0; i--)
            {
                if (isBackPressed) return;
                if (isPaused) await WaitForResume();

                // Окрашиваем элементы, которые меняются местами, в красный
                rectangles[0].Fill = Brushes.Red;
                rectangles[i].Fill = Brushes.Red;

                // Обмен значениями
                (array[0], array[i]) = (array[i], array[0]);
                UpdateBarPositions();

                await Delay();

                // После обмена возвращаем элементы в синий цвет
                rectangles[0].Fill = Brushes.Blue;
                rectangles[i].Fill = Brushes.Green; // Элемент отсортирован

                // Восстанавливаем кучу для оставшихся элементов
                await Heapify(array, i, 0);
            }

            // Окрашиваем последний элемент в зеленый
            rectangles[0].Fill = Brushes.Green;
        }

        private async Task Heapify(int[] array, int n, int i)
        {
            int largest = i; // Инициализируем наибольший элемент как корень
            int left = 2 * i + 1; // левый = 2*i + 1
            int right = 2 * i + 2; // правый = 2*i + 2

            // Если левый дочерний больше корня
            if (left < n && array[left] > array[largest])
            {
                largest = left;
            }

            // Если правый дочерний больше, чем наибольший элемент
            if (right < n && array[right] > array[largest])
            {
                largest = right;
            }

            // Если наибольший элемент не корень
            if (largest != i)
            {
                // Окрашиваем элементы, которые меняются местами, в красный
                rectangles[i].Fill = Brushes.Red;
                rectangles[largest].Fill = Brushes.Red;

                // Обмен значениями
                (array[i], array[largest]) = (array[largest], array[i]);
                UpdateBarPositions();

                await Delay();

                // После обмена возвращаем элементы в синий
                rectangles[i].Fill = Brushes.Blue;
                rectangles[largest].Fill = Brushes.Blue;

                // Рекурсивно восстанавливаем кучу
                await Heapify(array, n, largest);
            }
        }
        private async Task QuickSort(int[] array, int low, int high)
        {
            if (isBackPressed) return;
            if (isPaused) await WaitForResume();

            if (low < high)
            {
                int pivotIndex = await Partition(array, low, high);

                // Рекурсивно сортируем элементы до и после разделителя
                await QuickSort(array, low, pivotIndex - 1);
                await QuickSort(array, pivotIndex + 1, high);

                // Окрашиваем элемент, находящийся в позиции pivot, в зеленый (отсортирован)
                rectangles[pivotIndex].Fill = Brushes.Green;
            }
            else if (low == high)
            {
                // Когда один элемент остался на месте, его можно считать отсортированным
                rectangles[low].Fill = Brushes.Green;
            }
        }

        private async Task<int> Partition(int[] array, int low, int high)
        {
            int pivot = array[high];
            rectangles[high].Fill = Brushes.Yellow; // Окрашиваем опорный элемент
            await Delay();

            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (isBackPressed) return i;
                if (isPaused) await WaitForResume();

                // Окрашиваем текущий элемент, который сравнивается, в желтый
                rectangles[j].Fill = Brushes.Yellow;
                await Delay();

                if (array[j] < pivot)
                {
                    i++;
                    Swap(array, i, j);
                    UpdateBarPositions();
                    await Delay();

                    // Окрашиваем обработанный элемент в синий, если он не является опорным
                    rectangles[i].Fill = Brushes.Blue;
                }

                // Возвращаем цвет текущего элемента обратно
                rectangles[j].Fill = Brushes.Blue;
            }

            // Меняем местами опорный элемент с элементом в позиции (i+1)
            Swap(array, i + 1, high);
            UpdateBarPositions();
            await Delay();

            // Возвращаем цвет опорного элемента после сортировки
            rectangles[high].Fill = Brushes.Blue;

            return i + 1;
        }

        private async Task Swap(int[] array, int i, int j)
        {
            // Меняем элементы в массиве
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;

            // Обновляем визуальные блоки (прямоугольники) в интерфейсе
            double tempHeight = rectangles[i].Height;
            rectangles[i].Height = rectangles[j].Height;
            rectangles[j].Height = tempHeight;

            double tempTop = Canvas.GetTop(rectangles[i]);
            Canvas.SetTop(rectangles[i], Canvas.GetTop(rectangles[j]));
            Canvas.SetTop(rectangles[j], tempTop);

            // Добавляем задержку, чтобы обновления визуализации успели отобразиться
            await Task.Delay(100); // Adjust the delay as needed
        }

        private void DrawBars()
        {
            double barWidth = SortingCanvas.ActualWidth / numbers.Length;
            double canvasHeight = SortingCanvas.ActualHeight;

            // Ограничиваем минимальную ширину бара, чтобы избежать ошибок
            double minBarWidth = 5; // минимальная ширина бара
            barWidth = Math.Max(barWidth, minBarWidth); // Применяем минимальную ширину

            for (int i = 0; i < numbers.Length; i++)
            {
                // Adjust the initial bar height relative to the canvas height
                double barHeight = numbers[i] * (canvasHeight / 100);
        
                // Проверяем, что ширина и высота бара валидны
                if (barWidth <= 0 || barHeight <= 0)
                {
                    continue; // Пропускаем такие элементы
                }

                var rect = new Rectangle
                {
                    Width = barWidth - 2,
                    Height = barHeight,
                    Fill = Brushes.Blue
                };

                Canvas.SetLeft(rect, i * barWidth);
                Canvas.SetBottom(rect, 0);
                SortingCanvas.Children.Add(rect);
                rectangles[i] = rect;
            }
        }

        private async Task SelectionSort(int[] array)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
                if (isBackPressed) return;
                if (isPaused) await WaitForResume();

                int minIndex = i;

                // Окрашиваем текущий элемент в желтый (он будет сравниваться)
                rectangles[i].Fill = Brushes.Yellow;

                for (int j = i + 1; j < array.Length; j++)
                {
                    Log($"Comparing: {array[j]} and {array[minIndex]}");

                    // Окрашиваем текущий элемент для сравнения в желтый
                    rectangles[j].Fill = Brushes.Yellow;

                    // Если новый элемент меньше, меняем минимальный индекс
                    if (array[j] < array[minIndex])
                    {
                        // Сбрасываем старый минимальный элемент в синий
                        rectangles[minIndex].Fill = Brushes.Blue;
                        minIndex = j;

                        // Окрашиваем новый минимальный элемент в желтый
                        rectangles[minIndex].Fill = Brushes.Yellow;
                    }
                    else
                    {
                        // Если элементы не обменялись, возвращаем их в синий
                        rectangles[j].Fill = Brushes.Blue;
                    }

                    // Задержка для визуализации
                    await Delay();
                }

                // Если минимальный элемент поменялся, меняем их местами
                if (minIndex != i)
                {
                    Log($"Swapping: {array[i]} and {array[minIndex]}");

                    // Окрашиваем элементы, которые меняются местами, в красный
                    rectangles[i].Fill = Brushes.Red;
                    rectangles[minIndex].Fill = Brushes.Red;

                    // Обмен значениями в массиве
                    (array[i], array[minIndex]) = (array[minIndex], array[i]);
                    UpdateBarPositions();

                    Log($"Array: {string.Join(", ", array)}");

                    await Delay(); // Задержка для визуализации обмена

                    // После обмена возвращаем элементы в синий цвет
                    rectangles[i].Fill = Brushes.Blue;
                    rectangles[minIndex].Fill = Brushes.Blue;
                }

                // Окрашиваем отсортированный элемент в зеленый
                rectangles[i].Fill = Brushes.Green;

                // Убираем жёлтую окраску с минимального элемента (если обмен был)
                if (minIndex != i)
                {
                    rectangles[minIndex].Fill = Brushes.Blue;
                }

                await Delay();
            }

            // Окрашиваем последний элемент в зеленый
            rectangles[array.Length - 1].Fill = Brushes.Green;
        }

        private async Task InsertionSort(int[] array)
        {
            for (int i = 1; i < array.Length; i++)
            {
                if (isBackPressed) return;
                if (isPaused) await WaitForResume();

                int key = array[i];
                int j = i - 1;

                // Окрашиваем текущий элемент (который вставляется) в желтый
                rectangles[i].Fill = Brushes.Yellow;
                await Delay();

                while (j >= 0 && array[j] > key)
                {
                    // Окрашиваем сравниваемый элемент в желтый
                    rectangles[j].Fill = Brushes.Yellow;

                    // Сдвиг элементов вправо
                    array[j + 1] = array[j];
                    UpdateBarPositions();
                    await Delay();

                    // Возвращаем цвет обратно в синий для обработанного элемента
                    rectangles[j].Fill = Brushes.Blue;

                    j--;
                }

                // Вставляем элемент на правильное место
                array[j + 1] = key;
                UpdateBarPositions();

                // Окрашиваем вставленный элемент в красный
                rectangles[j + 1].Fill = Brushes.Red;
                await Delay();

                // Возвращаем цвет вставленного элемента в зеленый (отсортирован)
                rectangles[j + 1].Fill = Brushes.Green;
            }

            // Окрашиваем все элементы в зеленый, когда сортировка завершена
            for (int i = 0; i < array.Length; i++)
            {
                rectangles[i].Fill = Brushes.Green;
            }
        }
        private void UpdateBarPositions()
        {
            // Убедитесь, что длина массива numbers и rectangles совпадает
            if (rectangles == null || rectangles.Length != numbers.Length)
            {
                return;
            }

            for (int i = 0; i < numbers.Length; i++) 
            {
                // Если массив numbers пуст, пропустите итерацию
                if (numbers.Length == 0) return;

                // Получаем высоту баров, пропорциональную величине числа
                double barHeight = numbers[i] * (SortingCanvas.ActualHeight / 100);

                // Если прямоугольник не был инициализирован, пропустите его
                if (rectangles[i] == null)
                {
                    continue;
                }

                // Обновляем высоту прямоугольника
                rectangles[i].Height = barHeight;
        
                // Устанавливаем позицию прямоугольника по оси X (горизонтальное расположение)
                Canvas.SetLeft(rectangles[i], i * (SortingCanvas.ActualWidth / numbers.Length));

                // Устанавливаем позицию по оси Y (вертикальное расположение, начиная с низа)
                Canvas.SetBottom(rectangles[i], 0);
            }
        }


        private bool TryParseInput(out int[] array)
        {
            array = null;
            string input = CountTextBox.Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Please enter a valid array.", "Error");
                return false;
            }

            try
            {
                int n = int.Parse(input);

                // Check if the value exceeds 1000
                if (n > 1000)
                {
                    MessageBox.Show("The number of elements cannot exceed 1000.", "Error");
                    return false;
                }

                array = GenerateRandomArray(n);
                return true;
            }
            catch
            {
                MessageBox.Show("Invalid input format. Please enter a valid number.", "Error");
                return false;
            }
        }
        
        private int[] GenerateRandomArray(int n)
        {
            Random rand = new Random();
            return Enumerable.Range(0, n).Select(x => rand.Next(1, 100)).ToArray();
        }


        private void SetButtonsEnabled(bool isEnabled)
        {
            SelectionSortButton.IsEnabled = isEnabled;
            InsertionSortButton.IsEnabled = isEnabled;
            HeapSortButton.IsEnabled = isEnabled;
            QuickSortButton.IsEnabled = isEnabled;
        }

        private void Log(string message)
        {
            LogListBox.Items.Insert(0, message);
        }

        private async Task WaitForResume()
        {
            while (isPaused)
            {
                await Task.Delay(100);
            }
        }
        private void OnPauseButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isSorting) return;

            isPaused = !isPaused;

            PauseButton.Content = isPaused ? "Resume" : "Pause";
        }
    }
}
