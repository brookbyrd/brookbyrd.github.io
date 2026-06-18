# Effective Field Sizes & Field Matching

## Effective Field Sizes

### Equivalent Square Field

The equivalent square field size is used to convert a rectangular field into an equivalent square field for scatter calculations.

$$  
a = \frac{4A}{P}  
$$

where:

- $a$ = equivalent square side length
    
- $A$ = field area
    
- $P$ = field perimeter
    

---

### Equivalent Circular Field

The equivalent circular field radius is:

$$  
r = \frac{4}{\sqrt{\pi}}\frac{A}{P}  
$$

where:

- $r$ = equivalent circle radius
    
- $A$ = field area
    
- $P$ = field perimeter
    

---

## Field Matching Equations

### Match-at-Depth

The gap required to achieve field matching at depth is:

$$  
S = \frac{L_1}{2}\left(\frac{z}{SSD}\right)  
+  
\frac{L_2}{2}\left(\frac{z}{SSD}\right)  
$$

where:

- $S$ = surface gap
    
- $L_1$ = length of first field
    
- $L_2$ = length of second field
    
- $z$ = depth of match
    
- $SSD$ = source-to-surface distance
    

### Clinical Meaning

- Used when matching adjacent photon fields.
    
- Creates perfect field matching at a specified depth.
    
- Prevents overlap (hot spots) and separation (cold spots).
    

---

## Hinge Angle

For two intersecting beams:

$$  
\phi = 90^\circ - \frac{\theta}{2}  
$$

where:

- $\phi$ = hinge angle between central axes
    
- $\theta$ = desired wedge angle
    

### Clinical Meaning

- Used in wedge pair calculations.
    
- Commonly tested on board examinations.
    

---

# Craniospinal Irradiation (CSI)

### Half-Beam Block Technique

Half-beam blocking (HBB) is used to eliminate beam divergence between the cranial and spinal fields.

---

### Spinal Field Collimator Rotation

# $$  
\theta_{collimator}

\tan^{-1}  
\left(  
\frac{L_{spine}}  
{2 \times SSD_{spine}}  
\right)  
$$

where:

- $L_{spine}$ = spinal field length
    
- $SSD_{spine}$ = spinal field SSD
    

---

### Brain Field Couch Rotation

# $$  
\theta_{couch}

\tan^{-1}  
\left(  
\frac{L_{brain}}  
{2 \times SAD_{brain}}  
\right)  
$$

where:

- $L_{brain}$ = brain field length
    
- $SAD_{brain}$ = brain field SAD
    

### Clinical Meaning

- Eliminates field divergence at the craniospinal junction.
    
- Produces geometric matching between brain and spine fields.
    
- Historically important for conventional CSI techniques.
    

---

## Board Pearls:

!!! tip "Exam Tip"


Equivalent Square:

$$
a = \frac{4A}{P}
$$

Match-at-Depth:

$$
S = \frac{L_1 z}{2SSD} + \frac{L_2 z}{2SSD}
$$

Hinge Angle:

$$
\phi = 90^\circ - \frac{\theta}{2}
$$

Craniospinal Junction Matching:

- Use half-beam blocking to eliminate divergence.
