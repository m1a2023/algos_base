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
            for (int i = n / 2 - 1; i >= 0; i--)
            {
                Log($"Heapifying at index {i}");
                await Heapify(array, n, i);
            }

            for (int i = n - 1; i > 0; i--)
            {
                if (isBackPressed) return;
                if (isPaused) await WaitForResume();
                
                Log($"Swapping root {array[0]} with element {array[i]} at index {i}");
                rectangles[0].Fill = Brushes.Red;
                rectangles[i].Fill = Brushes.Red;
                (array[0], array[i]) = (array[i], array[0]);
                UpdateBarPositions();

                Log($"Array: {string.Join(", ", array)}");
                await Delay();
                rectangles[0].Fill = Brushes.Blue;
                rectangles[i].Fill = Brushes.Green;
                await Heapify(array, i, 0);
            }
            rectangles[0].Fill = Brushes.Green;
        }

        private async Task Heapify(int[] array, int n, int i)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;
            if (left < n && array[left] > array[largest])
            {
                largest = left;
            }
            if (right < n && array[right] > array[largest])
            {
                largest = right;
            }
            if (largest != i)
            {
                rectangles[i].Fill = Brushes.Red;
                rectangles[largest].Fill = Brushes.Red;
                (array[i], array[largest]) = (array[largest], array[i]);
                UpdateBarPositions();

                await Delay();
                rectangles[i].Fill = Brushes.Blue;
                rectangles[largest].Fill = Brushes.Blue;
                await Heapify(array, n, largest);
            }
        }
        private async Task QuickSort(int[] array, int low, int high)
        {
            if (isBackPressed) return;
            if (isPaused) await WaitForResume();

            if (low < high)
            {
                Log($"QuickSort called on range [{low}, {high}]");
                int pivotIndex = await Partition(array, low, high);
                Log($"Pivot element {array[pivotIndex]} placed at index {pivotIndex}");
                await QuickSort(array, low, pivotIndex - 1);
                await QuickSort(array, pivotIndex + 1, high);
                rectangles[pivotIndex].Fill = Brushes.Green;
            }
            else if (low == high)
            {
                rectangles[low].Fill = Brushes.Green;
            }
        }

        private async Task<int> Partition(int[] array, int low, int high)
        {
            int pivot = array[high];
            rectangles[high].Fill = Brushes.Yellow;
            await Delay();

            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (isBackPressed) return i;
                if (isPaused) await WaitForResume();
                rectangles[j].Fill = Brushes.Yellow;
                await Delay();
                Log($"Comparing element {array[j]} with pivot {pivot}");


                if (array[j] < pivot)
                {
                    i++;
                    Log($"Swapping element {array[i]} at index {i} with {array[j]} at index {j}");
                    Swap(array, i, j);
                    UpdateBarPositions();
                    await Delay();
                    rectangles[i].Fill = Brushes.Blue;
                }
                rectangles[j].Fill = Brushes.Blue;
            }

            Log($"Swapping pivot {array[high]} with element {array[i + 1]} at index {i + 1}");
            Swap(array, i + 1, high);
            UpdateBarPositions();
            await Delay();
            rectangles[high].Fill = Brushes.Blue;
            Log($"Array: {string.Join(", ", array)}");


            return i + 1;
        }

        private async Task Swap(int[] array, int i, int j)
        {
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
            double tempHeight = rectangles[i].Height;
            rectangles[i].Height = rectangles[j].Height;
            rectangles[j].Height = tempHeight;

            double tempTop = Canvas.GetTop(rectangles[i]);
            Canvas.SetTop(rectangles[i], Canvas.GetTop(rectangles[j]));
            Canvas.SetTop(rectangles[j], tempTop);
            await Task.Delay(100);
        }

        private void DrawBars()
        {
            double barWidth = SortingCanvas.ActualWidth / numbers.Length;
            double canvasHeight = SortingCanvas.ActualHeight;
            double minBarWidth = 5;
            barWidth = Math.Max(barWidth, minBarWidth);

            for (int i = 0; i < numbers.Length; i++)
            {
                double barHeight = numbers[i] * (canvasHeight / 100);
                if (barWidth <= 0 || barHeight <= 0)
                {
                    continue;
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
                rectangles[i].Fill = Brushes.Yellow;

                for (int j = i + 1; j < array.Length; j++)
                {
                    Log($"Comparing: {array[j]} and {array[minIndex]}");
                    rectangles[j].Fill = Brushes.Yellow;
                    if (array[j] < array[minIndex])
                    {
                        rectangles[minIndex].Fill = Brushes.Blue;
                        minIndex = j;
                        rectangles[minIndex].Fill = Brushes.Yellow;
                    }
                    else
                    {
                        rectangles[j].Fill = Brushes.Blue;
                    }
                    await Delay();
                }
                
                if (minIndex != i)
                {
                    Log($"Swapping: {array[i]} and {array[minIndex]}");
                    rectangles[i].Fill = Brushes.Red;
                    rectangles[minIndex].Fill = Brushes.Red;
                    
                    (array[i], array[minIndex]) = (array[minIndex], array[i]);
                    UpdateBarPositions();

                    Log($"Array: {string.Join(", ", array)}");

                    await Delay();
                    rectangles[i].Fill = Brushes.Blue;
                    rectangles[minIndex].Fill = Brushes.Blue;
                }
                
                rectangles[i].Fill = Brushes.Green;
                if (minIndex != i)
                {
                    rectangles[minIndex].Fill = Brushes.Blue;
                }

                await Delay();
            }
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
                
                rectangles[i].Fill = Brushes.Yellow;
                await Delay();

                while (j >= 0 && array[j] > key)
                {
                    Log($"Comparing: {array[j]} and {key}");
                    
                    rectangles[j].Fill = Brushes.Yellow;
                    
                    array[j + 1] = array[j];
                    UpdateBarPositions();
                    
                    Log($"Array: {string.Join(", ", array)}");
                    
                    await Delay();
                    
                    rectangles[j].Fill = Brushes.Blue;

                    j--;
                }
                
                array[j + 1] = key;
                Log($"Inserted {key} at position {j + 1}");
                
                UpdateBarPositions();
                
                Log($"Array: {string.Join(", ", array)}");
                
                rectangles[j + 1].Fill = Brushes.Red;
                
                await Delay();
                
                rectangles[j + 1].Fill = Brushes.Green;
            }
            
            for (int i = 0; i < array.Length; i++)
            {
                rectangles[i].Fill = Brushes.Green;
            }
        }
        private void UpdateBarPositions()
        {
            if (rectangles == null || rectangles.Length != numbers.Length)
            {
                return;
            }

            for (int i = 0; i < numbers.Length; i++) 
            {
                if (numbers.Length == 0) return;
                double barHeight = numbers[i] * (SortingCanvas.ActualHeight / 100);

                if (rectangles[i] == null)
                {
                    continue;
                }
                rectangles[i].Height = barHeight;
                Canvas.SetLeft(rectangles[i], i * (SortingCanvas.ActualWidth / numbers.Length));
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
