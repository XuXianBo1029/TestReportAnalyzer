using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace TestReportAnalyzer
{
    public partial class Form1 : Form
    {
        private Button btnLoadCsv;
        private Button btnExportSummaryCsv;
        private ComboBox cboFilter;
        private Button btnSaveToDb;
        private Button btnLoadFromDb;
        private DataGridView dataGridView;
        private Label lblTotal;
        private Label lblPass;
        private Label lblFail;
        private Label lblYield;
        private ListBox listErrorCode;
        private DataTable? currentTable;
        private readonly string dbPath = Path.Combine(Application.StartupPath, "test_reports.db");

        public Form1()
        {
            InitializeComponent();
            BuildUI();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            string sql = @"
                CREATE TABLE IF NOT EXISTS TestRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TestTime TEXT,
                    Station TEXT,
                    Item TEXT,
                    Result TEXT,
                    ErrorCode TEXT
                );
            ";

            using SqliteCommand command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private int SaveTableToDatabase(DataTable table)
        {
            int insertCount = 0;

            using SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            foreach (DataRow row in table.Rows)
            {
                string testTime = table.Columns.Contains("Time") ? row["Time"]?.ToString() ?? "" : "";
                string station = table.Columns.Contains("Station") ? row["Station"]?.ToString() ?? "" : "";
                string item = table.Columns.Contains("Item") ? row["Item"]?.ToString() ?? "" : "";
                string result = table.Columns.Contains("Result") ? row["Result"]?.ToString() ?? "" : "";
                string errorCode = table.Columns.Contains("ErrorCode") ? row["ErrorCode"]?.ToString() ?? "" : "";

                string sql = @"
                    INSERT INTO TestRecords (TestTime, Station, Item, Result, ErrorCode)
                    VALUES ($TestTime, $Station, $Item, $Result, $ErrorCode);
                ";

                using SqliteCommand command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("$TestTime", testTime);
                command.Parameters.AddWithValue("$Station", station);
                command.Parameters.AddWithValue("$Item", item);
                command.Parameters.AddWithValue("$Result", result);
                command.Parameters.AddWithValue("$ErrorCode", errorCode);

                command.ExecuteNonQuery();
                insertCount++;
            }

            return insertCount;
        }

        private DataTable LoadTableFromDatabase()
        {
            DataTable table = new DataTable();

            using SqliteConnection connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            string sql = @"
                SELECT 
                    Id,
                    TestTime AS Time,
                    Station,
                    Item,
                    Result,
                    ErrorCode
                FROM TestRecords
                ORDER BY Id;
            ";

            using SqliteCommand command = new SqliteCommand(sql, connection);
            using SqliteDataReader reader = command.ExecuteReader();

            table.Load(reader);

            return table;
        }

        private void BuildUI()
        {
            this.Text = "Test Report Analyzer";
            this.Width = 1000;
            this.Height = 730;

            btnLoadCsv = new Button();
            btnLoadCsv.Text = "選擇 CSV";
            btnLoadCsv.Left = 20;
            btnLoadCsv.Top = 20;
            btnLoadCsv.Width = 120;
            btnLoadCsv.Height = 35;
            btnLoadCsv.Click += BtnLoadCsv_Click;
            this.Controls.Add(btnLoadCsv);

            btnExportSummaryCsv = new Button();
            btnExportSummaryCsv.Text = "匯出摘要CSV";
            btnExportSummaryCsv.Left = btnLoadCsv.Left + btnLoadCsv.Width + 10;
            btnExportSummaryCsv.Top = 20;
            btnExportSummaryCsv.Width = 120;
            btnExportSummaryCsv.Height = 35;
            btnExportSummaryCsv.Click += BtnExportSummaryCsv_Click;
            this.Controls.Add(btnExportSummaryCsv);

            cboFilter = new ComboBox();
            cboFilter.Left = btnExportSummaryCsv.Left + btnExportSummaryCsv.Width + 10;
            cboFilter.Top = 20;
            cboFilter.Width = 100;
            cboFilter.Height = 35;
            cboFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFilter.Items.Add("All");
            cboFilter.Items.Add("PASS");
            cboFilter.Items.Add("FAIL");
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += CboFilter_SelectedIndexChanged;
            this.Controls.Add(cboFilter);

            btnSaveToDb = new Button();
            btnSaveToDb.Text = "儲存到資料庫";
            btnSaveToDb.Left = cboFilter.Left + cboFilter.Width + 10;
            btnSaveToDb.Top = 20;
            btnSaveToDb.Width = 130;
            btnSaveToDb.Height = 35;
            btnSaveToDb.Click += BtnSaveToDb_Click;
            this.Controls.Add(btnSaveToDb);

            btnLoadFromDb = new Button();
            btnLoadFromDb.Text = "載入資料庫紀錄";
            btnLoadFromDb.Left = btnSaveToDb.Left + btnSaveToDb.Width + 10;
            btnLoadFromDb.Top = 20;
            btnLoadFromDb.Width = 140;
            btnLoadFromDb.Height = 35;
            btnLoadFromDb.Click += BtnLoadFromDb_Click;
            this.Controls.Add(btnLoadFromDb);

            lblTotal = new Label();
            lblTotal.Text = "總筆數：0";
            lblTotal.Left = 20;
            lblTotal.Top = btnLoadCsv.Top + btnLoadCsv.Height + 10;
            lblTotal.Width = 150;
            this.Controls.Add(lblTotal);

            lblPass = new Label();
            lblPass.Text = "PASS：0";
            lblPass.Left = lblTotal.Left + lblTotal.Width + 10;
            lblPass.Top = lblTotal.Top;
            lblPass.Width = 120;
            this.Controls.Add(lblPass);

            lblFail = new Label();
            lblFail.Text = "FAIL：0";
            lblFail.Left = lblPass.Left + lblPass.Width + 10;
            lblFail.Top = lblTotal.Top;
            lblFail.Width = 120;
            this.Controls.Add(lblFail);

            lblYield = new Label();
            lblYield.Text = "良率：0%";
            lblYield.Left = lblFail.Left + lblFail.Width + 10;
            lblYield.Top = lblTotal.Top;
            lblYield.Width = 150;
            this.Controls.Add(lblYield);

            dataGridView = new DataGridView();
            dataGridView.Left = 20;
            dataGridView.Top = lblTotal.Top + lblTotal.Height + 10;
            dataGridView.Width = 700;
            dataGridView.Height = 550;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(dataGridView);

            Label lblErrorTitle = new Label();
            lblErrorTitle.Text = "ErrorCode 統計";
            lblErrorTitle.Left = 750;
            lblErrorTitle.Top = 70;
            lblErrorTitle.Width = 200;
            this.Controls.Add(lblErrorTitle);

            listErrorCode = new ListBox();
            listErrorCode.Left = 750;
            listErrorCode.Top = 100;
            listErrorCode.Width = 200;
            listErrorCode.Height = 520;
            this.Controls.Add(listErrorCode);
        }

        private void BtnLoadCsv_Click(object? sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                DataTable table = ReadCsv(openFileDialog.FileName);
                currentTable = table;
                dataGridView.DataSource = table;
                AnalyzeReport(table);
            }
        }

        private void BtnExportSummaryCsv_Click(object? sender, EventArgs e)
        {
            if (currentTable is null)
            {
                MessageBox.Show("請先載入 CSV 檔案");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog.FileName = "Summary.csv";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ExportSummaryCsv(currentTable, saveFileDialog.FileName);
                MessageBox.Show("摘要 CSV 已匯出");
            }
        }

        private void BtnSaveToDb_Click(object? sender, EventArgs e)
        {
            if (currentTable is null)
            {
                MessageBox.Show("請先載入 CSV 檔案");
                return;
            }

            int insertCount = SaveTableToDatabase(currentTable);
            MessageBox.Show($"已儲存 {insertCount} 筆資料到 SQLite 資料庫");
        }

        private void BtnLoadFromDb_Click(object? sender, EventArgs e)
        {
            DataTable table = LoadTableFromDatabase();

            currentTable = table;
            dataGridView.DataSource = table;
            AnalyzeReport(table);

            cboFilter.SelectedIndex = 0;

            MessageBox.Show($"已載入 {table.Rows.Count} 筆資料");
        }

        private DataTable ReadCsv(string filePath)
        {
            DataTable table = new DataTable();

            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length == 0)
            {
                return table;
            }

            string[] headers = lines[0].Split(',');

            foreach (string header in headers)
            {
                table.Columns.Add(header.Trim());
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                table.Rows.Add(values);
            }

            return table;
        }


        private void AnalyzeReport(DataTable table)
        {
            if (!table.Columns.Contains("Result"))
            {
                MessageBox.Show("CSV 檔案找不到 Result 欄位");
                return;
            }

            int total = table.Rows.Count;
            int pass = 0;
            int fail = 0;

            foreach (DataRow row in table.Rows)
            {
                string result = row["Result"].ToString()?.Trim().ToUpper() ?? "";

                if (result == "PASS")
                {
                    pass++;
                }
                else if (result == "FAIL")
                {
                    fail++;
                }
            }

            double yieldRate = total > 0 ? (double)pass / total * 100 : 0;

            lblTotal.Text = $"總筆數：{total}";
            lblPass.Text = $"PASS：{pass}";
            lblFail.Text = $"FAIL：{fail}";
            lblYield.Text = $"良率：{yieldRate:F2}%";

            AnalyzeErrorCode(table);
        }

        private void AnalyzeErrorCode(DataTable table)
        {
            listErrorCode.Items.Clear();

            if (!table.Columns.Contains("ErrorCode"))
            {
                listErrorCode.Items.Add("找不到 ErrorCode 欄位");
                return;
            }

            Dictionary<string, int> errorCount = new Dictionary<string, int>();

            foreach (DataRow row in table.Rows)
            {
                string code = row["ErrorCode"]?.ToString()?.Trim() ?? "";

                if (code == "")
                {
                    continue;
                }

                if (errorCount.ContainsKey(code))
                {
                    errorCount[code]++;
                }
                else
                {
                    errorCount[code] = 1;
                }
            }

            foreach (var item in errorCount.OrderByDescending(x => x.Value))
            {
                listErrorCode.Items.Add($"{item.Key}：{item.Value}");
            }
        }

        /*   LINQ
                private void AnalyzeErrorCode(DataTable table)
                {
                    listErrorCode.Items.Clear();

                    if (!table.Columns.Contains("ErrorCode"))
                    {
                        listErrorCode.Items.Add("找不到 ErrorCode 欄位");
                        return;
                    }

                    var errorGroups = table.AsEnumerable()
                        .Select(row => row["ErrorCode"].ToString()?.Trim())
                        .Where(code => !string.IsNullOrEmpty(code))
                        .GroupBy(code => code)
                        .Select(group => new
                        {
                            ErrorCode = group.Key,
                            Count = group.Count()
                        })
                        .OrderByDescending(item => item.Count);

                    foreach (var item in errorGroups)
                    {
                        listErrorCode.Items.Add($"{item.ErrorCode}：{item.Count}");
                    }
                }
        */

        private void ExportSummaryCsv(DataTable table, string filePath)
        {
            int total = table.Rows.Count;
            int pass = 0;
            int fail = 0;

            foreach (DataRow row in table.Rows)
            {
                string result = row["Result"]?.ToString()?.Trim().ToUpper() ?? "";

                if (result == "PASS")
                {
                    pass++;
                }
                else if (result == "FAIL")
                {
                    fail++;
                }
            }

            double yieldRate = total > 0 ? (double)pass / total * 100 : 0;

            Dictionary<string, int> errorCount = new Dictionary<string, int>();

            if (table.Columns.Contains("ErrorCode"))
            {
                foreach (DataRow row in table.Rows)
                {
                    string code = row["ErrorCode"]?.ToString()?.Trim() ?? "";

                    if (code == "")
                    {
                        continue;
                    }

                    if (errorCount.ContainsKey(code))
                    {
                        errorCount[code]++;
                    }
                    else
                    {
                        errorCount[code] = 1;
                    }
                }
            }

            List<string> lines = new List<string>();

            lines.Add("Category,Name,Value");
            lines.Add($"Summary,Total,{total}");
            lines.Add($"Summary,PASS,{pass}");
            lines.Add($"Summary,FAIL,{fail}");
            lines.Add($"Summary,Yield,{yieldRate:F2}%");

            foreach (var item in errorCount.OrderByDescending(x => x.Value))
            {
                lines.Add($"ErrorCode,{item.Key},{item.Value}");
            }

            File.WriteAllLines(filePath, lines);
        }

        private void CboFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (currentTable is null)
            {
                return;
            }

            string selected = cboFilter.SelectedItem?.ToString() ?? "All";
            DataView view = currentTable.DefaultView;

            if (selected == "All")
            {
                view.RowFilter = "";
            }
            else
            {
                string safe = selected.Replace("'", "''");
                view.RowFilter = $"Result = '{safe}'";
            }

            dataGridView.DataSource = view;
        }
    }
}