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

        public Task01()
        {
            InitializeComponent();
            PauseButton.IsEnabled = false;
        }
        
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SortingCanvas.Width = e.NewSize.Width;
            SortingCanvas.Height = e.NewSize.Height;
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

        private void DrawBars()
        {
            double barWidth = SortingCanvas.ActualWidth / numbers.Length;

            for (int i = 0; i < numbers.Length; i++)
            {
                double barHeight = numbers[i] * 2;
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

                if (isPaused)
                {
                    await WaitForResume();
                }

                int minIndex = i;
                for (int j = i + 1; j < array.Length; j++)
                {
                    Log($"Comparing: {array[j]} and {array[minIndex]}");
                    if (array[j] < array[minIndex])
                    {
                        minIndex = j;
                    }

                    await Delay();
                }

                if (minIndex != i)
                {
                    Log($"Swapping: {array[i]} and {array[minIndex]}");
                    (array[i], array[minIndex]) = (array[minIndex], array[i]);
                    
                    UpdateBarPositions();
                    Log($"Array: {string.Join(", ", array)}");
                }

                await Delay();
            }
        }

        private async Task InsertionSort(int[] array)
        {
            for (int i = 1; i < array.Length; i++)
            {
                if (isBackPressed) return;

                if (isPaused)
                {
                    await WaitForResume();
                }

                int key = array[i];
                int j = i - 1;

                while (j >= 0 && array[j] > key)
                {
                    Log($"Comparing: {array[j]} and {key}");
                    array[j + 1] = array[j];
                    j--;
                    
                    UpdateBarPositions();
                    Log($"Array: {string.Join(", ", array)}");
                    await Delay();
                }

                array[j + 1] = key;
                Log($"Inserted {key} at position {j + 1}");
                
                UpdateBarPositions();
                Log($"Array: {string.Join(", ", array)}");

                await Delay();
            }
        }

        private void UpdateBarPositions()
        {
            double barWidth = SortingCanvas.ActualWidth / numbers.Length;

            for (int i = 0; i < numbers.Length; i++)
            {
                Canvas.SetLeft(rectangles[i], i * barWidth);
                rectangles[i].Height = numbers[i] * 2;
            }
        }

        private void Log(string message)
        {
            LogListBox.Items.Add(message);
            LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]);
        }

        private async Task Delay()
        {
            int delay = (int)DelaySlider.Value;
            await Task.Delay(delay);
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

        private void SetButtonsEnabled(bool enabled)
        {
            SelectionSortButton.IsEnabled = enabled;
            InsertionSortButton.IsEnabled = enabled;
            PauseButton.IsEnabled = enabled;
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


        private void OnPauseButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isSorting) return;

            isPaused = !isPaused;

            PauseButton.Content = isPaused ? "Resume" : "Pause";
        }

        private async Task WaitForResume()
        {
            while (isPaused)
            {
                await Task.Delay(100);
            }
        }
        private void PreviousPageButtonClick(object sender, RoutedEventArgs e)
        {
            isBackPressed = true;
            NavigationService.GoBack();
        }
    }
}
