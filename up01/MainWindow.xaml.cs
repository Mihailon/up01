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
using Microsoft.Win32;
using System.Data;
using Microsoft.Office.Interop.Excel;
using ClosedXML.Excel;
using GanttChart = nGantt.GanttChart;
using nGantt.GanttChart;
using System.Collections.ObjectModel;
using nGantt.PeriodSplitter;

namespace up01
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        OpenFileDialog opf;
        private int GantLenght { get; set; }
        private ObservableCollection<ContextMenuItem> ganttTaskContextMenuItems = new ObservableCollection<ContextMenuItem>();
        private ObservableCollection<SelectionContextMenuItem> selectionContextMenuItems = new ObservableCollection<SelectionContextMenuItem>();

        public MainWindow()
        {
            InitializeComponent(); opf = new OpenFileDialog();
            opf.Filter = "xlsx files (*.xlsx)|*.xlsx";
            opf.FileOk += Opf_FileOk;
        }

        public void CreateGant(List<ImportModel> items)
        {

            // Set max and min dates
            gant_grid.Initialize(items[0].date_start, items[items.Count - 1].date_end);
            // Create timelines and define how they should be presented
            gant_grid.CreateTimeLine(new PeriodYearSplitter(items[0].date_start, items[items.Count - 1].date_end), FormatYear);
            gant_grid.CreateTimeLine(new PeriodMonthSplitter(items[0].date_start, items[items.Count - 1].date_end), FormatMonth);
            var gridLineTimeLine = gant_grid.CreateTimeLine(new PeriodDaySplitter(items[0].date_start, items[items.Count - 1].date_end), FormatDay);

            // Set the timeline to atatch gridlines to
            gant_grid.SetGridLinesTimeline(gridLineTimeLine, DetermineBackground);
            var rowgroup = gant_grid.CreateGanttRowGroup("Этапы");
            foreach (var item in items)
            {
                CreateData(rowgroup, item);
            }
        }

        private System.Windows.Media.Brush DetermineBackground(TimeLineItem timeLineItem)
        {
            if (timeLineItem.End.Date.DayOfWeek == DayOfWeek.Saturday || timeLineItem.End.Date.DayOfWeek == DayOfWeek.Sunday)
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue);
            else
                return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Transparent);
        }

        private void CreateData(HeaderedGanttRowGroup rowgroup, ImportModel item)
        {
            // Create and data
            var row1 = gant_grid.CreateGanttRow(rowgroup, item.Name);
            System.Windows.Media.Brush brush = Brushes.Green;
            gant_grid.AddGanttTask(row1, new GanttTask() { Start = item.date_start, End = item.date_end, Name = item.responsible });
        }

        private string FormatYear(Period period)
        {
            return period.Start.Year.ToString();
        }

        private string FormatMonth(Period period)
        {
            return period.Start.Month.ToString();
        }

        private string FormatDay(Period period)
        {
            return period.Start.Day.ToString();
        }

        /// <summary>
        /// Functions for load excel file
        /// </summary>

        private void Opf_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ClearDatagrid();
            var metrics = EnumerateData(opf.FileName).ToList();
            datagrid_table.ItemsSource = metrics;
            CreateGant(metrics.ToList());
        }

        private void btn_load_Click(object sender, RoutedEventArgs e)
        {
            opf.ShowDialog();
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            ClearDatagrid();
        }

        public void ClearDatagrid()
        {
            datagrid_table.ItemsSource = null;
            datagrid_table.Items.Refresh();

            // clear Gantt

            gant_grid.ClearGantt();
        }

        public static IEnumerable<ImportModel> EnumerateData(string xlsxpath)
        {
            // Открываем книгу
            
            using (var workbook = new XLWorkbook(xlsxpath))
            {
                IXLWorksheet worksheet = workbook.Worksheets.Worksheet(1);
                {
                    var totalRows = worksheet.RowsUsed().Count();
                    // Перебираем диапазон нужных строк
                    for (int row = 2; row <= totalRows; ++row)
                    {
                        // По каждой строке формируем объект
                        var metric = new ImportModel
                        {
                            Name = worksheet.Cell(row, 1).GetValue<string>(),
                            date_start = Convert.ToDateTime(worksheet.Cell(row, 2).GetValue<string>()),
                            duration = worksheet.Cell(row, 3).GetValue<int>(),
                            delay = worksheet.Cell(row, 4).GetValue<int>(),
                            responsible = worksheet.Cell(row, 6).GetValue<string>(),
                        };
                        // И возвращаем его
                        yield return metric;
                    }
                }

            }
        }
    }
}
