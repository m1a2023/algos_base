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

        public Task02()
        {
            InitializeComponent();
            _delay = (int)DelaySlider.Value; // Default value for delay is the slider's value
            DelaySlider.ValueChanged += DelaySlider_ValueChanged; // Event handler for slider value change
        }

        // Handle slider value change (update delay)
        private void DelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _delay = (int)e.NewValue;
        }

        // Handle file browsing
        private void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.AppendText("Browse button clicked. Opening file dialog...\n");

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                _filePath = openFileDialog.FileName;
                LogTextBox.AppendText($"File selected: {_filePath}\n");
            }
            else
            {
                LogTextBox.AppendText("No file selected.\n");
            }
        }

        // Handle start sorting
        private void OnStartSortingClick(object sender, RoutedEventArgs e)
        {
            LogTextBox.AppendText("Start Sorting button clicked.\n");

            // Check if file is selected
            if (string.IsNullOrEmpty(_filePath))
            {
                MessageBox.Show("Please select a file first.");
                LogTextBox.AppendText("Error: No file selected.\n");
                return;
            }

            // Get sorting method and key attribute
            string selectedMethod = ((ComboBoxItem)SortingMethodComboBox.SelectedItem)?.Content.ToString();
            string keyAttribute = KeyAttributeTextBox.Text;

            if (string.IsNullOrEmpty(selectedMethod) || string.IsNullOrEmpty(keyAttribute))
            {
                MessageBox.Show("Please select sorting method and key attribute.");
                LogTextBox.AppendText("Error: Sorting method or key attribute is not selected.\n");
                return;
            }

            // Log selected method and attribute
            LogTextBox.AppendText($"Sorting method selected: {selectedMethod}\n");
            LogTextBox.AppendText($"Key attribute: {keyAttribute}\n");

            try
            {
                LogTextBox.AppendText($"Starting sorting using {selectedMethod} by {keyAttribute}...\n");

                // Read the file
                List<string> lines = new List<string>(File.ReadLines(_filePath));
                LogTextBox.AppendText($"File loaded. Number of records: {lines.Count}\n");

                // Skip header if it exists
                var header = lines.First();
                lines = lines.Skip(1).ToList();

                // Identify the index of the key attribute (column)
                var columns = header.Split(',');
                int keyIndex = Array.IndexOf(columns, keyAttribute);

                if (keyIndex == -1)
                {
                    MessageBox.Show("Key attribute not found in header.");
                    LogTextBox.AppendText("Error: Key attribute not found in header.\n");
                    return;
                }

                // Sort based on the key attribute
                var sortedLines = lines
                    .Select(line => line.Split(','))
                    .OrderBy(parts =>
                    {
                        string value = parts[keyIndex];

                        // Check if the key attribute contains numeric values (e.g., Area, Population)
                        if (double.TryParse(value, out double numericValue))
                        {
                            return (object)numericValue; // Sort numerically if it's a number
                        }
                        return (object)value; // Otherwise, sort as a string (e.g., Country, Continent)
                    })
                    .Select(parts => string.Join(",", parts)) // Join back into a string
                    .ToList();

                // Prepend header back if needed
                sortedLines.Insert(0, header);

                // Save the sorted file
                string sortedFilePath = Path.Combine(Path.GetDirectoryName(_filePath), "sorted_" + Path.GetFileName(_filePath));
                File.WriteAllLines(sortedFilePath, sortedLines);
                LogTextBox.AppendText($"Sorted file saved at {sortedFilePath}\n");
            }
            catch (Exception ex)
            {
                LogTextBox.AppendText($"Error during sorting: {ex.Message}\n");
            }
        }



        // Example of logging for sorting action: Natural Merge Sort
        private async Task NaturalMergeSort(List<string> lines, string keyAttribute)
        {
            LogTextBox.AppendText("Natural Merge Sort started...\n");

            // Dummy sort logic for logging with delay
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    LogTextBox.AppendText($"Comparing: {lines[i]} and {lines[j]}\n");
                    // Simulate a comparison action
                    if (string.Compare(lines[i], lines[j]) > 0)
                    {
                        LogTextBox.AppendText($"Swapping: {lines[i]} with {lines[j]}\n");
                        // Simulate a swap action
                        string temp = lines[i];
                        lines[i] = lines[j];
                        lines[j] = temp;
                    }

                    // Apply delay between actions
                    await Task.Delay(_delay);
                }
            }

            LogTextBox.AppendText("Natural Merge Sort completed.\n");
        }

        // Example of logging for sorting action: Direct Merge Sort
        private async Task DirectMergeSort(List<string> lines, string keyAttribute)
        {
            LogTextBox.AppendText("Direct Merge Sort started...\n");

            // Dummy sort logic for logging with delay
            for (int i = 0; i < lines.Count; i++)
            {
                for (int j = i + 1; j < lines.Count; j++)
                {
                    LogTextBox.AppendText($"Comparing: {lines[i]} and {lines[j]}\n");
                    // Simulate a comparison action
                    if (string.Compare(lines[i], lines[j]) > 0)
                    {
                        LogTextBox.AppendText($"Swapping: {lines[i]} with {lines[j]}\n");
                        // Simulate a swap action
                        string temp = lines[i];
                        lines[i] = lines[j];
                        lines[j] = temp;
                    }

                    // Apply delay between actions
                    await Task.Delay(_delay);
                }
            }

            LogTextBox.AppendText("Direct Merge Sort completed.\n");
        }

        // Example of logging for sorting action: Heap Sort
        private async Task HeapSort(List<string> lines)
        {
            LogTextBox.AppendText("Heap Sort started...\n");

            // Dummy sort logic for logging with delay
            for (int i = 0; i < lines.Count; i++)
            {
                LogTextBox.AppendText($"Processing: {lines[i]}\n");
                // Simulate action
                await Task.Delay(_delay);
            }

            LogTextBox.AppendText("Heap Sort completed.\n");
        }
    }
}
