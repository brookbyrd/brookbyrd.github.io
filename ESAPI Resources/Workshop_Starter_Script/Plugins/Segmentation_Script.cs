using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]
[assembly: AssemblyInformationalVersion("1.0")]

namespace VMS.TPS
{
    public class Script
    {
        private readonly double outerMarginX = 1.95;
        private readonly double outerMarginY = 1.95;
        private readonly double outerMarginZ = 6.0;
        private readonly Action<string> logAction;

        private readonly StructureSet structureSet;

        public Script()
        {

        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext context /*, System.Windows.Window window, ScriptEnvironment environment*/)
        {
            // TODO : Add here the code that is called when the script is launched from Eclipse.


            if (context.StructureSet != null)
            {
                if (context.StructureSet.Structures != null)
                {
                    if (context.StructureSet.Structures.FirstOrDefault().Id != null)
                    {
                        MessageBox.Show("Beginning HighDensity Segmentation");
                        CreateHighDensityStructure(context.StructureSet);
                        MessageBox.Show("Completed HighDensity Segmentation");

                    }
                    else
                    {
                        MessageBox.Show("No Structure set found");
                    }


                }

            }
            else
            {
                MessageBox.Show("Script Errored out");
            }
        }


        private void CreateHighDensityStructure(StructureSet structureSet)
        {
            try
            {
   
                //Create high density structure
                var existingBodyStruct = structureSet.Structures.FirstOrDefault(s => s.Id == "BODY");

                //// If existingStruct isn't null, remove it from the structure set.
                //if (existingBodyStruct != null)
                //{
                //    MessageBox.Show($"Removing existing BODY structure");
                //    structureSet.RemoveStructure(existingBodyStruct);
                //}

                // Get the default search body structures
                MessageBox.Show("Getting search body params");
                var baseParams = structureSet.GetDefaultSearchBodyParameters();
                baseParams.LoadDefaults();
                baseParams.LowerHUThreshold = 2000; // Threshold at 2500 HU
                baseParams.PreDisconnect = true;
                baseParams.PreDisconnectRadius = 2;     // Remove and Voxel islands that are < 4 voxels
                baseParams.Smoothing = true;
                baseParams.SmoothingLevel = 3;
                
                // Search the body
                MessageBox.Show("Searching body");
                var resHighDensity = structureSet.CreateAndSearchBody(baseParams);
                MessageBox.Show("Body Searched");


                if (resHighDensity == null)
                {
                    MessageBox.Show("Failed to create high-density structure, no high HU present");
                    // throw new Exception("Failed to create high-density structure, no high HU present.");
                }


                resHighDensity.Color = Color.FromArgb(255, 0, 200, 0);

                // Upsample to high resolution.
                MessageBox.Show("Converting to high resolution");
                ConvertToHighResolutionIfPossible(resHighDensity);

                // Rename the structure since CreateAndSearchBody always creates "BODY"
                //string currentId = highDensityStruct.Id;
                //var renamedStruct = structureSet.AddStructure("CONTROL", structName);
                //renamedStruct.SegmentVolume = highDensityStruct.SegmentVolume;
                //structureSet.RemoveStructure(bodyStruct);

                // Ensure resBodyHR is defined somewhere before using it
                MessageBox.Show("Using custom margin");
                var customMargin = new AxisAlignedMargins(StructureMarginGeometry.Outer, outerMarginX, outerMarginY, outerMarginZ, outerMarginX, outerMarginY, outerMarginZ);

                // Apply asymmetric margin
                MessageBox.Show("Expanding with custom margin");
                resHighDensity.SegmentVolume = resHighDensity.AsymmetricMargin(customMargin);

            }
            catch (Exception ex)
            {
                logAction($"Error in CreateHighDensityStructure: {ex.Message}");
                throw;
            }



        }

        private void ConvertToHighResolutionIfPossible(Structure structure)
        {
            if (structure.CanConvertToHighResolution())
            {
                structure.ConvertToHighResolution();
            }
        }
    }
}
