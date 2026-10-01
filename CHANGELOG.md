# Changelog

---

## [2.0.0] - 2026-10-01

Major release: the bot runs as a service natively, has a documentation site and many fixes. Existing systemd units and NSSM services keep working.

### What's new

- **Documentation site.** The documentation moved from the README to [corsinvest.github.io/cv4pve-botgram](https://corsinvest.github.io/cv4pve-botgram/): getting started, creating the Telegram bot and restricting it to your chats, permissions command by command, connection, running as a service, every command, API commands with placeholders, aliases and troubleshooting. Every page was checked against the code.

- **Native service support.** Linux systemd (`Type=notify`) and Windows services work out of the box, as in cv4pve-metrics-exporter: the bot recognises by itself how it was started, and no NSSM is needed on Windows. `systemctl start` returns once the bot is logged in to Proxmox VE and to Telegram. As a Windows service the bot writes to the Event Log. See [Run as a service](https://corsinvest.github.io/cv4pve-botgram/service/).

### Deprecated

- `--service-mode` is not needed anymore: the bot always runs until it is stopped. It is still accepted and ignored, with a warning in the log, and will be removed in the next major version: remove it from systemd units and NSSM services

### Fixes

- The bot exits with code 1 when it cannot start or Telegram refuses the token, so a service set to restart on failure tries again; it used to exit with 0
- VM, container and node commands answer with the error of Proxmox VE when the action is refused; they used to answer as if it had worked
- `/get`, `/set`, `/create` and `/delete` without a path ask for it; they used to call the command name as a path
- A parameter value between double quotes keeps its spaces
- `/alias` Delete removes the alias for good and confirms it; the alias used to come back
- `/alias` Create says when the name is not valid; it used to answer "Command created!" anyway
- `/help` describes `/create` and `/delete` correctly
- Built-in aliases that did not work: `qemu-migrate` and `qemu-vzdump-restore` did nothing, `node-shutdown` sent a command Proxmox VE refuses, `qemu-migrate` and `lxc-migrate` only read the migration preconditions. `node-shutdown` now asks for `reboot` or `shutdown`; the migrate aliases start the migration and ask for `online` (VM) or `restart` (container)

### Changed

- In a terminal the bot stops with Ctrl+C; it used to wait for Enter
- Corsinvest.ProxmoxVE.Api 9.2.4
- An API call refused by Proxmox VE is reported as an error message, also when the answer has status 200 with errors
- `/usage` lists the methods in the order get, set, create, delete
- Packages published with each release: deb, rpm, AUR, Homebrew and macOS pkg, besides the zips and WinGet
- Windows executable icon
- Project metadata, symbols (Source Link, `.snupkg`) and code style aligned with the other cv4pve tools

## [1.9.0] - 2025-12-12

### Fixes

- Admin functionality and bot commands ([#11](https://github.com/Corsinvest/cv4pve-botgram/pull/11))
