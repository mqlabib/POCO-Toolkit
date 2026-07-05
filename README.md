# POCO Toolkit

> **Analyze before you modify.**

POCO Toolkit is a free, open-source ADB toolkit designed to safely analyze and maintain stock POCO devices running HyperOS.

Instead of blindly removing applications, POCO Toolkit analyzes the connected device first, then generates device-specific recommendations and reversible scripts.

---

# Philosophy

Every device is different.

POCO Toolkit follows a safety-first workflow:

```
Connect Device
      │
      ▼
Scan Device
      │
      ▼
Analyze Packages
      │
      ▼
Generate Safe Toolkit
      │
      ▼
Review Changes
      │
      ▼
Apply
      │
      ▼
Verify
```

The toolkit prioritizes:

- Device awareness
- Reversible changes
- Stock compatibility
- OTA friendliness
- Beginner-friendly workflow

---

# Features

## Current

- Universal POCO Scanner
- Device Information Collection
- Safe Package Analysis
- HyperOS Detection
- Bootloader Detection

## Planned

- Debloat Script Generator
- Restore Script Generator
- Verify Script Generator
- Windows GUI
- Safe Package Database
- Device Profiles
- Automatic Report Analysis

---

# Supported Devices

Current focus:

- POCO devices running HyperOS

Future support may include additional Xiaomi ecosystem devices.

---

# Requirements

- Windows
- Android Platform Tools (ADB)
- USB Debugging enabled
- Stock (non-root) devices supported
- Locked or unlocked bootloader supported

---

# Safety

POCO Toolkit does **not** generate scripts blindly.

Every recommendation is based on device analysis whenever possible.

The project always prioritizes reversible maintenance over aggressive debloating.

---

# Project Status

🚧 Active Development

Current milestone:

**v0.1 Foundation**

---

# Documentation

Project documentation:

- README.md
- ROADMAP.md
- CHANGELOG.md
- CONTRIBUTING.md
- DISCLAIMER.md

---

# Roadmap

v0.1
- Universal Scanner

v0.2
- Device Analyzer

v0.3
- Windows GUI

v0.5
- Toolkit Generator

v1.0
- Stable Release

---

# Contributing

Suggestions, testing, bug reports, and device scan reports are welcome.

Please include a device scan whenever reporting issues.

---

# Disclaimer

POCO Toolkit is an independent community project.

It is not affiliated with, endorsed by, or sponsored by Xiaomi, POCO, or Google.

Use at your own risk.

---

# License

MIT License
