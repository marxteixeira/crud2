---
name: Modern Enterprise Inventory UI
colors:
  surface: '#f8f9ff'
  surface-dim: '#cbdbf5'
  surface-bright: '#f8f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#eff4ff'
  surface-container: '#e5eeff'
  surface-container-high: '#dce9ff'
  surface-container-highest: '#d3e4fe'
  on-surface: '#0b1c30'
  on-surface-variant: '#444653'
  inverse-surface: '#213145'
  inverse-on-surface: '#eaf1ff'
  outline: '#757684'
  outline-variant: '#c4c5d5'
  surface-tint: '#3755c3'
  primary: '#00288e'
  on-primary: '#ffffff'
  primary-container: '#1e40af'
  on-primary-container: '#a8b8ff'
  inverse-primary: '#b8c4ff'
  secondary: '#565e74'
  on-secondary: '#ffffff'
  secondary-container: '#dae2fd'
  on-secondary-container: '#5c647a'
  tertiary: '#003d28'
  on-tertiary: '#ffffff'
  tertiary-container: '#00563a'
  on-tertiary-container: '#5bcf9e'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dde1ff'
  primary-fixed-dim: '#b8c4ff'
  on-primary-fixed: '#001453'
  on-primary-fixed-variant: '#173bab'
  secondary-fixed: '#dae2fd'
  secondary-fixed-dim: '#bec6e0'
  on-secondary-fixed: '#131b2e'
  on-secondary-fixed-variant: '#3f465c'
  tertiary-fixed: '#85f8c4'
  tertiary-fixed-dim: '#68dba9'
  on-tertiary-fixed: '#002114'
  on-tertiary-fixed-variant: '#005137'
  background: '#f8f9ff'
  on-background: '#0b1c30'
  surface-variant: '#d3e4fe'
typography:
  display:
    fontFamily: Plus Jakarta Sans
    fontSize: 36px
    fontWeight: '700'
    lineHeight: 44px
    letterSpacing: -0.025em
  headline-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 30px
    fontWeight: '700'
    lineHeight: 38px
    letterSpacing: -0.02em
  headline-lg-mobile:
    fontFamily: Plus Jakarta Sans
    fontSize: 24px
    fontWeight: '700'
    lineHeight: 32px
    letterSpacing: -0.015em
  headline-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
    letterSpacing: -0.015em
  headline-sm:
    fontFamily: Plus Jakarta Sans
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
    letterSpacing: -0.01em
  title-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 16px
    fontWeight: '600'
    lineHeight: 24px
    letterSpacing: -0.005em
  body-lg:
    fontFamily: Manrope
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
    letterSpacing: 0em
  body-md:
    fontFamily: Manrope
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
    letterSpacing: 0em
  body-sm:
    fontFamily: Manrope
    fontSize: 13px
    fontWeight: '400'
    lineHeight: 18px
    letterSpacing: 0.005em
  label-md:
    fontFamily: Manrope
    fontSize: 13px
    fontWeight: '600'
    lineHeight: 18px
    letterSpacing: 0.01em
  label-sm:
    fontFamily: Manrope
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.02em
  caption:
    fontFamily: Manrope
    fontSize: 11px
    fontWeight: '500'
    lineHeight: 14px
    letterSpacing: 0.025em
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  gutter: 1.5rem
  gutter-sm: 1rem
  margin: 2rem
  margin-sm: 1rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 1rem
  space-lg: 1.5rem
  space-xl: 2rem
---

## Brand & Style

This design system targets high-scale operational teams, supply chain directors, and enterprise inventory controllers who demand precision, clarity, and rapid legibility. The visual tone is structured, authoritative, and frictionless—eliminating unnecessary visual noise while maintaining an elevated, high-polish B2B atmosphere.

The design movement combines **Modern Corporate Minimalism** with the utilitarian precision of Tailwind UI and shadcn/ui. High-contrast typography, strict linear containment, deliberate whitespace, and crisp tabular figures form an interface optimized for prolonged operational focus, high data density, and effortless inventory workflows.

## Colors

The palette leverages cool slate fundamentals paired with a deep royal navy primary to convey stability and command.

