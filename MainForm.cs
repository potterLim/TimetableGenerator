﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TimetableGenerator
{
    public partial class MainForm : Form
    {
        private List<List<TimeSlot>> validSchedules;
        private string inputFileName;

        public MainForm()
        {
            InitializeComponent();
            try
            {
                LoadData();
                InitializeUI();
                SaveSchedulesAsPng();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "프로그램 실행 중 문제가 발생했습니다. 프로그램을 종료합니다.\n" +
                    $"오류 메시지:\n{ex.Message}",
                    "프로그램 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void LoadData()
        {
            var dataLoader = new DataLoader();
            validSchedules = dataLoader.LoadAndGenerateSchedules(out inputFileName);

            if (validSchedules == null || validSchedules.Count == 0)
            {
                MessageBox.Show(
                    "유효한 시간표가 없습니다.",
                    "시간표 생성 실패",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                Environment.Exit(1);
            }
        }

        private void InitializeUI()
        {
            try
            {
                // Create a TableLayoutPanel for layout
                var tableLayoutPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2, // Two columns: ListBox and DataGridView
                };

                // Set column widths
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100)); // Fixed width for ListBox (100px)
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Remaining width for DataGridView

                // ListBox for schedule selection
                var listBox = new ListBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10),
                    ItemHeight = 30
                };

                for (int i = 1; i <= validSchedules.Count; i++)
                {
                    listBox.Items.Add($"시간표 {i}");
                }

                // DataGridView for displaying the schedule
                var dataGridView = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, // Disable auto-sizing
                    BorderStyle = BorderStyle.FixedSingle,
                    BackgroundColor = Color.White,
                    AllowUserToAddRows = false,
                    AllowUserToResizeRows = false,
                    RowHeadersVisible = false,
                    ColumnHeadersVisible = false, // Hide default column headers
                    RowTemplate = { Height = 50 }, // Increase row height for better visibility
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Font = new Font("Segoe UI", 10),
                        Alignment = DataGridViewContentAlignment.MiddleCenter, // Central alignment for all cells
                        BackColor = Color.White,
                        ForeColor = Color.Black,
                        SelectionBackColor = Color.LightBlue,
                        SelectionForeColor = Color.Black
                    }
                };

                // Add alternate row colors for better contrast
                dataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);

                // Add controls to TableLayoutPanel
                tableLayoutPanel.Controls.Add(listBox, 0, 0);
                tableLayoutPanel.Controls.Add(dataGridView, 1, 0);

                // Set row styles (single row)
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                // Add TableLayoutPanel to the form
                Controls.Add(tableLayoutPanel);

                // Adjust DataGridView column widths dynamically
                dataGridView.DataBindingComplete += (sender, e) =>
                {
                    if (dataGridView.Columns.Count > 0)
                    {
                        int originalWidth = 150; // Assume the original width of the "시간" column was 150px
                        dataGridView.Columns[0].Width = originalWidth / 3;

                        int remainingWidth = dataGridView.Width - dataGridView.Columns[0].Width;
                        int dayColumnWidth = remainingWidth / (dataGridView.Columns.Count - 1);

                        for (int i = 1; i < dataGridView.Columns.Count; i++)
                        {
                            dataGridView.Columns[i].Width = dayColumnWidth;
                        }
                    }

                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        if (row.Index == 0) // First row (header-like row)
                        {
                            row.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                        }

                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell.ColumnIndex == 0) // First column
                            {
                                cell.Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                            }
                        }
                    }
                };

                // Handle ListBox selection
                listBox.SelectedIndexChanged += (sender, e) =>
                {
                    if (listBox.SelectedIndex >= 0)
                    {
                        int index = listBox.SelectedIndex;
                        var dataTable = ScheduleGenerator.Generate(validSchedules[index]);
                        dataGridView.DataSource = dataTable;
                    }
                };

                // Display the first schedule by default
                if (validSchedules.Count > 0)
                {
                    listBox.SelectedIndex = 0;
                }

                // Ensure the form is visible and active
                this.WindowState = FormWindowState.Normal; // Restore the window if minimized
                this.Activate(); // Bring the window to the front and give it focus

                // Set Form properties for better appearance
                Text = "시간표 생성기";
                Width = 1200; // Ensure the form width provides enough space for DataGridView
                Height = 700;
                StartPosition = FormStartPosition.CenterScreen;
                BackColor = Color.WhiteSmoke;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "UI 초기화 중 문제가 발생했습니다. 프로그램이 종료됩니다.\n" +
                    $"오류 메시지:\n{ex.Message}",
                    "UI 초기화 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }


        private void SaveSchedulesAsPng()
        {
            try
            {
                string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output", Path.GetFileNameWithoutExtension(inputFileName));
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                for (int i = 0; i < validSchedules.Count; i++)
                {
                    var schedule = validSchedules[i];
                    var filePath = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(inputFileName)}_시간표{i + 1}.png");
                    SaveScheduleAsPng(schedule, filePath);
                }

                MessageBox.Show(
                    $"시간표가 성공적으로 저장되었습니다.\n저장 위치: {outputDir}",
                    "저장 완료",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "시간표 저장 중 문제가 발생했습니다.\n" +
                    $"오류 메시지:\n{ex.Message}",
                    "저장 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SaveScheduleAsPng(List<TimeSlot> schedule, string filePath)
        {
            try
            {
                using (var tempForm = new Form())
                {
                    tempForm.FormBorderStyle = FormBorderStyle.None;
                    tempForm.StartPosition = FormStartPosition.Manual;
                    tempForm.Location = new Point(-2000, -2000);
                    tempForm.Size = new Size(1200, 700);

                    var tempDataGridView = new DataGridView
                    {
                        Dock = DockStyle.Fill,
                        ReadOnly = true,
                        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, // Disable auto-sizing
                        BorderStyle = BorderStyle.FixedSingle,
                        BackgroundColor = Color.White,
                        AllowUserToAddRows = false,
                        AllowUserToResizeRows = false,
                        RowHeadersVisible = false,
                        ColumnHeadersVisible = false, // Hide default column headers
                        RowTemplate = { Height = 50 }, // Increase row height for better visibility
                        DefaultCellStyle = new DataGridViewCellStyle
                        {
                            Font = new Font("Segoe UI", 10),
                            Alignment = DataGridViewContentAlignment.MiddleCenter, // Central alignment for all cells
                            BackColor = Color.White,
                            ForeColor = Color.Black,
                            SelectionBackColor = Color.LightBlue,
                            SelectionForeColor = Color.Black
                        }
                    };

                    // Add alternate row colors for better contrast
                    tempDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);

                    tempDataGridView.DataBindingComplete += (sender, e) =>
                    {
                        if (tempDataGridView.Columns.Count > 0)
                        {
                            int originalWidth = 150; // Assume the original width of the "시간" column was 150px
                            tempDataGridView.Columns[0].Width = originalWidth / 3;

                            int remainingWidth = tempDataGridView.Width - tempDataGridView.Columns[0].Width;
                            int dayColumnWidth = remainingWidth / (tempDataGridView.Columns.Count - 1);

                            for (int i = 1; i < tempDataGridView.Columns.Count; i++)
                            {
                                tempDataGridView.Columns[i].Width = dayColumnWidth;
                            }
                        }
                    };

                    tempDataGridView.DataSource = ScheduleGenerator.Generate(schedule);
                    tempForm.Controls.Add(tempDataGridView);
                    tempForm.Show();

                    Bitmap bitmap = new Bitmap(tempForm.Width, tempForm.Height);
                    tempForm.DrawToBitmap(bitmap, new Rectangle(0, 0, tempForm.Width, tempForm.Height));
                    bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "시간표 이미지를 저장하는 중 문제가 발생했습니다.\n" +
                    $"파일 경로: {filePath}\n오류 메시지:\n{ex.Message}",
                    "이미지 저장 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}