# Z80 Emulator Bug Analysis

This document analyzes potential bugs in the Z80 emulator implementation (`z80/Z80.cs`) against the official Zilog Z80 CPU specifications.

## Critical Bugs

### 1. DAA (Decimal Adjust Accumulator) - Incomplete Implementation
**Location:** `Z80.cs:886-905`

```csharp
case 0x27:
{
    // DAA
    var a = registers[A];
    var f = registers[F];
    if ((a & 0x0F) > 0x09 || (f & (byte)Fl.H) > 0)
    {
        Add(0x06);
        a = registers[A];
    }
    if ((a & 0xF0) > 0x90 || (f & (byte)Fl.C) > 0)
    {
        Add(0x60);
    }
```

**Problem:** The DAA instruction behavior depends on the **N flag** (whether the previous operation was addition or subtraction). The current implementation:
- Only handles the addition case (using `Add`)
- Never checks the N flag
- Doesn't subtract 0x06/0x60 when N=1

**Expected behavior:** When N=1 (after SUB/SBC/DEC/NEG), DAA should subtract correction values, not add them.

**Severity:** Critical - BCD arithmetic after subtraction will be completely wrong.

---

### 2. Operator Precedence Bug in Half-Carry Calculation (ADD)
**Location:** `Z80.cs:3288`

```csharp
if ((a & 0xF + b & 0xF) > 0xF)
    f |= (byte)Fl.H;
```

**Problem:** C# operator precedence: `+` has higher precedence than `&`. This evaluates as:
```csharp
(a & (0xF + b) & 0xF)  // WRONG
```
Instead of:
```csharp
((a & 0xF) + (b & 0xF))  // CORRECT
```

**Fix:** Add parentheses: `if (((a & 0xF) + (b & 0xF)) > 0xF)`

**Severity:** Critical - Half-carry flag will be incorrect in most ADD operations.

---

### 3. Same Operator Precedence Bug in ADC
**Location:** `Z80.cs:3308`

```csharp
if ((a & 0xF + b & 0xF) > 0xF)
    f |= (byte)Fl.H;
```

Same issue as above. Also missing the carry bit in half-carry calculation for ADC.

**Severity:** Critical

---

### 4. RLCA Missing Bit Rotation
**Location:** `Z80.cs:1064-1076`

```csharp
case 0x07:
{
    var a = registers[A];
    var c = (byte)((a & 0x80) >> 7);
    a <<= 1;
    registers[A] = a;  // BUG: bit 0 is 0, should be old bit 7
```

**Problem:** RLCA should rotate A left, with bit 7 going to BOTH the carry flag AND bit 0. The code sets carry but forgets to OR the old bit 7 into bit 0.

**Fix:** Add `a |= c;` before storing to register A.

**Severity:** Critical - Rotation is actually a shift.

---

### 5. RRCA Missing Bit Rotation
**Location:** `Z80.cs:1095-1107`

```csharp
case 0x0F:
{
    var a = registers[A];
    var c = (byte)(a & 0x01);
    a >>= 1;
    registers[A] = a;  // BUG: bit 7 is 0, should be old bit 0
```

**Problem:** RRCA should rotate A right, with bit 0 going to BOTH the carry flag AND bit 7.

**Fix:** Add `a |= (byte)(c << 7);` before storing to register A.

**Severity:** Critical - Rotation is actually a shift.

---

### 6. SBC (8-bit) Carry Flag Logic Inverted
**Location:** `Z80.cs:3350`

```csharp
if (diff > 0xFF) f |= (byte)Fl.C;
```

**Problem:** For subtraction, the carry flag (borrow) should be set when `diff < 0`, not when `diff > 0xFF`. The variable `diff` is an `int` that goes negative when a borrow occurs.

**Fix:** `if (diff < 0) f |= (byte)Fl.C;`

**Severity:** Critical - Carry flag wrong for all SBC operations.

---

### 7. SUB/SBC Overflow Detection Wrong
**Location:** `Z80.cs:3329-3330` and `Z80.cs:3347-3348`

```csharp
if ((a >= 0x80 && b >= 0x80 && (sbyte)diff > 0) || (a < 0x80 && b < 0x80 && (sbyte)diff < 0))
    f |= (byte)Fl.PV;
```

**Problem:** This logic is for addition overflow, not subtraction. For subtraction:
- Overflow occurs when: Positive - Negative = Negative
- Or when: Negative - Positive = Positive

**Fix:** Should be:
```csharp
if ((a < 0x80 && b >= 0x80 && (sbyte)diff < 0) || (a >= 0x80 && b < 0x80 && (sbyte)diff >= 0))
    f |= (byte)Fl.PV;
```

**Severity:** Critical - Overflow flag wrong for subtraction.

---

## High Severity Bugs

### 8. INC Sets N Flag (Should Reset)
**Location:** `Z80.cs:3428`

```csharp
f = (byte)(f | 0x02);  // Sets N flag
```

**Problem:** INC is an addition operation, so N should be RESET (0), not set. The N flag indicates subtract operation.

**Severity:** High - Affects DAA and other flag-dependent operations.

---

### 9. INC Does Not Preserve Carry Flag
**Location:** `Z80.cs:3419`

```csharp
var f = (byte)(registers[F] & 0x28);
```

**Problem:** According to Z80 specifications, INC should NOT affect the carry flag. The mask 0x28 clears the carry flag.

**Fix:** Should be `var f = (byte)(registers[F] & 0x29);` to preserve carry (bit 0).

**Severity:** High - Carry incorrectly cleared.

---

### 10. DEC Does Not Preserve Carry Flag
**Location:** `Z80.cs:3438`

