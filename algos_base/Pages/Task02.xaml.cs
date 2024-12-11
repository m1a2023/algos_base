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
        }
        private void PreviousPageButtonClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
        
        private void Log(string message)
        {
            LogTextBox.AppendText(message);
        }
        private void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            Log("Browse button clicked. Opening file dialog...\n");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
            if (openFileDialog.ShowDialog() == true)
            {
                _filePath = openFileDialog.FileName;
                Log($"File selected: {_filePath}\n");
                LoadColumnsFromFile(_filePath);
            }
            else
            {
                Log("No file selected.\n");
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

                Log("Columns loaded into ComboBox.\n");
            }
            catch (Exception ex)
            {
                Log($"Error loading columns: {ex.Message}\n");
            }
        }
        private async void OnStartSortingClick(object sender, RoutedEventArgs e)
        {
            Log("Start Sorting button clicked.\n");

            if (string.IsNullOrEmpty(_filePath))
            {
                MessageBox.Show("Please select a file first.");
                Log("Error: No file selected.\n");
                return;
            }

            string selectedMethod = ((ComboBoxItem)SortingMethodComboBox.SelectedItem)?.Content?.ToString();
            string keyAttribute = KeyAttributeComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedMethod) || string.IsNullOrEmpty(keyAttribute))
            {
                MessageBox.Show("Please select sorting method and key attribute.");
                Log("Error: Sorting method or key attribute is not selected.\n");
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
                    Log("Error: Key attribute not found in header.\n");
                    return;
                }

                Log($"Sorting method selected: {selectedMethod}\n");
                Log($"Key attribute: {keyAttribute}\n");
                switch (selectedMethod)
                {
                    case "Natural Merge":
                        await NaturalMergeSort(rows, keyIndex);
                        break;
                    case "Direct Merge":
                        await DirectMergeSort(rows, keyIndex);
                        break;
                    case "Heap Sort":
                        await HeapSort(rows, keyIndex);
                        break;
                    default:
                        Log("Error: Unsupported sorting method.\n");
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
                Log($"Sorted file saved at {sortedFilePath}\n");

            }
            catch (Exception ex)
            {
                Log($"Error during sorting: {ex.Message}\n");
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
        private async Task NaturalMergeSort(List<IXLRow> rows, int keyIndex)
        {
            List<(int start, int end)> runs = FindRuns(rows, keyIndex);
    
            while (runs.Count > 1)
            {
                List<(int start, int end)> newRuns = new List<(int start, int end)>();

                for (int i = 0; i < runs.Count; i += 2)
                {
                    if (i + 1 < runs.Count)
                    {
                        var leftRun = runs[i];
                        var rightRun = runs[i + 1];

                        MergeInto(rows, leftRun.start, rightRun.start, rightRun.end, keyIndex);
                        await Task.Delay(_delay);

                        newRuns.Add((leftRun.start, rightRun.end));
                    }
                    else
                    {
                        newRuns.Add(runs[i]);
                    }
                }
                runs = newRuns;
            }
        }

        private List<(int start, int end)> FindRuns(List<IXLRow> rows, int keyIndex)
        {
            List<(int start, int end)> runs = new List<(int start, int end)>();
            int start = 0;

            for (int i = 1; i < rows.Count; i++)
            {
                string currentKey = rows[i].Cell(keyIndex + 1).Value.ToString();
                string previousKey = rows[i - 1].Cell(keyIndex + 1).Value.ToString();

                if (CompareKeys(previousKey, currentKey) > 0)
                {
                    runs.Add((start, i));
                    start = i;
                }
            }
            runs.Add((start, rows.Count));
            return runs;
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

