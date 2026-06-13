using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Example_DVH
{
  /// <summary>
  /// Interaction logic for MainControl.xaml
  /// </summary>
  /// 


  public partial class MainControl : UserControl
  {
    public MainControl()
    {
      InitializeComponent();
    }

    public PlanSetup planSetup = null;

    public void DrawDVH(DVHData dvhData)
    {
      // Calculate multipliers for scaling DVH to canvas.
      double xCoeff = MainCanvas.Width / dvhData.MaxDose.Dose;
      double yCoeff = MainCanvas.Height / 100;

      // Set Y axis label
      DoseMaxLabel.Content = string.Format("{0:F0}%", dvhData.MaxDose.Dose);

      // Draw histogram 
      for (int i = 0; i < dvhData.CurveData.Length - 1; i++)
      {
        // Set drawing line parameters
        var line = new Line() { Stroke = Brushes.Blue, StrokeThickness = 4.0 };

        // Set line coordinates
        line.X1 = dvhData.CurveData[i].DoseValue.Dose * xCoeff;
        line.X2 = dvhData.CurveData[i + 1].DoseValue.Dose * xCoeff;
        // Y axis start point is top-left corner of window, convert it to bottom-left.
        line.Y1 = MainCanvas.Height - dvhData.CurveData[i].Volume * yCoeff;
        line.Y2 = MainCanvas.Height - dvhData.CurveData[i + 1].Volume * yCoeff;

        // Add line to the existing canvas
        MainCanvas.Children.Add(line);
      }
    }

        private void onChooseROI(object sender, SelectionChangedEventArgs e)
        {
            

            // Get selected structure
            Structure selectedStructure = planSetup.StructureSet.Structures.Single(o => o.Id == structureBox.SelectedItem.ToString());


            // Retrieve DVH data
            DVHData dvhData = planSetup.GetDVHCumulativeData(selectedStructure,
                                          DoseValuePresentation.Relative,
                                          VolumePresentation.Relative, 0.1);

            if (dvhData == null)
                throw new ApplicationException("DVH data does not exist. Script execution cancelled.");
            DrawDVH(dvhData);
        }
    }
}
