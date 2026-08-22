# Accessibility

> Last updated: 2026-07-08

## Overview

Basic accessibility patterns followed in the Payment Admin Frontend.

## Patterns

### Buttons

- All buttons have visible text labels
- Icon-only buttons have `title` attributes (e.g., copy, refresh, action buttons)
- Destructive buttons use red color as visual indicator (not the only indicator)

### Forms

- All form fields have associated `<label>` elements
- Required fields are marked with `*` indicator
- Validation errors are displayed directly under the corresponding field
- Error messages use clear red color and text

### Tables

- Data tables have proper `<th>` headers (via `column.header`)
- Tables use `role="table"` semantics where applicable
- Sortable headers indicate sort state

### Dialogs

- `ConfirmDialog` traps focus within the dialog
- Dialog overlay prevents interaction with background
- Close button is always available

### Color and Status

- Status badges use color + text (not color alone)
- Health status uses icons + text + color
- Error states use red color + icon + descriptive text

## Known Gaps

1. **No keyboard navigation audit** — Focus management, skip links, and keyboard
   navigation patterns have not been formally tested.
2. **No screen reader testing** — ARIA labels, live regions, and announcements
   have not been optimized for screen readers.
3. **No color contrast audit** — Shadcn default theme colors are used, but no
   formal contrast ratio verification has been performed.
4. **No focus ring customization** — Default browser focus indicators are used
   on most interactive elements.
5. **No reduced motion support** — CSS transitions and animations do not check
   `prefers-reduced-motion`.

## Recommendations

1. Add `aria-label` to all icon-only buttons as an additional fallback
2. Add `role="alert"` to error toasts for screen reader announcements
3. Add keyboard event handlers for custom interactive elements
4. Test with screen readers (VoiceOver, NVDA) before production release
5. Consider adding a focus trap library for modals

---

> This document is part of Phase 7.8 final polish documentation.
