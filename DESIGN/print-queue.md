# PrintHub - Print Queue & UI Design

## Print Queue Overview

The print queue is the core workflow. It transforms a list of "I want to print X of Product Y" into optimized print jobs, accounting for:
- Shared/generic parts (batch print once, use across multiple products)
- Part versions (use the current approved version)
- Printer capabilities (Bambu vs. OctoEverywhere)
- Print time estimates

---

## Print Queue Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                         USER ACTION                              │
│   User wants to print 5x "Dino Hook", 3x "Cat Hook"             │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    RESOLUTION ENGINE                              │
│                                                                  │
│  For each product requested:                                     │
│    - Look up ProductParts                                         │
│    - Look up current PrintFileVersion for each Part              │
│    - Calculate total parts needed                                │
│                                                                  │
│  Before:                                                         │
│    5x Dino Hook  → 5x Generic Hook + 5x Dino Char                │
│    3x Cat Hook   → 3x Generic Hook + 3x Cat Char                 │
│                                                                  │
│  Consolidate shared parts:                                       │
│    Generic Hook: 5+3 = 8 total                                  │
│    Dino Char: 5                                                  │
│    Cat Char: 3                                                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      PRINT JOBS                                  │
│                                                                  │
│  Job 001: Generic Hook x8 (1 print bed layout)                   │
│  Job 002: Dino Char x5                                           │
│  Job 003: Cat Char x3                                            │
│                                                                  │
│  Each job targets a printer and has status tracking              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        PRINTER                                    │
│                                                                  │
│  Bambu: Push directly via Bambu Connect API                     │
│  OctoEverywhere: Send to user's Pi bridge                         │
└─────────────────────────────────────────────────────────────────┘
```

---

## UI: Print Queue Page

### Layout Concept

```
┌─────────────────────────────────────────────────────────────────────────┐
│  PrintHub                            [Shop: My Etsy Shop ▼]  [👤 User] │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  📦 Add to Queue     🔄 Sync Etsy     ⚙️ Printer Settings         │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ══════════════════════════════════════════════════════════════════════  │
│                                                                          │
│  INVENTORY ALERTS                                                          │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │ ⚠️ Low stock: Dino Hook (3 remaining, reorder at 6)                 │  │
│  │ 💡 Tip: You sold 12 Cat Hooks this month. Print 10 more? [Print]   │  │
│  └────────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  ══════════════════════════════════════════════════════════════════════  │
│                                                                          │
│  PRINT QUEUE                                                              │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │  Product              │ Qty │ Parts Breakdown          │ Est.Time  │  │
│  ├───────────────────────┼─────┼──────────────────────────┼───────────┤  │
│  │ 🦕 Dino Wall Hook     │  5  │ Hook×5, Dino×5           │ ~2h 30m   │  │
│  │ 🐱 Cat Wall Hook      │  3  │ Hook×3, Cat×3           │ ~1h 30m   │  │
│  │ 🐻 Bear Wall Hook     │  2  │ Hook×2, Bear×2          │ ~1h 00m   │  │
│  └────────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  ─────────────────────────────────────────────────────────────────────  │
│                                                                          │
│  CONSOLIDATED VIEW (what will actually be printed)                      │
│  ┌────────────────────────────────────────────────────────────────────┐  │
│  │  Part                │ To Print │ On Hand │ Net After   │ Status   │  │
│  ├──────────────────────┼──────────┼─────────┼─────────────┼──────────┤  │
│  │ Basic Wall Hook      │     10   │    12   │   +2 ▲       │ Ready    │  │
│  │ Dino Character       │      5   │     0   │    -5 ⚠️     │ Low      │  │
│  │ Cat Character        │      3   │     2   │    -1 ⚠️     │ Low      │  │
│  │ Bear Character       │      2   │     4   │   +2 ▲       │ Ready    │  │
│  └────────────────────────────────────────────────────────────────────┘  │
│                                                                          │
│  Total estimated print time: 5h 00m                                     │
│  Total filament: ~180g ($2.70)                                          │
│                                                                          │
│  ┌─────────────────────┐                                                │
│  │    ▶ PRINT ALL     │  (or select specific items)                    │
│  └─────────────────────┘                                                │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Queue Item Actions

