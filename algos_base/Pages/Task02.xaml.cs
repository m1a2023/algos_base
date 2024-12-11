using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace algos_base
{
    public partial class Task02 : Page
    {
        private string _filePath;
        private int _delay;

        public Task02()
        {
            InitializeComponent();
            _delay = (int)DelaySlider.Value;
            DelaySlider.ValueChanged += DelaySlider_ValueChanged;
        }
        private void PreviousPageButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void DelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _delay = (int)e.NewValue;
        }

        private void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.AppendText("Browse button clicked. Opening file dialog...\n");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
            if (openFileDialog.ShowDialog() == true)
            {
                _filePath = openFileDialog.FileName;
                LogTextBox.AppendText($"File selected: {_filePath}\n");
                LoadColumnsFromFile(_filePath);
            }
            else
            {
                LogTextBox.AppendText("No file selected.\n");
            }
        }
        private void LoadColumnsFromFile(string filePath)
        {
            try
            {
                var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1);
                var header = worksheet.Row(1).Cells().Select(c => c.Value.ToString()).ToList();
                KeyAttributeComboBox.ItemsSource = header;
                KeyAttributeComboBox.SelectedIndex = 0;

                LogTextBox.AppendText("Columns loaded into ComboBox.\n");
            }
            catch (Exception ex)
            {
                LogTextBox.AppendText($"Error loading columns: {ex.Message}\n");
            }
        }
        private async void OnStartSortingClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.AppendText("Start Sorting button clicked.\n");

            if (string.IsNullOrEmpty(_filePath))
            {
                MessageBox.Show("Please select a file first.");
                LogTextBox.AppendText("Error: No file selected.\n");
                return;
            }

            string selectedMethod = ((ComboBoxItem)SortingMethodComboBox.SelectedItem)?.Content?.ToString();
            string keyAttribute = KeyAttributeComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedMethod) || string.IsNullOrEmpty(keyAttribute))
            {
                MessageBox.Show("Please select sorting method and key attribute.");
                LogTextBox.AppendText("Error: Sorting method or key attribute is not selected.\n");
                return;
            }

            try
            {
                var workbook = new XLWorkbook(_filePath);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1).ToList();

                var header = worksheet.Row(1).Cells().Select(c => c.Value.ToString()).ToList();
                int keyIndex = header.IndexOf(keyAttribute);

                if (keyIndex == -1)
                {
                    MessageBox.Show("Key attribute not found in header.");
                    LogTextBox.AppendText("Error: Key attribute not found in header.\n");
                    return;
                }

                LogTextBox.AppendText($"Sorting method selected: {selectedMethod}\n");
                LogTextBox.AppendText($"Key attribute: {keyAttribute}\n");
                switch (selectedMethod)
                {
                    case "Natural Merge":
                        await DirectMergeSort(rows, keyIndex);
                        break;
                    case "Direct Merge":
                        await DirectMergeSort(rows, keyIndex);
                        break;
                    case "Heap Sort":
                        await HeapSort(rows, keyIndex);
                        break;
                    default:
                        LogTextBox.AppendText("Error: Unsupported sorting method.\n");
                        return;
                }
                
                var sortedFilePath = Path.Combine(Path.GetDirectoryName(_filePath), "sorted_" + Path.GetFileName(_filePath));
                var newWorkbook = new XLWorkbook();
                var newWorksheet = newWorkbook.Worksheets.Add("SortedData");
                for (int i = 0; i < header.Count; i++)
                {
                    newWorksheet.Cell(1, i + 1).Value = header[i];
                }
                for (int rowIdx = 0; rowIdx < rows.Count; rowIdx++)
                {
                    var row = rows[rowIdx];
                    for (int colIdx = 0; colIdx < row.Cells().Count(); colIdx++)
                    {
                        newWorksheet.Cell(rowIdx + 2, colIdx + 1).Value = row.Cell(colIdx + 1).Value;
                    }
                }

                newWorkbook.SaveAs(sortedFilePath);
                LogTextBox.AppendText($"Sorted file saved at {sortedFilePath}\n");

            }
            catch (Exception ex)
            {
                LogTextBox.AppendText($"Error during sorting: {ex.Message}\n");
            }
        }
        private async Task NaturalMergeSort(List<IXLRow> rows, int keyIndex)
        {

            bool sorted = false;

            while (!sorted)
            {
                List<List<IXLRow>> runs = new List<List<IXLRow>>();
                List<IXLRow> currentRun = new List<IXLRow> { rows[0] };

                for (int i = 1; i < rows.Count; i++)
                {
                    var currentKey = rows[i].Cell(keyIndex + 1).Value.ToString();
                    var previousKey = rows[i - 1].Cell(keyIndex + 1).Value.ToString();

                    if (CompareKeys(currentKey, previousKey) >= 0)
                    {
                        currentRun.Add(rows[i]);
                    }
                    else
                    {
                        runs.Add(new List<IXLRow>(currentRun));
                        currentRun.Clear();
                        currentRun.Add(rows[i]);
                    }
                }

                runs.Add(currentRun);

                if (runs.Count == 1)
                {
                    sorted = true;
                }
                else
                {
                    rows = MergeRuns(runs, keyIndex);
                    await Task.Delay(_delay);
                }
            }
            
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

        private List<IXLRow> MergeRuns(List<List<IXLRow>> runs, int keyIndex)
        {
            while (runs.Count > 1)
            {
                List<IXLRow> leftRun = runs[0];
                List<IXLRow> rightRun = runs[1];
                List<IXLRow> mergedRun = MergeTwoRuns(leftRun, rightRun, keyIndex);
                runs.RemoveAt(0);
                runs.RemoveAt(0);
                runs.Add(mergedRun);
            }

            return runs[0];
        }

        private List<IXLRow> MergeTwoRuns(List<IXLRow> left, List<IXLRow> right, int keyIndex)
        {
            List<IXLRow> merged = new List<IXLRow>();
            int i = 0, j = 0;

            while (i < left.Count && j < right.Count)
            {
                string leftKey = left[i].Cell(keyIndex + 1).Value.ToString();
                string rightKey = right[j].Cell(keyIndex + 1).Value.ToString();

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

        private async Task DirectMergeSort(List<IXLRow> rows, int keyIndex)
        {

            int n = rows.Count;
            for (int width = 1; width < n; width *= 2)
            {
                for (int i = 0; i < n; i += 2 * width)
                {
                    int mid = Math.Min(i + width, n);
                    int right = Math.Min(i + 2 * width, n);

                    MergeInto(rows, i, mid, right, keyIndex);
                    await Task.Delay(_delay);
                }
            }
            
        }

        private void MergeInto(List<IXLRow> rows, int left, int mid, int right, int keyIndex)
        {
            List<IXLRow> temp = new List<IXLRow>();
            int i = left, j = mid;

            while (i < mid && j < right)
            {
                string leftKey = rows[i].Cell(keyIndex + 1).Value.ToString();
                string rightKey = rows[j].Cell(keyIndex + 1).Value.ToString();

                if (CompareKeys(leftKey, rightKey) <= 0)
                {
                    temp.Add(rows[i++]);
                }
                else
                {
                    temp.Add(rows[j++]);
                }
            }

            temp.AddRange(rows.Skip(i).Take(mid - i));
            temp.AddRange(rows.Skip(j).Take(right - j));

            for (int k = 0; k < temp.Count; k++)
            {
                rows[left + k] = temp[k];
            }
        }

        private async Task HeapSort(List<IXLRow> rows, int keyIndex)
        {

            int n = rows.Count;
            for (int i = n / 2 - 1; i >= 0; i--)
            {
                await Heapify(rows, n, i, keyIndex);
            }
            for (int i = n - 1; i > 0; i--)
            {
                (rows[0], rows[i]) = (rows[i], rows[0]);
                await Heapify(rows, i, 0, keyIndex);
                await Task.Delay(_delay);
            }
            
        }

        private async Task Heapify(List<IXLRow> rows, int n, int i, int keyIndex)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            string largestKey = rows[largest].Cell(keyIndex + 1).Value.ToString();

            if (left < n && CompareKeys(rows[left].Cell(keyIndex + 1).Value.ToString(), largestKey) > 0)
            {
                largest = left;
            }

            if (right < n && CompareKeys(rows[right].Cell(keyIndex + 1).Value.ToString(), rows[largest].Cell(keyIndex + 1).Value.ToString()) > 0)
            {
                largest = right;
            }

            if (largest != i)
            {
                (rows[i], rows[largest]) = (rows[largest], rows[i]);
                await Heapify(rows, n, largest, keyIndex);
            }
        }

    }
}