Same issue as INC.

**Severity:** High

---

### 11. ADC HL,ss Half-Carry Uses Wrong Carry Value
**Location:** `Z80.cs:1620`

```csharp
if ((value1 & 0x0FFF) + (value2 & 0x0FFF) + (byte)Fl.C > 0x0FFF)
    f |= (byte)Fl.H;
```

**Problem:** `(byte)Fl.C` is the constant 1, not the actual carry flag value. Should use `(registers[F] & (byte)Fl.C)`.

**Severity:** High - Half-carry incorrect when C=0.

---

### 12. ADC HL,ss Overflow Detection Incorrect
**Location:** `Z80.cs:1622-1623`

```csharp
if (sum > 0x7FFF)
    f |= (byte)Fl.PV;
```

**Problem:** Overflow occurs when the sign changes incorrectly, not simply when result > 0x7FFF. Need to check if both operands have same sign but result has different sign.

**Severity:** High

---

### 13. ADC HL,ss Zero Flag Check
**Location:** `Z80.cs:1618`

```csharp
if (sum == 0)
    f |= (byte)Fl.Z;
```

**Problem:** Should check `(ushort)sum == 0` since sum could be 0x10000 which truncates to 0.

**Severity:** High

---

### 14. CPI/CPD/CPIR/CPDR Sign Flag Calculation Wrong
**Location:** `Z80.cs:1998, 2029, 2067, 2098`

```csharp
if (a < b) f = (byte)(f | 0x80);
```

**Problem:** Sign flag should be bit 7 of the result `(a - b)`, not based on unsigned comparison. Should be:
```csharp
if (((a - b) & 0x80) != 0) f |= 0x80;
```

**Severity:** High

---

### 15. CPI/CPD/CPIR/CPDR Half-Carry Calculation Wrong
**Location:** `Z80.cs:2000, 2031, 2069, 2100`

```csharp
if ((a & 8) < (b & 8)) f = (byte)(f | 0x10);
```

**Problem:** This only tests bit 3, not actual half-carry. Should be:
```csharp
if ((a & 0xF) < (b & 0xF)) f |= 0x10;
```

**Severity:** High

---

### 16. CCF Missing H Flag Behavior
**Location:** `Z80.cs:917-926`

```csharp
case 0x3F:
{
    // CCF
    registers[F] &= (byte)~(Fl.N);
    registers[F] ^= (byte)(Fl.C);
```

**Problem:** According to Z80 spec, CCF should set H to the previous value of C before complementing.

**Fix:** Add: `if ((registers[F] & (byte)Fl.C) != 0) registers[F] |= (byte)Fl.H; else registers[F] &= (byte)~Fl.H;`

**Severity:** Medium-High

---

### 17. BIT Instruction Missing S and P/V Flags
**Location:** `Z80.cs:1564-1570`

```csharp
private void Bit(byte bit, byte value)
{
    var f = (byte)(registers[F] & (byte)~(Fl.Z | Fl.H | Fl.N));
    if ((value & (0x01 << bit)) == 0) f |= (byte)Fl.Z;
    f |= (byte)Fl.H;
    registers[F] = f;
}
```

**Problem:**
- S flag should be set if testing bit 7 and it's 1
- P/V flag should be set the same as Z (contains the complement of the tested bit)

**Severity:** Medium-High

---

### 18. CP (Compare) Overflow Detection Off-by-One
**Location:** `Z80.cs:3408`

```csharp
if ((a > 0x80 && b > 0x80 && (sbyte)diff > 0) || (a < 0x80 && b < 0x80 && (sbyte)diff < 0))
```

**Problem:** Uses `a > 0x80` but should be `a >= 0x80` (0x80 is -128, a negative number).

**Severity:** Medium - Affects edge case at exactly 0x80.

---

## Medium Severity Bugs

### 19. NMI Handler Redundant Assignment
**Location:** `Z80.cs:71-72`

```csharp
IFF1 = IFF2;
IFF1 = false;
```

**Problem:** The first line is immediately overwritten, making it dead code. According to spec, NMI should just set IFF1 = false while preserving IFF2.

**Severity:** Low (functionally correct, just confusing code)

---

### 20. CPDR Wrong Timing When Match Found
**Location:** `Z80.cs:2106`

```csharp
Wait(21);  // Should be 16 when terminating
```

**Problem:** When CPDR terminates (match found or BC=0), it should take 16 T-states, not 21.

**Severity:** Medium - Timing inaccuracy.

---

### 21. Missing Undocumented Flag Handling (Bits 3 and 5)
**Location:** Throughout the file

The code uses `& 0x28` to "preserve" bits 3 and 5, but according to actual Z80 behavior, these undocumented flags should typically be set from the result value or operand, not preserved from the previous flags.

**Severity:** Low - Only affects undocumented behavior.

---

### 22. IN r,(C) - Missing (HL) Handling
**Location:** `Z80.cs:2326-2345`

The `IN` instruction cases don't include 0x70 for `IN (HL),(C)` which should read from port but not store anywhere (flags only).

**Severity:** Medium - Missing instruction variant.

---

## Summary

| Category | Count |
|----------|-------|
| Critical | 7 |
| High | 11 |
| Medium | 4 |
| Low | 2 |

The most impactful bugs are:
1. **DAA** - Completely broken for subtraction cases
2. **RLCA/RRCA** - Rotations are actually shifts
3. **Half-carry operator precedence** - Affects all ADD/ADC
4. **SBC carry flag** - Inverted logic
5. **SUB/SBC overflow** - Using addition logic

These bugs would cause significant compatibility issues with software that relies on correct flag behavior, BCD arithmetic, or bit rotation operations.
