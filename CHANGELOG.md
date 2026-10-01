# Changelog

---

## [Unreleased]

### What's new

- **Documentation site.** The documentation moved from the README to [corsinvest.github.io/cv4pve-botgram](https://corsinvest.github.io/cv4pve-botgram/): getting started, creating the Telegram bot and restricting it to your chats, permissions command by command, connection, running as a service, every command, API commands with placeholders, aliases and troubleshooting. Every page was checked against the code.

### Fixes

- VM, container and node commands answer with the error of Proxmox VE when the action is refused; they used to answer as if it had worked
- `/get`, `/set`, `/create` and `/delete` without a path ask for it; they used to call the command name as a path
- A parameter value between double quotes keeps its spaces
- `/alias` Delete removes the alias for good and confirms it; the alias used to come back
- `/alias` Create says when the name is not valid; it used to answer "Command created!" anyway
- `/help` describes `/create` and `/delete` correctly

### Changed

- Corsinvest.ProxmoxVE.Api 9.2.4
- An API call refused by Proxmox VE is reported as an error message, also when the answer has status 200 with errors
- `/usage` lists the methods in the order get, set, create, delete
- `.deb`, `.rpm` and macOS `.pkg` packages attached to each release
- Windows executable icon
- Project metadata, symbols (Source Link, `.snupkg`) and code style aligned with the other cv4pve tools

## [1.9.0] - 2025-12-12

### Fixes

- Admin functionality and bot commands ([#11](https://github.com/Corsinvest/cv4pve-botgram/pull/11))
