using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using AutoPlanning;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using Newtonsoft.Json;
using System.Windows.Controls;

// Add this class definition at the top of the file, before the BreastPlanManager class

public class OptimizationHistoryEntry
{
    public string PlanId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Action { get; set; }  // e.g. "Added objective", "Modified structure", etc.
    public Dictionary<string, object> Parameters { get; set; }
    public Dictionary<string, double> DVHMetrics { get; set; }
    public string UserNotes { get; set; }

    public OptimizationHistoryEntry()
    {
        Parameters = new Dictionary<string, object>();
        DVHMetrics = new Dictionary<string, double>();
    }
}

namespace AutoPlanning.TreatmentSites.Breast
{
    //TODO:
    // 1. Add a new class that generates a nice image of the scorecard, also shows plan comparison tool
    // Also need something for physician to pick. Eclipse is ok but why not make it better? Could send an image of the dose distribution 
    // or collection of gifs that show contours, 90/% scrolling, ..100% scrolling. Have it unqiue to each site and team. 
    // Optimize the reinforcement learning!!! 

    public class BreastPlanManager : IPlanManager
    {
        private Patient Patient { get; set; }
        private VMS.TPS.Common.Model.API.Application App { get; set; }
        private string logFilePath = @"P:\Rafe\ABPI PROJECT\ASTRO_2025\Logs\AutoPlanningLog.txt";
        private List<string> LogMessages { get; set; } = new List<string>();
        private BreastStructureManager StructureManager { get; set; }
        private List<OptimizationHistoryEntry> optimizationHistory = new List<OptimizationHistoryEntry>();
        private string historyFilePath;
        public bool IsLeftSided { get; private set; }
        private bool structureMatchingLogged = false;

        public BreastPlanManager(Patient patient, VMS.TPS.Common.Model.API.Application app)
        {
            Patient = patient;
            App = app;
            historyFilePath = Path.Combine(@"P:\Rafe\ABPI PROJECT\ASTRO_2025\Logs\OptimizationHistory", 
                $"{patient.Id}_optimization_history.json");
            
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            Directory.CreateDirectory(Path.GetDirectoryName(historyFilePath));
            
            IsLeftSided = DetermineBreastSide();
        }

        public event Action<string> LogMessageAdded;

        private void LogMessage(string message)
        {
            // Log to file through Logger.Instance
            Logger.Instance.LogGeneral(message);
            
            // Log to UI through event
            LogMessageAdded?.Invoke(message);
        }

        public List<string> GetLogMessages()
        {
            return Logger.Instance.GetLogMessages();
        }

        public string CreateAutoPlan()
        {
            try
            {
                // Get the structure manager
                var structureManager = GetStructureManager();
                
                // Create the body structure (either method works)
                structureManager.CreateBodyStructure();
                // Or for body + support:
                // structureManager.CreateBodyAndSupportStructures();
                
                // Create target structures
                structureManager.CreateTargetExpansions();
                
                // Create the plan
                return CreatePlanOnly();
            }
            catch (Exception ex)
            {
                return $"Error in breast auto-planning: {ex.Message}";
            }
        }

        private ExternalPlanSetup FindOrCreateInitialPlan(bool isLeftSided, StructureSet structureSet = null)
        {
            // Always create a new course for each run
            var existingCourse = Patient.Courses.FirstOrDefault(c => c.Id == "APBI_INI");
            if (existingCourse != null)
            {
                LogMessage("Removing existing APBI_INI course...");
                Patient.RemoveCourse(existingCourse);
            }

            LogMessage("Creating new APBI_INI course and initial plan...");
            
            // Use the provided structure set instead of creating a new one
            if (structureSet == null)
            {
                LogMessage("WARNING: No structure set provided. Will create a new one.");
                structureSet = SetupStructureSet(isLeftSided);
                if (structureSet == null)
                {
                    LogMessage("ERROR: Failed to create structure set.");
                    return null;
                }
            }
            else
            {
                LogMessage($"Using provided structure set: {structureSet.Id}");
            }

            // Create a new course
            Course course = Patient.AddCourse();
            course.Id = "APBI_INI";

            // Create the plan using the specified structure set
            ExternalPlanSetup plan = course.AddExternalPlanSetup(structureSet);
            plan.Id = "Initial";

            SetupBreastPlanParameters(plan);
            AddBreastBeams(plan, Patient, isLeftSided);

            var structureMatches = StructureManager.MatchStructures();
            var targetDoseLevels = StructureManager.SetTargetDoseLevels(plan.TotalDose);

            GenerateRapidPlanEstimates(plan, targetDoseLevels, structureMatches);
            App.SaveModifications();

            AddCustomObjectives(plan);
            App.SaveModifications();

            OptimizeAndCalculateDose(plan);
            App.SaveModifications();

            return plan;
        }

        public string CreatePlanOnly(StructureSet structureSet = null)
        {
            Course course = null;
            try
            {
                LogMessage("Starting breast planning only...");
                
                // If a specific structure set is provided, use it
                if (structureSet != null)
                {
                    LogMessage($"Using provided structure set: {structureSet.Id}");
                    // Update the StructureManager to use the provided structure set
                    StructureManager = new BreastStructureManager(structureSet, IsLeftSided, LogMessage);
                }
                else
                {
                    // Find the most recently modified structure set
                    LogMessage("No structure set provided. Finding most recently modified structure set...");
                    
                    // Refresh patient data to ensure we have the latest changes
                    Patient.BeginModifications();
                    
                    var structureSets = Patient.StructureSets.ToList();
                    if (structureSets.Count > 0)
                    {
                        // Sort by last modified date (most recent first)
                        var latestStructureSet = structureSets
                            .OrderByDescending(ss => ss.HistoryDateTime)
                            .FirstOrDefault();
                        
                        if (latestStructureSet != null)
                        {
                            LogMessage($"Using most recently modified structure set: {latestStructureSet.Id} (modified: {latestStructureSet.HistoryDateTime})");
                            StructureManager = new BreastStructureManager(latestStructureSet, IsLeftSided, LogMessage);
                            structureSet = latestStructureSet;
                        }
                        else
                        {
                            LogMessage("Could not find a valid structure set. Using default structure set creation.");
                        }
                    }
                }
                
                // Verify that we have a structure set to work with
                if (structureSet == null)
                {
                    return "No valid structure set found. Please select a structure set.";
                }
                
                // Log all available structures for debugging
                LogMessage("Available structures in selected structure set:");
                foreach (var structure in structureSet.Structures)
                {
                    LogMessage($"  - {structure.Id} ({structure.DicomType}) - Empty: {structure.IsEmpty}");
                }
                
                // Pass the structure set to FindOrCreateInitialPlan
                LogMessage("Proceeding with plan creation using available structures.");
                ExternalPlanSetup initialPlan = FindOrCreateInitialPlan(IsLeftSided, structureSet);
                if (initialPlan == null)
                {
                    return "Failed to find or create initial breast plan.";
                }

                App.SaveModifications();

                LogMessage($"Using plan: {initialPlan.Id}. Starting optimization iterations.");

                // Use the initial plan as the final plan since we've already optimized it
                ExternalPlanSetup finalPlan = initialPlan;

                // Move DVH evaluation to after optimization is complete
                if (finalPlan != null && finalPlan.IsDoseValid)
                {
                    GenerateDVHMetrics(finalPlan);
                }

                App.SaveModifications();

                LogMessage("Breast planning completed successfully.");
                return $"Breast planning completed successfully. Final plan ID: {finalPlan.Id}";
            }
            catch (Exception ex)
            {
                return $"Error in breast planning: {ex.Message}";
            }
        }

