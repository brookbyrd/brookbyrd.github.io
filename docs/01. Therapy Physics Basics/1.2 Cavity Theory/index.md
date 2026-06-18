# Cavity Theory


> **Big idea:** Cavity theory allows us to relate the dose measured inside a detector cavity to the dose that would have existed in the surrounding medium.

---

## Learning Goals

By the end of this section you should be able to:

- Define a cavity detector.
- Explain charged particle equilibrium (CPE).
- Describe Bragg-Gray cavity theory.
- Explain Spencer-Attix cavity theory.
- Describe Burlin cavity theory.
- Understand why TG-51 is based on cavity theory.
- Relate ion chamber measurements to absorbed dose.

---

# Why Cavity Theory Exists

![Ion Chamber in Water Phantom](https://chatgpt.com/c/images/ion_chamber_water_phantom.png)

When we place an ion chamber into a phantom:

- The chamber replaces some volume of water.
    
- The detector changes the medium.
    
- We no longer directly measure dose to water.
    
- We measure ionization inside the chamber cavity.
    

The question becomes:

> How do we convert ionization inside the cavity into dose to the surrounding medium?

That is the purpose of cavity theory.

---

# What is a Cavity?

![Detector Cavity Diagram](https://chatgpt.com/c/images/detector_cavity.png)

A cavity is a small region embedded inside another medium.

Examples include:

|Detector|Cavity|
|---|---|
|Farmer chamber|Air|
|Parallel plate chamber|Air|
|Exradin chamber|Air|
|Microchamber|Air|
|Calorimeter|Different material|

The surrounding medium may be:

- Water
- Plastic
- Tissue
- Solid water
- Other detector materials
    

---

# Charged Particle Equilibrium (CPE)

![Charged Particle Equilibrium](https://chatgpt.com/c/images/cpe_diagram.png)

Charged particle equilibrium exists when:

```text
Energy entering
=
Energy leaving
```

through charged particles.

In this situation:

```text
Kerma ≈ Dose
```

---

## Physical Meaning

For every electron leaving a volume:

- another electron enters,
    
- energy balance is maintained,
    
- absorbed dose becomes stable.
    

---

## When CPE Fails

![Build Up Region](https://chatgpt.com/c/images/build_up_region.png)

Examples:

- Surface dose
- Buildup region
- Small fields
- Tissue interfaces
- Air cavities
    

In these situations:

```text
Kerma ≠ Dose
```

---

# Electronic Equilibrium

![Electronic Equilibrium](https://chatgpt.com/c/images/electronic_equilibrium.png)

Electronic equilibrium is closely related to CPE.

It occurs when:

- secondary electron fluence entering a region
    
- equals
    
- secondary electron fluence leaving the region.
    

This assumption is fundamental to most cavity theories.

---

# Bragg-Gray Cavity Theory

![Bragg Gray Cavity](https://chatgpt.com/c/images/bragg_gray_cavity.png)

The Bragg-Gray model is the foundation of modern cavity theory.

### Assumptions

The cavity must be:

- Small enough to not disturb electron fluence.
    
- Traversed by electrons generated in the surrounding medium.
    

The cavity itself should not create significant electrons.

---

## Core Concept

The cavity acts as a passive observer.

Electrons are generated in the surrounding medium and simply pass through the cavity.

---

## Bragg-Gray Relationship

Dose to medium is related to dose in the cavity through stopping power ratios.

D_m = D_{gas}\left(\frac{S}{\rho}\right)_{m,gas}

Where:

- (D_m) = dose to medium
    
- (D_{gas}) = dose in cavity gas
    
- ((S/\rho)) = mass stopping power ratio
    

---

## Clinical Meaning

Bragg-Gray forms the basis for:

- Ion chamber dosimetry
    
- TG-21
    
- TG-51 concepts
    
- Reference dosimetry
    

---

# Small Cavity Requirement

![Small vs Large Cavity](https://chatgpt.com/c/images/small_vs_large_cavity.png)

A Bragg-Gray cavity must be:

```text
Much smaller than electron range
```

If the cavity becomes large:

- electrons stop inside the cavity,
    
- assumptions break down,
    
- Bragg-Gray is no longer valid.
    

---

# Spencer-Attix Cavity Theory

![Spencer Attix Diagram](https://chatgpt.com/c/images/spencer_attix.png)

Spencer-Attix improved Bragg-Gray theory.

---

## Why It Was Needed

Bragg-Gray assumes:

- all electrons contribute equally.
    

Reality:

- many low-energy electrons stop locally.
    

These electrons complicate dose calculations.

---

## Delta Cutoff

Spencer-Attix introduces:

```text
Δ (delta cutoff)
```

Electrons below Δ:

- deposit energy locally.
    

Electrons above Δ:

- contribute to stopping power calculations.
    

---

## Clinical Importance

Modern protocols including TG-51 use:

- Spencer-Attix stopping power ratios
    

rather than pure Bragg-Gray stopping powers.

---

# Burlin Cavity Theory

![Burlin Theory Diagram](https://chatgpt.com/c/images/burlin_cavity.png)

Burlin theory bridges:

- Bragg-Gray cavities
    
- Large cavity theory
    

---

## Why Burlin Exists

Some detectors are:

- too large for Bragg-Gray,
    
- too small for large cavity assumptions.
    

Burlin provides an intermediate solution.

---

## Concept

Dose comes from:

1. electrons crossing the cavity
    
2. photon interactions occurring within the cavity
    

with weighting between the two.

---

# Large Cavity Theory

![Large Cavity Diagram](https://chatgpt.com/c/images/large_cavity_theory.png)

For large cavities:

```text
Electron equilibrium no longer dominates.
```

Instead:

- photon interactions inside the cavity become important.
    

Large cavity theory becomes more appropriate.

---

# Cavity Theory Family Tree

![Cavity Theory Flowchart](https://chatgpt.com/c/images/cavity_theory_flowchart.png)

|Theory|Best For|
|---|---|
|Bragg-Gray|Small cavities|
|Spencer-Attix|Small cavities with realistic electron transport|
|Burlin|Intermediate cavities|
|Large cavity theory|Large cavities|

---

# Ion Chambers and Cavity Theory

![Farmer Chamber Cross Section](https://chatgpt.com/c/images/farmer_chamber_cross_section.png)

The Farmer chamber is essentially:

- an air cavity
    
- inside a wall material
    
- embedded in water
    

Cavity theory provides the framework to convert:

```text
Collected charge
→ Ionization
→ Dose to gas
→ Dose to water
```

---

# TG-51 Connection

![TG51 Workflow](https://chatgpt.com/c/images/tg51_workflow.png)

The entire TG-51 protocol depends on cavity theory.

Measurement chain:

```text
Raw Charge
↓
Corrected Charge
↓
M
↓
ND,w
↓
kQ
↓
Dose to Water
```

---

# Dose-To-Water Formalism

Modern reference dosimetry uses:

```text
Dose to Water
```

rather than exposure or air kerma.

TG-51 is fundamentally a dose-to-water protocol built on Spencer-Attix cavity theory.

---

# Common Oral Board Questions

### Why must a Bragg-Gray cavity be small?

Because it must not disturb electron fluence.

---

### Why was Spencer-Attix developed?

To account for low-energy electrons that violate Bragg-Gray assumptions.

---

### What theory is used in TG-51?

Spencer-Attix cavity theory.

---

### What happens if the cavity becomes too large?

Bragg-Gray assumptions fail and photon interactions inside the cavity become important.

---

### Why do we care about stopping power ratios?

They relate energy deposition in the gas cavity to energy deposition in the surrounding medium.

---

# Comparison Table

|Feature|Bragg-Gray|Spencer-Attix|Burlin|
|---|---|---|---|
|Small cavity required|Yes|Yes|Not strictly|
|Delta cutoff|No|Yes|Yes|
|Used in TG-51|Foundation|Yes|No|
|Modern reference dosimetry|Limited|Primary|Rare|

---

# Clinical Applications

![Clinical Detectors](https://chatgpt.com/c/images/clinical_detectors.png)

Applications include:

- TG-51 calibration
    
- Reference dosimetry
    
- Farmer chambers
    
- Small-field dosimetry
    
- Chamber selection
    
- Proton dosimetry
    
- Electron dosimetry
    

---

# One-Page Summary

![Cavity Theory Summary](https://chatgpt.com/c/images/cavity_theory_summary.png)

### Bragg-Gray

- Small cavity
    
- Electron fluence unchanged
    
- Stopping power ratio based
    

### Spencer-Attix

- Extension of Bragg-Gray
    
- Introduces Δ cutoff
    
- Used in TG-51
    

### Burlin

- Intermediate cavity
    
- Mix of photon and electron contributions
    

### Large Cavity

- Photon interactions inside cavity become important
    

---

# Check Yourself

1. Why does cavity theory exist?
    
2. What assumptions define Bragg-Gray theory?
    
3. What is charged particle equilibrium?
    
4. Why was Spencer-Attix developed?
    
5. What is Δ in Spencer-Attix theory?
    
6. Which cavity theory underlies TG-51?
    
7. When does Bragg-Gray fail?
    
8. What role does stopping power play?
    

---

# Key Takeaway

Cavity theory provides the mathematical bridge between ionization measured inside a detector cavity and absorbed dose in the surrounding medium. Modern clinical dosimetry, including TG-51, relies primarily on **Spencer-Attix cavity theory**, which extends the original Bragg-Gray framework to better describe real electron transport.