- **Primary (`#1E40AF`)**: Deep royal navy anchor for primary actions, selected navigation states, interactive toggles, and primary link targets.
- **Secondary (`#0F172A`)**: Deep slate for high-priority headings, critical iconography, dark contextual badges, and high-emphasis interface states.
- **Tertiary (`#059669`)**: Emerald green dedicated to positive inventory metrics, stock health confirmations, and active status indicators.
- **Neutral (`#64748B`)**: Slate neutral defining structural borders, secondary labels, disabled surface states, and ambient grid lines.

### Functional Surfaces and Accents
- **Canvas Base**: `#F8FAFC` provides an eye-resting, cool architectural canvas.
- **Surface Elevation**: `#FFFFFF` provides crisp contrast for cards, panels, and data tables.
- **Dividers & Strokes**: `#E2E8F0` delivers hairline borders across table rows and card perimeters.
- **Warning / Alert**: `#D97706` (Amber) for low-stock warnings, reorder alerts, and pending shipments.
- **Error / Destructive**: `#DC2626` (Crimson) for out-of-stock anomalies, order cancellations, and critical warehouse discrepancies.

## Typography

The typographic hierarchy pairs the structured geometry of **Plus Jakarta Sans** for navigational clarity and structural headers with the balanced legibility of **Manrope** for data-dense tables, metric dashboards, and long-form SKU specs.

- **Tabular Figures**: All instances of numbers in tables, SKU counts, financial balances, and stock volumes must enforce `font-feature-settings: "tnum" 1` via Manrope to guarantee vertical column alignment.
- **Letter Spacing**: Subtle negative tracking is applied to headlines to deliver a compact, architectural look reminiscent of contemporary developer tools and enterprise dashboards.
- **Label Hierarchy**: Uppercase micro-labels (`caption`) are used sparingly for section dividers and table header attributes to ensure clean vertical scanning.

## Layout & Spacing

The interface employs a fixed-fluid hybrid structure: a fixed structural sidebar (260px wide expanded, 72px collapsed) paired with a responsive fluid content canvas constrained to a maximum width of `1600px` for multi-column inventory dashboards.

### Grid & Breakpoints
- **Mobile (`< 640px`)**: Single-column reflow, stacked metric cards, off-canvas sliding navigation drawer, horizontal touch-scrolling for tabular structures with fixed primary SKU columns. Margin: `1rem`, Gutter: `1rem`.
- **Tablet (`640px - 1024px`)**: 6-column fluid grid, sidebar collapsed into icon-rail mode, aggregated KPI tiles arranged 2x2. Margin: `1.5rem`, Gutter: `1rem`.
- **Desktop (`> 1024px`)**: 12-column fluid grid with persistent sidebar navigation, multi-column inventory breakdown, and side-panel inspection sheets for granular stock edits. Margin: `2rem`, Gutter: `1.5rem`.

### Vertical Rhythm
A base 4px metric grid underpins all layout offsets. Internal card paddings default strictly to `space-lg` (24px) for expansive sections and `space-md` (16px) for compact data tables and modal drawers.

## Elevation & Depth

Visual hierarchy relies on crisp borders and refined, low-opacity ambient drop shadows over heavy blurs or saturated overlays.

- **Level 0 (Flat Canvas)**: `#F8FAFC` base page background, borderless.
- **Level 1 (Card & Panel Surface)**: Pure `#FFFFFF` resting on `#F8FAFC`, delimited by a subtle 1px border (`#E2E8F0`) and an ambient micro-shadow: `0 1px 2px 0 rgba(15, 23, 42, 0.04)`.
- **Level 2 (Popovers, Filter Menus, Dropdowns)**: Elevated `#FFFFFF` surfaces with a 1px border (`#E2E8F0`) and focused ambient depth: `0 4px 6px -1px rgba(15, 23, 42, 0.08), 0 2px 4px -2px rgba(15, 23, 42, 0.04)`.
- **Level 3 (Modal Dialogs & Slide-Over Inventory Sheets)**: Deep operational overlays with backdrop curtain (`rgba(15, 23, 42, 0.4)` with 4px backdrop blur) accompanied by `0 20px 25px -5px rgba(15, 23, 42, 0.1), 0 8px 10px -6px rgba(15, 23, 42, 0.06)`.