        private StructureSet SetupStructureSet(bool isLeftSided)
        {
            // Find the AI OAR structure set
            var TPStructureSet = Patient.StructureSets.FirstOrDefault(x => x.Id.Contains("AICT"));
            if (TPStructureSet == null)
            {
                LogMessage("Failed to find AI OAR structure set.");
                return null;
            }

            var image = TPStructureSet.Image;
            var structureSet = image.CreateNewStructureSet();
            if (structureSet == null)
            {
                LogMessage("Failed to create new structure set.");
                return null;
            }

            // Generate a unique ID for the new structure set
            string ssName = TPStructureSet.Id.Replace("AICT", "CT");
            int i = 0;
            while (Patient.StructureSets.Any(x => x.Id == ssName))
            {
                ssName = ssName.Substring(0, 9) + "_" + i.ToString();
                i++;
                if (i > 9)
                {
                    throw new ApplicationException($"Unable to create structure set named {ssName}. Please delete or rename existing structure sets.");
                }
            }
            structureSet.Id = ssName;

            StructureManager = new BreastStructureManager(structureSet, isLeftSided, LogMessage);
            StructureManager.SetupStructureSet();

            App.SaveModifications();

            return structureSet;
        }
        private bool GetBreastSide()
        {
            MessageBoxResult result = MessageBox.Show("Is this a left-sided breast case?", "Breast Side Selection", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
        private void SetupBreastPlanParameters(ExternalPlanSetup plan)
        {
            // Set breast-specific plan parameters
            var rx_total = new DoseValue(3000, DoseValue.DoseUnit.cGy);
            var rx_per = new DoseValue(600, DoseValue.DoseUnit.cGy);
            plan.SetPrescription(5, rx_per, 1.0);

            // Set target structure
            var target = plan.StructureSet.Structures.FirstOrDefault(s => s.Id == "(R) GTV_AI");
            if (target != null)
            {
                plan.SetTargetStructureIfNoDose(target, new StringBuilder());
            }
        }

        private void AddBreastBeams(ExternalPlanSetup plan, Patient patient, bool isLeftSided)
        {
            // Add breast-specific beams
            var machine = "PPMC_Linac_1215";
            string energy = "6X";
            int doseRate = 800;
            var technique = "ARC";
            var arc_ebmp = new ExternalBeamMachineParameters(machine, energy, doseRate, technique, "FFF");
            var static_ebmp = new ExternalBeamMachineParameters(machine, energy, doseRate, "STATIC", "FFF");

            // Add couch structures assuming HFS, Radnor patient no prior plan...
            if (plan.StructureSet.CanRemoveCouchStructures(out string error))
            {
                plan.StructureSet.RemoveCouchStructures(out IReadOnlyList<string> removedStructureIds, out error);
                plan.StructureSet.AddCouchStructures("RDS_Couch_Top", PatientOrientation.HeadFirstSupine, RailPosition.In, RailPosition.In, null, null, null, out var addedStructures, out var imageResized, out error);
            }

            // Get isocenter
            VVector isocenter = GetBreastIsocenter(patient);

            // Add VMAT arcs
            double[] metersetWeights = { 0, .05, .1, .15, .2, .25, .3, .35, .4, .45, .5, .55, .6, .65, .7, .75, .8, .85, .9, .95, 1 };

            if (isLeftSided)
            {
                plan.AddVMATBeamForFixedJaws(arc_ebmp, metersetWeights, 330, 315, 160, GantryDirection.Clockwise, 0, isocenter);
                plan.AddVMATBeamForFixedJaws(arc_ebmp, metersetWeights, 30, 160, 315, GantryDirection.CounterClockwise, 0, isocenter);
                plan.AddVMATBeamForFixedJaws(arc_ebmp, metersetWeights, 60, 315, 160, GantryDirection.Clockwise, 0, isocenter);
            }
            else
            {
                plan.AddVMATBeamForFixedJaws(arc_ebmp, metersetWeights, 330, 45, 200, GantryDirection.CounterClockwise, 0, isocenter);
                plan.AddVMATBeamForFixedJaws(arc_ebmp, metersetWeights, 30, 200, 45, GantryDirection.Clockwise, 0, isocenter);
                plan.AddVMATBeamForFixedJaws(arc_ebmp, metersetWeights, 60, 45, 200, GantryDirection.CounterClockwise, 0, isocenter);
            }

            // Add imaging beam
            var target_ss = plan.StructureSet.Structures.FirstOrDefault(s => s.Id == "(R) GTV_AI");
            plan.AddImagingSetup(static_ebmp,
                new ImagingBeamSetupParameters(ImagingSetup.kVCBCT, 200, 200, 200, 200, 200, 200), target_ss);
        }
        private VVector GetBreastIsocenter(Patient patient)
        {
            // Logic to determine breast isocenter
            // This is a placeholder. Eventually will be expanded to make a "smart" isocenter placement
            // For now expects "sim" course that contains ISO_setup plan
            var iso_plan = patient.Courses.FirstOrDefault(x => x.Id.Contains("sim")).ExternalPlanSetups.FirstOrDefault(x => x.Id.Contains("ISO_SETUP"));
            var iso_beam = iso_plan.Beams.FirstOrDefault();
            var iso_point = iso_beam.IsocenterPosition;

            return iso_point;
        }

        private void GenerateRapidPlanEstimates(ExternalPlanSetup plan, Dictionary<string, DoseValue> targetDoseLevels, Dictionary<string, string> structureMatches)
        {
            try
            {
                // Only log structure matching debug info if it hasn't been logged already
                if (!structureMatchingLogged)
                {
                    LogMessage("\n=== RapidPlan Structure Matching Debug ===");
                    LogMessage("Structure Matches:");
                    foreach (var match in structureMatches)
                    {
                        LogMessage($"  {match.Key} -> {match.Value}");
                    }

                    LogMessage("\nTarget Dose Levels:");
                    foreach (var target in targetDoseLevels)
                    {
                        LogMessage($"  {target.Key} -> {target.Value.Dose} {target.Value.Unit}");
                    }

                    LogMessage("\nAvailable Structures in Plan:");
                    foreach (var structure in plan.StructureSet.Structures)
                    {
                        LogMessage($"  {structure.Id} ({structure.DicomType})");
                    }
                    
                    structureMatchingLogged = true;
                }

                LogMessage("\nAttempting to apply RapidPlan model...");
                plan.CalculateDVHEstimates("UPENN Breast APBI", targetDoseLevels, structureMatches);
                LogMessage("RapidPlan model applied successfully.");
            }
            catch (Exception ex)
            {
                LogMessage($"Error in GenerateRapidPlanEstimates: {ex.Message}");
                throw;
            }
        }

        public void AddCustomObjectives(ExternalPlanSetup plan)
        {
            var customObjectives = new Dictionary<string, List<ObjectiveParameters>>
        {
        { "Rind_90", new List<ObjectiveParameters>
            {
                //new ObjectiveParameters("gEUD", 2700, 0, 0, 40)
            }
        },
        // Add more structures and objectives as needed
        { "PTV_Eval", new List<ObjectiveParameters>
            { 
            //new ObjectiveParameters("upper", 3160, 2, 0),
            //new ObjectiveParameters("lower", 3030, 99, 0),
            //new ObjectiveParameters("lower", 3045, 98, 0),
            //new ObjectiveParameters("lower", 3060, 97, 0),
            //new ObjectiveParameters("gEUD", 2850, 0, 0, 5)
            } 
        },
        { "BODY-PTV+2cm", new List<ObjectiveParameters>
            { 
            //new ObjectiveParameters("gEUD", 1000, 0, 0, 30)
            } 
        }
        };
            LogMessage("Adding custom objectives...");

            foreach (var structureObj in customObjectives)
            {
                var structure = plan.StructureSet.Structures.FirstOrDefault(s => s.Id == structureObj.Key);
                if (structure != null)
                {
                    foreach (var obj in structureObj.Value)
                    {
                        try
                        {
                            AddObjective(plan, structure, obj);
                            //LogMessage($"Added {obj.Type} objective for structure: {structure.Id}");
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Error adding {obj.Type} objective for structure {structure.Id}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    LogMessage($"Warning: Structure {structureObj.Key} not found. Skipping custom objectives.");
                }
            }
        }

        private void AddObjective(ExternalPlanSetup plan, Structure structure, ObjectiveParameters obj)
        {
            var dose = new DoseValue(obj.Dose, DoseValue.DoseUnit.cGy);
            switch (obj.Type.ToLower())
            {
                case "upper":
                    plan.OptimizationSetup.AddPointObjective(structure, OptimizationObjectiveOperator.Upper, dose, obj.Volume, obj.Priority);
                    break;
                case "lower":
                    plan.OptimizationSetup.AddPointObjective(structure, OptimizationObjectiveOperator.Lower, dose, obj.Volume, obj.Priority);
                    break;
                case "geud":
                    plan.OptimizationSetup.AddEUDObjective(structure, OptimizationObjectiveOperator.Upper, dose, obj.Alpha, obj.Priority);
                    break;
                case "mean":
                    plan.OptimizationSetup.AddMeanDoseObjective(structure, dose, obj.Priority);
                    break;
                default:
                    throw new ArgumentException($"Unsupported objective type: {obj.Type}");
            }
        }

        // -------------------------------------------------------------------
        public class ObjectiveParameters
        {
            public string Type { get; }
            public double Dose { get; }
            public double Volume { get; }
            public double Priority { get; }
            public double Alpha { get; }

            public ObjectiveParameters(string type, double dose, double volume, double priority, double alpha = 0)
            {
                Type = type;
                Dose = dose;
                Volume = volume;
                Priority = priority;
                Alpha = alpha;
            }
        }

        public class PlanModificationStep
        {
            public string Operation { get; set; }  // e.g., "AddObjective", "CreateStructure", "ModifyStructure"
            public string StructureId { get; set; }  // Target structure ID
            public string Type { get; set; }  // For objectives: "upper", "lower", "mean", "geud"
            public double Dose { get; set; }
            public double Volume { get; set; }
            public double Priority { get; set; }
            public Dictionary<string, object> AdditionalParams { get; set; }  // For operation-specific parameters

            public PlanModificationStep()
            {
                AdditionalParams = new Dictionary<string, object>();
            }
        }

        public class OptimizationSettings
        {
            public int MaxAttempts { get; set; }
            public double TargetHotspot { get; set; }
            public double InitialHotspotThreshold { get; set; }
            public double HotspotReduction { get; set; }
            public string OptimizationMLCType { get; set; }
            public List<PlanModificationStep> ModificationSteps { get; set; }

            public static OptimizationSettings Default => new OptimizationSettings
            {
                MaxAttempts = 0,  // Changed to 0 to only run RapidPlan initially
                TargetHotspot = 105,
                InitialHotspotThreshold = 107.5,
                HotspotReduction = 0.5,
                OptimizationMLCType = "SX2 MLC",
                ModificationSteps = new List<PlanModificationStep>()
            };

            // Example JSON format:
            /*
            {
                "MaxAttempts": 2,
                "TargetHotspot": 105,
                "InitialHotspotThreshold": 107.5,
                "HotspotReduction": 0.5,
                "OptimizationMLCType": "SX2 MLC",
                "ModificationSteps": [
                    {
                        "Operation": "AddObjective",
                        "StructureId": "PTV_Eval",
                        "Type": "upper",
                        "Dose": 3150,
                        "Volume": 0,
                        "Priority": 150
                    },
                    {
                        "Operation": "CreateStructure",
                        "StructureId": "Ring_2cm",
                        "Type": "CONTROL",
                        "AdditionalParams": {
                            "BaseStructure": "PTV_Eval",
                            "InnerMargin": 0.2,
                            "OuterMargin": 2.0
                        }
                    }
                ]
            }
            */

            public static OptimizationSettings LoadSettings()
            {
                // Get the path to a directory outside of the program directory
                string configDir = @"P:\Ledi__Wang\AutoPlanning";
                string configPath = Path.Combine(configDir, "optimization_settings.json");

                try
                {
                    if (!Directory.Exists(configDir))
                    {
                        Directory.CreateDirectory(configDir);
                    }

                    if (!File.Exists(configPath))
                    {
                        // Create default settings file if it doesn't exist
                        File.WriteAllText(configPath, JsonConvert.SerializeObject(Default, Newtonsoft.Json.Formatting.Indented));
                        return Default;
                    }

                    // Read the JSON content from the file
                    string jsonContent = File.ReadAllText(configPath);

                    // Deserialize the JSON into a List of dictionaries (or objects) representing optimization steps
                    // Assuming each step in the JSON is a dictionary with keys: "Operation", "StructureId", "Type", "Dose", "Volume", and "Priority"
                    var optimizationSteps = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonContent);

                    if (optimizationSteps == null || optimizationSteps.Count == 0)
                    {
                        return Default;
                    }

                    OptimizationSettings settings = new OptimizationSettings
                    {
                        MaxAttempts = Default.MaxAttempts,
                        TargetHotspot = Default.TargetHotspot,
                        InitialHotspotThreshold = Default.InitialHotspotThreshold,
                        HotspotReduction = Default.HotspotReduction,
                        OptimizationMLCType = Default.OptimizationMLCType,
                        ModificationSteps = new List<PlanModificationStep>() // Initialize the list
                    };

                    // Iterate through the list of optimization steps
                    foreach (var step in optimizationSteps)
                    {
                        // Extract values from the dictionary (they will be cast to the correct type)
                        var operation = step["Operation"].ToString();
                        var structureId = step["StructureId"].ToString();
                        var type = step["Type"].ToString();
                        var dose = Convert.ToDouble(step["Dose"]);
                        var volume = Convert.ToDouble(step["Volume"]);
                        var priority = Convert.ToDouble(step["Priority"]);

                        // Create a new PlanModificationStep object and add it to the list
                        settings.ModificationSteps.Add(new PlanModificationStep
                        {
                            Operation = operation,
                            StructureId = structureId,
                            Type = type,
                            Dose = dose,
                            Volume = volume,
                            Priority = priority
                        });
                    }

                    // Validate settings and use defaults for any missing or invalid values
                    ValidateAndApplyDefaults(settings);

                    return settings;
                }
                catch (Exception ex)
                {
                    // Log the error but don't crash - use default settings instead
                    File.AppendAllText(
                        Path.Combine(configDir, "error.log"),
                        $"{DateTime.Now}: Error loading settings: {ex.Message}\n"
                    );
                    return Default;
                }
            }

            private static void ValidateAndApplyDefaults(OptimizationSettings settings)
            {
                if (settings.MaxAttempts <= 0) settings.MaxAttempts = Default.MaxAttempts;
                if (settings.TargetHotspot <= 0) settings.TargetHotspot = Default.TargetHotspot;
                if (settings.InitialHotspotThreshold <= 0) settings.InitialHotspotThreshold = Default.InitialHotspotThreshold;
                if (settings.HotspotReduction <= 0) settings.HotspotReduction = Default.HotspotReduction;
                if (string.IsNullOrEmpty(settings.OptimizationMLCType)) settings.OptimizationMLCType = Default.OptimizationMLCType;
                if (settings.ModificationSteps == null || settings.ModificationSteps.Count == 0)
                {
                    settings.ModificationSteps = Default.ModificationSteps;
                }
            }
        }
        // -------------------------------------------------------------------
      
        private void OptimizeAndCalculateDose(ExternalPlanSetup plan)
        {
            LogMessage("Starting the optimization process with RapidPlan model UPENN Breast APBI...");
            
            try
            {
                // Set calculation model to Acuros XB
                plan.SetCalculationModel(CalculationType.PhotonVolumeDose, "AcurosXB_1610");
                LogMessage("Set calculation model to AcurosXB_1610");
                
                // Ensure we have the structures matched correctly
                var structureMatches = StructureManager.MatchStructures();
                var targetDoseLevels = StructureManager.SetTargetDoseLevels(plan.TotalDose);
                
                // Generate RapidPlan estimates
                LogMessage("Generating RapidPlan estimates...");
                plan.CalculateDVHEstimates("UPENN Breast APBI", targetDoseLevels, structureMatches);
                
                // Add normal tissue objective
                plan.OptimizationSetup.AddNormalTissueObjective(120, 2, 100, 50, 0.25);
                
                LogMessage("Starting VMAT optimization...");
                plan.OptimizeVMAT();

                LogMessage("Starting dose calculation with Acuros XB...");
                
                try
                {
                    plan.CalculateDose();
                    LogMessage("Initial RapidPlan optimization completed.");
                    
                    // Create a copy of the plan with AAA algorithm for comparison
                    CreateAAAComparisonPlan(plan);
                }
                catch (Exception doseCalcEx)
                {
                    if (doseCalcEx.Message.Contains("high density") || doseCalcEx.Message.Contains("HU"))
                    {
                        LogMessage("\nERROR: High-density artifacts detected in the CT image.");
                        LogMessage("Please follow these manual steps to resolve the issue:");
                        LogMessage("1. In Eclipse, create a new structure named 'HighDensity'");
                        LogMessage("2. Use the 'Segment High Density Artifacts' tool (right-click on structure)");
                        LogMessage("3. Assign 'Titanium' material to this structure");
                        LogMessage("4. Run the planning process again");
                        
                        // Re-throw the exception to be handled by the calling method
                        throw;
                    }
                    else
                    {
                        // For other errors, just log and re-throw
                        LogMessage($"Error during dose calculation: {doseCalcEx.Message}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error during optimization: {ex.Message}");
                throw; // Re-throw the exception to be handled by the calling method
            }
        }

        private void CreateAAAComparisonPlan(ExternalPlanSetup originalPlan)
        {
            try
            {
                LogMessage("Creating AAA comparison plan...");
                
                // Copy the original plan
                var aaaPlan = originalPlan.Course.CopyPlanSetup(originalPlan);
                if (aaaPlan is ExternalPlanSetup externalPlan)
                {
                    // Generate a unique ID for the AAA plan
                    string baseId = originalPlan.Id + "_AAA";
                    string uniqueId = baseId;
                    int counter = 1;
                    
                    // Check if the ID already exists and generate a unique one if needed
                    while (originalPlan.Course.ExternalPlanSetups.Any(p => p.Id == uniqueId))
                    {
                        uniqueId = $"{baseId}_{counter}";
                        counter++;
                    }
                    
                    // Rename the plan to indicate AAA calculation with the unique ID
                    externalPlan.Id = uniqueId;
                    LogMessage($"Created AAA comparison plan with ID: {uniqueId}");
                    
                    // Set the calculation model to AAA
                    externalPlan.SetCalculationModel(CalculationType.PhotonVolumeDose, "AAA_1610");
                    LogMessage("Set calculation model to AAA_1610");
                    
                    // Recalculate dose with AAA
                    externalPlan.CalculateDose();
                    LogMessage($"AAA comparison plan '{externalPlan.Id}' created and calculated successfully.");
                    
                    // Save the changes
                    App.SaveModifications();
                }
                else
                {
                    LogMessage("Failed to create AAA comparison plan: Copied plan is not an ExternalPlanSetup.");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error creating AAA comparison plan: {ex.Message}");
            }
        }

        private Structure CreateStructureFromIsodose(ExternalPlanSetup plan, StructureSet structureSet, double isodoseLevel)
        {
            try
            {
                string structureName = $"Hotspot_{isodoseLevel:F0}";
                LogMessage($"Attempting to create structure '{structureName}' from {isodoseLevel}% isodose level");

                // Check if the structure already exists and delete it if it does
                var existingStructure = structureSet.Structures.FirstOrDefault(s => s.Id == structureName);
                if (existingStructure != null)
                {
                    structureSet.RemoveStructure(existingStructure);
                    LogMessage($"Removed existing structure '{structureName}'");
                }

                // Create the new structure
                var hotspotStructure = structureSet.AddStructure("DOSE_REGION", structureName);

                // Create the structure from the isodose using percentage
                DoseValue doseValue = new DoseValue(isodoseLevel, DoseValue.DoseUnit.Percent);
                hotspotStructure.ConvertDoseLevelToStructure(plan.Dose, doseValue);

                // Check if the resulting structure is valid
                if (hotspotStructure.IsEmpty)
                {
                    LogMessage($"Created hotspot structure is empty for isodose level {isodoseLevel}%");
                    structureSet.RemoveStructure(hotspotStructure);
                    return null;
                }

                LogMessage($"Successfully created hotspot structure for isodose level {isodoseLevel}% with volume {hotspotStructure.Volume:F2} cc");
                return hotspotStructure;
            }
            catch (Exception ex)
            {
                LogMessage($"Error creating structure from isodose {isodoseLevel}%: {ex.Message}\nStack Trace: {ex.StackTrace}");
                return null;
            }
        }

        public ExternalPlanSetup IterateAndOptimizePlan(ExternalPlanSetup initialPlan)
        {
            // Load optimization settings and objectives from external file
            var settings = OptimizationSettings.LoadSettings();
            LogMessage("Starting plan optimization with RapidPlan model...");

            if (settings.MaxAttempts == 0)
            {
                LogMessage("Running initial RapidPlan optimization only (no additional iterations)");
                OptimizeAndCalculateDose(initialPlan);
                return initialPlan;
            }

            LogMessage($"------------ STEPS STEPS {settings.ModificationSteps.Count}");
            // iterate through the optimization steps log each optimizationStep type, dose, volume, priority
            foreach (var optimizationStep in settings.ModificationSteps)
            {
                LogMessage($"----------- Optimization Step: {optimizationStep.Operation}, {optimizationStep.StructureId}, {optimizationStep.Type}, {optimizationStep.Dose}, {optimizationStep.Volume}, {optimizationStep.Priority}");
            }

            int maxAttempts = settings.MaxAttempts;
            double targetHotspot = settings.TargetHotspot;
            double initialHotspotThreshold = settings.InitialHotspotThreshold;
            double hotspotReduction = settings.HotspotReduction;

            ExternalPlanSetup currentPlan = initialPlan;
            int iterationCount = 0;

            while (iterationCount < maxAttempts)
            {
                iterationCount++;
                var maxDose = currentPlan.Dose.DoseMax3D;
                // optimization step at iteration, if there exist one in the settings
                if (settings.ModificationSteps.Count < iterationCount)
                {
                    LogMessage($"No optimization step found for iteration {iterationCount}. Stopping optimization.");
                    break;
                }
                var optimizationStep = settings.ModificationSteps[iterationCount - 1];

                if (maxDose.Dose <= targetHotspot)
                {
                    LogMessage($"Target hotspot of {targetHotspot}% achieved. Stopping optimization.");
                    break;
                }

                LogMessage($"Iteration {iterationCount}: Plan has a hotspot of {maxDose.Dose:F1}%. Attempting to reduce hotspot.");

                var newPlanName = $"APBI_{iterationCount}";
                currentPlan = CopyPlan(currentPlan, newPlanName);
                if (currentPlan == null)
                {
                    LogMessage("Failed to create new plan iteration. Stopping optimization.");
                    break;
                }

                double currentHotspotLevel = Math.Round(Math.Min(maxDose.Dose, initialHotspotThreshold) - (iterationCount - 1) * hotspotReduction, 1);
                var hotspotStructure = CreateStructureFromIsodose(currentPlan, currentPlan.StructureSet, currentHotspotLevel);

                if (hotspotStructure == null)
                {
                    LogMessage("Failed to create a valid hotspot structure. Stopping optimization attempts.");
                    break;
                }

                App.SaveModifications();
                // optimize with the optimization objectives at this iteration
                ReoptimizePlan(currentPlan, hotspotStructure, currentPlan.TotalDose.Dose, currentHotspotLevel, optimizationStep);
                App.SaveModifications();
                currentPlan.CalculateDose();

                maxDose = currentPlan.Dose.DoseMax3D;
                LogMessage($"After reoptimization attempt {iterationCount}, the maximum dose is now {maxDose.Dose:F1}%.");
            }

            LogMessage("Plan optimization complete. Please review the final plan.");
            return currentPlan;
        }

        private ExternalPlanSetup CopyPlan(ExternalPlanSetup sourcePlan, string newPlanName)
        {
            try
            {
                var course = sourcePlan.Course;
                var copiedPlan = course.CopyPlanSetup(sourcePlan);
                if (copiedPlan is ExternalPlanSetup externalPlan)
                {
                    externalPlan.Id = newPlanName;
                    LogMessage($"Successfully copied plan. New plan ID: {newPlanName}");
                    return externalPlan;
                }
                else
                {
                    LogMessage("Copied plan is not an ExternalPlanSetup.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error copying plan: {ex.Message}");
                return null;
            }
        }

        // added optimization step to the function
        private void ReoptimizePlan(ExternalPlanSetup plan, Structure hotspotStructure, double prescribedDose, double hotspot, PlanModificationStep optimizationStep)
        {
            try
            {
                if (hotspotStructure == null || hotspotStructure.IsEmpty)
                {
                    LogMessage($"Hotspot structure is not valid for optimization. Skipping additional optimization.");
                    return;
                }

                var optimizationSetup = plan.OptimizationSetup;

                // Remove any existing objectives for the hotspot structure
                var existingObjectives = optimizationSetup.Objectives.Where(o => o.StructureId == hotspotStructure.Id).ToList();
                foreach (var obj in existingObjectives)
                {
                    optimizationSetup.RemoveObjective(obj);
                }

                // Calculate the absolute dose for 105% of the prescribed dose
                double hotspotDose = prescribedDose * hotspot / 100; // 105% of prescribed dose

                // Add a new upper objective for the hotspot structure
                // Use the AddObjective function to add the objective
                var priorityHot = 175;
                AddObjective(plan, hotspotStructure, new ObjectiveParameters(optimizationStep.Type, hotspotDose, 0, priorityHot));

                // Run VMAT optimization from current optimization state
                var OptimizationOptionsVMAT = new OptimizationOptionsVMAT(OptimizationOption.ContinueOptimizationWithPlanDoseAsIntermediateDose, "SX2 MLC");
                plan.OptimizeVMAT(OptimizationOptionsVMAT);

                // Calculate the dose
                plan.CalculateDose();

                LogMessage("Plan reoptimization completed.");
            }
            catch (Exception ex)
            {
                LogMessage($"Error during plan reoptimization: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        public void GenerateDVHMetrics(ExternalPlanSetup plan)
        {
            try
            {
                if (!plan.IsDoseValid)
                {
                    LogMessage("Plan dose is not valid. Cannot generate DVH metrics.");
                    return;
                }

                // Initialize StructureManager if it's null
                if (StructureManager == null)
                {
                    LogMessage("Initializing StructureManager for evaluation...");
                    StructureManager = new BreastStructureManager(plan.StructureSet, IsLeftSided, LogMessage);
                }

                StringBuilder metricsReport = new StringBuilder();
                
                // Add plan details header
                metricsReport.AppendLine("==============================================");
                metricsReport.AppendLine($"DVH Evaluation for Plan: {plan.Id}");
                metricsReport.AppendLine($"Course: {plan.Course.Id}");
                metricsReport.AppendLine($"Created: {plan.CreationDateTime:g}");
                metricsReport.AppendLine($"Last Modified: {plan.HistoryDateTime:g}");
                metricsReport.AppendLine($"Total Dose: {plan.TotalDose.Dose:F1} {plan.TotalDose.UnitAsString}");
                metricsReport.AppendLine($"Number of Fractions: {plan.NumberOfFractions}");
                metricsReport.AppendLine($"Side: {(IsLeftSided ? "Left" : "Right")}-sided breast case");
                metricsReport.AppendLine("==============================================");
                metricsReport.AppendLine();
                metricsReport.AppendLine("DVH Metrics and Score Card Evaluation:");
                metricsReport.AppendLine("```");

                // Update PTV evaluation to check for both old and new structure names
                var ptv = plan.StructureSet.Structures.FirstOrDefault(s => 
                    (s.Id == "(R) PTV_AI_Eval" || s.Id == "PTV_Eval") && !s.IsEmpty);

                if (ptv != null)
                {
                    // PTV D95
                    var ptvD95 = plan.GetDoseAtVolume(ptv, 95.0, VolumePresentation.Relative, DoseValuePresentation.Absolute);
                    metricsReport.AppendLine($"[{EvaluateMetric(ptvD95.Dose, 2850, 2700, true)}] PTV D95: Goal > 2850 cGy -- Acceptable > 2700 cGy: {ptvD95.Dose:F2}");

                    // PTV D0.03cc
                    var ptvD003 = plan.GetDoseAtVolume(ptv, 0.03, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Absolute);
                    metricsReport.AppendLine($"[{EvaluateMetric(ptvD003.Dose, 3150, 3150, false)}] PTV D0.03cc: Goal < 3150 cGy: {ptvD003.Dose:F2}");
                }
                else
                {
                    LogMessage("WARNING: PTV structure not found for evaluation");
                    metricsReport.AppendLine("[ERROR] PTV metrics could not be evaluated - structure not found");
                }

                // Body evaluation
                var Bodystruct = StructureManager.GetStructureFromNameList(plan.StructureSet, "BODY");
                var BodymaxDose = plan.GetDoseAtVolume(Bodystruct, 0.03, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Absolute);

                metricsReport.AppendLine($"[{EvaluateMetric(BodymaxDose.Dose, 3150, 3150, false)}] Body D0.03cc: Goal < 3150 cGy: {BodymaxDose.Dose:F2}");

                // Heart evaluation
                var Heartstruct = StructureManager.GetStructureFromNameList(plan.StructureSet, "Heart");
                var Heartv3Gy = plan.GetVolumeAtDose(Heartstruct, new DoseValue(300, DoseValue.DoseUnit.cGy), VolumePresentation.Relative);
                var HeartmeanDose = plan.GetDVHCumulativeData(Heartstruct, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 1).MeanDose;

                metricsReport.AppendLine($"[{EvaluateMetric(Heartv3Gy, 700, 1000, false)}] Heart V3Gy: Goal < 700 cGy Accept < 1000 cGy: {Heartv3Gy:F2}");
                metricsReport.AppendLine($"[{EvaluateMetric(HeartmeanDose.Dose, 100, 200, false)}] Heart Mean Dose: Goal < 100 cGy Accept < 200 cGy: {HeartmeanDose.Dose:F2}");

                // Ipsilateral Breast evaluation
                var ipsiBreastStruct = StructureManager.GetStructureFromNameList(plan.StructureSet, IsLeftSided ? "Breast_L" : "Breast_R");
                var ipsiBreast15Gy = plan.GetVolumeAtDose(ipsiBreastStruct, new DoseValue(1500, DoseValue.DoseUnit.cGy), VolumePresentation.Relative);
                var ipsiBreast28Gy = plan.GetVolumeAtDose(ipsiBreastStruct, new DoseValue(2850, DoseValue.DoseUnit.cGy), VolumePresentation.Relative);

                metricsReport.AppendLine($"[{EvaluateMetric(ipsiBreast15Gy, 50, 55, false)}] {(IsLeftSided ? "Left" : "Right")} Breast V15Gy: Goal <50 Accept <55: {ipsiBreast15Gy:F2}");
                metricsReport.AppendLine($"[{EvaluateMetric(ipsiBreast28Gy, 25, 30, false)}] {(IsLeftSided ? "Left" : "Right")} Breast V28.5Gy: Goal <25 Accept <30: {ipsiBreast28Gy:F2}");

                // Contralateral Breast evaluation
                var contraBreastStruct = StructureManager.GetStructureFromNameList(plan.StructureSet, IsLeftSided ? "Breast_R" : "Breast_L");
                var contraBreastMaxDose = plan.GetDoseAtVolume(contraBreastStruct, 0.03, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Absolute);

                metricsReport.AppendLine($"[{EvaluateMetric(contraBreastMaxDose.Dose, 100, 300, false)}] {(IsLeftSided ? "Right" : "Left")} Breast D0.03cc: Goal < 100 cGy Accept < 300 cGy: {contraBreastMaxDose.Dose:F2}");

                // Ipsilateral Lung evaluation
                var ipsiLungStruct = StructureManager.GetStructureFromNameList(plan.StructureSet, IsLeftSided ? "Lung_L" : "Lung_R");
                var ipsiLung10Gy = plan.GetVolumeAtDose(ipsiLungStruct, new DoseValue(1000, DoseValue.DoseUnit.cGy), VolumePresentation.Relative);
                var ipsiLung5Gy = plan.GetVolumeAtDose(ipsiLungStruct, new DoseValue(500, DoseValue.DoseUnit.cGy), VolumePresentation.Relative);

                metricsReport.AppendLine($"[{EvaluateMetric(ipsiLung10Gy, 15, 20, false)}] {(IsLeftSided ? "Left" : "Right")} Lung V10Gy: Goal < 15% Accept < 20%: {ipsiLung10Gy:F2}");
                metricsReport.AppendLine($"[{EvaluateMetric(ipsiLung5Gy, 50, 55, false)}] {(IsLeftSided ? "Left" : "Right")} Lung V5Gy: Goal < 50% Accept < 55%: {ipsiLung5Gy:F2}");

                // Contralateral Lung evaluation
                var contraLungStruct = StructureManager.GetStructureFromNameList(plan.StructureSet, IsLeftSided ? "Lung_R" : "Lung_L");
                var contraLung5Gy = plan.GetVolumeAtDose(contraLungStruct, new DoseValue(500, DoseValue.DoseUnit.cGy), VolumePresentation.Relative);

                metricsReport.AppendLine($"[{EvaluateMetric(contraLung5Gy, 10, 15, false)}] {(IsLeftSided ? "Right" : "Left")} Lung V5Gy: Goal < 10% Accept < 15%: {contraLung5Gy:F2}");

                metricsReport.AppendLine("```");
                metricsReport.AppendLine($"Evaluation completed at: {DateTime.Now:g}");
                metricsReport.AppendLine("==============================================\n");

                LogMessage(metricsReport.ToString());

                // After evaluating the current plan, save comparison for all plans
                SavePlanComparisonMetrics(plan.Course);
            }
            catch (Exception ex)
            {
                LogMessage($"Error generating DVH metrics: {ex.Message}\nStack trace: {ex.StackTrace}");
                throw;
            }
        }
        private string EvaluateMetric(double value, double goalValue, double acceptableValue, bool isMinimum)
        {
            if (isMinimum)
            {
                if (value >= goalValue)
                    return "PASS";
                else if (value >= acceptableValue)
                    return "ACCEPTABLE";
                else
                    return "FAIL";
            }
            else
            {
                if (value <= goalValue)
                    return "PASS";
                else if (value <= acceptableValue)
                    return "ACCEPTABLE";
                else
                    return "FAIL";
            }
        }

        private void ApplyPlanModification(ExternalPlanSetup plan, PlanModificationStep step)
        {
            switch (step.Operation.ToLower())
            {
                case "addobjective":
                    var structure = plan.StructureSet.Structures.FirstOrDefault(s => s.Id == step.StructureId);
                    if (structure != null)
                    {
                        AddObjective(plan, structure, 
                            new ObjectiveParameters(step.Type, step.Dose, step.Volume, step.Priority));
                        LogMessage($"Added {step.Type} objective to {step.StructureId}");
                    }
                    break;

                case "createstructure":
                    // Example of creating a ring structure
                    if (step.AdditionalParams.ContainsKey("BaseStructure"))
                    {
                        var baseStruct = plan.StructureSet.Structures.FirstOrDefault(
                            s => s.Id == step.AdditionalParams["BaseStructure"].ToString());
                        if (baseStruct != null)
                        {
                            var newStructure = plan.StructureSet.AddStructure(step.Type, step.StructureId);
                            // Add structure creation logic based on AdditionalParams
                            LogMessage($"Created structure {step.StructureId}");
                        }
                    }
                    break;

                // Add more operations as needed
                default:
                    LogMessage($"Unknown operation: {step.Operation}");
                    break;
            }
        }

        private void SaveOptimizationHistory()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(historyFilePath));
                string json = JsonConvert.SerializeObject(optimizationHistory, Formatting.Indented);
                File.WriteAllText(historyFilePath, json);
                LogMessage($"Saved optimization history to {historyFilePath}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error saving optimization history: {ex.Message}");
            }
        }

        private void LogOptimizationStep(ExternalPlanSetup plan, string action, Dictionary<string, object> parameters = null)
        {
            var entry = new OptimizationHistoryEntry
            {
                PlanId = plan.Id,
                Timestamp = DateTime.Now,
                Action = action,
                Parameters = parameters ?? new Dictionary<string, object>(),
                DVHMetrics = CollectCurrentDVHMetrics(plan)
            };

            optimizationHistory.Add(entry);
            SaveOptimizationHistory();
        }

        private Dictionary<string, double> CollectCurrentDVHMetrics(ExternalPlanSetup plan)
        {
            var metrics = new Dictionary<string, double>();
            try
            {
                // Collect key DVH metrics
                var ptv = plan.StructureSet.Structures.FirstOrDefault(s => s.Id == "PTV_Eval");
                if (ptv != null)
                {
                    metrics["PTV_D95"] = plan.GetDoseAtVolume(ptv, 95, VolumePresentation.Relative, DoseValuePresentation.Absolute).Dose;
                    metrics["PTV_Max"] = plan.GetDoseAtVolume(ptv, 0.03, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Absolute).Dose;
                }

                // Add more metrics as needed...

                return metrics;
            }
            catch (Exception ex)
            {
                LogMessage($"Error collecting DVH metrics: {ex.Message}");
                return metrics;
            }
        }

        public void AddOptimizationObjective(ExternalPlanSetup plan, string structureId, string objectiveType, 
            double dose, double volume, double priority)
        {
            try
            {
                var structure = plan.StructureSet.Structures.FirstOrDefault(s => s.Id == structureId);
                if (structure == null)
                {
                    LogMessage($"Structure {structureId} not found");
                    return;
                }

                var parameters = new Dictionary<string, object>
                {
                    ["StructureId"] = structureId,
                    ["Type"] = objectiveType,
                    ["Dose"] = dose,
                    ["Volume"] = volume,
                    ["Priority"] = priority
                };

                AddObjective(plan, structure, new ObjectiveParameters(objectiveType, dose, volume, priority));
                LogOptimizationStep(plan, "AddObjective", parameters);
                
                LogMessage($"Added {objectiveType} objective to {structureId}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error adding objective: {ex.Message}");
            }
        }

        public void OptimizeCurrentPlan(ExternalPlanSetup plan)
        {
            try
            {
                LogMessage("Starting optimization...");
                plan.OptimizeVMAT();
                LogOptimizationStep(plan, "RunOptimization");
                
                plan.CalculateDose();
                LogMessage("Optimization and dose calculation complete");
                
                // Generate DVH metrics after optimization
                GenerateDVHMetrics(plan);
            }
            catch (Exception ex)
            {
                LogMessage($"Error during optimization: {ex.Message}");
            }
        }

        // Example UI interaction
        public void AddObjectiveFromUI(ExternalPlanSetup plan)
        {
            var window = new Window
            {
                Title = "Add Optimization Objective",
                Width = 400,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Structure selection
            var structureCombo = new ComboBox { Margin = new Thickness(5) };
            foreach (var structure in plan.StructureSet.Structures)
            {
                structureCombo.Items.Add(structure.Id);
            }
            Grid.SetRow(structureCombo, 0);
            grid.Children.Add(structureCombo);

            // Objective type selection
            var typeCombo = new ComboBox { Margin = new Thickness(5) };
            typeCombo.Items.Add("Upper");
            typeCombo.Items.Add("Lower");
            typeCombo.Items.Add("Mean");
            typeCombo.Items.Add("gEUD");
            Grid.SetRow(typeCombo, 1);
            grid.Children.Add(typeCombo);

            // Dose input
            var doseInput = new TextBox { Margin = new Thickness(5) };
            Grid.SetRow(doseInput, 2);
            grid.Children.Add(doseInput);

            // Volume input
            var volumeInput = new TextBox { Margin = new Thickness(5) };
            Grid.SetRow(volumeInput, 3);
            grid.Children.Add(volumeInput);

            // Priority input
            var priorityInput = new TextBox { Margin = new Thickness(5) };
            Grid.SetRow(priorityInput, 4);
            grid.Children.Add(priorityInput);

            // Add button
            var addButton = new Button
            {
                Content = "Add Objective",
                Margin = new Thickness(5)
            };
            addButton.Click += (s, e) =>
            {
                if (double.TryParse(doseInput.Text, out double dose) &&
                    double.TryParse(volumeInput.Text, out double volume) &&
                    double.TryParse(priorityInput.Text, out double priority))
                {
                    AddOptimizationObjective(
                        plan,
                        structureCombo.SelectedItem?.ToString(),
                        typeCombo.SelectedItem?.ToString(),
                        dose,
                        volume,
                        priority
                    );
                    window.Close();
                }
                else
                {
                    MessageBox.Show("Please enter valid numbers for dose, volume, and priority.");
                }
            };
            Grid.SetRow(addButton, 5);
            grid.Children.Add(addButton);

            window.Content = grid;
            window.ShowDialog();
        }

        private bool DetermineBreastSide()
        {
            try
            {
                // First try to determine from structures
                var structureSets = Patient.StructureSets.ToList();
                foreach (var ss in structureSets)
                {
                    var leftBreast = ss.Structures.FirstOrDefault(s => s.Id.Contains("Breast_L"));
                    var rightBreast = ss.Structures.FirstOrDefault(s => s.Id.Contains("Breast_R"));

                    if (leftBreast != null && !leftBreast.IsEmpty && 
                        (rightBreast == null || rightBreast.IsEmpty))
                    {
                        LogMessage("Automatically determined left-sided breast case from structures.");
                        return true;
                    }
                    if (rightBreast != null && !rightBreast.IsEmpty && 
                        (leftBreast == null || leftBreast.IsEmpty))
                    {
                        LogMessage("Automatically determined right-sided breast case from structures.");
                        return false;
                    }
                }

                // If can't determine automatically, ask user
                LogMessage("Could not automatically determine breast side. Asking user...");
                MessageBoxResult result = MessageBox.Show(
                    "Is this a left-sided breast case?",
                    "Breast Side Selection",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                bool isLeft = result == MessageBoxResult.Yes;
                LogMessage($"User selected {(isLeft ? "left" : "right")}-sided breast case.");
                return isLeft;
            }
            catch (Exception ex)
            {
                LogMessage($"Error determining breast side: {ex.Message}. Defaulting to user selection.");
                MessageBoxResult result = MessageBox.Show(
                    "Is this a left-sided breast case?",
                    "Breast Side Selection",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            }
        }

        // Add this helper method to create properly typed metric entries
        private (string Name, double Goal, double Acceptable, Func<ExternalPlanSetup, double> Collector) CreateMetricEntry(
            string name, 
            double goal, 
            double acceptable, 
            Func<ExternalPlanSetup, double> collector)
        {
            return (Name: name, Goal: goal, Acceptable: acceptable, Collector: collector);
        }

        // Then modify the metrics.Add calls to use this helper
        private void SavePlanComparisonMetrics(Course course)
        {
            try
            {
                string csvPath = Path.Combine(@"P:\Rafe\ABPI PROJECT\ASTRO_2025\Logs\PlanComparisons", 
                    $"{Patient.Id}_plan_comparison.csv");
                
                Directory.CreateDirectory(Path.GetDirectoryName(csvPath));

                var plans = course.ExternalPlanSetups.ToList();
                StringBuilder csv = new StringBuilder();

                // Header row
                csv.AppendLine("Metric,Goal,Acceptable," + string.Join(",", plans.Select(p => p.Id)));

                var metrics = new List<(string Name, double Goal, double Acceptable, Func<ExternalPlanSetup, double> Collector)>();
                
                metrics.Add(CreateMetricEntry(
                    "PTV D95",
                    2850.0,
                    2700.0,
                    p => GetDoseAtVolume(p, "PTV_Eval", 95.0, VolumePresentation.Relative)
                ));
                
                metrics.Add(CreateMetricEntry(
                    "PTV D0.03cc",
                    3150.0,
                    3150.0,
                    p => GetDoseAtVolume(p, "PTV_Eval", 0.03, VolumePresentation.AbsoluteCm3)
                ));
                
                metrics.Add(CreateMetricEntry(
                    "Body D0.03cc",
                    3150.0,
                    3150.0,
                    p => GetDoseAtVolume(p, "BODY", 0.03, VolumePresentation.AbsoluteCm3)
                ));
                
                metrics.Add(CreateMetricEntry(
                    "Heart V3Gy",
                    700.0,
                    1000.0,
                    p => GetVolumeAtDose(p, "Heart", 300.0)
                ));
                
                metrics.Add(CreateMetricEntry(
                    "Heart Mean",
                    100.0,
                    200.0,
                    p => GetMeanDose(p, "Heart")
                ));

                // Add each metric to CSV
                foreach (var metric in metrics)
                {
                    var values = plans.Select(p => metric.Collector(p).ToString("F2"));
                    csv.AppendLine($"{metric.Name},{metric.Goal},{metric.Acceptable},{string.Join(",", values)}");
                }

                File.WriteAllText(csvPath, csv.ToString());
                LogMessage($"Plan comparison metrics saved to: {csvPath}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error saving plan comparison metrics: {ex.Message}");
            }
        }

        // Helper methods for metric calculations
        private double GetDoseAtVolume(ExternalPlanSetup plan, string structureId, double volume, VolumePresentation volumePresentation)
        {
            try
            {
                var structure = plan.StructureSet.Structures.FirstOrDefault(s => s.Id == structureId);
                if (structure == null || !plan.IsDoseValid)
                    return double.NaN;

                return plan.GetDoseAtVolume(structure, volume, volumePresentation, DoseValuePresentation.Absolute).Dose;
            }
            catch
            {
                return double.NaN;
            }
        }

        private double GetVolumeAtDose(ExternalPlanSetup plan, string structureId, double doseInCGy)
        {
            try
            {
                var structure = plan.StructureSet.Structures.FirstOrDefault(s => s.Id == structureId);
                if (structure == null || !plan.IsDoseValid)
                    return double.NaN;

                return plan.GetVolumeAtDose(structure, new DoseValue(doseInCGy, DoseValue.DoseUnit.cGy), VolumePresentation.Relative);
            }
            catch
            {
                return double.NaN;
            }
        }


        private double GetMeanDose(ExternalPlanSetup plan, string structureId)
        {
            try
            {
                var structure = plan.StructureSet.Structures.FirstOrDefault(s => s.Id == structureId);
                if (structure == null || !plan.IsDoseValid)
                    return double.NaN;

                var dvhData = plan.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.1);
                return dvhData.MeanDose.Dose;
            }
            catch
            {
                return double.NaN;

            }
        }

        // Add this method to expose the StructureManager
        public BreastStructureManager GetStructureManager()
        {
            // If StructureManager is null, create it
            if (StructureManager == null)
            {
                // Find the appropriate structure set
                var structureSet = Patient.StructureSets.FirstOrDefault(ss => ss.Id.Contains("CT"));
                
                if (structureSet == null)
                {
                    LogMessage("No appropriate structure set found. Creating a new one...");
                    var image = Patient.StructureSets.FirstOrDefault()?.Image;
                    if (image == null)
                        throw new Exception("No image available for creating a structure set.");
                        
                    structureSet = image.CreateNewStructureSet();
                    structureSet.Id = "CT_AutoPlanning";
                }
                
                StructureManager = new BreastStructureManager(structureSet, IsLeftSided, LogMessage);
            }
            
            return StructureManager;
        }

        // Add this method to set a new structure set for the plan manager
        public void SetStructureSet(StructureSet newStructureSet)
        {
            this.StructureManager = new BreastStructureManager(newStructureSet, this.IsLeftSided, this.LogMessage);
        }

        // Add this method to set up a clinical structure set with structures from AICT and ResAI
        public string SetupClinicalStructureSet(StructureSet aictSet, StructureSet resAISet, StructureSet clinicalSet)
        {
            try
            {
                LogMessage("Setting up clinical structure set...");
                
                // First make sure we have a body structure
                var bodyStructure = clinicalSet.Structures.FirstOrDefault(s => s.Id == "BODY");
                if (bodyStructure == null || bodyStructure.IsEmpty)
                {
                    LogMessage("Creating body structure in clinical set...");
                    StructureManager.CreateBodyStructure();
                }
                
                // Use SetupStructureSet method since it already has the transfer logic
                StructureManager.SetupStructureSet();
                
                return "Clinical structure set setup complete.";
            }
            catch (Exception ex)
            {
                LogMessage($"Error in SetupClinicalStructureSet: {ex.Message}");
                return $"ERROR: {ex.Message}";
            }
        }
    }
}
