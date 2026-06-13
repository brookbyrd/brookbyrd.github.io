using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


// Do not change namespace and class name
// otherwise Eclipse will not be able to run the script.
namespace VMS.TPS
{
  class Script
  {
    public Script()
    {
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Execute(ScriptContext context, Window window)
    {
      PlanSetup planSetup = context.PlanSetup;

        // If there's no selected plan with calculated dose throw an exception
        if (planSetup == null || planSetup.Dose == null)
            throw new ApplicationException("Please open a calculated plan before using this script.");

        // Retrieve StructureSet
        StructureSet structureSet = planSetup.StructureSet;
        if (structureSet == null)
            throw new ApplicationException("The selected plan does not reference a StructureSet.");

        // Add existing WPF control to the script window.
        var mainControl = new Example_DVH.MainControl();
      window.Content = mainControl;
      window.Width = 610;
      window.Height = 460;
            foreach (Structure s in context.StructureSet.Structures)
                mainControl.structureBox.Items.Add(s.Id);
      mainControl.planSetup = context.PlanSetup;

      window.Title = "Plan : " + planSetup.Id;

      // Draw DVH
      //mainControl.DrawDVH(dvhData);
    }
  }
}
