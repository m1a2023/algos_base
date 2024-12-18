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
            LogTextBox.ScrollToEnd(); 
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
                        await NaturalMergeSort(_filePath, keyIndex);
                        break;
                    case "Direct Merge":
                        await DirectMergeSort(_filePath, keyIndex);
                        break;
                    case "Heap Sort":
                        await HeapSort(_filePath, keyIndex);
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

                //newWorkbook.SaveAs(sortedFilePath);
                //Log($"Sorted file saved at {sortedFilePath}\n");

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
        
        private async Task NaturalMergeSort(string inputFilePath, int keyIndex)
        {
            Log("Starting Natural Merge Sort...\n");

            // Открытие исходного файла и извлечение строк
            var workbook = new XLWorkbook(inputFilePath);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1).ToList(); // пропускаем заголовок

            int n = rows.Count;
            int mergeStep = 0;

            // Шаг 1: Найдём все "беги" (runs)
            List<(int start, int end)> runs = FindRuns(rows, keyIndex);

            Log("Step 1: Identifying runs...\n");

            List<string> tempFilePaths = new List<string>();

            // Создаем временные файлы для каждого бега
            foreach (var run in runs)
            {
                var tempWorkbook = new XLWorkbook();
                var tempWorksheet = tempWorkbook.Worksheets.Add("MergedData");
                int rowIdx = 1;

                for (int i = run.start; i < run.end; i++)
                {
                    CopyRowToWorksheet(rows[i], tempWorksheet, rowIdx++, keyIndex);
                }

                string tempFilePath = Path.Combine(Path.GetDirectoryName(inputFilePath), $"Run_{mergeStep++}_" + Path.GetFileName(inputFilePath));
                tempWorkbook.SaveAs(tempFilePath);
                tempFilePaths.Add(tempFilePath);

                Log($"Created temporary run file: {tempFilePath}\n");
            }

            // Шаг 2: Слияние файлов
            while (tempFilePaths.Count > 1)
            {
                List<string> newTempFiles = new List<string>();

                Log($"Step 2: Merging run files... Remaining files: {tempFilePaths.Count}\n");

                for (int i = 0; i < tempFilePaths.Count; i += 2)
                {
                    if (i + 1 < tempFilePaths.Count)
                    {
                        string leftFilePath = tempFilePaths[i];
                        string rightFilePath = tempFilePaths[i + 1];

                        // Сливаем два файла в новый
                        string mergedFilePath = Path.Combine(Path.GetDirectoryName(inputFilePath), $"Merged_{mergeStep++}_" + Path.GetFileName(inputFilePath));
                        MergeFiles(leftFilePath, rightFilePath, mergedFilePath, keyIndex);
                        newTempFiles.Add(mergedFilePath);

                        Log($"Merged {leftFilePath} and {rightFilePath} into {mergedFilePath}\n");
                    }
                    else
                    {
                        // Если нечётное количество, просто добавляем последний файл
                        newTempFiles.Add(tempFilePaths[i]);
                    }
                }

                // Обновляем список файлов для следующего шага
                tempFilePaths = newTempFiles;
            }

            // Шаг 3: Сохраняем итоговый результат
            if (tempFilePaths.Count == 1)
            {
                string resultPath = Path.Combine(Path.GetDirectoryName(inputFilePath),
                    "sorted_" + Path.GetFileName(inputFilePath));
                File.Copy(tempFilePaths[0], resultPath, true);
                Log($"Sorting complete. Final sorted file saved as {resultPath}\n");
            }
        }

        private List<(int start, int end)> FindRuns(List<IXLRow> rows, int keyIndex)
        {
            var runs = new List<(int start, int end)>();
            int n = rows.Count;

            if (n == 0)
                return runs;

            int start = 0;
            for (int i = 1; i < n; i++)
            {
                string prevKey = rows[i - 1].Cell(keyIndex + 1).Value.ToString();
                string currentKey = rows[i].Cell(keyIndex + 1).Value.ToString();

                // Если порядок нарушен, начинаем новый бег
                if (CompareKeys(prevKey, currentKey) > 0)
                {
                    runs.Add((start, i));
                    start = i;
                }
            }

            // Добавляем последний бег
            runs.Add((start, n));

            Log($"Found {runs.Count} runs.");

            return runs;
        }
        private async Task DirectMergeSort(string inputFilePath, int keyIndex)
        {
            Log("Starting Direct Merge Sort...\n");

            var workbook = new XLWorkbook(inputFilePath);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1).ToList(); // пропускаем заголовок

            int n = rows.Count;
            int mergeStep = 0;

            // Начальный этап: каждую строку считаем отдельным "сегментом"
            List<string> tempFilePaths = new List<string>();

            Log("Step 1: Creating individual temporary files for each row...\n");

            for (int i = 0; i < n; i++)
            {
                // Создаем новый файл для текущей строки
                var tempWorkbook = new XLWorkbook();
                var tempWorksheet = tempWorkbook.Worksheets.Add("MergedData");

                CopyRowToWorksheet(rows[i], tempWorksheet, 1, keyIndex);
                
                string tempFilePath = Path.Combine(Path.GetDirectoryName(_filePath), $"Temp_{mergeStep++}_" + Path.GetFileName(_filePath));
                tempWorkbook.SaveAs(tempFilePath);
                tempFilePaths.Add(tempFilePath);

                Log($"Created temporary file: {tempFilePath}\n");
            }

            // Продолжаем слияние
            while (tempFilePaths.Count > 1)
            {
                List<string> newTempFiles = new List<string>();

                Log($"Step 2: Merging files... Remaining temp files: {tempFilePaths.Count}\n");

                for (int i = 0; i < tempFilePaths.Count; i += 2)
                {
                    if (i + 1 < tempFilePaths.Count)
                    {
                        string leftFilePath = tempFilePaths[i];
                        string rightFilePath = tempFilePaths[i + 1];

                        // Сливаем два файла в новый
                        string mergedFilePath = Path.Combine(Path.GetDirectoryName(_filePath), $"Merged_{mergeStep++}_" + Path.GetFileName(_filePath));
                        MergeFiles(leftFilePath, rightFilePath, mergedFilePath, keyIndex);
                        newTempFiles.Add(mergedFilePath);

                        Log($"Merged {leftFilePath} and {rightFilePath} into {mergedFilePath}\n");
                    }
                    else
                    {
                        // Если нечётное количество, просто добавляем последний файл
                        newTempFiles.Add(tempFilePaths[i]);
                    }
                }

                // Обновляем список файлов для следующего шага
                tempFilePaths = newTempFiles;
            }

            // Когда осталось только одно итоговое слияние, сохраняем результат
            if (tempFilePaths.Count == 1)
            {
                string resultPath = Path.Combine(Path.GetDirectoryName(_filePath),
                    "sorted_" + Path.GetFileName(_filePath));
                File.Copy(tempFilePaths[0], resultPath, true);
                Log($"Sorting complete. Final sorted file saved as {resultPath}\n");
            }
        }

        private void MergeFiles(string leftFilePath, string rightFilePath, string outputFilePath, int keyIndex)
        {
            Log($"Merging files: {leftFilePath} and {rightFilePath}\n");

            var leftWorkbook = new XLWorkbook(leftFilePath);
            var rightWorkbook = new XLWorkbook(rightFilePath);

            var leftWorksheet = leftWorkbook.Worksheet(1);
            var rightWorksheet = rightWorkbook.Worksheet(1);

            var leftRows = leftWorksheet.RowsUsed().ToList();
            var rightRows = rightWorksheet.RowsUsed().ToList();

            var newWorkbook = new XLWorkbook();
            var newWorksheet = newWorkbook.Worksheets.Add("MergedData");

            int i = 0, j = 0, rowIdx = 1;

            Log("Step 3: Merging rows...\n");

            // Слияние двух файлов
            while (i < leftRows.Count && j < rightRows.Count)
            {
                string leftKey = leftRows[i].Cell(keyIndex + 1).Value.ToString();
                string rightKey = rightRows[j].Cell(keyIndex + 1).Value.ToString();

                if (CompareKeys(leftKey, rightKey) <= 0)
                {
                    CopyRowToWorksheet(leftRows[i], newWorksheet, rowIdx++, keyIndex);
                    i++;
                }
                else
                {
                    CopyRowToWorksheet(rightRows[j], newWorksheet, rowIdx++, keyIndex);
                    j++;
                }
            }

            // Копируем оставшиеся строки
            while (i < leftRows.Count)
            {
                CopyRowToWorksheet(leftRows[i], newWorksheet, rowIdx++, keyIndex);
                i++;
            }
            while (j < rightRows.Count)
            {
                CopyRowToWorksheet(rightRows[j], newWorksheet, rowIdx++, keyIndex);
                j++;
            }

            newWorkbook.SaveAs(outputFilePath);
            Log($"Merged file saved at {outputFilePath}\n");
        }

        private void CopyRowToWorksheet(IXLRow sourceRow, IXLWorksheet destWorksheet, int destRowIdx, int keyIndex)
        {
            Log($"Copying row {destRowIdx} to new worksheet...\n");

            for (int colIdx = 1; colIdx <= sourceRow.LastCellUsed().Address.ColumnNumber; colIdx++)
            {
                destWorksheet.Cell(destRowIdx, colIdx).Value = sourceRow.Cell(colIdx).Value;
            }
        }
        private async Task HeapSort(string inputFilePath, int keyIndex)
        {
            Log("Starting Heap Sort...\n");

            // Открытие исходного файла и извлечение строк
            var workbook = new XLWorkbook(inputFilePath);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1).ToList(); // пропускаем заголовок

            int n = rows.Count;

            Log("Step 1: Building the heap...\n");

            // Строим кучу (heap) для массива строк
            for (int i = n / 2 - 1; i >= 0; i--)
            {
                await Heapify(rows, n, i, keyIndex);
            }

            Log("Step 2: Sorting the heap...\n");

            // Проводим сортировку
            for (int i = n - 1; i > 0; i--)
            {
                // Меняем местами корень и последний элемент
                (rows[0], rows[i]) = (rows[i], rows[0]);

                // Сохраняем текущий результат в новый временный файл
                var tempWorkbook = new XLWorkbook();
                var tempWorksheet = tempWorkbook.Worksheets.Add("SortedData");
                for (int j = 0; j < i; j++)
                {
                    CopyRowToWorksheet(rows[j], tempWorksheet, j + 1, keyIndex);
                }
                string tempFilePath = Path.Combine(Path.GetDirectoryName(inputFilePath), $"HeapSort_{i}.xlsx");
                tempWorkbook.SaveAs(tempFilePath);

                Log($"Heap after swap and before heapifying saved as: {tempFilePath}\n");

                // Восстановление кучи для оставшейся части массива
                await Heapify(rows, i, 0, keyIndex);

                await Task.Delay(_delay);  // Добавление задержки, если необходимо
            }

            // После завершения сортировки сохраняем итоговый файл
            string resultPath = Path.Combine(Path.GetDirectoryName(inputFilePath), "sorted_" + Path.GetFileName(inputFilePath));
            var finalWorkbook = new XLWorkbook();
            var finalWorksheet = finalWorkbook.Worksheets.Add("SortedData");
            for (int i = 0; i < n; i++)
            {
                CopyRowToWorksheet(rows[i], finalWorksheet, i + 1, keyIndex);
            }
            finalWorkbook.SaveAs(resultPath);

            Log($"Sorting complete. Final sorted file saved as {resultPath}\n");
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
                // Меняем местами строки
                (rows[i], rows[largest]) = (rows[largest], rows[i]);

                Log($"Swapping rows {i + 1} and {largest + 1}\n");

                // Рекурсивно восстанавливаем кучу для затронутой части
                await Heapify(rows, n, largest, keyIndex);
            }
        }
    }
}

