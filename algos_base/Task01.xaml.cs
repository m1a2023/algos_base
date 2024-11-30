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

namespace algos_base
{
    /// <summary>
    /// Interaction logic for Task01.xaml
    /// </summary>
    public partial class Task01 : Page 
    {
        public Task01()
        {
            InitializeComponent();
        }

        private async void OnSelectionSortClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseInput(out int[] array)) return;

            LogListBox.Items.Clear();
            await SelectionSort(array);
            MessageBox.Show("Sorting Completed!", "Success");
        }

        private async void OnInsertionSortClick(object sender, RoutedEventArgs e)
        {
            if (!TryParseInput(out int[] array)) return;

            LogListBox.Items.Clear();
            await InsertionSort(array);
            MessageBox.Show("Sorting Completed!", "Success");
        }

        private async Task SelectionSort(int[] array)
        {
            for (int i = 0; i < array.Length - 1; i++)
            {
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
                    Log($"Array: {string.Join(", ", array)}");
                }

                await Delay();
            }
        }

        private async Task InsertionSort(int[] array)
        {
            for (int i = 1; i < array.Length; i++)
            {
                int key = array[i];
                int j = i - 1;

                while (j >= 0 && array[j] > key)
                {
                    Log($"Comparing: {array[j]} and {key}");
                    array[j + 1] = array[j];
                    j--;

                    Log($"Array: {string.Join(", ", array)}");
                    await Delay();
                }

                array[j + 1] = key;
                Log($"Inserted {key} at position {j + 1}");
                Log($"Array: {string.Join(", ", array)}");

                await Delay();
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
            string input = InputTextBox.Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Please enter a valid array.", "Error");
                return false;
            }

            try
            {
                array = input.Split(' ').Select(int.Parse).ToArray();
                return true;
            }
            catch
            {
                MessageBox.Show("Invalid input format. Please enter numbers separated by spaces.", "Error");
                return false;
            }
        }
        
        private void InputTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            PlaceholderLabel.Visibility = string.IsNullOrEmpty(InputTextBox.Text) ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
