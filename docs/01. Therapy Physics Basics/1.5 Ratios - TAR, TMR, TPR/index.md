
#  PDD, TAR, TPR, TMR,  BSF Definitions:

---


- **PDD**: Percent depth dose
	- For non-isocentric (SSD) setups, we use PDD:

$$ PDD = \frac{D(d)}{D(d_{\max})} \times 100 $$



- **TMR**: Tissue Maximum Ratio
	- For isocentric (SAD) setups, we use TMR:

$$ TMR = \frac{D_{\text{tissue}}(d)}{D_{\text{tissue}}(d_{\max})} $$


- **Converting PDD-to-TMR**: 
	
$$  
TMR = \frac{PDD}{100}  
\left(\frac{f+d}{f+d_0}\right)^2  
\frac{S_p(d_0)}{S_p(d)}  
$$


- **TAR**: Tissue Air Ratio
	- **SSD-independent**:
$$ TAR = \frac{D_{\text{tissue}}(d)}{D_{\text{air}}(d_{\max})} $$


- **BSF**: Backscatter Factor
	- (ratio of backscatter dose to given medium vs. air):
$$  
BSF = TAR(d_{\max})  
= \frac{D_{\text{tissue}}(d_{\max})}  
{D_{\text{air}}(d_{\max})}  
\times 100  
$$
- **Mayneord F-Factor**: 
	- Can allow for us to switch between PDDs, put the reference depth and reference SSD on top, then switch it for the Dmax term, and square both.
	- PDD$_2$  = F x PDD$_1$
	
$$F=\left(\frac{f_2+d_m}{f_1+d_m}\right)^2\left(\frac{f_1+d}{f_2+d}\right)^2$$

- **Magnification Factor**: 
	- Need to use for calculating appropriate FS to use for Sc look up.

$$  
r_d = r \frac{f+d}{f}  
$$
___

### Summary of Photon Beam Dose Ratios:

|Quantity|Stands For|Numerator|Denominator|Geometry|SSD Dependent?|Notes|
|---|---|---|---|---|---|---|
|PDD|Percentage Depth Dose|Dose at depth (d)|Dose at ${d_{\max}}$|SSD|Yes|Most common SSD quantity|
|TAR|Tissue-Air Ratio|Dose in tissue at depth (d)|Dose in air at same point|SAD|No|Tissue compared to air|
|TMR|Tissue Maximum Ratio|Dose in tissue at depth (d)|Dose in tissue at ${d_{\max}}$|SAD|No|Tissue compared to maximum dose depth|
|TPR|Tissue Phantom Ratio|Dose at depth (d)|Dose at reference depth|SAD|No|Generalized form of TMR|
|BSF|Backscatter Factor|Dose in tissue at ${d_{\max}}$|Dose in air at same point|SAD|No|Measures phantom scatter contribution|
____
### High-Yield Relationships

- BSF = TAR evaluated at (d_{\max})
    
- TMR = TPR when the reference depth is (d_{\max})
    
- TAR = TMR × BSF


