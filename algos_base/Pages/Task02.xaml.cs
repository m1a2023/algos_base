using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace algos_base
{
    public partial class Task02 : Page
    {
        private string _filePath;
        private int _delay;
        private bool isPaused = false;
        private bool isSorting = false;
        private bool isBackPressed = true;

        public Task02()
        {
            InitializeComponent();
            _delay = (int)DelaySlider.Value;
            DelaySlider.ValueChanged += DelaySlider_ValueChanged;
        }
        
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SortingCanvas.Width = e.NewSize.Width;
            SortingCanvas.Height = e.NewSize.Height;
        }
        
        private void PreviousPageButtonClick(object sender, RoutedEventArgs e)
        {
            isBackPressed = true;
            NavigationService.GoBack();
        }

        private void DelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _delay = (int)e.NewValue;
        }

        private void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.Items.Add("Browse button clicked. Opening file dialog...\n");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                _filePath = openFileDialog.FileName;
                LogTextBox.Items.Add($"File selected: {_filePath}\n");
            }
            else
            {
                LogTextBox.Items.Add("No file selected.\n");
            }
        }

        private async void OnStartSortingClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.Items.Add("Start Sorting button clicked.\n");

            if (string.IsNullOrEmpty(_filePath))
            {
                MessageBox.Show("Please select a file first.");
                LogTextBox.Items.Add("Error: No file selected.\n");
                return;
            }

            string selectedMethod = ((ComboBoxItem)SortingMethodComboBox.SelectedItem)?.Content?.ToString();
            string keyAttribute = KeyAttributeTextBox.Text;

            if (string.IsNullOrEmpty(selectedMethod) || string.IsNullOrEmpty(keyAttribute))
            {
                MessageBox.Show("Please select sorting method and key attribute.");
                LogTextBox.Items.Add("Error: Sorting method or key attribute is not selected.\n");
                return;
            }

            try
            {
                var lines = File.ReadAllLines(_filePath).ToList();
                var header = lines.First();
                lines = lines.Skip(1).ToList();

                var columns = header.Split(',');
                int keyIndex = Array.IndexOf(columns, keyAttribute);

                if (keyIndex == -1)
                {
                    MessageBox.Show("Key attribute not found in header.");
                    LogTextBox.Items.Add("Error: Key attribute not found in header.\n");
                    return;
                }

                LogTextBox.Items.Add($"Sorting method selected: {selectedMethod}\n");
                LogTextBox.Items.Add($"Key attribute: {keyAttribute}\n");

                switch (selectedMethod)
                {
                    case "Natural Merge":
                        await NaturalMergeSort(lines, keyIndex);
                        break;
                    case "Direct Merge":
                        await DirectMergeSort(lines, keyIndex);
                        break;
                    case "Heap Sort":
                        await HeapSort(lines, keyIndex);
                        break;
                    default:
                        LogTextBox.Items.Add("Error: Unsupported sorting method.\n");
                        return;
                }

                lines.Insert(0, header); // Add header back to the sorted data
                string sortedFilePath = Path.Combine(Path.GetDirectoryName(_filePath), "sorted_" + Path.GetFileName(_filePath));
                File.WriteAllLines(sortedFilePath, lines);
                LogTextBox.Items.Add($"Sorted file saved at {sortedFilePath}\n");
            }
            catch (Exception ex)
            {
                LogTextBox.Items.Add($"Error during sorting: {ex.Message}\n");
            }
        }

        private async Task NaturalMergeSort(List<string> lines, int keyIndex)
        {
            LogTextBox.Items.Add("Natural Merge Sort started...\n");

            bool sorted = false;

            while (!sorted)
            {
                List<List<string>> runs = new List<List<string>>();
                List<string> currentRun = new List<string> { lines[0] };

                for (int i = 1; i < lines.Count; i++)
                {
                    var currentKey = lines[i].Split(',')[keyIndex];
                    var previousKey = lines[i - 1].Split(',')[keyIndex];

                    if (CompareKeys(currentKey, previousKey) >= 0)
                    {
                        currentRun.Add(lines[i]);
                    }
                    else
                    {
                        runs.Add(new List<string>(currentRun));
                        currentRun.Clear();
                        currentRun.Add(lines[i]);
                    }
                }

                runs.Add(currentRun);

                if (runs.Count == 1)
                {
                    sorted = true;
                }
                else
                {
                    lines = MergeRuns(runs, keyIndex);
                    await Task.Delay(_delay);
                }
            }

            LogTextBox.Items.Add("Natural Merge Sort completed.\n");
        }

        private int CompareKeys(string key1, string key2)
        {
            bool isNumeric1 = double.TryParse(key1, out double num1);
            bool isNumeric2 = double.TryParse(key2, out double num2);

            if (isNumeric1 && isNumeric2)
            {
                return num1.CompareTo(num2);
            }
            else
            {
                return string.Compare(key1, key2);
            }
        }

        private List<string> MergeRuns(List<List<string>> runs, int keyIndex)
        {
            while (runs.Count > 1)
            {
                List<string> leftRun = runs[0];
                List<string> rightRun = runs[1];
                List<string> mergedRun = MergeTwoRuns(leftRun, rightRun, keyIndex);
                runs.RemoveAt(0);
                runs.RemoveAt(0);
                runs.Add(mergedRun);
            }

            return runs[0];
        }

        private List<string> MergeTwoRuns(List<string> left, List<string> right, int keyIndex)
        {
            List<string> merged = new List<string>();
            int i = 0, j = 0;

            while (i < left.Count && j < right.Count)
            {
                string leftKey = left[i].Split(',')[keyIndex];
                string rightKey = right[j].Split(',')[keyIndex];

                if (CompareKeys(leftKey, rightKey) <= 0)
                {
                    merged.Add(left[i++]);
                }
                else
                {
                    merged.Add(right[j++]);
                }
            }

            merged.AddRange(left.Skip(i));
            merged.AddRange(right.Skip(j));

            return merged;
        }

        private async Task DirectMergeSort(List<string> lines, int keyIndex)
        {
            LogTextBox.Items.Add("Direct Merge Sort started...\n");

            int n = lines.Count;
            for (int width = 1; width < n; width *= 2)
            {
                for (int i = 0; i < n; i += 2 * width)
                {
                    int mid = Math.Min(i + width, n);
                    int right = Math.Min(i + 2 * width, n);

                    MergeInto(lines, i, mid, right, keyIndex);
                    await Task.Delay(_delay);
                }
            }

            LogTextBox.Items.Add("Direct Merge Sort completed.\n");
        }

        private async Task HeapSort(List<string> lines, int keyIndex)
        {
            LogTextBox.Items.Add("Heap Sort started...\n");

            int n = lines.Count;

            for (int i = n / 2 - 1; i >= 0; i--)
            {
                await Heapify(lines, n, i, keyIndex);
            }

            for (int i = n - 1; i > 0; i--)
            {
                (lines[0], lines[i]) = (lines[i], lines[0]);
                await Heapify(lines, i, 0, keyIndex);
                await Task.Delay(_delay);
            }

            LogTextBox.Items.Add("Heap Sort completed.\n");
        }

        private void MergeInto(List<string> lines, int left, int mid, int right, int keyIndex)
        {
            List<string> temp = new List<string>();
            int i = left, j = mid;

            while (i < mid && j < right)
            {
                string leftKey = lines[i].Split(',')[keyIndex];
                string rightKey = lines[j].Split(',')[keyIndex];

                if (CompareKeys(leftKey, rightKey) <= 0)
                {
                    temp.Add(lines[i++]);
                }
                else
                {
                    temp.Add(lines[j++]);
                }
            }

            temp.AddRange(lines.Skip(i).Take(mid - i));
            temp.AddRange(lines.Skip(j).Take(right - j));

            for (int k = 0; k < temp.Count; k++)
            {
                lines[left + k] = temp[k];
            }
        }

        private async Task Heapify(List<string> lines, int n, int i, int keyIndex)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            string largestKey = lines[largest].Split(',')[keyIndex];

            if (left < n && CompareKeys(lines[left].Split(',')[keyIndex], largestKey) > 0)
            {
                largest = left;
            }

            if (right < n && CompareKeys(lines[right].Split(',')[keyIndex], lines[largest].Split(',')[keyIndex]) > 0)
            {
                largest = right;
            }

            if (largest != i)
            {
                (lines[i], lines[largest]) = (lines[largest], lines[i]);
                await Heapify(lines, n, largest, keyIndex);
            }
        }
    }
}

