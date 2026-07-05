# UI Specification

This document defines the graphical user interface (GUI) for POCO Toolkit.

---

# Design Philosophy

The interface must be:

- Clean
- Beginner Friendly
- Safe
- Easy to understand
- Workflow based

Every screen should have ONE primary action.

---

# Application Workflow

Dashboard

↓

Scanner

↓

Analyzer

↓

Generate Script

↓

Done

---

# Window Layout

+------------------------------------------------------+
| Sidebar | Main Content                               |
|         |                                            |
|         |                                            |
|         |                                            |
+------------------------------------------------------+

---

# Sidebar

🏠 Dashboard

📱 Scanner

🧠 Analyzer

🧹 Debloat

♻ Restore

✔ Verify

⚙ Settings

ℹ About

---

# Header

POCO Toolkit

Analyze Before You Modify.

---

# Dashboard

Purpose:

Show device status.

Primary Button:

Connect Device

---

# Scanner

Purpose:

Read information from the connected device.

Primary Button:

Scan Device

Output:

Phone_Scan.txt

---

# Analyzer

Purpose:

Analyze scan results.

Primary Button:

Analyze

Outputs:

- Safe Packages
- Keep Packages
- Review Packages

---

# Debloat

Purpose:

Generate uninstall script.

Primary Button:

Generate Script

Output:

debloat.bat

---

# Restore

Purpose:

Generate restore script.

Primary Button:

Generate Restore

Output:

restore.bat

---

# Verify

Purpose:

Verify toolkit changes.

Primary Button:

Verify Device

---

# Settings

Purpose:

Application configuration.

Examples:

ADB Location

Language

Theme

Output Folder

---

# About

Purpose:

Project information.

Version

License

GitHub

Contributors

---

# Design Rules

Never overwhelm the user.

Never show dangerous operations first.

Always guide the user.

Every action should be reversible.

Analyze before modifying.