For each product row:
- **+ / -** — Adjust quantity
- **Split** — Queue just this product separately
- **Preview** — See 3D preview of parts
- **History** — See past prints of this product

### Consolidated View Explained

The consolidated view shows what *actually* gets printed after shared part optimization:

| Scenario | Meaning |
|----------|---------|
| `+2 ▲` | You're printing more than needed; inventory will increase |
| `-5 ⚠️` | You're printing less than on-hand; will decrease inventory |
| `0 ─` | Perfect match |
| `Low` flag | This part needs replenishment before next order |

---

## UI: Add to Queue Flow

### Step 1: Select Products

```
┌─────────────────────────────────────────────────────────────────────────┐
│  ADD TO QUEUE                                              [× Close]     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  🔍 Search products...                                                   │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  🦕 Dino Wall Hook                      $24.99    [On Etsy]        │   │
│  │     12 sold · 4 on hand                                         │   │
│  │                                                                 │   │
│  │  🐱 Cat Wall Hook                       $24.99    [On Etsy]        │   │
│  │     28 sold · 2 on hand ⚠️                                     │   │
│  │                                                                 │   │
│  │  🐻 Bear Wall Hook                      $24.99    [On Etsy]        │   │
│  │     8 sold · 4 on hand                                         │   │
│  │                                                                 │   │
│  │  ⭐ Custom Keychain (no Etsy)           —         [Not Listed]    │   │
│  │     Generic design                                              │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  [ Cancel ]                                        [ Next: Set Qty → ]  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Step 2: Set Quantities + Personalizations

```
┌─────────────────────────────────────────────────────────────────────────┐
│  ADD TO QUEUE                                              [× Close]     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Quantities & Personalization                                           │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  🦕 Dino Wall Hook                                               │   │
│  │  ┌────────────┐  Personalization: [None ↓]                       │   │
│  │  │     5      │  (+ / -)                                         │   │
│  │  └────────────┘                                                   │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  🐱 Cat Wall Hook                                                │   │
│  │  ┌────────────┐  Personalization: [Name on back ↓]              │   │
│  │  │     3      │  (+ / -)                                        │   │
│  │  └────────────┘  Note: 2 items need personalization              │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  For personalization, you'll provide names/notes after clicking   │   │
│  │  Next. Etsy order data will be used if available.                │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  [ ← Back ]                                      [ Next: Review → ]     │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Step 3: Personalization Entry (if needed)

```
┌─────────────────────────────────────────────────────────────────────────┐
│  ADD TO QUEUE                                              [× Close]     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  Enter Personalizations                                                 │
│                                                                          │
│  Cat Wall Hook (3 items)                                                │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  Item 1 of 3                                                      │   │
│  │  Customer: Mike                                                   │   │
│  │  Name for back: [ Mike                                    ]       │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  Item 2 of 3                                                      │   │
│  │  Customer: Sarah                                                  │   │
│  │  Name for back: [ Sarah                                   ]       │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  Item 3 of 3                                                      │   │
│  │  Customer: Chris                                                   │   │
│  │  Name for back: [ Chris                                   ]       │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  [ ← Back ]                                          [ Add to Queue → ]  │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## UI: Product Management

### Product Detail Page

```
┌─────────────────────────────────────────────────────────────────────────┐
│  ← Back to Products              🦕 Dino Wall Hook          [ Edit ]   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────────┐  ┌─────────────────────────────────────────┐  │
│  │   [3D Preview]       │  │  Etsy Listing #12345                    │  │
│  │                     │  │  Price: $24.99                          │  │
│  │   Hook + Dino Char  │  │  Sold: 47 · In Stock: 3 ⚠️              │  │
│  │                     │  │  Last sold: 2 days ago                  │  │
│  └─────────────────────┘  └─────────────────────────────────────────┘  │
│                                                                          │
│  PARTS IN THIS PRODUCT                                                   │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  Part              │ Version │ Generic │ Cost/Unit │ On Hand    │   │
│  ├─────────────────────┼─────────┼─────────┼───────────┼───────────┤   │
│  │  Basic Wall Hook   │ v3      │ ✅ Yes  │ $0.15     │ 12         │   │
│  │  Dino Character    │ v1      │ ❌ No   │ $0.30     │ 0 ⚠️       │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  PRINT HISTORY                                                           │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  Date       │ Qty  │ Parts Used            │ Status             │   │
│  ├─────────────┼──────┼────────────────────────┼────────────────────┤   │
│  │  Jan 12     │  5   │ Hook×5, Dino×5        │ ✅ Completed       │   │
│  │  Jan 8      │  3   │ Hook×3, Dino×3        │ ✅ Completed       │   │
│  │  Jan 3      │  5   │ Hook×5, Dino×5        │ ✅ Completed       │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  INVENTORY SETTINGS                                                      │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  Reorder Point: [ 6 ]     Reorder Qty: [ 10 ]                   │   │
│  │  ✓ Alert me when inventory is low                               │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  FILES                                                                  │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  Basic Wall Hook                                                │   │
│  │    v3 (current) — uploaded Jan 10   [Preview] [Download] [Set←] │   │
│  │    v2 — uploaded Dec 15          [Preview] [Download] [Set←]   │   │
│  │    v1 — uploaded Nov 20          [Preview] [Download] [Set←]   │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  [+ Add File Version]                                                    │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## UI: Dashboard / Insights

