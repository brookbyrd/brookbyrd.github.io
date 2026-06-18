
# ART AAPM Summer Course 

## Topics  
- [Fundamentals of ART](#fundamentals-of-art)  
- [CBCT-based Workflow (Ethos)](#CBCT-based-workflow)
- [MRgRT](#mrgrt)
- [Proton Adaptive RT](#proton-adaptive-rt)
- [Interactive Debates](#interactive-debates)

## Fundamentals of ART
### 8:00: Laura Ceverina - MSK

Overview of ART Workflows: 
- Offline -> Days
- Online -> Minutes
- Real-time ART -> Seconds

> [!NOTE]
> RTOG1106 showed 2.8% improvement in toxicity.
> - Stage III NSCLC
>- Mid-treatment FDG-PET performed during chemoradiation
>- Radiation plan adapted to boost persistent metabolically active tumor
>- First randomized multicenter trial demonstrating the feasibility and safety of biologically adaptive RT escalation.
> 
> 

**Key Workflow Pieces**: 
- Imaging
- Autosegmentation
- Evaluation & Decision Making
- Planning 

Challenges: 
- Operational demands
- Ethical and equity concerns
- Training and workforce development
- Infrastructure.

### 8:15: DanDan  - University of Rochester


### 8:30: Heng Li - JHU



## CBCT-based Workflow (Ethos)
### 10:00 Dennis Stanley: 
Real-patient case: 
- IDENTIFY 
- CBCT using Pelvis Large and iCBCT Acuros 
- RTT Co-pilot: Refining ROI and monitoring patient. 
- Influence structures: 
	- Bladder overlaps with normal tissue, is that okay?
	- Bowel contour toors. 
- Verifications structures: What you can't change
	- Body
	- High density.
 - Report only structures: No optimization on these. 

> [!QUOTE]
>  > How long have we been contouring? : > 5 min 
> 


- Target Review:
	- Check derivation recipe for each target. 
	- Co-register images. 
	- "Chase target" to follow target all the way the way soon. 
	- Look at target vs. bowel extent. 
	- Calling out the structures to RTTs. 

> [!QUOTE]
> > MD contouring is "High-stakes coloring" is accessible under the right educational teaching. 

- Plan Review:
	-  Goals are things we meet or exceed. 
	-  DVH review 
- Plan Sign off:
	-  Technical and MD review
- Confirmation CBCT:
	- TG-395 recommendation. 
	- IDENTIFY- gated CBCT
	- Are targets still within margins?
- IDENTIFY-gated beam delivery
	- IDENTIFY5 is autogated. 
-  EOT
	- Image quality
	- Contouring
	- Plan selection
	- Feedback


> [!QUOTE] 
> Check your ego at the door and act as if every patient is treating your mom.  s this PTV hill a hill that you want to die on? Someone is going to die on this hill.
> 


- IMRT or VMAT? 
	- VMAT: Breast and Abdominal: 
	- IMRT or VMAT
- How do you deal with tumors that aren't easily circumsribed?
	- Excessive notes
	- Rigidly propagate over target. 

> [!Questions:]
> Questions: How do you deal with kidney lesions where it's not easy to see them? Even for physicians.


___




## Sim-free Direct-to-Unit (DTU)
### 2:00 Mu-Hai Lin: Direct-to-Unit (Sim free)

What is direct-to-unit (DTU)? 
- DTU is sim-free to reduce the workload and shrink the amount of time that the patient is going to be at the hospital. 

Real-time oncouch planning:
- Require pre-planning
- CBCT on the couch.
- Commissioning of DTU workflow is necessary using an E2E phantom. 
## MRgRT 

### 10:30 MRgRT

#### MRgRT Overview: 

Key notes: 
- NRG GU-015 requires adaptive for bladder (ARCHER)

History: 
- MRiDIAN came out in 2017
- Unity came out in 2018
- A3I
- Aurora 2022
- Elekta Unity Motion Management: 2022

Why is online adaptive necessary? 
- Accurate targeting
- Safe dose escalation
- Decrease OAR dose.

Clinical trials: Long list of MRI-guided adaptive trials
- MIRAGE trial: Phase 3 RCT between CT vs. 
	- Urinary bowel toxicity with smaller 
- Pancreas trial: 
- MARGE trial: Rectum
- Smart1: Multi-site UW trial with safe single fraction. 
- UNITED: H&N

Abdominal Immobilization: 
- Not necessary and reduces image quality. 

Breath-hold Immobilization:
- Helps with reducing uncertainty and allows us to reduce margins, and isonormalize to OARs.

MRI-Linacs: 
- MRidean: 0.35T field is perpendicular and linac inside B-field.
- Unity: 1.5T field is perpendicular and linac outside B-field.
- Aurora: 0.5T field parallel to linac.

#### Physics Principles: 

##### Electron-return effect
- Electron-return effect from perpendicular fields, which is less of a problem for parallel B-fields. 
$$  F_{Lorentz}  = q v \times  B 
$$
- Radius of curvature for MRIdean ~4cm. 
- LINAC is shielded around magnetic field for MRidean

##### MRI artifacts: 
- RF artifacts
- Aliasing and Wrap around.
- Susceptibility
- Chemical Shift
- Motion artifacts

##### Spatial distortion: 
- TG-284 has recommendations to account for spatial distortion.
- 10 cm is 1 mm, 20 cm is 2mm 
- Improve mag-field with shim, rotational shims, and superconducting shims.
- B-field isocenter has a walkout 

##### MRidiean specific terminology 
- ATS - Adapt to shape. 
- OAR of the day


#### Workflow
- Contouring 2-3 within PTV
- Adaptive reoptimization, MRidean allows full reoptimization control. 
- Plan quality review is important. 
- Call outs occur during review. 
- 2D sine-MRI or 3D MRI then 2D sine-MRI

> [!NOTE]
> Full access to optimization objectives sets the MRidean apart.
> >

#### Key Takeaways

Key Takeaways: 
- MRgRT started in 2014. 
- TG-442 is in development for MRgRT


> [!NOTE]
> Nuggets of MRgRT: 
> - Keep conformality in mind with close segmentation. 
> - VMAT is of interest in the future. 
> - MR-fingerprinting - using biological adaptive for treatment planning. 
> 
> 


#### Questions: 
1) What sequences do we use for: Fast, spatial fidelity, tumor-to-contrast, distortion. 

___ 

## Proton Adaptive RT

### Proton Basics:
- Inelastic Coloumbic collisions with outer electrons.  $\rightarrow$ Bragg peak
- Elastic Coloumbic scatter $\rightarrow$ Multiple Columb scattering $\rightarrow$ Lateral scatter
- Nuetron interaction -> Bremsstrahlung tail

### Planning Uncertainties
Uncertainties involved: 
- Anatomy changes
- Imaging (SPR)
- Fluid and air filling
- Respiratory Motion
- Expected anatomic changes


### Planning Degrees of Freedom
- Field angle
- Beam specific target 
- Air gap
- Range shifter
- Spot size and shaping
- MFO vs. SFO




___
## PET real-time adaptive therapy

### 2:30: PET-adaptive therapy with Reflexion (Yale)

#### Clinical Quantitative PET metrics: 
- Diagnostic: 
	- SUV: Standard Uptake Value
	- Max SUV: Maximum Standard Uptake Value.
	- MTV: mean tumor volume. 
	- TLG: total lesion glycoloysis.

- Radiotherapy:
	- AS: Activity concentration, 
	- NTS: Normalized target signal.

#### RefleXion: 
- Has PET inside linac bore. 
- Requires fast moving bore?
- BgRT workflow: 
	1. sim and plan
	2. planning PET
	3. plan optimization and eval
	4. pre-scan pet
	5. PET guided beam delivery. 

#### Workflow metrics: 
- Activity Concentration (AC) & Normalized Target Signal (NTS) are the whats used to figure out where to target the beam. 

$$ NTS = \frac{AC^{target} - AC^{background}}{\sigma_{background}}$$

- TC_FM: target concentration at functional modeling.
- TCR:  target concentration during radiotherapy.


## Biological-guided ART
**Bin Cai, PhD**,

Anatomy accounts for: 
- Size, shape, position, and body difference.
- Limitations: Not reflecting tumor/tissue biology/physiology. 
- Volume change is often the late response.

Biological Information to adapt: 
- Timing
- Scale of functional adaptive
- Tumor vs. normal tissue functional adaptation. 

What Bio-informed RT can help with? 
- Patient selection
- Dose escalation/de-escalation
- Boost volumes. 

Clinical example: 
- Lung NSCLC boosting to improve local control, while keeping lung dose the same. 
- Hypoxia-directed H&N treatment (dose-deescalation).
	- If persistent hypoxia 70 Gy, otherwise if not hypoxic, use 30 Gy. 
- Functional liver avoidance planning (FLAP):
	- Functional liver volume (FLV) taken from SPECT. 
	- FLAP constraint: V16Gy < 12%. 
- Functional lung avoidance and response-adaptive escalation (FLARE):
	- PET for target discretization.
	- SPECT for functional lung volume. 
	- Contours from PET propogated over to sim CT.
- PULSAR: 
	1. Use high dose to half tumor for months of control. 
	2. Stop and monitor biological progress
	3. Plan the next pulse to patient with biological information. 


**Pros:** Allows for boosting and dropping toxicity. 
**Cons:** Uncertain benefit and incomplete outcome data available. 
___

## Image Registration and Segmentation - Emory, Xiaofeng 

#### AI that enables fast radiotherapy 
- AI-based image segmentation & AI-based image registration.

#### ART requires autosegmentation
- Precision should be structure specific based on location and dosimetric impact. 
- Autosegmentation models: 
	- Patch-based CNN:
		- Traditional neural network.
	- U-Net
		- Most commonly used. 
		- Decoder and encoder
		- Balance global context with what local detail requires
	- Attention cascaded models
		- Region--based/cascaded networks localize structures, then refein each organ in a targeted ROI. 
	- GAN/synthetic models 
		- Shape plausibility and complimentary contrast can help with difficult segmentation. 
		- Generates predicted shape, then corrects for it.
		- Challenges by CBCT image quality, so <u>synthetic CT</u> can greatly assist with GAN-based segmentation. 
	
- Mutliorgan segmentation introduces class imbalance. 
	- Small organs dominate clinical risk. 
	- Voxel count is not clinically important, so volumetric correction is needed. 
	- Region--based/cascaded networks localize structures, then refein each organ in a targeted ROI. 

#### Segmentation and Evaluation of Autocontours

- Segmentation Eval and QA
	- Overlap statics
		- - <span style="color: yellow;">What exact statistics are you using?</span>
	- DVF plausibility 
	- Dosimetric relevance.

- Evaluation Metrics: 
	- Workflow
	- Geometric Metris 
	- Clinical Metrics
	- Failure Checks
	
- Failure modes
	- Unusual anatomy
	- FOV issues
	- Pelvic filling/gas
	- Hallucinations
	- Image artifacts

## Dose Accumulation in Adaptive Radiotherapy 

###  Dose Accumulation, Mihaela, VCU 
#### What is dose accumulation trying to answer?
- OAR and target coverages over time, i.e. treatment efficacy and toxicity. 
- We use EQD2 for OAR assessment, not target coverage.
- <span style="color: yellow;">Will we use EQD2 when combining radiopharmaceuticals + EBRT ?</span>
#### CBCT vs. MRI 
- Are they solving the same problem for dose accumulation?
	- Mihaela says yes, I would say no because MRI captures the change throughout the fraction, not just at the start. 

#### Uncertainties in Deformable Registration 
- How to report uncertainties
- DIR plausability
- HU/density and accuracy

#### How is real-time dose accumulation used. 
- Majority is off-line analysis or just research. 

#### Uncertainty reporting template 
- <span style="color: yellow;">How do we characterize uncertainty?</span>

> [!NOTE]
> What we see on paper is not what's happening in real-time.
> 
> 


____

## Re-irradiation - Rachel Girr (MGH)

Patients are living longer. 
By 2030, over 4 million US survivors will have prior RT. 
#### How can ART help address reirradiation?
- Precise dose sculpting
- Dynamic adaption
- Safer reirradiation

#### Steps for Reirradiation
1. Prior data
2. Adaptive contour
3. Cumulative risk

#### Challenges in re-irradiation
- Deformable Image Registration
- Dose accumulation is inconsistent methods, with interpolation and replanning adding compounding error. 
	-  <span style="color: yellow;">How do we minimize these inconsistencies?</span>
- Imaging Modality (CBCT vs. MRgRT).
- Radiobiological modeling (EQD2 and BED missing prior subclinical toxicity).
- Tolerance doses - QUANTEC, HyTEC, TG-101, disease specific re-RT trials. 
- Lacking clinical trials.

#### Key take-aways: 
- Retrieve prior plans with proper registration and dose accumulation.
- Consider proton-based ART. 
- Adapt when it matters. 
- Build the evidence (ReCOG, E2-RADIe/ReCare).

![[Pasted image 20260618093443.png]]

> [!LOOK OUT FOR]
> - NRG reports on Dose Accumulation - Mihaela ()
> - NRG reports on ReRT - Martha (Michigan)

____


## [Workflow Challenges:  MaiTa UTSW]()

<u>Major shared challenges:</u>
- Staffing and MD time
- Chicken and egg problem with patient enrollment/reimbursement.
![[Pasted image 20260618101618.png]]

<u>TG-395 Structuring:</u>
- TG-395 tells us that we need restructure the workflow demands (advanced RT and physics contouring) to be able to scale the adaptive workflow. 

![[Pasted image 20260618101242.png|581]]

![[Pasted image 20260618101900.png|594]]


<u>New Workflow demands:</u>
- Imaging
	- Accurate and appropriate fusion.
- Contouring
	- Edit what matters
	- Judge the source
	- Co-pilot with MD
- Replan
	- How many things can you tweek in the optimization?

![[Pasted image 20260618102121.png]]

<u>Build judgement *before* the first patient :</u>
- Site visits
- Emulators
- Retrospective case replays
- Phantom dry runs and E2E rehearsals. 
![[Pasted image 20260618102436.png|722]]

<u>Credentialing:</u>
- Platform-specific trainings.
- Document who may do what. 
- Same discipline, different priveledges. 
- Re-train after any changes.

![[Pasted image 20260618102457.png|622]]

<u>Example: Contour training:</u>
- Sent therapists to class and must learn platform-specific tools. 
![[Pasted image 20260618102555.png|564]]
![[Pasted image 20260618102723.png|571]]
___
## Synthetic Imaging with Jess Scholey - UCSF


#### **What are the areas that require imaging?**
- Image generation
- Contour generation
- Dose reconstruction on new anatomy. 
- Dose optimization 
- Dose accumulation
- Geometric accuracy: 
	- 1mm for stereotactic.
	- 2mm for IMRT. 
- Motion artifacts
- Metal & air artifacts
![[Pasted image 20260618104033.png]]


#### MRI Imaging: 


## Interactive Debates: 

1) H&N needing adaption. What would you do? 
	- Progressive external contour/weight loss change causing a need of offline review. 
	- $\Delta$ Anatomy change timeline may need to be considered for smarter future planning. 
	- ** Key takeaway**: Dosimetric eval important. 
	
2) Pancreatic SBRT with rectal filling. What would you do? (El Beard et al., 2019)
	- Adapt same day because its a bowel-filling problem. 
	- IGRT "Snapshot Fallacy" vs. controlling the low-dose spread.
	- What do you do with air-filling bowels? Does it matter that much for photon electron density?

> [!NOTE]
> 
> > We must "think in 4D" with Adaptive Planning Robustness: 	
>    - Tweeking anatomy and see if planning technique holds up. 
>  - "Physics has to be a good planner."
>  - Templates in Ethos is the one thing that allows us to standardize.

3) Prostate ART with bladder filling while planning. Do you treat? 
	- YES, the bowel is further away now. The reported dosimetrics are now the worst case scenarios.

4) Lung SBRT: 50 Gy in 5 Fx with 5cm respiratory motion
	- Re-sim with abdominal compression. 
	- Cyberknife with real-time tracking is much superior to an ITV. 
	- What is the latency in Cyberknife? $\rightarrow$ tracking and training on 4DCT for CK. 


## Jeopardy

What is the effect of motion in blurring targets?
- Partial volume effect.

Type of planning that takes into account changes?
- Robust planning

Physical uncertainty based on imaging, anatomy, and modeling? 
- Range uncertainty

Whether adaption occurs before or at treatment unit. 
- Offline and online

Gain from adaptive benefit vs. risk? 
- Risk-benefit.

Ethics of access?
- Equity

Risk of reducing coverage for too tight of margin?
- Undercoverage

Adapting before seeing the 
- Predictive modeling. 

CTV estimation
- Van Herk CTV approximation.

Local anatomical deformation. 
- Deformable registration

Target that needs to be anatomically boosted. 
- Biological target volume (BTV)

