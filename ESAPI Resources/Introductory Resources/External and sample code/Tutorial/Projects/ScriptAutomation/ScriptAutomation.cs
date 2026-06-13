using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            // TODO : Add here the code that is called when the script is launched from Eclipse.
            Patient patient = context.Patient;
            patient.BeginModifications();

            //add a plan to the structure set of the patient
            StructureSet structureSet = context.StructureSet;
            if (structureSet == null)
            {
                MessageBox.Show("Must have a structure set in scope.");
                return;
            }
            Course C_Auto = patient.AddCourse();
            C_Auto.Id = "AutoCourse";
            ExternalPlanSetup eps = C_Auto.AddExternalPlanSetup(structureSet);

            //find the PTV
            Structure target = structureSet.Structures.Where(x => x.DicomType == "PTV").Count() == 0 ? null : structureSet.Structures.First(x => x.DicomType == "PTV");
            if (target == null)
            {
                MessageBox.Show("No PTVs in the structure set");
                return;
            }

            //add some fields
            double[] gantry_angles = new double[] { 0, 90, 270, 180 };
            ExternalBeamMachineParameters ebmp = new ExternalBeamMachineParameters(
                "iX 1100",)

        }
    }
}
