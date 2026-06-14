---
title: SRS / SBRT Guidance
tags:
  - radiation-oncology
  - medical-physics
  - srs
  - sbrt
---

# SRS / SBRT Guidance

> [!info]
> This page serves as a central resource for stereotactic radiosurgery (SRS) and stereotactic body radiation therapy (SBRT) commissioning, QA, treatment planning, and clinical implementation.

---

## Navigation

### Related Topics
- [[Linac Anatomy]]
- [[docs/03. Quality Assurance/3.3 Machine QA/index]]
- [[Small Field Dosimetry]]
- [[Gamma Knife]]
- [[TG-51 Dosimetry]]
- [[IMRT Planning]]
- [[Proton Therapy]]

### Resident Resources
- [[ABR Study Guide]]
- [[Commissioning Reports]]
- [[Clinical Competencies]]

---

## Overview

SRS and SBRT are highly conformal treatment techniques that deliver large doses per fraction with steep dose gradients and tight geometric tolerances.

### Key Characteristics

| Feature | SRS | SBRT |
|---|---|---|
| Treatment Site | Intracranial | Extracranial |
| Fractions | 1-5 | 1-5 |
| Accuracy Requirement | Sub-mm | ~1 mm |
| Motion Management | Usually None | Often Required |

---

## Clinical Workflow

1. Patient Simulation
2. Immobilization
3. Image Registration
4. Target Delineation
5. Treatment Planning
6. Physics QA
7. Treatment Delivery
8. Follow-up

---

## Example Images

### Frameless SRS Setup

![[frameless_srs_setup.jpg]]

*Figure 1. Example frameless SRS immobilization setup.*

### VMAT Dose Distribution

![[srs_vmat_plan.png]]

*Figure 2. Example VMAT dose distribution demonstrating conformal target coverage.*

### Winston-Lutz Analysis

![[winston_lutz_example.png]]

*Figure 3. Example Winston-Lutz isocenter verification.*

---

## Important Tolerances

> [!warning]
> High-dose-per-fraction treatments require tighter tolerances than conventional radiotherapy.

| Test | Typical Tolerance |
|---|---|
| Winston-Lutz | ≤ 1 mm |
| CBCT Coincidence | ≤ 1 mm |
| Couch Walkout | ≤ 1 mm |
| End-to-End Test | ≤ 1 mm |

---

## Commissioning

### Required Measurements

- Small field output factors
- Beam profile measurements
- End-to-end testing
- Winston-Lutz testing
- Imaging coincidence verification
- Couch modeling validation

### Common Equipment

- SRS phantom
- Film dosimetry
- Small-volume ion chamber
- Diode detector
- EPID

---

## Quality Assurance

### Daily

- [ ] Imaging functionality
- [ ] Laser verification
- [ ] Output constancy

### Monthly

- [ ] Imaging coincidence
- [ ] Couch accuracy
- [ ] Winston-Lutz

### Annual

- [ ] End-to-end testing
- [ ] Full mechanical evaluation
- [ ] Comprehensive imaging QA

---

## Videos & Lectures

- [[SRS Commissioning Lecture]]
- [[Winston-Lutz Tutorial]]
- [[Gamma Knife Training]]
- [[TG-101 Review]]

---

## Embedded PowerPoint

![[SRS Commissioning Presentation.pptx]]

---

## Embedded PDF

![[AAPM_TG101.pdf]]

---

## Helpful External References

- [AAPM Website](https://www.aapm.org)
- [ASTRO Clinical Guidance](https://www.astro.org)
- [Medical Physics Journal](https://aapm.onlinelibrary.wiley.com)

---

## Frequently Asked Questions

### What is the purpose of a Winston-Lutz test?

To verify coincidence of the radiation isocenter and mechanical isocenter.

### Why are small field measurements challenging?

Detector volume averaging and loss of charged particle equilibrium.

### What report should I read first?

AAPM TG-101.

---

## Resident Checklist

- [ ] Read TG-101
- [ ] Review MPPG 9.a
- [ ] Review MPPG 9.b
- [ ] Observe Winston-Lutz testing
- [ ] Perform end-to-end testing
- [ ] Review SRS treatment planning workflow

---

## References

1. AAPM TG-101
2. MPPG 9.a
3. MPPG 9.b
4. TG-142
5. ASTRO SRS/SBRT Guidance

---

## Last Updated

June 2026