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
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace Patient_Report
{
    /// <summary>
    /// Interaction logic for ReportControl.xaml
    /// </summary>
    public partial class ReportControl : UserControl
    {
        public PlanningItem pi;
        public ReportControl()
        {
            InitializeComponent();
        }

        private void AddDQM_btn_Click(object sender, RoutedEventArgs e)
        {
            //add a horizontal stackpanel to hold created controls
            StackPanel sp = new StackPanel();
            sp.Height = 40;
            sp.Width = params_sp.Width;
            sp.Orientation = Orientation.Horizontal;
            sp.Margin = new Thickness(5);

            //create a combobox to add structures
            ComboBox cb = new ComboBox();
            cb.Name = "structure_cb";
            cb.Width = 150;
            cb.Height = sp.Height - 5;
            cb.HorizontalAlignment = HorizontalAlignment.Left;
            cb.VerticalAlignment = VerticalAlignment.Top;
            cb.Margin = new Thickness(5, 5, 0, 0);
            foreach(Structure s in pi.StructureSet.Structures)
                cb.Items.Add(s.Id);
            sp.Children.Add(cb);

            //create a combobox to hold the metrics
            ComboBox metric_cb = new ComboBox();
            metric_cb.Name = "metric_cb";
            metric_cb.Width = 120;
            metric_cb.Height = sp.Height - 5;
            metric_cb.HorizontalAlignment = HorizontalAlignment.Left;
            metric_cb.VerticalAlignment = VerticalAlignment.Top;
            metric_cb.Margin = new Thickness(5, 5, 0, 0);
            string[] metrics = new string[] { "Mean", "Max", "Min" };
            foreach (string s in metrics)
                metric_cb.Items.Add(s);
            sp.Children.Add(metric_cb);

            //add the stackpanel to the parent stackpanel
            params_sp.Children.Add(sp);
        }

        private void Print_btn_Click(object sender, RoutedEventArgs e)
        {
            //create a list of current parameters
            List<DoseQMetric> dqm_list = new List<DoseQMetric>();
            foreach (StackPanel sp in params_sp.Children)
            {
                //create a new dose metric
                DoseQMetric dqm = new DoseQMetric();
                foreach(Control contr in sp.Children)
                {
                    if (contr.Name == "structure_cb")
                        dqm.StructureId = (contr as ComboBox).SelectedItem.ToString();
                    else if (contr.Name == "metric_cb")
                        dqm.DType = (contr as ComboBox).SelectedItem.ToString();
                }
                dqm.Value = Calculate_val(dqm);
                dqm_list.Add(dqm);
            }

            string output_s = "";
            foreach(DoseQMetric dqm in dqm_list)
                output_s += String.Format("Structure: {0}; Parameter: {1}; Value: {2}\n", dqm.StructureId, dqm.DType, dqm.Value);
            MessageBox.Show(output_s);
        }

        private string Calculate_val(DoseQMetric dqm)
        {
            //throw new NotImplementedException();
            string s = "";
            if (String.IsNullOrEmpty(dqm.StructureId) || String.IsNullOrEmpty(dqm.DType))
                return "NAN";
            Structure st = pi.StructureSet.Structures.Single(x => x.Id == dqm.StructureId);
            DVHData dvh = pi.GetDVHCumulativeData(st, DoseValuePresentation.Absolute, VolumePresentation.Relative, 1);
            if (dvh == null)
                return "No DVH Data.";
            switch (dqm.DType)
            {
                case "Mean":
                    s = dvh.MeanDose.ToString();
                    break;
                case "Max":
                    s = dvh.MaxDose.ToString();
                    break;
                case "Min":
                    s = dvh.MinDose.ToString();
                    break;
            }
            return s;
        }


        public class DoseQMetric
        {
            public string StructureId { get; set; }
            public string DType { get; set; }
            public string Value { get; set; }
        }
    }
}