```
┌─────────────────────────────────────────────────────────────────────────┐
│  Dashboard                                                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  THIS MONTH                         vs LAST MONTH                        │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐        │
│  │ Products  │  │  Print    │  │  Revenue   │  │ Print Cost │        │
│  │ Sold: 34  │  │ Jobs: 12  │  │ $849.66    │  │ $18.40     │        │
│  │  ↑ 12%    │  │  ↑ 2      │  │  ↑ 15%     │  │  ↓ 8%      │        │
│  └────────────┘  └────────────┘  └────────────┘  └────────────┘        │
│                                                                          │
│  ══════════════════════════════════════════════════════════════════════  │
│                                                                          │
│  🔥 ATTENTION NEEDED                                                     │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  ⚠️ 3 products below reorder point                                │   │
│  │  Cat Hook (2 left), Dino Hook (3 left), Bear Hook (1 left)       │   │
│  │  [Print 30 more of each?]                                        │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  💡 Seasonal insight                                             │   │
│  │  Heart products sell 3x better in February. Start building       │   │
│  │  inventory in January for Valentine's Day.                        │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  ══════════════════════════════════════════════════════════════════════  │
│                                                                          │
│  TOP PERFORMERS (LAST 30 DAYS)                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  Product           │ Sold │ Revenue │ Margin │ Trend             │   │
│  ├────────────────────┼──────┼─────────┼────────┼───────────────────┤   │
│  │  🐱 Cat Wall Hook  │  12  │ $299.76 │ $11.42 │ 📈 20% vs prior   │   │
│  │  🦕 Dino Wall Hook│   8  │ $199.92 │ $10.12 │ 📉 5% vs prior    │   │
│  │  🐻 Bear Wall Hook│   6  │ $149.94 │  $9.82 │ ➡️ flat           │   │
│  │  ⭐ Star Keychain  │   5  │  $74.85 │  $4.50 │ 📈 50% vs prior  │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
│  INVENTORY OVERVIEW                                                     │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  Total SKUs: 24    │  Items in stock: 127  │  Below reorder: 3  │   │
│  │  Generic parts: 8  │  Total print files: 67                   │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Mobile Considerations

The print queue should be mobile-friendly since sellers often check status on their phones:

- **Simplified queue view** — show consolidated parts only
- **One-tap reorder** — tap alert → add to queue with one tap
- **Push notifications** — job completed, low stock, new Etsy order
- **Voice input** — "Hey, add 5 more Cat Hooks to the queue"

---

## Print Job Status States

```
┌─────────┐    ┌────────┐    ┌────────────┐    ┌───────────┐    ┌───────────┐
│ Pending │───▶│ Queued │───▶│ InProgress │───▶│ Completed │    │ Failed    │
└─────────┘    └────────┘    └────────────┘    └───────────┘    └───────────┘
                     │              │                                   │
                     │              ├─────────────▶───────────▶ Cancelled│
                     │              │                                   
                     ▼              ▼                                   
              ┌──────────┐    ┌────────────┐                            
              │ Paused   │◀───│ UserPause  │                            
              └──────────┘    └────────────┘                            
```

Status updates come from:
- **Bambu printers**: WebSocket connection via Bambu Connect
- **OctoEverywhere**: Polling user's Pi endpoint