## Shapes

With `roundedness: 2`, structural components inherit a balanced modern corner radius (`0.5rem` / `8px`) that softens data density without sacrificing screen efficiency.

- **Base Radius (`0.5rem` / `8px`)**: Applied consistently to form inputs, buttons, dropdown panels, and table row selections.
- **Large Radius (`rounded-lg`, `1rem` / `16px`)**: Assigned to external card containers, metric summary tiles, and slide-in drawer shells.
- **Extra-Large Radius (`rounded-xl`, `1.5rem` / `24px`)**: Reserved for root modal overlays and full-screen contextual dialogs.
- **Full Radius (`pill`, `9999px`)**: Exclusively reserved for status chips, numeric quantity badges, and user avatars.

## Components

### Buttons
- **Primary**: Solid background `#1E40AF`, text `#FFFFFF`, font Manrope SemiBold (`label-md`). Hover state `#1D4ED8`, active state `#1E3A8A`. Focus ring `2px offset 2px` using `#1E40AF`.
- **Secondary / Outline**: `#FFFFFF` background, 1px border `#E2E8F0`, text `#0F172A`. Hover background `#F1F5F9`.
- **Ghost**: Transparent background, text `#475569`. Hover background `#F1F5F9`, hover text `#0F172A`.
- **Destructive**: `#DC2626` background, text `#FFFFFF`. Hover background `#B91C1C`.

### Data Tables & Grids
- **Header**: `#F8FAFC` background, height 40px, 1px bottom border `#E2E8F0`, typography Manrope 12px SemiBold uppercase (`caption`) tracking `0.05em`, color `#64748B`.
- **Rows**: Alternating hover effect (`#F8FAFC` on hover), base background `#FFFFFF`, height 52px, 1px bottom border `#F1F5F9`.
- **Cells**: Typography Manrope 14px Regular (`body-md`), numeric cells aligned right with tabular figures enabled.

### Status Badges & Chips
- Pill-shaped (`rounded-full`), height 24px, padding 2px 10px, typography Manrope 12px SemiBold (`label-sm`).
- **Active / In Stock**: Emerald background `#ECFDF5`, text `#047857`, 1px border `#A7F3D0`.
- **Alert / Low Stock**: Amber background `#FFFBEB`, text `#B45309`, 1px border `#FDE68A`.
- **Draft / Inactive**: Slate background `#F1F5F9`, text `#475569`, 1px border `#CBD5E1`.
- **Critical / Depleted**: Red background `#FEF2F2`, text `#B91C1C`, 1px border `#FECACA`.

### Form Inputs & Selects
- Height 40px, padding horizontal 12px, border 1px solid `#E2E8F0`, background `#FFFFFF`, text `#0F172A`, placeholder `#94A3B8`, border radius 8px (`0.5rem`).
- **Focus**: Border `#1E40AF`, outline ring 3px `rgba(30, 64, 175, 0.15)`.
- **Error State**: Border `#DC2626`, outline ring 3px `rgba(220, 38, 38, 0.15)`.

### Checkboxes & Radios
- Size 16px by 16px, border 1px solid `#CBD5E1`, border radius 4px (checkbox) or 50% (radio).
- **Checked**: Background `#1E40AF`, border `#1E40AF`, crisp white check icon or centered dot.

### Cards & Metric Containers
- Surface `#FFFFFF`, border 1px solid `#E2E8F0`, radius 16px (`rounded-lg`), padding 24px (`space-lg`).
- KPI metric displays feature Plus Jakarta Sans Bold 30px (`headline-lg`) paired with Manrope 13px Medium labels and integrated percentage change chips.

### Sidebar Navigation
- Width 260px, surface `#FFFFFF`, right border 1px solid `#E2E8F0`.
- **Navigation Links**: Height 40px, padding 8px 12px, border radius 8px, font Manrope 14px Medium.
- **Active Link**: Background `rgba(30, 64, 175, 0.08)`, text `#1E40AF`, font weight SemiBold, with a 3px vertical accent bar on the left edge.